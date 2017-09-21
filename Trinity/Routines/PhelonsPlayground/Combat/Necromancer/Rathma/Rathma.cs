using System;
using System.Collections.Generic;
using Trinity.Components.Combat.Resources;
using Trinity.Framework.Objects;
using Trinity.Framework.Reference;
using Trinity.Settings;
using Zeta.Common;
using Zeta.Game;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer.Rathma
{
    using System.ComponentModel;
    using System.Windows.Controls;
    using DbProvider;
    using Framework.Helpers;
    using UI;
    using Utils;
    using static Basics;
    using static Basics.Conditionals;

    public partial class Rathma : RoutineBase, IRoutine
    {
        #region Definition

        public string DisplayName => "Necromonger";
        public string Description => "Phelons Playground - Necromancer Rathma Set";
        public string Author => "Phelon";
        public string Version => "1.0";
        public string Url => "https://www.thebuddyforum.com/threads/phelons-playground-4-man-botting-routine.403317/";
        public Build BuildRequirements => new Build
        {
            Sets = new Dictionary<Set, SetBonus>
            {
                { Sets.BonesOfRathma, SetBonus.Third }
            },
            Skills = new Dictionary<Skill, Rune>
            {
                //{ Skills.Necromancer.SkeletalMage, null },
                //{ Skills.Necromancer.CommandSkeletons, null },
                //{ Skills.Necromancer.LandOfTheDead, null },
                //{ Skills.Necromancer.BoneSpikes, null },
                //{ Skills.Necromancer.Decrepify, null },
                //{ Skills.Necromancer.Devour, null },
            },
        };
        public IDynamicSetting RoutineSettings { get; }

        #endregion

        #region IRoutine Defaults

        public virtual ActorClass Class => ActorClass.Necromancer;
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
            return null;
        }

        private int _lastTarget;
        public TrinityPower GetBuffPower()
        {
            if (ShouldDevour())
                return Spells.Devour();
            return null;
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
        public Rathma.RathmaSettings Settings { get; } = new Rathma.RathmaSettings();

        public sealed class RathmaSettings : NotifyBase, IDynamicSetting
        {
            private int _clusterSize;
            private int _cooldownTrashSize;
            private float _emergencyHealthPct;
            private SkillSettings _coolDowns;

            [DefaultValue(8)]
            public int ClusterSize
            {
                get { return _clusterSize; }
                set { SetField(ref _clusterSize, value); }
            }
            [DefaultValue(30)]
            public int CooldownTrashSize
            {
                get { return _cooldownTrashSize; }
                set { SetField(ref _cooldownTrashSize, value); }
            }

            [DefaultValue(0.4f)]
            public float EmergencyHealthPct
            {
                get { return _emergencyHealthPct; }
                set { SetField(ref _emergencyHealthPct, value); }
            }
            
            public SkillSettings Cooldowns
            {
                get { return _coolDowns; }
                set { SetField(ref _coolDowns, value); }
            }


            #region Skill Defaults

            private static readonly SkillSettings CooldownsDefaults = new SkillSettings
            {
                UseMode = UseTime.Always,
                Reasons = UseReasons.Elites | UseReasons.Surrounded | UseReasons.Champions
            };

            #endregion

            public override void LoadDefaults()
            {
                base.LoadDefaults();
                Cooldowns = CooldownsDefaults.Clone();
            }
            public string GetName() => GetType().Name;
            public UserControl GetControl() => UILoader.LoadXamlByFileName<UserControl>(GetName() + ".xaml");
            public object GetDataContext() => this;
            public string GetCode() => JsonSerializer.Serialize(this);
            public void ApplyCode(string code) => JsonSerializer.Deserialize(code, this, true);
            public void Reset() => LoadDefaults();
            public void Save() { }
        }

        #endregion
    }
}
