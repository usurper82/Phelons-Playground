using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Routines.PhelonsPlayground.Combat.Witchdoctor.LoN
{
    using System.ComponentModel;
    using System.Windows.Controls;
    using Components.Combat.Resources;
    using Framework.Helpers;
    using Framework.Objects;
    using Framework.Reference;
    using Necromancer;
    using Settings;
    using UI;
    using Zeta.Common;
    using Zeta.Game;
    using static Basics;

    public partial class LoN : RoutineBase, IRoutine
    {
        #region Definition

        public string DisplayName => "LoN Barrage";
        public string Description => "Phelons Playground - Witch doctor LoN Set";
        public string Author => "Phelon";
        public string Version => "1.0";
        public string Url => "https://www.thebuddyforum.com/threads/phelons-playground-4-man-botting-routine.403317/";
        public Build BuildRequirements => new Build
        {
            Sets = new Dictionary<Set, SetBonus>
            {
                { Sets.LegacyOfNightmares, SetBonus.First }
            },
            Skills = new Dictionary<Skill, Rune>
            {
                { Skills.WitchDoctor.LocustSwarm, null },
                { Skills.WitchDoctor.Haunt, null },
                { Skills.WitchDoctor.Sacrifice, null },
                { Skills.WitchDoctor.GraspOfTheDead, null },
                { Skills.WitchDoctor.SoulHarvest, null },
                { Skills.WitchDoctor.SpiritBarrage, null }
            },
        };
        public IDynamicSetting RoutineSettings { get; }

        #endregion



        #region IRoutine Defaults

        public virtual ActorClass Class => ActorClass.Witchdoctor;
        public virtual int PrimaryEnergyReserve => 80;
        public virtual int SecondaryEnergyReserve => 0;
        public virtual KiteMode KiteMode => KiteMode.Never;
        public virtual float KiteDistance => 15f;
        public virtual int KiteStutterDuration => 800;
        public virtual int KiteStutterDelay => 1400;
        public virtual int KiteHealthPct => 100;
        public virtual float TrashRange => 65f;
        public virtual float EliteRange => 120f;
        public virtual float HealthGlobeRange => 60f;
        public virtual float ShrineRange => 80f;
        public virtual Func<bool> ShouldIgnoreNonUnits { get; } = () => false;
        public virtual Func<bool> ShouldIgnorePackSize { get; } = () => false;
        public virtual Func<bool> ShouldIgnoreAvoidance { get; } = () => false;
        public virtual Func<bool> ShouldIgnoreKiting { get; } = () => false;
        public virtual Func<bool> ShouldIgnoreFollowing { get; } = () => false;

        #endregion

        #region Skill Selection

        public TrinityPower GetOffensivePower()
        {
            return Player.IsInTown ? null : OffensivePower();
        }

        public TrinityPower GetDefensivePower()
        {
            return DefensivePower();
        }

        public TrinityPower GetBuffPower()
        {
            return BuffPower();
        }

        public TrinityPower GetDestructiblePower()
        {
            return DestructiblePower();
        }

        public TrinityPower GetMovementPower(Vector3 destination)
        {
            return MovementPower(destination);
        }

        #endregion

        #region Settings

        public override int ClusterSize => Settings.ClusterSize;
        public override float EmergencyHealthPct => Settings.EmergencyHealthPct;

        IDynamicSetting IRoutine.RoutineSettings => Settings;
        public LoN.LoNSettings Settings { get; } = new LoN.LoNSettings();

        public sealed class LoNSettings : NotifyBase, IDynamicSetting
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
