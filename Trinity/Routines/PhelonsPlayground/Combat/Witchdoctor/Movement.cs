namespace Trinity.Routines.PhelonsPlayground.Combat.Witchdoctor
{
    using Framework;
    using Framework.Helpers;
    using Framework.Objects;
    using Framework.Reference;
    using Utils;
    using Zeta.Common;
    using static Basics;

    public class Movement
    {

        public static bool ShouldWalkToBuff(out Vector3 buffposition, Vector3 fromPosition, float distance = 15f)
        {
            buffposition = Vector3.Zero;

            if (Target == null)
                return false;

            var buffPosition = Targeting.BestBuffPosition(distance, fromPosition, Player.CurrentHealthPct > 0.65, out buffposition);

            if (buffPosition && Player.Position.Distance2D(buffposition) > 7 && buffposition.EasyNavToPosition(Player.Position))
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[WalkToBuff] - To Best Buff Position: {buffposition} Distance: {Player.Position.Distance2D(buffposition)}");
                return true;
            }
            return false;
        }

        public static bool ShouldWalk(out Vector3 position, float distance)
        {
            if (CurrentTarget != null && !CurrentTarget.IsUnit)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[Walk] - Grabbing {CurrentTarget.Name}");
                position = CurrentTarget.Position;
                return true;
            }
            position = Vector3.Zero;

            var closestGlobe = Targeting.ClosestGlobe(15f);
            if (closestGlobe != null && Player.CurrentHealthPct < 0.50 && Player.Position.EasyNavToPosition(closestGlobe.Position))
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
            if (Target != null && Target.RadiusDistance > distance)
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
