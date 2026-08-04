# Changelog

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
