# Sprint Change Proposal

**Date:** 2026-02-22
**Author:** Bob (Scrum Master)
**Status:** Pending Approval
**Change Scope:** Moderate

---

## 1. Issue Summary

### Problem Statement

A comprehensive brainstorming session (2026-02-20) on Perch-Gallery catalog curation produced **15 actionable stories** covering:
- Taxonomy redesign (8 top-level categories replacing current structure)
- Schema changes (removing `kind`/`hidden` fields, adding `cli-tool`/`sort` fields)
- Detection system improvements (config link targets, perch-config manifest existence)
- Category re-mapping for 261 catalog entries

These changes are **foundational** and directly impact how Epic 22 (Apps page with sorting, categories, badges) should be implemented.

### Discovery Context

- **Trigger:** Deliberate planning session, not implementation failure
- **Source:** `_bmad-output/brainstorming/brainstorming-session-2026-02-20.md`
- **Scope:** Two repos (Perch + perch-gallery)
- **Output:** 78 questions analyzed, 8-category taxonomy defined, 15 stories with dependency graph

### Evidence

The brainstorming session produced a complete taxonomy redesign:

| # | Category | Pattern | Key Changes |
|---|----------|---------|-------------|
| 1 | Languages | Drill-down | Subcategories per language ecosystem |
| 2 | Terminal | Direct | Merged from Dotfiles + Terminal |
| 3 | Development | Direct | IDEs, APIs, Databases, Containers |
| 4 | Essentials | Drill-down | Browsers, Communication, Passwords, etc. |
| 5 | Media | Direct | Graphics, Video, Audio |
| 6 | Gaming | Direct | Launchers, Controllers, Modding |
| 7 | Power User | Direct | File Management, System Tools |
| 8 | Companion Tools | Direct | Alternatives to Perch |

---

## 2. Impact Analysis

### Epic Impact

| Epic | Current Status | Impact | Action |
|------|----------------|--------|--------|
| Epic 20 (Languages) | in-progress | None | Continue to completion |
| Epic 21 (Dotfiles) | in-progress | None | Continue to completion |
| Epic 22 (Apps) | in-progress | **Blocked** | Pause 22-1/22-2/22-3 |
| **Epic 30 (NEW)** | — | Insert | Gallery Taxonomy & Schema Overhaul |

### Story-Level Impact on Epic 22

| Story | Status | Impact |
|-------|--------|--------|
| 22-5 (AppCard shared component) | review | **No impact** — can complete |
| 22-4 (App detail page) | ready-for-dev | **No impact** — can continue |
| 22-1 (sorting/categories) | backlog | **Blocked** — depends on new taxonomy |
| 22-2 (alternatives/suggestions) | backlog | **Blocked** — depends on new taxonomy |
| 22-3 (hot badges) | backlog | **Blocked** — depends on badge strategy |

### Artifact Conflicts

| Artifact | Section | Current State | Required Change |
|----------|---------|---------------|-----------------|
| PRD | FR-G64 | Deep paths like `Apps/Languages/.NET/Editors/Visual Studio` | Update to 8-category structure |
| Architecture | Decision #16 | Same deep path examples | Update taxonomy, document drill-down pattern |
| Architecture | Gallery Schema | `type: app \| tweak \| font` | Remove `kind`, add `cli-tool` flag |
| UX Spec | Unified tree taxonomy | Deep path examples | Update to 8-category structure |
| UX Spec | Three-tier layout | "Your apps / Suggested / Other" | Remove Suggested tier (hot badges only) |
| gallery-epics.md | Epics 3-4 | Schema evolution stories | Reconcile overlap with Epic 30 |

### Technical Impact

