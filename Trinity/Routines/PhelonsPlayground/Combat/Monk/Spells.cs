using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Components.Combat.Resources;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Routines.PhelonsPlayground.Combat.Monk.zDPS;
using Zeta.Common;
using Zeta.Game.Internals.Actors;

namespace Trinity.Routines.PhelonsPlayground.Combat.Monk
{
    public class Spells
    {

        public static TrinityPower FistsOfThunder(TrinityActor target)
            => new TrinityPower(SNOPower.Monk_FistsofThunder, zDPSMonk.MeleeAttackRange, target.AcdId);

        public static TrinityPower DeadlyReach(TrinityActor target)
            => new TrinityPower(SNOPower.Monk_DeadlyReach, zDPSMonk.MeleeAttackRange, target.AcdId);

        public static TrinityPower CripplingWave(TrinityActor target)
            => new TrinityPower(SNOPower.Monk_CripplingWave, zDPSMonk.MeleeAttackRange, target.AcdId);

        public static TrinityPower WayOfTheHundredFists(TrinityActor target)
            => new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, zDPSMonk.MeleeAttackRange, target.AcdId);

        public static TrinityPower SevenSidedStrike(TrinityActor target)
            => new TrinityPower(SNOPower.Monk_SevenSidedStrike, 16f, target.AcdId) { CastWhenBlocked = true, };

        public static TrinityPower LashingTailKick(TrinityActor target)
            => new TrinityPower(SNOPower.Monk_LashingTailKick, zDPSMonk.MeleeAttackRange, target.AcdId);

        public static TrinityPower WaveOfLight(TrinityActor target)
            => new TrinityPower(SNOPower.Monk_WaveOfLight, 60f, target.Position);

        public static TrinityPower ExplodingPalm(TrinityActor target)
            => new TrinityPower(SNOPower.Monk_ExplodingPalm, zDPSMonk.MeleeAttackRange, target.AcdId);

        public static TrinityPower TempestRush(Vector3 position)
            => new TrinityPower(SNOPower.Monk_TempestRush, 60f, position);

        public static TrinityPower DashingStrike(Vector3 position)
            => new TrinityPower(SNOPower.X1_Monk_DashingStrike, 50f, position);

        public static TrinityPower BlindingFlash()
            => new TrinityPower(SNOPower.Monk_BlindingFlash);

        public static TrinityPower BreathOfHeaven()
            => new TrinityPower(SNOPower.Monk_BreathOfHeaven);

        public static TrinityPower Serenity()
            => new TrinityPower(SNOPower.Monk_Serenity);

        public static TrinityPower CycloneStrike()
            => new TrinityPower(SNOPower.Monk_CycloneStrike);

        public static TrinityPower MantraOfSalvation()
            => new TrinityPower(SNOPower.X1_Monk_MantraOfEvasion_v2);

        public static TrinityPower MantraOfRetribution()
            => new TrinityPower(SNOPower.X1_Monk_MantraOfRetribution_v2);

        public static TrinityPower MantraOfHealing()
            => new TrinityPower(SNOPower.X1_Monk_MantraOfHealing_v2);

        public static TrinityPower MantraOfConviction()
            => new TrinityPower(SNOPower.X1_Monk_MantraOfConviction_v2);

        public static TrinityPower SweepingWind()
            => new TrinityPower(SNOPower.Monk_SweepingWind);

        public static TrinityPower InnerSanctuary()
            => new TrinityPower(SNOPower.X1_Monk_InnerSanctuary);

        public static TrinityPower Epiphany()
            => new TrinityPower(SNOPower.X1_Monk_Epiphany);

        public static TrinityPower MysticAlly()
            => new TrinityPower(SNOPower.X1_Monk_MysticAlly_v2);
    }
}
