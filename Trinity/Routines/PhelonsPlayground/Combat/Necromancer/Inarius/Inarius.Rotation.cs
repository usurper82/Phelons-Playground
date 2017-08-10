﻿using Trinity.Components.Combat.Resources;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Routines.PhelonsPlayground.Utils;
using Zeta.Common;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer.Inarius
{
    using Framework.Reference;

    public partial class Inarius
    {
        public static TrinityActor Target = CurrentTarget;
        public TrinityPower OffensivePower()
        {
            Target = Targeting.BestAoeUnit();
            if (Target == null)
                return null;

            Vector3 location;

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

                if (ShouldLeech(out Target))
                    return Spells.Leech(Target);

                if (ShouldFrailty(out Target))
                    return Spells.Frailty(Target);

                if (ShouldDecrepify(out Target))
                    return Spells.Decrepify(Target);

                if (ShouldBoneSpirit())
                    return Spells.BoneSpirit(Target);

                if (ShouldCorpseExplosion())
                    return Spells.CorpseExplosion(Target.Position);
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
            return Spells.BoneArmor();
        }

        public TrinityPower MovementPower(Vector3 destination)
        {
            if (Player.IsInTown)
                return null;

            if (Skills.Necromancer.BloodRush.TimeSinceUse > 500)
                return Spells.BloodRush(destination);

            return destination.Distance(Player.Position) > 7f ? Walk(destination) : null;
        }
    }
}