**Perch Repo (C# changes):**
- `AppsViewModel`: Remove `_subcategoryOrder` dictionary
- `CatalogParser`: Remove `Kind` property handling
- `CatalogEntry`: Add `CliTool` boolean, `Sort` integer
- Detection services: Add config link target check, perch-config manifest check
- Sidebar navigation: Update to 8 top-level items

**perch-gallery Repo (YAML changes):**
- `categories.yaml`: Complete rewrite with 8-category structure
- All 261 entries: Update `category` field
- 93 entries: Remove `kind` field
- 128 entries: Remove `hidden` field
- Schema: Add `sort` field support

---

## 3. Recommended Approach

### Selected Path: Insert Epic 30 (Gallery Sprint)

**Strategy:**
1. Complete Epics 20-21 (no dependency on taxonomy)
2. Complete Epic 22-4 and 22-5 (no dependency on taxonomy)
3. Insert Epic 30 with 15 brainstorming stories
4. Execute Epic 30 as focused "Gallery Sprint"
5. Resume Epic 22-1/22-2/22-3 with correct taxonomy

### Execution Flow

```
CURRENT STATE                    PROPOSED STATE
─────────────────────────────    ─────────────────────────────
Epic 20 (Languages) [IP]    →   Epic 20 (Languages) [continue]
Epic 21 (Dotfiles) [IP]     →   Epic 21 (Dotfiles) [continue]
Epic 22 (Apps) [IP]         →   Epic 22-4, 22-5 [continue]
                                 ↓
                                 Epic 30 (Gallery) [INSERT]
                                 ↓
                                 Epic 22-1/22-2/22-3 [resume]
```

### Effort & Risk Assessment

| Factor | Assessment | Notes |
|--------|------------|-------|
| Effort | **Medium** | 15 stories, clear acceptance criteria |
| Timeline | **+1 sprint** | Epic 30 adds work before Epic 22 completion |
| Technical Risk | **Low** | Schema/taxonomy changes, not architectural |
| Rework Risk | **Eliminated** | Avoids implementing wrong taxonomy |

### Alternatives Considered

| Alternative | Why Not Selected |
|-------------|------------------|
| Continue Epic 22 as-is | Would require rework when taxonomy changes |
| Merge stories into Epic 22 | Epic becomes too large (22 stories), mixes concerns |
| Defer brainstorming output | Loses momentum, stories become stale |
| Rollback | Nothing to roll back — trigger is new scope, not failure |

---

## 4. Detailed Change Proposals

### Epic 30: Gallery Taxonomy & Schema Overhaul

**Objective:** Implement the taxonomy, schema, and detection changes from the 2026-02-20 brainstorming session.

**Stories:**

| # | Story | Repo | Blocked By |
|---|-------|------|------------|
| 30-1 | Redesign categories.yaml with new taxonomy | perch-gallery | — |
| 30-2 | Remove hardcoded subcategory sort from AppsViewModel | Perch | 30-1 |
| 30-3 | Re-categorize all 261 entries to new taxonomy | perch-gallery | 30-1 |
| 30-4 | Remove `kind` field, add `cli-tool` flag | Both | — |
| 30-5 | Remove `hidden` field from all entries | Both | — |
| 30-6 | Add `sort` field to catalog entries | Both | 30-2 |
| 30-7 | Implement drill-down pattern for Essentials | Perch | 30-2 |
| 30-8 | Update sidebar navigation for 8 categories | Perch | 30-7 |
| 30-9 | Remove Suggested tier from Apps view | Perch | — |
| 30-10 | Detection: config link target existence | Perch | — |
| 30-11 | Detection: perch-config manifest existence | Perch | — |
| 30-12 | Add `install.detect` to entries with detection gaps | perch-gallery | 30-3 |
| 30-13 | Audit and add missing GitHub links | perch-gallery | — |
| 30-14 | Validation script for missing fields | perch-gallery | — |
| 30-15 | Retire index.yaml (build from individual files) | Both | — |

### Dependency Graph

```
30-1 (categories.yaml)
  ├── 30-2 (remove hardcoded sort)
  │     ├── 30-6 (sort field on entries)
  │     └── 30-7 (drill-down for Essentials)
  │           └── 30-8 (sidebar navigation)
  ├── 30-3 (re-categorize 261 entries)
  │     └── 30-12 (add install.detect values)
  └── (no dependency)
        ├── 30-4 (remove kind, add cli-tool)
        └── 30-5 (remove hidden)

Independent:
  30-9  (remove Suggested tier)
  30-10 (detection: config target)
  30-11 (detection: perch-config manifest)
  30-13 (add GitHub links)
  30-14 (validation script)
  30-15 (retire index.yaml)
```

### PRD Update (FR-G64)

**OLD:**
> Gallery uses a unified tree taxonomy with deep category paths (e.g., `Apps/Languages/.NET/Editors/Visual Studio`).

**NEW:**
> Gallery uses an 8-category taxonomy with two navigation patterns:
> - **Drill-down** (Languages, Essentials): Click category to see subcategory cards
> - **Direct** (Terminal, Development, Media, Gaming, Power User, Companion Tools): Click category to see app cards directly
>
> Categories defined in `categories.yaml` with `sort` fields at every level. See brainstorming session 2026-02-20 for full taxonomy specification.

### Architecture Update (Decision #16)

**OLD:**
> Content taxonomy: Unified tree with deep category paths (e.g., `Apps/Languages/.NET/Editors/Visual Studio`)

**NEW:**
> Content taxonomy: 8 top-level categories with drill-down vs direct navigation patterns. Taxonomy and sort order defined in `categories.yaml`, not hardcoded. `kind` field removed, replaced by `cli-tool: true` flag + category placement.

---

## 5. Implementation Handoff

### Change Scope Classification

**Moderate** — Requires backlog reorganization and documentation updates, but implementation is straightforward.

### Handoff Assignments

| Role | Agent | Responsibility |
|------|-------|----------------|
| Scrum Master | Bob | Create Epic 30, update sprint-status.yaml, set up blocking relationships |
| Product Manager | John | Update PRD FR-G64 with new taxonomy |
| Architect | Winston | Update Architecture Decision #16 and Gallery Schema section |
| UX Designer | Sally | Update UX Spec taxonomy examples, remove Suggested tier references |
| Developer | Amelia | Execute stories once epic is set up |

### Success Criteria

- [ ] Epic 30 created with 15 stories in sprint-status.yaml
- [ ] Stories 22-1, 22-2, 22-3 marked as blocked by Epic 30
- [ ] PRD FR-G64 updated
- [ ] Architecture Decision #16 updated
- [ ] UX Spec taxonomy sections updated
- [ ] All 15 stories completed with passing tests
- [ ] Epic 22-1/22-2/22-3 unblocked and ready to resume

### Next Steps After Approval

1. **Immediate:** Update sprint-status.yaml with Epic 30 and blocking relationships
2. **This session:** Update PRD, Architecture, UX Spec documentation
3. **Next sprint:** Begin Epic 30 story execution (start with 30-1: categories.yaml)

---

## Approval

**Requesting approval to:**
1. Create Epic 30 (Gallery Taxonomy & Schema Overhaul)
2. Block Epic 22 stories 22-1/22-2/22-3 until Epic 30 completes
3. Update PRD, Architecture, and UX Spec per proposals above
4. Continue Epics 20-21 and Epic 22-4/22-5 without interruption

---

*Generated by Correct Course workflow*
*Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>*
