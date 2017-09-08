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

            var percentTargetsWithdebuff = (Targeting.DebuffedPercent(SNOPower.Witchdoctor_Haunt) + Targeting.DebuffedPercent(SNOPower.Witchdoctor_Haunt))/2;

            if (Player.Summons.ZombieDogCount < 1 || percentTargetsWithdebuff < 0.90 && Player.Summons.ZombieDogCount < 3)
                return false;

            Core.Logger.Error(LogCategory.Routine,
                $"[Sacrifice] - Percent Debuffed: {percentTargetsWithdebuff} Total Dogs: {Player.Summons.ZombieDogCount}.");

            return true;
        }
        private static int _barrageCount;
        public static bool ShouldSpiritBarrage()
        {
            if (!Skills.WitchDoctor.SpiritBarrage.CanCast())
                return false;

            if (_barrageCount == 4 && Skills.WitchDoctor.SpiritBarrage.TimeSinceUse < 3000)
                return false;

            if (_barrageCount == 4)
                _barrageCount = 0;

            if (SpellHistory.LastPowerUsed == SNOPower.Witchdoctor_SpiritBarrage)
                _barrageCount++;
            
            Core.Logger.Error(LogCategory.Routine,
                $"[Spirit Barrage] - Spirit Barrage Count: {_barrageCount}");
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
            if (needsdebuffs != null)
                target = needsdebuffs;
            Core.Logger.Error(LogCategory.Routine,
                $"[Grasp Of the Dead] - On {target}.");
            return true;
        }
    }
}
