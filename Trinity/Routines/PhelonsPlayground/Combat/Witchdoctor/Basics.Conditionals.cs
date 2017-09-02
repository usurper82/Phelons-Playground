using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Routines.PhelonsPlayground.Combat.Witchdoctor
{
    using System.Diagnostics;
    using Components.Combat.Resources;
    using Framework;
    using Framework.Actors.ActorTypes;
    using Framework.Helpers;
    using Framework.Reference;
    using Utils;
    using Zeta.Game.Internals.Actors;

    public partial class Basics
    {
        public class Conditionals
        {
            public static bool ShouldLocustSwarm(out TrinityActor target)
            {
                target = Target;

                if (!Skills.WitchDoctor.LocustSwarm.CanCast())
                    return false;

                if (!Targeting.HasDebuff(target, SNOPower.Witchdoctor_Locust_Swarm))
                    return true;

                if (Player.PrimaryResourcePct < 0.90 && Legendary.AquilaCuirass.IsEquipped || SpellHistory.LastPowerUsed == SNOPower.Witchdoctor_Locust_Swarm)
                    return false;

                var percentTargetsWithHaunt = Targeting.DebuffedPercent(SNOPower.Witchdoctor_Locust_Swarm);
                if (percentTargetsWithHaunt > 0.90)
                    return false;
                target =
                    Targeting.UnitsWithOutDebuff(SNOPower.Witchdoctor_Locust_Swarm, Player.Position)
                        .OrderBy(x => x.IsBoss || x.IsElite || x.IsChampion || x.IsMinion)
                        .FirstOrDefault();

                if (target == null)
                    return false;

                Core.Logger.Error(LogCategory.Routine, $"Locust Swarm on {target}");
                return true;
            }

            private static Stopwatch _firstHaunt = new Stopwatch();
            public static bool ShouldHaunt(out TrinityActor target)
            {
                target = Target;

                if (!Skills.WitchDoctor.Haunt.CanCast())
                    return false;

                //if (Player.PrimaryResourcePct < 0.90 && Legendary.AquilaCuirass.IsEquipped)
                //    return false;

                if (_firstHaunt.ElapsedMilliseconds > 4000 && Skills.WitchDoctor.Haunt.TimeSinceUse < 4000)
                {
                    _firstHaunt.Stop();
                    return false;
                }

                if (!_firstHaunt.IsRunning)
                {
                    _firstHaunt.Reset();
                    _firstHaunt.Start();
                }

                if (!Targeting.HasDebuff(target, SNOPower.Witchdoctor_Haunt))
                    return true;

                var percentTargetsWithHaunt = Targeting.DebuffedPercent(SNOPower.Witchdoctor_Haunt);
                if (percentTargetsWithHaunt > 0.90)
                    return false;

                target =
                    Targeting.UnitsWithOutDebuff(SNOPower.Witchdoctor_Haunt, Player.Position)
                        .OrderBy(x => x.IsBoss || x.IsElite || x.IsChampion || x.IsMinion)
                        .FirstOrDefault();

                if (target == null)
                    return false;

                Core.Logger.Error(LogCategory.Routine, $"Haunt on {target}");
                return true;
            }

            public static bool ShouldSoulHarvest()
            {
                if (!Skills.WitchDoctor.SoulHarvest.CanCast())
                    return false;
                var mobCount = Target != null ? 1 : 3;
                if (Targeting.NumMobsInRange() > mobCount)
                    return true;
                return false;
            }
        }
    }
}
