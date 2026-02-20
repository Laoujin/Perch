# Story 20.2: Language SDK Detection

Status: ready-for-dev

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

- [ ] Task 1: Runtime PATH detection (AC: #1, #2, #3)
  - [ ] Add `IRuntimeDetectionService` interface with `DetectRuntimeAsync(CatalogEntry)` returning `RuntimeDetectionResult(IsInstalled, Version)`
  - [ ] Implement `RuntimeDetectionService` — for each runtime, define the CLI command and version parse regex
  - [ ] Detection map: dotnet → `dotnet --version`, node → `node --version`, python → `python --version` or `python3 --version`, go → `go version`, ruby → `ruby --version`, rustup → `rustup --version`, java → `java --version`
  - [ ] Wire into `GalleryDetectionService.DetectLanguageRuntimesAsync()` as fallback after winget/choco check
  - [ ] Store version on `AppCardModel.DetectedVersion`

- [ ] Task 2: Global tool detection (AC: #4)
  - [ ] For dotnet: parse `dotnet tool list -g` output → match tool IDs against gallery entries with `install.dotnet-tool`
  - [ ] For node: parse `npm list -g --json` output → match package names against gallery entries with `install.node-package`
  - [ ] Mark matched entries as `CardStatus.Detected`
  - [ ] Other ecosystems (pip, gem, cargo) deferred — add when gallery entries exist

- [ ] Task 3: Async with timeouts (AC: #5)
  - [ ] Use `IProcessRunner.RunAsync()` with 2-second timeout per command
  - [ ] Catch `TimeoutException` and `Win32Exception` (command not found) — return not-detected
  - [ ] Log detection failures at Debug level, don't surface to user

- [ ] Task 4: Tests (AC: #6)
  - [ ] Test runtime detection with mocked process output (version string parsing)
  - [ ] Test fallback: winget-detected runtime skips PATH detection
  - [ ] Test timeout handling returns not-detected
  - [ ] Test global tool matching against gallery entries
  - [ ] Build passes with zero warnings

## Files to Modify

| File | Change |
|------|--------|
| `src/Perch.Desktop/Services/IRuntimeDetectionService.cs` | New interface |
| `src/Perch.Desktop/Services/RuntimeDetectionService.cs` | New implementation |
| `src/Perch.Desktop/Services/GalleryDetectionService.cs` | Wire runtime detection as fallback in `DetectLanguageRuntimesAsync` |
| `src/Perch.Desktop/Models/AppCardModel.cs` | Add `DetectedVersion` property |
| `src/Perch.Desktop/App.xaml.cs` | Register `IRuntimeDetectionService` in DI |
| `tests/Perch.Desktop.Tests/RuntimeDetectionServiceTests.cs` | New test file |

## Dependencies

- **20-1 (Languages page scaffold)** — page must exist for detection results to display. Can be developed in parallel if detection service is independent.
- Gallery content stories (20-3 through 20-9) define which runtimes have gallery entries to detect against.

## Constraints

- **No new NuGet packages.** Use existing `IProcessRunner` infrastructure.
- **Windows-only for now.** PATH detection commands are the same cross-platform, but test only on Windows. Linux/macOS paths deferred.
- **Detection is best-effort.** Missing runtimes are not errors. A runtime installed in a non-standard location may not be detected — that's acceptable.
