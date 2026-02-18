# Gallery Catalog YAML Schema

Reference schema for perch-gallery catalog entries. Derived from the [brainstorming session](../brainstorming/brainstorming-session-2026-02-18.md).

## Common Fields

These fields are shared across all entry types (app, font, tweak).

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `type` | enum | yes | `app`, `font`, `tweak` |
| `name` | string | yes | Full display name |
| `display-name` | string | no | Short/alternate name (e.g. "VS Code" for "Visual Studio Code") |
| `category` | string | yes | Hierarchical category (e.g. `Development/IDEs`). Source of truth for taxonomy. |
| `tags` | string[] | yes | Flat tags for filtering and search |
| `description` | string | yes | One-line description |
| `profiles` | string[] | no | Target user profiles. Values: `developer`, `power-user`, `casual`, `gamer`, `creative` |
| `os` | string[] | no | Platform scope. Values: `windows`, `linux`, `macos`. Omit = windows-only (most entries). |
| `hidden` | bool | no | Low-relevance entry, excluded from default views. Default: false. |
| `license` | string | no | SPDX identifier or short label (e.g. `MIT`, `proprietary`, `freemium`) |
| `links` | object | no | Reference URLs |

### links

| Field | Type | Description |
|-------|------|-------------|
| `website` | string | Official website |
| `docs` | string | Documentation URL |
| `github` | string | GitHub repository URL |

## App Schema

```yaml
type: app
name: Visual Studio Code
display-name: VS Code
kind: app                                    # app | cli-tool | runtime | dotfile
category: Development/IDEs
tags: [editor, ide, microsoft]
description: Lightweight but powerful source code editor
profiles: [developer]
os: [windows, linux, macos]
license: MIT
links:
  website: https://code.visualstudio.com
  docs: https://code.visualstudio.com/docs
  github: https://github.com/microsoft/vscode
install:
  winget: Microsoft.VisualStudio.Code
  choco: vscode
config:
  links:
    - source: settings.json
      target:
        windows: "%APPDATA%/Code/User/settings.json"
        linux: "$HOME/.config/Code/User/settings.json"
        macos: "$HOME/Library/Application Support/Code/User/settings.json"
    - source: keybindings.json
      target:
        windows: "%APPDATA%/Code/User/keybindings.json"
        linux: "$HOME/.config/Code/User/keybindings.json"
        macos: "$HOME/Library/Application Support/Code/User/keybindings.json"
extensions:
  bundled: [ms-dotnettools.csharp]
  recommended: [esbenp.prettier-vscode]
tweaks:
  - id: telemetry-off
    name: Disable Telemetry
    description: Disable VS Code telemetry reporting
    registry:
      - key: HKCU\Software\Policies\Microsoft\VisualStudioCode
        name: UpdateMode
        value: none
        type: string
alternatives: [sublimetext, notepadplusplus]
suggests: [git, windows-terminal]
```

### App-Specific Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `kind` | enum | no | `app` (default), `cli-tool`, `runtime`, `dotfile` |
| `install` | object | yes* | Installation sources. *Required unless kind is `dotfile`. |
| `config` | object | no | Symlink/dotfile configuration |
| `extensions` | object | no | IDE/editor extensions |
| `tweaks` | object[] | no | App-owned tweaks (same schema as standalone tweaks) |
| `alternatives` | string[] | no | IDs of alternative apps (bidirectional -- Core resolves both directions) |
| `suggests` | string[] | no | IDs of suggested companion apps (one-way) |
| `requires` | string[] | no | IDs of required dependencies (one-way) |

### install

| Field | Type | Description |
|-------|------|-------------|
| `winget` | string | Winget package ID |
| `choco` | string | Chocolatey package ID |
| `dotnet-tool` | string | .NET global tool ID (e.g. `dotnet-ef`) |
| `node-package` | string | Node global package name (e.g. `typescript`). Resolved by user's chosen package manager. |

### config

| Field | Type | Description |
|-------|------|-------------|
| `links` | object[] | Symlink definitions |
| `clean-filter` | object | Git clean/smudge filter for sensitive values |

### config.links[]

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `source` | string | yes | Relative path in dotfiles repo |
| `target` | map | yes | Platform-keyed target paths (`windows`, `linux`, `macos`) |
| `link-type` | enum | no | `symlink` (default), `junction`, `copy` |
| `platforms` | string[] | no | Restrict this link to specific platforms |
| `template` | bool | no | File contains `{{perch.xxx}}` placeholders. Default: false. |

### Relationship Rules

- **alternatives**: Mutual. Declared on one side; Core auto-mirrors. `[vscode] -> alternatives: [sublimetext]` means sublimetext also shows vscode as alternative.
- **suggests**: Soft, one-way. "You might also want..."
- **requires**: Hard, one-way. "Won't work without..."

### kind Values

