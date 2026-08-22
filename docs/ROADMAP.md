# ROADMAP — Transplant

The mod is **published** (v1.0.0, [Nexus mod 126](https://www.nexusmods.com/moonlightpeaks/mods/126)).
It is feature-complete for its stated purpose; the trajectory from here is verification and polish,
not new scope.

## Done

- ✅ Research the decorate-mode move path and the watering trap (`research/01-moving-growables.md`).
- ✅ Move planted crops without losing growth; the soil-safety rule; cancel restore; wild-plant
  opt-in; full config + Mod Menu integration.
- ✅ v1.0.0 release + Nexus page.
- ✅ Workspace tooling adoption: version single-sourcing, generic `pack.ps1`, synced
  `Directory.Build.props`.

## In progress

- 🧪 **Verification pass** — the save+reload test that gates the "keeps its growth" claim is still
  unrun (see [BACKLOG.md](BACKLOG.md), [../TESTING.md](../TESTING.md)).

## Planned / if warranted

- 🔜 Structural tidies triaged in [BACKLOG.md](BACKLOG.md) (only if the code grows).
- 🔜 Consider stacked sub-grid (herb pot) support — currently out of scope; would need the
  `DecorateSelectStackedState` path tested end to end.

_Living doc — refresh with /project-docs when it drifts._
