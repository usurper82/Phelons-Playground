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

        public virtual bool ShouldDevour()
        {
            if (!Skills.Necromancer.Devour.CanCast())
                return false;
            var corpseCount = Targeting.CorpseCount(60f);
            if (Player.PrimaryResourceMax - Player.PrimaryResource > corpseCount * 10 &&
                Player.PrimaryResourcePct > 0.65 ||
                corpseCount < 3 && CurrentTarget != null)
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

        public virtual bool ShouldBoneSpikes()
        {
            if (!Skills.Necromancer.BoneSpikes.CanCast())
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $"[Bone Spikes] - On {Target}.");
            return true;
        }

        public virtual bool ShouldSiphonBlood()
        {
            if (!Skills.Necromancer.SiphonBlood.CanCast())
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $"[Siphon Blood] - On {Target}.");
            return true;
        }

        public virtual bool ShouldGrimScythe(out TrinityActor target)
        {
            target = null;
            if (!Skills.Necromancer.GrimScythe.CanCast())
                return false;

            target = (Targeting.BestTargetWithoutDebuff(12f, SNOPower.P6_Necro_Decrepify, Player.Position) ?? 
                Targeting.BestTargetWithoutDebuff(12f, SNOPower.P6_Necro_Leech, Player.Position) ?? 
                Targeting.BestTargetWithoutDebuff(12f, SNOPower.P6_Necro_Frailty, Player.Position)) ??
                Target;

            Core.Logger.Error(LogCategory.Routine,
                $"[Grim Scythe] - On {target}.");
            return true;
        }

        public bool ShouldBloodRush(float distance, out Vector3 position)
        {
            position = Vector3.Zero;

            if (!Skills.Necromancer.BloodRush.CanCast())
                return false;

            var closestGlobe = Targeting.ClosestGlobe(distance);
            if (Player.CurrentHealthPct < 0.50)
            {
                position = closestGlobe.Position;
                Core.Logger.Error(LogCategory.Routine,
                    $"[Blood Rush] - To get Health Globe.");
                return true;
            }

            if (Target == null || Skills.Necromancer.BloodRush.TimeSinceUse < 2500)
                return false;

            //if (!Target.IsInLineOfSight)
            //{
            //    position = Target.Position;
            //    Core.Logger.Error(LogCategory.Routine,
            //        $"[Blood Rush] - Monster is not in LoS.");
            //    return true;
            //}

            var buffPosition = Targeting.BestBuffPosition(distance, Target.Position, Player.CurrentHealthPct > 0.35, out position);

            if (buffPosition && Player.Position.Distance2D(position) > 7)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[Blood Rush] - To Best Buff Position: {position} Distance: {Player.Position.Distance2D(position)}");
                return true;
            }
            return false;
        }

        public virtual bool ShouldWalkToBuff(out Vector3 position)
        {
            position = Vector3.Zero;

            if (Target == null)
                return false;

            var buffPosition = Targeting.BestBuffPosition(15f, Player.Position, Player.CurrentHealthPct > 0.65, out position);

            if (buffPosition && Player.Position.Distance2D(position) > 7 && position.EasyNavToPosition(Player.Position))
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[WalkToBuff] - To Best Buff Position: {position} Distance: {Player.Position.Distance2D(position)}");
                return true;
            }
            return false;
        }



        public virtual bool ShouldWalk(out Vector3 position)
        {
            position = Vector3.Zero;

            var closestGlobe = Targeting.ClosestGlobe(15f);
            if (Player.CurrentHealthPct < 0.50 && Player.Position.EasyNavToPosition(closestGlobe.Position))
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[Walk] - Grabbing Health Globe.");
                position = closestGlobe.Position;
                return true;
            }

            if (Target == null)
                return false;

            if (!Target.IsInLineOfSight)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[Walk] - Target is not in Line Of Sight.");
                position = Target.Position;
                return true;
            }
            if (Target != null && Target.RadiusDistance > 50f)
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[Walk] - To get into Range with Target.");
                position = Target.Position;
                return true;
            }
            return false;
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
