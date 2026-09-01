# STRUCTURE — Transplant

<!-- Last full review: 2026-08-22 -->

Where things live in the **Transplant** mod repo and how the code is shaped. Pairs with
[README.md](README.md) (human quick-start + design rationale) and the [docs/](docs/) set.
This is the code map; [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) is the system design.

## Overview

A BepInEx 5 / HarmonyX plugin for the Unity (Mono) game *Moonlight Peaks*. It makes a **planted,
growing crop pickable in decorate mode** and refuses to put it down anywhere its watering could
never resume. No new save data — the mod only lets the game's own `SetPosition` run on an object
the game already owns.

Target: `netstandard2.1`. Sources sit under `src/` in the workspace-wide mod taxonomy — `Plugin.cs`
at the `src/` root beside the `.csproj`, Harmony/live-game code in `src/game/`, the mod's own logic
in `src/core/` (this mod draws no UI, so there is no `src/ui/`). See [Layout](#layout). Version
is single-sourced from `src/Transplant.csproj` `<Version>` via `GenerateModBuildInfo` in
`Directory.Build.props`, surfaced to code as `ModBuildInfo.Version`.

## Layout

```
Transplant/
├── src/
│   ├── Plugin.cs          BepInEx entry point + config/logging — MUST stay beside the .csproj
│   ├── Transplant.csproj  netstandard2.1; **/*.cs globbing is recursive, so moves need no edit
│   ├── game/              Harmony patches and live-game bridges
│   │   ├── Patches.cs     the 7 [HarmonyPatch] adapter classes
│   │   ├── MoveGate.cs    movability guard the patches consult (reads live grid/persistence)
│   │   └── SoilCheck.cs   the water/soil placement rule, mirroring the game's own lookup
│   └── core/              the mod's own logic and diagnostics
│       ├── Hotkey.cs      key-state helpers for the arming binding
│       └── Diagnostics.cs opt-in verbose logging
├── scripts/               repo git-hook shell scripts
├── docs/                  the living-doc set (prose only)
├── research/              game-behaviour notes (prose only)
└── pack.ps1               workspace-synced build/pack script — must stay at the repo root
```

Folders are a filing convention only: **every type stays in the single flat `Transplant`
namespace**, because C# does not tie namespaces to directories and renaming them would churn every
`using` for no gain. Never add a folder-derived namespace here.

**Enforced homes:**

- `src/game/` — Harmony patches and live-game bridges
- `src/core/` — the mod's own logic, state, input and diagnostics
- `scripts/` — repo tooling shell scripts (git hooks)
- `src/Plugin.cs` — BepInEx entry point; must sit beside the `.csproj`
- `pack.ps1` — workspace-synced build/pack script; must sit at the repo root

There is deliberately no `src/ui/`: Transplant draws no panels or widgets, and the taxonomy says to
create a folder only when a file belongs in it.

## Architecture at a glance

Seven Harmony patches sit on the game's decorate-mode state machine and grid/placement helpers.
They route game events into three small, single-purpose static classes that hold the mod's own
state and decisions:

```
game decorate events ──▶ game/Patches.cs ──▶ game/MoveGate    (pick up / carry?)
                                         ├──▶ game/SoilCheck   (put down here?)
                                         └──▶ core/Diagnostics (explain it, if asked)
                          Plugin.cs      = entry point + config/logging
                          core/Hotkey.cs = key-state helpers for the arming binding
```

The full runtime shape and data flow are in [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Components

| Component | Responsibility | Key files | Exposes | Depends on | Seam |
|---|---|---|---|---|---|
| **Plugin entry** | Register patches at load; hold config + logging | `src/Plugin.cs` | `TransplantPlugin`, `Plugin` (static config/log) | BepInEx, HarmonyX | add a config entry / a patch class |
| **Patches** | Bridge game events → mod logic | `src/game/Patches.cs` | 7 `[HarmonyPatch]` classes | game types, MoveGate, SoilCheck, Diagnostics | add/adjust a hook onto a game method |
| **Move gate** | Arming/carrying state + "is this a plant we move?" | `src/game/MoveGate.cs` | `MoveGate` (static) | Plugin config, Hotkey, game persistence | change what counts as movable / arming rules |
| **Soil check** | The water/soil placement rule | `src/game/SoilCheck.cs` | `SoilCheck` (static) | game persistence, ItemAsset grow graph | change placement safety logic |
| **Diagnostics** | Opt-in verbose logging that tells failures apart | `src/core/Diagnostics.cs` | `Diagnostics` (static) | Plugin.Debug, MoveGate | add/trim a diagnostic line |
| **Hotkey** | Key-state helpers robust to held movement keys | `src/core/Hotkey.cs` | `Hotkey` (static) | Unity legacy Input | change key handling |

## Key flows

- **Pick up a plant** — decorate select state runs → `CanMoveGridViewPatch` consults
  `MoveGate.Armed` + `MoveGate.IsMovablePlant` → allows the pickup → `DecorateStatePatches.AfterPickUp`
  latches `MoveGate.Carrying`.
- **Put it down** — `PlacementPatch` (on `GridObjectHelper.IsPlacementAllowed`) asks
  `SoilCheck.NeedsWater` / `HasWaterableAt`; a veto turns the cursor red and blocks the click, and
  `ShoutPatch` explains it on the frame it happened.
- **Cancel (Esc)** — `CancelPatch` restores the original position/rotation, which vanilla skips for
  anything not flagged movable.
- **Arm/disarm** — optional; `SelectStatePatches` polls the toggle and forces the game to recompute
  its selection the frame arming changes (`MoveGate.ConsumeArmedChanged`).

## Conventions

- Code is filed per the [Layout](#layout) contract (`src/Plugin.cs` at the `src/` root, plus
  `src/game/` and `src/core/`) under one flat `Transplant` namespace; `pack.ps1` +
  `Directory.Build.props` at repo root are
  **workspace-synced canonicals** — do not hand-edit here (regenerated by `../../tools/sync-mod-files.ps1`).
- Version bumped in `src/Transplant.csproj` only, at publish time; never hardcode it in `Plugin.cs`.
- Commit identity: `dirtyredz <dirtyredz@live.com>`.
- Mod state is held in `static` classes (single plugin instance, no multiplayer) — deliberate,
  see [docs/DECISIONS.md](docs/DECISIONS.md).
- Read-before-you-code workspace gates: saves → `../../11`; colour → `../../16`; UI → `../../10`/`17`.

## Where to find things

- "How does the pickup gate work?" → `src/game/Patches.cs` (`CanMoveGridViewPatch`) +
  `src/game/MoveGate.cs`.
- "Why won't it place here?" → `src/game/SoilCheck.cs` + `PlacementPatch`/`ShoutPatch` in
  `src/game/Patches.cs`.
- "What do the config options do?" → `src/Plugin.cs` (`Plugin.Bind`) + [README.md](README.md).
- "What's still untested / next?" → [docs/BACKLOG.md](docs/BACKLOG.md), [TESTING.md](TESTING.md).
- "Why is it built this way?" → [docs/DECISIONS.md](docs/DECISIONS.md), [research/](research/).

## Structural debt

Assessed in the full structural review of **2026-08-22** (componentization + abstraction lenses +
Codex cross-model pass). This is a small (6-file, ~1.2k-line), **mostly cohesive** codebase — no
file is near the ~800-line God-class tripwire and 6 files map to 6 genuine responsibilities. (That
review predates the 2026-09-01 move out of a flat `src/` into `src/game/` + `src/core/`, which was a
pure relocation — no file, type or namespace changed.) All
findings are **P2** (minor, deferred on a stable published mod); full triage in
[docs/BACKLOG.md](docs/BACKLOG.md). Headlines:

- **`Plugin.cs` carries two responsibilities** — the `TransplantPlugin` BepInEx entry type *and*
  the `Plugin` static that owns all config binding + logging. Intentional-ish (config kept off the
  plugin type on purpose); the natural split into a `Config.cs` is only worth it if config grows. **P2.**
- **`Diagnostics` depends on decision logic** — it reads `MoveGate.Armed`/`IsPlayerPlanted` and
  partially *reproduces* `MoveGate.IsMovablePlant`'s branch set (`Considered`), so an eligibility
  change could make the explanation drift. One-directional and read-only today. **P2.**
- **Placement feedback leaks through `SoilCheck`** — `SoilCheck.VetoedOnFrame` is written by
  `PlacementPatch` and read by `ShoutPatch`, making `SoilCheck` a hidden channel between two Harmony
  adapters rather than a pure soil query. The frame stamp really belongs on `PlacementPatch`. **P2.**
- **Internal dedup pattern not reused** — `Diagnostics` re-hand-rolls its own "log once per key"
  guard in `Allowed`/`Verdict`/`Column` instead of routing through `Say`. **P2, diagnostics-only.**
- No leaky/over-abstraction of consequence; no wrong-direction deps *into* the patch layer; no
  duplication beyond the deliberate `Hotkey.cs` copy shared with Plant Peek and a two-site
  `GamePersistence` room guard (both documented — see [docs/DECISIONS.md](docs/DECISIONS.md)). The
  7-class `Patches.cs` is a cohesive Harmony adapter layer, not a God-file.

_Living doc — refresh with /project-docs when it drifts._
