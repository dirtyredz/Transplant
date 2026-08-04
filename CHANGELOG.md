# Changelog

## 0.1.4 — 2026-08-04

**No key is needed any more.** `RequireModifier` now defaults to false, so plants are movable
whenever decorate mode is open.

The arming key has been the cause of two consecutive failures — first a hold-vs-tap race, then
a toggle that made things worse — and it was never part of the request. It was a safety feature
this mod added on its own. Taking it off the critical path removes the whole class of problem
at once: no key conflict, no timing window, and no dependence on the key reaching the mod at
all. `RequireModifier = true` still restores it for anyone who wants the guard.

- **Added a probe on `BaseDecorateState.OnActivate`.** Every decorate state derives from it, so
  the log now names each one as it runs. The 0.1.3 session produced no decorate lines
  whatsoever, which left "the player never opened decorate mode" and "these are not the states
  this game uses" impossible to tell apart.
- The first decorate line now also reports the arming configuration, so a log is
  self-describing without anyone having to go and read the config file.

## 0.1.3 — 2026-08-04

Selection is confirmed working — the log shows a grape vine found, gated and marked movable.
Two things were still wrong.

- **The arming key is now a toggle: press once to arm, press again to disarm.** The first three
  builds required the key to be *held*, which created a race the player had to win: press and
  release, and the selection recomputes as unselectable the moment the key comes up, so the
  pickup only lands if the click happens inside that window. The original request was "hitting
  X", and that is now what it does. `PressToToggle` can be turned off to get the old
  hold behaviour back.
- **Added diagnostics that say who refused a placement.** A picked-up vine could not be put down
  anywhere, and nothing shipped so far distinguished the mod's soil veto from the game's own
  placement rules — two different fixes behind one symptom.

Placement is not fixed, only made legible. The vine is four cells tall, which is the likely
complication.

## 0.1.2 — 2026-08-04

The 0.1.1 log showed the machinery working — decorate mode detected, key detected, gate
consulted — while a grape vine still could not be selected. Two separate problems.

- **Wild plants were not being excluded.** `Item_Wild_Vegetation_Weeds_5` was offered as
  movable with `IncludeWildPlants` off, because the filter tested
  `ItemAsset.VegetationAddon` and weeds are plain growables with no vegetation addon. Now uses
  `GrowablePersistence.IsPlayerPlanted`, the flag the game itself keeps.
- **Added probes above our own gate.** The vine produced no log line at all, meaning the game's
  selection gave up before consulting us — and nothing shipped so far could see that far up.
  Two postfixes now report what the game finds in the column under the cursor, and the game's
  own verdict on each growable including the two checks it makes before reaching ours.

Still not confirmed working. This release is aimed at making the vine's failure legible.

## 0.1.1 — 2026-08-04

Fixes the reason 0.1.0 appeared to do nothing at all.

- **The arming key now takes effect the moment you press it.**
  `DecorateSelectState.ProcessSelection` returns immediately unless the cursor moved that frame,
  so holding X while the mouse was still never re-ran the selection and the gate was never
  consulted. It only worked if you happened to move the mouse while holding the key. Arming now
  forces the game to recompute its selection on the frame the state flips.
- **Decorate mode is detected from two independent signals**, `DecorateSelectState` as well as
  `PlayerDecorateStateMachine`. The first build relied on one and gave no way to tell from the
  outside whether it had fired.
- **Added diagnostics that distinguish the three "it does nothing" failures** — decorate mode
  not registering, the key not registering, and the gate declining — so a bad run points at a
  cause instead of needing another guess. Rate-limited and on by default, because the run where
  they matter is the one where nobody knew to enable verbose logging.

## 0.1.0 — 2026-08-04

First build. **Compiles and deploys; never run in game.** See [TESTING.md](TESTING.md).

- Hold **X** in decorate mode to make planted crops selectable, then move them like any other
  decoration. Growth stage, planted day, harvest count and drank/fed logs all travel with the
  plant, because the move keeps its GUID and only rewrites its position.
- **Refuses to place a plant where it could not be watered.** Watering is keyed to the cell,
  not the plant, so a crop left on bare ground would stop growing permanently while still
  looking healthy. The cursor reads invalid and the character says so.
- **Esc returns a plant to where it started.** Vanilla skips its restore for anything not
  flagged movable, which would otherwise drop the crop under the cursor.
- Herb-garden pots and wild weeds are out of scope; weeds can be opted in.
- Settings exposed through `Config.Bind`, so Mod Menu and ConfigurationManager pick them up.

Writes nothing new to the save.
