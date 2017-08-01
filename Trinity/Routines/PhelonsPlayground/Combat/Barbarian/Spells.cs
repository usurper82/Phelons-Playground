using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Components.Combat.Resources;
using Trinity.Framework;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Framework.Reference;
using Zeta.Common;
using Zeta.Game.Internals.Actors;

namespace Trinity.Routines.PhelonsPlayground.Combat.Barbarian
{
    public class Spells
    {

        #region Expressions 

        // Primary

        public static TrinityPower Bash(TrinityActor target)
            => new TrinityPower(Skills.Barbarian.Bash, 7f, target.AcdId);

        public static TrinityPower Cleave(TrinityActor target)
            => new TrinityPower(Skills.Barbarian.Cleave, 7f, target.AcdId);

        public static TrinityPower Frenzy(TrinityActor target)
            => new TrinityPower(Skills.Barbarian.Frenzy, 7f, target.AcdId);

        public static TrinityPower WeaponThrow(TrinityActor target)
            => new TrinityPower(Skills.Barbarian.WeaponThrow, 60f, target.AcdId);

        //Secondary

        public static TrinityPower HammerOfTheAncients(TrinityActor target)
            => new TrinityPower(Skills.Barbarian.HammerOfTheAncients, 16f, target.AcdId);

        public static TrinityPower SeismicSlam(TrinityActor target)
            => new TrinityPower(Skills.Barbarian.SeismicSlam, 20f, target.AcdId);

        public static TrinityPower Rend()
            => new TrinityPower(Skills.Barbarian.Rend);

        public static TrinityPower Rend(Vector3 position)
            => new TrinityPower(Skills.Barbarian.Rend, 10f, position);

        public static TrinityPower Whirlwind()
            => new TrinityPower(Skills.Barbarian.Whirlwind);

        public static TrinityPower Whirlwind(Vector3 position)
            => new TrinityPower(Skills.Barbarian.Whirlwind, 9999f, position);

        public static TrinityPower AncientSpear(TrinityActor target)
            => new TrinityPower(Skills.Barbarian.AncientSpear, 70f, target.AcdId);

        // Defensive

        public static TrinityPower GroundStomp()
            => new TrinityPower(Skills.Barbarian.GroundStomp);

        public static TrinityPower GroundStomp(Vector3 position)
            => new TrinityPower(Skills.Barbarian.GroundStomp, 10f, position);

        public static TrinityPower IgnorePain()
            => new TrinityPower(Skills.Barbarian.IgnorePain);

        public static TrinityPower Leap(Vector3 position)
            => new TrinityPower(Skills.Barbarian.Leap, 45f, position);

        public static TrinityPower Sprint()
            => new TrinityPower(Skills.Barbarian.Sprint);

        // Might

        public static TrinityPower Overpower()
            => new TrinityPower(Skills.Barbarian.Overpower);

        public static TrinityPower Overpower(Vector3 position)
            => new TrinityPower(Skills.Barbarian.Overpower, 10f, position);

        public static TrinityPower FuriousCharge(Vector3 position)
            => new TrinityPower(Skills.Barbarian.FuriousCharge, 60f, position);

        public static TrinityPower Revenge()
            => new TrinityPower(Skills.Barbarian.Revenge);

        public static TrinityPower Revenge(Vector3 position)
            => new TrinityPower(Skills.Barbarian.Revenge, 10f, position);

        // Tactics

        public static TrinityPower BattleRage()
            => new TrinityPower(Skills.Barbarian.BattleRage);

        public static TrinityPower ThreateningShout()
            => new TrinityPower(Skills.Barbarian.ThreateningShout);

        public static TrinityPower ThreateningShout(Vector3 position)
            => new TrinityPower(Skills.Barbarian.ThreateningShout, 12f, position);

        public static TrinityPower WarCry()
            => new TrinityPower(Skills.Barbarian.WarCry);

        public static TrinityPower Avalanche()
            => new TrinityPower(Skills.Barbarian.Avalanche);

        public static TrinityPower Avalanche(Vector3 position)
            => new TrinityPower(Skills.Barbarian.Avalanche, 60f, position);

        // Rage

        public static TrinityPower Earthquake()
            => new TrinityPower(Skills.Barbarian.Earthquake);

        public static TrinityPower Earthquake(Vector3 position)
            => new TrinityPower(Skills.Barbarian.Earthquake, 12f, position);

        public static TrinityPower CallOfTheAncients()
            => new TrinityPower(Skills.Barbarian.CallOfTheAncients);

        public static TrinityPower WrathOfTheBerserker()
            => new TrinityPower(Skills.Barbarian.WrathOfTheBerserker);

        #endregion
    }
}
