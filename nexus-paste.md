> ⚠️ **Superseded — do not paste from this file.**
> The live pages were restyled on 2026-08-04 and this BBCode is the *pre-style* version.
> The live page is now the source of truth; pull its BBCode from the edit form's description
> field. Structure: [14-description-review.md](../../14-description-review.md). Look:
> [15-page-style.md](../../15-page-style.md). Mechanics: [13-nexus-page-standard.md](../../13-nexus-page-standard.md).

# Transplant — Nexus page source

**Nexus page:** [mod 126](https://www.nexusmods.com/moonlightpeaks/mods/126)

The description field is **SCEditor with a BBCode source**, so the block below is the literal
value that gets set. Structure per [14-description-review.md](../../14-description-review.md).

Description prose and Main features wording are **yours, unchanged**.

Like Last Swing, the live version has no lists at all — 48 hard line breaks, with every bullet
and numbered step as literal `-` and `1.` text.

## Other fields

| Field | Change |
|---|---|
| Name | `Transplant` — **no change.** Adding a subtitle was considered and rejected; mod names stay clean. The searchable words *move*, *planted* and *crops* now carry in the short description instead |
| Category | User Interface → **Gameplay** ✅ applied — where Free Decorate, Place Items Diagonally and Walk Through Crops all sit, which is where someone looking for this would browse |
| Tags | Quality of Life, Gameplay. Note Nexus tags are a fixed vocabulary — free-text tags like `farming` cannot be added |
| Short description | replace, see below |

**Short description** (replaces *"open decorate mode, move a planted crop. Growth stage,
planted day and harvest count all survive"* — lowercase start, no full stop, and it opens with
an instruction to someone who does not yet know what the mod is):

```
Planted the row slightly wrong? Move it. Crops keep their growth, and they will not let you strand them somewhere they cannot be watered.
```

## Description source

```bbcode
[size=4][b]Description[/b][/size]
[color=#D4D4D8]You plant a row of grapes, step back, and it is one tile off.

There is nothing you can do about it. Moonlight Peaks lets you move furniture, fences, paths and whole buildings — but not a plant you have already planted. The only fix is to lose the crop and start it again.

Transplant makes a growing plant behave like any other decoration. Pick it up in decorate mode, put it somewhere better, and it carries on exactly where it left off — same grow stage, same planted day, same harvest count.

It also refuses to let you ruin one. Watering in this game is recorded on the [i]tile[/i], not on the plant, so a crop moved onto bare ground would quietly stop growing forever while still looking perfectly healthy. Transplant will not place a plant anywhere it could never be watered, and your character tells you why.

Nothing new is written to your save.[/color]

[size=4][b]Main features[/b][/size]
[list]
[*]Move planted crops in decorate mode, like any other decoration
[*]Growth is kept exactly: grow stage, day planted, times harvested, watered and fed history
[*]Refuses to place a plant where it could never be watered, so you cannot strand one by accident
[*]Your character says why when a spot is refused
[*]Esc puts a plant back precisely where it came from
[*]Works on multi-tile crops such as grape trellises
[*]Wild trees, bushes and weeds can be moved too, if you turn them on
[*]No hotkey to learn — open decorate mode and move a plant
[*]An optional arming key, if you would rather crops stayed locked while you decorate
[*]Nothing new is written to your save, and uninstalling leaves no trace
[/list]

[size=4][b]Requirements[/b][/size]
[list]
[*][b]BepInEx 5 (win_x64)[/b], version 5.4.23.5 or newer — the only thing this mod needs
[/list]
[color=#D4D4D8]PC/Steam only. The Switch and mobile builds cannot load BepInEx.[/color]

[size=4][b]Installation[/b][/size]
[b]With Vortex[/b]
[color=#D4D4D8]Open the Files tab, click the Vortex button, and enable the mod. Done.[/color]

[b]Manually[/b]
[list=1]
[*]Install [b]BepInEx 5 (win_x64)[/b] into your Moonlight Peaks folder, if you do not have it already. The BepInEx folder sits beside Moonlight Peaks.exe.
[*]Launch the game once, then quit. This creates the BepInEx/plugins folder.
[*]Download the archive from the Files tab and extract it over your Moonlight Peaks folder, so the file ends up at BepInEx/plugins/Transplant/Transplant.dll
[*]Launch the game.
[/list]
[color=#D4D4D8]To uninstall, delete the BepInEx/plugins/Transplant folder. Plants stay wherever you last put them, which is an ordinary game state — nothing was ever written to your save.[/color]

[size=4][b]Configuration[/b][/size]
[color=#D4D4D8]Settings are written to BepInEx/config/com.dirtyredz.moonlightpeaks.transplant.cfg on first launch. The defaults are meant to be left alone.

Install [url=https://www.nexusmods.com/moonlightpeaks/mods/127][b]Mod Nook[/b][/url] and you can change them in game instead. Transplant shows up in it on its own, so you can set the optional arming key by pressing the key you want rather than spelling it out in a file, and turn wild plants on and off without leaving the game. Nothing here needs it — it just makes this mod easier to live with.[/color]

[size=4][b]Compatibility[/b][/size]
[color=#D4D4D8]Serena's Conjuring deliberately excludes crops from its building mover, so the two do not overlap and can be installed together. Free Decorate changes placement validation rather than what can be picked up, so it does not conflict either — though note that Transplant's soil rule still applies on top of it, by design.

The in-game Move Grid Object spell is untouched: it uses its own rules, which this mod does not patch.[/color]

[size=4][b]Shout outs[/b][/size]
[list]
[*][b]Little Chicken Game Company[/b] for making a game worth spending this much time inside.
[*]The [b]BepInEx[/b] and [b]HarmonyX[/b] teams, without whom none of this scene exists.
[*][b]SerenaEnchanted[/b], whose Conjuring page states plainly that crops were left out to keep their watering and growth records safe. That was the right call, and it is what pointed straight at the problem this mod had to solve.
[*][b]My Mate[/b], for being my inspiration.
[/list]
```
