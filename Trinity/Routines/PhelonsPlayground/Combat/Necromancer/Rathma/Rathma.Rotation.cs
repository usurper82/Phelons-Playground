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
            Target = Targeting.BestAoeUnit();
            if (Target == null)
                return null;

            Vector3 location;
            TrinityActor target = Target;

            if (Target.RadiusDistance < 65f)
            {
                if (ShouldDevour())
                    return Spells.Devour();

                if (ShouldLandOfTheDead())
                    return Spells.LandOfTheDead();

                if (ShouldSimulacrum())
                    return Spells.Simulacrum(Target.Position);

                if (ShouldSkeletalMage())
                    return Spells.SkeletalMage(Target);

                if (ShouldFrailty(out target))
                    return Spells.Frailty(target);

                if (ShouldDecrepify(out target))
                    return Spells.Decrepify(target);

                if (ShouldCommandSkeletons())
                    return Spells.CommandSkeletons(Target);

                if (ShouldBoneSpikes())
                    return Spells.BoneSpikes(Target);
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
            return DefaultPower;
        }

        public TrinityPower MovementPower(Vector3 destination)
        {
            return null; //return Walk(destination);
        }
    }
}