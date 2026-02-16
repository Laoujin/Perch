---
stepsCompleted: [1, 2, 3]
inputDocuments:
  - '_bmad-output/implementation-artifacts/tech-spec-perch-full-scope-reconciliation.md'
  - '_bmad-output/planning-artifacts/prd.md'
  - '_bmad-output/planning-artifacts/architecture.md'
scope: 'Gallery/Config Reconciliation & Ecosystem Build'
---

# Perch Gallery/Config Reconciliation - Epic Breakdown

## Overview

This document covers epics and stories for the Gallery/Config Reconciliation scope: establishing perch-gallery as the single source of truth, template engine, change-ignore, gallery website, and config repo modernization.

## Requirements Inventory

### Functional Requirements

**Gallery Schema & Content (Stream A)**
- FR-G1: Gallery app YAML schema supports link-type, platforms, extensions, logo, clean-filter fields
- FR-G2: Legacy apps (dotfiles-legacy) converted to gallery catalog entries
- FR-G3: perch-config apps missing from gallery get catalog entries
- FR-G4: Node ecosystem apps (nvm, bun) have gallery entries with correct install metadata
- FR-G5: WinUtil JSON-to-gallery-YAML converter tool with diff mode
- FR-G6: Gallery entries support app logos (SVG/PNG)
- FR-G7: All 109 packages.yaml apps have gallery entries (prerequisite for retirement)

