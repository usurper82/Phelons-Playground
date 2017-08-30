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

    public partial class Rathma : RoutineBase, IRoutine
    {
        #region Definition

        public string DisplayName => "Necromonger";
        public string Description => "Phelons Playground - Necromancer Rathma Set";
        public string Author => "Phelon";
        public string Version => "0.2";
        public string Url => "https://www.thebuddyforum.com/threads/phelons-playground-4-man-botting-routine.403317/";
        public Build BuildRequirements => new Build
        {
            Sets = new Dictionary<Set, SetBonus>
            {
                { Sets.BonesOfRathma, SetBonus.Third }
            },
            Skills = new Dictionary<Skill, Rune>
            {
                { Skills.Necromancer.SkeletalMage, null },
                { Skills.Necromancer.CommandSkeletons, null },
                { Skills.Necromancer.LandOfTheDead, null },
                { Skills.Necromancer.BoneSpikes, null },
                { Skills.Necromancer.Decrepify, null },
                { Skills.Necromancer.Devour, null },
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
            //if (CurrentTarget != null)
            //    return null;
            //var closestTarget = Targeting.ClosestUnit(35f);
            //if (closestTarget == null)
            //    return null;
            //if (Skills.Necromancer.CommandSkeletons.TimeSinceUse < closestTarget.Distance*1000 && Player.PrimaryResourcePct < 0.50 &&
            //    closestTarget.AcdId != _lastTarget)
            //    return null;
            //_lastTarget = closestTarget.AcdId;
            //return Spells.CommandSkeletons(closestTarget);
            return null;
        }

        public TrinityPower GetDestructiblePower()
        {
            return DestructiblePower();
        }

        public TrinityPower GetMovementPower(Vector3 destination)
        {
            return PlayerMover.IsBlocked && Skills.Necromancer.BloodRush.CanCast() ? Spells.BloodRush(destination) : Walk(destination);
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
