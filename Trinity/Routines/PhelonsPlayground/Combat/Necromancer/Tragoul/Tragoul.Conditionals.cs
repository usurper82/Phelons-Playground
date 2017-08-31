namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer.Tragoul
{
    using Framework;
    using Framework.Helpers;
    using Framework.Reference;
    using Utils;
    using static Basics;

    public partial class Tragoul
    {
        public virtual bool ShouldCorpseLance()
        {
            if (!Skills.Necromancer.CorpseLance.CanCast() ||
                Skills.Necromancer.CorpseLance.IsLastUsed && Targeting.NumMobsInRange(65f) > 1 && Skills.Necromancer.LandOfTheDead.TimeSinceUse > 10000)
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $"[Corpse Lance] - On {Target}.");
            return true;
        }
    }

}
