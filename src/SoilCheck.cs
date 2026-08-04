using System.Collections.Generic;
using Chicken.Utilities;
using UnityEngine;

namespace Transplant
{
    /// <summary>
    /// The rule the whole mod exists to enforce: a plant may only be put down on a cell that
    /// has something waterable on it.
    ///
    /// Watering is not stored on the plant. WaterGrowStageRequirement resolves it by *position*:
    ///
    ///     foreach (GridObjectPersistence g in CurrentRoom.GridObjects)
    ///         if (g.Position == plant.Position)
    ///             foreach (WaterablePersistence w in CurrentRoom.Waterables)
    ///                 if (w.Guid == g.Guid) return w;
    ///     return null;                      // -> requirement is never completed
    ///
    /// A null there means the water requirement can never be satisfied again, so a plant moved
    /// onto bare ground stops growing permanently while still looking perfectly healthy. There
    /// is no warning and no log line; the player finds out days later.
    ///
    /// This mirrors that lookup exactly rather than asking "is there a farm tile here", because
    /// the game's question is the one that decides whether the plant lives. Mirroring it also
    /// answers two of the research questions for free:
    ///
    ///  - if a growable turns out to carry its own WaterableView, it satisfies its own lookup
    ///    here just as it would in the game, and moving it anywhere is genuinely safe;
    ///  - the check is on the origin cell only, because that is the only cell
    ///    WaterGrowStageRequirement looks at. Testing the whole footprint would be stricter
    ///    than the game and would block legitimate moves of multi-cell trees.
    ///
    /// Everything here is read-only. Find() returns null when absent - unlike FindOrCreate(),
    /// which writes a record for whatever it is asked about.
    /// </summary>
    internal static class SoilCheck
    {
        private static Vector2Int cachedCell;
        private static int cachedFrame = -1;
        private static bool cachedResult;

        // Keyed by ItemAsset, because whether a plant can ever need water is a property of the
        // asset rather than of the individual plant.
        private static readonly Dictionary<ItemAsset, bool> needsWaterCache =
            new Dictionary<ItemAsset, bool>();

        /// <summary>
        /// Whether this kind of plant can ever be gated on watering.
        ///
        /// The soil rule exists to stop a plant being stranded somewhere its water requirement
        /// can never be satisfied. A plant with no water requirement anywhere in its grow graph
        /// has nothing to strand, so applying the rule to it would refuse perfectly good
        /// placements for no reason - which is exactly what would happen to wild trees and
        /// bushes, since they grow through WildTreeGrowStageRequirement and are not watered.
        ///
        /// Enumerating the requirement components is safe. Evaluating them is not:
        /// RandomChanceGrowStageRequirement.IsRequirementCompleted consumes Unity's global RNG,
        /// so GrowPath.CheckIfRequirementsMet and GrowStageContainer.GetDesiredGrowPath must
        /// never be called from here. Same rule Plant Peek follows.
        /// </summary>
        internal static bool NeedsWater(IGridObjectView gridObjectView)
        {
            var itemAsset = gridObjectView?.ItemAsset;
            if (itemAsset == null)
            {
                return true;
            }

            if (needsWaterCache.TryGetValue(itemAsset, out var cached))
            {
                return cached;
            }

            var result = ScanForWaterRequirement(itemAsset);
            needsWaterCache[itemAsset] = result;
            return result;
        }

        private static bool ScanForWaterRequirement(ItemAsset itemAsset)
        {
            var container = itemAsset.GrowableAddon?.GrowStageContainer;
            var stages = container?.CachedGrowStages;
            if (stages == null)
            {
                // Unknown shape - assume it needs water, because the failure this rule prevents
                // is silent and permanent while a wrong refusal is merely annoying.
                return true;
            }

            foreach (var stage in stages)
            {
                if (stage?.GrowPaths == null)
                {
                    continue;
                }

                foreach (var path in stage.GrowPaths)
                {
                    if (path == null)
                    {
                        continue;
                    }

                    foreach (var requirement in path.GetComponents<IGrowStageRequirement>())
                    {
                        if (requirement is WaterGrowStageRequirement)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Set when a placement was refused purely because of this rule, so the shout can
        /// explain the real reason instead of the game's generic "cannot place here".
        /// </summary>
        internal static int VetoedOnFrame = -1;

        internal static bool HasWaterableAt(Vector2Int cell)
        {
            // Placement is validated several times per frame for the same cell, once per
            // footprint cell. Only the first one does the scan.
            var frame = Time.frameCount;
            if (cachedFrame == frame && cachedCell == cell)
            {
                return cachedResult;
            }

            cachedCell = cell;
            cachedFrame = frame;
            cachedResult = Scan(cell);
            return cachedResult;
        }

        private static bool Scan(Vector2Int cell)
        {
            var persistence = GamePersistence.Instance;
            if (persistence == null)
            {
                // No save loaded. Refusing here would be wrong; there is nothing to protect.
                return true;
            }

            var room = persistence.CurrentRoom;
            if (room == null)
            {
                return true;
            }

            foreach (var gridObject in room.GetGridObjectPersistences(cell))
            {
                if (gridObject == null)
                {
                    continue;
                }

                if (room.Waterables.Find(gridObject.Guid) != null)
                {
                    return true;
                }
            }

            return false;
        }

        internal static void Reset()
        {
            cachedFrame = -1;
            VetoedOnFrame = -1;
        }
    }
}
