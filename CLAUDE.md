# CLAUDE.md — working on Transplant

How to work in this mod repo. Orientation lives in the doc set — read those, don't duplicate them.

- **[README.md](README.md)** — human quick-start + the design rationale ("why this doesn't exist").
- **[STRUCTURE.md](STRUCTURE.md)** — where the code lives (6 files in `src/`).
- **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)** — how the patches + gates work at runtime.
- **[docs/DECISIONS.md](docs/DECISIONS.md) · [FEATURES.md](docs/FEATURES.md) ·
  [GOTCHAS.md](docs/GOTCHAS.md) · [ROADMAP.md](docs/ROADMAP.md) · [BACKLOG.md](docs/BACKLOG.md)**

## This is a standalone repo nested in the Moonlight Peaks workspace

It is its **own git repo** under `mods/Transplant`. Honor **this** repo's structure-review gate and
baseline — not the workspace root's. Scope every sweep with
`git ls-files --cached --others --exclude-standard`. Workspace-wide conventions and the
release/pack pipeline live in `../../CLAUDE.md`, `../../STRUCTURE.md`, `../../docs/ARCHITECTURE.md`.

## Build & release

- Build/pack: `powershell -File pack.ps1` → `dist/Transplant-<version>.zip` (Nexus layout).
- **`pack.ps1` and `Directory.Build.props` are workspace-synced canonicals** — do not hand-edit
  here; change `../../tools/*` and re-run `../../tools/sync-mod-files.ps1`.
- Version bumped in `src/Transplant.csproj` `<Version>` only, at publish time; never hardcode it in
  `Plugin.cs`. See [docs/DECISIONS.md](docs/DECISIONS.md).
- No test project — every path reads live game state. [TESTING.md](TESTING.md) carries the manual
  checklist. **Do not launch the game** as part of automated work.

## Conventions

- Commit identity: `dirtyredz <dirtyredz@live.com>`.
- Plugin `.cs` flat in `src/` (no `src/Transplant/`).
- Read-before-you-code workspace gates: saves → `../../11`; colour → `../../16`; UI → `../../10`/`17`.

## Structure-review gate

This repo is gated (pre-push hook, installed 2026-08-22). Edit/debug freely; the review fires once
at **push** on the accumulated change, not per edit or commit. Commit freely at logical boundaries;
Claude runs the review and pushes (asking first) when work is ready. `/gate status` shows what's
pending.
