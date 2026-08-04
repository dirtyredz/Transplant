using System.Text;
using UnityEngine;

namespace Transplant
{
    /// <summary>
    /// Enough logging to tell the three "it does nothing" failures apart without another guess:
    ///
    ///   1. decorate mode never registers      -> no "Selection state active" line
    ///   2. the key never registers            -> no "Armed" line
    ///   3. the gate is asked and says no      -> "considered" lines but no "now movable"
    ///
    /// 0.1.0 shipped with none of this and cost a whole test cycle to learn only that something
    /// upstream was wrong. These lines are rate-limited rather than gated behind VerboseLogging,
    /// because the one time they matter is the run where the player did not know to turn it on.
    /// </summary>
    internal static class Diagnostics
    {
        private const float MinSecondsBetweenLines = 2f;

        private static float lastConsideredAt = -99f;
        private static string lastAllowedName;
        private static Vector3Int lastColumn = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
        private static string lastVerdictKey;

        internal static void Considered(IGridObjectView gridObjectView)
        {
            if (Time.realtimeSinceStartup - lastConsideredAt < MinSecondsBetweenLines)
            {
                return;
            }

            lastConsideredAt = Time.realtimeSinceStartup;

            var itemAsset = gridObjectView?.ItemAsset;
            if (itemAsset == null)
            {
                Plugin.Log.LogInfo("Armed, but the object under the cursor has no item asset.");
                return;
            }

            // The reason matters: "no GrowableAddon" means the thing being pointed at is not a
            // plant at all, which points at the selection path rather than at this gate.
            string reason;
            if (itemAsset.GrowableAddon == null)
            {
                reason = "not a growable";
            }
            else if (gridObjectView.ParentSubGridSurface != null)
            {
                reason = "sits on a sub-grid (herb pot) - out of scope";
            }
            else if (!Plugin.IncludeWildPlants.Value && !MoveGate.IsPlayerPlanted(gridObjectView))
            {
                reason = "not player-planted - enable IncludeWildPlants to move it";
            }
            else
            {
                reason = "excluded";
            }

            Plugin.Log.LogInfo($"Armed, considered '{itemAsset.name}': {reason}.");
        }

        internal static void Allowed(IGridObjectView gridObjectView)
        {
            var name = gridObjectView?.ItemAsset?.name ?? "?";
            if (name == lastAllowedName)
            {
                return;
            }

            lastAllowedName = name;
            Plugin.Log.LogInfo($"'{name}' at {gridObjectView.Position} is now movable.");
        }

        /// <summary>
        /// Dumps everything the game found in the column under the cursor, once per cell, while
        /// armed. This is the line that says whether an object the player is pointing at is
        /// even a selection candidate.
        /// </summary>
        internal static void Column(GridSurface grid, int x, int y, int z, IGridObjectView result)
        {
            if (!MoveGate.Armed || grid == null)
            {
                return;
            }

            var cell = new Vector3Int(x, y, z);
            if (cell == lastColumn)
            {
                return;
            }

            lastColumn = cell;

            var found = new StringBuilder();
            // Same five-cell upward walk the game's own column search does.
            for (var i = 0; i < 5; i++)
            {
                var objects = grid.GetGridObjects(new Vector3Int(x, y + i, z));
                if (objects == null)
                {
                    continue;
                }

                foreach (var view in objects)
                {
                    var itemAsset = view?.ItemAsset;
                    if (itemAsset == null)
                    {
                        continue;
                    }

                    if (found.Length > 0)
                    {
                        found.Append(", ");
                    }

                    found.Append($"{itemAsset.name}[y+{i}");
                    if (itemAsset.GrowableAddon != null)
                    {
                        found.Append(" growable");
                    }

                    if (itemAsset.PathAddon != null)
                    {
                        found.Append(" path");
                    }

                    if (!view.IsAddedToWorldGrid)
                    {
                        found.Append(" NOT-ON-GRID");
                    }

                    found.Append(']');
                }
            }

            Plugin.Log.LogInfo(
                $"Column ({x},{y},{z}) contains: {(found.Length > 0 ? found.ToString() : "nothing")} " +
                $"-> selected {result?.ItemAsset?.name ?? "nothing"}");
        }

        /// <summary>
        /// The game's own verdict, which is reached through two checks that run before ours and
        /// return early - a locked tile, or an object not on the world grid.
        /// </summary>
        internal static void Verdict(IGridObjectView gridObjectView, bool result)
        {
            if (!MoveGate.Armed)
            {
                return;
            }

            var itemAsset = gridObjectView?.ItemAsset;
            if (itemAsset == null || itemAsset.GrowableAddon == null)
            {
                return;
            }

            var key = $"{itemAsset.name}:{result}";
            if (key == lastVerdictKey)
            {
                return;
            }

            lastVerdictKey = key;
            Plugin.Log.LogInfo(
                $"Game gate on '{itemAsset.name}': {(result ? "movable" : "NOT movable")} " +
                $"(onGrid={gridObjectView.IsAddedToWorldGrid}, playerPlanted={MoveGate.IsPlayerPlanted(gridObjectView)}).");
        }

        internal static void Reset()
        {
            lastConsideredAt = -99f;
            lastAllowedName = null;
            lastColumn = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
            lastVerdictKey = null;
        }
    }
}
