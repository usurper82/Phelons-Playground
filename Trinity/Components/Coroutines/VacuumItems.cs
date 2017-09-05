using System.Collections.Generic;
using System.Linq;
using Trinity.Framework;
using Trinity.Components.Combat.Resources;
using Trinity.Framework.Actors.ActorTypes;
using Zeta.Bot;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Trinity.Framework.Avoidance.Structures;
using System.Threading.Tasks;

namespace Trinity.Components.Coroutines
{
    using System;
    using Adventurer.Game.Actors;
    using Buddy.Coroutines;
    using IronPython.Modules;
    using Zeta.Bot.Coroutines;
    using Zeta.Bot.Navigation;
    using static Core;

    /// <summary>
    /// Pickup items within range.
    /// </summary>
    public class VacuumItems
    {
        static VacuumItems()
        {
            GameEvents.OnWorldChanged += (sender, args) => VacuumedAcdIds.Clear();
        }

        /* Should be able to wait for finishing this task While doing something.
         * Otherwise we see the bot locks in dilemma between going to Quest/Rift
         * or Vacuuming nearby items -Seq */

        public static async Task<bool> Execute()
        {

            bool isVacuuming = false;
            if (Player.IsCasting)
                return isVacuuming = false;

            var oldItems =
                VacuumedAcdIds.Where(x => (DateTime.Now - x.Value).TotalSeconds > 30).Select(x => x.Key).ToList();
            if (oldItems.Any())
            {
                foreach (var item in oldItems)
                {
                    VacuumedAcdIds.Remove(item);
                }
            }

            var count = 0;

            // Items that shouldn't be picked up are currently excluded from cache.
            // a pickup evaluation should be added here if that changes.
            foreach (var item in Targets.OfType<TrinityItem>().OrderBy(x => !x.IsPickupNoClick).ThenBy(x => x.Distance).Where(x => x.IsValid && !VacuumedAcdIds.Keys.Contains(x.AcdId)))
            {
                var validApproach =
                    Grids.Avoidance.IsIntersectedByFlags(Player.Position, item.Position,
                        AvoidanceFlags.NavigationBlocking, AvoidanceFlags.NavigationImpairing) &&
                    !Player.IsFacing(item.Position, 90);
                Inventory.Backpack.Update();
                if (Player.FreeBackpackSlots < 4)
                    break;
                /* Added checkpoints to avoid approach stuck -Seq */
                if (item.Distance > 8f || !validApproach)
                    continue;
                if (!VacuumedAcdIds.Keys.Contains(item.AcdId))
                    VacuumedAcdIds.Add(item.AcdId, DateTime.Now);
                Logger.Warn(
                    $"Vacuumed: {item.Name} ({item.ActorSnoId}) InternalName={item.InternalName} GbId={item.GameBalanceId}");
                SpellHistory.RecordSpell(SNOPower.Axe_Operate_Gizmo);
                item.Interact();
                await Coroutine.Sleep(Math.Max((int)item.Position.Distance2D(Player.Position) * 100, 1000));
                //if (!ZetaDia.Me.UsePower(SNOPower.Axe_Operate_Gizmo, item.Position, Player.WorldDynamicId,
                //        item.AcdId))
                //{
                //    Logger.Warn($"Failed to vacuum item {item.Name} AcdId={item.AcdId}");
                //    continue;
                //}
                count++;
                isVacuuming = true;
            }
            Logger.Verbose($"Vacuumed {count} items");
            return isVacuuming;
        }

        public static Dictionary<int, DateTime> VacuumedAcdIds { get; } = new Dictionary<int, DateTime>();
    }
}