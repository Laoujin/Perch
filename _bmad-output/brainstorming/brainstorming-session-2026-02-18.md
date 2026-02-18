---
stepsCompleted: [1, 2, 3, 4]
inputDocuments: []
session_topic: 'Expanding and restructuring the perch-gallery apps catalog -- more apps, deeper ecosystems, richer metadata, better organization'
session_goals: 'Volume expansion from external sources, ecosystem deep-dives (.NET/Node/Git/Bash), richer metadata (icons/stars/profiles/OS), taxonomy rework, catalog directory structure, relationship types, GitHub stars sync, config consolidation'
selected_approach: 'ai-recommended'
techniques_used: ['Mind Mapping', 'Cross-Pollination', 'Morphological Analysis']
ideas_generated: 23
context_file: ''
session_active: false
workflow_completed: true
---

# Brainstorming Session Results

**Facilitator:** Wouter
**Date:** 2026-02-18

## Session Overview

**Topic:** Expanding and restructuring the perch-gallery apps catalog -- more apps, deeper ecosystems, richer metadata, better organization
**Goals:**
1. Volume expansion -- mine ninite/choco/winget/WinUtil/Scoop for apps to add
2. Ecosystem deep-dives -- .NET, Node, Git, Bash as fully worked-out ecosystems
3. Richer metadata -- icons, GitHub links, profiles, OS scope, cross-platform targets
4. New tweak category -- "Default Apps" (file associations)
5. Taxonomy rework -- categories, subcategories, language-centric grouping
6. Catalog structure -- subdirectories for scale, language-based grouping
7. Relationship types -- alternatives, suggested, and other sub-section types
8. GitHub stars -- separate file + sync script
9. Config consolidation -- gallery absorbs manifest.json data from perch-config
10. Cross-platform / OS indicators

### Session Setup

- **Approach selected:** AI-Recommended Techniques
- **Technique sequence:** Mind Mapping -> Cross-Pollination -> Morphological Analysis

## Technique Execution Results

### Phase 1: Mind Mapping

Mapped 9 branches from central node "perch-gallery catalog":

**Branch 1: Ecosystem Deep-Dives**
- .NET: editors (VS, Rider, VS Code), decompilers (ILSpy, dotPeek, dnSpy), profilers (dotTrace, dotMemory, PerfView), SDK version selection, global tools (ef, outdated, format, counters, dump, trace, user-jwts), build tools (Nuke, Cake), dotfiles (nuget.config, global.json, dotnet-tools.json), LINQPad
- Node: runtimes (node, bun as alternatives), version managers (nvm, fnm, volta), package managers (npm, pnpm, yarn), global packages as individual entries, dotfiles (.npmrc, bunfig.toml). Install type `node-package:` resolved by user's chosen manager.
- Git: dotfile with template .gitconfig (includes + {{perch.xxx}} variables), GUI tools as separate entries, SSH keys from vault, "Git Bash Here" as app-owned context menu tweak
- Bash/PowerShell: thin dotfile entries, template profiles, generic symlink capability
- Java: JDK version/vendor selection (AdoptOpenJDK, Corretto), JRE variants
- C++: VC++ redistributable versions, compilers, cmake

**Key decisions:**
- Boundary rule: in scope = `winget install` or `dotnet tool install -g`. Out of scope = NuGet packages, VS extensions, binaries
- Bun is an alternative runtime within the Node ecosystem, not a separate language
- Node global packages get individual YAML files in the Node directory (not embedded in a collection)

**Branch 2: Catalog Structure**
- Subdirectories for human navigation, `category:` field is source of truth
- Directory path and category don't need to match
- Split pragmatically -- only when a folder exceeds ~20 files

**Branch 3: Relationship Types**
- Three types only: `alternatives` (mutual), `suggests` (soft), `requires` (hard)
- One-way in YAML, Core resolves bidirectional for alternatives

**Branch 4: Richer Metadata**
- Icons: by naming convention next to YAML (`vscode.yaml` + `vscode.png`)
- GitHub stars: separate `catalog/metadata/github-stars.yaml` keyed by type+id, sync script
- Profiles: `profiles: [developer, power-user, casual, gamer, creative]` as filter tags
- OS: `os: [windows]` or `os: [windows, linux, macos]`
- Targets: always explicit, no magic defaults

