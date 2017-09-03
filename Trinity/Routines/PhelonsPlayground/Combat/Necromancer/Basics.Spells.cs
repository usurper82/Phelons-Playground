using Trinity.Components.Combat.Resources;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Framework.Reference;
using Zeta.Common;
using Zeta.Game.Internals.Actors;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer
{
    public partial class Basics
    {
        public class Spells
        {

            public static TrinityPower ArmyOfTheDead(TrinityActor target, float range = 65f)
                => new TrinityPower(Skills.Necromancer.ArmyOfTheDead, range, target.AcdId);

            public static TrinityPower BloodRush(Vector3 position, float range = 50f)
                => new TrinityPower(Skills.Necromancer.BloodRush, range, position) {CastWhenBlocked = true};

            public static TrinityPower BoneArmor()
                => new TrinityPower(Skills.Necromancer.BoneArmor);

            public static TrinityPower BoneSpear(TrinityActor target, float range = 65f)
                => new TrinityPower(Skills.Necromancer.BoneSpear, range, target.AcdId);

            public static TrinityPower BoneSpikes(TrinityActor target, float range = 65f)
                => new TrinityPower(Skills.Necromancer.BoneSpikes, range, target.AcdId);

            public static TrinityPower BoneSpirit(TrinityActor target, float range = 65f)
                => new TrinityPower(Skills.Necromancer.BoneSpirit, range, target.AcdId);

            public static TrinityPower CommandGolem(TrinityActor target, float range = 65f)
                => new TrinityPower(Skills.Necromancer.CommandGolem, range, target.AcdId);

            public static TrinityPower CommandSkeletons(TrinityActor target, float range = 65f)
                => new TrinityPower(Skills.Necromancer.CommandSkeletons, range, target.AcdId);

            public static TrinityPower CorpseExplosion(Vector3 position, float range = 65f)
                => new TrinityPower(Skills.Necromancer.CorpseExplosion, range, position);

            public static TrinityPower DeathNova(TrinityActor target, float range = 65f)
                => new TrinityPower(Skills.Necromancer.DeathNova, 20f, target.AcdId);

            public static TrinityPower DeathNova()
                => new TrinityPower(Skills.Necromancer.DeathNova);

            public static TrinityPower Devour()
                => new TrinityPower(Skills.Necromancer.Devour);

            public static TrinityPower Frailty(TrinityActor target, float range = 65f)
                => new TrinityPower(Skills.Necromancer.Frailty, 70f, target.AcdId);

            public static TrinityPower GrimScythe(TrinityActor target, float range = 65f)
                => new TrinityPower(Skills.Necromancer.GrimScythe, 15f, target.AcdId);

            public static TrinityPower LandOfTheDead()
                => new TrinityPower(Skills.Necromancer.LandOfTheDead);

            public static TrinityPower Leech(TrinityActor target, float range = 65f)
                => new TrinityPower(Skills.Necromancer.Leech, range, target.AcdId);

            public static TrinityPower Decrepify(TrinityActor target, float range = 65f)
                => new TrinityPower(Skills.Necromancer.Decrepify, range, target.AcdId);

            public static TrinityPower Revive(Vector3 position, float range = 65f)
                => new TrinityPower(Skills.Necromancer.Revive, range, position);

            public static TrinityPower Simulacrum()
                => new TrinityPower(Skills.Necromancer.Simulacrum);

            public static TrinityPower SiphonBlood(TrinityActor target, float range = 65f)
                => new TrinityPower(Skills.Necromancer.SiphonBlood, range, target.AcdId);

            public static TrinityPower SkeletalMage(TrinityActor position, float range = 65f)
                => new TrinityPower(Skills.Necromancer.SkeletalMage, range, position);

            public static TrinityPower CorpseLance(TrinityActor target, float range = 65f)
                => new TrinityPower(Skills.Necromancer.CorpseLance, range, target.AcdId);

            public static TrinityPower Walk(Vector3 position, float range = 0f) =>
                new TrinityPower(SNOPower.Walk, range, position);
        }
    }
}