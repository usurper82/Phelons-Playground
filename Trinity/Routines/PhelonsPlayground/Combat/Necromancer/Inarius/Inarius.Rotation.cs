using Trinity.Components.Combat.Resources;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Routines.PhelonsPlayground.Utils;
using Zeta.Common;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer.Inarius
{
    using DbProvider;
    using Framework.Reference;

    public partial class Inarius
    {
        public static TrinityActor Target = null;

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
                    return Spells.BloodRush(Target.Position);

                if (ShouldBoneArmor())
                    return Spells.BoneArmor();

                if (ShouldDevour())
                    return Spells.Devour();

                if (ShouldLandOfTheDead())
                    return Spells.LandOfTheDead();

                if (ShouldSimulacrum())
                    return Spells.Simulacrum(Target.Position);

                if (ShouldLeech(out target))
                    return Spells.Leech(target);

                if (ShouldFrailty(out target))
                    return Spells.Frailty(target);

                if (ShouldDecrepify(out target))
                    return Spells.Decrepify(target);

                if (ShouldBoneSpirit())
                    return Spells.BoneSpirit(Target);

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

            if (ShouldBoneArmor())
                return Spells.BoneArmor();
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
                return Spells.BloodRush(destination);

            return destination.Distance(Player.Position) > 7f ? Walk(destination) : null;
        }
    }
}