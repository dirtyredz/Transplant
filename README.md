# Transplant

Move planted crops in decorate mode — without losing their growth.

**Status:** 🚀 **Published** — v1.0.0 live on Nexus as
[mod 126](https://www.nexusmods.com/moonlightpeaks/mods/126). Moving planted crops in decorate
mode does what it says, grape trellises included. See [RELEASING.md](RELEASING.md).

Build the archive with `.\pack.ps1` → `dist/Transplant-1.0.0.zip`.

**Nexus title (planned):** `Transplant - Move Planted Crops Without Losing Growth`

## How to use it

**Open decorate mode and move a plant.** No key, no mode to arm — plants behave like any other
decoration.

If you would rather plants stayed locked until you ask for them, set `RequireModifier = true`
and they become selectable only while **X** is active (press to toggle, or hold if you set
`PressToToggle = false`). That guard is off by default: it was this mod's own idea, and two
releases in a row failed because of it rather than because of anything to do with plants.

Every setting is configurable through Mod Menu.

## Wild trees and bushes

Not movable by default. The game records `IsPlayerPlanted` on a growable and sets it in exactly
one place — `PlayerPlantItemState`, when *you* plant a seed — so anything the world grew is
excluded. Wild trees even grow through a requirement that checks for it:

```csharp
// WildTreeGrowStageRequirement
public override bool IsRequirementCompleted(...) => !growablePersistence.IsPlayerPlanted;
```

Set `IncludeWildPlants = true` to move them anyway. Note this changes the character of the mod
— rearranging the landscape rather than tidying your farm — which is why it is opt-in.

The soil rule does not apply to them, and that is deliberate rather than an oversight. It exists
to stop a plant being stranded where its water requirement can never be met; a wild tree has no
water requirement to strand, and nothing waterable sits under one, so enforcing it would let you
pick a tree up and then refuse every spot you tried to put it down.

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

Seven patches, no new data.

| Patch | Why |
|---|---|
| `BaseDecorateStateMachineContext.CanMoveGridView` | The pickup gate. The one method on the selection path that reads `GridControlType.Movable`. |
| `GridObjectHelper.IsPlacementAllowed` | The soil rule. Vetoing at the outermost validation turns the cursor red and blocks the click through the game's own path. |
| `BaseDecorateStateMachineContext.ShoutIfPlacementIsInValid` | Explains the refusal. The game calls this only when the player pressed place and it failed, so it cannot spam. |
| `ObjectPickupAction.Cancel` | Returns a cancelled plant to its original tile. Vanilla skips its restore for anything not flagged movable. |
| `PlayerDecorateStateMachine` (4 methods) | Tracks whether decorate mode is open and whether a plant is in hand. |
| `DecorateSelectState` (5 methods) | Forces the game to recompute its selection when arming changes, plus diagnostics. |
| `BaseDecorateState.OnActivate` | Names each decorate state in the log as it runs. |

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

## Guarantees

- **Save-safe.** No sidecar file and no new persistence type — the mod only lets the game's
  own `SetPosition` run on an object the game already owns.
- **Comfort, not a cheat.** Moves a plant. Does not skip a growth day, change a yield, or
  duplicate anything.

## License

MIT — see [LICENSE](LICENSE).
