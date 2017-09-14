using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Components.Combat;
using Trinity.Framework;
using Trinity.Framework.Actors.ActorTypes;
using Zeta.Bot;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.Actors.Gizmos;
using Zeta.Game.Internals.SNO;

namespace AutoFollow.Resources
{
    using Trinity.Components.Combat.Resources;
    using Trinity.DbProvider;

    public enum CombatState
    {
        None = 0,
        Enabled,
        Disabled,
        Pulsing,
        Movement
    }

    public static class Targetting
    {
        private static DateTime _nextPulse = DateTime.MinValue;
        private static CombatState _state;

        public static CombatState State
        {
            get { return _state; }
            set
            {
                if (Settings.Misc.DebugLogging && _state != value)
                    Log.Info("CombatState Changed to {0}", value);

                _state = value;
            }
        }

        static Targetting()
        {
            Pulsator.OnPulse += Pulsator_OnPulse;
        }

        private static void Pulsator_OnPulse(object sender, EventArgs eventArgs)
        {

            if (ZetaDia.Me == null || !ZetaDia.Me.IsValid)
                return;

            if (!Core.TrinityIsReady)
                return;

            switch (State)
            {
                case CombatState.Pulsing:

                    if (DateTime.UtcNow >= _nextPulse)
                    {
                        ToggleCombat();

                        _nextPulse =
                            DateTime.UtcNow.Add(CombatTargeting.Instance.AllowedToKillMonsters
                                ? TimeSpan.FromMilliseconds(300)
                                : TimeSpan.FromMilliseconds(600));
                    }
                    break;
                case CombatState.Movement:
                    SafeZerg();
                    break;
                case CombatState.Disabled:
                    TurnCombatOff();
                    break;

                default:
                    TurnCombatOn();
                    break;
            }
        }

        private static void ToggleCombat()
        {
            if (TrinityCombat.CombatMode == CombatMode.KillAll || TrinityCombat.CombatMode == CombatMode.Normal)
                TurnCombatOff();
            else
                TurnCombatOn();
        }

        private static void SafeZerg()
        {
            if (TrinityCombat.CombatMode != CombatMode.SafeZerg)
            {
                Log.Debug("Combat was turned to Movement Only");
                TrinityCombat.CombatMode = CombatMode.SafeZerg;
            }
        }

        private static void TurnCombatOff()
        {
            if (TrinityCombat.CombatMode == CombatMode.KillAll || TrinityCombat.CombatMode == CombatMode.Normal)
            {
                Log.Debug("Combat was turned off");
                TrinityCombat.CombatMode = CombatMode.Off;
            }
        }
        private static void TurnCombatOn()
        {
            if (TrinityCombat.CombatMode != CombatMode.KillAll && TrinityCombat.CombatMode != CombatMode.Normal)
            {
                Log.Debug("Combat was turned on");
                TrinityCombat.CombatMode = CombatMode.Normal;
            }
        }

        public static bool IsPriorityTarget => RoutineWantsToLoot() || RoutineWantsToClickGizmo();

        public static bool RoutineWantsToAttackGoblin()
        {
            var combatTarget = TrinityCombat.Targeting.CurrentTarget;
            return combatTarget != null && combatTarget.MonsterInfo.MonsterRace == MonsterRace.TreasureGoblin;
        }

        public static bool RoutineWantsToLoot()
        {
            var combatTarget = TrinityCombat.Targeting.CurrentTarget;
            return combatTarget != null && combatTarget.ActorType == ActorType.Item;
        }

        public static TrinityActor Target => TrinityCombat.Targeting.CurrentTarget;

        public static bool RoutineWantsToClickGizmo()
        {
            //var combatTarget = TrinityCombat.Targeting.CurrentTarget? ;
            //return combatTarget != null && combatTarget is GizmoShrine && combatTarget.Distance < 80f;

            return Target != null && Target.IsGizmo && !Target.IsUsed && Target.Weight > 0 && Target.Distance < 80f;
        }

        public static bool RoutineWantsToAttackUnit()
        {
            var combatTarget = TrinityCombat.Targeting.CurrentTarget?.ToDiaObject();
            return combatTarget != null && combatTarget is DiaUnit && combatTarget.Distance < 80f;
        }

    }
}