**Branch 5: External Sources (Ninite Cross-Reference)**

22 apps to add:
Brave, Cursor, Thunderbird, Zoom, Audacity, HandBrake, GIMP, Krita, Blender, Inkscape, SumatraPDF, LibreOffice, KeePass 2, qBittorrent, WinSCP, WinMerge, WizTree, Epic Games, Google Drive, OneDrive, Revo Uninstaller, Open-Shell

19 hidden entries:
PuTTY, Pidgin, Trillian, foobar2000, AIMP, Winamp, MusicBee, MediaMonkey, XnView, FastStone, Foxit Reader, CutePDF, OpenOffice, PeaZip, WinRAR, Launchy, Eclipse, TeamViewer, AnyDesk

Skipped: antivirus (Defender sufficient), disc burning (dead), codec packs (unnecessary), system cleaners (snake oil), SugarSync (dead), Evernote (Obsidian replaces), remote access tools (enterprise niche)

**Branch 6: App-Owned Tweaks**
- Tweaks that only make sense for a specific app live in the app's YAML
- Same schema as standalone tweaks -- one code path in Core
- Core aggregates cross-cutting views (all context menus, all default apps)
- "Default Apps" = app-owned `type: default-app` tweaks, WPF aggregates into unified view

**Branch 7: Generic Bootstrap Capabilities**
- Four primitives any entry can use: symlink/junction (existing), template files (Core scans for `{{perch.xxx}}`), git clone/submodule (apply manifest if present), secrets import from vault
- No per-app wizards -- generic capabilities
- Same pattern for Git, PowerShell, Bash bootstrapping

**Branch 8: Config Consolidation**
- Gallery is source of truth, manifest stores only deviations (confirmed from previous session)

**Branch 9: Taxonomy Rework**
- Languages promoted to ecosystem directories: .NET, Node, Java, C++, Python, Ruby
- New categories: Development/API Tools, Development/Databases, Development/Containers, Communication/Email, Communication/Video, Media/Audio, Media/Video, Media/3D, Productivity/Office, Utilities/PDF, Utilities/Downloads, Utilities/Storage
- `Development/Tools` survives as catch-all
- Docker/Containers: Docker Desktop + Rancher Desktop for now, Kubernetes out of scope
- mitmproxy + Hoppscotch + Postman (hidden) consolidated under API Tools

### Phase 2: Cross-Pollination

Studied Scoop (1,438 manifests), WinUtil (~200 apps), Chocolatey (10,900+ packages).

**From Scoop:**
- Flat structure, no categories -- what perch-gallery is outgrowing
- `persist:` concept (directories to preserve) -- out of scope
- `env_add_path:` / `env_set:` -- backlogged as env/PATH declarations
- `checkver:` / `autoupdate:` -- backlogged as health check CI
- Naming conventions for variants (`nodejs.json`, `nodejs-lts.json`)

**From WinUtil:**
- `foss: true` flag -- evolved into richer `license:` field
- Dual install IDs with `"na"` for unavailable -- already have this pattern
- Very flat metadata proves even minimal data is useful at scale

**From Chocolatey:**
- Tag-based discovery scales better than categories alone -- tags become first-class filter in WPF alongside category tree
- Download counts as popularity signal -- GitHub stars serve this role
- Author/maintainer tracking -- not adopted

**From Homebrew (conceptual):**
- Formula/Cask split inspired `kind:` taxonomy: `app`, `cli-tool`, `runtime`, `dotfile`
- Tap system (third-party catalogs) -- out of scope

**Key finding:** Nobody does relationships or app-owned tweaks. Both are genuine differentiators for Perch.

**Ideas adopted:**
- #16: Env/PATH declarations + vault secrets (backlog)
- #17: `license:` field in YAML, script-assisted
- #18: Tags as first-class filter alongside category tree
- #19: `kind:` taxonomy (app, cli-tool, runtime, dotfile)
- #20: Package health check CI (backlog)

### Phase 3: Morphological Analysis

Mapped 10 axes of the gallery design parameter space:

