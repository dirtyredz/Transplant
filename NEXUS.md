# Nexus Mod Page — Transplant

Draft copy for the Nexus listing. Same shape as
[CoffinBreak/NEXUS.md](../CoffinBreak/NEXUS.md); read that one's notes on the upload form,
thumbnail ratio and art direction first — they all still apply.

---

## Fields

| Field | Value |
|---|---|
| **Name** | Transplant - Move Planted Crops Without Losing Growth |
| **Summary** (short, shows in listings) | Planted the row slightly wrong? Move it. Crops keep their growth, and they will not let you strand them somewhere they cannot be watered. |
| **Category** | Gameplay — where Free Decorate, Place Items Diagonally and Walk Through Crops all sit |
| **Version** | 1.0.0 |
| **Requirements** | BepInEx 5 (win_x64) — required |
| | Mod Menu — optional, for in-game settings |
| **Tags** | quality of life, gameplay, save-safe, farming, decorating |
| **Licence** | MIT (confirm before upload) |

**Keep "Move Planted Crops" in the title.** A keyword sweep on 2026-08-04 found `transplant`
and `root` both returning zero results for this game, and `plant` returning only Plant Peek,
Endless Harvest and Faster Planting. The searchable words are *move*, *planted* and *crops*.

---

## Full description — paste into Nexus

### Description

You plant a row of grapes, step back, and it is one tile off.

There is nothing you can do about it. Moonlight Peaks lets you move furniture, fences, paths and
whole buildings — but not a plant you have already planted. The only fix is to lose the crop and
start it again.

Transplant makes a growing plant behave like any other decoration. Pick it up in decorate mode,
put it somewhere better, and it carries on exactly where it left off — same grow stage, same
planted day, same harvest count.

It also refuses to let you ruin one. Watering in this game is recorded on the *tile*, not on the
plant, so a crop moved onto bare ground would quietly stop growing forever while still looking
perfectly healthy. Transplant will not place a plant anywhere it could never be watered, and
your character tells you why.

Nothing new is written to your save.

---

### Installation instructions

1. Install BepInEx 5 (win_x64) into your Moonlight Peaks folder, if you do not have it already.
   The BepInEx folder should sit beside Moonlight Peaks.exe.
2. Extract this mod into the same folder. It will land in BepInEx/plugins/Transplant.
3. Start the game.

To uninstall, delete the BepInEx/plugins/Transplant folder. Plants stay wherever you last put
them, which is an ordinary game state — nothing was ever written to your save.

---

### Main features

- Move planted crops in decorate mode, like any other decoration
- Growth is kept exactly: grow stage, day planted, times harvested, watered and fed history
- Refuses to place a plant where it could never be watered, so you cannot strand one by accident
- Your character says why when a spot is refused
- Esc puts a plant back precisely where it came from
- Works on multi-tile crops such as grape trellises
- Wild trees, bushes and weeds can be moved too, if you turn them on
- No hotkey to learn — open decorate mode and move a plant
- An optional arming key, if you would rather crops stayed locked while you decorate
- Nothing new is written to your save, and uninstalling leaves no trace

---

### Requirements

**Required**

- BepInEx 5 (win_x64)

**Optional**

- Mod Menu — adds a Mods page to the pause menu so you can change this mod's settings in game.
  Not needed; without it the settings live in a plain config file, and the defaults are meant to
  be left alone.

**Compatibility**

Serena's Conjuring deliberately excludes crops from its building mover, so the two do not
overlap and can be installed together. Free Decorate changes placement validation rather than
what can be picked up, so it does not conflict either — though note that Transplant's soil rule
still applies on top of it, by design.

The in-game Move Grid Object spell is untouched: it uses its own rules, which this mod does not
patch.

---

### Shout outs

