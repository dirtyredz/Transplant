# Moving Plants in Decorate Mode

Decompiled from `Vampire.Runtime.dll` on 2026-08-04, before writing any code. Regenerate with
the commands in [09-exploring-the-assembly.md](../../../09-exploring-the-assembly.md).

## Verdict on the five-minute feasibility check

**Weekend, not a slog — but the easy half is a trap.**

Making a plant pickable in decorate mode is *one boolean*. Making the plant still grow
afterwards is the actual mod, and it is the reason this doesn't already exist.

## Part 1 — the gate is a single virtual method

Decorate mode's whole "can I grab this?" decision is here:

```csharp
// BaseDecorateStateMachineContext
public virtual bool CanMoveGridView(IGridObjectView gridObjectView)
{
    if (SROptionsVampire.Current.CreativeMode) return true;
    if (gridObjectView.GridObjectAddon.ControlTypes.HasFlag(GridControlType.Movable))
        return gridObjectView.MovementBlocker.IsFree;
    return false;
}
```

Crops fail it because their `ItemAsset` doesn't carry the `Movable` flag. Nothing else stops
them — I checked:

- **`MovementBlocker` is irrelevant here.** Exactly one thing in the entire assembly ever adds
  a grid-object movement blocker: `GridObjectConstructionView` (buildings mid-construction).
  Plants are always `IsFree`.
- **`GridControlType.Movable` is read in exactly three places** — `CanMoveGridView`,
  `DecorateCursor` (line ~320), and `ObjectPickupAction.Cancel`. That is the complete blast
  radius of flipping it. Verified with a grep over the full decompile.
- **The selection path already handles plants.** `DecorateSelectState.ProcessSelection` falls
  back to `GetLowestMovableObjectAtColumn`, a grid-cell lookup filtered only by
  `CanMoveGridObject` → `CanMoveGridView`. So the mod does not need to build hover, raycasting
  or cursor logic — unlike Chest Labels and Plant Peek, which each had to.
- **Placement will pass.** A plant is `GridObjectLayer.Object` (8); a farm tile is `Pathway`
  (2). `IsPlacementOnLayerAllowed` is `(layerToPlace & existingLayer) == 0` → `8 & 2 == 0` →
  allowed. Plants and soil legitimately coexist on one cell.

**And identity survives the move.** `PickUpGridObjectView` → `RemoveFromGrid()`, then
`PlaceGridObjectView` → `AddToGrid()` on the *same view instance*. `SetPosition` only writes
`GridObjectPersistence.Position`. The GUID never changes, nothing is destroyed and re-created:

```csharp
public void SetPosition(Vector2Int position)
{
    GridObjectPersistence.Position = position;   // identity untouched
    ...
}
```

## Part 2 — what a move actually costs the plant

This is the part that matters, and it splits cleanly in two.

### GUID-keyed — travels with the plant, safe

| Data | Where | Effect of a move |
|---|---|---|
| `GrowablePersistence` | `CurrentRoom.Growables`, by GUID | none — stage, `DayPlanted`, `DayGrowStageChanged`, `TimesHarvested` all intact |
| `DrinkingPlantPersistence` | by GUID | none |
| `FeedablePlantPersistence` | by GUID | none |
| `DamagePersistence` | by GUID | none |

### Position-keyed — re-evaluated at the destination

| Requirement | What it reads | Effect of a move |
|---|---|---|
| **`WaterGrowStageRequirement`** | **another grid object at the same position** | **⚠️ see below** |
| `NearWaterGrowStageRequirement` | `TileHasState(position, Close_To_Water)` | recomputed — correct behaviour |
| `CropsNearGrowStageRequirement` | neighbouring growables in range | recomputed — correct behaviour |
| `FootprintGrowStageRequirement` | destination cells must be empty except self | recomputed — correct behaviour |
| `SeasonGrowStageRequirement` | the calendar | unaffected |

Three of those four re-evaluating is *desirable* — a plant moved next to water genuinely is
near water now. Water is the one that bites.

### ⚠️ The trap: watering lives on the soil, not on the plant

```csharp
// WaterGrowStageRequirement
private WaterablePersistence GetWaterablePersistence(GridObjectPersistence g, GrowablePersistence p)
{
    foreach (GridObjectPersistence gridObject in GamePersistence.Instance.CurrentRoom.GridObjects)
    {
        if (!(gridObject.Position == g.Position)) continue;      // ← position, not GUID
        foreach (WaterablePersistence w in GamePersistence.Instance.CurrentRoom.Waterables)
            if (w.Guid == gridObject.Guid) return w;
        ...
    }
    return null;                                                 // ← nothing there
}
```

It scans for *a different grid object sharing the plant's cell* — the farm tile — and reads
that tile's watered log. Confirmed by `FarmTileView`, which holds a `WaterableView` component
and creates a `WaterablePersistence` for every tile on `Load`.

So:

1. **Move a plant onto bare ground and it stops growing. Forever.** `GetWaterablePersistence`
   returns null → `IsRequirementCompleted` returns false → the plant sits at that stage
   permanently, looking perfectly healthy. Nothing warns the player, nothing logs, and the
   damage is only visible days later. **This is the single thing the mod must prevent.**
2. Move a watered plant to a dry tile → it reads as unwatered; re-water it.
3. Move an unwatered plant to a watered tile → it counts as watered today.

(2) and (3) are arguably *right* — water belongs to soil. (1) is a silent save-damaging bug.

**The rule that falls out of this: refuse to place a growable on a cell that has no
waterable.** That is not a nice-to-have, it is the mod's core correctness constraint, and it
is cheap — the check is the same scan the game does, reproduced against the destination cell.

