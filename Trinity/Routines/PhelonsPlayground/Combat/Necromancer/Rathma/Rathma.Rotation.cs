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

            if (ShouldLandOfTheDead())
                return Spells.LandOfTheDead();
            TrinityActor target;
            if (ShouldDecrepify(out target))
                return Spells.Decrepify(target);

            if (ShouldSkeletalMage())
                return Spells.SkeletalMage(Target);

            if (ShouldCommandSkeletons())
                return Spells.CommandSkeletons(Target);

            if (ShouldBoneSpikes())
                return Spells.BoneSpikes(Target);

            if (ShouldWalk(out location))
                Walk(location, 3f);

            return null;
        }

        public static TrinityPower DefensivePower()
        {
            return null;
        }

        public static TrinityPower BuffPower()
        {
            return null;
        }

        public static TrinityPower DestructiblePower()
        {
            return DefaultPower;
        }

        public static TrinityPower MovementPower(Vector3 destination)
        {
            return null; //return Walk(destination);
        }
    }
}