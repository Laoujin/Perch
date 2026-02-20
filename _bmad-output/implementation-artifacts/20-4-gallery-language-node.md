# Story 20.4: Gallery Language — Node Ecosystem

Status: ready-for-dev

## Story

As a Perch Desktop user browsing the Languages page,
I want the Node/JavaScript ecosystem to have complete gallery entries covering runtimes, version managers, package managers, global packages, and config files,
so that the Node ecosystem detail page shows my full JS toolchain with correct detection and alternatives.

## Design Source

`_bmad-output/design-thinking-2026-02-19.md` — unified gallery architecture, alternative switching (npm -> bun).

## Current Gallery State

In `../perch-gallery/catalog/apps/node/`:
- `nodejs.yaml` — runtime with suggests for yarn, pnpm, fnm, volta, nvm, deno, bun
- `bun.yaml`, `deno.yaml`, `yarn.yaml`, `pnpm.yaml`, `fnm.yaml`, `nvm.yaml`, `volta.yaml`

## What Needs Verification / Enrichment

### Sub-categories Expected
1. **Runtimes**: Node.js, Deno, Bun
2. **Version Managers**: fnm, nvm, volta
3. **Package Managers**: npm (bundled), yarn, pnpm
4. **Global Packages**: common CLI tools installed via npm/bun (typescript, tsx, eslint, prettier, etc.)
5. **Configuration Files**: .npmrc, .nvmrc, bunfig.toml

### Per-Entry Checklist
- [ ] `nodejs.yaml`: verify kind: runtime, suggests covers full ecosystem
- [ ] Alternatives: Node.js ↔ Deno ↔ Bun as runtime alternatives; npm ↔ yarn ↔ pnpm as package manager alternatives
- [ ] Version managers: fnm ↔ nvm ↔ volta as alternatives
- [ ] Global packages: create entries for common global npm tools with install.node-package
- [ ] Config files: .npmrc (exists on nodejs), .nvmrc, bunfig.toml need kind: dotfile entries with config.links

## Acceptance Criteria

1. **Complete ecosystem.** Node runtime's `suggests` list references all ecosystem tools.
2. **Sub-category coverage.** At least one entry per: Runtimes, Version Managers, Package Managers, Global Packages, Configuration Files.
3. **Alternatives set.** Runtime alternatives (Node/Deno/Bun), package manager alternatives (npm/yarn/pnpm), version manager alternatives (fnm/nvm/volta) are correctly defined.
4. **Config file entries.** .npmrc and bunfig.toml have kind: dotfile entries with config.links and platform targets.
5. **Install IDs verified.** All entries have correct winget/choco/node-package IDs.
6. **Gallery index regenerated.**

## Tasks / Subtasks

- [ ] Task 1: Audit existing Node gallery entries against sub-category checklist
- [ ] Task 2: Create missing entries (global packages, config files) with complete metadata
- [ ] Task 3: Verify/update suggests, requires, alternatives relationships
- [ ] Task 4: Add config.links to config file entries (.npmrc, .nvmrc, bunfig.toml)
- [ ] Task 5: Verify install IDs
- [ ] Task 6: Regenerate gallery index

## Constraints

- Changes are in `../perch-gallery/` repo.
- Follow existing YAML schema conventions.
