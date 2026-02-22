# Story 30.2: Remove hardcoded sort order, read from categories.yaml

Status: ready-for-dev

## Story

As a **Perch Desktop developer**,
I want **to load category definitions (sort order, pattern) from categories.yaml instead of hardcoded C# dictionaries**,
so that **category structure is data-driven, maintainable in the gallery repo, and supports the new taxonomy without code changes**.

## Acceptance Criteria

1. **Categories loaded from gallery** - Perch fetches and parses `categories.yaml` from the gallery (via existing `ICatalogFetcher`)
2. **YAML model created** - New `CategoryDefinitionYamlModel` class handles deserialization of the new schema (sort, pattern, children)
3. **Domain model created** - New `CategoryDefinition` record exposes `Sort`, `Pattern`, and nested `Children`
4. **ICatalogService extended** - New method `GetCategoriesAsync()` returns parsed category definitions
5. **Hardcoded dictionary removed** - `_subcategoryOrder` dictionary deleted from `AppsViewModel.cs`
6. **Sort order uses YAML data** - `GetSubCategoryPriority` reads sort values from loaded categories
7. **Pattern field accessible** - The `pattern` field (drill-down/direct) is parsed and available for later stories (30-7, 30-8)
8. **Backward compatible** - If categories.yaml is missing or malformed, fall back gracefully (log warning, use alphabetical sort)
9. **Caching** - Categories are cached like other catalog data (via `ICatalogCache`)

## Tasks / Subtasks

- [ ] **Task 1: Create YAML model for categories** (AC: 2)
  - [ ] 1.1 Add `CategoryDefinitionYamlModel` to `CatalogYamlModels.cs` with `Sort`, `Pattern`, `Children` properties
  - [ ] 1.2 Handle recursive children structure (categories can nest 3 levels deep)

- [ ] **Task 2: Create domain model for categories** (AC: 3)
  - [ ] 2.1 Create `CategoryDefinition.cs` in `Perch.Core/Catalog/` with `sealed record CategoryDefinition(string Name, int Sort, string? Pattern, ImmutableDictionary<string, CategoryDefinition> Children)`
  - [ ] 2.2 Pattern values: `"drill-down"` or `"direct"` (null defaults to direct)

- [ ] **Task 3: Add parsing logic** (AC: 2, 3)
  - [ ] 3.1 Add `ParseCategories(string yaml)` method to `CatalogParser`
  - [ ] 3.2 Recursively convert YAML model to domain model
  - [ ] 3.3 Return `ImmutableDictionary<string, CategoryDefinition>` keyed by category name

- [ ] **Task 4: Extend ICatalogService** (AC: 4, 9)
  - [ ] 4.1 Add `Task<ImmutableDictionary<string, CategoryDefinition>> GetCategoriesAsync(CancellationToken)` to `ICatalogService`
  - [ ] 4.2 Implement in `CatalogService` - fetch `categories.yaml`, parse, cache result
  - [ ] 4.3 Add `_categories` field for caching, include in `InvalidateAll()`
  - [ ] 4.4 Update `NoOpCatalogService` with empty implementation

- [ ] **Task 5: Update AppsViewModel to use categories** (AC: 5, 6, 7)
  - [ ] 5.1 Inject `ICatalogService` into `AppsViewModel` constructor
  - [ ] 5.2 Delete `_subcategoryOrder` static dictionary (lines 24-33)
  - [ ] 5.3 Add `_categories` field to store loaded categories
  - [ ] 5.4 Load categories in `RefreshAsync` before building the view
  - [ ] 5.5 Rewrite `GetSubCategoryPriority` to look up sort from `_categories`

- [ ] **Task 6: Handle fallback gracefully** (AC: 8)
  - [ ] 6.1 If categories.yaml fetch/parse fails, log warning and continue
  - [ ] 6.2 `GetSubCategoryPriority` returns `int.MaxValue` if category not found (existing behavior)
  - [ ] 6.3 Pattern defaults to `"direct"` if not specified

