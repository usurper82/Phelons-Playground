using Zeta.Game.Internals.Actors;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer.Tragoul
{
    using Trinity.Components.Combat.Resources;
    using Trinity.Framework.Actors.ActorTypes;
    using Trinity.Framework.Objects;
    using Trinity.Framework.Reference;
    using Trinity.Routines.PhelonsPlayground.Utils;
    using Zeta.Common;
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
                if (ShouldWalk(50f, out location))
                    return Spells.BloodRush(Target.Position);

                if (ShouldDevour())
                    return Spells.Devour();

                if (ShouldLandOfTheDead())
                    return Spells.LandOfTheDead();

                if (ShouldSimulacrum())
                    return Spells.Simulacrum(Target.Position);

                if (ShouldFrailty(out target))
                    return Spells.Frailty(target);

                if (ShouldDecrepify(out target))
                    return Spells.Decrepify(target);

                if (ShouldCorpseLance())
                    return Spells.CorpseLance(Target);

                if (ShouldBoneSpikes())
                    return Spells.BoneSpikes(Target);

                if (ShouldSiphonBlood())
                    return Spells.SiphonBlood(Target);

                if (ShouldGrimScythe())
                    return Spells.GrimScythe(Target);
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
                return Spells.Devour();
            return null;
        }

        public TrinityPower DestructiblePower()
        {

            if (ShouldBoneSpikes())
                return Spells.BoneSpikes(Target);

            if (ShouldSiphonBlood())
                return Spells.SiphonBlood(Target);

            if (ShouldGrimScythe())
                return Spells.GrimScythe(Target);
            return null;
            //return Spells.SiphonBlood();
        }

        public TrinityPower MovementPower(Vector3 destination)
        {
            if (Player.IsInTown)
                return null;

            if (CanPortTo(destination) && Skills.Necromancer.BloodRush.TimeSinceUse > 500)
                return Spells.BloodRush(destination);

            return destination.Distance(Player.Position) > 7f ? Walk(destination) : null;
        }
    }
}