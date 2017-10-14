using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Components.Combat.Resources;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Framework.Reference;
using Trinity.Routines.PhelonsPlayground.Utils;
using Zeta.Common;

namespace Trinity.Routines.PhelonsPlayground.Combat.Monk.zDPS
{
    using Components.Coroutines.Town;
    using Framework;
    using Framework.Helpers;

    public partial class zDPSMonk
    {
        public static TrinityActor Target = CurrentTarget;
        public TrinityPower OffensivePower()
        {
            var distance = Skills.Monk.Epiphany.TimeSinceUse < 10000 ? 18 : 12;
            Target = Targeting.BestTarget(distance, true);
            if (Target == null)
                return null;

            TrinityPower power;

            if (ShouldEpiphany())
                return Spells.Epiphany();

            if (TryMantra(out power))
                return power;

            if (ShouldInnerSanctuary())
                return Spells.InnerSanctuary();

            if (ShouldCycloneStrike())
                return Spells.CycloneStrike();

            if (ShouldBlindingFlash())
                return Spells.BlindingFlash();

            TrinityActor target;
            if (ShouldCripplingWave(out target))
               return Spells.CripplingWave(target);
            return Walk(Target.Position);
        }

        public TrinityPower BuffPower()
        {
            if (Player.IsInTown || TrinityTownRun.IsTryingToTownPortal())
                return null;

            if (Skills.Monk.CycloneStrike.TimeSinceUse > 3000 && Player.PrimaryResource > 45 && Targeting.NumMobsInRange(22f) > 3)
                return Spells.CycloneStrike();

            TrinityPower power;
            if (TryMantra(out power))
                return power;

            return null;
        }

        public TrinityPower DestructiblePower() => DefaultPower;

        public TrinityPower MovementPower(Vector3 destination)
        {
            return Walk(destination);
        }
    }
}
