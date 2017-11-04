﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoFollow.Events;
using AutoFollow.Networking;
using AutoFollow.Resources;
using Buddy.Coroutines;
using Trinity.Components.Adventurer;
using Zeta.Bot;
using Zeta.Bot.Coroutines;
using Zeta.Bot.Logic;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;

namespace AutoFollow.Coroutines
{
    public static class Coordination
    {
        private static DateTime? _startedWaiting;
        private static DateTime _ignoreOtherBotsUntilTime;
        private static Stopwatch _teleportTimer = new Stopwatch();

        /// <summary>
        /// Time to wait before starting the profile/ opening the rift etc.
        /// WaitForGameStartDelay() will start sooner than this if the bots are nearby.
        /// </summary>
        public static DateTime StartAllowedTime = DateTime.MinValue;

        private static DateTime _waitUntil = DateTime.MinValue;

        public static DateTime WaitUntil => _waitUntil;

        public static void WaitFor(TimeSpan time, Func<bool> condition = null)
        {
            var newTime = DateTime.UtcNow + time;
            if (DateTime.UtcNow + time > _waitUntil && (condition == null || !condition()))
                _waitUntil = newTime;
        }

        static Coordination()
        {
            BotMain.OnStart += bot => Reset();
        }

        private static void Reset()
        {
            _startedWaiting = null;
            _ignoreOtherBotsUntilTime = DateTime.MinValue;
        }

