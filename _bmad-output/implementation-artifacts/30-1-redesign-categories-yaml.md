# Story 30.1: Redesign categories.yaml with new 8-category taxonomy

Status: ready-for-dev

## Story

As a **Perch gallery maintainer**,
I want **a redesigned categories.yaml file with 8 top-level categories, 3-level nesting support, pattern fields, and sort definitions**,
so that **the WPF Desktop app can render the correct navigation structure, apply drill-down vs direct patterns, and sort items correctly without hardcoded C# dictionaries**.

## Acceptance Criteria

1. **New 8-category taxonomy** - categories.yaml contains exactly these 8 top-level categories: Languages, Terminal, Development, Essentials, Media, Gaming, Power User, Companion Tools
2. **3-level nesting support** - Schema supports `Category/Subcategory/SubSubcategory` paths (e.g., `Languages/.NET/Package Managers`)
3. **Pattern field** - Each category specifies `pattern: drill-down` or `pattern: direct` to control UI rendering behavior
4. **Sort fields at every level** - Every category, subcategory, and sub-subcategory has a `sort` integer field for ordering
5. **Subcategory definitions** - All subcategories from the brainstorming session are defined with correct hierarchy
6. **Backward compatibility preserved** - Existing YAML parsing code in Perch can still read the file (new fields are additive)
7. **No breaking changes to existing entries** - Category paths in app entries will be updated in Story 30-3; this story only changes the categories.yaml structure

## Tasks / Subtasks

- [ ] **Task 1: Create new categories.yaml structure** (AC: 1, 2, 3, 4, 5)
  - [ ] 1.1 Define Languages category with drill-down pattern and all language subcategories (.NET, Node, Python, Rust, Go, Ruby, Java, C/C++, PHP) with nested children (Runtimes, Version Managers, Package Managers, IDEs, Global Packages)
  - [ ] 1.2 Define Terminal category with direct pattern and subcategories (Shells/Bash, Shells/PowerShell, Terminal Apps, CLI Tools, Git, SSH)
  - [ ] 1.3 Define Development category with direct pattern and subcategories (IDEs & Editors, API Tools, Databases, Containers, Diff Tools)
  - [ ] 1.4 Define Essentials category with drill-down pattern and subcategories (Browsers, Communication, Passwords, Office, Note-Taking, Cloud Storage, Compression, Screenshots, Clipboard, PDF, Downloads, FTP, Window Managers)
  - [ ] 1.5 Define Media category with direct pattern and subcategories (Graphics, Video, Audio, Other)
  - [ ] 1.6 Define Gaming category with direct pattern and subcategories (Launchers, Controllers, Modding, Streaming, Performance)
  - [ ] 1.7 Define Power User category with direct pattern and subcategories (File Management, System Monitors, System Tools, Networking)
  - [ ] 1.8 Define Companion Tools category with direct pattern (flat list)
  - [ ] 1.9 Assign sort values at all levels ensuring logical ordering

- [ ] **Task 2: Preserve existing tweak categories** (AC: 6)
  - [ ] 2.1 Keep all existing tweak categories (Appearance, Explorer, Taskbar, Privacy, Search, Input, Accessibility, Power, Performance, System) with their current structure
  - [ ] 2.2 Keep Fonts category intact

- [ ] **Task 3: Validate YAML syntax and structure** (AC: 6, 7)
  - [ ] 3.1 Validate YAML parses correctly
  - [ ] 3.2 Verify all sort values are unique within their sibling level
  - [ ] 3.3 Confirm no duplicate category names at any level

## Dev Notes

### Key Design Decisions

1. **Drill-down vs Direct Pattern**
   - `pattern: drill-down` means the UI shows category cards that expand to subcategory views (Languages, Essentials)
   - `pattern: direct` means the UI shows all apps in a flat list grouped by subcategory (Development, Media, Gaming, etc.)
   - This pattern field will be consumed by the WPF navigation code in later stories (30-7, 30-8)

2. **Sort Priority Logic**
   - Sort values should use gaps (10, 20, 30...) to allow future insertions
   - Lower sort values appear first
   - Sub-categories inherit parent sort unless overridden (existing behavior)

3. **Tweak Categories Preserved**
   - The current categories.yaml contains both app categories AND tweak categories
   - Tweak categories (Appearance, Explorer, Taskbar, Privacy, etc.) remain unchanged
   - Only app-related categories are redesigned

### Schema Changes from Current State

**Current structure:**
```yaml
Development:
  sort: 100
  children:
    IDEs: { sort: 10 }
    Editors: { sort: 20 }
```

**New structure (adds pattern):**
```yaml
Development:
  sort: 300
  pattern: direct
  children:
    IDEs & Editors:
      sort: 10
      children:
        IDEs: { sort: 10 }
        Editors: { sort: 20 }
```

### Target File

**Repository:** perch-gallery (NOT Perch)
**File:** `perch-gallery/catalog/categories.yaml`
**Dist file:** Will be copied to `perch-gallery/dist/catalog/categories.yaml` by existing build process

### Final 8-Category Taxonomy Reference

| # | Category | Pattern | Sort | Subcategories |
|---|----------|---------|------|---------------|
| 1 | Languages | drill-down | 100 | .NET, Node, Python, Rust, Go, Ruby, Java, C/C++, PHP (each with Runtimes, Version Managers, Package Managers, IDEs, Global Packages) |
| 2 | Terminal | direct | 200 | Shells, Terminal Apps, CLI Tools, Git, SSH |
| 3 | Development | direct | 300 | IDEs & Editors, API Tools, Databases, Containers, Diff Tools |
| 4 | Essentials | drill-down | 400 | Browsers, Communication, Passwords, Office, Note-Taking, Cloud Storage, Compression, Screenshots, Clipboard, PDF, Downloads, FTP, Window Managers |
| 5 | Media | direct | 500 | Graphics, Video, Audio, Other |
| 6 | Gaming | direct | 600 | Launchers, Controllers, Modding, Streaming, Performance |
| 7 | Power User | direct | 700 | File Management, System Monitors, System Tools, Networking |
| 8 | Companion Tools | direct | 800 | (flat list) |

### Project Structure Notes

- **This story modifies perch-gallery repo, NOT Perch repo**
- The Perch Desktop app loads categories.yaml from the gallery at runtime
- No C# code changes in this story - categories.yaml is pure data
- The hardcoded `_subcategoryOrder` dictionary in `AppsViewModel.cs` will be removed in Story 30-2

### Blocking Relationships

**This story blocks:**
- 30-2 (Remove hardcoded sort order from AppsViewModel)
- 30-3 (Re-categorize all 261 entries to new taxonomy)
- 30-4 (Remove kind field, add cli-tool flag)
- 30-5 (Remove hidden field)

### References

- [Source: _bmad-output/brainstorming/brainstorming-session-2026-02-20.md#Final Taxonomy]
- [Source: _bmad-output/brainstorming/brainstorming-session-2026-02-20.md#Schema Changes]
- [Source: _bmad-output/brainstorming/brainstorming-session-2026-02-20.md#Story 1]
- [Source: perch-gallery/catalog/categories.yaml] (current structure)
- [Source: Perch/src/Perch.Desktop/ViewModels/AppsViewModel.cs:24] (hardcoded _subcategoryOrder to be replaced)

## Dev Agent Record

### Agent Model Used

TBD

### Debug Log References

### Completion Notes List

### File List
