# Story 21.3: Gallery Dotfile — PowerShell

Status: ready-for-dev

## Story

As a Perch Desktop user on the Dotfiles page,
I want the PowerShell dotfile entry to have complete gallery metadata including profile config links, tweaks, and module suggestions,
so that PowerShell appears as a rich card with accurate profile symlink detection.

## Design Source

`_bmad-output/design-thinking-2026-02-19.md` — Dotfiles page prototype, cross-cutting dotfiles.

## Current Gallery State

`../perch-gallery/catalog/apps/powershell.yaml`:
- kind: dotfile
- category: Development/Shells
- config.links: profile.ps1 targeting PowerShell profile path
- tweaks: enable-module-logging, enable-script-logging
- install: winget + choco IDs for PowerShell 7

## What Needs Verification / Enrichment

### Expected Detail Page Content
PowerShell has a gear icon (has tweaks):
1. **Config links**: Microsoft.PowerShell_profile.ps1 — verify platform paths (Windows: `%USERPROFILE%\Documents\PowerShell\`, Linux/macOS: `$HOME/.config/powershell/`)
2. **Tweaks**: Module logging, script block logging, execution policy
3. **Suggests**: Oh My Posh, Starship (prompt themes), PSReadLine config
4. **Extensions / Modules**: Posh-Git, PSFzf, Terminal-Icons (common PowerShell modules)

### Per-Entry Checklist
- [ ] Verify config.links paths — Windows Documents path varies (OneDrive may redirect)
- [ ] Verify tweaks have complete registry entries
- [ ] Add suggests for Oh My Posh, Starship (if gallery entries exist)
- [ ] Consider adding a second config.link for PSReadLine settings (if applicable)
- [ ] Ensure kind: dotfile for Dotfiles page placement

## Acceptance Criteria

1. **Config links verified.** PowerShell profile has correct platform-specific target paths.
2. **Tweaks complete.** Module logging and script logging tweaks have full registry definitions.
3. **Suggests populated.** References to prompt theming tools (Oh My Posh, Starship) and related shell tools.
4. **Gallery index regenerated.**

## Tasks / Subtasks

- [ ] Task 1: Verify config.links target paths across platforms
- [ ] Task 2: Verify/add tweaks with complete registry definitions
- [ ] Task 3: Verify/update suggests for shell ecosystem tools
- [ ] Task 4: Regenerate gallery index

## Constraints

- Changes are in `../perch-gallery/` repo.
- PowerShell is a cross-cutting dotfile — stays on Dotfiles page.
