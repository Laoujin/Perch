---
stepsCompleted: [1, 2, 3, 4]
inputDocuments: []
session_topic: 'Stabilizing Perch WPF Desktop app - regressions, test gaps, scope management'
session_goals: 'Root cause analysis, regression prevention, sustainable development process'
selected_approach: 'progressive-flow'
techniques_used: ['Reverse Brainstorming', 'Five Whys', 'First Principles Thinking', 'Solution Matrix']
ideas_generated: [10]
context_file: ''
---

# Brainstorming Session: Desktop App Stability

**Facilitator:** Wouter
**Date:** 2026-02-17

## Session Overview

**Topic:** Stabilizing the Perch WPF Desktop app -- preventing regressions, addressing scope growth, and establishing a sustainable development process
**Goals:** Root cause analysis (merge hygiene vs test gaps), strategies to prevent silent breakage, managing expanding scope

### Context

- 465 core tests, zero ViewModel tests -- entire UI/MVVM layer untested
- Worktree merges (perch-2, perch-3) appear clean in git history
- Scope has grown beyond original PRD: 6 pages, 6 controls, wizard, 3 desktop services
- Things disappear after working, remarks need repeating across sessions
- perch-2 used as active feature branch with frequent merges
- GalleryDetectionService (the three-way join engine) is barely tested

## Phase 1: Reverse Brainstorming -- Sabotage Vectors

**10 failure modes identified:**

1. **Never test ViewModels** -- 7 ViewModels with zero tests, property bindings and state transitions unchecked
2. **Merge 53-file changesets across worktrees** -- git merge success != behavioral correctness
3. **Rely on session memory** -- knowledge disappears between sessions, not the code
4. **Run concurrent Claude instances** -- each session has amnesia about the other
5. **Only test by clicking through the UI** -- manual testing doesn't scale with 6 pages x multiple states
6. **Don't define what "done" looks like** -- no reference point, every session is a fresh design exercise
7. **Build against incomplete gallery data** -- UI looks fine with test data, breaks with nulls and missing fields
8. **Only test with empty perch-config** -- empty and populated states are two different apps
9. **Bolt a full desktop UI onto a CLI-first tool** -- scope grew but development approach didn't
10. **Wire up UI elements that don't do anything** -- deploy bars, drop zones, YAML editors that silently do nothing

### Half-Baked Features Audit

| Feature | Issue |
|---|---|
| Drag-drop zones (Apps, Dotfiles) | Logs only, never links |
| YAML editor toggle | Read-only, no save |
| Deploy bars (Dotfiles, Tweaks) | No deploy command wired |
| Apply Tweaks in main app | Doesn't exist outside wizard |
| Add to Startup UI | Command exists, no UI entry point |
| Alternatives section | Binds DisplayName which is null |
| Gamer/Casual profiles | Almost no matching catalog entries |
| Catalog error states | Silent blank screen on failure |
| Wizard MinHeight > Height | Layout typo (800 vs 720) |

## Phase 2: Five Whys -- Root Causes

### Thread A: "Things disappear after working"
**Root cause:** No living spec for the desktop app + no ViewModel tests = invisible regression surface. Manual testing doesn't catch what automated tests would.

### Thread B: "Concurrent sessions cause confusion"
**Root cause:** No feature status tracker that survives across sessions. The codebase is the only source of truth, but it doesn't self-document what's done vs. what's a stub.

### Thread C: "Don't know what the end result should be"
**Root cause:** Gallery data layer and UI layer are co-dependent but built independently. Can't finish the UI without the data; data work feels low-priority until the UI looks broken.

## Phase 3: First Principles -- Rebuild

### Testing Priority: GalleryDetectionService, not ViewModels
The three-way join (system + gallery + config) determines what every screen shows. Current gaps:
- `DetectAppsAsync` -- 0 tests
- `DetectFontsAsync` -- 0 tests
- `DetectTweaksAsync` -- 0 tests
- `IsDrift` branch -- 0 tests
- Empty config path -- 0 tests

Core StatusService and DeployService are well-tested (two-way) but never exercise gallery involvement.

### Gallery and UI Must Co-Evolve
Gallery IS the UI content. Build them together: logos, descriptions, profile mappings, display names.

### Desktop App Needs Its Own Identity
Not "CLI with a window" -- a first-class product with its own spec, test strategy, and definition of done.

## Phase 4: Solution Matrix -- Action Plan

### Decisions Made

1. **Desktop App spec** lives at `_bmad-output/planning-artifacts/desktop-spec.md`, covers wizard + all pages together
2. **Gallery "complete entry"** schema: name, displayName, category, description, tags, install, profile mapping, logo all required
3. **Logo strategy:** Standalone hybrid script (winget metadata -> choco API -> local .exe -> generated avatar). Not part of Perch CLI.
4. **Dead UI:** Delete entirely (drag-drop, YAML editor, deploy bars, Apply Tweaks, Add to Startup)
5. **Session discipline:** Named worktrees (`perch-{feature}`), scoped to non-overlapping areas, frequent merge to master
6. **Wizard:** Part of the desktop spec, not separate -- wizard and pages share components
7. **Test fixtures:** Unit tests with mocks only. Defer rich sample-config-repo until gallery format stabilizes.
8. **FlaUI:** Skip. Low value for this project.
9. **smoke-test.ps1:** Add to CI Windows job.

### Worktree Convention

1. Create from master: `git worktree add ../perch-{feature} -b {feature}`
2. Work, commit often
3. Frequently merge master into feature branch
4. When done, merge to master, delete worktree
5. No scope overlap between active worktrees

### Execution Sequence

| Phase | Actions |
|---|---|
| **Now** | GalleryDetectionService tests + Add smoke-test.ps1 to CI |
| **Next** | Desktop App spec (wizard + all pages, states, what's wired) |
| **Then** | Kill dead UI + gallery entry schema definition |
| **After** | Visual/UX brainstorm + logo enrichment script |
| **Ongoing** | Desktop service tests + ViewModel state tests |

### Dependency Map

```
Desktop App spec
  ├── unlocks: Kill dead UI
  ├── unlocks: ViewModel tests
  └── unlocks: Visual/UX brainstorm

GalleryDetectionService tests -- standalone, start now

Gallery entry schema
  ├── unlocks: Logo enrichment script
  └── unlocks: Visual/UX brainstorm

smoke-test.ps1 to CI -- standalone, quick win
```
