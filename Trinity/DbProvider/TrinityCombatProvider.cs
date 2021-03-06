﻿using System.Collections.Generic;
using Trinity.Components.Combat;
using Trinity.Components.Combat.Resources;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Routines;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using ITargetingProvider = Zeta.Bot.ITargetingProvider;

namespace Trinity.DbProvider
{
    public class TrinityCombatProvider : ITargetingProvider
    {
        public List<DiaObject> GetObjectsByWeight() => new List<DiaObject> { ZetaDia.Actors.GetActorById(CurrentTargetRActorId) };

        public float CurrentCastRange => CurrentPower?.MinimumRange ?? RoutineBase.DefaultWeaponDistance;

        public TrinityPower CurrentPower => TrinityCombat.Targeting.CurrentPower;

        public TrinityActor CurrentTarget => TrinityCombat.Targeting.CurrentTarget;

        public int CurrentTargetRActorId => CurrentTarget?.RActorId ?? -1;

        public bool IsAvoiding => TrinityCombat.IsCurrentlyAvoiding;

        public bool IsKiting => TrinityCombat.IsCurrentlyKiting;
    }
}