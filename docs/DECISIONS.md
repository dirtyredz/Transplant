# DECISIONS — Transplant

Design decisions worth not re-litigating, newest first. Rationale drawn from the code, the
[README](../README.md), [research/](../research/) and git history.

## 2026-08-04 — Patch `CanMoveGridView`, not `GridObjectItemAddon.ControlTypes`

The research first preferred `ControlTypes` because it would fix cancel for free.
**Why not:** `ControlTypes` is a trivial auto-property getter; a getter small enough for the Mono
JIT to inline is a poor Harmony target — call sites already inlined before the patch applies keep
the old answer. `CanMoveGridView` has a real body and cannot be inlined away.
**Cost accepted:** one extra patch on `ObjectPickupAction.Cancel` to restore a cancelled plant.
**Rejected:** patching `ControlTypes`; patching the derived spell context (would change the in-game
Move Grid Object spell).

## 2026-08-04 — The soil rule *is* the mod; enforce it at `IsPlacementAllowed`

The core risk is a plant stranded where watering can never resume (silent, permanent). The rule is
therefore "refuse to place a growable on a cell with nothing waterable," enforced at the outermost
placement validation so the game's own path turns the cursor red and blocks the click.
**Why there:** `GridObjectHelper.IsPlacementAllowed` is the method `DecorateMoveObjectState`
actually calls — one veto point covers cursor feedback and the click.
**Rejected:** a separate enforcement pass; asking "is there a farm tile here" (mirroring
`WaterGrowStageRequirement`'s exact position lookup is what makes self-watering growables and
multi-cell trees behave correctly).

## 2026-08-04 — Check the origin cell only, mirror the game's lookup exactly

`SoilCheck` reproduces `WaterGrowStageRequirement`'s position-keyed lookup rather than inventing a
tile test. **Why:** the game only ever looks at the origin cell; testing the whole footprint would
be stricter than vanilla and block legitimate multi-cell tree moves. A growable that carries its
own waterable then satisfies the check exactly as it would satisfy the game.

## 2026-08-04 — A pickup latches arming on until placed or cancelled

`ObjectPickupAction.Cancel` restores position only for objects that read as movable *at the moment
Esc is pressed*. **Why latch:** arming is normally tied to a held key; releasing it mid-carry would
make cancel drop the plant wherever the cursor was. So `MoveGate.Carrying` forces `Armed` true.

## 2026-08-04 — Arming key is press-to-toggle, and off by default

Earlier builds used hold-to-arm, which lost a race: press+release armed for a frame or two and the
selection recomputed as unselectable before the player could click. `RequireModifier` is **off** by
default — plants are simply movable in decorate mode — because the guard was the mod's own idea and
caused two failed releases. **Rejected:** hold-to-arm as the default; requiring a key at all.

## 2026-08-04 — `IsPlayerPlanted` via `GrowablePersistence`, not `VegetationAddon`

The obvious `ItemAsset.VegetationAddon != null` test is wrong — weeds are plain growables with no
vegetation addon and sailed through it. `CurrentRoom.Growables.Find(guid).IsPlayerPlanted` is the
flag the game itself uses (`WildTreeGrowStageRequirement` checks it). Wild plants are opt-in
(`IncludeWildPlants`) and exempt from the soil rule (nothing waterable sits under a wild tree).

## 2026-08-04 — Custom `Hotkey` helpers instead of `KeyboardShortcut.IsPressed/IsDown`

Those built-ins report false whenever *any* other supported key is held — wrong when the player
holds a movement key while walking a row of crops. `Hotkey.cs` checks the main key + declared
modifiers only. **Rejected:** a shared package — these mods ship as independent DLLs with no common
dependency, so the file is a deliberate copy of Plant Peek's (see [GOTCHAS.md](GOTCHAS.md)).

## (workspace) — Version single-sourced from csproj; flat `src/` layout

Version lives only in `src/Transplant.csproj` `<Version>`, compiled into `ModBuildInfo.Version`;
never hardcoded in `Plugin.cs`. Sources sit flat in `src/`. Both are workspace conventions — see
[../../../docs/DECISIONS.md](../../../docs/DECISIONS.md).

_Living doc — refresh with /project-docs when it drifts._
