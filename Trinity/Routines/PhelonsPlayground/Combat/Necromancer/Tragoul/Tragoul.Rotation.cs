using Zeta.Game.Internals.Actors;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer.Tragoul
{
    using Trinity.Components.Combat.Resources;
    using Trinity.Framework.Actors.ActorTypes;
    using Trinity.Framework.Objects;
    using Trinity.Framework.Reference;
    using Trinity.Routines.PhelonsPlayground.Utils;
    using Zeta.Common;
    using static Basics;
    using static Basics.Conditionals;
    using static Basics.Spells;
    using static Movement;

    public partial class Tragoul
    {
        public TrinityPower OffensivePower()
        {
            Target = Targeting.BestAoeUnit(45f, true);
            if (Target == null)
                return null;

            //Core.Logger.Warn(LogCategory.Routine, $"[Current Target] - Name: {Target.Name} | Elite: {Target.IsElite || Target.IsBoss || Target.IsChampion}.");
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
                    return Simulacrum();

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
            if (ShouldWalk(out location, 65f))
                Walk(location, 3f);

            return null;
        }
    }
}