**Template Engine (Stream B)**
- FR-G8: TemplateProcessor handles general {{variable.path}} syntax alongside existing {{op://...}}
- FR-G9: IVariableResolver reads MachineProfile.Variables with built-in fallbacks (machine.name, user.name, user.email, platform, date)
- FR-G10: Deploy generates resolved files to .generated/<module>/<filename> and symlinks that instead of template source
- FR-G11: Links opt-in to template processing via template: true flag

**Change-Ignore / Clean Filters (Stream C)**
- FR-G12: `perch filter clean <module>` CLI command: reads stdin, applies module's filter rules, writes to stdout
- FR-G13: Deploy registers git clean/smudge filters in config repo's .git/config and .gitattributes
- FR-G14: Filter supports strip-xml-elements and strip-ini-keys rule types in C#
- FR-G15: Gallery app definitions can include clean-filter rules

**Config Restructuring (Stream D)**
- FR-G16: Deploy detects and auto-initializes git submodules in config module folders
- FR-G17: Git-Config repo replaces perch-config/git/ as submodule with .gitconfig.template
- FR-G18: bun-packages module retired, replaced with bun module with global-packages
- FR-G19: packages.yaml retired, replaced with install.yaml referencing gallery app IDs with per-machine add/exclude
- FR-G20: Module manifests reference gallery definitions via gallery: field for overlay

**Gallery-Aware Engine (Stream E)**
- FR-G21: Gallery/manifest merge strategy designed: subtraction syntax, array merge rules, scalar precedence, nested objects
- FR-G22: CatalogParser loads full gallery from URL or local path with evolved schema
- FR-G23: PerchSettings has GalleryUrl (default: GitHub Pages) and GalleryLocalPath override
- FR-G24: ModuleDiscoveryService merges gallery defaults with manifest overrides per merge rules
- FR-G25: Install resolution reads install.yaml, resolves platform-appropriate commands from gallery metadata
- FR-G26: `perch registry capture <app-id>` reads current registry state into manifest YAML

**Gallery Website (Stream F)**
- FR-G27: Astro-based one-page gallery site with forest green branding
- FR-G28: Website lists all entries (apps/fonts/tweaks) with client-side search/filter
- FR-G29: GitHub Pages deploy builds Astro site while keeping catalog YAML at existing URLs

**External Import (Stream G)**
- FR-G30: Scoop manifest importer generates gallery YAML stubs from bucket JSON

### Non-Functional Requirements

- NFR-G1: Non-critical errors produce warning and continue; critical errors abort
- NFR-G2: Clean break migration -- no dual-format support
- NFR-G3: Template processing strictly opt-in (template: true) -- non-flagged files never scanned
- NFR-G4: Clean filter binary is perch CLI itself -- cross-platform, no external scripts
- NFR-G5: `perch filter clean` must be fast -- runs on every git diff/add/status

### Additional Requirements

- Extend existing TemplateProcessor/IReferenceResolver -- don't rewrite
- CatalogParser already partially built in Core -- extend it
- CleanFilterDefinition already exists in manifest model -- wire it
- MachineProfile.Variables already provides data source for template variables
- All new services registered via AddPerchCore() DI extension
- Immutable records for all new data models
- HyphenatedNamingConvention for all YAML parsing
- Priority tiers: P0 (core engine) -> P1 (gallery content + config) -> P2 (website + polish)

### FR Coverage Map

- FR-G1: Epic 3 - Gallery schema evolution
- FR-G2: Epic 4 - Legacy app gallery entries
- FR-G3: Epic 4 - perch-config app gallery entries
- FR-G4: Epic 4 - Node ecosystem gallery entries
- FR-G5: Epic 8 - WinUtil converter
- FR-G6: Epic 4 - App logos
- FR-G7: Epic 4 - All packages.yaml gallery entries
- FR-G8: Epic 1 - Generalize TemplateProcessor
- FR-G9: Epic 1 - IVariableResolver + MachineVariableResolver
- FR-G10: Epic 1 - Deploy generates resolved files
- FR-G11: Epic 1 - template: true flag
- FR-G12: Epic 2 - perch filter clean command
- FR-G13: Epic 2 - Deploy registers clean filters
- FR-G14: Epic 2 - Filter rule types (XML, INI)
- FR-G15: Epic 4 - Gallery clean-filter definitions
- FR-G16: Epic 6 - Submodule-aware deploy
- FR-G17: Epic 6 - Git-Config submodule
- FR-G18: Epic 6 - bun module
- FR-G19: Epic 6 - install.yaml
- FR-G20: Epic 6 - Gallery references in manifests
- FR-G21: Epic 5 - Merge strategy design
- FR-G22: Epic 3 - CatalogParser full gallery
- FR-G23: Epic 3 - Gallery URL in settings
- FR-G24: Epic 5 - Gallery/manifest overlay
- FR-G25: Epic 5 - Gallery-based install resolution
- FR-G26: Epic 5 - Registry capture command
- FR-G27: Epic 7 - Astro gallery site
- FR-G28: Epic 7 - Searchable gallery listing
- FR-G29: Epic 7 - GitHub Pages deploy
- FR-G30: Epic 8 - Scoop importer

## Epic List

### Epic 1: Machine-Specific Config Generation
User can define template configs with {{user.name}}, {{machine.name}} etc., and Perch generates machine-specific files at deploy time.
**FRs covered:** FR-G8, FR-G9, FR-G10, FR-G11
**Priority:** P0

### Epic 2: Clean Git Diffs via Change-Ignore
User's git diffs stay clean despite volatile config fields. Perch registers git clean filters automatically.
**FRs covered:** FR-G12, FR-G13, FR-G14
**Priority:** P0

### Epic 3: Gallery Engine Integration
Perch engine can load and parse the full gallery catalog from GitHub Pages URL or local path. Gallery schema evolved with new fields.
**FRs covered:** FR-G1, FR-G22, FR-G23
**Priority:** P0

### Epic 4: Gallery Content Expansion
All apps have gallery entries with install metadata, logos, and clean-filter rules.
**FRs covered:** FR-G2, FR-G3, FR-G4, FR-G6, FR-G7, FR-G15
**Priority:** P1

### Epic 5: Gallery-Aware Deploy
Deploy resolves everything via gallery overlay: merge rules, app definitions, install commands, registry capture.
**FRs covered:** FR-G21, FR-G24, FR-G25, FR-G26
**Priority:** P1

### Epic 6: Config Repo Modernization
Config repo restructured: submodule-aware deploy, Git-Config submodule, bun module, install.yaml, gallery references.
**FRs covered:** FR-G16, FR-G17, FR-G18, FR-G19, FR-G20
**Priority:** P1

### Epic 7: Gallery Website
Astro one-page site with forest green branding, searchable gallery, YAML download via GitHub Pages.
**FRs covered:** FR-G27, FR-G28, FR-G29
**Priority:** P2

### Epic 8: External Source Import
Import app definitions from WinUtil JSON and Scoop manifests into gallery format.
**FRs covered:** FR-G5, FR-G30
**Priority:** P2

---

## Epic 1: Machine-Specific Config Generation

User can define template configs with variable placeholders and Perch generates machine-specific resolved files at deploy time, symlinking the generated output instead of the template source.

### Story 1.1: Add template flag to manifest link entries

As a config author,
I want to mark specific links as templates in my manifest,
So that Perch knows which files need variable processing before symlinking.

**Acceptance Criteria:**

**Given** a manifest YAML with a link entry containing `template: true`
**When** the manifest is parsed by ManifestParser
**Then** the resulting LinkEntry record has `IsTemplate = true`

**Given** a manifest YAML with a link entry without the template field
**When** the manifest is parsed
**Then** the resulting LinkEntry record has `IsTemplate = false` (default)

**Given** a manifest with mixed template and non-template links
**When** parsed
**Then** each LinkEntry correctly reflects its template flag independently

### Story 1.2: Create IVariableResolver and MachineVariableResolver

As a config author,
I want template variables resolved from my machine profile and built-in values,
So that generated configs contain the correct machine-specific data.

**Acceptance Criteria:**

**Given** a MachineProfile with Variables containing `user.name: Wouter`
**When** `ResolveAsync("user.name")` is called
**Then** it returns "Wouter"

**Given** no MachineProfile variable for `machine.name`
**When** `ResolveAsync("machine.name")` is called
**Then** it returns `Environment.MachineName` as the built-in fallback

**Given** a MachineProfile variable that shadows a built-in (e.g., `machine.name: CUSTOM`)
**When** `ResolveAsync("machine.name")` is called
**Then** it returns "CUSTOM" (MachineProfile wins)

**Given** an unknown variable `foo.bar` with no MachineProfile entry and no built-in
**When** `ResolveAsync("foo.bar")` is called
**Then** it returns null

**And** the built-in set includes: machine.name, user.name, user.email, platform, date

### Story 1.3: Generalize TemplateProcessor for variable patterns

As a config author,
I want {{variable.path}} placeholders resolved alongside existing {{op://...}} references,
So that I can use both secrets and machine variables in the same template.

**Acceptance Criteria:**

**Given** a template containing `{{user.name}}`
**When** processed by TemplateProcessor
**Then** the variable is resolved via IVariableResolver

**Given** a template containing `{{op://vault/item}}`
**When** processed by TemplateProcessor
**Then** the reference is resolved via IReferenceResolver (existing behavior preserved)

**Given** a template with both `{{user.name}}` and `{{op://vault/item}}`
**When** processed
**Then** both are resolved by their respective resolvers

**Given** a template with `{{unknown.var}}` that resolves to null
**When** processed
**Then** a warning is produced and the placeholder is left as-is

### Story 1.4: Integrate template processing into deploy pipeline

As a user deploying configs,
I want template files automatically processed and the generated output symlinked,
So that my machine gets the correct resolved config without manual steps.

**Acceptance Criteria:**

**Given** a link with `template: true` pointing to a file with `{{user.name}}`
**When** `perch deploy` runs
**Then** a resolved file is written to `<config-repo>/.generated/<module>/<filename>`
**And** the symlink points to the generated file, not the template source

**Given** a link WITHOUT `template: true`
**When** `perch deploy` runs
**Then** the symlink points directly to the source file (current behavior unchanged)

**Given** a template link with multiple variables
**When** deployed on a machine named "DESKTOP-ABC" with user.name "Wouter"
**Then** all variables are resolved in the generated file

**Given** the `.generated/` directory doesn't exist yet
**When** deploy runs with template links
**Then** the directory is created automatically

---

## Epic 2: Clean Git Diffs via Change-Ignore

User's git diffs stay clean despite volatile app config changes (window positions, search history, timestamps). Perch registers git clean filters that strip noise automatically.

### Story 2.1: Implement filter rule types for volatile patterns

As a config author,
I want to define rules that strip volatile XML elements and INI keys from config files,
So that git only tracks meaningful changes.

**Acceptance Criteria:**

**Given** a `strip-xml-elements` rule targeting `<FindHistory>` and `<Session>`
**When** XML content containing those elements is processed
**Then** the elements and their children are removed from the output

**Given** a `strip-ini-keys` rule targeting `LastOpened` and `WindowPosition`
**When** INI content containing those keys is processed
**Then** the matching key=value lines are removed from the output

**Given** XML content without any matching elements
**When** the strip-xml-elements rule runs
**Then** the content is returned unchanged

**Given** multiple filter rules applied to the same content
**When** processed
**Then** all rules are applied in sequence

### Story 2.2: Add `perch filter clean` CLI subcommand

As a user with volatile config files,
I want a CLI command that git can invoke as a clean filter,
So that volatile sections are stripped transparently on git operations.

**Acceptance Criteria:**

**Given** `perch filter clean notepadplusplus` is invoked
**When** config.xml content with `<FindHistory>` and `<Session>` is piped to stdin
**Then** cleaned content is written to stdout with those elements removed

**Given** a module name that doesn't exist
**When** `perch filter clean nonexistent` is invoked
**Then** the command exits with an error code and message

**Given** the command receives content that doesn't match any filter rules
**When** processed
**Then** the content is passed through unchanged

**And** the command reads from stdin and writes to stdout (git clean filter protocol)

### Story 2.3: Wire CleanFilterService into deploy pipeline

As a user deploying configs,
I want git clean filters automatically registered when I deploy a module with filter rules,
So that I don't have to manually configure git for each volatile file.

**Acceptance Criteria:**

**Given** a module with a `clean-filter` definition
**When** `perch deploy` runs
**Then** a git clean filter is registered in the config repo's `.git/config` with `clean = perch filter clean <module>`
**And** matching file patterns are added to `.gitattributes`

**Given** a module without a clean-filter definition
**When** `perch deploy` runs
**Then** no git filter changes are made for that module

**Given** a clean filter is already registered for a module
**When** `perch deploy` runs again
**Then** the filter registration is idempotent (no duplicate entries)

---

## Epic 3: Gallery Engine Integration

Perch engine can load and parse the full gallery catalog from GitHub Pages URL or local path. Gallery schema evolved with new fields (link-type, platforms, extensions, logo, clean-filter).

### Story 3.1: Evolve gallery app YAML schema

As a gallery contributor,
I want the gallery YAML schema to support link-type, platforms, extensions, logo, and clean-filter fields,
So that gallery entries carry all the metadata needed for automated deploy.

**Acceptance Criteria:**

**Given** a gallery app YAML with `link-type: junction`
**When** parsed
**Then** the link type is correctly represented in the parsed model

**Given** a gallery app YAML with `platforms: [Windows]`
**When** parsed
**Then** the platform constraint is captured

**Given** a gallery app YAML with `extensions` map and `logo` field
**When** parsed
**Then** both fields are available in the parsed model

**And** all existing gallery YAML files use forward slashes consistently
**And** all 10 existing app YAMLs are updated to the new schema

### Story 3.2: Add gallery settings to PerchSettings

As a Perch user,
I want to configure where Perch loads the gallery catalog from,
So that I can use the public gallery or a local clone for development.

**Acceptance Criteria:**

**Given** no gallery settings configured
**When** PerchSettings is loaded
**Then** GalleryUrl defaults to `https://laoujin.github.io/perch-gallery/`

**Given** GalleryLocalPath is set in settings.yaml
**When** CatalogParser loads the gallery
**Then** the local path is used instead of the URL

**Given** both GalleryUrl and GalleryLocalPath are set
**When** loading
**Then** GalleryLocalPath takes precedence

### Story 3.3: Extend CatalogParser to load full gallery

As a Perch developer,
I want the CatalogParser to load the complete gallery with the evolved schema,
So that the engine has access to all app definitions during deploy.

**Acceptance Criteria:**

**Given** GalleryLocalPath points to a gallery repo clone
**When** `CatalogParser.ParseAsync()` runs
**Then** all apps, fonts, and tweaks are loaded with full schema (link-type, platforms, extensions, logo, clean-filter)

**Given** GalleryUrl points to GitHub Pages and no local path is set
**When** `CatalogParser.ParseAsync()` runs
**Then** gallery is fetched from the URL

**Given** gallery URL is unreachable and no local path or cache exists
**When** `CatalogParser.ParseAsync()` runs
**Then** a critical error is reported per NFR-G1

**Given** a gallery entry with unknown/extra YAML fields
**When** parsed
**Then** unknown fields are ignored (IgnoreUnmatchedProperties)

---

## Epic 4: Gallery Content Expansion

All apps have comprehensive gallery entries with accurate install metadata, logos, and clean-filter rules where applicable.

### Story 4.1: Add legacy apps to gallery catalog

As a Perch user migrating from dotfiles-legacy,
I want all legacy apps available in the gallery,
So that I can reference them by gallery ID instead of maintaining legacy JSON.

**Acceptance Criteria:**

**Given** the legacy apps (beyondcompare, cmder, filezilla, heidisql, sublimetext)
**When** gallery entries are created
**Then** each has name, display-name, category, description, install (winget/choco), and config.links
**And** config path mappings are preserved from legacy JSON files
**And** all entries are added to index.yaml

### Story 4.2: Add missing perch-config apps to gallery

As a Perch user,
I want apps that exist in my config but not in the gallery to have gallery entries,
So that gallery coverage is complete for my setup.

**Acceptance Criteria:**

**Given** apps in perch-config not in gallery (bash, ditto, greenshot, visualstudio)
**When** gallery entries are created
**Then** each has appropriate metadata and install IDs
**And** ditto is correctly marked as registry-only (no config links)

### Story 4.3: Add node ecosystem apps to gallery

As a developer using nvm and bun,
I want gallery entries for my node toolchain,
So that Perch can manage their installation.

**Acceptance Criteria:**

**Given** nvm (CoreyButler.NVMforWindows) and bun (Oven-sh.Bun)
**When** gallery entries are created
**Then** each has correct winget/choco IDs and platform-appropriate variants
**And** pnpm/yarn are NOT gallery entries (they're node packages, not standalone apps)

### Story 4.4: Add clean-filter definitions to gallery apps

As a user with volatile config files,
I want gallery entries to include clean-filter rules for known noisy apps,
So that Perch can auto-configure git filters for me.

**Acceptance Criteria:**

**Given** notepad++ gallery entry
**When** the clean-filter section is added
**Then** it defines strip-xml-elements rules for FindHistory, Session, and position-related GUIConfig elements

**Given** any other app with known volatile config patterns
**When** identified
**Then** clean-filter rules are added to their gallery entries

### Story 4.5: Add logo field and collect logos

As a gallery website visitor,
I want apps to have logos,
So that I can visually identify apps in the catalog.

**Acceptance Criteria:**

**Given** existing gallery apps
**When** logos are collected
**Then** SVG/PNG files are stored in `catalog/logos/`
**And** each app YAML has a `logo` field with relative path

**Given** an app without an available logo
**When** rendered
**Then** a placeholder is shown (no error)

### Story 4.6: Create gallery entries for all packages.yaml apps

As a Perch user retiring packages.yaml,
I want all 109 packages to have gallery entries,
So that install.yaml can reference them by gallery ID.

**Acceptance Criteria:**

**Given** all 109 packages from perch-config/packages.yaml
**When** gallery entries are created
**Then** each has name, description, install.winget (and install.choco where available), category, and links

**Given** niche apps not in WinUtil (HeidiSQL, Robo3T, etc.)
**When** entries are created
**Then** winget/choco IDs are manually verified for accuracy

**And** all entries are added to index.yaml

---

## Epic 5: Gallery-Aware Deploy

Deploy resolves everything via gallery overlay: gallery provides defaults, config manifest provides overrides. Merge rules are documented and implemented.

### Story 5.1: Design gallery/manifest merge strategy

As a Perch architect,
I want clear, documented rules for how gallery defaults merge with config overrides,
So that the overlay behavior is predictable and well-understood.

**Acceptance Criteria:**

**Given** the merge design document
**Then** it covers: subtraction syntax, array merge rules for links, array merge rules for extensions, scalar field precedence, nested object handling
**And** includes at least 2 worked examples with real modules (e.g., vscode, notepadplusplus)

### Story 5.2: Implement gallery/manifest overlay in ModuleDiscoveryService

As a Perch user with gallery-referenced modules,
I want gallery defaults automatically merged with my manifest overrides,
So that I only need to specify what's different from the gallery default.

**Acceptance Criteria:**

**Given** a manifest with `gallery: vscode` and the gallery defines a clean-filter
**When** the manifest does NOT define a clean-filter
**Then** the gallery's clean-filter is applied

**Given** a manifest with `gallery: vscode` and BOTH define extensions
**When** merged
**Then** extensions are combined (union, no duplicates)

**Given** a manifest with `gallery: vscode` and the manifest overrides a scalar field
**When** merged
**Then** the manifest value wins

### Story 5.3: Replace PackageManifestParser with gallery-based install resolution

As a Perch user,
I want app installations resolved from gallery metadata via install.yaml,
So that package management uses the same source of truth as everything else.

**Acceptance Criteria:**

**Given** install.yaml with `apps: [git, vscode, nvm]` and machine HOME-PC with `add: [heidisql]`
**When** `perch deploy` runs on HOME-PC
**Then** resolved app list is git, vscode, nvm, heidisql with correct winget commands from gallery

**Given** install.yaml references `nonexistent` app ID
**When** `perch deploy` runs
**Then** an error is reported for that app per NFR-G1

**Given** the old packages.yaml is removed
**When** `perch deploy` runs
**Then** no error about missing packages.yaml (engine reads install.yaml)

### Story 5.4: Add `perch registry capture` CLI command

As a Perch user managing registry tweaks,
I want to capture current registry state into my config manifest,
So that I can version and reproduce my registry settings.

**Acceptance Criteria:**

**Given** `perch registry capture ditto`
**When** run on a system with ditto's registry keys
**Then** current values are read and written to the module's manifest.yaml registry section

**Given** a registry key that doesn't exist
**When** `perch registry capture` runs
**Then** a warning is produced per NFR-G1 (not an error)

---

## Epic 6: Config Repo Modernization

Config repo restructured with submodule support, gallery references, modern package list format, and retired legacy formats.

### Story 6.1: Add submodule-aware deploy

As a config author using git submodules for shared config repos,
I want Perch to auto-detect and initialize submodules during deploy,
So that submodule-based modules work seamlessly.

**Acceptance Criteria:**

**Given** a config module folder that is a git submodule not yet initialized
**When** `perch deploy` runs
**Then** Perch auto-runs `git submodule init` + `git submodule update` before processing

**Given** a config module folder that is a regular directory (not a submodule)
**When** `perch deploy` runs
**Then** behavior is unchanged (no submodule operations)

**Given** a submodule that is already initialized and up-to-date
**When** `perch deploy` runs
**Then** no redundant submodule operations are performed

### Story 6.2: Replace git/ folder with Git-Config submodule

As a config author,
I want my git config to live in a separate repo (Git-Config) linked as a submodule,
So that git config is shareable and uses template-based generation.

**Acceptance Criteria:**

**Given** perch-config/git/ is replaced with Laoujin/Git-Config submodule
**When** `perch deploy` runs
**Then** git config files are symlinked from the submodule path

**Given** the Git-Config submodule has a .gitconfig.template with template: true
**When** deployed on machine HOME-PC
**Then** ~/.gitconfig symlinks to .generated/git/.HOME-PC.gitconfig with resolved user.name/email

### Story 6.3: Retire bun-packages, create bun module

As a config author,
I want bun managed as a proper app module with global packages,
So that the config structure is consistent.

**Acceptance Criteria:**

**Given** the old bun-packages/ module is removed
**When** a new bun/manifest.yaml is created
**Then** it contains display-name, global-packages list, and manager: bun

### Story 6.4: Update existing manifests to reference gallery

As a config author,
I want my manifests to reference gallery app definitions,
So that gallery defaults are applied and I only override what's personal.

**Acceptance Criteria:**

**Given** 13 existing module manifests
**When** updated with `gallery: {app-id}` field
**Then** each links to the correct gallery definition
**And** redundant fields already in gallery are removed from manifests

### Story 6.5: Retire packages.yaml

As a config author,
I want packages.yaml replaced with install.yaml,
So that package management uses gallery IDs instead of duplicated metadata.

**Acceptance Criteria:**

**Given** all 109 packages have gallery entries (Story 4.6)
**When** packages.yaml is deleted and install.yaml created
**Then** install.yaml references gallery app IDs with per-machine add/exclude sections

**Given** install.yaml format
**Then** it follows the schema: apps array + machines map with add/exclude arrays

---

## Epic 7: Gallery Website

Astro-based one-page gallery site with forest green branding, deployed to GitHub Pages.

### Story 7.1: Build gallery one-pager with Astro

As a Perch user browsing the gallery,
I want a searchable, filterable website showing all gallery entries,
So that I can discover and download app definitions.

**Acceptance Criteria:**

**Given** the gallery website is built
**When** visiting the GitHub Pages URL
**Then** a single-page site renders with forest green (#2D5016) branding

**Given** the catalog contains 30+ YAML entries
**When** the website loads
**Then** all entries are listed and searchable by name/category/tags

**Given** a user clicks on an app entry
**Then** they can view the full YAML definition and download it

### Story 7.2: Update GitHub Pages deploy workflow

As a gallery maintainer,
I want pushes to main automatically build and deploy the Astro site,
So that the website stays current with catalog changes.

**Acceptance Criteria:**

**Given** a push to main branch
**When** the deploy workflow runs
**Then** the Astro site is built and deployed to GitHub Pages
**And** catalog YAML files remain accessible at their existing URLs (backward compatibility)

---

## Epic 8: External Source Import

Import app definitions from external catalogs into gallery YAML format.

### Story 8.1: Build WinUtil JSON to gallery YAML converter

As a gallery curator,
I want to convert WinUtil's app catalog into gallery YAML format,
So that I can quickly expand the gallery with vetted app definitions.

**Acceptance Criteria:**

**Given** winutil-applications.json (~300 apps)
**When** the converter runs
**Then** individual YAML files are generated with name, description, install.winget, install.choco, category, links.website

**Given** the converter runs in diff mode against existing gallery
**When** a new app exists in winutil but not in gallery
**Then** it is reported as a candidate for addition

**And** the converter supports filtering by category or manual selection

### Story 8.2: Build Scoop manifest importer

As a gallery curator,
I want to import Scoop manifests into gallery YAML format,
So that developer-focused tools from Scoop's catalog can be added.

**Acceptance Criteria:**

**Given** Scoop main bucket JSON manifests
**When** the importer runs on selected apps
**Then** gallery YAML stubs are generated with install.scoop field populated

**And** generated stubs require manual review before adding to gallery
