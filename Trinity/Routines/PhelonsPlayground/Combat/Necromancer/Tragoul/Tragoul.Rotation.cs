using Zeta.Game.Internals.Actors;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer.Tragoul
{
    using Trinity.Components.Combat.Resources;
    using Trinity.Framework.Actors.ActorTypes;
    using Trinity.Framework.Objects;
    using Trinity.Framework.Reference;
    using Trinity.Routines.PhelonsPlayground.Utils;
    using Zeta.Common;
    using static Basics.Spells;

    public partial class Tragoul
    {
        public static TrinityActor Target = CurrentTarget;
        public TrinityPower OffensivePower()
        {
            Target = Targeting.BestAoeUnit();
            if (Target == null)
                return null;

            Vector3 location;
            TrinityActor target = Target;

            if (Target.RadiusDistance < 65f)
            {
                if (ShouldBloodRush(50f, out location))
                    return BloodRush(Target.Position);

                if (ShouldDevour())
                    return Devour();

                if (ShouldLandOfTheDead())
                    return LandOfTheDead();

                if (ShouldSimulacrum())
                    return Simulacrum(Target.Position);

                if (ShouldFrailty(out target))
                    return Frailty(target);

                if (ShouldDecrepify(out target))
                    return Decrepify(target);

                if (ShouldCorpseLance())
                    return CorpseLance(Target);

                if (ShouldBoneSpikes())
                    return BoneSpikes(Target);

                if (ShouldSiphonBlood())
                    return SiphonBlood(Target);

                if (ShouldGrimScythe(out target))
                    return GrimScythe(target);
            }
            if (ShouldWalk(15f, out location))
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
            return null;
        }

        public TrinityPower DestructiblePower()
        {
            if (ShouldBoneSpikes())
                return BoneSpikes(Target);

            if (ShouldSiphonBlood())
                return SiphonBlood(Target);

            if (ShouldGrimScythe(out Target))
                return GrimScythe(Target);
            return null;
            //return Spells.SiphonBlood();
        }

        public TrinityPower MovementPower(Vector3 destination)
        {
            if (Player.IsInTown)
                return null;

            if (CanPortTo(destination) && Skills.Necromancer.BloodRush.TimeSinceUse > 500)
                return BloodRush(destination);

            return destination.Distance(Player.Position) > 7f ? Walk(destination) : null;
        }
    }
}