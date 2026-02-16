---
title: 'Perch Full-Scope -- Gallery/Config Reconciliation & Ecosystem Build'
slug: 'perch-full-scope-reconciliation'
created: '2026-02-16'
status: 'ready-for-dev'
stepsCompleted: [1, 2, 3, 4]
tech_stack: ['.NET 10', 'C# latest', 'YamlDotNet 16.3', 'Spectre.Console 0.54', 'Avalonia 11.2', 'NUnit 4.4', 'NSubstitute 5.3', 'Roslynator', 'GitHub Pages']
files_to_modify: ['Perch.Core/Templates/TemplateProcessor.cs', 'Perch.Core/Templates/IReferenceResolver.cs', 'Perch.Core/Git/CleanFilterService.cs', 'Perch.Core/Catalog/CatalogParser.cs', 'Perch.Core/Packages/PackageManifestParser.cs', 'Perch.Core/Modules/ManifestParser.cs', 'Perch.Core/Modules/ManifestYamlModel.cs', 'Perch.Core/Modules/ModuleDiscoveryService.cs', 'Perch.Core/Deploy/DeployService.cs', 'Perch.Core/Config/PerchSettings.cs', 'Perch.Core/ServiceCollectionExtensions.cs', 'Perch.Cli/Commands/FilterCommand.cs (new)', 'Perch.Core/Git/SubmoduleService.cs (new)', 'perch-gallery/catalog/apps/*.yaml', 'perch-gallery/src/ (new Astro site)', 'perch-gallery/.github/workflows/deploy.yml', 'perch-config/packages.yaml (retire)', 'perch-config/install.yaml (new)', 'perch-config/git/ (replace with submodule)', 'perch-config/bun-packages/ (retire)']
code_patterns: ['Immutable records for data', 'Interface-based platform abstraction', 'DI via AddPerchCore()', 'HyphenatedNamingConvention YAML', 'Results as records not exceptions', 'IProgress<T> streaming', 'CancellationToken threading']
test_patterns: ['NUnit 4.4 [TestFixture] sealed class', 'NSubstitute Substitute.For<T>()', 'Assert.That() fluent syntax', 'Assert.Multiple() grouping', 'Temp dir with try/finally cleanup', '[SetUp] for mock initialization']
---

# Tech-Spec: Perch Full-Scope -- Gallery/Config Reconciliation & Ecosystem Build

**Created:** 2026-02-16

## Overview

### Problem Statement

Three disconnected data sources (perch-gallery, perch-config, dotfiles-legacy) with overlapping formats, no authority hierarchy, and an MVP PRD that doesn't cover the full vision. packages.yaml duplicates gallery catalog data. Config modules mix generic app definitions with personal overrides. No templating for machine-specific configs, no change-ignore mechanism for volatile file sections, and no website for browsing the gallery.

### Solution

Establish perch-gallery as the single source of truth for app definitions, install metadata, and starter templates. Redesign perch-config to contain only personal overrides and file content. Add a template engine for machine-specific generated files. Implement change-ignore for volatile config sections. Build a simple one-page gallery website.

### Scope

**In Scope:**

1. **Gallery as sole source of truth** -- retire packages.yaml from perch-config, absorb apps from dotfiles-legacy into gallery catalog
2. **Gallery/config authority split** -- gallery = generic app definitions + starter templates; config = personal overrides and actual config file content
3. **Gallery starter template repos** (e.g., `perch-gallery-git` with default .gitconfig template containing user.name/email placeholders)
4. **Template engine** for generated files (e.g., `.machineName.gitconfig` generated from template + machine variables; `.gitconfig` symlinked to machine-specific generated file)
5. **Git-Config submodule** -- `Laoujin/Git-Config` replaces `git/` folder in perch-config; add manifest.yaml to that repo
6. **Change-ignore mechanism** -- ignore volatile fields in symlinked files (window positions, recent files, last-opened dates) for git cleanliness
7. **Registry key read/capture** -- read current registry state and write to config manifest for versioning
8. **Node ecosystem** -- nvm as the app for node version management; pnpm/yarn as installable node packages with user choice
9. **Bun as standalone app** -- bun has native installer, independent of node; retire nonsensical `bun-packages` module
10. **Extend gallery from external sources** -- import app definitions from ninite, ChrisTitusTech/winutil JSON files; build conversion tooling (JSON → YAML)
11. **App logos** in gallery entries
12. **Gallery website** -- simple one-page site (forest green theme from UX docs, browse/download YAML files); informed stack choice for future extensibility

**Out of Scope:**

- PS-modules modernization (separate research)
- MAUI desktop app
- AI app discovery / Windows Sandbox
- Migration tools (chezmoi/dotbot import/export)
- Secrets management (1Password integration)
- Full marketing site (one-pager only for now)

### Priority Tiers

| Tier | Meaning | Streams / Tasks |
|------|---------|-----------------|
| **P0** | Core engine changes -- must ship first | B1-B4 (template engine), C1-C3 (clean filters), E0 (merge design), E1-E2 (catalog parser + settings) |
| **P1** | Gallery content + config restructuring -- builds on P0 | A1-A4 (gallery schema + content), D1-D4 (config restructuring), E3-E5 (overlay + install + registry capture) |
| **P2** | Polish, bulk content, website -- ships last | A5-A7 (converters + logos + bulk curation), F1-F3 (gallery website), G1 (Scoop importer) |