### The corroborating evidence

[Serena's Conjuring](https://www.nexusmods.com/moonlightpeaks/mods/62) (v1.8.6, 1,029 unique
downloads) already unlocks moving *buildings* in decorate mode. Its mod page says, verbatim:

> Crops, tilled soil, paths and other location-dependent farming objects are deliberately
> excluded so their watering and growth records remain safe.

**The most-downloaded decorate mod in the scene hit this exact wall and walked away from it.**
That is the strongest possible confirmation that (a) the gap is real, and (b) it is unclaimed
because it is hard, not because nobody wanted it. Solving it *is* the mod.

## Part 3 — the second trap: Esc doesn't put the plant back

```csharp
// ObjectPickupAction.Cancel
if (gridObjectView.GridObjectAddon.ControlTypes.HasFlag(GridControlType.Movable)
    || SROptionsVampire.Current.CreativeMode
    || context.ShouldResetPositionOnCancelPlacement)
{
    gridObjectView.SetPosition(identityPosition);
}
stateMachine.PlaceGridObjectView();          // ← runs either way
```

Patch `CanMoveGridView` alone and this branch is skipped, because the plant's asset still
lacks `Movable` — so `PlaceGridObjectView()` drops the plant **wherever the cursor happens to
be**. The player picks up a crop, changes their mind, presses Esc, and the crop lands
somewhere random. Same story for `SetRotation`, which needs `Rotatable`.

This is why the cleaner patch target is `GridObjectItemAddon.get_ControlTypes` (postfix, OR in
`Movable` when `__instance.Item.GrowableAddon != null`) rather than `CanMoveGridView`. One
patch, and all three call sites — cursor, selection, and cancel-restore — agree. The back
reference exists: `BaseItemAddonAsset.Item` is public.

Trade-off: `get_ControlTypes` is hot (per raycast hit, per frame). Gate it on a static
"decorate mode is active and the modifier is held" bool so the added work is a bool test
outside decorate mode.

## Part 4 — smaller findings

- **Weeds.** `FarmTileView.ProcessDay` spawns wild vegetation on a tile only when
  `GetGridObjects(position).Length <= 1`. Move a plant off a tile and that tile starts its
  2–4 day weed timer; move one onto an empty tile and the timer is suppressed. Vanilla
  behaviour, no bug — but worth one line on the mod page.
- **Watering days before the current stage don't count.** `WaterGrowStageRequirement` discards
  logged days earlier than `growablePersistence.DayGrowStageChanged`.
- **The input action is `Toggle_Pickup = 34`**, `friendlyName = "Pick Up Place"`, category
  `Decoration` (`RewiredConsts.Actions`). Rotate is 28/29, put-in-inventory 42, change-shape
  108, floor-mode toggle 164. **The default keyboard binding is not in the DLL** — Rewired
  keeps bindings in its input-manager asset — so whether that is literally `X` on this machine
  is a one-launch check against the game's control settings. It doesn't change the design:
  patching the gate means whatever key already picks things up starts working on plants.
- **`Free Decorate` does not overlap.** It patches placement *validation*
  (`CanPlaceGridObject`) and grid snap. It never touches `CanMoveGridView`, so it does not
  make plants selectable. The two mods are compatible and orthogonal — though note that Free
  Decorate forcing validation to always return true would also bypass a "must land on soil"
  placement check, so the soil rule must be enforced on the *pickup/place* path, not by
  relying on placement validity.

## Design that falls out of all this

- **Arm it explicitly.** Plants become movable only while a configured modifier is held
  (default: the key the user already associates with this, config-bound as a
  `KeyboardShortcut` so Mod Menu renders a picker). Without a modifier, every decorate session
  starts grabbing crops the player was trying to decorate around.
  ⚠️ Use the raw `Input.GetKey` check from
  [PlantPeek/Hotkey.cs](../../PlantPeek/src/PlantPeek/Hotkey.cs), **not**
  `KeyboardShortcut.IsPressed()` — see the note in
  [08-mod-ideas.md](../../../08-mod-ideas.md) about it being false while any other key is held.
- **Refuse placement on cells with no waterable.** Non-negotiable, per Part 2. Reuse
  `ShoutIfPlacementIsInValid` / `shout-cannot-place` so the refusal looks native.
- **Fix cancel.** Per Part 3, or the mod loses people's crops.
- **Write nothing new to the save.** The mod only lets the game's own `SetPosition` run on an
  object it already owns. No sidecar file, no new persistence type. That makes **save-safe**
  honestly claimable, and uninstalling leaves plants wherever they were last placed — which is
  a legal game state, indistinguishable from having planted them there.
- **Comfort, not a cheat.** It moves a plant; it does not skip a growth day, change a yield or
  duplicate anything. Say so on the page.

## Open questions, all answerable in one launch

1. **Do any growables carry their own `WaterableView`?** If a plant is its own waterable,
   `GetWaterablePersistence` may return the plant rather than the tile, and the soil rule needs
   to exclude self. `WaterableView.IsOnFarmTile` is public and is probably the discriminator.
2. **Herb-garden pots.** `HerbFarmTileView` and `HerbGardenPersistence.HerbFarmTile` are a
   second kind of soil, on a sub-grid. Decide whether pots are in scope; the sub-grid path
   goes through `DecorateSelectStackedState`, which is a different state and not covered above.
3. **Trees and multi-cell growables.** `GrowableView.SetFootprint` overrides the footprint per
   grow stage, so a grown tree occupies more cells than its sapling. Confirm the soil rule is
   applied per occupied cell, not just the origin.
4. **What the default `Pick Up Place` key actually is on keyboard.**
