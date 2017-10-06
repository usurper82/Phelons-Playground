using Buddy.Coroutines;
using System;
using Trinity.Framework;
using Trinity.Framework.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trinity.Components.Adventurer.Game.Actors;
using Trinity.Components.Adventurer.Game.Exploration;
using Trinity.Components.Adventurer.Game.Rift;
using Trinity.Components.Adventurer.Settings;
using Trinity.Framework.Reference;
using Zeta.Bot;
using Zeta.Bot.Coroutines;
using Zeta.Bot.Logic;
using Zeta.Common;
using Zeta.Common.Helpers;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using RiftStep = Trinity.Components.Adventurer.Game.Rift.RiftStep;

namespace Trinity.Components.Adventurer.Coroutines.RiftCoroutines
{
    using Framework.Objects;
    using Zeta.Game.Internals;

    public sealed class UpgradeGemsCoroutine : ICoroutine
    {
        public UpgradeGemsCoroutine()
        {
            Id = Guid.NewGuid();
        }

        private readonly WaitTimer _pulseTimer = new WaitTimer(TimeSpan.FromMilliseconds(250));
        private bool _isPulsing;
        private Vector3 _urshiLocation;

        private readonly InteractionCoroutine _interactWithUrshiCoroutine = new InteractionCoroutine(RiftData.UrshiSNO, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1), 3);

        #region State

        private enum States
        {
            NotStarted,
            UrshiSpawned,
            SearchingForUrshi,
            MovingToUrshi,
            InteractingWithUrshi,
            UpgradingGems,
            Completed,
            Failed
        }

        private States _state;

        private States State
        {
            get { return _state; }
            set
            {
                if (_state == value) return;
                if (value != States.NotStarted)
                {
                    Core.Logger.Debug("[Gem Upgrade Coroutine] " + value);
                    StatusText = "[Gem Upgrade Coroutine] " + value;
                }
                _state = value;
            }
        }

        private async Task<bool> GetCoroutine()
        {
            CoroutineCoodinator.Current = this;


            if (_isPulsing)
            {
                PulseChecks();
            }

            await HandleDeath();

            switch (State)
            {
                case States.NotStarted:
                    return NotStarted();

                case States.UrshiSpawned:
                    return UrshiSpawned();

                case States.SearchingForUrshi:
                    return await SearchingForUrshi();

                case States.MovingToUrshi:
                    return await MovingToUrshi();

                case States.InteractingWithUrshi:
                    return await InteractingWithUrshi();

                case States.UpgradingGems:
                    return await UpgradingGems();

                case States.Completed:
                    return Completed();

                case States.Failed:
                    return Failed();
            }
            return false;
        }

        private async Task HandleDeath()
        {
            if (ZetaDia.Me.IsDead)
            {
                State = States.Failed;
                GameUI.ReviveAtCorpseButton.Click();
                await Coroutine.Sleep(500);
            }
        }

        public Guid Id { get; set; }

        #endregion State

        private bool NotStarted()
        {
            State = States.UrshiSpawned;
            return false;
        }

        private bool UrshiSpawned()
        {
            EnablePulse();
            State = States.SearchingForUrshi;
            return false;
        }

        private async Task<bool> SearchingForUrshi()
        {
            EnablePulse();
            if (_urshiLocation != Vector3.Zero)
            {
                State = States.MovingToUrshi;
                return false;
            }
            if (!await ExplorationCoroutine.Explore(new HashSet<int> { AdvDia.CurrentLevelAreaId }))
            {
                Core.Logger.Log("[Gem Upgrade Coroutine] Exploration for urshi has failed, the sadness!");
                State = States.Failed;
                return false;
            }

            Core.Logger.Log("[Gem Upgrade Coroutine] Where are you, my dear Urshi!");
            Core.Scenes.Reset();
            return false;
        }

