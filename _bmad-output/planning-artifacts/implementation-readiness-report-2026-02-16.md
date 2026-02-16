---
stepsCompleted: [step-01-document-discovery, step-02-prd-analysis, step-03-epic-coverage-validation, step-04-ux-alignment, step-05-epic-quality-review, step-06-final-assessment]
filesIncluded:
  prd: '_bmad-output/planning-artifacts/prd.md'
  architecture: '_bmad-output/planning-artifacts/architecture.md'
  epics: '_bmad-output/planning-artifacts/epics.md'
  ux: '_bmad-output/planning-artifacts/ux-design-specification.md'
---

# Implementation Readiness Assessment Report

**Date:** 2026-02-16
**Project:** Perch

## Document Inventory

### PRD
- **File:** `prd.md`
- **Status:** Updated in this session — WPF Desktop app additions (previously referenced MAUI)

### Architecture
- **File:** `architecture.md`
- **Status:** Up to date — includes WPF Desktop decisions (#8-12), project structure, DI host, MVVM patterns

### Epics & Stories
- **File:** `epics.md`
- **Status:** Up to date — includes Desktop Epic 10 (Wizard & Onboarding), Epic 11 (Dashboard & Drift), renumbered Epic 12 (Migration)

### UX Design Specification
- **File:** `ux-design-specification.md`
- **Status:** Up to date — complete WPF Desktop UX specification (design system, journeys, components, accessibility)

### Issues Resolved
- PRD was missing WPF Desktop requirements (still referenced MAUI) — updated before assessment
- No duplicate documents found
- No missing required documents

## PRD Analysis

### Functional Requirements

**Manifest & Module Management**
- FR1: User can define an app module by creating a named folder containing a manifest file and config files [Scope 1]
- FR2: System discovers all app modules automatically by scanning for manifest files in the config repo (no central registration) [Scope 1]
- FR3: User can specify in a manifest where config files should be symlinked to, using platform-appropriate path variables (`%AppData%`, `$HOME`, `$XDG_CONFIG_HOME`, etc.) [Scope 1]
- FR4: System resolves pattern-based/glob config paths for apps with dynamic settings locations [Scope 2]
- FR5: User can specify version-range-aware symlink paths in a manifest [Scope 3]
- FR6: User can pull manifest templates from an external repository/gallery [Scope 3]
- FR41: System supports platform-aware target paths in manifests — different target locations per OS from a single module [Scope 2]
- FR42: User can mark modules as platform-specific — system only processes them on matching OS [Scope 2]

**Symlink Engine**
- FR7: System creates symlinks (and junctions on Windows) from config repo files to target locations on the filesystem. If a target file already exists, it is moved to `.backup` before creating the symlink [Scope 1]
- FR8: System re-runs deploy without affecting existing symlinks — only modules without an existing symlink are processed [Scope 1]
- FR9: System detects locked target files and reports them [Scope 2]
- FR10: System detects drift between expected and actual symlink state [Scope 2]
- FR11: System performs dry-run showing what would change without modifying the filesystem [Scope 2]
- FR12: System creates full pre-deploy backup snapshots of all target files [Scope 2]
- FR13: User can restore files from a backup (conflict `.backup` or full snapshot) [Scope 3]

**CLI Interface**
- FR14: User can run a deploy command that processes all discovered modules [Scope 1]
- FR15: System streams each action to the console in real-time with colored status indicators [Scope 1]
- FR16: System returns clean exit codes indicating success or specific failure types [Scope 1]
- FR17: User can abort execution mid-deploy via Ctrl+C (graceful shutdown — current module completes, then deploy halts) [Scope 1]
- FR18: System outputs structured JSON results for machine consumption [Scope 2]
- FR19: System displays a live-updating progress table alongside action streaming [Scope 2]
- FR20: User can run deploy in interactive mode with step-level and command-level confirmation [Scope 3]
- FR21: User can tab-complete Perch commands in the shell [Scope 3]

**Package Management**
- FR22: User can define all managed packages in a single manifest file, supporting chocolatey and winget with per-package manager specification [Scope 2]
- FR23: System detects installed apps and cross-references against managed modules [Scope 2]
- FR24: System reports apps installed but without a config module [Scope 2]
- FR48: User can define packages for cross-platform package managers (apt, brew, VS Code extensions, npm/bun global packages) using the same manifest format [Scope 3]

**Git Integration**
- FR25: System registers per-app git clean filters to suppress noisy config diffs [Scope 2]
- FR26: System performs before/after filesystem diffing to discover config file changes [Scope 2]

**App Discovery & Onboarding**
- FR27: User can scan the system for installed apps and see which have config modules [Scope 2]
- FR28: System looks up known config file locations for popular apps [Scope 3]
- FR29: System launches an app in Windows Sandbox to discover its config locations [Scope 3]
- FR30: User can generate a new module manifest via interactive onboarding workflow (CLI or Desktop) [Scope 3]

**Machine Configuration**
- FR31: User can define base config values with per-machine overrides [Scope 3]
- FR32: User can specify which modules apply to which machines [Scope 3]
- FR33: User can manage Windows registry settings declaratively [Scope 3]
- FR34: System applies and reports on registry state (context menus, default programs, power settings, etc.) [Scope 3]

**Secrets Management**
- FR43: System can inject secrets from a supported password manager into config files at deploy time, producing generated (non-symlinked) files [Scope 3]
- FR44: User can define secret placeholders in config templates that are resolved from a configured password manager during deploy [Scope 3]
- FR45: System manages any config file containing secret placeholders as a generated (non-symlinked) file [Scope 3]

**Desktop UI (WPF)**
- FR35: User can view sync status of all managed modules in a visual dashboard with drift hero banner showing aggregate health (linked/attention/broken counts) and attention cards grouped by severity with one-click fix actions [Scope 3]
- FR36: User can interactively explore an app's filesystem to find config locations [Scope 3 — future]
- FR37: User can generate and edit module manifests via a visual interface [Scope 3 — future]
- FR49: User is guided through a first-run wizard: profile selection (Developer/Power User/Gamer/Casual) drives which steps and content are shown, with card-based browsing and toggling of detected configs, and a final review + deploy step [Scope 3]
- FR50: Desktop app detects installed apps and existing config files on the filesystem and presents them as cards in a three-tier layout: "Your Apps" (detected), "Suggested for You" (profile-based), "Other Apps" (gallery) [Scope 3]
- FR51: Card-based view components (Apps, Dotfiles, System Tweaks) are shared between wizard steps and dashboard sidebar pages [Scope 3]
- FR52: User can deploy selected configs from the Desktop app with per-card progress feedback and a contextual deploy bar showing selection count and deploy action [Scope 3]
- FR53: Desktop app supports card grid and compact list display modes with a density toggle [Scope 3]

**Plugin Lifecycle**
- FR38: User can define pre-deploy and post-deploy hooks per module [Scope 2]

**Engine Configuration**
- FR39: User can specify the config repo location as a CLI argument [Scope 1]
- FR40: System persists the config repo location (settings file alongside engine) so it doesn't need to be specified on subsequent runs [Scope 1]

**Migration & Compatibility**
- FR46: System can import/convert a chezmoi-managed dotfiles repo into Perch format (manifests + plain config files) [Scope 4]
- FR47: System can import/convert Dotbot and Dotter repos into Perch format, and export Perch format to those tools (two-way migration) [Scope 4]

**Total FRs: 53** (FR1-FR53, numbering is non-sequential due to additions over time)

**By Scope:**
- Scope 1 (MVP): 11 FRs — FR1-3, FR7-8, FR14-17, FR39-40
- Scope 2: 14 FRs — FR4, FR9-12, FR18-19, FR22-27, FR38, FR41-42
- Scope 3: 22 FRs — FR5-6, FR13, FR20-21, FR28-37, FR43-45, FR48-53
- Scope 3 (future): 2 FRs — FR36, FR37
- Scope 4: 2 FRs — FR46-47

### Non-Functional Requirements

**Reliability**
- NFR1: On Ctrl+C, the in-progress module completes fully (backup + symlink creation), then deploy halts. No module is ever left in a partial state
- NFR2: Failed symlink operations for one module generate an error (logged and displayed), but do not prevent other modules from processing
- NFR3: Missing target directories are logged to the deploy context and displayed to the user. The affected module is skipped, deploy continues

**Maintainability**
- NFR4: Human-readable codebase following KISS and YAGNI principles. Max cyclomatic complexity enforced via analyzers. TDD development approach
- NFR5: Platform-specific logic abstracted behind interfaces with separate implementations per platform. Core engine depends only on interfaces, never on platform implementations directly
- NFR6: 100% unit test coverage on core engine logic (symlink creation, manifest parsing, module discovery). All branches and flows tested [Scope 2]
- NFR7: CI pipeline fails on any failing test, any analyzer warning, or any compiler warning. Static analysis via Roslynator and Microsoft.CodeAnalysis.Analyzers with warnings treated as errors. CI runs on Windows and Linux runners [Scope 2]

**Portability**
- NFR8: Scope 1: runs on Windows 10+ with .NET 10 runtime
- NFR9: Scope 2: runs on Windows 10+, Linux (major distros), and macOS with .NET 10 runtime
- NFR10: No dependency on specific shell (PowerShell, cmd, bash, zsh all work)
- NFR11: Config repo format: plain files + YAML manifests — no binary formats, no database, no proprietary encoding
- NFR12: `dotnet tool install perch -g` works on all supported platforms [Scope 2]
- NFR13: WPF Desktop app is Windows-only; CLI remains cross-platform [Scope 3]

**Total NFRs: 13**

### Additional Requirements

**From PRD Domain-Specific Requirements:**
- Dynamic config paths require glob/pattern matching (Scope 2)
- Special folder path variables must be supported per-platform
- Symlink conflict resolution: existing files moved to `.backup`
- File locking detection during deploy (Scope 2)
- Short root path recommendation documented for Windows

**From PRD Scoping:**
- Manifest format is YAML (Architecture Decision #1 changed from original JSON)
- Engine/config repo split — engine is open-sourceable, config is personal
- Config repo location specified via CLI arg or persisted setting
- No hard-coded Windows assumptions in core engine — even in Scope 1

**From PRD Risk Mitigation:**
- Existing repo from previous AI session needs assessment before building on it
- Symlink permissions on Windows to be verified on new machine early

### PRD Completeness Assessment

**Strengths:**
- All FRs explicitly numbered with scope assignments
- NFRs have verification criteria
- User journeys cover all scopes with clear capability mapping
- Journey Requirements Summary table provides traceability from capabilities to journeys
- Phase boundaries are well-defined with explicit "NOT in MVP" list
- Desktop UI requirements now fully captured (FR35, FR49-53) with dedicated journeys (J4, J9)

**Observations:**
- FR numbering is non-sequential (FR1-48 original, FR49-53 added for Desktop) — functional but could cause confusion
- FR36 and FR37 are marked "future" with no scope timeline — these are placeholders rather than actionable requirements
- The PRD references "JSON manifests" in the Config Schema section but adds a note about YAML — the architecture decision overrides this
- Journey 4b (AI-Assisted Discovery) overlaps with Epic 9 stories but has no dedicated Desktop FRs — it's CLI + Desktop without a clear Desktop-specific FR for the interactive explorer beyond FR36 (future)

## Epic Coverage Validation

### Coverage Matrix

| FR | PRD Requirement (abbreviated) | Epic | Story | Status |
|----|------|------|-------|--------|
| FR1 | Module definition via folder + manifest | 1 | 1.2 | Covered |
| FR2 | Auto-discovery by scanning for manifests | 1 | 1.3 | Covered |
| FR3 | Platform path variables in manifests | 1 | 1.2 | Covered |
| FR4 | Glob/pattern-based dynamic paths | 2 | 2.3 | Covered |
| FR5 | Version-range-aware manifest paths | 9 | 9.1 | Covered |
| FR6 | Manifest templates from gallery | 9 | 9.5 | Covered |
| FR7 | Symlink/junction creation with backup | 1 | 1.4 | Covered |
| FR8 | Re-runnable additive deploy | 1 | 1.4 | Covered |
| FR9 | Locked file detection | 3 | 3.3 | Covered |
| FR10 | Drift detection | 3 | 3.2 | Covered |
| FR11 | Dry-run mode | 3 | 3.1 | Covered |
| FR12 | Pre-deploy backup snapshots | 3 | 3.4 | Covered |
| FR13 | Restore from backup | 8 | 8.1 | Covered |
| FR14 | Deploy command | 1 | 1.6 | Covered |
| FR15 | Real-time colored output | 1 | 1.6 | Covered |
| FR16 | Clean exit codes | 1 | 1.6 | Covered |
| FR17 | Graceful Ctrl+C shutdown | 1 | 1.6 | Covered |
| FR18 | Structured JSON output | 3 | 3.5 | Covered |
| FR19 | Live progress table | 3 | 3.6 | Covered |
| FR20 | Interactive deploy mode | 8 | 8.2 | Covered |
| FR21 | Shell tab-completion | 8 | 8.3 | Covered |
| FR22 | Package manifest (choco/winget) | 4 | 4.1 | Covered |
| FR23 | Installed app cross-reference | 4 | 4.2 | Covered |
| FR24 | Missing config module reporting | 4 | 4.3 | Covered |
| FR25 | Per-app git clean filters | 5 | 5.1 | Covered |
| FR26 | Before/after filesystem diffing | 5 | 5.2 | Covered |
| FR27 | Installed app scanning | 4 | 4.2 | Covered |
| FR28 | AI config path lookup | 9 | 9.2 | Covered |
| FR29 | Windows Sandbox discovery | 9 | 9.3 | Covered |
| FR30 | Interactive manifest generation (CLI or Desktop) | 9 | 9.4 | Covered |
| FR31 | Per-machine overrides | 6 | 6.1 | Covered |
| FR32 | Module-to-machine filtering | 6 | 6.2 | Covered |
| FR33 | Declarative registry management | 6 | 6.3 | Covered |
| FR34 | Registry state reporting | 6 | 6.4 | Covered |
| FR35 | Desktop drift dashboard with hero banner, attention cards, one-click fix | 11 | 11.1 | Covered |
| FR36 | Desktop filesystem explorer (future) | 11 | — | Covered (future) |
| FR37 | Desktop visual manifest editor (future) | 11 | — | Covered (future) |
| FR38 | Pre/post-deploy lifecycle hooks | 5 | 5.3 | Covered |
| FR39 | Config repo path via CLI argument | 1 | 1.5 | Covered |
| FR40 | Persisted config repo path | 1 | 1.5 | Covered |
| FR41 | Platform-aware target paths | 2 | 2.1 | Covered |
| FR42 | Platform-specific module filtering | 2 | 2.2 | Covered |
| FR43 | Secret injection from password manager | 7 | 7.2 | Covered |
| FR44 | Secret placeholder syntax | 7 | 7.1 | Covered |
| FR45 | Generated files for secret configs | 7 | 7.3 | Covered |
| FR46 | Chezmoi import/conversion | 12 | 12.1 | Covered |
| FR47 | Dotbot/Dotter import/export | 12 | 12.2, 12.3 | Covered |
| FR48 | Cross-platform package managers | 8 | 8.4 | Covered |
| FR49 | Desktop wizard with profile selection and dynamic steps | 10 | 10.2, 10.4 | Covered (stories exist, FR Coverage Map outdated) |
| FR50 | Desktop detection-first three-tier card layout | 10 | 10.3 | Covered (stories exist, FR Coverage Map outdated) |
| FR51 | Shared card-based view components (wizard + dashboard) | 10+11 | 10.3, 11.2 | Covered (stories exist, FR Coverage Map outdated) |
| FR52 | Desktop deploy with per-card progress and deploy bar | 10+11 | 10.4, 11.2 | Covered (stories exist, FR Coverage Map outdated) |
| FR53 | Desktop card grid and compact list density toggle | 10+11 | 10.3, 11.2 | Covered (stories exist, FR Coverage Map outdated) |

### NFR Coverage

| NFR | Requirement (abbreviated) | Epic Coverage | Status |
|-----|---------------------------|---------------|--------|
| NFR1 | Graceful Ctrl+C — current module completes | Epic 1 (Story 1.6) | Covered |
| NFR2 | Fault isolation — one module failure doesn't block others | Epic 1 (Stories 1.4, 1.6) | Covered |
| NFR3 | Missing target dirs logged, module skipped | Epic 1 (Story 1.4) | Covered |
| NFR4 | KISS/YAGNI, analyzers, TDD | Cross-cutting (Story 1.1) | Covered |
| NFR5 | Platform abstraction via interfaces | Epic 1 (Story 1.4) | Covered |
| NFR6 | 100% unit test coverage on core engine | Epic 3 (Story 3.7 CI) | Covered |
| NFR7 | CI fails on warnings/test failures | Epic 3 (Story 3.7) | Covered |
| NFR8 | Runs on Windows 10+ | Epic 1 (implicit) | Covered |
| NFR9 | Cross-platform Windows/Linux/macOS | Epic 2 | Covered |
| NFR10 | Shell-agnostic | Cross-cutting | Covered |
| NFR11 | Plain files + YAML — no binary formats | Epic 1 (Story 1.2) | Covered |
| NFR12 | dotnet tool install works cross-platform | Epic 2 (Story 2.4) | Covered |
| NFR13 | Desktop is Windows-only, CLI cross-platform | Epic 10+11 (implicit) | Covered (not in epics NFR list) |

### Missing Requirements

#### Documentation Gaps (not functional gaps)

**FR49-FR53 missing from epics FR Coverage Map table:**
The epics document's FR Coverage Map (the table mapping FRs to epics) only lists FR1-FR48. The new Desktop FRs (FR49-53) added to the PRD are NOT in the FR Coverage Map. However, the epic stories themselves DO fully cover these requirements — this is a documentation sync gap, not a functional gap.
- **Impact:** Low — traceability table is incomplete, but the actual stories are well-written
- **Recommendation:** Update the FR Coverage Map table in `epics.md` to add FR49-53 mappings

**FR35 expanded but epics FR description not updated:**
FR35 was expanded from "visual dashboard" to include drift hero banner, attention cards, severity grouping, and one-click fix actions. Epic 11 Story 11.1 fully covers the expanded scope, but the epics Requirements Inventory section still uses the shorter description.
- **Impact:** Low — story content is correct, inventory header is stale
- **Recommendation:** Update the epics Requirements Inventory to match the expanded FR35 text

**NFR13 not in epics NFR section:**
NFR13 (Desktop is Windows-only; CLI remains cross-platform) is new and not listed in the epics NFR section.
- **Impact:** Low — this is an implicit constraint of using WPF
- **Recommendation:** Add NFR13 to the epics NFR list for completeness

#### No Functional Gaps

All 53 FRs have corresponding epic stories. No PRD requirement is missing implementation coverage.

### Coverage Statistics

- **Total PRD FRs:** 53
- **FRs covered in epics (via stories):** 53
- **FRs in epics FR Coverage Map table:** 48 (FR1-FR48)
- **FRs covered in stories but missing from Coverage Map:** 5 (FR49-FR53)
- **Coverage percentage:** 100% functional, 90.6% documented in traceability table
- **Total NFRs:** 13
- **NFRs covered:** 13 (12 explicitly, 1 implicitly)
- **NFRs in epics but not in PRD:** 0
- **FRs in epics but not in PRD:** 0

## UX Alignment Assessment

### UX Document Status

**Found:** `ux-design-specification.md` — comprehensive 14-step UX design specification, completed 2026-02-16. Covers executive summary, core UX, emotional design, pattern analysis, design system, defining experience, visual design, design direction, user journeys, component strategy, consistency patterns, responsive design, and accessibility.

### UX ↔ PRD Alignment

**Strong alignment after PRD update.** Key mappings:

| UX Concept | PRD Requirement | Status |
|------------|----------------|--------|
| Wizard onboarding (profile → detect → browse → deploy) | FR49 | Aligned |
| Detection-first three-tier card layout | FR50 | Aligned |
| Shared views (wizard + dashboard) | FR51 | Aligned |
| Per-card deploy progress + DeployBar | FR52 | Aligned |
| Grid/list density toggle | FR53 | Aligned |
| Drift hero banner + attention cards | FR35 (expanded) | Aligned |
| Profile-driven content filtering | FR49 | Aligned |
| Card-based interaction model | FR50 | Aligned |
| WPF UI + HandyControl + CommunityToolkit.Mvvm | PRD Tech Stack | Aligned |
| Dark Fluent theme, forest green accent | PRD Desktop UI section | Aligned |

**UX user journeys vs PRD journeys:**
- UX Journey 1 (Wizard Onboarding) → PRD Journey 4 (Desktop Wizard): Aligned
- UX Journey 2 (Dashboard Drift Resolution) → PRD Journey 9 (Desktop Dashboard): Aligned
- UX Journey 3 (App Discovery & Onboarding) → PRD Journey 4b + J8: Aligned

### UX ↔ Architecture Alignment

**Strong alignment.** Key mappings:

| UX Design Decision | Architecture Support | Status |
|-------------------|---------------------|--------|
| NavigationView sidebar (Home, Dotfiles, Apps, System Tweaks, Settings) | Architecture project tree: DashboardPage, DotfilesPage, AppsPage, SystemTweaksPage, SettingsPage | Aligned |
| Wizard with StepBar header | Architecture: WizardShell.xaml with HandyControl StepBar | Aligned |
| Shared UserControls (AppsView, DotfilesView, SystemTweaksView) | Architecture: Views/Controls/ folder with shared UserControls | Aligned |
| Custom components (StatusRibbon, ProfileCard, AppCard, etc.) | Architecture: Views/Controls/ lists all 9 custom components | Aligned |
| MVVM with CommunityToolkit.Mvvm | Architecture: ViewModels/ folder, [ObservableProperty], [RelayCommand] | Aligned |
| IProgress<DeployResult> for per-card progress | Architecture: Desktop deploy data flow diagram | Aligned |
| Singleton pages for dashboard, Transient for wizard steps | Architecture: DI Lifetimes section | Aligned |
| Generic Host DI in App.xaml.cs | Architecture Decision #12 | Aligned |
| Core engine via AddPerchCore() — same as CLI | Architecture: Architectural Boundaries section | Aligned |
| Dark Fluent theme with #10B981 accent | Architecture: Story 10.1 mentions forest green accent | Aligned |

### Alignment Issues

**Minor — ViewLocator vs INavigableView<T>:**
UX spec mentions "ViewLocator pattern for ViewModel → View resolution (same approach as Avalonia prototype)." Architecture uses `INavigableView<TViewModel>` from WPF UI instead. These serve the same purpose but are different mechanisms. The Architecture approach is correct for WPF UI — the UX spec carried over Avalonia terminology.
- **Impact:** Very low — implementation detail, not a functional gap
- **Recommendation:** Update UX spec to reference `INavigableView<T>` instead of ViewLocator

**Minor — Gallery dependency for detection:**
UX describes detection as "filesystem-based using known config paths from the gallery." This creates a soft dependency on gallery data being available for the "Suggested for You" and "Other Apps" tiers. Architecture notes gallery as future/network dependency. The UX Journey 3 flow diagram acknowledges "Gallery requires network for initial download but caches locally afterward."
- **Impact:** Low — "Your Apps" tier (local detection) works independently. Gallery tiers degrade gracefully
- **Recommendation:** Ensure Epic 10 Story 10.3 explicitly handles the "no gallery data" empty state

### Warnings

**Accessibility not captured as PRD NFR:**
UX design specification includes detailed accessibility requirements (WCAG AA contrast ratios, keyboard navigation, screen reader support via AutomationProperties, 36x36px hit targets, High Contrast mode support) in the "Responsive Design & Accessibility" section. These are NOT captured as NFRs in the PRD. The UX spec is the only place accessibility requirements are documented.
- **Impact:** Medium — accessibility requirements could be overlooked if PRD is the primary implementation reference
- **Recommendation:** Consider adding an Accessibility NFR to the PRD (e.g., NFR14: Desktop app meets WCAG AA equivalent — keyboard-navigable, screen-reader compatible, High Contrast supported)

**No misalignment between UX emotional design goals and PRD success criteria:**
UX emotional goals (confidence, impressed, relief, closure) are design guidelines, not testable requirements. PRD success criteria are measurable. These are complementary, not competing.

### UX Alignment Summary

- **Overall alignment: Strong** — all three documents are consistent on technology choices, component strategy, user journeys, and architectural patterns
- **Critical misalignments: 0**
- **Minor misalignments: 2** (ViewLocator terminology, gallery dependency handling)
- **Warnings: 1** (accessibility NFR gap)

## Epic Quality Review

### Epic-Level Validation

#### User Value Focus

| Epic | Title | User Value? | Assessment |
|------|-------|-------------|------------|
| 1 | Deploy Managed Configs | "Developer clones config repo, runs deploy, configs symlinked" | Clear user value |
| 2 | Cross-Platform Deploy | "Developer can deploy configs on Linux and macOS" | Clear user value |
| 3 | Deploy Safety & Observability | "Developer can preview changes, detect drift, see progress" | Clear user value |
| 4 | Package & App Awareness | "Developer tracks packages, discovers unmanaged apps" | Clear user value |
| 5 | Git Integration & Deploy Hooks | "Developer suppresses noisy diffs, runs deploy scripts" | Clear user value |
| 6 | Multi-Machine & Registry | "Developer defines per-machine overrides, manages registry" | Clear user value |
| 7 | Secrets Management | "Developer manages configs with secrets via templates" | Clear user value |
| 8 | Advanced Usability | "Developer restores backups, interactive mode, tab-completion" | Clear user value |
| 9 | App Onboarding & Discovery | "Developer discovers config locations, generates manifests" | Clear user value |
| 10 | Desktop Wizard & Onboarding | "User is guided through wizard, card-based browsing, deploy" | Clear user value |
| 11 | Desktop Dashboard & Drift | "Returning user sees drift dashboard, one-click fix actions" | Clear user value |
| 12 | Migration Tools | "Users of chezmoi/Dotbot/Dotter can import/export" | Clear user value |

**Result: All 12 epics deliver user value. No technical-milestone epics found.**

#### Epic Independence Validation

| Epic | Depends On | Forward Dependency? | Assessment |
|------|-----------|---------------------|------------|
| 1 | None | No | Standalone |
| 2 | Epic 1 (deploy engine) | No | Uses Epic 1 output |
| 3 | Epic 1 (deploy engine) | No | Uses Epic 1 output |
| 4 | Epic 1 (config repo modules) | No | Uses Epic 1 output |
| 5 | Epic 1 (modules, deploy) | No | Uses Epic 1 output |
| 6 | Epic 1 (deploy engine) | No | Uses Epic 1 output |
| 7 | Epic 1 (deploy engine) | No | Uses Epic 1 output |
| 8 | Epic 1 + **Epic 3** (snapshots) | No | Backward deps |
| 9 | Epic 1 (manifest format) | No | Uses Epic 1 output |
| 10 | Epic 1 (Perch.Core engine) | No | Uses Epic 1 output |
| 11 | Epic 10 (Desktop project) + **Epic 3** (drift detection) | No | Backward deps |
| 12 | Epic 1 (manifest format) | No | Uses Epic 1 output |

**Result: No forward dependencies. All dependencies are backward (Epic N depends on earlier epics). Two cross-epic dependencies need documentation (see Major Issues).**

### Story-Level Validation

#### Story Sizing & Independence

**Epic 1 Stories (1.1-1.6):**
- 1.1 (Initialize Project Structure) — Technical scaffolding story. Accepted pattern for greenfield. Creates empty structure; does NOT front-load all models/services. Each subsequent story creates what it needs.
- 1.2-1.5 — Each story is independently completable using prior story outputs. Proper BDD acceptance criteria. Well-scoped.
- 1.6 (Deploy Command) — Integration story pulling together 1.2-1.5 into the end-to-end deploy pipeline. Appropriately placed as the final story.

**Epic 2 Stories (2.1-2.4):** All well-structured. Platform-aware paths, module filtering, dynamic paths, dotnet tool packaging — each independently completable.

**Epic 3 Stories (3.1-3.7):** All well-structured. 3.7 (CI Pipeline) is infrastructure but appropriate for this scope.

**Epic 4 Stories (4.1-4.3):** Clean. Package manifest, app detection, reporting.

**Epic 5 Stories (5.1-5.3):** Clean. Git clean filters, diffing, hooks.

**Epic 6 Stories (6.1-6.4):** Clean. Machine overrides, module filtering, registry management/reporting.

**Epic 7 Stories (7.1-7.3):** Clean. Placeholder syntax, password manager integration, generated files.

**Epic 8 Stories (8.1-8.4):** Clean. Note: Story 8.1 (Restore) references `--snapshot` which depends on Epic 3 Story 3.4 (pre-deploy snapshots). This is a backward dependency but should be explicitly noted.

**Epic 9 Stories (9.1-9.5):** Clean. Version-range paths, AI lookup, sandbox, manifest generation, gallery.

**Epic 10 Stories (10.1-10.4):**
- 10.1 (Initialize Desktop Project) — Technical scaffolding. Same accepted pattern as 1.1. Creates WPF project shell without front-loading logic.
- 10.2 (Profile Selection) — Well-scoped. 4 ACs covering multi-select, dynamic steps, no-selection.
- 10.3 (Card-Based Config Views) — Largest story: 7 ACs covering detection, toggle, expand, search, density, reuse. Could potentially split into "card display + toggle" and "search + density + reuse" but the ACs form a coherent interaction unit. Acceptable.
- 10.4 (Wizard Flow & Deploy) — 5 ACs covering StepBar, review, deploy progress, completion, window close. Coherent.

**Epic 11 Stories (11.1-11.3):**
- 11.1 (Dashboard Home & Drift Summary) — 5 ACs. Well-structured. Implicitly depends on drift detection from Core (Epic 3).
- 11.2 (Dashboard Card Pages) — Reuses shared views from Epic 10. 4 ACs.
- 11.3 (Settings & Configuration) — 3 ACs. Clean.

**Epic 12 Stories (12.1-12.3):** Clean. Import chezmoi, import dotbot/dotter, export.

#### Acceptance Criteria Quality

| Quality Aspect | Assessment |
|---------------|------------|
| Given/When/Then format | Consistent across all stories |
| Testable criteria | All ACs specify verifiable outcomes |
| Error scenarios covered | Present in most stories (manifest parse errors, missing dirs, locked files, missing scripts) |
| Edge cases | Good coverage (empty repos, no matches, platform mismatches, no selection) |
| Specificity | Strong — exit codes specified, color indicators named, file paths described |

### Best Practices Compliance

| Check | Epic 1 | Epic 2 | Epic 3 | Epic 4 | Epic 5 | Epic 6 | Epic 7 | Epic 8 | Epic 9 | Epic 10 | Epic 11 | Epic 12 |
|-------|--------|--------|--------|--------|--------|--------|--------|--------|--------|---------|---------|---------|
| User value | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass |
| Independent | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Note | Pass |
| Story sizing | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Note | Pass | Pass |
| No forward deps | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass |
| Entities created when needed | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass |
| Clear ACs | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass |
| FR traceability | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Pass | Note | Note | Pass |

### Findings by Severity

#### Critical Violations

**None.**

#### Major Issues

**1. Epic 11 has an undocumented dependency on Epic 3 (drift detection)**
Story 11.1 (Dashboard Home & Drift Summary) displays drift state: "X linked, Y attention, Z broken." The drift detection logic (FR10) lives in Epic 3 Story 3.2. If Epic 11 is implemented before Epic 3, the dashboard has no drift data to display.
- **Impact:** High — the entire dashboard home experience depends on drift detection
- **Recommendation:** Add an explicit note to Epic 11 header: "Requires: Epic 3 (drift detection) or equivalent Core-level status checking." Alternatively, ensure Story 11.1 implements basic status checking (symlink exists/missing) as a lightweight form of drift that doesn't require Epic 3's full `perch status` command.

**2. Story 8.1 (Restore) has a cross-epic dependency on Epic 3 Story 3.4 (snapshots)**
The `--snapshot` flag in Story 8.1 requires pre-deploy backup snapshots from Story 3.4. The story also covers `.backup` file restoration which only requires Epic 1.
- **Impact:** Medium — part of the story (snapshot restore) depends on Epic 3, while another part (.backup restore) is independent
- **Recommendation:** Note the dependency explicitly in Story 8.1, or split into two stories: 8.1a (restore from .backup, needs Epic 1) and 8.1b (restore from snapshot, needs Epic 3).

#### Minor Concerns

**1. Stories 1.1 and 10.1 are technical scaffolding stories**
These are "Initialize Project Structure" stories with no direct user value. Accepted pattern for greenfield projects, and the Architecture document explicitly mandates this approach. Both create empty shells without front-loading logic.
- **Recommendation:** No change needed — accepted pattern.

**2. Story 10.3 (Card-Based Config Views) is large — 7 ACs**
This story covers detection display, toggle, expand, search, density toggle, and view reuse. Each AC is well-defined, but the story represents significant implementation effort.
- **Recommendation:** Consider splitting if implementation reveals the story is too large for a single sprint. The ACs form a natural split: "card display + interaction" (4 ACs) and "search + density + reuse" (3 ACs). Leave as-is for now; split during sprint planning if needed.

**3. FR Coverage Map in epics is stale for FR49-53 and expanded FR35**
Already noted in Step 3. The stories cover the requirements, but the traceability table needs updating.
- **Recommendation:** Update the FR Coverage Map in `epics.md` to add FR49-53 and update FR35 description.

**4. Epic 11 references "FR36 (future)" and "FR37 (future)" without stories**
These are explicitly marked as future in the PRD and have no implementation stories. The epic header lists them but no stories implement them.
- **Recommendation:** No change needed — these are correctly deferred. The epic header acknowledging them prevents them from being forgotten.

## Summary and Recommendations

### Overall Readiness Status

**READY** — with minor remediation items.

The planning artifacts are comprehensive, well-aligned, and ready for implementation. All 53 FRs have traceable implementation paths through 12 epics and 39 stories. The architecture, UX design specification, and epics are consistent in technology choices, component strategy, and patterns. No critical violations were found. The two major issues are dependency documentation gaps — the actual functionality is correctly planned, but the dependency relationships between epics need to be made explicit.

### All Findings Summary

| Category | Critical | Major | Minor | Warnings |
|----------|----------|-------|-------|----------|
| FR Coverage | 0 | 0 | 1 (FR Coverage Map stale) | 0 |
| UX Alignment | 0 | 0 | 2 (ViewLocator term, gallery handling) | 1 (accessibility NFR) |
| Epic Quality | 0 | 2 (Epic 11 drift dep, Story 8.1 snapshot dep) | 4 (scaffolding stories, story size, FR map, future FRs) | 0 |
| **Total** | **0** | **2** | **7** | **1** |

### Issues Requiring Action Before Implementation

**1. Update epics FR Coverage Map (5 min)**
Add FR49-53 to the coverage map table. Update FR35 description to match expanded PRD text. Add NFR13.

**2. Document Epic 11's dependency on Epic 3 (2 min)**
Add a prerequisites note to Epic 11 header stating it requires drift detection from Epic 3 (or a lightweight status check in Core). This prevents confusion if someone attempts Epic 11 before Epic 3 is complete.

**3. Document Story 8.1's cross-epic dependency (2 min)**
Add a note to Story 8.1 that the `--snapshot` restore path requires Epic 3 Story 3.4. The `.backup` restore path is independent.

### Recommended But Not Blocking

**4. Add accessibility NFR to PRD**
UX spec has detailed accessibility requirements (WCAG AA, keyboard nav, screen reader, High Contrast) that aren't captured as a PRD NFR. Consider adding NFR14 for completeness.

**5. Update UX spec ViewLocator reference**
Replace "ViewLocator pattern" with "INavigableView<T> pattern" to match the actual WPF UI approach.

**6. Consider splitting Story 10.3 during sprint planning**
7 ACs is large. If it proves too big during implementation, split into "card display + interaction" and "search + density + reuse."

### Readiness Scorecard

| Dimension | Score | Notes |
|-----------|-------|-------|
| PRD completeness | 9/10 | All FRs numbered and scoped. Minor: accessibility NFR gap |
| Architecture alignment | 10/10 | Fully consistent with PRD and UX. Clear boundaries, DI strategy, project structure |
| Epic coverage | 10/10 | 100% FR coverage via stories. Documentation table needs update |
| Epic quality | 9/10 | All user-value focused, proper BDD ACs. Minor: 2 undocumented cross-epic dependencies |
| UX alignment | 9/10 | Strong three-way alignment. Minor terminology inconsistency |
| Story readiness | 9/10 | 39 stories with clear ACs. One large story may need splitting |
| **Overall** | **9.3/10** | **Ready for implementation** |

### Final Note

This assessment identified 10 items across 4 categories (0 critical, 2 major, 7 minor, 1 warning). The major issues are documentation gaps in dependency relationships — the actual functionality is correctly planned and covered. Address items 1-3 before starting implementation (estimated 10 minutes of edits). Items 4-6 are recommended improvements that can be addressed during implementation.

The Perch project artifacts are well-prepared for Phase 4 implementation, starting with Epic 1 (Deploy Managed Configs).
