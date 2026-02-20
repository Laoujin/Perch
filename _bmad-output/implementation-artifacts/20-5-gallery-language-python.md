# Story 20.5: Gallery Language — Python Ecosystem

Status: ready-for-dev

## Story

As a Perch Desktop user browsing the Languages page,
I want the Python ecosystem to have complete gallery entries covering runtimes, version/environment managers, linters, formatters, and config files,
so that the Python ecosystem detail page shows my full Python toolchain.

## Design Source

`_bmad-output/design-thinking-2026-02-19.md` — Languages page, unified gallery architecture.

## Current Gallery State

In `../perch-gallery/catalog/apps/python/`:
- `python.yaml` — runtime with suggests for uv, poetry, ruff, mypy, pipx
- Additional entries for ecosystem tools

## What Needs Verification / Enrichment

### Sub-categories Expected
1. **Runtimes**: Python, PyPy
2. **Version/Environment Managers**: uv, pyenv, conda, pipx
3. **Package Managers**: pip (bundled), poetry, uv
4. **Linters & Formatters**: ruff, mypy, black, flake8, pylint
5. **IDEs & Editors**: PyCharm (if not on Apps page)
6. **Configuration Files**: pip.conf / pip.ini, pyproject.toml

### Per-Entry Checklist
- [ ] `python.yaml`: verify kind: runtime, suggests covers full ecosystem
- [ ] Alternatives: uv ↔ pyenv ↔ conda as version managers; poetry ↔ uv ↔ pip as package managers
- [ ] Linters: ruff ↔ flake8 ↔ pylint as alternatives
- [ ] Config files: pip.conf/pip.ini with kind: dotfile, config.links with platform targets (Windows: `%APPDATA%\pip\pip.ini`, Linux/macOS: `$HOME/.config/pip/pip.conf`)

## Acceptance Criteria

1. **Complete ecosystem.** Python runtime's `suggests` list references all ecosystem tools.
2. **Sub-category coverage.** At least one entry per: Runtimes, Version Managers, Linters, Configuration Files.
3. **Alternatives set.** Version managers, package managers, and linters have alternatives defined.
4. **Config file entries.** pip.conf/pip.ini has kind: dotfile entry with platform-specific config.links.
5. **Install IDs verified.** All entries have correct winget/choco IDs.
6. **Gallery index regenerated.**

## Tasks / Subtasks

- [ ] Task 1: Audit existing Python gallery entries
- [ ] Task 2: Create missing entries (linters, formatters, config files)
- [ ] Task 3: Verify/update suggests, requires, alternatives
- [ ] Task 4: Add config.links for Python config files
- [ ] Task 5: Verify install IDs
- [ ] Task 6: Regenerate gallery index

## Constraints

- Changes are in `../perch-gallery/` repo.
- Follow existing YAML schema conventions.
