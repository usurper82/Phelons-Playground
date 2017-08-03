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
    using static Spells;
    public partial class Rathma
    {
        private Vector3 _loiterPosition = Vector3.Zero;
        public virtual bool ShouldWalk(out Vector3 position)
        {
            position = Vector3.Zero;

            var closestGlobe = Targeting.ClosestGlobe(15f);
            if (Player.CurrentHealthPct < 0.50 && Player.Position.EasyNavToPosition(closestGlobe.Position))
            {
                position = closestGlobe.Position;
                return true;
            }

            if (!Target.IsInLineOfSight)
            {
                position = Target.Position;
                return true;
            }

            if (Targeting.BestBuffPosition(10f, Player.Position, false, out position) &&
                Player.Position.Distance(position) > 5 && position.EasyNavToPosition(Player.Position))
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[Walk] - To Best Buff Position.");
                return true;
            }
            return false;

            if (_loiterPosition != Vector3.Zero && !_loiterPosition.InCriticalAvoidance() && _loiterPosition.Distance(Player.Position) < 25f)
            {
                position = _loiterPosition;
                return true;
            }
            else
            {
                position = Targeting.GetLoiterPosition(Target, 25f);
                Core.Logger.Error(LogCategory.Routine,
                    $"[Walk] - To Best Loiter Position.");
                return true;
            }

            return false;
        }

        public virtual bool ShouldDevour()
        {
            return Player.PrimaryResourceMax - Player.PrimaryResource < Targeting.CorpseSafeList().Count * 10 && Targeting.CorpseSafeList().Count >= 5;
        }
        public virtual bool ShouldLandOfTheDead()
        {
            if (!Skills.Necromancer.LandOfTheDead.CanCast())
                return false;
            var elite = Targeting.BestLOSEliteInRange(65f);
            if (elite == null || Skills.Necromancer.LandOfTheDead.TimeSinceUse < 10000 || elite.HitPointsPct > 50 || Player.PrimaryResourcePct < 0.90)
                return false;

            //Trying to alternate cooldowns
            if (Skills.Necromancer.Frailty.CanCast() && elite.IsChampion)
                return false;

            Core.Logger.Error(LogCategory.Routine,
                $"[Land of the Dead] - Because of Elite {elite}.");
            return true;
        }
        public virtual bool ShouldSimulacrum()
        {
            if (!Skills.Necromancer.Simulacrum.CanCast())
                return false;

            var elite = Targeting.BestLOSEliteInRange(65f);
            if (elite == null || Skills.Necromancer.Simulacrum.TimeSinceUse < 15000 || elite.HitPointsPct > 50)
                return false;

            //Trying to alternate cooldowns
            if (Skills.Necromancer.Frailty.CanCast() && elite.IsChampion || Skills.Necromancer.Decrepify.CanCast() && elite.IsElite)
                return false;

            Core.Logger.Error(LogCategory.Routine,
                $"[Simulaccrum] - Because of Elite {elite}.");
            return true;
        }

        protected virtual bool ShouldFrailty(out TrinityActor target)
        {
            target = null;
            if (!Skills.Necromancer.Frailty.CanCast())
                return false;

            if (Skills.Necromancer.Frailty.TimeSinceUse < 4000)
                return false;

            var needsdebuffs = Targeting.BestTargetWithoutDebuff(65f, SNOPower.P6_Necro_Frailty, Player.Position);
            if (needsdebuffs == null)
                return false;
            target = needsdebuffs;
            Core.Logger.Error(LogCategory.Routine,
                $"[Frailty] - On {target}.");
            return true;
        }

        protected virtual bool ShouldDecrepify(out TrinityActor target)
        {
            target = null;
            if (!Skills.Necromancer.Decrepify.CanCast())
                return false;

            //var hasDebuffs = Targeting.UnitsWithDebuff(SNOPower.P6_Necro_Decrepify);
            //if (hasDebuffs > 0)
            //    return false;

            if (Skills.Necromancer.Decrepify.TimeSinceUse < 4000)
                return false;

            var needsdebuffs = Targeting.BestTargetWithoutDebuff(65f, SNOPower.P6_Necro_Decrepify, Player.Position);
            if (needsdebuffs == null)
                return false;
            target = needsdebuffs;
            Core.Logger.Error(LogCategory.Routine,
                $"[Decrepify] - On {target}.");
            return true;
        }

        protected int LastSkeletonCommandTargetAcdId { get; set; }
        protected virtual bool ShouldCommandSkeletons()
        {
            if (!Skills.Necromancer.CommandSkeletons.CanCast() || Skills.Necromancer.Simulacrum.TimeSinceUse < 12500)
                return false;

            if (Target.AcdId == LastSkeletonCommandTargetAcdId || Skills.Necromancer.CommandSkeletons.TimeSinceUse < 2500)
                return false;

            LastSkeletonCommandTargetAcdId = Target.AcdId;

            Core.Logger.Error(LogCategory.Routine,
                $"[Command Skeletons] - On {Target}.");
            return true;
        }

        protected virtual bool ShouldSkeletalMage()
        {

            if (!Skills.Necromancer.SkeletalMage.CanCast())
                return false;

            if (Player.PrimaryResourcePct < 0.75)
                return false;

            Core.Logger.Error(LogCategory.Routine,
                $"[Skeletal Mage] - On {Target}.");
            return true;
        }

        public virtual bool ShouldBoneSpikes()
        {
            if (!Skills.Necromancer.BoneSpikes.CanCast())
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $"[Bone Spikes] - On {Target}.");
            return true;
        }
    }
}
