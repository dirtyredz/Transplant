# Releasing Transplant

Repo-wide rules live at the root; this file only covers what is specific to this mod.

- Versioning and archive layout: [12-versioning-and-release.md](../../12-versioning-and-release.md)
- Visual integration: [10-visual-integration.md](../../10-visual-integration.md)
- Save safety: [11-mod-data-and-saves.md](../../11-mod-data-and-saves.md)

Short version on numbering: the version is for players, not a build counter. The first published
version is **1.0.0**; bump only when publishing, one CHANGELOG entry per release.

## Build a release

```bash
powershell -File pack.ps1
```

Produces `dist/Transplant-<version>.zip`, reading the version from the csproj so the archive can
never disagree with the DLL; `Plugin.cs` derives that same version at build time via
`ModBuildInfo.Version`.

There is no test project. Every code path here reads live game state — the decorate state
machine, the grid surface, the persistence collections — none of which a headless runner can
assert. The checklist below carries the weight instead.

## Pre-release checklist

Root checklist first: [12-versioning-and-release.md](../../12-versioning-and-release.md).
Then the items specific to this mod.

### The two that matter

- [x] **A crop survives being moved.** Note a plant's stage, move it to another tilled tile,
      confirm the stage is unchanged, sleep, confirm it advances normally.
      **Confirmed in game 2026-08-04**, including multi-tile grape trellises.

- [ ] **A moved crop survives a save and reload.** The entire design rests on the plant keeping
      its GUID while only its position is rewritten. Move a crop, save, quit to the main menu,
      reload, and confirm it is still in the new spot with the right stage. **Not yet run, and
      it gates the "keeps its growth" claim on the mod page.**

Re-run both after any change to `MoveGate` or the `CanMoveGridView` patch.

### The safety rule

The failure this mod exists to prevent is silent and permanent: a plant on ground it can never
be watered from stops growing while still looking healthy.

- [ ] Carry a crop over untilled ground and confirm the cursor reads invalid
- [ ] Click anyway and confirm the shout appears — *"It needs watered ground."*
- [ ] Confirm the plant is still in hand afterwards, not on the grass
- [ ] Confirm a normal tilled tile still accepts it
- [ ] With `RequireSoil = false`, confirm placement on bare ground is allowed — the escape hatch
      works, and the config description warns what it costs

### Cancelling

- [ ] Pick a crop up, move well away, press **Esc**, confirm it returns to its original tile
      *and* rotation. Vanilla skips its own restore for anything not flagged movable, so this is
      entirely this mod's responsibility.

### Wild plants

- [ ] Default: trees, bushes and weeds are **not** selectable
- [ ] `IncludeWildPlants = true`: a tree can be picked up **and put down** — the soil rule must
      not apply to it, or it becomes unplaceable
- [ ] A moved wild tree still grows. It advances through `WildTreeGrowStageRequirement` rather
      than a watering path, and that has not been tested across a night

### Nothing else got looser

- [ ] Furniture, paths and chests move exactly as before
- [ ] The in-game **Move Grid Object spell** is unchanged — it overrides the patched method
      without calling base, so it should be untouched
- [ ] Herb-garden pots do not become movable; they are out of scope
- [ ] Install alongside Free Decorate and confirm the soil rule still holds

### Housekeeping

- [ ] `<Version>` is the single source of truth — `Plugin.cs` derives from it via `ModBuildInfo.Version`, but check the number is
      the one you meant
- [ ] CHANGELOG has one entry for this version
- [ ] `VerboseLogging` defaults to `false`, and a normal session writes only the load line
- [ ] Fresh install: delete `BepInEx/config/com.dirtyredz.moonlightpeaks.transplant.cfg`, launch,
      confirm sensible defaults are written — in particular `RequireModifier = false`
- [ ] Screenshots show the current build
- [ ] Thumbnail is composed at **16:9** — listing tiles use `object-fit: fill`, so an off-ratio
      image is stretched, not cropped. See [NEXUS.md](NEXUS.md)
- [ ] Archive extracted onto a clean install and verified in game

## Verifying save safety

Straightforward to argue: the mod has no storage of its own, adds no persistence type, and only
lets the game's own `SetPosition` run on an object the game already owns. `SoilCheck` and
`MoveGate` read persistence through `Find()` rather than `FindOrCreate()`, so inspecting a plant
never creates a record for it.

Measure it anyway, because the description claims it:

```powershell
$root = "$env:USERPROFILE\AppData\LocalLow\Little Chicken Game Company\Moonlight Peaks"
# Back up the save folder, move several plants, sleep a night, save, then diff.
```

Expect changes only to grid-object **positions**. Any new collection, or any new record keyed to
a plant you merely hovered over, is a bug — see the `Find()` note above.

## Licence

**MIT** — see [LICENSE](LICENSE). Permissive: anyone may use, modify and redistribute, provided
the copyright notice is kept.

Set the Nexus permissions to agree with it, or the page and the licence contradict each other:

| Nexus permission | Set to |
|---|---|
| Upload to other sites | Allowed |
| Convert to other games | Allowed |
| Modify and release | Allowed |
| Use assets in own files | Allowed |
| Include in mod packs / collections | Allowed |

Credit is customary rather than required under MIT. Asking for it in the description is fine; do
not set a permission that MIT already grants.

## Editing note

Do not round-trip these files through `Get-Content -Raw | Set-Content` in PowerShell. It
re-encodes non-ASCII characters and has corrupted em-dashes in this repo twice. `pack.ps1` reads
`Plugin.cs` with `Get-Content -Raw` but never writes it back, which is safe.
