using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using Trinity.Components.Combat.Resources;
using Trinity.DbProvider;
using Trinity.Framework;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Framework.Helpers;
using Trinity.Framework.Objects;
using Trinity.Framework.Reference;
using Trinity.Settings;
using Trinity.UI;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Routines.PhelonsPlayground.Combat.Monk.zDPS
{
    public partial class zDPSMonk : RoutineBase, IRoutine
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

        public string DisplayName => "zDPS Monk";
        public string Description => "Phelons Playground - Monk zDPS Inna's 2 Piece Set";
        public string Author => "Phelon"; // (Ported)
        public string Version => "0.1";
        public string Url => "https://www.thebuddyforum.com/threads/phelons-playground-4-man-botting-routine.403317/";
        public Build BuildRequirements => new Build
        {
            Sets = new Dictionary<Set, SetBonus>
            {
                { Sets.Innas, SetBonus.Second },
                { Sets.IstvansPairedBlades, SetBonus.First }
            },
            Skills = new Dictionary<Skill, Rune>
            {
                { Skills.Monk.InnerSanctuary, null },
                { Skills.Monk.CripplingWave, null },
            },
        };

        public IDynamicSetting RoutineSettings { get; }

        #endregion

        #region IRoutine Defaults

        public virtual ActorClass Class => ActorClass.Monk;
        public virtual int PrimaryEnergyReserve => 50;
        public virtual int SecondaryEnergyReserve => 0;
        public virtual KiteMode KiteMode => KiteMode.Never;
        public virtual float KiteDistance => 0;
        public virtual int KiteStutterDuration => 500;
        public virtual int KiteStutterDelay => 1000;
        public virtual int KiteHealthPct => 100;
        public virtual float TrashRange => 75f;
        public virtual float EliteRange => 200f;
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

        public TrinityPower GetDefensivePower()
        {
            return null;
        }

        public TrinityPower GetBuffPower()
        {
            return Player.IsInTown ? null : BuffPower();
        }

        public TrinityPower GetDestructiblePower()
        {
            return Player.IsInTown ? null : DestructiblePower();
        }

        public TrinityPower GetMovementPower(Vector3 destination)
        {
            return destination.Distance(Player.Position) > 7f ? Walk(destination) : null;
        }

        #region Settings

        public override int ClusterSize => Settings.ClusterSize;
        public override float EmergencyHealthPct => Settings.EmergencyHealthPct;

        IDynamicSetting IRoutine.RoutineSettings => Settings;
        public zDPSMonkSettings Settings { get; } = new zDPSMonkSettings();

        public sealed class zDPSMonkSettings : NotifyBase, IDynamicSetting
        {
            private SkillSettings _epiphany;
            private float _cycloneStrikeDelay;
            private int _cycloneStrikeMinMobs;
            private float _innerSanctuaryDelay;
            private int _innerSanctuaryMinRange;
            private float _mantraDelay;
            private int _clusterSize;
            private float _emergencyHealthPct;

            [DefaultValue(8)]
            public int ClusterSize
            {
                get { return _clusterSize; }
                set { SetField(ref _clusterSize, value); }
            }

            [DefaultValue(2000)]
            public float CycloneStrikeDelay
            {
                get { return _cycloneStrikeDelay; }
                set { SetField(ref _cycloneStrikeDelay, value); }
            }

            [DefaultValue(3)]
            public int CycloneStrikeMinMobs
            {
                get { return _cycloneStrikeMinMobs; }
                set { SetField(ref _cycloneStrikeMinMobs, value); }
            }

            [DefaultValue(5000)]
            public float InnerSanctuaryDelay
            {
                get { return _innerSanctuaryDelay; }
                set { SetField(ref _innerSanctuaryDelay, value); }
            }

            [DefaultValue(15)]
            public int InnerSanctuaryMinRange
            {
                get { return _innerSanctuaryMinRange; }
                set { SetField(ref _innerSanctuaryMinRange, value); }
            }

            [DefaultValue(3000)]
            public float MantraDelay
            {
                get { return _mantraDelay; }
                set { SetField(ref _mantraDelay, value); }
            }

            [DefaultValue(0.4f)]
            public float EmergencyHealthPct
            {
                get { return _emergencyHealthPct; }
                set { SetField(ref _emergencyHealthPct, value); }
            }

            public SkillSettings Epiphany
            {
                get { return _epiphany; }
                set { SetField(ref _epiphany, value); }
            }


            #region Skill Defaults

            private static readonly SkillSettings EpiphanyDefaults = new SkillSettings
            {
                UseMode = UseTime.Always,
                Reasons = UseReasons.Elites | UseReasons.HealthEmergency
            };

            #endregion

            public override void LoadDefaults()
            {
                base.LoadDefaults();
                Epiphany = EpiphanyDefaults.Clone();
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