Key blockers within tiers: B1-B2 before B3-B4. E0 (merge design) before E3. A1 before E1. D3 after E4.

### General Error Policy

Non-critical errors (missing gallery entry for an app, unknown template variable, registry key not found) produce a **warning** and continue. The deploy completes with a summary of warnings at the end.

Critical errors (missing config repo, unparseable manifest YAML, gallery URL unreachable with no cache) produce an **error** and abort the current operation.

No strict mode. No silent skips. Every anomaly is surfaced.

### Migration Strategy

Clean break -- no dual-format support. When upgrading Perch to the new engine, the config repo must be updated simultaneously. Document the migration steps in the Perch README. This is acceptable because Perch is single-user.

## Context for Development

### Codebase Patterns

- **Solution structure:** `Perch.Core` (136 .cs files), `Perch.Cli` (Spectre.Console commands), `Perch.Desktop` (Avalonia, future), `Perch.Core.Tests` (44 test files)
- **DI registration:** `ServiceCollectionExtensions.AddPerchCore()` wires all services; platform-specific via `OperatingSystem.IsWindows()` checks
- **Deploy pipeline:** `DeployService.DeployAsync()` → Discover modules → Filter (platform, machine profile, enabled) → Snapshot → Per-module execution (hooks → links → registry → packages → extensions → ps-modules) → System packages → Return exit code
- **Manifest parsing:** `ManifestParser` uses YamlDotNet with `HyphenatedNamingConvention`, `IgnoreUnmatchedProperties`; returns `ManifestParseResult` (success/error)
- **Key models (all immutable records):** `AppManifest`, `AppModule`, `LinkEntry`, `DeployResult`, `DiscoveryResult`, `MachineProfile`, `PerchSettings`
- **Platform abstraction:** `ISymlinkProvider`, `IRegistryProvider`, `IFileLockDetector`, `IPackageManagerProvider` with Windows/Unix implementations
- **Template system (exists, limited):** `TemplateProcessor` finds `{{op://...}}` patterns, resolves via `IReferenceResolver` → `OnePasswordResolver`. Extensible to general templates.
- **Clean filter (model exists, needs wiring):** `CleanFilterDefinition` with `Name`, `Script`, `Files` fields in manifest schema. `CleanFilterService` exists.
- **Machine profiles:** `MachineProfile` has `IncludeModules`, `ExcludeModules`, `Variables` (ImmutableDictionary) -- ready for template variables
- **Environment expansion:** `EnvironmentExpander.Expand()` resolves `%VAR%` (Windows) and `$VAR` (Unix) in target paths
- **Gallery parsing:** `CatalogParser` exists in Core -- partially built for reading gallery YAML
- **Config repo convention:** one folder per app module, each with `manifest.yaml` + actual config files
- **Gallery catalog format:** YAML with `name`, `category`, `tags`, `description`, `links` (website/docs/github), `install` (winget/choco), `config.links` (symlink mappings per platform)
- **Legacy format (reference only):** JSON with `requires`, `modules.Create-Links` array using path variables

### Files to Reference

| File | Purpose |
| ---- | ------- |
| **Perch.Core -- Key Services** | |
| `src/Perch.Core/Deploy/DeployService.cs` | Main deploy orchestrator |
| `src/Perch.Core/Modules/ModuleDiscoveryService.cs` | Module discovery from config repo |
| `src/Perch.Core/Modules/ManifestParser.cs` | YAML manifest deserialization + validation |
| `src/Perch.Core/Modules/ManifestYamlModel.cs` | YAML model (links, hooks, clean-filter, registry, packages, extensions) |
| `src/Perch.Core/Templates/TemplateProcessor.cs` | Template placeholder detection + replacement |
| `src/Perch.Core/Templates/IReferenceResolver.cs` | Reference resolution interface (currently 1Password) |
| `src/Perch.Core/Git/CleanFilterService.cs` | Git clean filter setup (change-ignore) |
| `src/Perch.Core/Catalog/CatalogParser.cs` | Gallery catalog reading |
| `src/Perch.Core/Packages/PackageManifestParser.cs` | packages.yaml parsing |
| `src/Perch.Core/Symlinks/SymlinkOrchestrator.cs` | Symlink/junction creation with backup |
| `src/Perch.Core/Modules/EnvironmentExpander.cs` | `%VAR%` / `$VAR` path expansion |
| `src/Perch.Core/Machines/MachineProfile.cs` | Machine-specific variables + module include/exclude |
| `src/Perch.Core/ServiceCollectionExtensions.cs` | DI wiring for all services |
| **Perch.Core -- Models** | |
| `src/Perch.Core/Modules/AppManifest.cs` | Parsed module definition (record) |
| `src/Perch.Core/Modules/AppModule.cs` | Discovered module in filesystem (record) |
| `src/Perch.Core/Modules/LinkEntry.cs` | Symlink definition with platform targets |
| `src/Perch.Core/Deploy/DeployResult.cs` | Single action result (record) |
| `src/Perch.Core/Deploy/DeployOptions.cs` | DryRun, Progress, BeforeModule callback |
| **Perch.Cli** | |
| `src/Perch.Cli/Program.cs` | CLI entry: deploy, status, apps, git setup, diff, restore, completion |
| `src/Perch.Cli/Commands/DeployCommand.cs` | Deploy command with --config-path, --dry-run, --output, --interactive |
| **Gallery** | |
| `perch-gallery/catalog/index.yaml` | Master registry (10 apps, 5 fonts, 15+ tweaks) |
| `perch-gallery/catalog/apps/*.yaml` | App definitions with install IDs + config mappings |
| `perch-gallery/catalog/tweaks/*.yaml` | Registry tweak definitions |
| `perch-gallery/.github/workflows/deploy.yml` | GitHub Pages deploy (raw files only) |
| **Config** | |
| `perch-config/packages.yaml` | 109 winget packages (to be retired) |
| `perch-config/*/manifest.yaml` | 13 module manifests (git, vscode, powershell, etc.) |
| `perch-config/git/` | 7 git config files (to be replaced by Git-Config submodule) |
| `perch-config/bun-packages/manifest.yaml` | Disabled bun global packages (to be retired/restructured) |
| **Legacy (reference)** | |
| `dotfiles-legacy/config/Programs/*.json` | notepad++, beyondcompare, conemu, filezilla, heidisql, sublimetext |
| **External Sources (downloaded)** | |
| `Perch/_bmad-output/research/winutil-applications.json` | ~300 curated apps with winget+choco IDs |
| `Perch/_bmad-output/research/scoop-main-bucket-applist.txt` | 1,438 app names in Scoop main bucket |
| `Perch/_bmad-output/research/scoop-example-*.json` | Scoop manifest format examples (git, nodejs, vscode) |

