using System;
using Trinity.DbProvider;
using Trinity.Framework;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Framework.Objects;
using Trinity.Framework.Reference;
using Trinity.Routines.PhelonsPlayground.Utils;
using Zeta.Common;
using Zeta.Game.Internals.Actors;

namespace Trinity.Routines.PhelonsPlayground.Combat.Barbarian.zDPS
{
    using System.Linq;
    using System.Security.Cryptography;
    using Components.Combat.Resources;
    using Framework.Helpers;

    public partial class zDPSBarb
    {

        // Misc

        protected static int RaekorDamageStacks
            => Core.Buffs.GetBuffStacks(SNOPower.P2_ItemPassive_Unique_Ring_026);

        protected static bool IsBandOfMightBuffActive
            => Player.HasBuff((SNOPower)447060, 1); //'P4_ItemPassive_Unique_Ring_049' (447060) Stacks=1 VariantId=1)

        protected static bool IsGirdleOfGiantsBuffActive
            => Player.HasBuff((SNOPower)451237); //P42_ItemPassive_Unique_Ring_002' (451237) 

        public bool CanChargeTo(Vector3 destination)
        {
            var destinationDistance = Player.Position.Distance(destination);

            if (PlayerMover.IsBlocked)
                return true;

            if (!destination.EasyNavToPosition(Player.Position))
                return true;

            if (destinationDistance < 10f)
                return false;

            if (!Skills.Barbarian.FuriousCharge.CanCast())
                return false;

            if (CurrentTarget?.Type == TrinityObjectType.Item && destinationDistance < 12f)
                return false;

            if (Math.Abs(destination.Z - Core.Player.Position.Z) > 5)
                return false;

            return true;
        }

        private static int lastChargeTargetAcdId = 0;

        protected virtual bool ShouldFuriousChargeInCombat(out Vector3 position)
        {
            position = Vector3.Zero;

            if (!Skills.Barbarian.FuriousCharge.CanCast() ||
                Skills.Barbarian.FuriousCharge.TimeSinceUse < 500 && Skills.Barbarian.AncientSpear.TimeSinceUse > 1500 &&
                Player.PrimaryResourcePct > 0.65 && Skills.Barbarian.AncientSpear.CanCast())
                return false;

            if (Targeting.HealthGlobeExists(40f))
            {
                position = Targeting.GetBestHealthGlobeClusterPoint(7f, 40f, false);
                Core.Logger.Error(LogCategory.Routine,
                    $"[FuriousCharge] -  On Closest Health Globe: [{position.Distance(Player.Position)}].");
                return true;
            }
            position = Target.Position;
            var lastCast = SpellHistory.GetLastUseHistoryItem(SNOPower.Barbarian_FuriousCharge);
            if (Targeting.FarthesttUnits(40f).Any() && lastCast != null && lastCast.TargetAcdId != Target.AcdId)
            {
                var rangedTarget =
                    Targeting.FarthesttUnits(40f)
                        .OrderByDescending(x => x.Distance)
                        .FirstOrDefault(x => x.AcdId != lastChargeTargetAcdId);
                if (rangedTarget != null && rangedTarget.Position.Distance(Target.Position) < 45f)
                {
                    position = rangedTarget.Position;
                    Core.Logger.Error(LogCategory.Routine,
                        $" [FuriousCharge] - On Farthest Target Distance: [{position.Distance(Player.Position)}].");
                }
            }
            else
                Core.Logger.Error(LogCategory.Routine,
                    $" [FuriousCharge] - On Best Target Distance: [{position.Distance(Player.Position)}].");
            return true;
        }

        protected virtual bool ShouldAncientSpear(out TrinityActor target)
        {
            target = Target;

            if (!Skills.Barbarian.AncientSpear.CanCast() || Player.PrimaryResourcePct < 0.85 || Target.Distance > 10f)
                return false;

            target = Targeting.FarthesttUnit(65f);

            if (target == null)
                return false;

            Core.Logger.Error(LogCategory.Routine,
                $" [Ancient Spear] -  On Farthest Target Distance: [{target.Distance}].");

            return true;
        }

        // Defensive

        protected virtual bool ShouldGroundStomp()
        {
            if (!Skills.Barbarian.GroundStomp.CanCast() || Target.Distance > 12f)
                return false;

            Core.Logger.Error(LogCategory.Routine,
                $" [Ground Stomp] -  off CD.");
            return true;
        }

        protected virtual bool ShouldIgnorePain()
        {
            if (!Skills.Barbarian.IgnorePain.CanCast())
                return false;

            if (Legendary.PrideOfCassius.IsEquipped && Target != null)
                return true;

            //if (Player.CurrentHealthPct < 0.90)
            //    return true;

            if (Player.IsIncapacitated)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $" [Ignore Pain] -  Incapacitated.");
                return true;
            }

            if (Runes.Barbarian.MobRule.IsActive && Targeting.AnyPlayer(p => p.HitPointsPct < 0.50 && p.Distance < 50f))
            {
                Core.Logger.Error(LogCategory.Routine,
                    $" [Ignore Pain] -  Mob Rule.");
                return true;
            }

            return false;
        }

        protected virtual bool ShouldSprint()
        {
            if (!Skills.Barbarian.Sprint.CanCast())
                return false;

            if (Player.PrimaryResource < PrimaryEnergyReserve)
                return false;

            if (!Core.Player.IsMoving && !Runes.Barbarian.Rush.IsActive)
                return false;

            if (Skills.Barbarian.Sprint.IsBuffActive)
                return false;

            Core.Logger.Error(LogCategory.Routine,
                $" [Sprint]");
            return true;
        }

        protected virtual bool ShouldThreateningShout()
        {
            if (!Skills.Barbarian.ThreateningShout.CanCast())
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $" [Threatening Shout] -  off CD.");
            return true;
        }

        protected virtual bool ShouldWarCry()
        {
            if (!Skills.Barbarian.WarCry.CanCast())
                return false;

            if (Legendary.ChilaniksChain.IsEquipped)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $" [Warcry] - Chalinks Girdle.");
                return true;
            }

            if (Legendary.BladeOfTheTribes.IsEquipped)
                return true;

            if (Player.PrimaryResourcePct < 0.8f)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $" [Warcry] -  Resources.");
                return true;
            }

            if (Skills.Barbarian.WarCry.IsBuffActive)
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $" [Warcry] -  General.");

            return true;
        }
    }
}
