# BACKLOG — Transplant

Prioritized deferred work + known issues, most useful first. P0 = do before next release · P1 =
should do · P2 = nice to have.

## Known issues / verification

- [x] **P0 — Run the save + reload test.** ✅ **Done 2026-08-22.** A Blood Grape crop was moved,
      saved, reloaded (still in the new spot at the same stage), and advanced after sleeping; verbose
      log corroborated the move + soil-rule vetoes. The "keeps its growth" claim is now verified.
      (See [../RELEASING.md](../RELEASING.md) checklist.)

## Structural (from the 2026-08-22 full review)

Full-depth review — componentization + abstraction lenses + Codex. Verdict: **sound**, all findings
P2 (minor), deferred on a stable published mod. See [../STRUCTURE.md](../STRUCTURE.md#structural-debt).
Two zero-risk cleanups were applied during the review (no version bump — internal only): the
redundant `VerboseLogging` guard in `CancelPatch` and a stale `PluginVersion` comment. The rest:

- [ ] **P2 — Move `VetoedOnFrame` off `SoilCheck` onto `PlacementPatch`.** It's written by
      `PlacementPatch` and read by `ShoutPatch` — a hidden channel between two Harmony adapters, not a
      soil-query result. Deferred: touches placement/shout coordination and wants an in-game recheck
      of the refusal shout timing. Also relocate its `SoilCheck.Reset()` clearing.
- [ ] **P2 — Consolidate the `Diagnostics` "log once per key" dedup.** `Allowed`/`Verdict`/`Column`
      hand-roll the pattern already named as `Say`. Non-trivial: `Say` uses a single shared last-key
      slot, so naive routing would cross-suppress lines — needs per-key slots (a small dict) to stay
      behavior-preserving, and only matters with verbose logging on (needs in-game verify).
- [ ] **P2 — Give `Diagnostics` the eligibility verdict instead of recomputing it.** `Considered`
      reproduces most of `MoveGate.IsMovablePlant`'s branches (and already omits `IsDestroyed`), so an
      eligibility change can silently make the explanation wrong. When eligibility next changes, have
      `MoveGate` return a small reason enum and pass it into `Diagnostics`.
- [ ] **P2 — Split config out of `Plugin.cs`.** Holds both the `TransplantPlugin` entry type and the
      `Plugin` static (8 config entries + logging). Extract a `Config.cs` only if the config surface
      grows — left whole now to avoid churn.
- [ ] **P2 — Extract the diagnostic-only patches** (`DecorateStateProbe`, `SelectStatePatches`'
      `AfterColumn`/`AfterCanMove`) into a `DiagnosticPatches` group *if* `Patches.cs` grows. Today
      the functional/diagnostic mix inside `SelectStatePatches` is a minor cosmetic blur, not a defect.
- [ ] **P2 — Watch the two-site `GamePersistence` room guard** (`SoilCheck.Scan`,
      `MoveGate.IsPlayerPlanted`). The callers deliberately fail in opposite directions (open vs
      closed), so extracting a shared helper isn't a clear win at two sites — only revisit if a third
      access point appears.

## Ideas / if warranted

- [ ] **P2 — Herb-garden pot (stacked sub-grid) support.** Deliberately out of scope; would need the
      `DecorateSelectStackedState` path tested end to end. Only if requested.

_Living doc — refresh with /project-docs when it drifts._