### Technical Decisions

- **Gallery is authority:** Gallery YAML defines app identity, install IDs, default config paths. Config only overrides and provides actual file content. Gallery is served from `https://laoujin.github.io/perch-gallery/` (GitHub Pages URL). Local path override available in settings for development.
- **Template engine extension:** Extend existing `TemplateProcessor`/`IReferenceResolver` to support general variables (`{{machine.name}}`, `{{user.email}}`), not just `op://` references. `MachineProfile.Variables` already provides the data source. Template processing is **opt-in per link** via `template: true` -- files without this flag are never processed, so `{{` in non-template files (Mustache, GitHub Actions, etc.) is safe.
- **Change-ignore via git clean filters:** `CleanFilterDefinition` already in manifest model. Wire `CleanFilterService` to set up git clean/smudge filters. **Filter binary is the Perch CLI itself** (`perch filter clean <module>`) -- cross-platform by default, no external script dependency. Git invokes Perch transparently on diff/add/status.
- **Gallery schema evolution:** Add `link-type`, `platforms`, `extensions` fields to gallery YAML to close gap with manifest schema. Standardize path separators (forward slashes everywhere).
- **packages.yaml retirement (clean break):** Gallery `install` section becomes source of truth. `install.yaml` replaces `packages.yaml` with gallery app ID references + per-machine add/exclude sections. Error on missing gallery IDs. No dual-format transition period.
- **Submodule-aware deploy:** Any config module folder can optionally be a git submodule. Perch deploy auto-detects submodules and runs `git submodule init/update`. Git-Config is the first use case (`Laoujin/Git-Config` replaces `perch-config/git/`), but PowerShell, .claude, or any other module can use the same pattern. User's choice per module.
- **Gallery website stack: Astro.** Static-first, component islands for future interactivity, handles YAML data sources natively, deploys to GitHub Pages via build step.
- **Gallery/manifest merge strategy:** Requires dedicated design work (Task E0) before implementation. Key questions: subtraction syntax, array merge rules (replace vs append), conflict resolution.
- **External source import:** Build conversion scripts for WinUtil JSON → gallery YAML. Scoop as secondary source. Ninite not programmatically accessible. Curation tasks are AI-assisted where possible (batch winget ID lookups, YAML stub generation, logo sourcing).
- **Node ecosystem:** nvm is the app (gallery entry). pnpm/yarn are node packages (config choice). Bun is independent app (native installer, own gallery entry).
- **bun-packages retirement:** Restructure as `bun` app with `global-packages` in its manifest, not a standalone module.

## Implementation Plan

### Tasks

#### Stream A: Gallery Schema & Content (perch-gallery repo)

- [ ] Task A1: Evolve gallery app YAML schema `P1`
  - Files: `perch-gallery/catalog/apps/*.yaml`
  - Action: Add optional fields to app schema: `link-type` (symlink/junction, default symlink), `platforms` (array, default all), `extensions` (map of extension-type → string array). Standardize all paths to forward slashes. Add `logo` field (URL or relative path).
  - Notes: Update all 10 existing app YAMLs to use forward slashes consistently. Existing fields remain unchanged.

- [ ] Task A2: Add legacy apps to gallery catalog `P1` `[curation, AI-assisted]`
  - Files: `perch-gallery/catalog/apps/` (new files), `perch-gallery/catalog/index.yaml`
  - Action: Create gallery YAML entries for apps currently only in dotfiles-legacy: `beyondcompare.yaml`, `cmder.yaml`, `filezilla.yaml`, `heidisql.yaml`, `sublimetext.yaml`. Extract install IDs (winget/choco) and default config paths from legacy JSON files. Add to index.yaml.
  - Notes: Reference `dotfiles-legacy/config/Programs/*.json` for config path mappings. These are Windows-only apps. Use AI to batch-lookup winget/choco IDs.

- [ ] Task A3: Add missing perch-config apps to gallery `P1` `[curation, AI-assisted]`
  - Files: `perch-gallery/catalog/apps/` (new files), `perch-gallery/catalog/index.yaml`
  - Action: Create gallery entries for apps in perch-config but not in gallery: `bash.yaml`, `ditto.yaml`, `greenshot.yaml`, `visualstudio.yaml`. Ditto is registry-only (no config links).
  - Notes: `bun-packages` does NOT get a gallery entry as-is -- see Task D2.

