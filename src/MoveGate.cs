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
        private static bool decorateActive;
        private static bool carrying;

        /// <summary>True while the player is inside decorate mode.</summary>
        internal static bool DecorateActive => decorateActive;

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

                if (!decorateActive)
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
            decorateActive = true;
            carrying = false;
        }

        internal static void ExitDecorate()
        {
            decorateActive = false;
            carrying = false;
        }

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
