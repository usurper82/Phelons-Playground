﻿using Zeta.Game.Internals.Actors;

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
            Target = Targeting.BestAoeUnit(45f, true);
            if (Target == null)
                return null;

            Vector3 location;
            TrinityActor target = Target;

            if (ShouldBloodRush(35f, out location))
                return Spells.BloodRush(Target.Position);

            if (ShouldWalkToBuff(out location))
                return Walk(location, 3f);

            if (Target.RadiusDistance < 50f)
            {

                if (ShouldBoneArmor())
                    return Spells.BoneArmor();

                if (ShouldLandOfTheDead())
                    return Spells.LandOfTheDead();

                if (ShouldSimulacrum())
                    return Spells.Simulacrum(Target.Position);

                if (ShouldSkeletalMage())
                    return Spells.SkeletalMage(Target);

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
                    return Spells.BoneSpikes(Target);

                if (ShouldSiphonBlood())
                    return Spells.SiphonBlood(Target);

                if (ShouldGrimScythe(out target))
                    return Spells.GrimScythe(target);
            }
            if (ShouldWalk(out location))
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