- [ ] Task A4: Add node ecosystem apps to gallery `P1` `[curation, AI-assisted]`
  - Files: `perch-gallery/catalog/apps/` (new files), `perch-gallery/catalog/index.yaml`
  - Action: Create gallery entries for: `nvm.yaml` (CoreyButler.NVMforWindows on winget, nvm on choco; cross-platform variants), `bun.yaml` (Oven-sh.Bun on winget; native installer, independent of node).
  - Notes: pnpm/yarn are node packages, not standalone apps -- they belong in a module's `global-packages` config, not in the gallery.

- [ ] Task A5: Build WinUtil JSON → gallery YAML converter `P2`
  - Files: New script in `perch-gallery/tools/` (PowerShell or Python)
  - Action: Parse `winutil-applications.json`, map fields to gallery YAML schema (`content` → `display-name`, `description` → `description`, `link` → `links.website`, `winget` → `install.winget`, `choco` → `install.choco`, `category` → `category`). Output individual YAML files per app. Include a diff mode that compares existing gallery entries against winutil data and reports new/changed apps.
  - Notes: ~300 apps in winutil. Not all need gallery entries -- the converter should allow filtering by category or manual selection. Store at `Perch/_bmad-output/research/winutil-applications.json` as source.

- [ ] Task A6: Add logo field and collect logos `P2` `[curation, AI-assisted]`
  - Files: `perch-gallery/catalog/apps/*.yaml`, new `perch-gallery/catalog/logos/` directory
  - Action: Add `logo` field to each app YAML (relative path to SVG/PNG in `logos/` folder). Collect logos for existing apps (prioritize SVG). For new apps from Task A2-A4, source logos from official sites or icon packs.
  - Notes: Logos are optional -- apps without logos render a placeholder on the website. Use AI to source logo URLs from official sites.

- [ ] Task A7: Create gallery entries for all packages.yaml apps `P2` `[curation, AI-assisted]`
  - Files: `perch-gallery/catalog/apps/` (new files), `perch-gallery/catalog/index.yaml`
  - Action: Map all 109 packages from `perch-config/packages.yaml` to gallery entries. For each: look up winget/choco IDs, create a gallery YAML file with `name`, `description`, `install`, `category`, and `links`. Many are niche apps (HeidiSQL, Robo3T, etc.) not covered by WinUtil.
  - Notes: This is the prerequisite for D3 (retire packages.yaml). Use AI to batch-generate YAML stubs with winget/choco ID lookups. Manual review required for accuracy. Depends on A1 (schema) being done first.

#### Stream B: Template Engine Extension (Perch repo)

- [ ] Task B1: Generalize TemplateProcessor for variable patterns `P0`
  - Files: `src/Perch.Core/Templates/TemplateProcessor.cs`
  - Action: Extend the regex pattern to match general `{{variable.path}}` syntax in addition to `{{op://...}}`. The processor should detect both reference types: `op://` references route to `IReferenceResolver`, general variables route to a new `IVariableResolver` that reads from `MachineProfile.Variables` and a built-in set (machine name, user name, date, platform).
  - Notes: Keep backward compatibility with existing `op://` pattern. The `{{` delimiters are already used, so this is additive. Template processing only runs on files with `template: true` -- files without the flag are never scanned, so `{{` in non-template files is safe.

- [ ] Task B2: Create IVariableResolver and MachineVariableResolver `P0`
  - Files: New `src/Perch.Core/Templates/IVariableResolver.cs`, new `src/Perch.Core/Templates/MachineVariableResolver.cs`
  - Action: Interface: `Task<string?> ResolveAsync(string variableName, CancellationToken ct)`. Implementation reads from `MachineProfile.Variables` first, then falls back to built-ins: `machine.name` (Environment.MachineName), `user.name` (configurable), `user.email` (configurable), `platform` (current OS), `date` (ISO format). Register in DI.
  - Notes: User-configurable values (name, email) come from `MachineProfile.Variables` in the machine's YAML profile. If a MachineProfile variable shadows a built-in, the MachineProfile value wins (intentional override).

- [ ] Task B3: Integrate template processing into deploy pipeline `P0`
  - Files: `src/Perch.Core/Deploy/DeployService.cs`, `src/Perch.Core/Symlinks/SymlinkOrchestrator.cs`
  - Action: During per-module link processing, if a link has `template: true`, generate the resolved file to a well-known output location (e.g., `<config-repo>/.generated/<module>/<filename>`) and symlink THAT instead of the template source. Mark generated files in `.gitignore` of the config repo.
  - Notes: This enables the `.gitconfig` use case: template in repo → generated `.machineName.gitconfig` → symlinked to `~/.gitconfig`. Only processes links with `template: true` -- never scans file content speculatively.

- [ ] Task B4: Add `template: true` flag to manifest link entries `P0`
  - Files: `src/Perch.Core/Modules/ManifestYamlModel.cs`, `src/Perch.Core/Modules/ManifestParser.cs`, `src/Perch.Core/Modules/LinkEntry.cs`
  - Action: Add optional `template: true` boolean to `LinkYamlModel`. When set, the link source is treated as a template file to be processed through `TemplateProcessor` before symlinking. Parse into `LinkEntry` record.
  - Notes: Without `template: true`, files are symlinked directly (current behavior). This is opt-in per link. This is the mechanism that makes `{{` collisions a non-issue.

