using Trinity.Components.Combat.Resources;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Routines.PhelonsPlayground.Utils;
using Zeta.Common;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer.Inarius
{
    using DbProvider;
    using Framework;
    using Framework.Helpers;
    using Framework.Reference;
    using static Basics;
    using static Basics.Conditionals;
    using static Basics.Spells;
    using static Movement;

    public partial class Inarius
    {
        public TrinityPower OffensivePower()
        {
            Target = Targeting.BestAoeUnit(45);
            if (Target == null)
                return null;

            //Core.Logger.Warn(LogCategory.Routine, $"[Current Target] - Name: {Target.Name} | Elite: {Target.IsElite || Target.IsBoss || Target.IsChampion}.");

            Vector3 location;
            TrinityActor target = Target;

            if (ShouldBloodRush(50f, out location))
                return BloodRush(Target.Position);

            if (ShouldBoneArmor())
                return BoneArmor();

            if (Target.RadiusDistance < 12f)
            {
                if (ShouldDevour())
                    return Devour();

                if (ShouldLandOfTheDead())
                    return LandOfTheDead();

                if (ShouldSimulacrum())
                    return Simulacrum();

                if (ShouldLeech(out target))
                    return Leech(target);

                if (ShouldFrailty(out target))
                    return Frailty(target);

                if (ShouldDecrepify(out target))
                    return Decrepify(target);

                if (ShouldBoneSpirit())
                    return BoneSpirit(Target);

                if (ShouldCorpseExplosion())
                    return CorpseExplosion(Target.Position);

                if (ShouldBoneSpikes())
                    return BoneSpikes(Target);

                if (ShouldSiphonBlood())
                    return SiphonBlood(Target);

                if (ShouldGrimScythe(out target))
                    return GrimScythe(target);
            }
            if (ShouldWalk(out location, 12f))
                return Walk(location, 3f);

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