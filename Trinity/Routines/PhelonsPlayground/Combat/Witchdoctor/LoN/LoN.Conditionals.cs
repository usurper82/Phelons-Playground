using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Routines.PhelonsPlayground.Combat.Witchdoctor.LoN
{
    using System.Diagnostics;
    using Components.Combat.Resources;
    using Framework;
    using Framework.Actors.ActorTypes;
    using Framework.Helpers;
    using Framework.Reference;
    using Utils;
    using Zeta.Common;
    using Zeta.Game.Internals.Actors;
    using static Basics;
    using Movement = Movement;

    public partial class LoN
    {

        public static bool ShouldSacrifice()
        {
            if (!Skills.WitchDoctor.Sacrifice.CanCast())
                return false;

            var percentTargetsWithHaunt = Targeting.DebuffedPercent(SNOPower.Witchdoctor_GraspOfTheDead);

            if (Player.Summons.ZombieDogCount < 1 || percentTargetsWithHaunt < 0.90)
                return false;

            return true;
        }
        private static Stopwatch _firstBarrage;
        public static bool ShouldSpiritBarrage()
        {
            if (!Skills.WitchDoctor.SpiritBarrage.CanCast())
                return false;

            if (_firstBarrage.ElapsedMilliseconds > 4000 && Skills.WitchDoctor.SpiritBarrage.TimeSinceUse < 4000)
            {
                _firstBarrage.Stop();
                return false;
            }

            if (!_firstBarrage.IsRunning)
            {
                _firstBarrage.Reset();
                _firstBarrage.Start();
            }
            
            Core.Logger.Error(LogCategory.Routine,
                $"[Grasp Of the Dead] - On Cooldown.");
            return true;
        }

        public static bool ShouldGraspOfTheDead(out TrinityActor target)
        {
            target = Target;

            if (!Skills.WitchDoctor.GraspOfTheDead.CanCast())
                return false;

            var percentTargetsWithHaunt = Targeting.DebuffedPercent(SNOPower.Witchdoctor_GraspOfTheDead);
            if (percentTargetsWithHaunt > 0.90)
                return false;

            if (Player.PrimaryResourcePct < 0.90 && Legendary.AquilaCuirass.IsEquipped)
                return false;

            var needsdebuffs = Targeting.BestTargetWithoutDebuff(30f, SNOPower.Witchdoctor_GraspOfTheDead,
                Player.Position);

            target = needsdebuffs;
            Core.Logger.Error(LogCategory.Routine,
                $"[Grasp Of the Dead] - On {target}.");
            return true;
        }
    }
}
