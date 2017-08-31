using Trinity.Routines.PhelonsPlayground.Utils;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer.Rathma
{
    using System;
    using System.Linq;
    using Components.Combat.Resources;
    using Framework;
    using Framework.Actors.ActorTypes;
    using Framework.Helpers;
    using Framework.Reference;
    using Zeta.Common;
    using Zeta.Game.Internals.Actors;
    using static Basics;

    public partial class Rathma
    {
        protected virtual bool ShouldCommandSkeletons()
        {
            if (!Skills.Necromancer.CommandSkeletons.CanCast() || Skills.Necromancer.Simulacrum.TimeSinceUse < 12500)
                return false;

            if (Skills.Necromancer.CommandSkeletons.TimeSinceUse < 2500)
                return false;

            var lastCast = SpellHistory.GetLastUseHistoryItem(SNOPower.P6_Necro_CommandSkeletons);

            if (Target.AcdId == lastCast?.TargetAcdId)
                return false;

            Core.Logger.Error(LogCategory.Routine,
                $"[Command Skeletons] - On {Target}.");
            return true;
        }

        protected virtual bool ShouldSkeletalMage()
        {

            if (!Skills.Necromancer.SkeletalMage.CanCast())
                return false;

            if (Player.PrimaryResourcePct < 0.95)
                return false;

            Core.Logger.Error(LogCategory.Routine,
                $"[Skeletal Mage] - On {Target}.");
            return true;
        }

        public virtual bool ShouldCorpseExplosion()
        {
            if (!Skills.Necromancer.CorpseExplosion.CanCast())
                return false;
            var corpseCount = Targeting.CorpseCountNearLocation(Target.Position, 20f);
            if (corpseCount < 1)
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $"[CorpseExplosion] - ({corpseCount}) Corpses to Explode.");
            return true;
        }
    }
}
