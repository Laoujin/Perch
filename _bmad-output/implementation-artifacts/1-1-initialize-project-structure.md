# Story 1.1: Initialize Project Structure

Status: ready-for-dev

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want the Perch solution scaffolded with all projects, dependencies, and analyzers configured,
So that I have a buildable, testable foundation to implement features against.

## Acceptance Criteria

1. **Given** a clean repository **When** the solution is initialized **Then** the solution contains `Perch.Core` (classlib), `Perch.Cli` (console), and `Perch.Core.Tests` (nunit) projects targeting net10.0
2. `Perch.Cli` references `Perch.Core`
3. `Perch.Core.Tests` references `Perch.Core`
4. `Perch.Cli` has Spectre.Console (0.54.0) and Spectre.Console.Cli (0.53.1) packages
5. `Perch.Core` has YamlDotNet package
6. `Perch.Core.Tests` has NSubstitute (5.3.0) package
7. Roslynator and Microsoft.CodeAnalysis.Analyzers are configured with warnings as errors
8. `dotnet build` succeeds with zero warnings
9. `dotnet test` runs and passes
10. `Perch.Core` has feature folders: Modules/, Symlinks/, Deploy/, Config/, Backup/
11. `Perch.Cli` has Commands/ folder
12. `Perch.Core` exposes an `AddPerchCore()` DI extension method (can be empty registration initially)
13. **Given** the existing Avalonia-based `Perch.Desktop` project **When** the project is replaced **Then** `Perch.Desktop` is a WPF project targeting `net10.0-windows` with WPF UI 4.2.0, HandyControl 3.5.1, CommunityToolkit.Mvvm 8.4.0, and Microsoft.Extensions.Hosting packages
14. `Perch.Desktop` references `Perch.Core` and uses `AddPerchCore()` for DI registration
15. `App.xaml.cs` sets up Generic Host with DI: WPF UI services, Core services via `AddPerchCore()`, all pages + ViewModels
16. `MainWindow.xaml` contains a WPF UI `NavigationView` sidebar with placeholder items (Home, Dotfiles, Apps, System Tweaks, Settings) and a content frame
17. Dark Fluent theme is applied with forest green (#10B981) accent color
18. `dotnet build` succeeds for the full solution including the new Desktop project with zero warnings

## Tasks / Subtasks

- [x] Task 1: Verify existing project structure (AC: #1-#12)
  - [x] Perch.Core, Perch.Cli, Perch.Core.Tests already exist and pass all original AC
  - [x] `dotnet build` zero warnings, `dotnet test` 371 tests pass
- [ ] Task 2: Remove Avalonia Desktop project (AC: #13)
  - [ ] Delete all `.axaml` files (Avalonia XAML format)
  - [ ] Delete `ViewLocator.cs` (Avalonia pattern, not needed for WPF UI)
  - [ ] Delete `Program.cs` (Avalonia entry point — WPF uses App.xaml)
  - [ ] Remove all Avalonia NuGet packages from csproj/Directory.Packages.props
  - [ ] Delete `bin/` and `obj/` to purge cached Avalonia assemblies
- [ ] Task 3: Create WPF Desktop project in place (AC: #13, #14)
  - [ ] Change SDK to `Microsoft.NET.Sdk` with `<UseWPF>true</UseWPF>`
  - [ ] Set `TargetFramework` to `net10.0-windows`
  - [ ] Add packages: WPF-UI (4.2.0), HandyControl (3.5.1), CommunityToolkit.Mvvm (8.4.0), Microsoft.Extensions.Hosting
  - [ ] Keep `ProjectReference` to `Perch.Core`
  - [ ] Keep `<OutputType>WinExe</OutputType>`
- [ ] Task 4: Set up App.xaml with Generic Host DI (AC: #15)
  - [ ] Create `App.xaml` with WPF UI Fluent theme resources
  - [ ] Create `App.xaml.cs` with Generic Host setup: `INavigationService`, `ISnackbarService`, `IContentDialogService`, `IThemeService`
  - [ ] Register Core services via `AddPerchCore()`
  - [ ] Register all pages and ViewModels
  - [ ] Apply dark Fluent theme with forest green (#10B981) accent (AC: #17)
- [ ] Task 5: Create MainWindow with NavigationView (AC: #16)
  - [ ] Create `MainWindow.xaml` with WPF UI `NavigationView` sidebar
  - [ ] Add placeholder items: Home, Dotfiles, Apps, System Tweaks, Settings
  - [ ] Add content frame for page navigation
  - [ ] Create `MainWindowViewModel.cs` (replace existing Avalonia version)
- [ ] Task 6: Convert existing ViewModels to WPF-compatible (AC: #13)
  - [ ] Keep `ViewModelBase.cs` pattern (CommunityToolkit.Mvvm `ObservableObject` is UI-agnostic)
  - [ ] Convert Wizard ViewModels: remove Avalonia-specific types, use WPF-compatible patterns
  - [ ] Convert Wizard Views: `.axaml` → `.xaml` with WPF UI controls
- [ ] Task 7: Startup routing (AC: #15)
  - [ ] Check for existing deploy state on startup
  - [ ] First run → wizard, returning user → dashboard (placeholder pages)
- [ ] Task 8: Build verification (AC: #18)
  - [ ] `dotnet build` succeeds for entire solution with zero warnings
  - [ ] `dotnet test` still passes (Desktop changes should not affect Core tests)

## Dev Notes

### Brownfield Assessment

**Core projects (Perch.Core, Perch.Cli, Perch.Core.Tests) are fully implemented** from a previous AI session. The actual new work is replacing the Avalonia-based Desktop project with WPF as specified in the architecture.

**Current state:**
- **Build:** `dotnet build` succeeds with zero warnings, zero errors
- **Tests:** 371 tests pass, 0 failures
- **Desktop:** Currently Avalonia — needs replacement with WPF

### Current Avalonia Desktop (TO BE REPLACED)

The existing `src/Perch.Desktop/` uses Avalonia UI:
- `.axaml` files (Avalonia XAML, not WPF `.xaml`)
- Packages: `Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter`
- `Program.cs` with Avalonia `AppBuilder` (WPF doesn't use this pattern)
- `ViewLocator.cs` (Avalonia convention, WPF UI uses `INavigableView<T>`)
- Wizard views: WelcomeStep, RepoSetupStep, SystemScanStep, DotfilesStep, AppCatalogStep, VsCodeExtensionsStep, WindowsTweaksStep, DeployStep, ReviewStep, DashboardStep
- ViewModels: Matching VMs for each view + `MainWindowViewModel`

### Target WPF Architecture (FROM ARCHITECTURE DOC)

**Packages:**
- WPF UI 4.2.0 (Fluent 2 design system for WPF)
- HandyControl 3.5.1 (StepBar component for wizard)
- CommunityToolkit.Mvvm 8.4.0 (keep — `[ObservableProperty]`, `[RelayCommand]`, `ObservableObject`)
- Microsoft.Extensions.Hosting (Generic Host for DI)
- Microsoft.Extensions.DependencyInjection (already present)

**Project Setup:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
  </PropertyGroup>
</Project>
```

**DI Setup (App.xaml.cs):**
- Generic Host with `IServiceProvider`
- WPF UI services: `INavigationService`, `ISnackbarService`, `IContentDialogService`, `IThemeService`
- Core services via `AddPerchCore()`
- All pages registered as Singleton (state preservation on nav)
- All ViewModels registered

**MainWindow.xaml:**
- WPF UI `NavigationView` sidebar
- Items: Home, Dotfiles, Apps, System Tweaks, Settings (footer)
- Content frame for page navigation
- Dark Fluent theme, forest green (#10B981) accent

**MVVM Pattern:**
- `ObservableObject` base class (CommunityToolkit.Mvvm — same as current)
- `[ObservableProperty]` for bindable properties
- `[RelayCommand]` for commands
- `INavigableView<T>` for page navigation (WPF UI pattern)

**Key Differences from Avalonia:**
| Avalonia | WPF |
|----------|-----|
| `.axaml` | `.xaml` |
| `Avalonia.Controls` namespace | `System.Windows.Controls` + `Wpf.Ui.Controls` |
| `AppBuilder` in `Program.cs` | `App.xaml.cs` with Generic Host |
| `ViewLocator` convention | `INavigableView<T>` + DI |
| `StyledProperty` / `DirectProperty` | `DependencyProperty` |
| `Avalonia.Themes.Fluent` | `Wpf.Ui` Fluent 2 theme |
| Cross-platform | Windows-only |

### Architecture Compliance

- **DI Pattern:** `AddPerchCore()` extension method on `IServiceCollection` in `ServiceCollectionExtensions.cs`
- **Rendering Boundary:** Spectre.Console types only in `Perch.Cli`, WPF/XAML only in `Perch.Desktop` — never in `Perch.Core`
- **Desktop DI:** Generic Host in `App.xaml.cs`, same `AddPerchCore()` registration as CLI
- **Desktop Modes:** Wizard (first-run, HandyControl StepBar) + Dashboard (ongoing, WPF UI NavigationView sidebar)
- **Shared Card Views:** `AppsView`, `DotfilesView` UserControls reusable in both wizard and dashboard
- **Platform Abstraction:** `ISymlinkProvider` with `WindowsSymlinkProvider` and `UnixSymlinkProvider`
- **Error Handling:** `DeployResult` record with `ResultLevel` enum (Info, Warning, Error)
- **Naming Conventions:** `I<Noun>Provider` for external resources, `I<Noun>Service` for orchestration

### CRITICAL: Use Context7 for Spectre.Console and WPF UI

Per CLAUDE.md: **Use Context7 MCP server to look up Spectre.Console and WPF UI API docs before writing code.** The WPF UI library has specific patterns for navigation, theming, and DI setup that differ from stock WPF.

### Testing Requirements

- TDD approach (write failing test first, then implement)
- 371 tests currently passing — Desktop changes must not break them
- Desktop-specific tests go in `tests/Perch.Desktop.Tests/` if needed (optional for this story)
- Test file naming mirrors source structure: `<ClassName>Tests.cs`

### Project Structure Notes

- No `.sln` file exists — projects discovered via `Directory.Build.props` implicit scanning
- `Directory.Packages.props` manages NuGet versions centrally — Avalonia packages must be removed from there too
- Desktop project is Windows-only (`net10.0-windows`) while Core/CLI remain cross-platform (`net10.0`)

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 1.1] — Original acceptance criteria
- [Source: _bmad-output/planning-artifacts/epics.md#Story 10.1] — Desktop project initialization spec (WPF UI, HandyControl, Generic Host DI)
- [Source: _bmad-output/planning-artifacts/architecture.md#Desktop UI] — WPF architecture: WPF UI 4.2.0, HandyControl 3.5.1, CommunityToolkit.Mvvm 8.4.0
- [Source: _bmad-output/planning-artifacts/architecture.md#Desktop MVVM] — ObservableObject, INavigableView<T>, page patterns
- [Source: _bmad-output/planning-artifacts/architecture.md#Desktop DI] — Generic Host in App.xaml.cs, AddPerchCore()
- [Source: _bmad-output/planning-artifacts/architecture.md#Desktop Modes] — Wizard (StepBar) + Dashboard (NavigationView)
- [Source: _bmad-output/planning-artifacts/ux-design-specification.md] — Forest green accent, dark Fluent theme, custom components

## Dev Agent Record

### Agent Model Used

<!-- To be filled by dev agent -->

### Debug Log References

### Completion Notes List

- Core project structure (AC #1-#12) already satisfied — focus is on Desktop replacement
- Avalonia → WPF migration is the primary deliverable
- Remove Avalonia packages from `Directory.Packages.props` (not just from csproj)
- Clean bin/obj after removing Avalonia to avoid stale assembly confusion
- WPF UI has specific Generic Host integration patterns — consult Context7 docs
- HandyControl StepBar is needed for wizard flow but placeholder pages are fine for this story
- Wizard views from Avalonia can inform WPF equivalents but cannot be directly converted (different XAML dialects)

### File List

- src/Perch.Desktop/Perch.Desktop.csproj (MODIFY: Avalonia → WPF)
- src/Perch.Desktop/App.axaml → App.xaml (REPLACE)
- src/Perch.Desktop/App.axaml.cs → App.xaml.cs (REPLACE)
- src/Perch.Desktop/Program.cs (DELETE: Avalonia entry point)
- src/Perch.Desktop/ViewLocator.cs (DELETE: Avalonia pattern)
- src/Perch.Desktop/Views/MainWindow.axaml → MainWindow.xaml (REPLACE)
- src/Perch.Desktop/Views/MainWindow.axaml.cs → MainWindow.xaml.cs (REPLACE)
- src/Perch.Desktop/ViewModels/MainWindowViewModel.cs (MODIFY)
- src/Perch.Desktop/Views/Wizard/*.axaml (REPLACE all with .xaml)
- src/Perch.Desktop/Views/Wizard/*.axaml.cs (REPLACE all with .xaml.cs)
- src/Perch.Desktop/ViewModels/Wizard/*.cs (MODIFY: remove Avalonia-specific types)
- src/Perch.Desktop/GlobalUsings.cs (MODIFY: Avalonia → WPF namespaces)
- Directory.Packages.props (MODIFY: remove Avalonia, add WPF UI + HandyControl)
