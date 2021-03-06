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
                var obelisk = Town.Actors.RiftObelisk;
                if (obelisk != null)
                {
                    await Movement.MoveTo(obelisk.Position);
                }

                if (AutoFollow.CurrentFollowers.Any(f => f.IsVendoring || f.InDifferentLevelArea))
                {
                    Log.Info("Waiting for followers to finish vendoring.");
                    await Coroutine.Sleep(1000);
                    return true;
                }

                if (ZetaDia.Service.Party.NumPartyMembers < 3)
                {
                    if (Equals(PartyJoinTimer, DateTime.MinValue))
                    {
                        PartyJoinTimer = DateTime.Now.AddMinutes(3);
                        return true;
                    }

                    if (DateTime.Now > PartyJoinTimer)
                    {
                        Log.Info("Followers took too long to join.  Leaving Game.");
                        await Party.LeaveGame(true);
                        await Coroutine.Sleep(10000);
                        PartyJoinTimer = DateTime.MinValue;
                        return true;
                    }
                    Log.Info("Waiting for followers to join game.");
                    await Coroutine.Sleep(10000);
                    return true;
                }

                if (AutoFollow.CurrentLeader.IsMe && Player.IsInTown && Trinity.Routines.PhelonsPlayground.Utils.Targeting.Players.Count() < 4 &&
                    AutoFollow.CurrentFollowers.All(f => f.IsInSameWorld && f.Distance > 35f))
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
        public static async Task<bool> TeleportToPlayer(Message playerMessage)
        {
            if (!await CanTeleportToPlayer(playerMessage))
                return false;

            if (!_teleportTimer.IsRunning)
                _teleportTimer.Restart();

            var portalNearby = Data.Portals.FirstOrDefault(p => p.Distance < 25f);

            // Allow time for leader to resolve in new world.
            if (!Player.IsInTown && portalNearby != null && _teleportTimer.ElapsedMilliseconds < 6000 && !AutoFollow.CurrentLeader.IsInRift)
            {
                Log.Debug("{0} is in a different world... waiting before teleport.  Portal Close:{1}", playerMessage.HeroAlias, portalNearby);
                return false;
            }
                
            _teleportTimer.Stop();

            // Safety check.
            var actor = Data.GetPlayerActor(playerMessage);
            if (actor != null && actor.Distance <= 100f)
                return false;

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

        private static async Task<bool> CanTeleportToPlayer(Message playerMessage)
        {
            if (!ZetaDia.IsInGame || ZetaDia.Globals.IsLoadingWorld)
                return false;
            if (playerMessage.IsInGreaterRift && RiftHelper.RiftQuest.Step != RiftQuest.RiftStep.UrshiSpawned)
            {
                Log.Debug("Can't teleport in greater rifts until Urshi spawns.");
                return false;
            }

            if (playerMessage.IsInSameWorld && playerMessage.Distance < 100f)
            {
                Log.Debug("In the same world and closer than 100 feet.");
                return false;
            }

            if (ZetaDia.Service.Party.NumPartyMembers <= 1)
            {
                Log.Info("Cant teleport because you are not in a party!");
                await Party.LeaveGame(true);
                await Party.RequestPartyInvite();
                return false;
            }

            if (Player.IsCasting)
                return false;

            return true;
        }

        /// <summary>
        /// If we're in a different world to the leader, teleport to him.
        /// </summary>
        public static async Task<bool> TeleportWhenInDifferentWorld(Message player)
        {
            if (player.WorldSnoId <= 0 || Player.CurrentWorldSnoId <= 0)
                return false;

            if (Player.IsInTown && BrainBehavior.IsVendoring)
                return false;

            if ((RiftHelper.IsInRift || player.IsInRift) && RiftHelper.IsGreaterRiftStarted)
                return false;

            if (Player.IsFollower && player.WorldSnoId != Player.CurrentMessage.WorldSnoId && player.IsInSameGame && !player.IsInCombat)
            {
                await Coordination.TeleportToPlayer(player);
                return true;
            }
            return false;
        }

        /// <summary>
        /// If we're too far away from the leader, teleport to him.
        /// </summary>
        public static async Task<bool> TeleportWhenTooFarAway(Message playerMessage)
        {
            if (RiftHelper.IsInGreaterRift || Player.IsInBossEncounter || playerMessage.IsInBossEncounter)
                return false;

            if (Player.IsFollower && playerMessage.WorldSnoId == Player.CurrentMessage.WorldSnoId && playerMessage.IsInSameGame &&
                !playerMessage.IsInCombat && playerMessage.Distance > Settings.Coordination.TeleportDistance)
            {
                Log.Info("{0} is getting quite far away... attempting teleport!", playerMessage.HeroAlias);
                await TeleportToPlayer(playerMessage);
                return true;
            }

            return false;
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