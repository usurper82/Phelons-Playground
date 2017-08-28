using Trinity.Routines.PhelonsPlayground.Utils;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer.Tragoul
{
    using System;
    using System.Linq;
    using Components.Combat.Resources;
    using DbProvider;
    using Framework;
    using Framework.Actors.ActorTypes;
    using Framework.Helpers;
    using Framework.Objects;
    using Framework.Reference;
    using Zeta.Common;
    using Zeta.Game.Internals.Actors;
    using static Spells;

    public partial class Tragoul
    {
        private Vector3 _loiterPosition = Vector3.Zero;


        public bool ShouldBloodRush(float distance, out Vector3 position)
        {
            position = Vector3.Zero;
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
        public virtual bool ShouldWalk(float distance, out Vector3 position)
        {
            position = Vector3.Zero;

            var closestGlobe = Targeting.ClosestGlobe(distance);
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

            if (Targeting.BestBuffPosition(distance, Player.Position, true, out position) &&
                Player.Position.Distance(position) > 5 && position.EasyNavToPosition(Player.Position))
            {
                Core.Logger.Error(LogCategory.Routine,
                    $"[Walk] - To Best Buff Position.");
                return true;
            }
            return CurrentTarget != null && (!Target.IsInLineOfSight || Target.RadiusDistance > 55f);

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
            if (!Skills.Necromancer.LandOfTheDead.CanCast() || Skills.Necromancer.Simulacrum.IsActive && !Skills.Necromancer.Simulacrum.CanCast())
                return false;
            var elite = Targeting.BestLOSEliteInRange(65f);
            if (elite == null || Skills.Necromancer.LandOfTheDead.TimeSinceUse < 10000)
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
        public virtual bool ShouldCorpseLance()
        {
            if (!Skills.Necromancer.CorpseLance.CanCast())
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $"[Corpse Lance] - On {Target}.");
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

        public bool CanPortTo(Vector3 destination)
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
    }
}
