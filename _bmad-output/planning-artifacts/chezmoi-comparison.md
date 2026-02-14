# Chezmoi vs Perch — Detailed Comparison

**Date:** 2026-02-14
**Purpose:** Understand chezmoi's strengths for potential inspiration in later Perch scopes

## Core Philosophy Difference

| | chezmoi | Perch |
|---|---|---|
| **Model** | Copy-on-apply. Source repo is the truth, files are generated into place | Symlink-first. Repo files ARE the live files |
| **After editing a config** | `chezmoi re-add` or `chezmoi merge` to capture change | `git commit && git push` — no extra step |
| **Trade-off** | Enables encryption, templates, private files — but adds friction | Zero friction workflow — but no encryption/templates (yet) |

chezmoi's rationale: "Instead of using a symlink to redirect from the dotfile's location to the centralized directory, chezmoi generates the dotfile as a regular file in its final location from the contents of the centralized directory." This enables features impossible with symlinks.

## Feature Comparison

### Scope 1 (Switch Machines)

| Feature | chezmoi | Perch |
|---|---|---|
| Symlink files into place | Optional mode (limited) | Core approach |
| Deploy from git repo | Yes | Yes |
| Re-runnable | Yes (`chezmoi apply`) | Yes (`perch deploy`) |
| Convention-over-config | No — file-level management | Yes — folder name = package name |
| App-aware manifests | No | Yes |
| Dynamic config paths (globs) | No | Yes (for apps like VS with hash paths) |
| Distribution | Single Go binary | `dotnet tool install perch -g` |

### Scope 2 (Rock Solid)

| Feature | chezmoi | Perch (planned) |
|---|---|---|
| Drift detection | `chezmoi diff` | Idempotent deploy with drift report |
| Dry run | `chezmoi apply --dry-run` | `--WhatIf` mode |
| Run scripts | `run_once_`, `run_onchange_` scripts | Lifecycle hooks per plugin |
| Git clean filters | Not needed (copy model) | Per-app `.gitattributes` + filters |
| App discovery | None — manual only | Before/after diffing, installed app detection |
| Testing | N/A (Go project) | NUnit + NSubstitute, GitHub Actions CI |
| Backup before apply | No built-in | Pre-deploy backup snapshots |

### Scope 3+ (Future Inspiration from chezmoi)

| Feature | chezmoi | Perch (future consideration) |
|---|---|---|
| Templates | Yes — powerful | Not yet. See details below |
| Encryption | Yes — age, GPG, git-crypt | Not yet. See details below |
| 1Password integration | Yes — native | Not yet. See details below |
| Machine-specific config | Templates + conditionals | Layered override system (planned) |
| Windows registry | No | Planned — dedicated brainstorm needed |
| MAUI UI | No | Planned |
| AI-assisted discovery | No | Planned |

## Chezmoi's Templating System — Inspiration for Perch Scope 3

**How it works:**
- Uses Go's `text/template` syntax extended with Sprig functions
- Files with `.tmpl` suffix are treated as templates
- Docs: https://www.chezmoi.io/user-guide/templating/

**Built-in variables available in templates:**
- `.chezmoi.os` — operating system
- `.chezmoi.hostname` — machine hostname
- `.chezmoi.username` — current user
- Custom data from `.chezmoidata` files (JSON, TOML, YAML)

**Example — machine-specific config:**
```
# shared config here

{{- if eq .chezmoi.hostname "work-laptop" }}
# work-specific settings
{{- end }}

{{- if eq .chezmoi.hostname "home-desktop" }}
# home-specific settings
{{- end }}
```

**Example — OS-specific:**
```
{{ if eq .chezmoi.os "darwin" }}
# macOS settings
{{ else if eq .chezmoi.os "windows" }}
# Windows settings
{{ end }}
```

