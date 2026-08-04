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
