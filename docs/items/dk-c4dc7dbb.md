---
id: dk-c4dc7dbb
type: task
created: 2026-10-05
status: todo
since: 2026-10-05
area: structure
priority: P2
rank: y
parent:
fixes: []
blocked_by: []
relates: []
---
# Split config out of Plugin.cs

From docs/BACKLOG.md (Structural, 2026-08-22 full review)

- [ ] **P2 — Split config out of `Plugin.cs`.** Holds both the `TransplantPlugin` entry type and the
      `Plugin` static (8 config entries + logging). Extract a `Config.cs` only if the config surface
      grows — left whole now to avoid churn.
