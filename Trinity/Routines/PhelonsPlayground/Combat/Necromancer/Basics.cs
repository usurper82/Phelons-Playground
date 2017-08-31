using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer
{
    using Components.Combat;
    using Components.Combat.Resources;
    using DbProvider;
    using Framework;
    using Framework.Actors.ActorTypes;
    using Framework.Reference;
    using Modules;
    using Zeta.Common;
    using Zeta.Game.Internals.Actors;
    using static Basics.Conditionals;

    public partial class Basics
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
            if (ShouldDevour())
                return Spells.Devour();
            return null;
        }

        public static TrinityPower DestructiblePower()
        {
            if (Skills.Necromancer.CommandSkeletons.CanCast())
                return Spells.CommandSkeletons(CurrentTarget);

            if (Skills.Necromancer.BoneSpikes.CanCast())
                return Spells.BoneSpikes(CurrentTarget);

            if (Skills.Necromancer.SiphonBlood.CanCast())
                return Spells.SiphonBlood(CurrentTarget);

            if (Skills.Necromancer.GrimScythe.CanCast())
                return Spells.GrimScythe(CurrentTarget);
            return null;
        }

        public static TrinityPower MovementPower(Vector3 destination)
        {
            return PlayerMover.IsCompletelyBlocked && Target == null && Skills.Necromancer.BloodRush.CanCast() ? Spells.BloodRush(destination) : null;
        }
    }
}
