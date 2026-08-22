# ARCHITECTURE — Transplant

How the system works at runtime. [../STRUCTURE.md](../STRUCTURE.md) maps the code; this maps the
design.

## System overview

Transplant is a single BepInEx plugin assembly (`Transplant.dll`) loaded into *Moonlight Peaks*.
On `Awake` it applies seven Harmony patch sets and binds its config. It adds **no new data and no
new persistence type**: it only enables and constrains the game's existing decorate-mode move on a
class of object (planted growables) the game normally refuses to move.

The whole mod turns on one game fact: **a plant's watering is keyed to the cell it stands on, not
to the plant.** `WaterGrowStageRequirement` resolves the plant's waterable by matching positions in
`CurrentRoom`. Move a crop onto a cell with no waterable and its water requirement can never
complete again — it silently stops growing while looking healthy. So the mod's core job is not "let
plants move"; it is **refuse to place a growable on a cell with nothing waterable on it.**

## The two gates

1. **Pickup gate** — `BaseDecorateStateMachineContext.CanMoveGridView` is the one method on the
   selection path that reads `GridControlType.Movable`. A postfix flips `false → true` for a plant
   the mod handles, *only while armed*. It patches the base method deliberately: the in-game "Move
   Grid Object" spell overrides `CanMoveGridView` without calling base, so it keeps its own rules.

2. **Placement gate** — `GridObjectHelper.IsPlacementAllowed` is the outermost placement validation
   and the one `DecorateMoveObjectState` actually calls. A postfix vetoes here, which turns the
   cursor red and blocks the click through the game's own path — no separate enforcement needed.
   `ShoutIfPlacementIsInValid` then explains the veto, but only on the exact frame the veto fired.

## Data model

No persisted data of its own. It reads game state through:

- **`GamePersistence.Instance.CurrentRoom`** — `Growables` (for `IsPlayerPlanted`), `Waterables`
  and `GridObjects`/`GetGridObjectPersistences(cell)` (for the soil lookup).
- **`ItemAsset.GrowableAddon.GrowStageContainer`** — walked (never *evaluated*) to decide whether a
  plant kind can ever be gated on watering.
- **Config** (`BepInEx` `.cfg`, also surfaced in Mod Menu via `ConfigDescription` tags) — the only
  mutable state the mod owns, plus in-memory arming/carrying flags in `MoveGate`.

## Key flows / sequences

- **Enter decorate** → `DecorateStatePatches.AfterActivate` + `SelectStatePatches.AfterActivate`
  set `MoveGate` active; `SoilCheck`/`Diagnostics` reset per-frame caches.
- **Arm** (only if `RequireModifier`) → `SelectStatePatches.BeforeUpdate` polls the toggle key and,
  when arming flips, sets `___forceUpdate = true` so the game re-runs its selection (which otherwise
  short-circuits unless the cursor moved that frame).
- **Pick up** → pickup gate allows → `AfterPickUp` latches `Carrying` (so releasing the arming key
  mid-carry can't strand the plant on cancel).
- **Place** → placement gate runs `SoilCheck`; allow if the plant needs no water, or the cell has a
  waterable; else veto + shout.
- **Cancel** → `CancelPatch` prefix restores original position + rotation.
- **Exit decorate** → state cleared, caches reset.

## External interfaces

- **BepInEx 5** host + config system; **HarmonyX** for patching.
- **Mod Menu** (optional) reads `ConfigDescription` tags (`ModMenu.Section=…`, `ModMenu.Label=…`)
  to render settings rows — display-only; the `.cfg` section/key names are never changed.
- **Game assemblies** referenced but never shipped (`Private=false`): `Vampire.Runtime`,
  `littlechickengamecompany.chicken-utilities.runtime`, UnityEngine modules.
- **Build/release**: `pack.ps1` → `dist/Transplant-<version>.zip` in Nexus layout; publish via the
  workspace nexus-publish skill. Full chain in [../../../docs/ARCHITECTURE.md](../../../docs/ARCHITECTURE.md).

## Design notes

- **Read-only game access everywhere.** `Find()` not `FindOrCreate()`; the grow graph is enumerated
  but never evaluated (evaluation consumes Unity's global RNG — see [GOTCHAS.md](GOTCHAS.md)).
- **Fail-open toward growth-safety, closed toward wrong refusals.** Unknown grow shapes are assumed
  to need water (the failure prevented is silent + permanent; a wrong refusal is merely annoying).
- **Diagnostics are opt-in.** Silent unless `VerboseLogging`; rate-limited so per-frame validation
  can't flood the log.

_Living doc — refresh with /project-docs when it drifts._
