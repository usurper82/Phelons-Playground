using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Framework;
using Zeta.Common;

namespace Trinity.Routines.PhelonsPlayground.Utils
{
    using Framework.Avoidance.Structures;

    public static class PhelonExtensions
    {
        public static bool InAvoidance(this Vector3 position)
        {
            return Core.Avoidance.InAvoidance(position);
        }
        public static bool InCriticalAvoidance(this Vector3 position)
        {
            return Core.Avoidance.InCriticalAvoidance(position);
        }

        public static bool EasyNavToPosition(this Vector3 position, Vector3 otherPosition)
        {
            return Targeting.UnitsBetweenLocations(position, otherPosition).Count < 3;
        }

        public static int UnitsAroundPosition(this Vector3 position)
        {
            return Targeting.NumMobsInRangeOfPosition(position, 10f);
        }

        public static Vector3 GetPositionBehind(this Vector3 position, Vector3 otherPosition)
        {
            return MathEx.CalculatePointFrom(otherPosition, position, otherPosition.Distance(position) + 4f);
        }
    }
}
