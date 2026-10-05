---
id: dk-cae89a05
type: task
created: 2026-10-05
status: todo
since: 2026-10-05
area: structure
priority: P2
rank: t
parent:
fixes: []
blocked_by: []
relates: []
---
# Consolidate the Diagnostics log-once-per-key dedup

From docs/BACKLOG.md (Structural, 2026-08-22 full review)

- [ ] **P2 — Consolidate the `Diagnostics` "log once per key" dedup.** `Allowed`/`Verdict`/`Column`
      hand-roll the pattern already named as `Say`. Non-trivial: `Say` uses a single shared last-key
      slot, so naive routing would cross-suppress lines — needs per-key slots (a small dict) to stay
      behavior-preserving, and only matters with verbose logging on (needs in-game verify).
