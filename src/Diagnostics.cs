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
            else if (!Plugin.IncludeWildPlants.Value && itemAsset.VegetationAddon != null)
            {
                reason = "wild vegetation - enable IncludeWildPlants to move it";
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

        internal static void Reset()
        {
            lastConsideredAt = -99f;
            lastAllowedName = null;
        }
    }
}
