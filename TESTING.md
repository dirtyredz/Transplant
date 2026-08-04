# Testing Transplant

**v0.1.0 has never been run.** It compiles, it deploys, and all thirteen Harmony targets were
checked to exist in the decompiled assembly — but no part of it has been exercised in game.
Everything below is unverified.

The default arming key is **X**, held, while in decorate mode.

## 0. It loads at all

Launch, open `BepInEx\LogOutput.log`, expect:

```
[Info: Transplant] Transplant 0.1.1 loaded. Nothing new is written to your save.
```

Any `HarmonyLib` exception near it means a patch target did not resolve.

## 0b. Read the log first if nothing happens

0.1.1 logs enough to tell the three "it does nothing" failures apart. Enter decorate mode,
hold **X** over a crop, then read the log:

| What you see | What it means |
|---|---|
| no `Selection state active` line | decorate mode is not being detected — the patches are on the wrong state |
| that line, but no `Armed` line | the key is not reaching the mod; try rebinding `Modifier` |
| `Armed`, then `considered '<name>': not a growable` | the mod is being asked about the wrong object — a selection-path problem, not a gate problem |
| `Armed`, then nothing at all | the gate is never consulted; selection is not reaching it |
| `'<name>' at (x, y) is now movable` | the gate said yes — if it still will not pick up, the problem is downstream |

That table is the point of this release; **quote whichever line you get** rather than only that
it did not work.

## 1. The arming key

Enter decorate mode and point at a growing crop.

| | Expected |
|---|---|
| Not holding X | Nothing changes. The crop cannot be selected — vanilla behaviour. |
| Holding X | The crop highlights and can be picked up. |

**If holding X does nothing**, the first thing to check is whether `X` is bound to something
else in decorate mode, and the second is whether the crop is being selected at all — the
selection path this mod relies on is a grid-column lookup keyed to the cell under the cursor,
not the plant's own collider, so aim at the **base** of a tall plant.

## 2. Growth survives the move — the whole point

The claim on the box is "without losing growth", so this is the test that matters.

1. Note a crop's stage (Plant Peek makes this easy — hold its detail key).
2. Move it to a different tilled tile.
3. Check the stage again: **unchanged**.
4. Sleep. It should advance exactly as it would have.

**Also confirm across a save/load**, since the whole design rests on the plant keeping its
GUID: move a crop, save, quit to menu, reload, and check the stage is still right and the
plant is where you left it.

## 3. The soil rule — the safety claim

1. Pick up a crop.
2. Move the cursor over untilled grass. The cursor should read **invalid**.
3. Click anyway. Expect the refusal shout: *"It needs watered ground."*
4. The plant must still be in hand, not on the grass.

**This is the test that protects people's saves.** If a plant can be placed on bare ground,
stop and turn `RequireSoil` off in the config as a temporary measure — a plant left there
stops growing permanently while still looking healthy.

## 4. Cancel puts it back

1. Pick up a crop, note its exact tile.
2. Move the cursor well away.
3. Press **Esc**.
4. The plant must return to its original tile and rotation — not land under the cursor.

Vanilla skips its own restore for anything not flagged movable, so this is a mod-specific
patch and a mod-specific way to lose a crop if it is wrong.

## 5. Watering follows the soil, not the plant

Expected, and worth confirming so the README's claim is honest:

- Move a **watered** plant to a **dry** tile → it now reads as unwatered; water it again.
- Move an **unwatered** plant to a **watered** tile → it counts as watered today.

Neither is a bug. Watering is stored on the tile.

## 6. Nothing else got looser

Regression checks, because the pickup gate is shared with the rest of decorate mode:

- Without holding X, furniture, paths and chests move exactly as before.
- The in-game **Move Grid Object spell** is unchanged. It overrides the patched method without
  calling base, so it should be untouched — confirm it still refuses what it always refused.
- Herb-garden pots are **out of scope** and should not become movable. They sit on a
  sub-grid and go through a different state machine.
- Wild weeds should not be movable unless `IncludeWildPlants` is turned on.

## 7. Save safety

Back up the save first, then: move several plants, sleep a night, save, and diff against the
backup. Expect changes only to grid-object **positions** — no new persistence records, no new
collections. The mod writes nothing of its own.

## Known gaps in v0.1.0

- **The refusal message is English only.** There is no localization key for "needs watered
  ground", so it is a config string rather than a `LocalizationLibrary.Translate` call.
- **Free Decorate compatibility is untested.** That mod forces placement validation to return
  true. Transplant vetoes in a postfix on the outermost validation, so it should still win, but
  the interaction has not been tried.
- **Plants cannot be put in your bag,** only moved. `Pickupable` is deliberately not granted.
