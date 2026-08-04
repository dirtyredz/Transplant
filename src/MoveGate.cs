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
        private static bool toggledOn;

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

                return Plugin.PressToToggle.Value ? toggledOn : Hotkey.IsHeld(Plugin.Modifier.Value);
            }
        }

        /// <summary>
        /// Flips the toggle when the key is pressed.
        ///
        /// The first three builds treated the key as hold-to-arm, which loses a race nobody
        /// should have to win: pressing and releasing arms for a frame or two, and the selection
        /// recomputes as unselectable the moment the key comes back up. The player has to click
        /// inside that window. "Hitting X" was the request, so pressing it now latches.
        /// </summary>
        internal static void PollToggle()
        {
            if (!Plugin.PressToToggle.Value || !DecorateActive)
            {
                return;
            }

            if (!Hotkey.WasPressed(Plugin.Modifier.Value))
            {
                return;
            }

            toggledOn = !toggledOn;
            // Deliberately silent - ConsumeArmedChanged is the single place that reports a
            // change of arming state, whichever mode produced it.
        }

        internal static void EnterDecorate()
        {
            machineActive = true;
            carrying = false;
            toggledOn = false;
            Plugin.Log.LogInfo("Decorate mode opened (state machine).");
        }

        internal static void ExitDecorate()
        {
            machineActive = false;
            carrying = false;
            toggledOn = false;
            Plugin.Log.LogInfo("Decorate mode closed (state machine).");
        }

        internal static void EnterSelect()
        {
            selectActive = true;
            Plugin.Log.LogInfo(
                Plugin.PressToToggle.Value
                    ? $"Selection state active. Press {Plugin.Modifier.Value.MainKey} to arm."
                    : $"Selection state active. Hold {Plugin.Modifier.Value.MainKey} to arm.");
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
            Plugin.Log.LogInfo(armed ? "Armed - plants are selectable." : "Disarmed.");
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

            if (!Plugin.IncludeWildPlants.Value && !IsPlayerPlanted(gridObjectView))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Whether the player planted this, as opposed to the world growing it.
        ///
        /// The obvious test - ItemAsset.VegetationAddon != null - is wrong, and the log proved
        /// it: Item_Wild_Vegetation_Weeds_5 sailed straight through it and was offered as
        /// movable. Weeds are plain growables with no vegetation addon. GrowablePersistence
        /// carries the flag the game itself uses.
        ///
        /// Find() rather than FindOrCreate(), which would write a growable record for anything
        /// the cursor passed over.
        /// </summary>
        internal static bool IsPlayerPlanted(IGridObjectView gridObjectView)
        {
            var persistence = gridObjectView?.GridObjectPersistence;
            if (persistence == null)
            {
                return false;
            }

            var gamePersistence = GamePersistence.Instance;
            if (gamePersistence == null || gamePersistence.CurrentRoom == null)
            {
                return false;
            }

            var growable = gamePersistence.CurrentRoom.Growables.Find(persistence.Guid);
            return growable != null && growable.IsPlayerPlanted;
        }
    }
}
