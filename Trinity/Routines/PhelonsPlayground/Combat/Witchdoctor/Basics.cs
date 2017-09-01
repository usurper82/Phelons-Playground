using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Routines.PhelonsPlayground.Combat.Witchdoctor
{
    using Components.Combat;
    using Components.Combat.Resources;
    using DbProvider;
    using Framework;
    using Framework.Actors.ActorTypes;
    using Framework.Reference;
    using Modules;
    using Utils;
    using Zeta.Common;
    using Zeta.Game.Internals.Actors;
    using static Basics.Spells;

    public partial class Basics
    {        // Misc

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
            if (Basics.Conditionals.ShouldSoulHarvest())
                return SoulHarvest(Player.Position);
            return null;
        }

        public static TrinityPower DestructiblePower()
        {
            if (Skills.WitchDoctor.SpiritBarrage.CanCast())
                return SpiritBarrage(CurrentTarget);
            return null;
        }

        public static TrinityPower MovementPower(Vector3 destination)
        {
            return null;
            //return PlayerMover.IsCompletelyBlocked && Target == null && Skills.WitchDoctor.BloodRush.CanCast() ? Basics.Spells.BloodRush(destination) : null;
        }
        public static int MaxDogs
        {
            get
            {
                var totalcount = 3;
                if (Passives.WitchDoctor.MidnightFeast.IsActive) totalcount += 1;
                if (Passives.WitchDoctor.FierceLoyalty.IsActive) totalcount += 1;
                if (Passives.WitchDoctor.ZombieHandler.IsActive) totalcount += 1;
                if (Legendary.TheTallMansFinger.IsEquipped) totalcount = 1;
                return totalcount;
            }
        }

        public static int MaxGargs
            => Legendary.TheShortMansFinger.IsEquipped ? 3 : 1;

        public static int MaxSoulHarvestStacks
            => Legendary.SacredHarvester.IsEquipped ? 10 : 5;

        public static float FireBatsRange
            => Runes.WitchDoctor.CloudOfBats.IsActive ? 12f : 35f;

        public static bool IsChannellingFirebats
            => Player.IsChannelling && Skills.WitchDoctor.Firebats.IsLastUsed;

        public static bool HasJeramsRevengeBuff
            => Player.HasBuff(SNOPower.P3_ItemPassive_Unique_Ring_010);

        public static bool IsChicken
            => Player.HasBuff(SNOPower.Witchdoctor_Hex, 2);
    }
}
