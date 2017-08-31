using Trinity.Components.Combat.Resources;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Routines.PhelonsPlayground.Utils;
using Zeta.Common;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer.Inarius
{
    using DbProvider;
    using Framework.Reference;
    using static Basics;
    using static Basics.Spells;

    public partial class Inarius
    {
        public TrinityPower OffensivePower()
        {
            Target = Targeting.BestAoeUnit(45);
            if (Target == null)
                return null;

            Vector3 location;
            TrinityActor target = Target;

            if (Target.RadiusDistance < 65f)
            {
                if (ShouldBloodRush(50f, out location))
                    return BloodRush(Target.Position);

                if (ShouldBoneArmor())
                    return BoneArmor();

                if (ShouldDevour())
                    return Devour();

                if (ShouldLandOfTheDead())
                    return LandOfTheDead();

                if (ShouldSimulacrum())
                    return Simulacrum(Target.Position);

                if (ShouldLeech(out target))
                    return Leech(target);

                if (ShouldFrailty(out target))
                    return Frailty(target);

                if (ShouldDecrepify(out target))
                    return Decrepify(target);

                if (ShouldBoneSpirit())
                    return BoneSpirit(Target);

                if (ShouldSkeletalMage())
                    return SkeletalMage(Target);

                if (ShouldCommandSkeletons())
                    return CommandSkeletons(Target);

                if (ShouldCorpseExplosion())
                    return CorpseExplosion(Target.Position);

                if (ShouldBoneSpikes())
                    return BoneSpikes(Target);

                if (ShouldSiphonBlood())
                    return SiphonBlood(Target);

                if (ShouldGrimScythe(out target))
                    return GrimScythe(target);
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
                return Devour();

            if (ShouldBoneArmor())
                return BoneArmor();
            return null;
        }

        public TrinityPower DestructiblePower()
        {
            return null;
        }

        public TrinityPower MovementPower(Vector3 destination)
        {
            if (Player.IsInTown)
                return null;

            if (PlayerMover.IsBlocked ||
                destination.Distance(Player.Position) > 30 && Skills.Necromancer.BloodRush.TimeSinceUse > 500)
                return BloodRush(destination);

            return destination.Distance(Player.Position) > 7f ? Walk(destination) : null;
        }
    }
}