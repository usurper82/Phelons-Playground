using System;
using Trinity.DbProvider;
using Trinity.Framework;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Framework.Helpers;
using Trinity.Framework.Objects;
using Trinity.Framework.Reference;
using Trinity.Routines.PhelonsPlayground.Utils;
using Zeta.Common;
using Zeta.Game.Internals.Actors;

namespace Trinity.Routines.PhelonsPlayground.Combat.Necromancer.Inarius
{
    using System.Linq;
    using static Basics;

    public partial class Inarius
    {
        public virtual bool ShouldBoneSpirit()
        {
            if (!Skills.Necromancer.BoneSpirit.CanCast())
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $"[BoneSpirit] - On {Target}.");
            return true;
        }

        public virtual bool ShouldCorpseExplosion()
        {
            if (!Skills.Necromancer.CorpseExplosion.CanCast())
                return false;
            var corpseCount = Targeting.CorpseCountNearLocation(Target.Position, 10f);
            if (corpseCount < 3 || Skills.Necromancer.CorpseExplosion.TimeSinceUse < 2000)
                return false;
            Core.Logger.Error(LogCategory.Routine,
                $"[CorpseExplosion] - ({corpseCount}) Corpses to Explode.");
            return true;
        }
    }
}