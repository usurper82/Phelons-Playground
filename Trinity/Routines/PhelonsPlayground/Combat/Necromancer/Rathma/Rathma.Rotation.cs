using Zeta.Game.Internals.Actors;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer.Rathma
{
    using Trinity.Components.Combat.Resources;
    using Trinity.Framework.Actors.ActorTypes;
    using Trinity.Framework.Objects;
    using Trinity.Framework.Reference;
    using Trinity.Routines.PhelonsPlayground.Utils;
    using Zeta.Common;
    public partial class Rathma
    {
        public static TrinityActor Target = CurrentTarget;
        public TrinityPower OffensivePower()
        {
            Target = Targeting.BestAoeUnit(45f);
            if (Target == null)
                return null;

            Vector3 location;
            TrinityActor target = Target;

            if (Target.RadiusDistance < 65f)
            {
                if (ShouldBloodRush(50f, out location))
                    return Spells.BloodRush(Target.Position);

                if (ShouldBoneArmor())
                    return Spells.BoneArmor();

                if (ShouldDevour())
                    return Spells.Devour();

                if (ShouldFrailty(out target))
                    return Spells.Frailty(target);

                if (ShouldDecrepify(out target))
                    return Spells.Decrepify(target);

                if (ShouldLandOfTheDead())
                    return Spells.LandOfTheDead();

                if (ShouldSimulacrum())
                    return Spells.Simulacrum(Target.Position);

                if (ShouldSkeletalMage())
                    return Spells.SkeletalMage(Target);

                if (ShouldCommandSkeletons())
                    return Spells.CommandSkeletons(Target);

                if (ShouldCorpseExplosion())
                    return Spells.CorpseExplosion(Target.Position);

                if (ShouldBoneSpikes())
                    return Spells.BoneSpikes(Target);

                if (ShouldSiphonBlood())
                    return Spells.SiphonBlood(Target);

                if (ShouldGrimScythe(out target))
                    return Spells.GrimScythe(target);
            }
            if (ShouldWalk(out location))
                Walk(location, 3f);

            return null;
        }

        public TrinityPower DefensivePower()
        {
            return null;
        }

        public TrinityPower BuffPower()
        {
            if (ShouldDevour())
                return Spells.Devour();
            return null;
        }

        public TrinityPower DestructiblePower()
        {
            if (ShouldSkeletalMage())
                return Spells.SkeletalMage(CurrentTarget);

            if (ShouldCommandSkeletons())
                return Spells.CommandSkeletons(CurrentTarget);

            if (ShouldBoneSpikes())
                return Spells.BoneSpikes(Target);

            if (ShouldSiphonBlood())
                return Spells.SiphonBlood(Target);

            if (ShouldGrimScythe(out Target))
                return Spells.GrimScythe(Target);
            return null;
        }

        public TrinityPower MovementPower(Vector3 destination)
        {
            return null; //return Walk(destination);
        }
    }
}