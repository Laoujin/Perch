# Story 20.1: Languages Page Scaffold

Status: review

## Story

As a Perch Desktop user,
I want the Languages page to match the design-thinking wireframes with collapsible sub-categories, sub-category status badges, and language-owned config files section,
so that I can browse language ecosystems and manage their full toolchains in a unified experience.

## Design Source

`_bmad-output/design-thinking-2026-02-19.md` — Languages Page prototype (Screen 1-3), unified architecture, Priority 1 action items.

## Current State

The Languages page already has:
- Ecosystem grid with `EcosystemCard` in WrapPanel (Screen 1)
- Ecosystem detail with sub-category sections and AppCards (Screen 2)
- Item detail with status, description, links, alternatives (Screen 3)
- Status summary badges (synced/drifted/detected pill counts)
- Sub-category sort order: IDEs (0), Runtimes (1), Decompilers (2), Profilers (3), IDE Extensions (4), CLI Tools (5), Others (10)

## What Changes

### Ecosystem Grid (Screen 1)
- Verify sort: ecosystems with drift first, then detected, then synced, then unmanaged
- Verify aggregate status badges on ecosystem cards show only non-zero counts

### Ecosystem Detail (Screen 2)
- Add collapsible sub-categories — sub-category headers act as expanders, collapsed by default for sections with 0 drifted/detected items and >5 items
- Add aggregate status badges on sub-category headers (e.g., "Runtimes & SDKs ── 1 drifted  2 synced")
- Add "Configuration Files" sub-category: language-owned dotfiles (.npmrc, nuget.config, global.json, bunfig.toml) from gallery entries with `kind: dotfile` that belong to this ecosystem. These show as AppCards with no gear icon (no drill-down). This ensures language-specific dotfiles appear HERE and not on the Dotfiles page.
- Verify secondary sort within sub-category: status (Drifted -> Detected -> Synced -> unmanaged), then gallery sort index

### Gear Icon (Screen 2 + 3)
- Gear icon visibility on AppCards: only show for entries that have tweaks, extensions, or a meaningful detail (requires, alternatives, config.links). Simple items (global CLI tools with no config) hide the gear — the card is the complete experience.
- Depends on 22-4 (inline expand) for the gear to actually do something. Until then, gear icon signals "detail available" but navigates to the existing Screen 3.

## Acceptance Criteria

