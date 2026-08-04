# Transplant

Move planted crops in decorate mode — without losing their growth.

**Status:** 🔬 **Researched, not started.** No code yet. This repo currently holds the
decompile notes that the implementation will be built from.

**Nexus title (planned):** `Transplant - Move Planted Crops Without Losing Growth`

## What it will do

Moonlight Peaks lets you rearrange furniture, paths and (with
[Serena's Conjuring](https://www.nexusmods.com/moonlightpeaks/mods/62)) whole buildings in
decorate mode. It will not let you move a plant you have already planted. Plant a row of
crops slightly wrong and the only fix is to lose them.

Transplant makes a growing plant pickable in decorate mode, and puts it back down with its
grow stage, planted day and harvest count intact.

## Why this doesn't already exist

Serena's Conjuring — the most-downloaded decorate mod for this game — moves buildings, and
says on its own page:

> Crops, tilled soil, paths and other location-dependent farming objects are deliberately
> excluded so their watering and growth records remain safe.

That is the right call given what the game does, and it is the problem this mod has to solve
rather than avoid. The reason is in [the research](research/01-moving-growables.md), but in
one line: **a plant's watering is keyed to the cell it stands on, not to the plant.** Move a
crop onto bare ground and it stops growing permanently, while still looking perfectly
healthy — no warning, no log line, visible only days later.

So the core rule is not "let plants move". It is **refuse to place a growable on a cell with
no waterable.** Everything else is comparatively easy: the pickup gate is a single virtual
method, and the game's own decorate selection already does the hover work.

## Research

- [research/01-moving-growables.md](research/01-moving-growables.md) — the decorate-mode gate,
  what survives a move and what doesn't, the two traps, and the four questions still open.

## Layout

Sources and the project file go **directly in `src/`** — there is no `src/Transplant/`
subfolder:

```
Directory.Build.props
src/
  Transplant.csproj
  Plugin.cs
  ...
```

This matches Last Swing, Coffin Break and Bigger UI. Chest Labels and Plant Peek still carry
an older `src/<ModName>/` nesting — **don't copy those two when scaffolding.** The flattening
is on disk only: the csproj still sets `AssemblyName`/`RootNamespace` to `Transplant` and
deploys to `BepInEx\plugins\MoonlightPeaksMods\Transplant`, so the built artifact is
unchanged.

## Planned guarantees

- **Save-safe.** No sidecar file and no new persistence type — the mod only lets the game's
  own `SetPosition` run on an object the game already owns.
- **Comfort, not a cheat.** Moves a plant. Does not skip a growth day, change a yield, or
  duplicate anything.

## License

MIT — see [LICENSE](LICENSE).