- **Little Chicken Game Company** for making a game worth spending this much time inside.
- The **BepInEx** and **HarmonyX** teams, without whom none of this scene exists.
- **SerenaEnchanted**, whose Conjuring page states plainly that crops were left out to keep
  their watering and growth records safe. That was the right call, and it is what pointed
  straight at the problem this mod had to solve.
- **Elsiabeth** for Mod Menu, which is why this mod's settings are configurable in game without
  it having to build a settings screen of its own.

---

## Changelog entries for the Nexus page

Player-facing. Describe the **symptom**, not the cause — the repo README names the Harmony
patches; that belongs in the repo.

### 1.0.0

```
First release.

- Move crops you have already planted, in decorate mode.
- They keep their growth stage, planted day and harvest count.
- The mod will not let you put a plant somewhere it could never be watered.
- Esc puts a plant back exactly where it came from.
```

---

## Screenshots

Files live in `screenshots/`. The thumbnail is set separately in the upload form.

| # | Shot | File | Status |
|---|---|---|---|
| - | Thumbnail, **16:9** | `thumbnail.png` | ⬜ to make |
| - | Title banner | `banner.png` | ⬜ optional |
| 1 | A crop lifted mid-move, cursor valid, farm visible | `01-moving-a-crop.png` | ⬜ to capture |
| 2 | The refusal — plant over bare ground, invalid cursor, shout visible | `02-needs-soil.png` | ⬜ to capture |
| 3 | Before/after of a row straightened | `03-row-fixed.png` | ⬜ to capture |
| 4 | Mod Menu settings panel *(optional)* | `04-settings.png` | ⬜ optional |

**Shot 2 is the one that sells it, not shot 1.** Anyone can claim "move your plants"; the shout
reading *"It needs watered ground."* over an invalid placement is the visible proof that the mod
protects the crop rather than just unlocking it. That is the difference between this and simply
removing a restriction, and it is the reason the most-downloaded decorate mod left crops alone.

Shot 3 wants a genuinely crooked row in the "before". Straightening something already nearly
straight reads as no change at all.

Grapes are the best subject — a trellis is tall, unmistakable, and obviously a thing you would
not want to replant.

### Thumbnail must be composed at 16:9

Listing tiles use `object-fit: fill`, so an off-ratio thumbnail is **stretched, not cropped**.
See the measurement and reasoning in [CoffinBreak/NEXUS.md](../CoffinBreak/NEXUS.md) — 1672x941
is a good render size. Proofread any generated lettering at 5x or more before accepting it; the
PowerShell crop-and-upscale snippet in that file is the tool for it.

### Art direction

Palette is fixed by [10-visual-integration.md](../../10-visual-integration.md): `#1B0F2E` plum
fill, `#C7A25B` gold rim, `#F7D994` warm gold text, `#2A1B3D` ink.

Concept worth trying first: **a grape vine lifted slightly out of its soil, roots and root-ball
intact, with the empty tile it came from still visible.** The intact root-ball is the whole
promise — the plant survives the move — and it needs no caption.

### Worth knowing before using AI-assisted art

At least one mod in this scene advertises **"NO AI."** in its description as a selling point,
which implies the opposite draws comment here. Not a reason to avoid it — just a signal specific
to this community that is better known in advance than discovered in the comments.

---

## Notes before publishing

- ✅ **Play-tested.** Confirmed working in game on 2026-08-04: crops select, move and place, on
  multi-tile grape trellises included.
- ⬜ **Save safety is argued, not yet measured.** The mod writes no new records — it only lets
  the game's own `SetPosition` run — but the save diff in
  [TESTING.md](TESTING.md) §7 and the save/load round-trip in §2 have not been run. **Do both
  before upload**, because "save-safe" is claimed in the description above.
- State plainly that it is **save-safe** — this community reads for that.
- Say **comfort, not a cheat**: it moves a plant without skipping a growth day, changing a yield
  or duplicating anything.
- List BepInEx as **required** and Mod Menu as **optional**.
- Decide the licence / permissions stance before upload.
- The subtitle carries the search. Do not drop it.
