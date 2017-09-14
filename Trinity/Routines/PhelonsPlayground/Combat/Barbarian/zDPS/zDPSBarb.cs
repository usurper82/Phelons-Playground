using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using Trinity.Components.Combat.Resources;
using Trinity.Framework.Helpers;
using Trinity.Framework.Objects;
using Trinity.Framework.Reference;
using Trinity.Routines.PhelonsPlayground.Utils;
using Trinity.Settings;
using Trinity.UI;
using Zeta.Common;
using Zeta.Game;

namespace Trinity.Routines.PhelonsPlayground.Combat.Barbarian.zDPS
{
    public partial class zDPSBarb : RoutineBase, IRoutine
    {
        // Important Notes!

        // Keep UI settings out of here! override in derived class with your settings
        // then call base.ShouldWhatever() if you dont wan't to duplicate the basic checks.

        // The bot will try to move into the range specified BEFORE casting a power.
        // => new TrinityPower(Skills.Barbarian.Leap, 15f, position)
        // means GetMovementPower() until within 15f of position, then cast Leap.

        // Wrap buff SNOPowers as expressions, because nobody is going to 
        // remember what P3_ItemPassive_Unique_Ring_010 means.

        // Use the Routine LogCategory for logging.
        // Core.Logger.Error(LogCategory.Routine, $"My Current Target is {CurrentTarget}");

        #region Definition

        public string DisplayName => "zDPS Barb";
        public string Description => "Phelons Playground - Barbarian zDPS Raekor Set";
        public string Author => "Phelon"; // (Ported)
        public string Version => "0.1";
        public string Url => "https://www.thebuddyforum.com/threads/phelons-playground-4-man-botting-routine.403317/";

        public Build BuildRequirements => new Build
        {
            Sets = new Dictionary<Set, SetBonus>
            {
                //{ Sets.TheLegacyOfRaekor, SetBonus.Second },
                //{ Sets.WrathOfTheWastes, SetBonus.Second },
            },
            Skills = new Dictionary<Skill, Rune>
            {
                //{ Skills.Barbarian.FuriousCharge, null },
                //{ Skills.Barbarian.AncientSpear, Runes.Barbarian.RageFlip },
            }
        };

        #endregion

        #region IRoutine Defaults

        public virtual ActorClass Class { get; } = ActorClass.Barbarian;
        public virtual int PrimaryEnergyReserve => 25;
        public virtual int SecondaryEnergyReserve => 0;
        public virtual KiteMode KiteMode => KiteMode.Never;
        public virtual float KiteDistance => 15f;
        public virtual int KiteStutterDuration => 800;
        public virtual int KiteStutterDelay => 1400;
        public virtual int KiteHealthPct => 100;
        public virtual float TrashRange => 75f;
        public virtual float EliteRange => 120f;
        public virtual float HealthGlobeRange => 60f;
        public virtual float ShrineRange => 80f;
        public virtual Func<bool> ShouldIgnoreNonUnits { get; } = () => false;
        public virtual Func<bool> ShouldIgnorePackSize { get; } = () => false;
        public virtual Func<bool> ShouldIgnoreAvoidance { get; } = () => false;
        public virtual Func<bool> ShouldIgnoreKiting { get; } = () => false;
        public virtual Func<bool> ShouldIgnoreFollowing { get; } = () => false;

        #endregion


        public TrinityPower GetOffensivePower()
        {
            return Player.IsInTown ? null : OffensivePower();
        }

        public TrinityPower GetBuffPower()
        {
            return Player.IsInTown ? null : BuffPower();
        }

        public TrinityPower GetDefensivePower()
        {
            var position = Targeting.HealthGlobeExists(35f) ? Targeting.GetBestHealthGlobeClusterPoint(35f) : Targeting.GetLoiterPosition(Targeting.ClosestUnit(35f, Target), 35f);
            return Spells.FuriousCharge(position);
        }

        public TrinityPower GetMovementPower(Vector3 destination)
        {
            if (ShouldSprint())
                return Spells.Sprint();

            if (!Player.IsInTown)
            {

                if (Skills.Barbarian.Whirlwind.CanCast() && CurrentTarget != null && !CurrentTarget.IsGizmo)
                    return Spells.Whirlwind(destination);

                if (CanChargeTo(destination) && Skills.Barbarian.FuriousCharge.TimeSinceUse > 500)
                {
                    // Limit movement casts so we have stacks to charge units and build more stacks.

                    var chargeStacks = Skills.Barbarian.FuriousCharge.BuffStacks;
                    var isImportantTarget = CurrentTarget != null && _importantActors.Contains(CurrentTarget.Type);

                    if (IsInCombat && (chargeStacks == 3 || isImportantTarget && chargeStacks > 1) ||
                        TargetUtil.PierceHitsMonster(destination))
                    {
                        return Spells.FuriousCharge(destination);
                    }
                }
            }
            return Walk(destination);
        }

        public TrinityPower GetDestructiblePower() => DefaultPower;

        #region Settings

        public override int ClusterSize => Settings.ClusterSize;
        public override float EmergencyHealthPct => Settings.EmergencyHealthPct;

        IDynamicSetting IRoutine.RoutineSettings => Settings;
        public zDPSBarbSettings Settings { get; } = new zDPSBarbSettings();

        public sealed class zDPSBarbSettings : NotifyBase, IDynamicSetting
        {
            private int _clusterSize;
            private float _emergencyHealthPct;

            [DefaultValue(8)]
            public int ClusterSize
            {
                get { return _clusterSize; }
                set { SetField(ref _clusterSize, value); }
            }

            [DefaultValue(0.4f)]
            public float EmergencyHealthPct
            {
                get { return _emergencyHealthPct; }
                set { SetField(ref _emergencyHealthPct, value); }
            }

            #region IDynamicSetting

            public string GetName() => GetType().Name;
            public UserControl GetControl() => UILoader.LoadXamlByFileName<UserControl>(GetName() + ".xaml");
            public object GetDataContext() => this;
            public string GetCode() => JsonSerializer.Serialize(this);
            public void ApplyCode(string code) => JsonSerializer.Deserialize(code, this, true);
            public void Reset() => LoadDefaults();
            public void Save() { }

            #endregion
        }

        #endregion

    }

}