- [ ] **Task 7: Write tests** (AC: 1-9)
  - [ ] 7.1 Unit test for `CatalogParser.ParseCategories` with nested structure
  - [ ] 7.2 Unit test for sort value resolution with 3-level nesting
  - [ ] 7.3 Integration test for `CatalogService.GetCategoriesAsync`
  - [ ] 7.4 ViewModel test verifying categories are loaded and sort works

## Dev Notes

### Code to Delete

**AppsViewModel.cs lines 24-33** - the hardcoded dictionary:
```csharp
private static readonly Dictionary<string, string[]> _subcategoryOrder = new(StringComparer.OrdinalIgnoreCase)
{
    ["Development"] = ["IDEs", "Editors", "Languages", ...],
    ["Gaming"] = ["Stores", "Launchers", ...],
    // etc.
};
```

### New Domain Model

```csharp
// Perch.Core/Catalog/CategoryDefinition.cs
public sealed record CategoryDefinition(
    string Name,
    int Sort,
    string? Pattern,  // "drill-down" or "direct" (null = direct)
    ImmutableDictionary<string, CategoryDefinition> Children);
```

### YAML Schema (from 30-1)

```yaml
Languages:
  sort: 100
  pattern: drill-down
  children:
    .NET:
      sort: 10
      children:
        Runtimes: { sort: 10 }
        Package Managers: { sort: 20 }
```

### Lookup Logic

To get sort for `"Languages/.NET/Package Managers"`:
1. Split path by `/`
2. Look up `"Languages"` in root → `Sort: 100`
3. Look up `".NET"` in Languages.Children → `Sort: 10`
4. Look up `"Package Managers"` in .NET.Children → `Sort: 20`
5. Return the leaf sort value (20)

For subcategory ordering within a category, compare the `Sort` values of siblings.

### Integration with Existing Code

- `ICatalogFetcher` already fetches from gallery - reuse for `categories.yaml`
- `ICatalogCache` already caches YAML content - reuse
- `CatalogParser` already has static `IDeserializer` - reuse
- Pattern follows existing `GetIndexAsync`, `GetGitHubStarsAsync` methods

### Files to Modify

| File | Changes |
|------|---------|
| `Perch.Core/Catalog/CatalogYamlModels.cs` | Add `CategoryDefinitionYamlModel` |
| `Perch.Core/Catalog/CategoryDefinition.cs` | New file - domain record |
| `Perch.Core/Catalog/CatalogParser.cs` | Add `ParseCategories` method |
| `Perch.Core/Catalog/ICatalogService.cs` | Add `GetCategoriesAsync` |
| `Perch.Core/Catalog/CatalogService.cs` | Implement `GetCategoriesAsync` |
| `Perch.Core/Catalog/NoOpCatalogService.cs` | Add empty implementation |
| `Perch.Desktop/ViewModels/AppsViewModel.cs` | Remove dict, inject service, use categories |
| `tests/Perch.Core.Tests/Catalog/CatalogParserTests.cs` | Add category parsing tests |
| `tests/Perch.Desktop.Tests/AppsViewModelTests.cs` | Add/update tests (if exists) |

### Project Structure Notes

- New `CategoryDefinition.cs` goes in `Perch.Core/Catalog/` alongside other domain records
- All changes are in Perch repo (not perch-gallery)
- Desktop ViewModel tests go in `Perch.Desktop.Tests`

### Dependencies

- **Blocked by:** Story 30-1 (categories.yaml must exist with new schema)
- **Blocks:** Story 30-6 (add sort field to entries), Story 30-7 (drill-down pattern for Essentials)

### References

- [Source: _bmad-output/brainstorming/brainstorming-session-2026-02-20.md#Story 2]
- [Source: Perch/src/Perch.Desktop/ViewModels/AppsViewModel.cs:24-33] (code to delete)
- [Source: Perch/src/Perch.Desktop/ViewModels/AppsViewModel.cs:313-320] (GetSubCategoryPriority to rewrite)
- [Source: Perch/src/Perch.Core/Catalog/CatalogService.cs] (pattern to follow)
- [Source: Perch/src/Perch.Core/Catalog/CatalogParser.cs] (add parsing here)
- [Source: _bmad-output/project-context.md] (coding conventions)

## Dev Agent Record

### Agent Model Used

TBD

### Debug Log References

### Completion Notes List

### File List
