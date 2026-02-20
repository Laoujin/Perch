# Story 20.2: Language SDK Detection

Status: review

## Story

As a Perch Desktop user,
I want Perch to automatically detect which language SDKs and runtimes are installed on my system,
so that the Languages page shows accurate Detected/Synced/Drifted status for each ecosystem without manual configuration.

## Design Source

`_bmad-output/design-thinking-2026-02-19.md` — Priority 1 action items: "Wire detection engine for language ecosystems."

## Current State

`GalleryDetectionService` already handles detection for:
- **Apps**: checks winget/choco install status via `IAppScanService`
- **Dotfiles**: checks symlink status of config.links targets via `ISymlinkProvider`
- **Tweaks**: checks registry values

The Languages page uses the same detection pipeline — `DetectLanguageRuntimesAsync()` calls into `DetectAppStatus()` for each runtime and its ecosystem tools. Detection works for apps with `install.winget` or `install.choco` IDs because `AppScanService` finds them.

## What's Missing

The current detection only checks package manager presence (winget/choco). Language SDKs have additional detection vectors:
1. **PATH-based detection**: `dotnet --version`, `node --version`, `python --version`, `go version`, `ruby --version`, `rustup --version`, `java --version`
2. **Well-known paths**: `%ProgramFiles%\dotnet\`, `%APPDATA%\nvm\`, `%USERPROFILE%\.cargo\`, `%USERPROFILE%\.rustup\`
3. **Registry keys**: Java JDK/JRE registry entries, .NET SDK registry entries
4. **Global tool detection**: `dotnet tool list -g`, `npm list -g`, `pip list`, `gem list`, `cargo install --list`

## Acceptance Criteria

1. **Runtime detection via PATH.** For each language runtime in the gallery (kind: runtime), check if the CLI binary is accessible via PATH. If found, mark as Detected even if not installed via winget/choco.
2. **Fallback detection.** If winget/choco detection finds the runtime, PATH detection is skipped (already detected). PATH detection is a fallback for runtimes installed via manual download, version managers, or platform installers.
3. **Version capture.** When detected via PATH, capture the version string (e.g., "10.0.100" for .NET, "22.14.0" for Node). Store on the `AppCardModel` for display.
4. **Global tool detection.** For runtimes with global tool ecosystems (dotnet-tool, node-package), detect installed global tools and map them to gallery entries. Mark matched gallery entries as Detected.
5. **Detection is async and non-blocking.** All CLI invocations use `ProcessRunner` with timeouts (2s per command). A failing/hanging command does not block the page.
6. **All existing tests pass.** New detection logic has unit tests with mocked process output.

## Tasks / Subtasks

- [x] Task 1: Runtime PATH detection (AC: #1, #2, #3)
  - [x] Add `IRuntimeDetectionService` interface with `DetectRuntimeAsync(CatalogEntry)` returning `RuntimeDetectionResult(IsInstalled, Version)`
  - [x] Implement `RuntimeDetectionService` — for each runtime, define the CLI command and version parse regex
  - [x] Detection map: dotnet → `dotnet --version`, node → `node --version`, python → `python --version` or `python3 --version`, go → `go version`, ruby → `ruby --version`, rustup → `rustup --version`, java → `java --version`
  - [x] Wire into `GalleryDetectionService.DetectAllAppsAsync()` as fallback after winget/choco check
  - [x] Store version on `AppCardModel.DetectedVersion`

- [x] Task 2: Global tool detection (AC: #4)
  - [x] For dotnet: parse `dotnet tool list -g` output → match tool IDs against gallery entries with `install.dotnet-tool`
  - [x] For node: parse `npm list -g --json` output → match package names against gallery entries with `install.node-package`
  - [x] Mark matched entries as `CardStatus.Detected`
  - [x] Other ecosystems (pip, gem, cargo) deferred — add when gallery entries exist

- [x] Task 3: Async with timeouts (AC: #5)
  - [x] Use `IProcessRunner.RunAsync()` with 2-second timeout per command
  - [x] Catch `TimeoutException` and `Win32Exception` (command not found) — return not-detected
  - [x] Log detection failures at Debug level, don't surface to user

- [x] Task 4: Tests (AC: #6)
  - [x] Test runtime detection with mocked process output (version string parsing)
  - [x] Test fallback: winget-detected runtime skips PATH detection
  - [x] Test timeout handling returns not-detected
  - [x] Test global tool matching against gallery entries
  - [x] Build passes with zero warnings

## Dev Agent Record

### Implementation Plan
- Created `IRuntimeDetectionService` with two methods: `DetectRuntimeAsync` (per-runtime CLI detection) and `DetectGlobalToolsAsync` (dotnet tools + npm packages)
- `RuntimeDetectionService` uses a static command map keyed by runtime ID/CLI name, with version-specific parsers (plain, strip-v prefix, prefixed word extraction, Go version format)
- Wired as fallback in `GalleryDetectionService.DetectAllAppsAsync()`: only invoked for Runtime entries not already detected by winget/choco
- Global tool detection runs for detected runtimes, matching output against catalog entries with `install.dotnet-tool` or `install.node-package`
- All process invocations use `CancellationTokenSource.CreateLinkedTokenSource` with 2s timeout
- Exceptions (Win32Exception, OperationCanceledException) caught and logged at Debug level

### Completion Notes
- 17 new tests in `RuntimeDetectionServiceTests.cs` covering all 7 runtime version parsers, error handling (command not found, timeout, non-zero exit), global tool detection (dotnet tools, npm packages), and edge cases (unknown runtime, non-runtime kind, invalid JSON)
- Existing 4 `GalleryDetectionService*Tests` files updated to pass new `IRuntimeDetectionService` mock to constructor
- Full test suite: 1292 tests passing (321 Desktop + 971 Core), zero failures
- Both `Perch.slnx` and `Perch.CrossPlatform.slnx` build with zero warnings

## File List

| File | Change |
|------|--------|
| `src/Perch.Desktop/Services/IRuntimeDetectionService.cs` | New — interface + result records |
| `src/Perch.Desktop/Services/RuntimeDetectionService.cs` | New — implementation with CLI command map and version parsers |
| `src/Perch.Desktop/Services/GalleryDetectionService.cs` | Modified — inject `IRuntimeDetectionService`, add runtime fallback in `DetectAllAppsAsync`, add `DetectGlobalToolsForRuntimes` |
| `src/Perch.Desktop/Models/AppCardModel.cs` | Modified — add `DetectedVersion` property |
| `src/Perch.Desktop/App.xaml.cs` | Modified — register `IRuntimeDetectionService` in DI |
| `tests/Perch.Desktop.Tests/RuntimeDetectionServiceTests.cs` | New — 17 tests |
| `tests/Perch.Desktop.Tests/GalleryDetectionServiceAppTests.cs` | Modified — add `IRuntimeDetectionService` mock to constructor |
| `tests/Perch.Desktop.Tests/GalleryDetectionServiceTweakTests.cs` | Modified — add `IRuntimeDetectionService` mock to constructor |
| `tests/Perch.Desktop.Tests/GalleryDetectionServiceFontTests.cs` | Modified — add `IRuntimeDetectionService` mock to constructor |
| `tests/Perch.Desktop.Tests/GalleryDetectionServiceDotfileTests.cs` | Modified — add `IRuntimeDetectionService` mock to constructor |

## Change Log

- 2026-02-20: Implemented runtime PATH detection, global tool detection, version capture, and async timeout handling with 17 new tests

## Dependencies

- **20-1 (Languages page scaffold)** — page must exist for detection results to display. Can be developed in parallel if detection service is independent.
- Gallery content stories (20-3 through 20-9) define which runtimes have gallery entries to detect against.

## Constraints

- **No new NuGet packages.** Use existing `IProcessRunner` infrastructure.
- **Windows-only for now.** PATH detection commands are the same cross-platform, but test only on Windows. Linux/macOS paths deferred.
- **Detection is best-effort.** Missing runtimes are not errors. A runtime installed in a non-standard location may not be detected — that's acceptable.
