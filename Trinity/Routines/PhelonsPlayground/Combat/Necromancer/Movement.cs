﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer
{
    using Framework;
    using Framework.Helpers;
    using Framework.Reference;
    using Utils;
    using Zeta.Common;
    using static Basics;

    public class Movement
    {

        public static bool ShouldBloodRush(float distance, out Vector3 position)
        {
            position = Vector3.Zero;

            if (!Skills.Necromancer.BloodRush.CanCast())
                return false;

            var closestGlobe = Targeting.ClosestGlobe(distance);
            if (Player.CurrentHealthPct < 0.50)
            {
                position = closestGlobe.Position;
                Core.Logger.Error(LogCategory.Routine,
                    $"[Blood Rush] - To get Health Globe.");
                return true;
            }

            if (Target == null || Skills.Necromancer.BloodRush.TimeSinceUse < 2500)
                return false;

            //if (!Target.IsInLineOfSight)
            //{
            //    position = Target.Position;
            //    Core.Logger.Error(LogCategory.Routine,
            //        $"[Blood Rush] - Monster is not in LoS.");
            //    return true;
            //}

            var buffPosition = Targeting.BestBuffPosition(distance, Target.Position, Player.CurrentHealthPct > 0.35, out position);

            if (buffPosition && Player.Position.Distance2D(position) > 7)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[Blood Rush] - To Best Buff Position: {position} Distance: {Player.Position.Distance2D(position)}");
                return true;
            }
            return false;
        }

        public static bool ShouldWalkToBuff(out Vector3 position)
        {
            position = Vector3.Zero;

            if (Target == null)
                return false;

            var buffPosition = Targeting.BestBuffPosition(15f, Player.Position, Player.CurrentHealthPct > 0.65, out position);

            if (buffPosition && Player.Position.Distance2D(position) > 7 && position.EasyNavToPosition(Player.Position))
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[WalkToBuff] - To Best Buff Position: {position} Distance: {Player.Position.Distance2D(position)}");
                return true;
            }
            return false;
        }



        public static bool ShouldWalk(out Vector3 position)
        {
            position = Vector3.Zero;

            var closestGlobe = Targeting.ClosestGlobe(15f);
            if (Player.CurrentHealthPct < 0.50 && Player.Position.EasyNavToPosition(closestGlobe.Position))
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[Walk] - Grabbing Health Globe.");
                position = closestGlobe.Position;
                return true;
            }

            if (Target == null)
                return false;

            if (!Target.IsInLineOfSight)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[Walk] - Target is not in Line Of Sight.");
                position = Target.Position;
                return true;
            }
            if (Target != null && Target.RadiusDistance > 50f)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[Walk] - To get into Range with Target.");
                position = Target.Position;
                return true;
            }
            return false;
        }
    }
}