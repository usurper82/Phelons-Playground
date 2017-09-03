using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Routines.PhelonsPlayground.Combat.Witchdoctor
{
    using Components.Combat.Resources;
    using Framework.Actors.ActorTypes;
    using Framework.Reference;
    using Zeta.Common;
    using Zeta.Game.Internals.Actors;

    public partial class Basics
    {
        public class Spells
        {
            // Primary

            public static TrinityPower PoisonDart(TrinityActor target)
                => new TrinityPower(Skills.WitchDoctor.PoisonDart, 65f, target.AcdId);

            public static TrinityPower CorpseSpiders(TrinityActor target)
                => new TrinityPower(Skills.WitchDoctor.CorpseSpiders, 65f, target.AcdId);

            public static TrinityPower PlagueOfToads(TrinityActor target)
                => new TrinityPower(Skills.WitchDoctor.PlagueOfToads, 45f, target.AcdId);

            public static TrinityPower Firebomb(TrinityActor target)
                => new TrinityPower(Skills.WitchDoctor.Firebomb, 65f, target.AcdId);

            // Secondary

            public static TrinityPower GraspOfTheDead(TrinityActor target)
                => new TrinityPower(Skills.WitchDoctor.GraspOfTheDead, 65f, target.AcdId);

            public static TrinityPower Firebats()
                => new TrinityPower(Skills.WitchDoctor.Firebats, 100, 250);

            public static TrinityPower Firebats(TrinityActor target)
                => new TrinityPower(Skills.WitchDoctor.Firebats, FireBatsRange, target.Position, 100, 250);

            public static TrinityPower Haunt(TrinityActor target)
                => new TrinityPower(Skills.WitchDoctor.Haunt, 70f, target.AcdId);

            public static TrinityPower LocustSwarm(TrinityActor target)
                => new TrinityPower(Skills.WitchDoctor.LocustSwarm, 45f, target.AcdId);

            // Defensive

            public static TrinityPower SummonZombieDogs()
                => new TrinityPower(Skills.WitchDoctor.SummonZombieDogs);

            public static TrinityPower SummonZombieDogs(Vector3 position)
                => new TrinityPower(Skills.WitchDoctor.SummonZombieDogs, 65f, position);

            public static TrinityPower Horrify()
                => new TrinityPower(Skills.WitchDoctor.Horrify);

            public static TrinityPower SpiritWalk()
                => new TrinityPower(Skills.WitchDoctor.SpiritWalk);

            public static TrinityPower Hex(Vector3 position)
                => new TrinityPower(Skills.WitchDoctor.Hex, 65f, position) {CastWhenBlocked = true};

            public static TrinityPower ExplodeChicken(Vector3 position)
                => new TrinityPower(SNOPower.Witchdoctor_Hex_Explode, 10f, position);

            // Terror

            public static TrinityPower SoulHarvest()
                => new TrinityPower(Skills.WitchDoctor.SoulHarvest);

            public static TrinityPower SoulHarvest(Vector3 position)
                => new TrinityPower(Skills.WitchDoctor.SoulHarvest, 12f, position);

            public static TrinityPower AcidCloud(TrinityActor target)
                => new TrinityPower(Skills.WitchDoctor.AcidCloud, 65f, target.AcdId);

            public static TrinityPower Sacrifice()
                => new TrinityPower(Skills.WitchDoctor.Sacrifice);

            public static TrinityPower MassConfusion()
                => new TrinityPower(Skills.WitchDoctor.MassConfusion);

            // Decay

            public static TrinityPower ZombieCharger(TrinityActor target)
                => new TrinityPower(Skills.WitchDoctor.ZombieCharger, 45f, target.AcdId);

            public static TrinityPower SpiritBarrage(TrinityActor target)
                => new TrinityPower(Skills.WitchDoctor.SpiritBarrage, 45f, target.AcdId);

            public static TrinityPower WallOfDeath(TrinityActor target)
                => new TrinityPower(Skills.WitchDoctor.WallOfDeath, 65f, target.AcdId);

            public static TrinityPower WallOfDeath(Vector3 position)
                => new TrinityPower(SNOPower.Witchdoctor_WallOfZombies, 65f, position);

            public static TrinityPower Piranhas(TrinityActor target)
                => new TrinityPower(Skills.WitchDoctor.Piranhas, 45f, target.AcdId);

            // Voodoo

            public static TrinityPower Gargantuan()
                => new TrinityPower(Skills.WitchDoctor.Gargantuan);

            public static TrinityPower Gargantuan(Vector3 position)
                => new TrinityPower(Skills.WitchDoctor.Gargantuan, 45f, position);

            public static TrinityPower BigBadVoodoo()
                => new TrinityPower(Skills.WitchDoctor.BigBadVoodoo);

            public static TrinityPower BigBadVoodoo(Vector3 position)
                => new TrinityPower(Skills.WitchDoctor.BigBadVoodoo, 65f, position);

            public static TrinityPower FetishArmy()
                => new TrinityPower(Skills.WitchDoctor.FetishArmy);

            public static TrinityPower Walk(Vector3 position, float range = 0f) =>
                new TrinityPower(SNOPower.Walk, range, position);
        }
    }
}