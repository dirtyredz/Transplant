using System;
using Chicken.Utilities;
using HarmonyLib;
using UnityEngine;

namespace Transplant
{
    /// <summary>
    /// Tracks whether decorate mode is open, and whether a plant is in hand.
    /// </summary>
    [HarmonyPatch(typeof(PlayerDecorateStateMachine))]
    internal static class DecorateStatePatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnActivate")]
        internal static void AfterActivate()
        {
            MoveGate.EnterDecorate();
            SoilCheck.Reset();
            Diagnostics.Reset();
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDeactivate")]
        internal static void AfterDeactivate()
        {
            MoveGate.ExitDecorate();
            SoilCheck.Reset();
            Diagnostics.Reset();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerDecorateStateMachine.PickUpGridObjectView))]
        internal static void AfterPickUp(IGridObjectView newGridObjectView)
        {
            if (MoveGate.IsMovablePlant(newGridObjectView))
            {
                MoveGate.PickedUp();

                if (Plugin.VerboseLogging.Value)
                {
                    Plugin.Log.LogInfo(
                        $"Picked up {newGridObjectView.ItemAsset.name} at {newGridObjectView.Position}.");
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerDecorateStateMachine.PlaceGridObjectView))]
        internal static void AfterPlace()
        {
            MoveGate.PutDown();
        }
    }

    /// <summary>
    /// Pure visibility. Every decorate state derives from BaseDecorateState and routes through
    /// this, so one line per state type proves which of them actually run.
    ///
    /// 0.1.3 produced a session log with no decorate lines at all, which left "the player never
    /// opened decorate mode" and "the states we hook are not the ones this game uses"
    /// indistinguishable. This settles that without another round of guessing.
    /// </summary>
    [HarmonyPatch(typeof(BaseDecorateState), "OnActivate")]
    internal static class DecorateStateProbe
    {
        [HarmonyPostfix]
        internal static void Postfix(BaseDecorateState __instance)
        {
            Diagnostics.DecorateStateEntered(__instance?.GetType().Name);
        }
    }

    /// <summary>
    /// Makes the arming key actually do something the moment it is pressed.
    ///
    /// DecorateSelectState.ProcessSelection returns immediately unless the cursor moved this
    /// frame or forceUpdate is set. Arming while the mouse is still therefore changed nothing
    /// in 0.1.0 - the gate below was never reached. Setting forceUpdate on the frame the
    /// arming state flips makes the game re-run its own selection with the new answer.
    ///
    /// This also tracks whether the selection state is open, which is the more reliable of the
    /// two "is decorate mode active" signals since it is the state that does the selecting.
    /// </summary>
    [HarmonyPatch(typeof(DecorateSelectState))]
    internal static class SelectStatePatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnActivate")]
        internal static void AfterActivate()
        {
            MoveGate.EnterSelect();
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDeactivate")]
        internal static void AfterDeactivate()
        {
            MoveGate.ExitSelect();
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnActiveUpdate")]
        internal static void BeforeUpdate(ref bool ___forceUpdate)
        {
            MoveGate.PollToggle();

            if (MoveGate.ConsumeArmedChanged())
            {
                ___forceUpdate = true;
            }
        }

        /// <summary>
        /// Probe: what the game actually finds in the column under the cursor.
        ///
        /// A grape vine produced no gate log line at all, which means the gate was never asked
        /// about it - the selection gave up first. This says whether the vine is even a
        /// candidate, which separates "not registered at the cell the cursor is over" from
        /// "found but rejected".
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch("GetLowestMovableObjectAtColumn")]
        internal static void AfterColumn(GridSurface grid, int x, int y, int z, IGridObjectView __result)
        {
            Diagnostics.Column(grid, x, y, z, __result);
        }

        /// <summary>
        /// Probe: the game's own gate, which runs two checks of its own before it consults ours
        /// and returns early on either.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch("CanMoveGridObject")]
        internal static void AfterCanMove(IGridObjectView gridObjectView, bool __result)
        {
            Diagnostics.Verdict(gridObjectView, __result);
        }
    }

    /// <summary>
    /// The pickup gate. Decorate mode refuses plants for exactly one reason - their ItemAsset
    /// does not carry GridControlType.Movable - and this is the only method that reads it on the
    /// selection path.
    ///
    /// Patched here rather than on GridObjectItemAddon.ControlTypes, which the research
    /// originally suggested. That property is a trivial auto-property getter, and a getter small
    /// enough for the Mono JIT to inline is a poor Harmony target: call sites already inlined
    /// before the patch applies would keep the old answer. This method has a real body, so it
    /// cannot be inlined away, and the cost is one extra patch on ObjectPickupAction.Cancel
    /// below.
    ///
    /// Note this deliberately patches the *base* implementation.
    /// MoveGridObjectSpellDecorateStateMachineContext overrides it without calling base, so the
    /// in-game Move Grid Object spell keeps its own rules and is untouched by this mod.
    /// </summary>
    [HarmonyPatch(typeof(BaseDecorateStateMachineContext), nameof(BaseDecorateStateMachineContext.CanMoveGridView))]
    internal static class CanMoveGridViewPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(IGridObjectView gridObjectView, ref bool __result)
        {
            if (__result)
            {
                return;
            }

            if (!MoveGate.Armed)
            {
                return;
            }

            if (!MoveGate.IsMovablePlant(gridObjectView))
            {
                // Armed and the game asked about something - just not a plant we handle. Worth
                // one line, because "the gate is never even asked about a plant" and "the gate
                // says no" are different bugs and otherwise look identical from the outside.
                Diagnostics.Considered(gridObjectView);
                return;
            }

            // The game's own second half of the test. Only GridObjectConstructionView ever adds
            // a grid-object movement blocker, so this is effectively always free for a plant -
            // but reading it keeps buildings-under-construction behaving as the game intends if
            // one ever grows.
            if (!gridObjectView.IsAddedToWorldGrid)
            {
                return;
            }

            var blocker = gridObjectView.MovementBlocker;
            if (blocker != null && !blocker.IsFree)
            {
                Plugin.Log.LogInfo(
                    $"Plant at {gridObjectView.Position} is movement-blocked; leaving it alone.");
                return;
            }

            __result = true;
            Diagnostics.Allowed(gridObjectView);
        }
    }

    /// <summary>
    /// Puts a cancelled plant back where it came from.
    ///
    /// ObjectPickupAction.Cancel restores position only when the object reads as Movable, which
    /// a plant never does on its own. Without this, pressing Esc mid-move runs straight into
    /// PlaceGridObjectView() and drops the plant wherever the cursor is - the player changes
    /// their mind and loses the crop anyway.
    ///
    /// Restoring in a prefix means the original method's own restore, if it fires at all, just
    /// writes the same values again.
    /// </summary>
    [HarmonyPatch(typeof(ObjectPickupAction), "Cancel")]
    internal static class CancelPatch
    {
        [HarmonyPrefix]
        internal static void Prefix(
            IGridObjectView ___gridObjectView,
            Vector2Int ___identityPosition,
            int ___identityRotation)
        {
            if (!MoveGate.IsMovablePlant(___gridObjectView))
            {
                return;
            }

            ___gridObjectView.SetRotation(___identityRotation);
            ___gridObjectView.SetPosition(___identityPosition);

            if (Plugin.VerboseLogging.Value)
            {
                Plugin.Log.LogInfo($"Cancelled - returned plant to {___identityPosition}.");
            }
        }
    }

    /// <summary>
    /// Refuses to put a plant down on a cell with nothing waterable on it. See SoilCheck for
    /// why this is the mod rather than a refinement of it.
    ///
    /// This is the outermost placement validation and the one DecorateMoveObjectState actually
    /// calls, so vetoing here turns the cursor red and blocks the click through the game's own
    /// path - no separate enforcement needed.
    /// </summary>
    [HarmonyPatch(typeof(GridObjectHelper), nameof(GridObjectHelper.IsPlacementAllowed), new[]
    {
        typeof(IGridObjectView),
        typeof(GridObjectPlacementCache),
        typeof(Func<IGridObjectView, IGridObjectView[], GridObjectPlacementCache, bool, ItemBundleAsset, bool>),
        typeof(Vector2Int?),
        typeof(Func<Vector3Int, Vector2Int, bool>),
        typeof(ItemBundleAsset)
    })]
    internal static class PlacementPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(
            IGridObjectView gridObjectView,
            Vector2Int? positionOffset,
            ref bool __result)
        {
            // Only ever narrows placement for a plant this mod put in the player's hand. A seed
            // planted the normal way is not affected.
            if (!MoveGate.Carrying || !MoveGate.IsMovablePlant(gridObjectView))
            {
                return;
            }

            var cell = gridObjectView.Position + (positionOffset ?? Vector2Int.zero);

            if (!__result)
            {
                // The game refused before we were consulted. Worth a line of its own: "the mod
                // will not let me put this down" and "the game will not let me put this down"
                // are indistinguishable from the player's side and need different fixes.
                Diagnostics.PlacementRefusedByGame(cell, SoilCheck.HasWaterableAt(cell));
                return;
            }

            if (!Plugin.RequireSoil.Value)
            {
                return;
            }

            if (SoilCheck.HasWaterableAt(cell))
            {
                Diagnostics.PlacementAllowed(cell);
                return;
            }

            __result = false;
            SoilCheck.VetoedOnFrame = Time.frameCount;
            Diagnostics.PlacementRefusedByMod(cell);
        }
    }

    /// <summary>
    /// Says why, when the refusal above is the reason a placement failed.
    ///
    /// The game calls this exactly when the player pressed place and placement was disallowed -
    /// never per frame - so it is the right moment to explain, and it cannot spam.
    /// </summary>
    [HarmonyPatch(typeof(BaseDecorateStateMachineContext), nameof(BaseDecorateStateMachineContext.ShoutIfPlacementIsInValid))]
    internal static class ShoutPatch
    {
        [HarmonyPostfix]
        internal static void Postfix()
        {
            if (SoilCheck.VetoedOnFrame != Time.frameCount)
            {
                return;
            }

            var playerView = MonoBehaviourSingleton<PlayerView>.Instance;
            if (playerView == null || playerView.Shouter == null)
            {
                return;
            }

            playerView.Shouter.Shout(Plugin.NeedsSoilMessage.Value, clampToScreen: true);
        }
    }
}
