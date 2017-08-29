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
    using System.Security.Cryptography;
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

        protected virtual bool ShouldFuriousChargeInCombat(out Vector3 position)
        {
            position = Target.Position;

            if (Targeting.HealthGlobeExists(45f))
            {
                position = Targeting.GetBestHealthGlobeClusterPoint(7f, 45f, false);
                Core.Logger.Error(LogCategory.Routine,
                    $"[FuriousCharge] -  On Closest Health Globe: [{position.Distance(Player.Position)}].");
                return true;
            }

            if (!Skills.Barbarian.FuriousCharge.CanCast() || Skills.Barbarian.FuriousCharge.TimeSinceUse < 500 && Player.PrimaryResourcePct > 0.85 && Skills.Barbarian.AncientSpear.CanCast())
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $" [FuriousCharge] - On Best Cluster Target Distance: [{position.Distance(Player.Position)}].");
            return true;
        }
        
        protected virtual bool ShouldAncientSpear(out TrinityActor target)
        {
            target = null;

            if (!Skills.Barbarian.AncientSpear.CanCast() || Skills.Barbarian.AncientSpear.TimeSinceUse < 1500 || Player.PrimaryResourcePct < 0.65)
                return false;

            target = Targeting.FarthesttUnit(55f);

            if (target == null)
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $" [Ancient Spear] -  On Farthest Target Distance: [{target.Distance}].");

            return true;
        }

        // Defensive

        protected virtual bool ShouldGroundStomp(out Vector3 position)
        {
            position = Vector3.Zero;

            if (!Skills.Barbarian.GroundStomp.CanCast())
                return false;

            position = Targeting.GetBestClusterPoint();
            return position != Vector3.Zero;
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
