# Story 21.2: Gallery Dotfile — Git

Status: ready-for-dev

## Story

As a Perch Desktop user on the Dotfiles page,
I want the Git dotfile entry to have complete gallery metadata including config links, tweaks, and detail page content,
so that Git appears as a rich, actionable card with accurate symlink detection and configurable tweaks.

## Design Source

`_bmad-output/design-thinking-2026-02-19.md` — Dotfiles page prototype, gear icon for dotfiles with tweaks.

## Current Gallery State

`../perch-gallery/catalog/apps/git.yaml`:
- kind: dotfile
- category: Development/Version Control
- config.links: .gitconfig, .gitignore_global (with platform targets)
- tweaks: git-bash-here (context menu)
- install: winget + choco IDs
- suggests: references to Git GUI tools

## What Needs Verification / Enrichment

### Expected Detail Page Content
Git is one of the few dotfiles with a gear icon (has tweaks and a rich detail page):
1. **Config links**: .gitconfig, .gitignore_global — verify platform paths
2. **Tweaks**: "Git Bash Here" context menu, "Add Open with Git GUI" context menu, "Enable git-lfs"
3. **Alternatives**: none (Git is the only VCS Perch supports)
4. **Suggests**: Git GUIs (Fork, GitKraken, SourceTree, GitHub Desktop, TortoiseGit, gitui, lazygit) — these should appear as "Also Consider" on the detail page
5. **Extensions**: git-lfs, git-credential-manager

### Per-Entry Checklist
- [ ] Verify config.links paths resolve correctly on Windows (%USERPROFILE%)
- [ ] Verify tweaks have complete registry entries
- [ ] Add git-lfs and git-credential-manager as extensions or suggests
- [ ] Verify suggests list covers all Git GUI apps in the gallery
- [ ] Ensure kind: dotfile so it appears on Dotfiles page (not just Apps page)

## Acceptance Criteria

1. **Config links verified.** .gitconfig and .gitignore_global have correct platform-specific target paths that resolve on Windows.
2. **Tweaks complete.** At least "Git Bash Here" tweak has full registry key/name/value/type definition. Additional tweaks (Git GUI context menu, git-lfs enable) added if applicable.
3. **Suggests populated.** Git GUI tools in the gallery are referenced in suggests.
4. **Detail page worthy.** Entry has enough content (tweaks + suggests + config.links) that the gear icon on the Dotfiles page card is justified.
5. **Gallery index regenerated.**

## Tasks / Subtasks

- [ ] Task 1: Verify config.links target paths
- [ ] Task 2: Verify/add tweaks with complete registry definitions
- [ ] Task 3: Verify/update suggests list for Git GUI tools
- [ ] Task 4: Add git-lfs, git-credential-manager references
- [ ] Task 5: Regenerate gallery index

## Constraints

- Changes are in `../perch-gallery/` repo.
- Git is a cross-cutting dotfile — it stays on the Dotfiles page, NOT on Languages page.