#### Stream C: Change-Ignore / Clean Filters (Perch repo)

- [ ] Task C0: Add `perch filter clean` CLI subcommand `P0`
  - Files: New `src/Perch.Cli/Commands/FilterCommand.cs`, `src/Perch.Core/Git/CleanFilterService.cs`
  - Action: New CLI command: `perch filter clean <module>`. Reads file content from stdin, applies the module's clean-filter rules (strip volatile XML elements, INI keys, etc.), writes cleaned content to stdout. This is the binary that git invokes as the clean filter -- cross-platform by default since it's the Perch .NET binary.
  - Notes: Git config entry will be: `clean = perch filter clean <module>`. The command must be fast (it runs on every git diff/add/status for filtered files).

- [ ] Task C1: Wire CleanFilterService into deploy pipeline `P0`
  - Files: `src/Perch.Core/Git/CleanFilterService.cs`, `src/Perch.Core/Deploy/DeployService.cs`
  - Action: After processing a module's links, if the module has a `clean-filter` definition, call `CleanFilterService` to register the git clean filter in the config repo's `.git/config` and `.gitattributes`. The filter command is `perch filter clean <module>` (see Task C0).
  - Notes: `CleanFilterDefinition` already has `Name`, `Files` fields. Update model: replace `Script` field with filter rules (XML element patterns, INI key patterns) that `perch filter clean` interprets directly.

- [ ] Task C2: Implement filter rule types for common volatile patterns `P0`
  - Files: `src/Perch.Core/Git/CleanFilterService.cs`, `src/Perch.Core/Git/CleanFilterDefinition.cs`
  - Action: Implement filter rule types in C#: `strip-xml-elements` (removes XML elements matching configurable patterns like `<FindHistory>`, `<Session>`, position attributes), `strip-ini-keys` (removes INI keys matching patterns). Rules are defined in gallery/manifest YAML, executed by `perch filter clean`.
  - Notes: Notepad++ is the primary use case: `config.xml` contains `<FindHistory>`, `<Session>`, window coordinates that change constantly. All filter logic is in C# -- no external scripts.

- [ ] Task C3: Add clean-filter definitions to gallery app YAMLs `P1`
  - Files: `perch-gallery/catalog/apps/notepadplusplus.yaml` (and others as needed)
  - Action: Add `clean-filter` section to gallery app definitions that have volatile config files. Example for notepad++: filter `config.xml` to strip `<FindHistory>`, `<Session>`, `<GUIConfig ... position>` elements. Rules reference the filter rule types from C2.
  - Notes: The gallery defines the default filter; config can override or extend.

#### Stream D: Config Repo Restructuring (perch-config repo)

- [ ] Task D0: Add submodule-aware deploy to Perch `P1`
  - Files: `src/Perch.Core/Modules/ModuleDiscoveryService.cs`, `src/Perch.Core/Git/SubmoduleService.cs` (new)
  - Action: During module discovery, detect if a module folder is a git submodule (check `.gitmodules` or `git submodule status`). If so, auto-run `git submodule init` + `git submodule update` before processing the module. This is a general capability -- any module folder can be a submodule.
  - Notes: Submodules are optional per module. A regular folder works identically. This enables Git-Config, PowerShell, .claude, or any other module to optionally live in a separate repo.

- [ ] Task D1: Replace git/ folder with Git-Config submodule `P1`
  - Files: `perch-config/git/` (delete), `perch-config/.gitmodules` (new/update)
  - Action: Remove `perch-config/git/` folder. Add `Laoujin/Git-Config` as a git submodule at `perch-config/git/`. In the Git-Config repo, add a `manifest.yaml` with the existing link definitions. Add a `.gitconfig.template` file with `{{user.name}}` and `{{user.email}}` placeholders.
  - Notes: First use case of submodule-aware deploy (D0). The main `.gitconfig` link uses `template: true` to generate a machine-specific version. Requires B4 (template flag).

- [ ] Task D2: Retire bun-packages, create bun module `P1`
  - Files: `perch-config/bun-packages/` (delete), `perch-config/bun/manifest.yaml` (new)
  - Action: Delete `bun-packages/` module. Create `bun/manifest.yaml` with `display-name: Bun`, `global-packages` listing (`eslint_d`, `http-server`, `rimraf`, `npm-check-updates`, `prettier`, `tsx`, `tldr`), and `manager: bun`.
  - Notes: Bun is an independent app with native installer -- it doesn't need node. The gallery entry (Task A4) defines the install method.

- [ ] Task D3: Retire packages.yaml `P1`
  - Files: `perch-config/packages.yaml` (delete), `perch-config/install.yaml` (new)
  - Action: Replace `packages.yaml` with `install.yaml` that references gallery app IDs. Format:
    ```yaml
    apps:
      - git
      - vscode
      - nvm
      - bun
    machines:
      HOME-PC:
        add: [heidisql, robo3t]
      WORK-PC:
        exclude: [steam]
    ```
    Each ID matches a gallery `catalog/apps/{id}.yaml`. The deploy engine resolves install commands from gallery metadata. Error on missing gallery IDs (per general error policy).
  - Notes: Requires A7 (all 109 packages have gallery entries) and E4 (gallery-based install resolution) to be done first.

