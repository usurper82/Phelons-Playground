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

    public partial class zDPSMonk
    {
        public static TrinityActor Target = CurrentTarget;
        public TrinityPower OffensivePower()
        {
            Target = Targeting.GetBestClusterUnit();
            if (Target == null)
                return null;

            TrinityPower power;

            if (TryMantra(out power))
                return power;

            if (ShouldCycloneStrike())
                return Spells.CycloneStrike();

            if (ShouldEpiphany())
                return Spells.Epiphany();

            if (ShouldInnerSanctuary())
                return Spells.InnerSanctuary();

            if (ShouldBlindingFlash())
                return Spells.BlindingFlash();

            return TryCripplingWave(out power) ? power : Walk(Target);
        }

        public TrinityPower BuffPower()
        {
            if (Player.IsInTown || TrinityTownRun.IsTryingToTownPortal())
                return null;
            if (Skills.Monk.CycloneStrike.TimeSinceUse > 3000 && Player.PrimaryResource > PrimaryEnergyReserve && Targeting.NumMobsInRange(22f) > 3)
                return Spells.CycloneStrike();
            return null;
        }

        public TrinityPower DestructiblePower() => DefaultPower;

        public TrinityPower MovementPower(Vector3 destination)
        {
            return Walk(destination);
        }
    }
}
