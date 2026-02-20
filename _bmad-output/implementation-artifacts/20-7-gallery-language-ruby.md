# Story 20.7: Gallery Language — Ruby Ecosystem

Status: ready-for-dev

## Story

As a Perch Desktop user browsing the Languages page,
I want the Ruby ecosystem to have complete gallery entries covering runtimes, version managers, gems, and config files,
so that the Ruby ecosystem detail page shows my full Ruby toolchain.

## Design Source

`_bmad-output/design-thinking-2026-02-19.md` — Languages page, unified gallery architecture.

## Current Gallery State

In `../perch-gallery/catalog/apps/ruby/`:
- `ruby.yaml` — runtime with suggests for bundler, rubocop, rails

## What Needs Verification / Enrichment

### Sub-categories Expected
1. **Runtimes**: Ruby (CRuby/MRI), JRuby
2. **Version Managers**: rbenv, mise (polyglot), RubyInstaller (Windows)
3. **Package Managers**: Bundler (bundled with Ruby)
4. **Linters & Formatters**: RuboCop, Standard, Solargraph
5. **Frameworks**: Rails, Sinatra
6. **Configuration Files**: .gemrc, .irbrc, .rubocop.yml

### Per-Entry Checklist
- [ ] `ruby.yaml`: verify kind: runtime, suggests covers ecosystem
- [ ] Version managers: rbenv ↔ mise as alternatives
- [ ] Linters: RuboCop ↔ Standard as alternatives
- [ ] Config files: .gemrc with kind: dotfile, config.links (`%USERPROFILE%\.gemrc` / `$HOME/.gemrc`)

## Acceptance Criteria

1. **Complete ecosystem.** Ruby runtime's `suggests` references all ecosystem tools.
2. **Sub-category coverage.** At least one entry per: Runtimes, Version Managers, Linters, Configuration Files.
3. **Alternatives set where applicable.**
4. **Config file entries.** .gemrc has kind: dotfile entry with config.links.
5. **Install IDs verified.**
6. **Gallery index regenerated.**

## Tasks / Subtasks

- [ ] Task 1: Audit existing Ruby gallery entries
- [ ] Task 2: Create missing entries (version managers, linters, config files)
- [ ] Task 3: Verify/update suggests, requires, alternatives
- [ ] Task 4: Add config.links for Ruby config files
- [ ] Task 5: Verify install IDs
- [ ] Task 6: Regenerate gallery index

## Constraints

- Changes are in `../perch-gallery/` repo.
- Follow existing YAML schema conventions.
