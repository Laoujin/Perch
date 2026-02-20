# Story 20.3: Gallery Language — .NET Ecosystem

Status: ready-for-dev

## Story

As a Perch Desktop user browsing the Languages page,
I want the .NET ecosystem to have complete gallery entries covering SDKs, IDEs, profilers, decompilers, global tools, and config files,
so that the .NET ecosystem detail page shows a rich, accurate picture of my .NET toolchain.

## Design Source

`_bmad-output/design-thinking-2026-02-19.md` — Screen 2 (.NET Ecosystem Detail) prototype.

## Current Gallery State

In `../perch-gallery/catalog/`:
- `apps/dotnet/dotnet-sdk.yaml` — runtime entry with suggests for 20+ tools
- `apps/dotnet/dotnet-ef.yaml`, `dotnet-format.yaml`, `dotnet-outdated.yaml`, etc. — individual tool entries
- `apps/visualstudio-2022.yaml`, `apps/visualstudio-2026.yaml` — IDE entries
- `apps/vscode.yaml` — editor entry (cross-ecosystem)

## What Needs Verification / Enrichment

### Sub-categories Expected (from design-thinking)
1. **Runtimes & SDKs**: dotnet-sdk (multiple versions), dotnet-framework
2. **Editors & IDEs**: Visual Studio 2022, Visual Studio 2026, Rider, VS Code
3. **Decompilers**: ILSpy, dnSpy, dotPeek
4. **Profilers**: dotTrace, dotMemory, PerfView
5. **Global Tools**: dotnet-ef, dotnet-format, dotnet-outdated, dotnet-aspire, etc.
6. **Configuration Files**: nuget.config, global.json

### Per-Entry Checklist
- [ ] `dotnet-sdk.yaml`: verify kind: runtime, suggests covers all sub-category tools, alternatives if any
- [ ] IDE entries: verify suggests/requires relationships, extensions (ReSharper, CodeMaid), tweaks (context menu, file associations)
- [ ] Global tools: verify install.dotnet-tool IDs, descriptions, tags
- [ ] Config files: verify/create entries with kind: dotfile, config.links with platform-specific targets
- [ ] Alternatives: Rider ↔ VS 2026 ↔ VS Code as alternatives for .NET development

## Acceptance Criteria

1. **Complete ecosystem.** The .NET runtime entry's `suggests` list references all tools that should appear in the ecosystem detail page.
2. **Sub-category coverage.** At least one entry exists per sub-category: Runtimes, IDEs, Decompilers, Profilers, Global Tools, Configuration Files.
3. **Config file entries.** `nuget.config` and `global.json` have gallery entries with kind: dotfile, config.links with correct platform targets, and are referenced in the .NET ecosystem's suggests.
4. **Alternatives set.** IDE entries have `alternatives` pointing to each other.
5. **Install IDs verified.** All entries with install commands have correct winget/choco/dotnet-tool IDs that resolve on a Windows system.
6. **Gallery index regenerated.** Running `node scripts/generate-index.mjs` produces a valid index.yaml including all new/modified entries.

## Tasks / Subtasks

- [ ] Task 1: Audit existing .NET gallery entries against sub-category checklist
- [ ] Task 2: Create missing entries (decompilers, profilers, config files) with complete metadata
- [ ] Task 3: Verify/update suggests, requires, alternatives relationships
- [ ] Task 4: Add config.links to nuget.config and global.json entries
- [ ] Task 5: Verify install IDs (winget/choco/dotnet-tool) are correct
- [ ] Task 6: Regenerate gallery index

## Constraints

- Changes are in `../perch-gallery/` repo (separate git repo).
- Follow existing YAML schema conventions (hyphenated-naming, see gallery-schema.md).
- Don't remove or rename existing entries that other repos may reference.
