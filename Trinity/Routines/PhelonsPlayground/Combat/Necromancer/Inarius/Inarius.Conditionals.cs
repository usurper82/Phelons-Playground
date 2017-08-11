using System;
using Trinity.DbProvider;
using Trinity.Framework;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Framework.Helpers;
using Trinity.Framework.Objects;
using Trinity.Framework.Reference;
using Trinity.Routines.PhelonsPlayground.Utils;
using Zeta.Common;
using Zeta.Game.Internals.Actors;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer.Inarius
{
    using System.Linq;

    public partial class Inarius
    {
        public virtual bool ShouldBoneSpirit()
        {
            if (!Skills.Necromancer.BoneSpirit.CanCast())
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $"[BoneSpirit] - On {Target}.");
            return true;
        }

        protected int LastSkeletonCommandTargetAcdId { get; set; }

        protected virtual bool ShouldCommandSkeletons()
        {
            if (!Skills.Necromancer.CommandSkeletons.CanCast() || Skills.Necromancer.Simulacrum.TimeSinceUse < 12500)
                return false;

            if (Target.AcdId == LastSkeletonCommandTargetAcdId ||
                Skills.Necromancer.CommandSkeletons.TimeSinceUse < 2500)
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

            if (Player.PrimaryResourcePct < 0.95)
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

        public virtual bool ShouldBoneArmor()
        {
            if (!Skills.Necromancer.BoneArmor.CanCast())
                return false;

            var unitsNearMe = Targeting.NearbyTargets(Core.Player.Actor, 15f).Count();

            if (unitsNearMe < 1)
                return false;

            if (!Skills.Necromancer.BoneArmor.IsBuffActive && unitsNearMe > 0)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[BoneArmor] - Missing Buff.");
                return true;
            }

            if (unitsNearMe <= Skills.Necromancer.BoneArmor.BuffStacks)
            {
                return false;
            }
            else
            {
                Core.Logger.Error(LogCategory.Routine,
                $"[BoneArmor] - I have {Skills.Necromancer.BoneArmor.BuffStacks} stacks with {unitsNearMe} mobs near me.");
                return true;
            }
            return false;
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

        public virtual bool ShouldDevour()
        {
            if (!Skills.Necromancer.Devour.CanCast())
                return false;
            var corpseCount = Targeting.CorpseCount(60f);
            if (Player.PrimaryResourceMax - Player.PrimaryResource > corpseCount * 10 &&
                Player.PrimaryResourcePct > 0.65 ||
                corpseCount < 3)
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $"[Devour] - ({corpseCount}) Corpses with ({Player.PrimaryResourceMax - Player.PrimaryResource}) Essence Deficit.");
            return true;
        }

        public virtual bool ShouldLandOfTheDead()
        {
            if (!Skills.Necromancer.LandOfTheDead.CanCast() || Player.PrimaryResourcePct < 0.90 ||
                Skills.Necromancer.Simulacrum.IsActive && !Skills.Necromancer.Simulacrum.CanCast())
                return false;
            var elite = Targeting.BestLOSEliteInRange(65f);
            if (elite == null || Skills.Necromancer.LandOfTheDead.TimeSinceUse < 10000)
                return false;

            //Trying to alternate cooldowns
            if (Skills.Necromancer.Frailty.CanCast() && elite.IsChampion ||
                Skills.Necromancer.Decrepify.CanCast() && elite.IsElite)
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
            if (elite == null || Skills.Necromancer.Simulacrum.TimeSinceUse < 10000 || Player.PrimaryResourcePct < 0.90)
                return false;

            //Trying to alternate cooldowns
            if (Skills.Necromancer.Frailty.CanCast() && elite.IsChampion ||
                Skills.Necromancer.Decrepify.CanCast() && elite.IsElite)
                return false;

            Core.Logger.Error(LogCategory.Routine,
                $"[Simulaccrum] - Because of Elite {elite}.");
            return true;
        }

        protected virtual bool ShouldLeech(out TrinityActor target)
        {
            target = null;
            if (!Skills.Necromancer.Leech.CanCast())
                return false;

            if (Skills.Necromancer.Leech.TimeSinceUse < 4000)
                return false;

            target = Targeting.BestTargetWithoutDebuff(65f, SNOPower.P6_Necro_Leech, Player.Position);
            if (target == null)
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $"[Leech] - On {target}.");
            return true;
        }

        protected virtual bool ShouldFrailty(out TrinityActor target)
        {
            target = null;
            if (!Skills.Necromancer.Frailty.CanCast() || Runes.Necromancer.AuraOfFrailty.IsActive)
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

        public virtual bool ShouldGrimScythe()
        {
            if (!Skills.Necromancer.GrimScythe.CanCast() && Target.Distance < 12)
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $"[Grim Scythe] - On {Target}.");
            return true;
        }

        public bool ShouldBloodRush(float distance, out Vector3 position)
        {
            position = Vector3.Zero;

            if (!Skills.Necromancer.BloodRush.CanCast())
                return false;

            var closestGlobe = Targeting.ClosestGlobe(distance);
            if (Player.CurrentHealthPct < 0.50 && Player.Position.EasyNavToPosition(closestGlobe.Position))
            {
                position = closestGlobe.Position;
                Core.Logger.Error(LogCategory.Routine,
                    $"[Blood Rush] - To get Health Globe.");
                return true;
            }

            if (Target == null)
                return false;

            if (!Target.IsInLineOfSight)
            {
                position = Target.Position;
                Core.Logger.Error(LogCategory.Routine,
                    $"[Blood Rush] - Monster is not in LoS.");
                return true;
            }

            if (Targeting.BestBuffPosition(distance, Player.Position, true, out position) &&
                Target.Position.Distance(position) > 5 && Target.Position.Distance(position) < 12.5f)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[Blood Rush] - To Best Buff Position.");
                return true;
            }

            if (Target.Position.Distance(position) > 12.5f)
            {
                position = Target.Position;
                Core.Logger.Error(LogCategory.Routine,
                    $"[Blood Rush] - Monster is too far away.");
                return true;
            }
            return false;
        }
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

            if (Target == null)
                return false;

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
            return CurrentTarget != null && (!Target.IsInLineOfSight || Target.RadiusDistance > 65f);

            if (_loiterPosition != Vector3.Zero && !_loiterPosition.InCriticalAvoidance() &&
                _loiterPosition.Distance(Player.Position) < 25f)
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
    }
}