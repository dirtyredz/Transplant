# GOTCHAS — Transplant

Non-obvious traps in this mod and its game. Each: **trap → why → do instead.**

- **Watering is keyed to the cell, not the plant.** → `WaterGrowStageRequirement` matches the
  plant's position against `CurrentRoom.Waterables`; move a crop onto bare ground and its water
  requirement can never complete — it stops growing silently and permanently, still looking healthy.
  → Never place a growable without `SoilCheck` confirming a waterable on the target cell. This is
  the reason the mod exists.

- **Don't patch `GridObjectItemAddon.ControlTypes`.** → It's a trivial auto-property getter; Mono
  may inline it, and call sites inlined before the patch applies keep the old value. → Patch
  `CanMoveGridView` (real body, can't inline away) and pay the one extra `ObjectPickupAction.Cancel`
  patch. See [DECISIONS.md](DECISIONS.md).

- **Never *evaluate* the grow graph off the hover/placement path.** →
  `RandomChanceGrowStageRequirement.IsRequirementCompleted` consumes Unity's global RNG, so calling
  `GrowPath.CheckIfRequirementsMet` / `GrowStageContainer.GetDesiredGrowPath` would perturb game
  randomness. → Only *enumerate* components (`GetComponents<IGrowStageRequirement>()`), as
  `SoilCheck.ScanForWaterRequirement` does. Same rule Plant Peek follows.

- **`FindOrCreate()` writes state.** → Using it to probe a growable/waterable would persist a record
  for anything the cursor passed over. → Use `Find()` (returns null when absent) everywhere the mod
  reads game persistence — `MoveGate.IsPlayerPlanted`, `SoilCheck`.

- **`IsPlayerPlanted` is not `VegetationAddon != null`.** → Weeds are plain growables with no
  vegetation addon and pass that test. → Read `Growables.Find(guid).IsPlayerPlanted`, the flag the
  game itself uses.

- **Arming while the mouse is still does nothing.** → `DecorateSelectState.ProcessSelection` returns
  early unless the cursor moved this frame or `forceUpdate` is set, so the pickup gate is never
  consulted. → When arming flips, set `___forceUpdate = true` (`SelectStatePatches.BeforeUpdate` +
  `MoveGate.ConsumeArmedChanged`).

- **Releasing the arming key mid-carry can strand the plant.** → `ObjectPickupAction.Cancel`
  restores position only for objects movable *at the moment Esc is pressed*. → A pickup latches
  `MoveGate.Carrying`, which forces `Armed` true until placed or cancelled.

- **`KeyboardShortcut.IsPressed()/IsDown()` fail when another key is held.** → They end in a check
  that returns false whenever any other supported key is down — e.g. a movement key while walking a
  crop row. → Use `Hotkey.IsHeld` / `Hotkey.WasPressed`.

- **Herb-garden pots use a different state machine.** → Stacked sub-grid objects go through
  `DecorateSelectStackedState`, not the state this mod patches. → `MoveGate.IsMovablePlant` excludes
  anything with a `ParentSubGridSurface`; don't "fix" that without testing the stacked path.

- **`Hotkey.cs` is a deliberate copy of Plant Peek's.** → Not shared code. → Keep the two in sync by
  hand; they ship as independent DLLs with no common dependency.

- **`pack.ps1` and `Directory.Build.props` are workspace canonicals.** → Editing them here is
  clobbered by the next sync. → Change `../../tools/*` and re-run `../../tools/sync-mod-files.ps1`.

- **Placement is validated several times per frame** (once per footprint cell). → An unthrottled
  scan or log line runs multiple times per frame. → `SoilCheck.HasWaterableAt` caches per
  (frame, cell); `Diagnostics` rate-limits.

_Living doc — refresh with /project-docs when it drifts._