| Value | Meaning | Example |
|-------|---------|---------|
| `app` | GUI application installed via package manager | Chrome, VS Code, Docker Desktop |
| `cli-tool` | Command-line tool installed via package manager | fzf, zoxide, ripgrep |
| `runtime` | Language runtime or SDK | .NET SDK, Python, Node, Bun |
| `dotfile` | Config-only entry, no installation | .gitconfig, PowerShell profile, .bashrc |

## Font Schema

```yaml
type: font
name: Cascadia Code Nerd Font
category: Fonts/Programming
tags: [monospace, nerd-font, ligatures, microsoft]
description: Microsoft's Cascadia Code with Nerd Font patches
license: OFL-1.1
preview-text: "let x => match { Ok(v) -> v }"
install:
  choco: cascadia-code-nerd-font
```

### Font-Specific Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `preview-text` | string | no | Sample text for font preview |
| `install` | object | yes | Same structure as app install |

## Tweak Schema

```yaml
type: tweak
name: Dark Mode
category: Appearance/Theme
tags: [theme, dark-mode, personalization]
description: Enable dark mode for apps and system UI
reversible: true
profiles: [developer, power-user]
windows-versions: [10, 11]
registry:
  - key: HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize
    name: AppsUseLightTheme
    value: 0
    type: dword
    default-value: 1
  - key: HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize
    name: SystemUsesLightTheme
    value: 0
    type: dword
    default-value: 1
suggests: [classic-context-menu]
```

### Tweak-Specific Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `reversible` | bool | no | Can be undone. Default: false. |
| `windows-versions` | int[] | no | Applicable Windows versions (e.g. `[10, 11]`) |
| `registry` | object[] | yes* | Registry modifications. *Required unless `script` is provided. |
| `script` | string | no | PowerShell script to apply the tweak |
| `undo-script` | string | no | PowerShell script to reverse the tweak |
| `alternatives` | string[] | no | Mutually exclusive tweaks |
| `suggests` | string[] | no | Related tweaks |
| `requires` | string[] | no | Prerequisite tweaks |

### registry[]

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `key` | string | yes | Full registry key path |
| `name` | string | yes | Value name |
| `value` | any | yes | Value to set |
| `type` | enum | yes | `dword`, `string`, `qword`, `expandstring`, `multistring`, `binary` |
| `default-value` | any | no | Original value for undo |

## App-Owned Tweaks

App-owned tweaks live in an app's `tweaks:` array and use the same schema as standalone tweaks, minus `type`, `category`, and `windows-versions` (inherited from the parent app).

```yaml
tweaks:
  - id: disable-telemetry          # unique within the app
    name: Disable Telemetry
    description: Stop telemetry reporting
    registry:
      - key: HKCU\Software\...
        name: TelemetryEnabled
        value: 0
        type: dword
        default-value: 1
```

Core aggregates app-owned tweaks into cross-cutting views (e.g. "All Context Menus", "All Default Apps") alongside standalone tweaks.

## Migration from Current Schema

| Change | Before | After |
|--------|--------|-------|
| `dotfiles: true` | Boolean flag | Remove. Use `kind: dotfile` instead. |
| `kind` values | Only `dotfile` | `app`, `cli-tool`, `runtime`, `dotfile` |
| `profiles` on apps | Not present | Add to apps and fonts |
| `os` | Not present | Add to all types |
| `hidden` | Not present | Add to all types |
| `license` | Not present | Add to all types |
| `alternatives` on apps | Not present | Add |
| `suggests` on apps | Not present | Add (already on tweaks) |
| `requires` on apps | Not present | Add (already on tweaks) |
| `install.dotnet-tool` | Not present | Add |
| `install.node-package` | Not present | Add |

## File Organization

- **ID = filename**: `vscode.yaml` -> id `vscode`
- **Icons by convention**: `vscode.yaml` + `vscode.png` (same directory)
- **Subdirectories for navigation**: tweaks already use subdirs, apps will when exceeding ~20 files per folder
- **Directory != category**: `category:` field is the source of truth, not the file path
- **`catalog/metadata/github-stars.yaml`**: Separate file keyed by `type/id`, updated by sync script

## Index Schema

Auto-generated `index.yaml` includes summary fields for quick loading:

```yaml
apps:
  - id: vscode
    name: Visual Studio Code
    category: Development/IDEs
    tags: [editor, ide, microsoft]
    kind: app
    profiles: [developer]
    os: [windows, linux, macos]
    hidden: false
fonts:
  - id: cascadia-code-nf
    name: Cascadia Code Nerd Font
    category: Fonts/Programming
    tags: [monospace, nerd-font, ligatures, microsoft]
tweaks:
  - id: dark-mode
    name: Dark Mode
    category: Appearance/Theme
    tags: [theme, dark-mode, personalization]
    profiles: [developer, power-user]
```
