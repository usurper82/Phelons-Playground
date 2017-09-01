using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Routines.PhelonsPlayground.Combat.Witchdoctor.LoN
{
    using Components.Combat.Resources;
    using Framework.Actors.ActorTypes;
    using Utils;
    using Zeta.Common;
    using static Basics;
    using static Basics.Conditionals;
    using static Basics.Spells;
    using static Movement;

    public partial class LoN
    {
        public TrinityPower OffensivePower()
        {
            Target = Targeting.BestAoeUnit(45f, true);
            if (Target == null)
                return null;

            Vector3 location;
            TrinityActor target = Target;


            if (ShouldWalkToBuff(out location, Target.Position, 45f))
                return Walk(location, 3f);

            if (Target.RadiusDistance < 45f)
            {
                if (ShouldSoulHarvest())
                    return SoulHarvest(Player.Position);
                if (ShouldLocustSwarm(out target))
                    return LocustSwarm(target);
                if (ShouldHaunt(out target))
                    return Haunt(target);
                if (ShouldSpiritBarrage())
                    return SpiritBarrage(Target);
                if (ShouldSacrifice())
                    return Sacrifice();
                if (ShouldGraspOfTheDead(out target))
                    return GraspOfTheDead(target);
            }
            if (ShouldWalk(out location, 45f))
                return Walk(location, 3f);

            return null;
        }
    }
}