**Relevance to Perch:**
Perch's scope 3 "machine-specific overrides" could take inspiration from this approach but adapt it to the symlink model. Instead of templates that generate files, Perch could use a layering system: base config + machine-specific overlay, merged at deploy time. The result is still a symlink, but the source file is computed from layers.

## Chezmoi's 1Password Integration — Inspiration for Perch

**How it works:**
- Uses the 1Password CLI (`op`) under the hood
- Exposes template functions to fetch secrets at apply time
- Docs: https://www.chezmoi.io/user-guide/password-managers/1password/
- Function reference: https://www.chezmoi.io/reference/templates/1password-functions/

**Template functions available:**
- `onepasswordRead` — read a secret by URI: `{{ onepasswordRead "op://Personal/api-token/password" }}`
- `onepassword` — get full item as structured data
- `onepasswordItemFields` — get item fields as a map
- `onepasswordDetailsFields` — get detail fields as a map

**Example — injecting a secret into a config file:**
```
export CF_API_TOKEN='{{ onepasswordRead "op://Personal/cloudflare-api-token/password" }}'
```

**Session management:**
chezmoi verifies session token validity and prompts for re-auth if expired.

**Relevance to Perch:**
Since Perch uses symlinks, it can't inject secrets into config files the same way (the repo file IS the live file, so secrets would be in git). Possible approaches for Perch:
1. **Separate secret layer** — a non-git-tracked file that Perch merges with the symlinked config at deploy time
2. **1Password CLI integration** — Perch resolves secret placeholders during deploy, writing the result to a non-symlinked file (hybrid approach for files that contain secrets)
3. **Ignore the problem** — keep secrets out of managed configs entirely, handle them separately

This is a genuine gap in the symlink-first model that's worth solving in scope 3.

## Chezmoi's Symlink Mode Limitations

When chezmoi is configured for symlink mode, it CANNOT create symlinks for:
- **Encrypted files** — source contains ciphertext, not plaintext
- **Executable files** — executable bit can't be set on source files cross-platform
- **Private files** — git doesn't preserve group/world permission bits
- **Templated files** — source contains template, not rendered result

This means chezmoi's symlink mode is always a compromise. Perch's symlink-first approach avoids this by not having encryption/templates in the core model.

Reference: https://www.chezmoi.io/user-guide/frequently-asked-questions/design/

## Key Takeaways

1. **Perch's symlink-first model is a genuine differentiator** — chezmoi explicitly chose copies over symlinks for good reasons, but those reasons don't apply to Perch's use case (no encryption, no templates in scope 1-2)
2. **chezmoi's templating approach is worth studying** for Perch's machine-specific overrides — but Perch should adapt it to work with symlinks (layered configs) rather than copying chezmoi's template-and-generate model
3. **1Password integration is a scope 3 consideration** — the symlink model creates a real tension with secrets that needs a thoughtful solution
4. **Registry management is Perch's unique territory** — no existing dotfiles tool touches this
5. **App-level awareness is Perch's structural advantage** — chezmoi thinks in files, Perch thinks in applications

## Reference Links

- chezmoi homepage: https://www.chezmoi.io/
- chezmoi comparison table: https://www.chezmoi.io/comparison-table/
- chezmoi design FAQ: https://www.chezmoi.io/user-guide/frequently-asked-questions/design/
- chezmoi architecture: https://www.chezmoi.io/developer-guide/architecture/
- chezmoi templating: https://www.chezmoi.io/user-guide/templating/
- chezmoi 1Password guide: https://www.chezmoi.io/user-guide/password-managers/1password/
- chezmoi 1Password functions: https://www.chezmoi.io/reference/templates/1password-functions/
- chezmoi target types: https://www.chezmoi.io/reference/target-types/
- chezmoi symlink feature request: https://github.com/twpayne/chezmoi/issues/1005
- chezmoi GitHub: https://github.com/twpayne/chezmoi
- Protecting secrets with chezmoi: https://kidoni.dev/chezmoi-templates-and-secrets
