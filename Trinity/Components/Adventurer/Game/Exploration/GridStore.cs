﻿using System;
using Trinity.Framework;
using Trinity.Framework.Helpers;
using System.Collections.Generic;
using System.Linq;
using Zeta.Game;

namespace Trinity.Components.Adventurer.Game.Exploration
{
    public static class GridStore
    {
        // Contains a list of grids that have been created without blocking their garbage disposal.

        internal static List<WeakReference<IGrid>> Grids = new List<WeakReference<IGrid>>();

        public static List<IGrid> GetCurrentGrids()
        {
            var worldId = ZetaDia.Globals.WorldId;
            var result = new List<IGrid>();
            foreach (var gridRef in Grids.ToList())
            {
                IGrid grid;
                if (!gridRef.TryGetTarget(out grid))
                {
                    Grids.Remove(gridRef);
                    continue;
                }

                if (grid.WorldDynamicId == worldId)
                    result.Add(grid);
            }

            Core.Logger.Debug("[GridStore] contains {0} grid instances, {1} for Current world", Grids.Count, result.Count);

            return result;
        }
    }
}