        /// <summary>
        /// If the bot is locked out of the current greater rift, wait around until the rift is finished.
        /// </summary>
        public static async Task<bool> WaitForGreaterRiftInProgress()
        {
            if (RiftHelper.IsLockedOutOfRift)
            {
                Log.Info("Locked out of rift, waiting for it to finish.");
                await Coroutine.Sleep(5000);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Waits after changing world until all bots are nearby or the duration has elapsed.
        /// </summary>
        public static async Task<bool> WaitAfterChangingWorlds(int durationSeconds = 10)
        {
            var secsSinceWorldChanged = DateTime.UtcNow.Subtract(ChangeMonitor.LastWorldChange).TotalSeconds;
            if (secsSinceWorldChanged < durationSeconds)
            {
                var allFollowersNearby = AutoFollow.CurrentFollowers.All(f => f.IsInSameWorld && f.Distance < 50f);
                if (!allFollowersNearby && !Data.Monsters.Any(m => m.Distance < 30f))
                {
                    await Coroutine.Sleep(1000);
                    return true;
                }
            }
            return false;
        }

        ///// <summary>
        ///// Waits after killing rift gaurdian until all bots are nearby
        ///// </summary>
        //public static async Task<bool> WaitAfterKillingRiftGaurdian(int durationSeconds = 10)
        //{
        //    var secsSinceWorldChanged = DateTime.UtcNow.Subtract(ChangeMonitor.LastRiftGaurdianKilledTime).TotalSeconds;
        //    if (secsSinceWorldChanged < durationSeconds)
        //    {
        //        var allFollowersNearby = AutoFollow.CurrentFollowers.All(f => f.IsInSameWorld && f.Distance < 50f);
        //        if (!allFollowersNearby && !Data.Monsters.Any(m => m.Distance < 30f))
        //        {
        //            await Coroutine.Sleep(1000);
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        public static DateTime PartyJoinTimer = DateTime.MinValue;

        /// <summary>
        /// Prevents a greater rift from being started until all bots are ready.
        /// </summary>
        public static async Task<bool> WaitBeforeStartingRift()
        {
            if (Player.IsInTown)
            {
                //if (ZetaDia.Me.WorldId != 332336)
                //{
                //    ZetaDia.Me.UseWaypoint(ZetaDia.Storage.ActManager.GetWaypointByLevelAreaSnoId(332336)?.Number ?? 0);
                //    Log.Info("Going to Act 1.");
                //    await Coroutine.Sleep(5000);
                //    return true;
                //}

                if (ZetaDia.Service.Party.NumPartyMembers < Settings.Coordination.ExpectedBots)
                {
                    if (Equals(PartyJoinTimer, DateTime.MinValue))
                    {
                        PartyJoinTimer = DateTime.UtcNow.AddMinutes(3);
                        return true;
                    }

                    if (DateTime.UtcNow > PartyJoinTimer)
                    {
                        Log.Info("Followers took too long to join.  Leaving Game.");
                        await Party.LeaveGame(true);
                        await Coroutine.Sleep(10000);
                        PartyJoinTimer = DateTime.MinValue;
                        return true;
                    }
                    foreach (var toon in AutoFollow.CurrentParty.Where(b => (!b.IsInGame || b.GameId != Player.GameId) && b.AcdId != ZetaDia.ActivePlayerACDId))
                    {
                        await Party.InviteFollower(toon);
                    }
                    Log.Info($"Waiting for followers to join game. Total of [{AutoFollow.CurrentParty.Count}] in Current Party.");
                    await Coroutine.Sleep(2000);
                    return true;
                }

                if (AutoFollow.CurrentFollowers.Any(
                        f => f.IsVendoring || !ZetaDia.Me.IsParticipatingInTieredLootRun && f.InDifferentLevelArea))
                {
                    Log.Info("Waiting for followers to finish vendoring.");
                    await Coroutine.Sleep(30000);
                    return true;
                }

                var obelisk = Town.Actors.RiftObelisk;
                if (obelisk != null)
                {
                    Log.Info("Moving to Obelisk.");
                    await Movement.MoveTo(obelisk.Position);
                }

                if (AutoFollow.CurrentLeader.IsMe && Player.IsInTown &&
                    (Trinity.Routines.PhelonsPlayground.Utils.Targeting.Players.Count() < AutoFollow.NumberOfConnectedBots ||
                     AutoFollow.NumberOfConnectedBots < Settings.Coordination.ExpectedBots-1))
                    //AutoFollow.CurrentFollowers.All(f => f.IsInSameWorld && f.Distance > 35f)))
                {
                    Log.Info("Waiting for followers to show up.");
                    await Coroutine.Sleep(1000);
                    return true;
                }
            }
            return false;
        }

        public static async Task<bool> LeaveFinishedRift()
        {
            var turnInStep = RiftHelper.RiftQuest?.Step == RiftQuest.RiftStep.NotStarted && RiftHelper.RiftQuest?.State == QuestState.InProgress;
            if (turnInStep)
            {
                if (Targetting.RoutineWantsToLoot())
                    return true;

                if (Player.IsInRift || Player.IsIsInGreaterRift)
                {
                    Log.Info("Going to town, rift is finished");
                    await CommonCoroutines.UseTownPortal();
                    return true;
                }
                if(Player.IsInTown && AutoFollow.CurrentLeader != null && !AutoFollow.CurrentLeader.IsInTown)
                {
                    Log.Info("Waiting for leader to return to town");
                    await Coroutine.Sleep(1000);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Waits after joining a game based on the "Leader Start Delay" setting, or until all followers are ready and nearby.
        /// This is to prevent combat starting before all the bots are properly into the game.
        /// </summary>
        public static async Task<bool> WaitForGameStartDelay()
        {
            var time = ChangeMonitor.LastGameJoinedTime > ChangeMonitor.LastLoadedProfileTime 
                ? ChangeMonitor.LastGameJoinedTime : ChangeMonitor.LastLoadedProfileTime;

             StartAllowedTime = time.Add(TimeSpan.FromSeconds(Settings.Coordination.DelayAfterJoinGame));

            if (DateTime.UtcNow < StartAllowedTime && Player.IsInTown &&
                !AutoFollow.CurrentParty.All(
                    b => b.IsInTown && b.IsInGame && b.IsInSameWorld && b.Distance < 60f && !b.IsVendoring))
            {
                Log.Debug("Waiting for game start delay to finish {0}",
                    StartAllowedTime.Subtract(DateTime.UtcNow).TotalSeconds);

                await Coroutine.Sleep(500);
                return true;
            }
            return false;
        }

        /// <summary>
        /// If the leader is currently vendoring, start a town run on this bot as well.
        /// </summary>
        public static async Task<bool> StartTownRunWithLeader()
        {
            if (AutoFollow.CurrentLeader.IsVendoring && !Player.IsVendoring)
            {
                if (DateTime.UtcNow.Subtract(_lastTownRunWithLeaderTime).TotalSeconds > 5)
                {
                    BrainBehavior.ForceTownrun("Townrun with Leader");
                    _lastTownRunWithLeaderTime = DateTime.UtcNow;
                    return true;
                }
            }
            return false;
        }

        private static DateTime _lastTownRunWithLeaderTime = DateTime.MinValue;

        /// <summary>
        /// Teleport to player if possible.
        /// </summary>
        public static async Task<bool> TeleportToPlayer(Message playerMessage, bool forceTeleport = false)
        {
            if (!await CanTeleportToPlayer(playerMessage) && forceTeleport)
            {
                Log.Error("Unable to teleport to leader.");
                return false;
            }

            Log.Warn("Teleporting to player {0} SameGame={1} SameWorld={2}",
                playerMessage.HeroAlias, playerMessage.IsInSameGame, playerMessage.IsInSameWorld);

            ZetaDia.Me.TeleportToPlayerByIndex(playerMessage.Index);

            await Coroutine.Sleep(250);

            while (ZetaDia.Globals.IsLoadingWorld || ZetaDia.Me.LoopingAnimationEndTime != 0)
            {
                await Coroutine.Sleep(250);
                await Coroutine.Yield();
            }

            return true;
        }
        /// <summary>
        /// Checks to see if we can teleport to player.
        /// </summary>
        /// <param name="playerMessage"></param>
        /// <returns></returns>
        private static async Task<bool> CanTeleportToPlayer(Message playerMessage)
        {
            if (!ZetaDia.IsInGame || ZetaDia.Globals.IsLoadingWorld || Player.IsCasting)
            {
                Log.Warn("Can't teleport because I am casting.");
                return false;
            }

            if (Player.IsInTown && BrainBehavior.IsVendoring)
            {
                Log.Warn("Can't teleport because I am vendoring.");
                return false;
            }

            if (playerMessage.IsInGreaterRift && RiftHelper.RiftQuest.Step != RiftQuest.RiftStep.UrshiSpawned)
            {
                Log.Warn("Can't teleport in greater rifts until Urshi spawns.");
                return false;
            }

            if (playerMessage.IsInSameWorld && playerMessage.Distance < 100f)
            {
                Log.Warn("In the same world and closer than 100 feet.");
                return false;
            }

            if (Player.IsInCombat)
            {
                Log.Warn("Cant teleport because you are in combat.");
                return false;
            }
            if (Player.IsInBossEncounter || playerMessage.IsInBossEncounter)
            {
                Log.Warn("Cant teleport because you are in BOSS combat.");
                return false;
            }
            if (Player.IsInTown && playerMessage.WorldSnoId == Player.CurrentMessage.WorldSnoId)
            {
                Log.Warn("Cant teleport because we are in the same town world.");
                return false;
            }
            var portalNearby = Data.Portals.FirstOrDefault(p => p.Distance < 25f);

            //if (_teleportTimer.ElapsedMilliseconds < 2500)
            //{
            //    Log.Warn("{0} waiting before teleport as it is on Cooldown.  Portal Close:{1}", playerMessage.HeroAlias, portalNearby);
            //    return false;
            //}

            // Allow time for leader to resolve in new world.
            if (!Player.IsInTown && portalNearby != null && _teleportTimer.ElapsedMilliseconds < 6000 && !AutoFollow.CurrentLeader.IsInRift)
            {
                Log.Warn("{0} is in a different world... waiting before teleport.  Portal Close:{1}", playerMessage.HeroAlias, portalNearby);
                return false;
            }

            if (!_teleportTimer.IsRunning)
                _teleportTimer.Restart();

            return true;
        }

        /// <summary>
        /// Use portal if it's the one our leader used last.
        /// </summary>
        /// <returns></returns>
        public async static Task<bool> FollowLeaderThroughPortal()
        {
            var leaderWasLastInMyCurrentWorld = AutoFollow.CurrentLeader.PreviousWorldSnoId == Player.CurrentWorldSnoId;
            var lastWorldPosition = AutoFollow.CurrentLeader.LastPositionInPreviousWorld;
            if (leaderWasLastInMyCurrentWorld)
            {
                var portalUsed = Data.Portals.Where(p => p.Position.Distance(lastWorldPosition) < 25f).OrderBy(p => p.Position.Distance(lastWorldPosition)).FirstOrDefault();
                if (portalUsed != null && portalUsed.CommonData.GizmoType != GizmoType.HearthPortal)
                {
                    Log.Info("Leader {0} appears to have used this portal here: '{1}' Dist={2}. Following.",
                        AutoFollow.CurrentLeader.HeroAlias, portalUsed.Name, portalUsed.Distance);

                    if(await Movement.MoveToAndInteract(portalUsed))
                        return true;
                }                
            }
            return false;
        }

        /// <summary>
        /// If there is a portal nearby and the bot is standing around doing nothing, take the portal.
        /// </summary>
        public static async Task<bool> UseNearbyPortalWhenIdle()
        {
            if (!Player.IsIdle || Player.IsInTown)
                return false;

            if (AutoFollow.CurrentLeader.IsInSameWorld && AutoFollow.CurrentLeader.Distance < 30f)
                return false;

            var nearbyPortal = Data.Portals.FirstOrDefault(p => p.Distance < 30f);
            if (nearbyPortal == null)
                return false;

            if (Navigation.CanRayCast(ZetaDia.Me.Position, nearbyPortal.Position))
                return false;

            Log.Info("Lets use this nearby portal, what could go wrong? Id={0} Distance={1}",
                nearbyPortal.Name, nearbyPortal.Distance);

            await Movement.MoveToAndInteract(nearbyPortal);
            return true;
        }

        public static async Task<bool> TeleportToRiftGaurdianLoot(Message player)
        {
            if (AutoFollow.CurrentLeader.Distance < 150f && AutoFollow.CurrentLeader.IsInSameWorld)
            {
                Log.Info("Leader is too close to teleport");
                await Movement.MoveToPlayer(player, 25f);
                return false;
            }

            if (await TeleportToPlayer(player))
                return true;

            return false;

        }
    }
}