- [ ] Task D4: Update existing manifests to reference gallery `P1`
  - Files: `perch-config/*/manifest.yaml` (all 13 modules)
  - Action: Add optional `gallery: {app-id}` field to each manifest, linking it to the gallery definition. This enables the overlay pattern: gallery provides defaults, manifest provides overrides. Remove any fields from manifests that are now redundant with gallery (e.g., if gallery already defines the same links).
  - Notes: Not all manifest content moves to gallery. Personal config files (actual .gitconfig content, VS Code settings.json) stay in config. Only generic metadata (install IDs, default target paths) lives in gallery. Requires E0 (merge design) for the overlay rules.

#### Stream E: Gallery-Aware Deploy Engine (Perch repo)

- [ ] Task E0: Design gallery/manifest merge strategy `P0`
  - Files: Decision document in `_bmad-output/planning-artifacts/`
  - Action: Design and document the merge rules for gallery defaults + config manifest overrides. Key questions to resolve: (1) How does a config manifest remove a gallery-provided link? (subtraction syntax, e.g., `- !remove: link-name`? or replace semantics?), (2) Array merge for links: additive or replace-if-present?, (3) Array merge for extensions: union/deduplicate?, (4) Scalar fields: manifest always wins?, (5) What about nested objects (clean-filter rules)? Produce a clear merge rules document with examples.
  - Notes: This is the architectural crux of the gallery overlay system. Must be resolved before E3 implementation. Consider prototyping with 2-3 real module examples to validate the design.

- [ ] Task E1: Extend CatalogParser to load full gallery `P0`
  - Files: `src/Perch.Core/Catalog/CatalogParser.cs`
  - Action: Extend to parse the evolved gallery schema (Tasks A1 additions: `link-type`, `platforms`, `extensions`, `logo`, `clean-filter`). Return a `GalleryCatalog` containing all parsed app/font/tweak definitions. Support loading from the gallery URL (`https://laoujin.github.io/perch-gallery/catalog/`) with local path override for development.
  - Notes: Existing `CatalogParser` partially handles this. Extend, don't rewrite. If gallery URL is unreachable and no cache exists, error per general error policy.

- [ ] Task E2: Add gallery URL to PerchSettings `P0`
  - Files: `src/Perch.Core/Config/PerchSettings.cs`, `src/Perch.Core/Config/YamlSettingsProvider.cs`
  - Action: Add `GalleryUrl` property to `PerchSettings`. Default: `https://laoujin.github.io/perch-gallery/`. Add `GalleryLocalPath` as an optional override for local development (points to a local gallery repo clone). Configurable via `settings.yaml` or CLI flag. When local path is set, it takes precedence over URL.
  - Notes: Gallery is served from GitHub Pages. Local override useful for testing gallery changes before pushing.

- [ ] Task E3: Implement gallery/manifest overlay in ModuleDiscoveryService `P1`
  - Files: `src/Perch.Core/Modules/ModuleDiscoveryService.cs`
  - Action: During discovery, if a module manifest has `gallery: {app-id}`, load the gallery definition and merge according to the rules defined in E0. Produce the final `AppModule` with merged data.
  - Notes: Depends on E0 (merge design) being completed. Merge rules document is the source of truth for implementation.

- [ ] Task E4: Replace PackageManifestParser with gallery-based install resolution `P1`
  - Files: `src/Perch.Core/Packages/PackageManifestParser.cs`, `src/Perch.Core/Deploy/DeployService.cs`
  - Action: Instead of reading `packages.yaml` for system package installation, read `install.yaml` (app ID list + per-machine add/exclude sections). Resolve machine profile to get final app list. For each ID, look up the gallery entry's `install` section to get the platform-appropriate package manager command (winget on Windows, brew on macOS, apt on Linux). Error on missing gallery IDs. Feed resolved packages to `SystemPackageInstaller`.
  - Notes: `PackageManifestParser` can be refactored or replaced. The `IPackageManagerProvider` implementations remain unchanged.

- [ ] Task E5: Add `perch registry capture` CLI command `P1`
  - Files: New `src/Perch.Cli/Commands/RegistryCaptureCommand.cs`, `src/Perch.Core/Registry/RegistryCaptureService.cs`
  - Action: New CLI command: `perch registry capture <app-id>`. Reads the registry keys defined in the gallery's tweak/app definition, captures current values, and writes them to the config module's `manifest.yaml` registry section. This is the "read current → write to config" flow. If registry keys don't exist, warn per general error policy.
  - Notes: Uses existing `IRegistryProvider` to read values. Write to YAML using YamlDotNet serialization.

#### Stream F: Gallery Website (perch-gallery repo)

- [x] Task F1: Choose website stack -- **Decision: Astro** `P2`
  - Static-first, component islands for future interactivity, handles YAML data sources natively, deploys to GitHub Pages via build step.

- [ ] Task F2: Build gallery one-pager `P2`
  - Files: New `perch-gallery/src/` (Astro project)
  - Action: Single-page Astro site with: Perch branding (forest green `#2D5016` from UX docs), hero section with tagline, searchable/filterable list of all gallery entries (apps, fonts, tweaks), each entry shows name, description, install commands, and download link to YAML file. Responsive layout. Client-side JS filtering for search.
  - Notes: Read catalog YAML at build time via Astro's data loading. No runtime API needed. Logo images from `catalog/logos/`.

