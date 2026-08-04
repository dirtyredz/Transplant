# Changelog

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
