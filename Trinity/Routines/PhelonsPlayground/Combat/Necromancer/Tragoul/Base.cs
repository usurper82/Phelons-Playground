using System;
using System.Collections.Generic;
using Trinity.Framework.Objects;
using Trinity.Framework.Reference;
using Trinity.Settings;
using Zeta.Game;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer.Tragoul
{
    public class Base : RoutineBase
    {
        #region Definition

        public string DisplayName => "Corpselancer";
        public string Description => "Phelons Playground - Necromancer Rathma Set";
        public string Author => "Phelon";
        public string Version => "0.1";
        public string Url => "https://www.thebuddyforum.com/threads/phelons-playground-4-man-botting-routine.403317/";
        public Build BuildRequirements => new Build
        {
            Sets = new Dictionary<Set, SetBonus>
            {
                { Sets.TragoulsAvatar, SetBonus.Third }
            },
            Skills = new Dictionary<Skill, Rune>
            {
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
    }
}
