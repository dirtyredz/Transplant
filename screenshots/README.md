# Screenshots

Drop captures here. Full reasoning, thumbnail ratio and art direction live in
[../NEXUS.md](../NEXUS.md); this is the shot list to work from while the game is open.

| # | File | What it needs to show | Status |
|---|---|---|---|
| 1 | `01-moving-a-crop.png` | A crop lifted mid-move, cursor reading valid, farm recognisable behind it | ✅ 541x455 — grape trellis, selection brackets clear |
| 2 | `02-needs-soil.png` | **The refusal.** Plant held over bare ground, cursor invalid, the shout *"It needs watered ground."* visible | ⬜ wanted, not blocking |
| 3 | `03-row-fixed.png` | A crooked row, then the same row straightened | ⬜ optional |
| 4 | `04-wild-plants.png` | A wild tree selected, showing the opt-in `IncludeWildPlants` feature | ✅ 641x517 |
| 5 | `05-settings.png` | Settings panel *(optional)* | ⬜ optional |
| - | `thumbnail.png` | Composed at **16:9**, e.g. 1672x941 | ⬜ to make |
| - | `banner.png` | Title banner *(optional)*, ratio only roughly matters | ⬜ optional |

### On the two captured so far

Both are **window crops at roughly 1.2:1**, around 540–640px wide. They read clearly and prove
the feature, but the gallery shows full frames — a native-resolution capture of the whole game
window will look markedly better next to other mods' listings. Worth re-taking at full size if
there is an easy opportunity; not worth blocking a release over.

Coffin Break hit the same thing and kept its crop as evidence rather than as a listing image.

## Shot 2 would sell it, but it is not blocking

Shot 1 shows a restriction removed, which anyone could claim. Shot 2 shows the mod **protecting
the crop** — the actual difference between this and simply unlocking plants. The
most-downloaded decorate mod in the scene left crops alone precisely because moving them safely
is the hard part, so the shout in frame is the proof that this one solved it.

**Ship without it if it is awkward to stage.** The description carries the safety claim in words
and opens on it; a missing image weakens the pitch rather than breaking it.

### Capturing it does not risk the plant

Worth stating plainly, because it sounds like it should be risky. A refused placement is a
placement that **did not happen** — the plant stays in your hand:

1. Enter decorate mode and pick up a crop
2. Hold it over plain untilled grass; the cursor reads invalid
3. Click anyway — the shout fires, and the plant is still in hand
4. Capture
5. Move back over its original tile and click to set it down

Nothing is written to the save at any point, and step 5 returns the plant to where it started.
Esc should restore it too, but that path is not yet verified, so prefer step 5.

## Practical notes

- **Grapes are the best subject.** A trellis is tall, instantly readable, and obviously
  something you would not want to replant by hand.
- **Shot 3 needs a genuinely crooked "before".** Straightening a row that was nearly straight
  reads as no change at all.
- **Night shots** sit better against the game's lighting and look unmistakably like Moonlight
  Peaks.
- Capturing is not timing-sensitive here — unlike Coffin Break, nothing hides on a keypress.
  Take your time framing.
- Turn `VerboseLogging` off before capturing if any HUD/console overlay is visible.
