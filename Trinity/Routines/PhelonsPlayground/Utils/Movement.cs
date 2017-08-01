using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Routines.PhelonsPlayground.Utils
{
    using DbProvider;
    using Framework.Objects;

    public class Movement : RoutineBase
    {

        private static float MinMoveDistance = PlayerMover.IsBlocked || IsCurrentlyAvoiding || HasInfiniteCasting
            ? 0
            : CurrentTarget != null &&
              (CurrentTarget.Type == TrinityObjectType.Item || CurrentTarget.IsNpc ||
               CurrentTarget.Type == TrinityObjectType.Shrine)
                ? 10
                : 25;
    }
}