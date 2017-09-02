using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer
{
    using Framework;
    using Framework.Actors.ActorTypes;
    using Framework.Helpers;
    using Framework.Reference;
    using Utils;
    using Zeta.Common;
    using Zeta.Game.Internals.Actors;

    public partial class Basics
    {
        public class Conditionals
        {
            #region Primary Skills

            public static bool ShouldBoneSpikes()
            {
                if (!Skills.Necromancer.BoneSpikes.CanCast())
                    return false;
                Core.Logger.Error(LogCategory.Routine,
                    $"[Bone Spikes] - On {Target}.");
                return true;
            }

            public static bool ShouldSiphonBlood()
            {
                if (!Skills.Necromancer.SiphonBlood.CanCast())
                    return false;
                Core.Logger.Error(LogCategory.Routine,
                    $"[Siphon Blood] - On {Target}.");
                return true;
            }

            public static bool ShouldGrimScythe(out TrinityActor target)
            {
                target = null;
                if (!Skills.Necromancer.GrimScythe.CanCast())
                    return false;

                target = (Targeting.BestTargetWithoutDebuff(12f, SNOPower.P6_Necro_Decrepify, Player.Position) ??
                          Targeting.BestTargetWithoutDebuff(12f, SNOPower.P6_Necro_Leech, Player.Position) ??
                          Targeting.BestTargetWithoutDebuff(12f, SNOPower.P6_Necro_Frailty, Player.Position)) ??
                         Target;
                if (target == null)
                    return false;
                Core.Logger.Error(LogCategory.Routine,
                    $"[Grim Scythe] - On {target}.");
                return true;
            }
            #endregion Primary Skills

            #region Defensives

            public static bool ShouldBoneArmor()
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
                Core.Logger.Error(LogCategory.Routine,
                    $"[BoneArmor] - I have {Skills.Necromancer.BoneArmor.BuffStacks} stacks with {unitsNearMe} mobs near me.");
                return true;
            }

            #endregion Defensives

            #region Debuffs

            public static bool ShouldDevour()
            {
                if (!Skills.Necromancer.Devour.CanCast())
                    return false;
                var corpseCount = Targeting.CorpseCount(60f);
                if (CurrentTarget != null && Player.PrimaryResourceMax - Player.PrimaryResource > corpseCount * 10 ||
                    Player.PrimaryResourcePct > 0.85 || corpseCount < 1)
                    return false;
                Core.Logger.Error(LogCategory.Routine,
                    $"[Devour] - ({corpseCount}) Corpses with ({Player.PrimaryResourceMax - Player.PrimaryResource}) Essence Deficit.");
                return true;
            }

            public static bool ShouldFrailty(out TrinityActor target)
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

            public static bool ShouldDecrepify(out TrinityActor target)
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

            #endregion Debuffs

            #region Cooldowns

            public static bool ShouldLandOfTheDead()
            {
                if (!Skills.Necromancer.LandOfTheDead.CanCast() || Player.PrimaryResourcePct < 0.90 ||
                    Skills.Necromancer.Simulacrum.IsActive && !Skills.Necromancer.Simulacrum.CanCast())
                    return false;
                var elite = Targeting.BestLOSEliteInRange(65f);
                if (elite == null || Skills.Necromancer.LandOfTheDead.TimeSinceUse < 10000)
                    return false;

                //Trying to alternate cooldowns
                if (!Skills.Necromancer.CorpseLance.IsActive && 
					Targeting.Players.Any() && (Skills.Necromancer.Frailty.CanCast() && elite.IsChampion ||
                                                Skills.Necromancer.Decrepify.CanCast() && elite.IsElite))
                    return false;

                Core.Logger.Error(LogCategory.Routine,
                    $"[Land of the Dead] - Because of Elite {elite}.");
                return true;
            }

            public static bool ShouldSimulacrum()
            {
                if (!Skills.Necromancer.Simulacrum.CanCast())
                    return false;

                var elite = Targeting.BestLOSEliteInRange(65f);
                if (elite == null || Skills.Necromancer.Simulacrum.TimeSinceUse < 10000 ||
                    Player.PrimaryResourcePct < 0.90)
                    return false;

                //Trying to alternate cooldowns
                if (!Skills.Necromancer.CorpseLance.IsActive &&
                    Targeting.Players.Any() && (Skills.Necromancer.Frailty.CanCast() && elite.IsChampion ||
                                                Skills.Necromancer.Decrepify.CanCast() && elite.IsElite))
                    return false;

                Core.Logger.Error(LogCategory.Routine,
                    $"[Simulaccrum] - Because of Elite {elite}.");
                return true;
            }

            #endregion Debuffs
        }
    }
}