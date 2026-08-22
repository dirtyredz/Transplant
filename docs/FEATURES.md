# FEATURES — Transplant

What the mod does today. Status: ✅ shipped · 🧪 shipped-but-unverified · 🔜 planned.

## Moving plants

- ✅ **Pick up & move planted crops in decorate mode** — growables the player planted become
  selectable and movable like any decoration.
- ✅ **Growth preserved across a move** — grow stage, planted day and harvest count are untouched
  (verified in game 2026-08-04, incl. multi-tile grape trellises).
- 🧪 **Survives save + reload** — rests on the plant keeping its GUID while only position is
  rewritten; the reload path is **not yet tested** (gates the mod-page "keeps its growth" claim —
  see [BACKLOG.md](BACKLOG.md), [../TESTING.md](../TESTING.md)).
- ✅ **Wild plants opt-in** (`IncludeWildPlants`) — trees, bushes, weeds move too; off by default,
  and exempt from the soil rule.
- ✅ **Cancel returns the plant** — Esc mid-move restores the original tile *and* rotation.

## Safety

- ✅ **Soil rule** (`RequireSoil`, on by default) — refuses placement on any cell with nothing
  waterable, preventing the silent permanent grow-stall; cursor reads invalid and the click is
  blocked through the game's own path.
- ✅ **Refusal shout** — the character says a configurable line
  (`NeedsSoilMessage`, default "It needs watered ground.") only on the frame a mod veto fires.
- ✅ **Escape hatch** — `RequireSoil = false` allows bare-ground placement (config description warns
  what it costs).

## Controls

- ✅ **No-arm default** — plants movable immediately in decorate mode.
- ✅ **Optional arming key** (`RequireModifier`) — default **X**; press-to-toggle (`PressToToggle`)
  or hold. Robust to held movement keys.

## Configuration / integration

- ✅ **Full BepInEx config** + **Mod Menu** section/label tags (Moving plants · Safety ·
  Diagnostics).
- ✅ **Opt-in verbose diagnostics** (`VerboseLogging`) — rate-limited logging that distinguishes the
  "does nothing" failure modes.

## Non-goals

- ❌ Not a cheat — no skipped growth days, changed yields, or duplication.
- ❌ No new save data or persistence type.
- ❌ Herb-garden pots / stacked sub-grid objects — a different state machine, deliberately out of
  scope.

_Living doc — refresh with /project-docs when it drifts._
