# Story 21.4: Gallery Dotfile — Node/npm Config

Status: ready-for-dev

## Story

As a Perch Desktop user,
I want the Node/npm config files (.npmrc, .nvmrc, bunfig.toml) to have gallery entries that correctly place them on the Languages page under the Node ecosystem,
so that language-specific dotfiles appear in their ecosystem context rather than the cross-cutting Dotfiles page.

## Design Source

`_bmad-output/design-thinking-2026-02-19.md` — "Language-owned dotfiles live on the Languages page, NOT on the Dotfiles page."

## Current Gallery State

`../perch-gallery/catalog/apps/node/nodejs.yaml` has:
- config.links: .npmrc with platform targets

No separate kind: dotfile entries exist for .nvmrc or bunfig.toml.

## What Needs Verification / Enrichment

### Expected Entries
1. **.npmrc**: Should be a kind: dotfile entry (or config.links on nodejs.yaml). Must appear in Node ecosystem detail under "Configuration Files" sub-category.
2. **.nvmrc**: Version pinning file. Kind: dotfile with config.links.
3. **bunfig.toml**: Bun config file. Kind: dotfile on bun.yaml or separate entry.

### Key Design Decision
Language-owned dotfiles must be **excluded from the Dotfiles page** (21-1 handles the filter). They appear on the Languages page under the owning ecosystem's "Configuration Files" sub-category (20-1 handles the display).

This story ensures the gallery entries exist and are correctly categorized so the filtering works.

### Per-Entry Checklist
- [ ] .npmrc: verify config.links targets (`%USERPROFILE%/.npmrc` / `$HOME/.npmrc`), ensure it's tied to Node ecosystem
- [ ] .nvmrc: create entry if missing, config.links to project root (or skip if it's project-local only)
- [ ] bunfig.toml: verify config.links on bun entry (`%USERPROFILE%/.bunfig.toml` / `$HOME/.bunfig.toml`)
- [ ] All entries tagged/categorized so they're identified as Node ecosystem dotfiles

## Acceptance Criteria

1. **.npmrc entry exists.** Has kind: dotfile (or is a config.link on nodejs.yaml), with correct platform targets.
2. **Language ecosystem ownership.** Entries are categorized or tagged so they appear on the Languages page under Node, not on the Dotfiles page.
3. **bunfig.toml entry exists.** Has config.links if bun has a global config file location.
4. **Gallery index regenerated.**

## Tasks / Subtasks

- [ ] Task 1: Verify .npmrc entry/config.links and platform targets
- [ ] Task 2: Create bunfig.toml entry if bun supports global config
- [ ] Task 3: Decide on .nvmrc — project-local files may not need gallery entries
- [ ] Task 4: Ensure category/tags identify these as Node ecosystem dotfiles
- [ ] Task 5: Regenerate gallery index

## Constraints

- Changes are in `../perch-gallery/` repo.
- These entries must NOT appear on the Dotfiles page — they are language-owned.
- Coordinate with 20-4 (Node ecosystem story) for suggests relationships.
