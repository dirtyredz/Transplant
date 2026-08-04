namespace Transplant
{
    /// <summary>
    /// Decides when a growing plant counts as movable, and remembers that a plant is currently
    /// in hand.
    ///
    /// The "carrying" half is not a convenience. Arming is normally tied to a held key, but the
    /// player has to be able to let go of that key while carrying a plant - and
    /// ObjectPickupAction.Cancel restores the original position only for objects that read as
    /// movable *at the moment Esc is pressed*. Disarm mid-carry and cancelling would drop the
    /// plant wherever the cursor happened to be. So a pickup latches arming on until the plant
    /// is placed or cancelled.
    /// </summary>
    internal static class MoveGate
    {
        private static bool machineActive;
        private static bool selectActive;
        private static bool carrying;

        /// <summary>
        /// True while the player is inside decorate mode.
        ///
        /// Two independent signals, because the first build set this only from
        /// PlayerDecorateStateMachine and there was no way to tell from the outside whether that
        /// had fired. DecorateSelectState is the state that actually does the selecting, so if
        /// either says decorate mode is open, it is.
        /// </summary>
        internal static bool DecorateActive => machineActive || selectActive;

        /// <summary>True while a growable has been picked up and not yet put back down.</summary>
        internal static bool Carrying => carrying;

        /// <summary>
        /// Whether plants should currently read as movable. Carrying always counts - see the
        /// class comment for why letting this go false mid-carry loses the plant.
        /// </summary>
        internal static bool Armed
        {
            get
            {
                if (!Plugin.Enabled.Value)
                {
                    return false;
                }

                if (!DecorateActive)
                {
                    return false;
                }

                if (carrying)
                {
                    return true;
                }

                if (!Plugin.RequireModifier.Value)
                {
                    return true;
                }

                return Hotkey.IsHeld(Plugin.Modifier.Value);
            }
        }

        internal static void EnterDecorate()
        {
            machineActive = true;
            carrying = false;
            Plugin.Log.LogInfo("Decorate mode opened (state machine).");
        }

        internal static void ExitDecorate()
        {
            machineActive = false;
            carrying = false;
            Plugin.Log.LogInfo("Decorate mode closed (state machine).");
        }

        internal static void EnterSelect()
        {
            selectActive = true;
            Plugin.Log.LogInfo(
                $"Selection state active. Hold {Plugin.Modifier.Value.MainKey} to arm.");
        }

        internal static void ExitSelect()
        {
            selectActive = false;
        }

        /// <summary>
        /// Whether Armed flipped since the last call, so the caller can force the game to
        /// recompute its selection.
        ///
        /// This exists because DecorateSelectState.ProcessSelection opens with
        ///
        ///     if (!DecorateCursor.Instance.MovedThisFrame &amp;&amp; !forceUpdate) return;
        ///
        /// so pressing the arming key while the mouse is still changes nothing at all - the
        /// gate is simply never consulted. That was the whole of why 0.1.0 appeared to do
        /// nothing: it worked only if you happened to move the mouse while holding the key.
        /// </summary>
        internal static bool ConsumeArmedChanged()
        {
            var armed = Armed;
            if (armed == lastArmed)
            {
                return false;
            }

            lastArmed = armed;
            Plugin.Log.LogInfo(armed
                ? "Armed - plants are selectable while the key is held."
                : "Disarmed - plants are no longer selectable.");
            return true;
        }

        private static bool lastArmed;

        internal static void PickedUp()
        {
            carrying = true;
        }

        internal static void PutDown()
        {
            carrying = false;
        }

        /// <summary>
        /// Whether this object is a plant this mod is willing to move.
        ///
        /// Stacked objects are excluded outright: herb-garden pots live on a SubGridSurface and
        /// go through DecorateSelectStackedState, a different state machine than the one this
        /// mod patches. Letting them look movable here would arm a path that was never tested.
        /// </summary>
        internal static bool IsMovablePlant(IGridObjectView gridObjectView)
        {
            if (gridObjectView == null || gridObjectView.IsDestroyed)
            {
                return false;
            }

            var itemAsset = gridObjectView.ItemAsset;
            if (itemAsset == null || itemAsset.GrowableAddon == null)
            {
                return false;
            }

            if (gridObjectView.ParentSubGridSurface != null)
            {
                return false;
            }

            if (!Plugin.IncludeWildPlants.Value && itemAsset.VegetationAddon != null)
            {
                return false;
            }

            return true;
        }
    }
}