| Axis | Decision |
|------|----------|
| Schema fields | name, display-name, kind, category, tags, description, profiles, os, hidden, license, links, install, config, tweaks, alternatives, suggests, requires |
| File organization | Grouped by domain, kind is a field, not a directory split |
| Icons | Next to YAML, same name convention (`vscode.png`) |
| External metadata | github-stars.yaml now, health-check.yaml later |
| Tweak ownership | App-owned if app-specific, standalone if OS-level |
| Install mechanisms | winget, choco, node-package, dotnet-tool (extensible) |
| Profiles | developer, power-user, casual, gamer, creative |
| Relationships | One-way in YAML, Core resolves. Alternatives auto-bidirectional. |
| Templates | Core scans for `{{perch.xxx}}` placeholders, no YAML declaration |
| Build/index | Auto-generate index.yaml + schema validation |

## Idea Organization and Prioritization

### Theme 1: Gallery Schema & Entry Design
- Unified tweak schema (app-owned = same as standalone)
- Install mechanisms as flat list (winget, choco, node-package, dotnet-tool)
- Three relationship types (alternatives, suggests, requires) -- one-way, Core resolves
- Profile tags (developer, power-user, casual, gamer, creative)
- License field (script-assisted, lives in YAML)
- Kind taxonomy (app, cli-tool, runtime, dotfile)
- Hidden flag for low-relevance entries
- OS field for platform scope

### Theme 2: Catalog Structure & Organization
- Directory as convenience, category field is truth
- Icon next to YAML by naming convention
- GitHub stars in separate metadata file with sync script
- Languages as first-class ecosystem directories
- Auto-generate index.yaml + schema validation

### Theme 3: Ecosystem Deep-Dives
- .NET: editors, decompilers, profilers, SDKs, global tools, dotfiles, LINQPad
- Node: runtimes, version managers, package managers, global packages, dotfiles
- Git: template .gitconfig, GUI tools, SSH from vault, context menu tweaks
- Java: JDK version/vendor selection
- C++: VC++ redistributables
- Bash/PowerShell: thin dotfile entries

### Theme 4: App-Owned Tweaks & Cross-Cutting Views
- Context menus, file associations, startup items embedded in app YAML
- Core aggregates into cross-cutting views (Default Apps, Context Menus)
- Tags as first-class filter alongside category tree

### Theme 5: Generic Bootstrap Capabilities
- Four primitives: symlink/junction, template files (`{{perch.xxx}}`), git clone, secrets from vault
- Generic capabilities, no per-app custom code

### Theme 6: Volume Expansion
- 22 new apps from Ninite cross-reference
- 19 hidden entries
- Ecosystem entries (.NET tools, Node packages, Git GUIs, Java JDKs, VC++ redists)

### Execution Order

#### Phase 1: Schema & Structure
Foundation that everything else builds on:
1. Define new YAML schema with all fields (kind, profiles, os, hidden, license, tweaks, alternatives, suggests, requires)
2. Set up directory structure with domain subdirectories
3. Build auto-generate index + validation tooling
4. Create github-stars.yaml + sync script

#### Phase 2: Volume Expansion
Quick wins -- prove the schema works at scale:
1. Add 22 new apps with new schema
2. Add 19 hidden entries
3. Enrich existing 55 entries with new fields (profiles, os, license, kind)
4. Add icons for all entries

#### Phase 3: Ecosystem Deep-Dives
Deep value -- self-contained chunks:
1. Work out .NET ecosystem (editors, decompilers, profilers, SDKs, global tools, dotfiles)
2. Work out Node ecosystem (runtimes, managers, global packages)
3. Work out Git ecosystem (template config, GUI tools, context menu tweaks)
4. Add Java and C++ ecosystems (JDKs, VC++ redists)
5. Bash/PowerShell as thin dotfile entries

#### Phase 4: App-Owned Tweaks & Views
WPF integration -- needs Core work:
1. Implement app-owned tweaks with unified schema
2. Core aggregation for cross-cutting views (Default Apps, Context Menus)
3. Tag-based filtering in WPF alongside category tree

#### Phase 5: Bootstrap Capabilities
Advanced -- needs WPF UI work:
1. Template file scanning (`{{perch.xxx}}`)
2. Vault integration for secrets
3. Git clone/submodule with manifest detection

### Backlog
- Env/PATH declarations + vault secrets for env variables
- Package health check CI script
- VSIX installation (if a mechanism is found)
