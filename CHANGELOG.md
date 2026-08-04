# Changelog

## 1.0.0 — 2026-08-04

First release. Move a crop you have already planted, in decorate mode, without losing its
growth.

- **Planted crops are movable in decorate mode.** Pick one up and put it down like any other
  decoration. Grow stage, planted day, harvest count and the drank/fed logs all travel with it,
  because the move keeps the plant's GUID and only rewrites its position.
- **It will not let you strand a plant.** Watering is stored on the tile rather than on the
  plant, so a crop left on bare ground would stop growing permanently while still looking
  healthy. Placement is refused where the plant could never be watered, and your character says
  why.
- **Esc returns a plant to exactly where it started.** The game skips its own restore for
  anything not flagged movable, which would otherwise drop the crop under the cursor.
- **Wild trees, bushes and weeds are excluded by default**, via the game's own `IsPlayerPlanted`
  flag. `IncludeWildPlants` turns them on; the soil rule correctly does not apply to plants that
  are never watered.
- Herb-garden pots are out of scope — they sit on a sub-grid and run through a different state
  machine.
- Settings exposed through `Config.Bind`, so Mod Menu and ConfigurationManager pick them up.

Writes nothing new to your save.

### Folded in from development

Kept because the reasoning is worth having, per
[12-versioning-and-release.md](../../12-versioning-and-release.md) — none of these were
published.

- **The arming key was a mistake, and it cost three builds.** The mod originally required a key
  to be *held* before plants became selectable, a safety guard that was never asked for. Holding
  lost a race against the game's own selection refresh; switching to a toggle made it worse. It
  is now off by default, and plants are simply movable whenever decorate mode is open.
- **`DecorateSelectState.ProcessSelection` only recomputes when the cursor moves.** The first
  build looked completely inert for this reason alone — the gate was never consulted. Arming now
  forces a recompute on the frame it changes.
- **`ItemAsset.VegetationAddon` does not identify wild plants.** Weeds are plain growables with
  no vegetation addon, and sailed straight through the filter.
  `GrowablePersistence.IsPlayerPlanted` is the flag the game itself uses.
- **The gate is patched on `CanMoveGridView`, not `GridObjectItemAddon.ControlTypes`.** The
  research preferred the latter, but a trivial auto-property getter is an unreliable Harmony
  target — anything the Mono JIT can inline may keep the old answer at call sites already
  compiled.
- **The soil rule applies only to plants with a `WaterGrowStageRequirement`.** Applying it to
  everything would have made wild trees pickable but impossible to put down anywhere.
