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
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDeactivate")]
        internal static void AfterDeactivate()
        {
            MoveGate.ExitDecorate();
            SoilCheck.Reset();
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
                return;
            }

            __result = true;
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
            if (!__result)
            {
                return;
            }

            // Only ever narrows placement for a plant this mod put in the player's hand. A seed
            // planted the normal way is not affected.
            if (!MoveGate.Carrying || !Plugin.RequireSoil.Value)
            {
                return;
            }

            if (!MoveGate.IsMovablePlant(gridObjectView))
            {
                return;
            }

            var cell = gridObjectView.Position + (positionOffset ?? Vector2Int.zero);
            if (SoilCheck.HasWaterableAt(cell))
            {
                return;
            }

            __result = false;
            SoilCheck.VetoedOnFrame = Time.frameCount;
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