        private async Task<bool> MovingToUrshi()
        {
            EnablePulse();
            if (!await NavigationCoroutine.MoveTo(_urshiLocation, 5)) return false;
            _urshiLocation = Vector3.Zero;
            State = States.InteractingWithUrshi;
            return false;
        }

        private async Task<bool> InteractingWithUrshi()
        {
            DisablePulse();
            if (RiftData.VendorDialog.IsVisible)
            {
                State = States.UpgradingGems;
                return false;
            }

            var urshi = ZetaDia.Actors.GetActorsOfType<DiaUnit>().FirstOrDefault(a => a.IsFullyValid() && a.ActorSnoId == RiftData.UrshiSNO);
            if (urshi == null)
            {
                Core.Logger.Debug("[Gem Upgrade Coroutine] Can't find the Urshi lady :(");
                State = States.Failed;
                return false;
            }

            if (!await _interactWithUrshiCoroutine.GetCoroutine())
                return false;

            await Coroutine.Wait(2500, () => RiftData.VendorDialog.IsVisible);

            _interactWithUrshiCoroutine.Reset();

            if (!RiftData.VendorDialog.IsVisible)
            {
                return false;
            }
            
            State = States.UpgradingGems;
            return false;
        }

        private async Task<bool> UpgradingGems()
        {
            if (RiftData.VendorDialog.IsVisible && RiftData.ContinueButton.IsVisible &&
                RiftData.ContinueButton.IsEnabled)
            {
                Core.Logger.Debug("[Gem Upgrade Coroutine] Clicking to Continue button.");
                RiftData.ContinueButton.Click();
                RiftData.VendorCloseButton.Click();
                await Coroutine.Sleep(3000);
                return false;
            }
            if (ZetaDia.Me.JewelUpgradesLeft == 0)
            {
                Core.Logger.Debug("[Gem Upgrade Coroutine] Finished all upgrades, returning to town.");
                State = States.Completed;
                return false;
            }
            if (AdvDia.RiftQuest.Step == RiftStep.Cleared)
            {
                Core.Logger.Debug("[Gem Upgrade Coroutine] Rift Quest is completed, returning to town");
                State = States.Completed;
                return false;
            }
            var gemToUpgrade = PluginSettings.Current.Gems.GetUpgradeTarget();
            if (gemToUpgrade == null)
            {
                Core.Logger.Log("[Gem Upgrade Coroutine] I couldn't find any gems to upgrade, failing.");
                State = States.Failed;
                return false;
            }

            await DoUpgrade(gemToUpgrade);

            return false;
        }

