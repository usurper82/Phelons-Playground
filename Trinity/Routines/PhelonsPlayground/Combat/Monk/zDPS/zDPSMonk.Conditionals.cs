using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using Trinity.Components.Combat.Resources;
using Trinity.DbProvider;
using Trinity.Framework;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Framework.Helpers;
using Trinity.Framework.Objects;
using Trinity.Framework.Reference;
using Trinity.UI;
using Zeta.Common;
using Zeta.Game.Internals.Actors;

namespace Trinity.Routines.PhelonsPlayground.Combat.Monk.zDPS
{
    using System.Linq;
    using Utils;

    public partial class zDPSMonk
    {
        #region Expressions 

        public static bool ShouldRefreshSpiritGuardsBuff
            => Legendary.SpiritGuards.IsEquipped && (SpellHistory.TimeSinceGeneratorCast >= 2500 || !HasSpiritGuardsBuff);

        public bool HasCycled(Skill s)
            => s.TimeSinceUse > 250 && !s.IsLastUsed;

        public static float WaveOfLightRange
            => Legendary.TzoKrinsGaze.IsEquipped ? 55f : 16f;

        public static int MaxDashingStrikeCharges
            => Runes.Monk.Quicksilver.IsActive ? 3 : 2;

        public static float MeleeAttackRange
            => IsEpiphanyActive ? 50f : 8f;

        public static float CycloneStrikeRange
            => Runes.Monk.Implosion.IsActive ? 34f : 24f;

        public static float CycloneStrikeSpirit
            => Runes.Monk.EyeOfTheStorm.IsActive ? 30 : 50;

        public static int MaxSweepingWindStacks
            => Legendary.VengefulWind.IsEquipped ? 10 : 3;

        public static bool HasShenLongBuff
            => Core.Buffs.HasBuff(SNOPower.P3_ItemPassive_Unique_Ring_026, 1);

        public static bool HasRaimentDashBuff
            => Core.Buffs.HasBuff(SNOPower.P2_ItemPassive_Unique_Ring_033, 2);

        public static bool HasSpiritGuardsBuff
            => Core.Buffs.HasBuff(SNOPower.P2_ItemPassive_Unique_Ring_034, 1);

        public static bool IsEpiphanyActive
            => Core.Buffs.HasBuff(SNOPower.X1_Monk_Epiphany);


        #endregion

        //private static bool ShouldWalk(float minRange, float maxRange, bool objectsInAoe)
        //{
        //    return TargetUtil.BestTankPosition(maxRange, objectsInAoe).Distance(Player.Position) < maxRange &&
        //           TargetUtil.BestTankPosition(maxRange, objectsInAoe).Distance(Player.Position) > minRange;
        //}

        public bool CanDashTo(Vector3 destination)
        {
            var destinationDistance = Player.Position.Distance(destination);
            if (destinationDistance < 5f)
                return false;

            if (!Skills.Monk.DashingStrike.CanCast())
                return false;

            if (Skills.Monk.DashingStrike.TimeSinceUse < 250)
                return false;

            if (CurrentTarget?.Type == TrinityObjectType.Item && destinationDistance < 20f)
                return false;

            if (Math.Abs(destination.Z - Core.Player.Position.Z) > 5)
                return false;

            if (Sets.ThousandStorms.IsFullyEquipped && Skills.Monk.DashingStrike.Charges < 2 && !PlayerMover.IsBlocked)
                return false;

            return true;
        }

        public bool TryMovementPower(Vector3 destination, out TrinityPower power)
        {
            power = null;

            var destinationDistance = Player.Position.Distance(destination);
            if (destinationDistance < 10f)
                return false;

            if (CurrentTarget?.Type == TrinityObjectType.Item && destinationDistance < 20f)
                return false;

            if (Math.Abs(destination.Z - Core.Player.Position.Z) > 5)
                return false;

            if (Skills.Monk.DashingStrike.CanCast())
            {
                if (Sets.ThousandStorms.IsFullyEquipped && Skills.Monk.DashingStrike.Charges < 2 && !PlayerMover.IsBlocked)
                    return false;

                power = Spells.DashingStrike(destination);
                return true;
            }

            return false;
        }

        protected virtual bool TryMantra(out TrinityPower power)
        {
            power = null;

            if (ShouldMantraOfConviction())
                power = Spells.MantraOfConviction();

            if (ShouldMantraOfHealing())
                power = Spells.MantraOfHealing();

            if (ShouldMantraOfRetribution())
                power = Spells.MantraOfRetribution();

            if (ShouldMantraOfSalvation())
                power = Spells.MantraOfSalvation();

            return power != null;
        }

        protected virtual bool TryCripplingWave(out TrinityPower power)
        {
            TrinityActor target;
            power = null;

            if (ShouldCripplingWave(out target))
                power = Spells.CripplingWave(target);

            return power != null;
        }

        protected virtual bool ShouldCripplingWave(out TrinityActor target)
        {
            target = Target;

            if (!Skills.Monk.CripplingWave.CanCast())
                return false;

            var range = Skills.Monk.Epiphany.TimeSinceUse < 15000 ? 20 : 7f;
            if (target.Distance > 7f && range > 7f)
                return true;

            target = TargetUtil.BestAuraUnit(SNOPower.Monk_CripplingWave, 10f, true) ?? target;
            if (target != null && target.Distance < range)
                return true;

            target = TargetUtil.GetClosestUnit(range);
            return target != null;
        }