- [ ] Task F3: Update GitHub Pages deploy workflow `P2`
  - Files: `perch-gallery/.github/workflows/deploy.yml`
  - Action: Replace raw catalog upload with Astro build + deploy. Keep catalog YAML files accessible at their current URLs (backward compatibility -- these are the URLs the Perch CLI fetches from). Add the website as the default page.
  - Notes: Current deploy just uploads `catalog/` as an artifact. New deploy builds Astro site + serves catalog as static assets.

#### Stream G: External Source Import Tooling

- [ ] Task G1: Build Scoop manifest importer (optional) `P2`
  - Files: New script in `perch-gallery/tools/`
  - Action: Clone/fetch Scoop main bucket, parse JSON manifests, generate gallery YAML stubs for selected apps. Include `install.scoop` field in generated YAML.
  - Notes: Secondary to WinUtil (Task A5). Scoop is more developer-focused and has 1,438 apps in main bucket alone.

### Acceptance Criteria

#### Gallery Schema & Content (Stream A)

- [ ] AC-A1: Given a gallery app YAML with `link-type: junction`, when parsed by CatalogParser, then the link type is correctly represented in the parsed model
- [ ] AC-A2: Given a gallery app YAML with `platforms: [Windows]`, when deployed on Linux, then the app is skipped
- [ ] AC-A3: Given a gallery app YAML with a `logo` field, when the website renders, then the logo image is displayed
- [ ] AC-A4: Given the legacy notepad++ JSON, when converted to gallery YAML, then all 4 config symlink mappings (config.xml, shortcuts.xml, langs.xml, contextMenu.xml) are preserved with correct target paths
- [ ] AC-A5: Given `winutil-applications.json`, when the converter runs, then it produces valid gallery YAML files with `name`, `description`, `install.winget`, `install.choco`, `category`, and `links.website` fields populated
- [ ] AC-A6: Given the converter runs in diff mode against existing gallery, when a new app exists in winutil but not in gallery, then it is reported as a candidate for addition

#### Template Engine (Stream B)

- [ ] AC-B1: Given a machine profile with `Variables: { "user.name": "Wouter", "user.email": "w@example.com" }`, when a template file containing `{{user.name}}` is processed, then "Wouter" is substituted in the output
- [ ] AC-B2: Given a link with `template: true` and a source file with `{{op://vault/item}}`, when deployed, then the 1Password reference is still resolved (backward compatibility)
- [ ] AC-B3: Given a link with `template: true`, when deployed, then the generated file is written to `<config-repo>/.generated/<module>/<filename>` and the symlink points to the generated file
- [ ] AC-B4: Given a link WITHOUT `template: true`, when deployed, then the symlink points directly to the source file (current behavior unchanged)
- [ ] AC-B5: Given a template with `{{machine.name}}`, when deployed on a machine named "DESKTOP-ABC", then "DESKTOP-ABC" is substituted
- [ ] AC-B6: Given a template with an unknown variable `{{foo.bar}}` and no matching MachineProfile variable, when deployed, then the deploy reports a warning and leaves the placeholder as-is (per general error policy -- warn and continue)

#### Change-Ignore (Stream C)

- [ ] AC-C0: Given `perch filter clean notepadplusplus` is invoked with config.xml content on stdin, when the module defines strip rules for `<FindHistory>` and `<Session>`, then cleaned content is written to stdout with those elements removed
- [ ] AC-C1: Given a module with a clean-filter defined, when `perch deploy` runs, then git clean/smudge filters are registered in the config repo's `.git/config` and `.gitattributes` with `clean = perch filter clean <module>`
- [ ] AC-C2: Given notepad++ config.xml with volatile `<FindHistory>` and `<Session>` elements, when the clean filter runs, then these elements are stripped from the git-staged version while the working copy retains them
- [ ] AC-C3: Given a clean filter is registered, when `git diff` runs on the filtered file, then volatile sections do not appear as changes

#### Config Restructuring (Stream D)

- [ ] AC-D0: Given a config module folder is a git submodule that hasn't been initialized, when `perch deploy` runs, then Perch auto-runs `git submodule init/update` before processing the module
- [ ] AC-D1: Given `perch-config/git/` is replaced with a Git-Config submodule, when `perch deploy` runs, then git config files are symlinked from the submodule path
- [ ] AC-D2: Given the Git-Config submodule has a `.gitconfig.template` with `template: true`, when deployed on machine "HOME-PC", then `~/.gitconfig` symlinks to `.generated/git/.HOME-PC.gitconfig` with resolved user.name/email
- [ ] AC-D3: Given `install.yaml` references `[git, vscode, nvm]` and a machine profile `HOME-PC` adds `[heidisql]`, when `perch deploy` runs on HOME-PC, then the resolved app list includes git, vscode, nvm, heidisql and the correct winget/choco commands are executed
- [ ] AC-D3b: Given `install.yaml` references an app ID `nonexistent` that has no gallery entry, when `perch deploy` runs, then an error is reported for that app (per general error policy)
- [ ] AC-D4: Given the old `packages.yaml` is removed, when `perch deploy` runs, then there is no error about missing packages.yaml (engine reads `install.yaml` instead)
- [ ] AC-D5: Given a manifest with `gallery: notepadplusplus`, when deployed, then gallery defaults (clean-filter, default links) are merged with manifest overrides

#### Gallery-Aware Engine (Stream E)

