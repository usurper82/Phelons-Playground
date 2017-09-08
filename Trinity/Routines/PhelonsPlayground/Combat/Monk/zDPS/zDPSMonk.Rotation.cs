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
            Target = Targeting.BestTarget(185, true);
            if (Target == null)
                return null;

            //Core.Logger.Warn(LogCategory.Routine, $"[Current Target] - Name: {Target.Name} | Elite: {Target.IsElite || Target.IsBoss || Target.IsChampion}.");

            if (Target.Distance > 10f)
            {
                if (Skills.Monk.Epiphany.TimeSinceUse < 15000 && Target.Distance < 20)
                    return Spells.CripplingWave(Target);
                return Walk(Target);
            }

            TrinityPower power;

            if (TryMantra(out power))
                return power;

            if (ShouldCycloneStrike())
                return Spells.CycloneStrike();

            if (ShouldInnerSanctuary())
                return Spells.InnerSanctuary();

            if (ShouldBlindingFlash())
                return Spells.BlindingFlash();

            if (ShouldEpiphany())
                return Spells.Epiphany();

            return TryCripplingWave(out power) ? power : Walk(Target.Position);
        }

        public TrinityPower BuffPower()
        {
            if (Player.IsInTown || TrinityTownRun.IsTryingToTownPortal())
                return null;
            //if (Skills.Monk.CycloneStrike.TimeSinceUse > 3000 && Player.PrimaryResource > 45 && Targeting.NumMobsInRange(22f) > 3)
            //    return Spells.CycloneStrike();
            return null;
        }

        public TrinityPower DestructiblePower() => DefaultPower;

        public TrinityPower MovementPower(Vector3 destination)
        {
            return Walk(destination);
        }
    }
}
