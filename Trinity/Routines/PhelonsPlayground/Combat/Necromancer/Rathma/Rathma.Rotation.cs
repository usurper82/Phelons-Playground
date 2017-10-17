using Zeta.Game.Internals.Actors;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer.Rathma
{
    using System;
    using Framework;
    using Framework.Helpers;
    using Trinity.Components.Combat.Resources;
    using Trinity.Framework.Actors.ActorTypes;
    using Trinity.Framework.Objects;
    using Trinity.Framework.Reference;
    using Trinity.Routines.PhelonsPlayground.Utils;
    using Zeta.Common;
    using static Basics;
    using static Basics.Conditionals;
    using static Movement;

    public partial class Rathma
    {
        public TrinityPower OffensivePower()
        {
            var castDistance = Skills.Necromancer.GrimScythe.IsActive ? 10f : 45f;

            Target = Targeting.BestTarget(castDistance, true);
            if (Target == null)
                return null;

            //Core.Logger.Warn(LogCategory.Routine, $"[Current Target] - Name: {Target.Name} | Elite: {Target.IsElite || Target.IsBoss || Target.IsChampion}.");

            Vector3 location;
            TrinityActor target = Target;

            if (ShouldBoneArmor())
                return Spells.BoneArmor();

            //if (ShouldWalkToBuff(out location, Target.Position, Math.Min(castDistance, 25f)))
            //return Walk(location, 3f);
            if (AllowedToUse(Settings.Cooldowns, Skills.Necromancer.LandOfTheDead) && ShouldLandOfTheDead())
                return Spells.LandOfTheDead();

            if (AllowedToUse(Settings.Cooldowns, Skills.Necromancer.Simulacrum) && ShouldSimulacrum())
                return Spells.Simulacrum();

            if (ShouldSkeletalMage())
                return Spells.SkeletalMage(Target);

            if (ShouldBloodRush(45f, out location))
                return Spells.BloodRush(location);

            if (ShouldCommandSkeletons())
                return Spells.CommandSkeletons(Target);

            if (ShouldDevour())
                return Spells.Devour();

            if (ShouldFrailty(out target))
                return Spells.Frailty(target);

            if (ShouldDecrepify(out target))
                return Spells.Decrepify(target);

            if (ShouldCorpseExplosion())
                return Spells.CorpseExplosion(Target.Position);

            if (ShouldBoneSpikes())
                return Spells.BoneSpikes(Targeting.GetBestClusterUnit(12f, castDistance, false, true));

            if (ShouldSiphonBlood())
                return Spells.SiphonBlood(Target);

            if (ShouldGrimScythe(out target))
                return Spells.GrimScythe(target);

            return ShouldWalk(out location, castDistance) ? Walk(location) : null;
        }
    }
}