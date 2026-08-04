# Transplant

Move planted crops in decorate mode — without losing their growth.

**Status:** v0.1.1 — 0.1.0 was tested and did nothing; the cause is fixed and the fix is not yet confirmed. See [TESTING.md](TESTING.md), which now starts with how to read the log.
were checked to exist in the decompiled assembly, but nothing here has been exercised. The
checks that matter are in [TESTING.md](TESTING.md).

**Nexus title (planned):** `Transplant - Move Planted Crops Without Losing Growth`

## How to use it

**Hold X** in decorate mode and plants become selectable; pick one up and move it like any
other decoration. Without the key held, decorate mode behaves exactly as it always did — the
arming key exists so the cursor does not start grabbing crops you were trying to decorate
around. Configurable, along with everything else, through Mod Menu.

## What it does

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

## How it is built

Five patches, no new data.

| Patch | Why |
|---|---|
| `BaseDecorateStateMachineContext.CanMoveGridView` | The pickup gate. The one method on the selection path that reads `GridControlType.Movable`. |
| `GridObjectHelper.IsPlacementAllowed` | The soil rule. Vetoing at the outermost validation turns the cursor red and blocks the click through the game's own path. |
| `BaseDecorateStateMachineContext.ShoutIfPlacementIsInValid` | Explains the refusal. The game calls this only when the player pressed place and it failed, so it cannot spam. |
| `ObjectPickupAction.Cancel` | Returns a cancelled plant to its original tile. Vanilla skips its restore for anything not flagged movable. |
| `PlayerDecorateStateMachine` (4 methods) | Tracks whether decorate mode is open and whether a plant is in hand. |

Two decisions worth knowing, both departures from the first draft of the research:

**The gate is patched on `CanMoveGridView`, not on `GridObjectItemAddon.ControlTypes`.** The
research preferred the latter because it would fix cancel for free. But `ControlTypes` is a
trivial auto-property getter, and a getter small enough for the Mono JIT to inline is a poor
Harmony target — call sites already inlined before the patch applies keep the old answer.
`CanMoveGridView` has a real body and cannot be inlined away. The cost is one extra patch on
`ObjectPickupAction.Cancel`, which is cheap and explicit.

**A pickup latches arming on until the plant is placed or cancelled.** Arming is normally tied
to the held key, but `ObjectPickupAction.Cancel` restores position only for objects that read
as movable *at the moment Esc is pressed*. Let go of X mid-carry without the latch and
cancelling would drop the plant wherever the cursor was.

Two of the research's open questions are answered by construction rather than by testing.
`SoilCheck` mirrors `WaterGrowStageRequirement`'s lookup exactly instead of asking "is there a
farm tile here", so a growable that turns out to carry its own waterable satisfies the check
the same way it would satisfy the game, and the check is on the **origin cell only** — the only
cell the game itself looks at — which avoids being stricter than vanilla for multi-cell trees.

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
