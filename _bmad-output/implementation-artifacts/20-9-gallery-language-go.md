# Story 20.9: Gallery Language — Go Ecosystem

Status: ready-for-dev

## Story

As a Perch Desktop user browsing the Languages page,
I want the Go ecosystem to have complete gallery entries covering the Go SDK, linters, and config files,
so that the Go ecosystem detail page shows my full Go toolchain.

## Design Source

`_bmad-output/design-thinking-2026-02-19.md` — Languages page, unified gallery architecture.

## Current Gallery State

In `../perch-gallery/catalog/apps/go/`:
- `go.yaml` — runtime with suggests for golangci-lint

## What Needs Verification / Enrichment

### Sub-categories Expected
1. **SDK / Runtime**: Go
2. **Linters & Formatters**: golangci-lint, gofumpt, staticcheck
3. **Tools**: delve (debugger), gopls (language server), air (live reload)
4. **Configuration Files**: go/env (GOPATH, GOBIN settings)

### Per-Entry Checklist
- [ ] `go.yaml`: verify kind: runtime, suggests covers ecosystem
- [ ] Linters: golangci-lint entry, add alternatives (staticcheck)
- [ ] Tools: create entries for delve, gopls
- [ ] Config files: go/env if applicable

## Acceptance Criteria

1. **Complete ecosystem.** Go runtime's `suggests` references all ecosystem tools.
2. **Sub-category coverage.** At least one entry per: SDK, Linters, Tools.
3. **Install IDs verified.**
4. **Gallery index regenerated.**

## Tasks / Subtasks

- [ ] Task 1: Audit existing Go gallery entries
- [ ] Task 2: Create missing entries (linters, tools)
- [ ] Task 3: Verify/update suggests relationships
- [ ] Task 4: Verify install IDs
- [ ] Task 5: Regenerate gallery index

## Constraints

- Changes are in `../perch-gallery/` repo.
- Follow existing YAML schema conventions.
- Go ecosystem is smaller than .NET/Node — don't pad with unnecessary entries.