- [ ] AC-E0: Given the merge design document is complete, then it covers: subtraction syntax, array merge rules for links, array merge rules for extensions, scalar field precedence, nested object handling, and includes at least 2 worked examples with real modules
- [ ] AC-E1: Given `PerchSettings.GalleryUrl` points to the gallery GitHub Pages URL, when `CatalogParser.ParseAsync()` runs, then all apps/fonts/tweaks are loaded with their full schema (including new fields)
- [ ] AC-E1b: Given `PerchSettings.GalleryLocalPath` is set, when `CatalogParser.ParseAsync()` runs, then the local path is used instead of the URL
- [ ] AC-E2: Given a module manifest with `gallery: vscode` and the gallery defines a `clean-filter`, when the manifest does NOT define a clean-filter, then the gallery's clean-filter is applied
- [ ] AC-E3: Given a module manifest with `gallery: vscode` and BOTH define `vscode-extensions`, when merged, then extensions are combined (union, no duplicates)
- [ ] AC-E4: Given `perch registry capture ditto`, when run, then the 4 registry values from ditto's manifest are read from the system and written back to the manifest with current values

#### Gallery Website (Stream F)

- [ ] AC-F1: Given the gallery website is built, when visiting the GitHub Pages URL, then a single-page site renders with forest green branding
- [ ] AC-F2: Given the catalog contains 30+ YAML entries, when the website loads, then all entries are listed and searchable by name/category/tags
- [ ] AC-F3: Given a user clicks on an app entry, then they can view the full YAML definition and download it
- [ ] AC-F4: Given a push to main branch, when the deploy workflow runs, then the website is rebuilt and deployed to GitHub Pages with catalog YAML files accessible at existing URLs

## Additional Context

### Dependencies

**External:**
- `Laoujin/Git-Config` repo must exist and have a `manifest.yaml` added before Task D1
- WinUtil `applications.json` already downloaded to `Perch/_bmad-output/research/`

**Internal (task ordering by priority tier):**
- **P0 first:** B1-B2 → B3-B4 (template engine). E0 (merge design) independently. C1-C3 (clean filters) independently. E1-E2 after A1.
- **P1 next:** A1-A4 (gallery content). D1-D4 (config restructuring, D1 requires B4). E3 (requires E0 + E1-E2). E4 (requires E1-E2). E5 independently. D3 after E4.
- **P2 last:** A5-A7 (converters, logos, bulk curation). F2-F3 (website). G1 (Scoop).

**NuGet packages (no new packages expected):**
- YamlDotNet (already in use) for all YAML parsing
- No new dependencies for template engine (regex-based, already in `TemplateProcessor`)

### Testing Strategy

**Unit tests (NUnit + NSubstitute, follow existing patterns):**
- `TemplateProcessorTests` -- extend with general variable pattern tests
- `MachineVariableResolverTests` -- variable resolution, fallback to built-ins, unknown variable handling
- `CatalogParserTests` -- extend for new schema fields (link-type, platforms, extensions, logo, clean-filter)
- `ModuleDiscoveryServiceTests` -- gallery overlay merging (defaults + overrides, array merging, conflict resolution)
- `PackageManifestParserTests` -- gallery-based install resolution
- `CleanFilterServiceTests` -- filter registration, `perch filter clean` stdin/stdout processing, XML/INI strip rules
- `SubmoduleServiceTests` -- submodule detection, init/update invocation
- `RegistryCaptureServiceTests` -- registry read + YAML write

**Integration tests:**
- Deploy with template files → verify generated files + correct symlink targets
- Deploy with gallery overlay → verify merged module has expected links/extensions
- Deploy with `install.yaml` → verify correct package manager commands resolved
- Clean filter registration → verify `.git/config` and `.gitattributes` updated

**Manual testing:**
- End-to-end: clone config + gallery → `perch deploy` → verify all symlinks, generated files, installed packages
- Gallery website: build locally, verify all entries render, search works, YAML download works
- WinUtil converter: run against latest `applications.json`, spot-check 10+ generated YAMLs
- Git-Config submodule: verify `.gitconfig` templating produces correct machine-specific file

### Notes

**High-risk items:**
- **Gallery/manifest merge complexity:** Dedicated design task (E0) addresses this. Key questions: subtraction syntax for removing gallery-provided items, array merge rules, conflict resolution. Must be resolved before E3 implementation.
- **packages.yaml migration:** 109 packages need gallery entries. Many are niche (HeidiSQL, Robo3T) and won't exist in winutil. Task A7 covers this with AI-assisted curation (~50+ manual entries).
- **Submodule workflow:** Submodules add friction to `git clone` (need `--recurse-submodules`). Mitigated: Perch deploy auto-detects and runs `git submodule init/update`. Submodules are optional per module -- user's choice.
- **Generated file management:** `.generated/` directory needs to be in `.gitignore`. On fresh clone before templates resolve, deploy produces a warning per the general error policy and skips the template link.

**Known limitations:**
- Clean filters only work when files are tracked by git. Untracked files in symlinked directories won't be filtered.
- Gallery website is static (Astro) -- no runtime search indexing. Search is client-side JS filtering.
- WinUtil converter produces stubs, not complete gallery entries. Config path mappings need manual curation per app (AI-assisted).
- Gallery fetched from URL requires internet on first deploy. Local path override available for offline use.

**Future considerations (out of scope):**
- Gallery versioning -- gallery entries could have version ranges for install commands
- Gallery CI validation -- lint YAML files on PR, ensure schema compliance
- Config repo scaffolding -- `perch init` could create a config repo from gallery selections
- Clean filter GUI -- visual editor for selecting which XML/INI fields to ignore