        protected virtual bool ShouldInnerSanctuary()
        {
            if (!Skills.Monk.InnerSanctuary.CanCast())
                return false;

            if (Skills.Monk.InnerSanctuary.TimeSinceUse < Settings.InnerSanctuaryDelay)
                return false;

            if (TargetUtil.BestAoeUnit(45, true).Distance > Settings.InnerSanctuaryMinRange)
                return false;

            Core.Logger.Error(LogCategory.Routine,
                $"[InnerSanctuary] - On CD.");
            return true;
        }

        protected virtual bool ShouldCycloneStrike()
        {
            if (!Skills.Monk.CycloneStrike.CanCast())
                return false;

            if (Player.PrimaryResource < 90 || Skills.Monk.CycloneStrike.TimeSinceUse < 200*Targeting.NearbyTargets(20f).Count())
                return false;

            var targetIsCloseElite = CurrentTarget.IsElite && CurrentTarget.Distance < CycloneStrikeRange;      //Checks for elites first
            var plentyOfTargetsToPull = TargetUtil.IsPercentUnitsWithinBand(15f, CycloneStrikeRange, 0.25);     //Checks percentage within band
            var anyTargetsInRange = TargetUtil.AnyMobsInRange(CycloneStrikeRange, Settings.CycloneStrikeMinMobs);   //Checks minimum mobs in range
            if (targetIsCloseElite)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[CycloneStrike] - Target Is Close Elite.");
                return true;
            }
            if (plentyOfTargetsToPull)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[CycloneStrike] - Plenty Of Targets To Pull");
                return true;
            }
            if (anyTargetsInRange)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[CycloneStrike] - Any Targets In Range.");
                return true;
            }
            return false;
        }

        protected virtual bool ShouldBlindingFlash()
        {
            if (!Skills.Monk.BlindingFlash.CanCast())
                return false;

            if (HasInstantCooldowns && Skills.Monk.BlindingFlash.TimeSinceUse > 2500)   //Overlap them by half a second
                return true;

            var lowHealth = Player.CurrentHealthPct <= Settings.EmergencyHealthPct;
            var enoughStuffToBlind = TargetUtil.AnyElitesInRange(20, 1) || TargetUtil.AnyMobsInRange(20, 3);
            var blindCurrentTarget = CurrentTarget != null && CurrentTarget.IsElite && CurrentTarget.RadiusDistance <= 15f;

            if (lowHealth)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[BlindingFlash] - Low Health");
                return true;
            }
            if (enoughStuffToBlind)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[BlindingFlash] - Enough Stuff To Blind");
                return true;
            }
            if (blindCurrentTarget)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[BlindingFlash] - Blind Current Target.");
                return true;
            }
            return false;
        }

        protected virtual bool ShouldMantraOfHealing()
        {
            if (!Skills.Monk.MantraOfHealing.CanCast())
                return false;

            //if (Skills.Monk.MantraOfHealing.TimeSinceUse < Settings.MantraDelay)
            //    return false;

            if (Player.PrimaryResourcePct <= 0.90)
                return false;

            Core.Logger.Error(LogCategory.Routine,
                $"[MantraOfHealing] - On CD.");

            return true;
        }

        protected virtual bool ShouldMantraOfSalvation()
        {
            if (!Skills.Monk.MantraOfSalvation.CanCast())
                return false;

            //if (Skills.Monk.MantraOfSalvation.TimeSinceUse < Settings.MantraDelay)
            //    return false;

            if (Player.PrimaryResourcePct <= 0.90)
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $"[MantraOfSalvation] - On CD.");

            return true;
        }

        protected virtual bool ShouldMantraOfRetribution()
        {
            if (!Skills.Monk.MantraOfRetribution.CanCast())
                return false;

            //if (Skills.Monk.MantraOfRetribution.TimeSinceUse < Settings.MantraDelay)
            //    return false;

            if (Player.PrimaryResourcePct <= 0.90)
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $"[MantraOfRetribution] - On CD.");

            return true;
        }

        protected virtual bool ShouldMantraOfConviction()
        {
            if (!Skills.Monk.MantraOfConviction.CanCast())
                return false;

            //if (Skills.Monk.MantraOfConviction.TimeSinceUse < Settings.MantraDelay)
            //    return false;

            if (Player.PrimaryResourcePct < 0.90)
                return false;

            if (TargetUtil.ClusterExists(15f, 30f, 3))
                return true;
            Core.Logger.Error(LogCategory.Routine,
                $"[MantraOfConviction] - On CD.");

            return true;
        }

        protected virtual bool ShouldEpiphany()
        {
            if (!AllowedToUse(Settings.Epiphany, Skills.Monk.Epiphany))
                return false;

            if (!Skills.Monk.Epiphany.CanCast())
                return false;

            if (HasInstantCooldowns && !Skills.Monk.Epiphany.IsLastUsed)
                return true;

            if (TargetUtil.BestAoeUnit(45, true).Distance > 20)
                return false;

            Core.Logger.Error(LogCategory.Routine,
                $"[Epiphany] - On CD.");
            return true;
        }
    }
}

