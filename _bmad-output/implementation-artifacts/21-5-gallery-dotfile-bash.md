# Story 21.5: Gallery Dotfile — Bash

Status: ready-for-dev

## Story

As a Perch Desktop user on the Dotfiles page,
I want the Bash dotfile entry to have complete gallery metadata including config links for .bashrc and .wslconfig,
so that Bash appears as a card with accurate symlink detection on Linux/macOS and WSL config detection on Windows.

## Design Source

`_bmad-output/design-thinking-2026-02-19.md` — Dotfiles page prototype, cross-cutting dotfiles.

## Current Gallery State

`../perch-gallery/catalog/apps/bash.yaml`:
- kind: dotfile
- category: Development/Shells
- config.links: .bashrc (Linux/macOS), .wslconfig (Windows)

## What Needs Verification / Enrichment

### Expected Detail Page Content
Bash is a simpler dotfile — likely no gear icon (no tweaks):
1. **Config links**: .bashrc (Linux/macOS: `$HOME/.bashrc`), .bash_profile (macOS: `$HOME/.bash_profile`), .wslconfig (Windows: `%USERPROFILE%/.wslconfig`)
2. **Additional configs**: .bash_aliases, .inputrc (readline config)
3. **Suggests**: Starship, Oh My Bash, atuin (shell history)
4. **No tweaks**: Bash has no Windows registry tweaks

### Per-Entry Checklist
- [ ] Verify .bashrc config.links targets and platform restrictions (linux, macos only)
- [ ] Verify .wslconfig config.links targets (windows only)
- [ ] Consider adding .bash_profile for macOS
- [ ] Consider adding .inputrc as separate config link
- [ ] Add suggests for shell enhancement tools
- [ ] Ensure kind: dotfile for Dotfiles page placement

## Acceptance Criteria

1. **Config links verified.** .bashrc has correct platform-specific target paths with platform restrictions. .wslconfig targets Windows only.
2. **Platform filtering correct.** .bashrc links are linux/macos only. .wslconfig link is windows only.
3. **Suggests populated.** References to shell enhancement tools if they exist in the gallery.
4. **Gallery index regenerated.**

## Tasks / Subtasks

- [ ] Task 1: Verify config.links target paths and platform restrictions
- [ ] Task 2: Consider adding .bash_profile, .inputrc links
- [ ] Task 3: Add suggests for shell enhancement tools
- [ ] Task 4: Regenerate gallery index

## Constraints

- Changes are in `../perch-gallery/` repo.
- Bash is a cross-cutting dotfile — stays on Dotfiles page.
- Bash is primarily Linux/macOS — the Windows entry (.wslconfig) is WSL-specific.
