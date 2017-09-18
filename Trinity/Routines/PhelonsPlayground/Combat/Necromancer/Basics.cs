﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer
{
    using System.Web.Routing;
    using Components.Adventurer.Game.Exploration;
    using Components.Combat;
    using Components.Combat.Resources;
    using DbProvider;
    using Framework;
    using Framework.Actors.ActorTypes;
    using Framework.Grid;
    using Framework.Objects;
    using Framework.Reference;
    using Modules;
    using Utils;
    using Zeta.Common;
    using Zeta.Game.Internals.Actors;
    using static Basics.Conditionals;

    public partial class Basics : RoutineBase
    {
        public static TrinityActor Target { get; set; }
        public static PlayerCache Player => Core.Player;
        public static TrinityActor CurrentTarget => TrinityCombat.Targeting.CurrentTarget;
        public static HashSet<SNOPower> Hotbar => Core.Hotbar.ActivePowers;

        public static TrinityPower DefensivePower()
        {
            return null;
        }

        public static TrinityPower BuffPower()
        {
            if (Targeting.CorpseCount(60f) > 3)
                return Spells.Devour();
            if (ShouldBoneArmor())
                return Spells.BoneArmor();
            return null;
        }

        public static TrinityPower DestructiblePower()
        {
            if (CurrentTarget.IsInLineOfSight && CurrentTarget.Distance < 45)
            {
                return Core.Routines.CurrentRoutine.GetOffensivePower();
                if (Skills.Necromancer.BoneSpikes.CanCast())
                    return Spells.BoneSpikes(CurrentTarget);

                if (Skills.Necromancer.SiphonBlood.CanCast())
                    return Spells.SiphonBlood(CurrentTarget);

                if (Skills.Necromancer.GrimScythe.CanCast())
                    return Spells.GrimScythe(CurrentTarget);
            }
            return DefaultPower;
        }

        public static TrinityPower MovementPower(Vector3 destination)
        {
            if (Player.IsInTown)
                return null;
            var projectedPosition = IsBlocked
                    ? Core.Grids.Avoidance.GetPathCastPosition(45f, true)
                    : Core.Grids.Avoidance.GetPathWalkPosition(45f, true);
            return (!Player.Position.EasyNavToPosition(destination) || projectedPosition.Distance(Player.Position) > 10 || HasInstantCooldowns) &&
                   Skills.Necromancer.BloodRush.CanCast()
                ? Spells.BloodRush(destination)
                : Spells.Walk(destination);
        }
    }
}
