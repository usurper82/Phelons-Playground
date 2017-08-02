using System.Collections.Generic;
using Trinity.Components.Combat.Resources;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Framework.Objects;
using Trinity.Routines.PhelonsPlayground.Utils;
using Zeta.Common;

namespace Trinity.Routines.PhelonsPlayground.Combat.Barbarian.zDPS
{
    using Components.Coroutines.Town;
    using Framework;
    using Framework.Helpers;
    using Zeta.Game;

    public partial class zDPSBarb
    {

        public static TrinityActor Target = CurrentTarget;
        public TrinityPower OffensivePower()
        {
            Target = Targeting.BestAoeUnit();
            if (Target == null)
                return null;

            Vector3 position;

            if (ShouldIgnorePain())
                return Spells.IgnorePain();

            if (ShouldAncientSpear(out Target))
                return Spells.AncientSpear(Target);

            if (ShouldWarCry())
                return Spells.WarCry();

            if (ShouldThreateningShout())
                return Spells.ThreateningShout();

            if (ShouldFuriousChargeInCombat(out position))
                return Spells.FuriousCharge(position);

            return Walk(Targeting.HealthGlobeExists(25f) ? Targeting.GetBestHealthGlobeClusterPoint(10f, 25f) : 
                TargetUtil.GetLoiterPosition(TargetUtil.GetBestClusterUnit(), 10f));
        }

        public TrinityPower BuffPower()
        { 
            if (Player.IsInTown || TrinityTownRun.IsTryingToTownPortal())
                return null;

            if (ShouldIgnorePain())
                return Spells.IgnorePain();

            if (ShouldWarCry())
                return Spells.WarCry();

            if (ShouldThreateningShout())
                return Spells.ThreateningShout();

            if (ShouldSprint())
                return Spells.Sprint();

            return null;
        }
       

        private readonly List<TrinityObjectType> _importantActors = new List<TrinityObjectType>
        {
            TrinityObjectType.ProgressionGlobe,
            TrinityObjectType.HealthGlobe,
            TrinityObjectType.ProgressionGlobe,
        };
    }
}