1. **Ecosystem grid sort.** Ecosystems sort: those with Drifted items first, then Detected, then Synced, then unmanaged. Within each tier, alphabetical.
2. **Collapsible sub-categories.** Sub-category sections have an expand/collapse header. Default state: expanded if section has drifted or detected items, collapsed otherwise (for sections with >5 items).
3. **Sub-category status badges.** Each sub-category header shows aggregate synced/drifted/detected pill counts (hidden when zero).
4. **Config files sub-category.** Language-owned dotfiles (kind: dotfile entries that are in this ecosystem's suggests or that have a category under `Languages/*`) appear in a "Configuration Files" sub-category at the bottom of the ecosystem detail.
5. **Gear icon conditional.** Gear/configure button only shows on cards that have tweaks, extensions, requires, suggests, alternatives, or config.links. Pure CLI tools with only an install ID hide the gear.
6. **All existing tests pass.** No regressions.

## Tasks / Subtasks

- [x] Task 1: Ecosystem grid sort (AC: #1)
  - [x] Verify/fix sort in `LanguagesViewModel` — ecosystems with Drifted counts first, then Detected, then Synced, then all unmanaged
  - [x] Add test for ecosystem sort order

- [x] Task 2: Collapsible sub-categories (AC: #2)
  - [x] Wrap each sub-category section in an `Expander` or similar collapse control
  - [x] Default state: expanded if section has any drifted/detected items, collapsed if all synced/unmanaged and count > 5
  - [x] Add `IsExpanded` property per sub-category group in ViewModel

- [x] Task 3: Sub-category status badges (AC: #3)
  - [x] Add synced/drifted/detected counts per sub-category group
  - [x] Add pill badge elements in sub-category header template
  - [x] Wire counts from grouped AppCardModels

- [x] Task 4: Config files sub-category (AC: #4)
  - [x] In ecosystem detail loading, collect language-owned dotfiles (kind: dotfile entries related to this ecosystem)
  - [x] Add them as a "Configuration Files" sub-category group (sort order: last)
  - [x] These cards show no gear icon — Add/Remove is the only action

- [x] Task 5: Gear icon conditional (AC: #5)
  - [x] Add `HasDetailPage` computed property to `AppCardModel` based on catalog entry richness
  - [x] Bind gear/configure button visibility to `HasDetailPage`

- [x] Task 6: Tests (AC: #6)
  - [x] Test ecosystem sort order
  - [x] Test sub-category badge counts
  - [x] Test config files sub-category population
  - [x] Test HasDetailPage logic
  - [x] Build passes with zero warnings

## Files to Modify

| File | Change |
|------|--------|
| `src/Perch.Desktop/ViewModels/LanguagesViewModel.cs` | Ecosystem sort, sub-category badges, config files group, collapsible state |
| `src/Perch.Desktop/Views/Pages/LanguagesPage.xaml` | Collapsible sub-categories, sub-category header badges, gear icon binding |
| `src/Perch.Desktop/Models/AppCardModel.cs` | Add `HasDetailPage` computed property |
| `src/Perch.Desktop/Models/EcosystemCardModel.cs` | Verify sort order accounts for drift/detected/synced priority |

## Dependencies

- **22-5 (AppCard finish)** — uses the new action button + 5-state badge model (already in review)
- **22-4 (App detail page)** — gear icon navigates to inline expand when 22-4 lands; until then, uses existing Screen 3

## Constraints

- **No new NuGet packages.**
- **Win10 safe.** Hardcoded opaque colors, no DynamicResource theme brushes for backgrounds.
- Gallery content (20-3 through 20-9) is separate — this story is about the page scaffold, not gallery YAML entries.

## Dev Agent Record

### Implementation Plan
- Task 1: Added `EcosystemStatusPriority` method + `OrderBy`/`ThenBy` in `ApplyFilter()` for status-priority + alphabetical sort
- Task 2+3: Enhanced `AppCategoryGroup` with `SyncedCount`, `DriftedCount`, `DetectedCount`, `IsExpanded` properties; replaced XAML `StackPanel` sub-category headers with `Expander` controls containing status pill badges
- Task 4: In `RebuildSubCategories`, dotfile entries (kind=Dotfile) are filtered out of regular groups and collected into a "Configuration Files" group at sort order 99; secondary sort within each group by status then name
- Task 5: Added `HasDetailPage` computed property to `AppCardModel` checking tweaks, extensions, requires, suggests, alternatives, config; added `HasDetailPage` DP to `AppCard` control; bound gear icon visibility

### Completion Notes
All 6 tasks implemented and tested. 14 new tests in `LanguagesViewModelTests.cs` covering ecosystem sort, sub-category badges, config files grouping, HasDetailPage logic, and collapsible state. Full suite: 956 tests pass, zero warnings.

## File List

| File | Action |
|------|--------|
| `src/Perch.Desktop/ViewModels/LanguagesViewModel.cs` | Modified — ecosystem sort, sub-category secondary sort, config files group, dotfile filtering |
| `src/Perch.Desktop/Views/Pages/LanguagesPage.xaml` | Modified — Expander with status badges, HasDetailPage binding on AppCards |
| `src/Perch.Desktop/Models/AppCardModel.cs` | Modified — Added `HasDetailPage` computed property |
| `src/Perch.Desktop/Models/AppCategoryGroup.cs` | Modified — Added status counts + `IsExpanded` |
| `src/Perch.Desktop/Views/Controls/AppCard.xaml` | Modified — Gear icon visibility bound to `HasDetailPage` |
| `src/Perch.Desktop/Views/Controls/AppCard.xaml.cs` | Modified — Added `HasDetailPage` DependencyProperty |
| `tests/Perch.Desktop.Tests/LanguagesViewModelTests.cs` | Added — 14 tests for all ACs |

## Change Log

- 2026-02-20: Implemented Languages Page Scaffold — ecosystem sort, collapsible sub-categories with status badges, config files sub-category, gear icon conditional visibility, 14 tests added
