# Story 20.8: Gallery Language — Rust Ecosystem

Status: ready-for-dev

## Story

As a Perch Desktop user browsing the Languages page,
I want the Rust ecosystem to have complete gallery entries covering rustup, cargo tools, and config files,
so that the Rust ecosystem detail page shows my full Rust toolchain.

## Design Source

`_bmad-output/design-thinking-2026-02-19.md` — Languages page, unified gallery architecture.

## Current Gallery State

In `../perch-gallery/catalog/apps/rust/`:
- `rustup.yaml` — runtime with suggests for cargo-edit, rust-analyzer

## What Needs Verification / Enrichment

### Sub-categories Expected
1. **Toolchain Manager**: rustup
2. **Cargo Tools**: cargo-edit, cargo-watch, cargo-expand, cargo-audit, cargo-deny, cargo-nextest
3. **Language Server**: rust-analyzer
4. **IDEs & Editors**: RustRover, VS Code (cross-ecosystem)
5. **Configuration Files**: cargo/config.toml, rustfmt.toml, clippy.toml

### Per-Entry Checklist
- [ ] `rustup.yaml`: verify kind: runtime, suggests covers ecosystem
- [ ] Cargo tools: create entries for common cargo extensions
- [ ] Config files: cargo/config.toml with kind: dotfile, config.links (`%USERPROFILE%\.cargo\config.toml` / `$HOME/.cargo/config.toml`)

## Acceptance Criteria

1. **Complete ecosystem.** Rustup's `suggests` references all ecosystem tools.
2. **Sub-category coverage.** At least one entry per: Toolchain, Cargo Tools, Language Server, Configuration Files.
3. **Config file entries.** cargo/config.toml has kind: dotfile entry with config.links.
4. **Install IDs verified.**
5. **Gallery index regenerated.**

## Tasks / Subtasks

- [ ] Task 1: Audit existing Rust gallery entries
- [ ] Task 2: Create missing entries (cargo tools, config files)
- [ ] Task 3: Verify/update suggests relationships
- [ ] Task 4: Add config.links for Rust config files
- [ ] Task 5: Verify install IDs
- [ ] Task 6: Regenerate gallery index

## Constraints

- Changes are in `../perch-gallery/` repo.
- Follow existing YAML schema conventions.