        public static async Task DoUpgrade(ACDItem gemToUpgrade)
        {
            Core.Logger.Debug("[Gem Upgrade Coroutine] ENTERING UPGRADE ROUTINE!");
            int upgrade = ZetaDia.Me.JewelUpgradesLeft;
            //for (int upgrade = ZetaDia.Me.JewelUpgradesLeft; upgrade > 0; upgrade--)
            //{
            Core.Logger.Debug("[Gem Upgrade Coroutine] Gem upgrades left before the attempt: {0}",
                ZetaDia.Me.JewelUpgradesLeft);
            var newGem = PluginSettings.Current.Gems.Gems.FirstOrDefault(x => x.Guid == gemToUpgrade.AnnId);
            //Core.Logger.Debug($"Before Gem Rank: {newGem?.Name} {newGem?.Rank} [{newGem?.Guid}]");
            if (!await CommonCoroutines.AttemptUpgradeGem(gemToUpgrade))
            {
                Core.Logger.Error("[Gem Upgrade Coroutine] Unable to Upgrade Gem.");
                //await Coroutine.Yield();
                await Coroutine.Sleep(2000);
                return;
            }
            PluginSettings.Current.Gems.Update();
            if (ZetaDia.Me.JewelUpgradesLeft != upgrade)
            {
                //await Coroutine.Sleep(4000);
                var currentGem = PluginSettings.Current.Gems.Gems.FirstOrDefault(x => x.Guid == gemToUpgrade.AnnId);
                //Core.Logger.Debug($"After Gem Rank: {currentGem?.Name} {currentGem?.Rank} [{currentGem?.Guid}]");
                if (newGem != null && currentGem != null)
                {
                    GemUpgrades.Load();
                    var existingGem = GemUpgrades.Upgrades.FirstOrDefault(x => x.Chance == newGem.UpgradeChance);
                    if (existingGem == null)
                    {
                        existingGem = new GemUpgrade
                        {
                            //Name = newGem.Name,
                            Chance = newGem.UpgradeChance,
                            Failure = 0,
                            Success = 0
                        };
                        if (newGem.Rank == currentGem.Rank)
                            existingGem.Failure++;
                        else
                            existingGem.Success++;
                        GemUpgrades.Upgrades.Add(existingGem);
                    }
                    else
                    {
                        if (newGem.Rank == currentGem.Rank)
                            existingGem.Failure++;
                        else
                            existingGem.Success++;
                    }
                    Core.Logger.Debug(
                        $"[Gem Upgrade Coroutine] {newGem.Name} {newGem.Rank} [{newGem.Guid}]");
                    Core.Logger.Debug(
                        $"[Gem Upgrade Coroutine] {currentGem.Name} {currentGem.Rank} [{currentGem.Guid}]");
                    Core.Logger.Debug(existingGem.ToString());
                    GemUpgrades.Save();
                }
                else
                    Core.Logger.Error("[Gem Upgrade Coroutine] Unable to find Gem Upgrade.");
                Core.Logger.Debug("[Gem Upgrade Coroutine] Gem upgrades left after the attempt: {0}",
                    ZetaDia.Me.JewelUpgradesLeft);
            }
            //}
        }

        private void EnablePulse()
        {
            if (!_isPulsing)
            {
                Core.Logger.Debug("[Gem Upgrade Coroutine] Registered to pulsator.");
                Pulsator.OnPulse += OnPulse;
                _isPulsing = true;
            }
        }

        private void DisablePulse()
        {
            if (_isPulsing)
            {
                Core.Logger.Debug("[Gem Upgrade Coroutine] Unregistered from pulsator.");
                Pulsator.OnPulse -= OnPulse;
                _isPulsing = false;
            }
        }

        private void OnPulse(object sender, EventArgs e)
        {
            if (_pulseTimer.IsFinished)
            {
                _pulseTimer.Stop();
                Scans();
                _pulseTimer.Reset();
            }
        }

        private void Scans()
        {
            switch (State)
            {
                case States.SearchingForUrshi:
                    ScanForUrshi();
                    return;
            }
        }

        private void ScanForUrshi()
        {
            var urshi =
                ZetaDia.Actors.GetActorsOfType<DiaUnit>()
                    .FirstOrDefault(a => a.IsFullyValid() && a.ActorSnoId == RiftData.UrshiSNO);
            if (urshi != null)
            {
                _urshiLocation = urshi.Position;
            }
            if (_urshiLocation != Vector3.Zero)
            {
                Core.Logger.Log("[Gem Upgrade Coroutine] Urshi is near.");
                State = States.MovingToUrshi;
                Core.Logger.Debug("[Gem Upgrade Coroutine] Found Urshi at distance {0}", AdvDia.MyPosition.Distance(_urshiLocation));
            }
        }

        private void PulseChecks()
        {
            if (BrainBehavior.IsVendoring || !ZetaDia.IsInGame || ZetaDia.Globals.IsLoadingWorld || ZetaDia.Globals.IsPlayingCutscene)
            {
                DisablePulse();
            }
        }

        public void Dispose()
        {
            DisablePulse();
        }

        Task<bool> ICoroutine.GetCoroutine()
        {
            return GetCoroutine();
        }

        public void Reset()
        {
            DisablePulse();
            State = States.NotStarted;
        }

        public string StatusText { get; set; }

        private bool Completed()
        {
            DisablePulse();
            return true;
        }

        private bool Failed()
        {
            DisablePulse();
            return true;
        }
    }
}