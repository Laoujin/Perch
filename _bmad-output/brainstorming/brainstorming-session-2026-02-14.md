---
stepsCompleted: [1, 2, 3, 4]
inputDocuments: []
session_topic: 'Comprehensive inventory of non-application Windows system-level settings worth syncing across machines'
session_goals: 'Catalogue every category of system-level Windows configuration worth syncing, surface less obvious settings, flag applicability constraints (managed machines, Windows edition, form factor)'
selected_approach: 'ai-recommended'
techniques_used: ['Morphological Analysis', 'Cross-Pollination', 'Role Playing']
ideas_generated: 225
context_file: ''
session_active: false
workflow_completed: true
---

# Brainstorming Session Results

**Facilitator:** Wouter
**Date:** 2026-02-14

## Session Overview

**Topic:** Comprehensive inventory of non-application Windows system-level settings worth syncing across machines
**Goals:**
- Catalogue every category of system-level Windows configuration worth syncing
- Surface the less obvious stuff beyond the usual Explorer tweaks
- Flag applicability constraints (managed machines, Windows edition, desktop vs. laptop, etc.)

### Context Guidance

_Perch project - cross-platform dotfiles/settings manager (C#/.NET 10). Existing PowerShell implementation already syncs some Explorer registry tweaks (file extensions, hidden files, nav pane icons). PRD Scope 3 flags registry management as needing dedicated brainstorm. Application-level config syncing is handled separately by Perch's manifest system and is out of scope here._

### Session Setup

- **Scope:** Non-application Windows system-level settings and configurations
- **Out of scope:** Per-application config stored in registry (covered by app manifests), secrets/credentials (Scope 4), anything achievable through symlinking (hosts file, PS profiles)
- **Key constraint:** Applicability varies by machine context - managed/work machines may lock certain settings (e.g., Group Policy), Windows edition matters (Home vs Pro vs Enterprise), form factor matters (desktop vs laptop)
- **Approach:** Exhaustive inventory of "what" with applicability tagging
- **Two outputs identified:** (1) Windows system-level settings to sync, (2) Community catalogue entries for GitHub Pages gallery

## Technique Selection

**Approach:** AI-Recommended Techniques
**Analysis Context:** Windows system-level settings inventory with applicability constraints

**Recommended Techniques:**

- **Morphological Analysis:** Systematically decompose the domain into parameters (category, location, scope, applicability) and explore combinations to ensure comprehensive coverage
- **Cross-Pollination:** Steal from other OS ecosystems (macOS defaults, Linux dconf/gsettings, enterprise tools) to reveal Windows equivalents we'd otherwise miss
- **Role Playing:** Walk through concrete user personas (fresh install, work-to-home switch, power user, new machine) to catch scenario-specific settings

**AI Rationale:** This is fundamentally a domain enumeration task. Morphological Analysis provides systematic structure, Cross-Pollination brings external inspiration, and Role Playing catches real-world gaps through scenario-based thinking.

## Technique Execution Results

**Total ideas generated:** ~225 across 3 techniques
**Techniques completed:** Morphological Analysis (full), Cross-Pollination (full), Role Playing (full)
**Duration:** Extended session with deep domain exploration
**Key insight:** Two meta-patterns emerged: (1) setting bundles/profiles for common personas, (2) conditional/per-machine settings based on machine role

---

## Research Priority: Interfacing with Existing Structured Data

**Key decision:** Don't build the settings catalogue from scratch. Interface with existing structured repositories so that adding settings to the Perch gallery is a matter of importing/mapping, not manually authoring each one.

### Primary Sources to Interface With

| Source | Format | Why It Matters | Integration Strategy | Link |
|--------|--------|---------------|---------------------|------|
| **Windows ADMX files** | XML (.admx + .adml) | Microsoft's own authoritative mapping: policy name → registry path + value type + valid options + human-readable description. Ships with every Windows install at `C:\Windows\PolicyDefinitions\`. Covers the vast majority of our P1-P3 settings. | Parse ADMX/ADML to auto-generate Perch setting definitions. This is the "import 500 settings in one go" path. | [ADMX Schema](https://learn.microsoft.com/en-us/previous-versions/windows/desktop/policy/admx-schema), [Understanding ADMX](https://learn.microsoft.com/en-us/windows/client-management/understanding-admx-backed-policies) |
| **ChrisTitusTech/winutil** | JSON | Pre-structured tweaks with `Path`, `Name`, `Value`, `Type`, `OriginalValue`, `Description`. Organized by category. Closest to what Perch's internal format would look like. Already covers many P1/P2 settings. | Map WinUtil JSON → Perch manifest format. Could be a direct import pipeline or a one-time migration script. | [GitHub](https://github.com/ChrisTitusTech/winutil) |
| **Sophia Script for Windows** | PowerShell + JSON | 150+ unique reversible tweaks. Export/import file associations as JSON. Most battle-tested open-source tweaker. Strong on privacy/debloat edge cases we'll encounter. | Reference for edge-case handling and validation logic. May also serve as import source for privacy bundle. | [GitHub](https://github.com/farag2/Sophia-Script-for-Windows) |

### Secondary Sources (Cross-Reference)

| Source | What It Provides | Link |
|--------|-----------------|------|
| **Policy Plus** | Open-source .NET Group Policy editor. Navigate policies by ID, text, or affected registry entry. Useful for verifying ADMX mappings. | [GitHub](https://github.com/Fleex255/PolicyPlus) |
| **so-many-registry-tweaks-for-windows-2024** | Massive .reg collection for performance/hardening. Good for discovering settings not in ADMX. | [GitHub](https://github.com/RamRendezvous/so-many-registry-tweaks-for-windows-2024) |
| **zedseven/windows-registry-tweaks** | Clean enable/disable .reg pairs per tweak. Good format reference. | [GitHub](https://github.com/zedseven/windows-registry-tweaks) |

### Recommended Approach

1. **Define Perch's setting manifest format** - what fields a setting definition needs (registry path, value name, type, valid values, default, description, applicability tags, restart requirement, elevation needed)
2. **Build an ADMX parser** - auto-import settings from ADMX/ADML files into Perch manifests. This gives us hundreds of settings with descriptions for free.
3. **Build a WinUtil JSON importer** - map WinUtil's format to Perch manifests for the curated "power user essentials" subset.
4. **Curate, don't just import** - auto-imported settings populate the gallery draft; human review assigns priority tiers and bundles before publishing.
5. **Link all sources in the registry module README** when implementing.

---

## Output 1: Windows System-Level Settings Inventory

### Legend

- **Applicability tags:** `[all]` all machines, `[pro+]` Pro/Enterprise only, `[laptop]` laptop-specific, `[desktop]` desktop-specific, `[personal]` personal machines only (GPO-locked on managed), `[dual-boot]` dual-boot machines, `[dev]` developer machines
- **Already implemented:** marked with ✅ (existing PowerShell implementation)

---

### Theme 1: Shell & Explorer UI

The most visible, universally-opinionated settings. High-value, easy to implement (mostly HKCU registry).

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 1 | Show file extensions | `HKCU\..\Explorer\Advanced\HideFileExt` | `[all]` ✅ |
| 2 | Show hidden files | `HKCU\..\Explorer\Advanced\Hidden` | `[all]` ✅ |
| 3 | Show protected OS files | `HKCU\..\Explorer\Advanced\ShowSuperHidden` | `[all]` ✅ |
| 4 | Full path in title bar | `HKCU\..\Explorer\CabinetState\FullPath` | `[all]` ✅ |
| 5 | Aero Shake to minimize | `HKCU\..\Explorer\Advanced\DisallowShaking` | `[all]` ✅ |
| 6 | Nav pane icons (Network, OneDrive, Libraries) | Various CLSIDs | `[all]` ✅ |
| 7 | Taskbar alignment (center vs left, Win11) | `HKCU\..\Explorer\Advanced\TaskbarAl` | `[all]` |
| 8 | Taskbar auto-hide | `HKCU\..\Explorer\StuckRects3` | `[all]` |
| 9 | Taskbar combine buttons | `HKCU\..\Explorer\Advanced\TaskbarGlomLevel` | `[all]` |
| 10 | Taskbar badge counts | `HKCU\..\Explorer\Advanced\TaskbarBadges` | `[all]` |
| 11 | Search box style (hidden/icon/bar) | `HKCU\..\Search\SearchboxTaskbarMode` | `[all]` |
| 12 | Task View button show/hide | `HKCU\..\Explorer\Advanced\ShowTaskViewButton` | `[all]` |
| 13 | Widgets button show/hide (Win11) | `HKCU\..\Explorer\Advanced\TaskbarDa` | `[all]` |
| 14 | Chat/Teams icon show/hide (Win11) | `HKCU\..\Explorer\Advanced\TaskbarMn` | `[all]` |
| 15 | Copilot button show/hide (Win11 23H2+) | `HKCU\..\Explorer\Advanced\ShowCopilotButton` | `[all]` |
| 16 | Desktop icons (This PC, Recycle Bin, Network, User, Control Panel) | `HKCU\..\HideDesktopIcons\NewStartPanel` | `[all]` |
| 17 | Explorer opens to Quick Access vs This PC | `HKCU\..\Explorer\Advanced\LaunchTo` | `[all]` |
| 18 | Dark mode (apps + system) | `HKCU\..\Themes\Personalize\AppsUseLightTheme` + `SystemUsesLightTheme` | `[all]` |
| 19 | Accent color on title bars & borders | `HKCU\Software\Microsoft\Windows\DWM\ColorPrevalence` | `[all]` |
| 20 | Compact mode vs touch spacing (Win11) | `HKCU\..\Explorer\Advanced\UseCompactMode` | `[all]` |
| 21 | Start menu "More pins" vs "More recommendations" (Win11) | `HKCU\..\Explorer\Advanced\Start_Layout` | `[all]` |
| 22 | Snap Assist config (layouts, groups, suggestions) | `HKCU\..\Explorer\Advanced` (multiple keys) | `[all]` |
| 23 | Alt-Tab exclude Edge tabs | `HKCU\..\Explorer\Advanced\MultiTaskingAltTabFilter` | `[all]` |
| 24 | Virtual desktop taskbar scope (all desktops vs current) | `HKCU\..\Explorer\Advanced\VirtualDesktopTaskbarFilter` | `[all]` |
| 25 | Multi-monitor taskbar behavior | `HKCU\..\Explorer\Advanced\MMTaskbarMode` | `[all]` |
| 26 | Explorer nav pane expand to current folder | `HKCU\..\Explorer\Advanced\NavPaneExpandToCurrentFolder` | `[all]` |
| 27 | Explorer status bar visibility | `HKCU\Software\Microsoft\Internet Explorer\Main\StatusBarOther` | `[all]` |
| 28 | Explorer menu bar always visible | `HKCU\..\Explorer\Advanced\AlwaysShowMenus` | `[all]` |
| 29 | Checkboxes for item selection | `HKCU\..\Explorer\Advanced\AutoCheckSelect` | `[all]` |
| 30 | Explorer details/preview pane defaults | Explorer layout settings | `[all]` |
| 31 | Show seconds in system clock (Win11) | `HKCU\..\Explorer\Advanced\ShowSecondsInSystemClock` | `[all]` |
| 32 | End Task from taskbar right-click (Win11) | `HKCU\..\Explorer\Advanced\TaskbarEndTask` | `[all]` |
| 33 | Folder view template defaults (force General Items) | `HKCU\Software\Classes\Local Settings\...\Shell\Bags` | `[all]` |
| 34 | Folder type auto-detection override | `HKCU\Software\Classes\Local Settings\...\AllFolders` | `[all]` |
| 35 | Show drive letters before/after name | `HKLM\..\Explorer\ShowDriveLettersFirst` | `[all]` |
| 36 | Quick Access pinned folders | ShellBag registry state | `[all]` |
| 37 | Jump list recent items count & tracking | `HKCU\..\Explorer\Advanced\Start_TrackDocs` | `[all]` |
| 38 | System tray icon overflow behavior | `HKCU\..\Explorer\NotifyIconSettings` | `[all]` |
| 39 | Additional clocks (extra time zones in tray) | `HKCU\Control Panel\TimeZoneInformation` | `[all]` |
| 40 | Search: disable web results / highlights | `HKCU\..\SearchSettings\IsDynamicSearchBoxEnabled` | `[all]` |
| 41 | Windows Spotlight vs custom lock screen | `HKCU\..\ContentDeliveryManager\RotatingLockScreenEnabled` | `[all]` |

### Theme 2: Input, Mouse & Keyboard

Personal feel preferences. Mostly HKCU, universally desired.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 42 | Keyboard repeat rate & delay | `HKCU\Control Panel\Keyboard` | `[all]` |
| 43 | Mouse speed & sensitivity | `HKCU\Control Panel\Mouse\MouseSensitivity` | `[all]` |
| 44 | Disable mouse acceleration ("Enhance pointer precision") | `HKCU\Control Panel\Mouse\MouseSpeed` = 0 | `[all]` |
| 45 | Touchpad gestures & sensitivity | Registry + Settings app | `[laptop]` |
| 46 | Scroll lines per notch | `HKCU\Control Panel\Desktop\WheelScrollLines` | `[all]` |
| 47 | Scroll direction (natural scrolling) | `HKLM\..\HID\...\FlipFlopWheel` | `[all]` |
| 48 | Double-click speed | `HKCU\Control Panel\Mouse\DoubleClickSpeed` | `[all]` |
| 49 | Drag-and-drop pixel threshold | `HKCU\Control Panel\Desktop\DragHeight` / `DragWidth` | `[all]` |
| 50 | Caps Lock remap (e.g., to Ctrl/Escape) | `HKLM\..\Keyboard Layout\Scancode Map` (binary) | `[dev]` |
| 51 | Compose key / dead keys config | `HKCU\Keyboard Layout\Scancode Map` | `[all]` |
| 52 | NumLock on boot | `HKCU\Control Panel\Keyboard\InitialKeyboardIndicators` | `[all]` |

### Theme 3: Privacy & Telemetry

High-value for privacy-conscious users. Mix of HKCU and HKLM/Policy.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 53 | Telemetry level (diagnostic data) | `HKLM\..\Policies\Microsoft\Windows\DataCollection\AllowTelemetry` | `[pro+]` `[personal]` |
| 54 | Advertising ID disable | `HKCU\..\AdvertisingInfo\Enabled` | `[all]` |
| 55 | Activity history (local) | `HKLM\..\Policies\Microsoft\Windows\System` | `[personal]` |
| 56 | Activity history (cloud sync) | `PublishUserActivities` + `UploadUserActivities` | `[personal]` |
| 57 | Location services toggle | System-level capability access | `[all]` |
| 58 | Camera & microphone default access | `HKCU\..\CapabilityAccessManager` | `[all]` |
| 59 | Inking & typing personalization | `HKCU\Software\Microsoft\InputPersonalization` | `[all]` |
| 60 | Online speech recognition | `HKCU\..\Speech_OneCore\...\OnlineSpeechPrivacy` | `[all]` |
| 61 | Suggested content / ads in Start, lock screen, Settings | `HKCU\..\ContentDeliveryManager` (~15 sub-keys) | `[all]` |
| 62 | Feedback frequency | `HKCU\Software\Microsoft\Siuf\Rules\NumberOfSIUFInPeriod` | `[all]` |
| 63 | Wi-Fi connection data sharing | `HKLM\..\WcmSvc\wifinetworkmanager\features` | `[all]` |
| 64 | Diagnostic data viewer enable | `HKLM\..\DiagTrack\EventTranscriptKey\EnableEventTranscript` | `[all]` |
| 65 | Windows Recall disable (Win11 24H2+) | `HKCU\..\Policies\Microsoft\Windows\WindowsAI\DisableAIDataAnalysis` | `[all]` |
| 66 | Error reporting (WER) disable | `HKLM\..\Windows Error Reporting\Disabled` | `[all]` |

### Theme 4: Developer Environment

Critical for dev machines. Mix of HKCU, HKLM, features, and environment variables.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 67 | Developer Mode toggle | `HKLM\..\AppModelUnlock` | `[dev]` |
| 68 | Long path support (>260 chars) | `HKLM\..\FileSystem\LongPathsEnabled` | `[dev]` |
| 69 | PATH entries (user + system) and order | `HKCU\Environment\Path` + `HKLM\..\Environment\Path` | `[dev]` |
| 70 | Custom environment variables | `HKCU\Environment` + `HKLM\..\Environment` | `[dev]` |
| 71 | PowerShell execution policy (system-level) | `HKLM\..\PowerShell\1\...\ExecutionPolicy` | `[dev]` |
| 72 | Windows Terminal as default terminal | `HKCU\Console\%%Startup\DelegationTerminal` | `[dev]` |
| 73 | Console code page (UTF-8) | `HKLM\..\Nls\CodePage\OEMCP` | `[dev]` |
| 74 | Beta: UTF-8 worldwide language support | `HKLM\..\Nls\CodePage` ACP=65001 | `[dev]` |
| 75 | Console host defaults (font, buffer, quick edit) | `HKCU\Console` | `[dev]` |
| 76 | App execution aliases (python → Store fix) | `HKCU\..\App Paths` + execution alias settings | `[dev]` |
| 77 | Symbolic link evaluation policy | `HKLM\..\FileSystem\SymlinkLocalToLocalEvaluation` | `[dev]` |
| 78 | Windows Defender exclusions (dev folders) | `HKLM\..\Windows Defender\Exclusions` | `[dev]` |
| 79 | WSL default distro & version default | `wsl --set-default` / registry state | `[dev]` |
| 80 | WSL PATH interop toggle | `HKCU\..\Lxss` settings | `[dev]` |
| 81 | Sysinternals EULA pre-acceptance (all tools) | `HKCU\Software\Sysinternals\*\EulaAccepted` | `[dev]` |
| 82 | JIT debugger registration (32/64-bit) | `HKLM\..\AeDebug\Debugger` | `[dev]` |
| 83 | Symbol server / debug symbol paths | `_NT_SYMBOL_PATH` env var | `[dev]` |
| 84 | ODBC data sources (System DSN) | `HKLM\SOFTWARE\ODBC\ODBC.INI` | `[dev]` |
| 85 | NuGet package sources (machine-level) | `%ProgramData%\NuGet\NuGet.Config` | `[dev]` |
| 86 | Case-sensitive directories (per-folder flag) | `fsutil file setCaseSensitiveInfo` | `[dev]` |
| 87 | BIOS time UTC (dual-boot clock fix) | `HKLM\..\TimeZoneInformation\RealTimeIsUniversal` | `[dual-boot]` `[dev]` |

### Theme 5: Windows Optional Features

Declarative desired-state for enabled/disabled features. Managed via `dism` / `Enable-WindowsOptionalFeature`.

| # | Setting | Feature Name | Applicability |
|---|---------|-------------|---------------|
| 88 | WSL (Windows Subsystem for Linux) | `Microsoft-Windows-Subsystem-Linux` | `[dev]` |
| 89 | Hyper-V | `Microsoft-Hyper-V-All` | `[pro+]` `[dev]` |
| 90 | Windows Sandbox | `Containers-DisposableClientVM` | `[pro+]` `[dev]` |
| 91 | SSH Client | `OpenSSH.Client` | `[dev]` |
| 92 | SSH Server | `OpenSSH.Server` | `[dev]` |
| 93 | .NET Framework versions | Various `.NET-Framework-*` | `[dev]` |
| 94 | Telnet Client | `TelnetClient` | `[dev]` |
| 95 | Virtual Machine Platform | `VirtualMachinePlatform` | `[dev]` |
| 96 | WSL installed distro list (desired state) | `wsl --install -d <distro>` | `[dev]` |

### Theme 6: Power & Performance

Mix of power plans, service tuning, and system performance. Form-factor dependent.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 97 | Active power plan + custom plan export | `powercfg /export` | `[all]` |
| 98 | Lid close action | Power plan settings | `[laptop]` |
| 99 | Sleep / display off timers (AC/battery) | Power plan settings | `[all]` |
| 100 | Fast startup (hybrid shutdown) disable | `HKLM\..\Power\HiberbootEnabled` | `[all]` |
| 101 | Hibernate enable/disable | `powercfg /hibernate` | `[all]` |
| 102 | USB selective suspend disable | `HKLM\..\Services\USB\DisableSelectiveSuspend` | `[all]` |
| 103 | Visual effects ("Adjust for best performance" / custom) | `HKCU\..\Explorer\VisualEffects` | `[all]` |
| 104 | Game Mode / Game Bar / Game DVR disable | `HKCU\..\GameBar` + `GameConfigStore` | `[all]` |
| 105 | Superfetch/SysMain disable | `HKLM\..\Services\SysMain\Start` | `[all]` |
| 106 | Memory compression enable/disable | `HKLM\..\Memory Management` | `[all]` |
| 107 | Core parking aggressiveness | `HKLM\..\Power\PowerSettings\...\CoreParkingMinCores` | `[all]` |
| 108 | Paging executive lock in memory | `HKLM\..\Memory Management\DisablePagingExecutive` | `[all]` |
| 109 | Network throttling index | `HKLM\..\Multimedia\SystemProfile\NetworkThrottlingIndex` | `[all]` |
| 110 | Page file configuration (size, drive) | `HKLM\..\Memory Management\PagingFiles` | `[all]` |

### Theme 7: Security & Hardening

Important for personal machines. Often GPO-locked on managed machines.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 111 | UAC level | `HKLM\..\Policies\System\ConsentPromptBehaviorAdmin` | `[personal]` |
| 112 | SmartScreen settings (apps, Edge, Store) | `HKLM\..\Policies\Microsoft\Windows\System\EnableSmartScreen` | `[personal]` |
| 113 | Remote Desktop enable/disable | `HKLM\..\Terminal Server\fDenyTSConnections` | `[pro+]` |
| 114 | Custom firewall rules export | `netsh advfirewall export` | `[all]` |
| 115 | SMBv1 disable | `HKLM\..\LanmanServer\Parameters\SMB1` | `[all]` |
| 116 | Lock screen timeout & notification visibility | `HKCU\..\PushNotifications` + lock policies | `[all]` |
| 117 | Screen saver timeout (lock workstation timer) | `HKCU\Control Panel\Desktop\ScreenSaveTimeOut` | `[all]` |
| 118 | Windows Hello PIN complexity | `HKLM\..\Policies\Microsoft\PassportForWork\PINComplexity` | `[personal]` |
| 119 | Removable storage access policy | `HKLM\..\Policies\Microsoft\Windows\RemovableStorageDevices` | `[personal]` |
| 120 | DEP policy | `bcdedit /set nx` | `[all]` |
| 121 | Credential Guard / Device Guard | `HKLM\..\DeviceGuard` | `[pro+]` `[personal]` |
| 122 | Spectre/Meltdown mitigation toggles | `HKLM\..\Memory Management\FeatureSettingsOverride` | `[all]` |

### Theme 8: Networking

Mix of connectivity preferences and security posture.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 123 | System proxy settings | `HKCU\..\Internet Settings` | `[all]` |
| 124 | DNS preferences (DoH, custom servers) | Registry + `netsh` | `[all]` |
| 125 | Network profile default (Public vs Private) | `HKLM\..\Policies\Microsoft\Windows NT\...\NetworkList` | `[all]` |
| 126 | Network discovery & file sharing per-profile | Registry + firewall rules | `[all]` |
| 127 | Wi-Fi Sense / Hotspot 2.0 disable | `HKLM\..\WcmSvc\wifinetworkmanager` | `[all]` |
| 128 | Delivery optimization (P2P updates) | `HKLM\..\Policies\Microsoft\Windows\DeliveryOptimization` | `[all]` |
| 129 | Bluetooth Quick Pair disable | `HKLM\..\Bluetooth\QuickPair` | `[all]` |
| 130 | Network adapter power management ("allow turn off to save power") | `HKLM\..\Control\Class\{adapter GUID}` | `[laptop]` |
| 131 | Persistent mapped network drives | `HKCU\Network\<DriveLetter>` | `[all]` |
| 132 | Nearby Sharing / Shared Experiences | `HKCU\..\CDP` | `[all]` |

### Theme 9: Regional, Locale & Language

Critical for non-US/non-English users. Often overlooked.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 133 | System locale & non-Unicode language | `HKLM\..\Nls\Language` | `[all]` |
| 134 | Date/time/number format overrides (ISO 8601, etc.) | `HKCU\Control Panel\International` | `[all]` |
| 135 | Keyboard layout & input methods installed | `HKCU\Keyboard Layout\Preload` | `[all]` |
| 136 | Input method switching hotkey (Alt+Shift vs Win+Space) | `HKCU\Keyboard Layout\Toggle` | `[all]` |
| 137 | Time zone: auto vs fixed + default | `HKLM\..\Services\tzautoupdate` | `[all]` |
| 138 | First day of week | `HKCU\Control Panel\International\iFirstDayOfWeek` | `[all]` |
| 139 | Display language override | `HKCU\Control Panel\Desktop\PreferredUILanguages` | `[all]` |

### Theme 10: File Associations & Protocol Handlers

Large surface area. High value for dev workflows.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 140 | Default browser | `HKCU\..\UserChoice` per HTTP/HTTPS/HTM/HTML | `[all]` |
| 141 | Default mail client | `HKCU\..\UserChoice` for mailto: | `[all]` |
| 142 | Default PDF viewer | `HKCU\..\UserChoice` for .pdf | `[all]` |
| 143 | Default image viewer | `HKCU\..\UserChoice` for .jpg/.png/etc. | `[all]` |
| 144 | Default media player | `HKCU\..\UserChoice` for media types | `[all]` |
| 145 | Full "Open With" map (extension → handler) | `HKCU\..\Explorer\FileExts` | `[all]` |
| 146 | File type verb registrations (Edit, Print, Preview) | `HKCR\<extension>\shell` | `[all]` |
| 147 | Custom protocol handlers (vscode://, steam://, etc.) | `HKCR\<protocol>` | `[all]` |
| 148 | MIME type → handler mapping | `HKCR\MIME\Database\Content Type` | `[all]` |

### Theme 11: Context Menus & Shell Extensions

Right-click UX. Mix of HKCR, HKCU, and shell extension management.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 149 | Win11 "Show more options" override (classic context menu) | `HKCU\Software\Classes\CLSID\{...}\InprocServer32` | `[all]` |
| 150 | "Open in Terminal" target (which terminal) | `HKCU\..\shell\...\command` | `[dev]` |
| 151 | Right-click "New" menu items (.md, .py, .txt, etc.) | `HKCR\.<ext>\ShellNew` | `[dev]` |
| 152 | Shell extension block list (disable slow extensions) | `HKLM\..\Shell Extensions\Blocked` | `[all]` |
| 153 | "Send To" registry-based COM entries | Shell:sendto + HKCR registrations | `[all]` |

### Theme 12: Fonts

Installation, registration, and rendering preferences.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 154 | Installed font set (file + registry registration) | `HKLM\..\Fonts` + font files | `[all]` |
| 155 | Per-user font installations (Win10 1809+) | `%LOCALAPPDATA%\..\Fonts` + per-user registry | `[all]` |
| 156 | Font substitution table | `HKLM\..\FontSubstitutes` | `[all]` |
| 157 | Default system font override (replace Segoe UI) | `HKCU\..\Fonts` | `[all]` |
| 158 | ClearType enable/disable | `HKCU\Control Panel\Desktop\FontSmoothing` | `[all]` |

### Theme 13: Scheduled Tasks & Automation

Portable task definitions. High value for power users.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 159 | Custom scheduled task definitions (XML export) | `schtasks /query /xml` | `[all]` |
| 160 | Logon-triggered tasks | Scheduled Task with logon trigger | `[all]` |
| 161 | Event-triggered tasks (VPN connect, lid open, USB insert) | Scheduled Task with event trigger | `[all]` |
| 162 | Idle-triggered maintenance tasks | Scheduled Task with idle trigger | `[all]` |
| 163 | Defrag schedule override (disable on SSD) | Built-in scheduled task modification | `[all]` |
| 164 | Automatic maintenance schedule | `HKLM\..\Schedule\Maintenance` | `[all]` |

### Theme 14: Windows Update & Maintenance

Control update behavior. Edition-dependent for some features.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 165 | Active hours | `HKLM\..\WindowsUpdate\UX\Settings` | `[all]` |
| 166 | Feature update deferral (days) | `HKLM\..\Policies\Microsoft\Windows\WindowsUpdate` | `[pro+]` |
| 167 | Quality update deferral (days) | `HKLM\..\Policies\Microsoft\Windows\WindowsUpdate` | `[pro+]` |
| 168 | Delivery optimization mode (P2P scope) | `HKLM\..\DeliveryOptimization\DODownloadMode` | `[all]` |
| 169 | Delivery optimization cache size | `HKLM\..\DeliveryOptimization\DOMaxCacheSize` | `[all]` |
| 170 | Storage Sense config (auto-cleanup rules) | `HKCU\..\StorageSense` | `[all]` |
| 171 | Disk Cleanup presets (which categories) | `HKLM\..\VolumeCaches\<Category>\StateFlags` | `[all]` |
| 172 | Microsoft Store auto-update | `HKLM\..\Policies\Microsoft\WindowsStore\AutoDownload` | `[all]` |

### Theme 15: Notifications & Focus

Attention management and interruption control.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 173 | Focus Assist / DND auto-rules | `HKCU\..\quiethourssettings` | `[all]` |
| 174 | Focus Assist during screen share | Focus Assist auto-rule | `[all]` |
| 175 | Notification sender allow/block per app | Per-app notification registry | `[all]` |
| 176 | Toast notification duration | `HKCU\Control Panel\Accessibility\MessageDuration` | `[all]` |
| 177 | Presentation mode settings | `HKCU\..\MobilePC\AdaptableSettings` | `[laptop]` |

### Theme 16: Storage & File System Behavior

File system tuning and storage management.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 178 | Default save locations (Documents, Downloads on D:) | `HKCU\..\User Shell Folders` | `[all]` |
| 179 | Recycle Bin size & per-drive settings | `HKCU\..\Explorer\BitBucket` | `[all]` |
| 180 | Windows Search index locations & exclusions | `HKLM\..\Windows Search` | `[all]` |
| 181 | Windows Search service disable (for Everything users) | `HKLM\..\Services\WSearch\Start` | `[all]` |
| 182 | Thumbnail cache disable (esp. network drives) | `HKCU\..\Explorer\Advanced\DisableThumbnailCache` | `[all]` |
| 183 | System Protection (restore points: drives, max size) | `HKLM\..\SystemRestore` | `[all]` |
| 184 | NTFS last access time updates disable | `HKLM\..\FileSystem\NtfsDisableLastAccessUpdate` | `[all]` |
| 185 | 8.3 short filename creation disable | `HKLM\..\FileSystem\NtfsDisable8dot3NameCreation` | `[all]` |
| 186 | Low disk space warning threshold | `HKCU\..\Policies\Explorer\NoLowDiskSpaceChecks` | `[all]` |

### Theme 17: Accessibility

Input accessibility and readability preferences.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 187 | Sticky Keys / Filter Keys / Toggle Keys disable | `HKCU\Control Panel\Accessibility\StickyKeys\Flags` etc. | `[all]` |
| 188 | Cursor size & scheme | `HKCU\Control Panel\Cursors` | `[all]` |
| 189 | Narrator auto-start & voice settings | Narrator registry settings | `[all]` |
| 190 | Menu show delay (ms) | `HKCU\Control Panel\Desktop\MenuShowDelay` | `[all]` |
| 191 | Tooltip popup delay | `HKCU\..\Explorer\Advanced\ExtendedUIHoverTime` | `[all]` |
| 192 | Window animation speed | `HKCU\Control Panel\Desktop\WindowMetrics\MinAnimate` | `[all]` |

### Theme 18: Startup & Boot

Boot-time configuration and startup management.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 193 | Startup app enable/disable state | `HKCU\..\StartupApproved\Run` | `[all]` |
| 194 | Boot timeout & default OS (dual-boot) | `bcdedit /timeout` | `[dual-boot]` |
| 195 | Verbose startup/shutdown messages | `HKLM\..\Policies\System\VerboseStatus` | `[dev]` |
| 196 | Crash dump type & auto-restart | `HKLM\..\CrashControl` | `[dev]` |

### Theme 19: Printing & Peripherals

Device behavior preferences.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 197 | Default printer: disable auto-switch to last used | `HKCU\..\Windows\LegacyDefaultPrinterMode` | `[all]` |
| 198 | Print Spooler service disable | `HKLM\..\Services\Spooler\Start` | `[all]` |
| 199 | Print to PDF default settings (paper size) | Print driver registry config | `[all]` |
| 200 | AutoPlay settings per media type | `HKCU\..\Explorer\AutoplayHandlers` | `[all]` |
| 201 | Bluetooth behavior (discovery, A2DP) | Bluetooth service registry | `[all]` |
| 202 | Windows Ink workspace toggle | `HKCU\..\PenWorkspace` | `[all]` |

### Theme 20: Services Configuration

Startup type overrides for built-in services.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 203 | Print Spooler (disable if no printer) | `HKLM\..\Services\Spooler\Start` | `[all]` |
| 204 | Bluetooth service (disable if no BT) | `HKLM\..\Services\bthserv\Start` | `[desktop]` |
| 205 | Xbox services (disable on non-gaming) | `HKLM\..\Services\Xbl*\Start` | `[all]` |
| 206 | Windows Search (disable for alt. search) | `HKLM\..\Services\WSearch\Start` | `[all]` |
| 207 | Superfetch/SysMain | `HKLM\..\Services\SysMain\Start` | `[all]` |
| 208 | WinRM / Remote Management | `HKLM\..\Services\WinRM\Start` | `[dev]` |
| 209 | SSH Server (OpenSSH sshd) | `HKLM\..\Services\sshd\Start` | `[dev]` |

### Theme 21: Windows Sandbox Policies

Global defaults for sandbox behavior.

| # | Setting | Registry / Mechanism | Applicability |
|---|---------|---------------------|---------------|
| 210 | Networking default | `HKLM\..\Policies\Microsoft\Windows\Sandbox` | `[pro+]` `[dev]` |
| 211 | vGPU default | Sandbox policy registry | `[pro+]` `[dev]` |
| 212 | Clipboard sharing default | Sandbox policy registry | `[pro+]` `[dev]` |
| 213 | Audio/video input defaults | Sandbox policy registry | `[pro+]` `[dev]` |

---

## Meta-Patterns Discovered

### Pattern A: Setting Bundles / Profiles

Several settings naturally cluster into personas that should be deployable as a unit:

| Bundle | Settings Included | Use Case |
|--------|-------------------|----------|
| **Privacy Hardened** | #53-66 (all telemetry/privacy toggles off) | Privacy-conscious users |
| **Developer Machine** | #67-87, #88-96 (dev mode, long paths, WSL, features, Defender exclusions) | First dev machine setup |
| **Dual-Boot Sanity** | #87, #100, #194 (UTC clock, fast startup off, boot timeout) | Linux + Windows machines |
| **Presentation Mode** | #173-174, #177 (focus assist, screen share rules, presentation settings) | Frequent presenters |
| **Gaming Disable** | #104 (game mode/bar/DVR off) | Non-gaming dev machines |
| **Performance Tuned** | #103, #105-110 (visual effects, superfetch, memory, core parking) | Max performance machines |

### Pattern B: Conditional / Per-Machine Settings

Some settings need different values depending on machine role:

| Condition | Example Settings That Vary |
|-----------|---------------------------|
| **Desktop vs Laptop** | Hibernate (#101), lid close (#98), touchpad (#45), battery timers (#99), Bluetooth (#204) |
| **Work/Managed vs Personal** | Group Policy locked: UAC (#111), telemetry (#53), activity history (#55-56), SmartScreen (#112) |
| **Pro/Enterprise vs Home** | Update deferral (#166-167), Hyper-V (#89), Sandbox (#90), RDP (#113), Credential Guard (#121) |
| **Gaming vs Dev** | Game Mode (#104), power plan (#97), performance tuning (#103-110) |
| **Dual-Boot vs Single-OS** | UTC clock (#87), fast startup (#100), boot config (#194) |

---

## Output 2: Community Catalogue Candidates

Settings/configs identified during brainstorming that belong in the GitHub Pages app gallery (symlinkable, per-application) rather than system-level registry sync:

| # | Item | Type | Notes |
|---|------|------|-------|
| C1 | Windows Terminal settings + profiles | Symlinkable JSON | Already covered by core Perch |
| C2 | PowerShell profiles | Symlinkable | Already core Perch feature |
| C3 | Hosts file customizations | Symlinkable | Ad-blocking, dev redirects |
| C4 | .wslconfig | Symlinkable | WSL2 resource limits |
| C5 | Win+X power user menu shortcuts | Symlinkable folder | Custom power user menu items |
| C6 | Windows Sandbox .wsb templates | Symlinkable files | Library of sandbox configs |
| C7 | OpenSSH sshd_config | Symlinkable | SSH server configuration |
| C8 | NuGet.Config (machine-level) | Symlinkable | Package feed configuration |
| C9 | Custom .reg file library | Importable | Bundled registry tweaks as .reg files |
| C10 | Scheduled task XML templates | Importable | Pre-built automation tasks |

---

## Prioritization Framework

**Proposed tiers for implementation ordering:**

- **P1 - Core:** Most users want this, easy to implement (simple registry read/write), already proven in PowerShell implementation
- **P2 - Power User:** Strong value for target audience, moderate complexity, HKCU-based
- **P3 - Advanced:** High value for specific personas, may require HKLM/admin, feature toggles, or special handling
- **P4 - Niche:** Specific use cases, complex implementation, or machine-specific concerns
- **P5 - Maybe Never:** Very niche, risky, or better handled by other tools

---

## Proposed Tier Assignments

### Tier Rationale

- **P1:** Simple HKCU DWORD read/write, universally wanted by target audience, proven pattern from existing PowerShell implementation. These are the "everyone does this on every machine" settings.
- **P2:** Strong value for power users / devs, moderate complexity (may need HKLM/admin, multiple keys, or command-based mechanism), but straightforward to implement.
- **P3:** High value for specific personas, requires special handling (feature toggles via DISM, binary formats, complex state, export/import commands, or depends on apps being installed).
- **P4:** Niche use cases, hardware/machine-dependent, complex implementation, or deep system tuning that most users won't touch.
- **P5:** Risky to automate, extremely niche, or better handled by specialized tools.

---

### P1 - Core (Implement First)

Simple registry values, universally wanted. The "no-brainer" set.

| # | Setting | Theme |
|---|---------|-------|
| 1 | Show file extensions ✅ | Shell |
| 2 | Show hidden files ✅ | Shell |
| 3 | Show protected OS files ✅ | Shell |
| 4 | Full path in title bar ✅ | Shell |
| 5 | Aero Shake disable ✅ | Shell |
| 6 | Nav pane icons ✅ | Shell |
| 7 | Taskbar alignment (Win11) | Shell |
| 9 | Taskbar combine buttons | Shell |
| 10 | Taskbar badge counts | Shell |
| 11 | Search box style | Shell |
| 12 | Task View button show/hide | Shell |
| 13 | Widgets button show/hide | Shell |
| 14 | Chat/Teams icon show/hide | Shell |
| 15 | Copilot button show/hide | Shell |
| 16 | Desktop icons (This PC, Recycle Bin, etc.) | Shell |
| 17 | Explorer opens to Quick Access vs This PC | Shell |
| 18 | Dark mode (apps + system) | Shell |
| 19 | Accent color on title bars | Shell |
| 20 | Compact mode vs touch spacing | Shell |
| 21 | Start menu layout (pins vs recommendations) | Shell |
| 23 | Alt-Tab exclude Edge tabs | Shell |
| 24 | Virtual desktop taskbar scope | Shell |
| 26 | Nav pane expand to current folder | Shell |
| 28 | Menu bar always visible | Shell |
| 29 | Checkboxes for item selection | Shell |
| 31 | Show seconds in system clock | Shell |
| 32 | End Task from taskbar | Shell |
| 40 | Disable web search / highlights in Start | Shell |
| 42 | Keyboard repeat rate & delay | Input |
| 43 | Mouse speed & sensitivity | Input |
| 44 | Disable mouse acceleration | Input |
| 46 | Scroll lines per notch | Input |
| 52 | NumLock on boot | Input |
| 54 | Advertising ID disable | Privacy |
| 59 | Inking & typing personalization disable | Privacy |
| 60 | Online speech recognition disable | Privacy |
| 61 | Suggested content / ads disable (~15 keys) | Privacy |
| 62 | Feedback frequency = Never | Privacy |
| 65 | Windows Recall disable | Privacy |
| 68 | Long path support enable | Dev |
| 72 | Windows Terminal as default terminal | Dev |
| 100 | Fast startup disable | Power |
| 104 | Game Mode / Game Bar / Game DVR disable | Power |
| 134 | Date/time/number format overrides (ISO 8601) | Locale |
| 138 | First day of week | Locale |
| 149 | Classic context menu (Win11) | Context |
| 187 | Sticky Keys / Filter Keys / Toggle Keys disable | Accessibility |

**Total P1: 47 settings**

---

### P2 - Power User (Implement Second)

Strong value, moderate complexity. The "power user essentials" set.

| # | Setting | Theme |
|---|---------|-------|
| 8 | Taskbar auto-hide | Shell |
| 22 | Snap Assist config (multiple keys) | Shell |
| 25 | Multi-monitor taskbar behavior | Shell |
| 27 | Explorer status bar visibility | Shell |
| 30 | Explorer details/preview pane defaults | Shell |
| 35 | Show drive letters before/after name | Shell |
| 37 | Jump list recent items & tracking | Shell |
| 39 | Additional clocks (time zones in tray) | Shell |
| 41 | Spotlight vs custom lock screen | Shell |
| 48 | Double-click speed | Input |
| 49 | Drag-and-drop pixel threshold | Input |
| 53 | Telemetry level | Privacy |
| 55 | Activity history (local) disable | Privacy |
| 56 | Activity history (cloud) disable | Privacy |
| 57 | Location services toggle | Privacy |
| 58 | Camera & microphone default access | Privacy |
| 63 | Wi-Fi data sharing disable | Privacy |
| 66 | Error reporting (WER) disable | Privacy |
| 67 | Developer Mode toggle | Dev |
| 69 | PATH entries + order | Dev |
| 70 | Custom environment variables | Dev |
| 71 | PowerShell execution policy | Dev |
| 73 | Console code page (UTF-8) | Dev |
| 74 | Beta: UTF-8 worldwide language support | Dev |
| 75 | Console host defaults | Dev |
| 76 | App execution aliases | Dev |
| 77 | Symbolic link evaluation policy | Dev |
| 78 | Windows Defender exclusions | Dev |
| 80 | WSL PATH interop toggle | Dev |
| 81 | Sysinternals EULA pre-acceptance | Dev |
| 87 | BIOS time UTC (dual-boot) | Dev |
| 88 | WSL feature enable | Features |
| 89 | Hyper-V feature enable | Features |
| 91 | SSH Client feature enable | Features |
| 97 | Power plan selection + custom export | Power |
| 98 | Lid close action | Power |
| 99 | Sleep / display off timers | Power |
| 101 | Hibernate enable/disable | Power |
| 103 | Visual effects preference | Power |
| 111 | UAC level | Security |
| 115 | SMBv1 disable | Security |
| 116 | Lock screen timeout & notifications | Security |
| 117 | Screen saver timeout (lock timer) | Security |
| 124 | DNS preferences (DoH, custom servers) | Network |
| 127 | Wi-Fi Sense / Hotspot 2.0 disable | Network |
| 135 | Keyboard layouts & input methods | Locale |
| 136 | Input method switching hotkey | Locale |
| 140 | Default browser | File Assoc. |
| 141 | Default mail client | File Assoc. |
| 142 | Default PDF viewer | File Assoc. |
| 143 | Default image viewer | File Assoc. |
| 144 | Default media player | File Assoc. |
| 150 | "Open in Terminal" target | Context |
| 151 | Right-click "New" menu items | Context |
| 154 | Installed font set (file + registration) | Fonts |
| 155 | Per-user font installations | Fonts |
| 165 | Windows Update active hours | Update |
| 166 | Feature update deferral | Update |
| 167 | Quality update deferral | Update |
| 170 | Storage Sense config | Update |
| 173 | Focus Assist / DND auto-rules | Notify |
| 174 | Focus Assist during screen share | Notify |
| 176 | Toast notification duration | Notify |
| 179 | Recycle Bin size & per-drive | Storage |
| 182 | Thumbnail cache disable | Storage |
| 188 | Cursor size & scheme | Accessibility |
| 190 | Menu show delay | Accessibility |
| 191 | Tooltip popup delay | Accessibility |
| 192 | Window animation speed | Accessibility |
| 193 | Startup app enable/disable state | Startup |
| 197 | Default printer auto-switch disable | Peripherals |
| 200 | AutoPlay settings per media type | Peripherals |
| 205 | Xbox services disable | Services |

**Total P2: 72 settings**

---

### P3 - Advanced (Implement When Personas Demand It)

Specific persona value, special handling required.

| # | Setting | Theme |
|---|---------|-------|
| 33 | Folder view template defaults | Shell |
| 34 | Folder type detection override | Shell |
| 36 | Quick Access pinned folders | Shell |
| 38 | System tray icon overflow behavior | Shell |
| 45 | Touchpad gestures & sensitivity | Input |
| 47 | Scroll direction (natural scrolling, per-device HID) | Input |
| 50 | Caps Lock remap (binary scancode map) | Input |
| 51 | Compose key / dead keys (binary scancode map) | Input |
| 64 | Diagnostic data viewer enable | Privacy |
| 79 | WSL default distro & version | Dev |
| 90 | Windows Sandbox feature enable | Features |
| 92 | SSH Server feature enable | Features |
| 93 | .NET Framework versions | Features |
| 95 | Virtual Machine Platform | Features |
| 96 | WSL installed distro list (desired state) | Features |
| 102 | USB selective suspend disable | Power |
| 105 | Superfetch/SysMain disable | Power |
| 112 | SmartScreen settings | Security |
| 113 | Remote Desktop enable/disable | Security |
| 114 | Custom firewall rules export/import | Security |
| 123 | System proxy settings | Network |
| 125 | Network profile default (Public vs Private) | Network |
| 126 | Network discovery & file sharing per-profile | Network |
| 128 | Delivery optimization mode | Network |
| 129 | Bluetooth Quick Pair disable | Network |
| 132 | Nearby Sharing / Shared Experiences | Network |
| 133 | System locale & non-Unicode language | Locale |
| 137 | Time zone auto vs fixed | Locale |
| 139 | Display language override | Locale |
| 145 | Full "Open With" map | File Assoc. |
| 147 | Custom protocol handlers | File Assoc. |
| 152 | Shell extension block list | Context |
| 157 | Default system font override | Fonts |
| 158 | ClearType enable/disable | Fonts |
| 159 | Custom scheduled task definitions (XML) | Tasks |
| 160 | Logon-triggered tasks | Tasks |
| 161 | Event-triggered tasks | Tasks |
| 162 | Idle-triggered maintenance tasks | Tasks |
| 163 | Defrag schedule override | Tasks |
| 164 | Automatic maintenance schedule | Tasks |
| 168 | Delivery optimization mode | Update |
| 171 | Disk Cleanup presets | Update |
| 172 | Microsoft Store auto-update | Update |
| 175 | Per-app notification allow/block | Notify |
| 177 | Presentation mode settings | Notify |
| 178 | Default save locations | Storage |
| 180 | Windows Search index locations | Storage |
| 181 | Windows Search service disable | Storage |
| 183 | System Protection config | Storage |
| 186 | Low disk space warning | Storage |
| 194 | Boot timeout & default OS | Startup |
| 198 | Print Spooler service disable | Services |
| 201 | Bluetooth behavior | Peripherals |
| 202 | Windows Ink workspace toggle | Peripherals |
| 203 | Print Spooler service (same as 198) | Services |
| 204 | Bluetooth service disable | Services |
| 206 | Windows Search service (same as 181) | Services |
| 207 | SysMain service (same as 105) | Services |
| 209 | SSH Server service | Services |

**Total P3: 59 settings**

---

### P4 - Niche (Implement If Requested)

Specific use cases, hardware-dependent, or deep tuning.

| # | Setting | Theme |
|---|---------|-------|
| 82 | JIT debugger registration | Dev |
| 83 | Symbol server / debug paths | Dev |
| 84 | ODBC data sources | Dev |
| 85 | NuGet package sources (machine-level) | Dev |
| 86 | Case-sensitive directories | Dev |
| 94 | Telnet Client feature | Features |
| 106 | Memory compression toggle | Power |
| 107 | Core parking aggressiveness | Power |
| 108 | Paging executive lock | Power |
| 109 | Network throttling index | Power |
| 110 | Page file configuration | Power |
| 118 | Windows Hello PIN complexity | Security |
| 119 | Removable storage access policy | Security |
| 130 | Network adapter power management | Network |
| 131 | Persistent mapped network drives | Network |
| 146 | File type verb registrations | File Assoc. |
| 148 | MIME type → handler mapping | File Assoc. |
| 153 | "Send To" COM entries | Context |
| 156 | Font substitution table | Fonts |
| 169 | Delivery optimization cache size | Update |
| 184 | NTFS last access time disable | Storage |
| 185 | 8.3 short filename disable | Storage |
| 189 | Narrator settings | Accessibility |
| 195 | Verbose boot messages | Startup |
| 196 | Crash dump config | Startup |
| 199 | Print to PDF paper size | Peripherals |
| 208 | WinRM service | Services |
| 210 | Sandbox: networking default | Sandbox |
| 211 | Sandbox: vGPU default | Sandbox |
| 212 | Sandbox: clipboard default | Sandbox |
| 213 | Sandbox: audio/video input default | Sandbox |

**Total P4: 31 settings**

---

### P5 - Maybe Never

Risky to automate, extremely niche, or better handled by specialized tools.

| # | Setting | Theme |
|---|---------|-------|
| 120 | DEP policy (bcdedit, risky) | Security |
| 121 | Credential Guard / Device Guard (enterprise) | Security |
| 122 | Spectre/Meltdown mitigation toggles (risky) | Security |

**Total P5: 3 settings**

---

## Summary

| Tier | Count | Description |
|------|-------|-------------|
| **P1** | 47 | Core - simple registry, universally wanted |
| **P2** | 72 | Power user - strong value, moderate complexity |
| **P3** | 59 | Advanced - persona-specific, special handling |
| **P4** | 31 | Niche - specific use cases, deep tuning |
| **P5** | 3 | Maybe never - risky or better handled elsewhere |
| **Dupes** | ~5 | Settings appearing in both theme tables and services (198/203, 181/206, 105/207) |
| **Total unique** | ~207 | |

**Note:** Some services entries (203, 206, 207) duplicate settings already listed in their domain themes (198, 181, 105). These should be consolidated during implementation planning.

**Removed from inventory:** #50 (Caps Lock remap) and #51 (Compose key) - moved to P5/out of scope. Wouter uses Mi-Ke for keyboard remapping.

---

## Chaos Analysis: Infrastructure Requirements

_Stress-test of the inventory: "What breaks if we blindly apply all settings to a fresh machine?"_

See also: [Research Priority](#research-priority-interfacing-with-existing-structured-data) at the top for existing structured data sources to interface with.

### Validated Requirements (Must-Have)

| ID | Requirement | Triggered By | Priority |
|----|------------|--------------|----------|
| I1 | **Dependency graph** - features (#88-96) must deploy before their dependent settings (#79, #80, #209-213) | Feature deps before settings | Must-have |
| I2 | **Validate before apply, report failures, surface drift** - settings referencing apps, paths, or fonts that don't exist on target machine should be validated, skipped with warning, and flagged as drift | App aliases, default apps, font registration, script paths, Defender exclusions | Must-have |
| I3 | **GPO conflict detection** - before applying `HKLM\Policies\...` settings, detect if Group Policy is managing that key; warn and skip instead of silently getting overwritten every 90 minutes | Managed machines overriding privacy settings | Must-have |
| I4 | **Restart-requirement metadata per setting** - categorize settings by activation: instant, Explorer restart, logoff, full reboot. Report "Applied X settings. Y require restart." Restart Explorer automatically for Shell theme settings. | Explorer settings need restart, UTF-8 needs reboot, scancode maps need logoff, DISM features need reboot | Must-have |
| I5 | **Deployment phases** - separate quick registry (HKCU, instant) from admin registry (HKLM) from features/services (DISM, reboot). Don't mix them in one pass. | HKCU vs HKLM elevation, feature installs triggering reboot | Must-have |
| I6 | **HKCU vs HKLM elevation awareness** - `perch deploy` without admin applies HKCU, reports HKLM skipped. `perch deploy --admin` applies everything. | Silent failure of HKLM writes without elevation | Must-have |
| I7 | **Machine-role profiles (layered config)** - `base.yaml` → `role-override.yaml` → `machine-specific.yaml`. Already described in PRD. Critical for execution policy, RDP, power settings, PATH. | Execution policy synced from wrong machine, multiple machines pushing different values | Must-have (in PRD) |

### Validated Requirements (Should-Have)

| ID | Requirement | Triggered By | Priority |
|----|------------|--------------|----------|
| I8 | **Path resolution with env vars** - use `%USERPROFILE%`, `%APPDATA%`, `~/` for cross-platform. Machine-specific config handles the rest. | Defender exclusions referencing nonexistent drives | Should-have |
| I9 | **Mutual exclusion / consistency validation** - warn when settings contradict (e.g., Search service disabled but index locations configured; hibernate off but power plan uses hibernate) | Cross-setting conflicts | Should-have |
| I10 | **Suppress known warnings** - ability to mark specific warnings as "acknowledged" so that 0 warnings actually means something actionable | Noisy warnings losing signal | Should-have |

### Deferred / Out of Scope

| ID | Requirement | Triggered By | Scope |
|----|------------|--------------|-------|
| I11 | **File association API (UserChoice hash)** - Windows validates a computed hash on UserChoice registry values; direct registry write = broken. Needs proper API or hash computation. | File associations (#140-145) | Scope 5 (unless specific association needed sooner) |
| I12 | **Combined-effect security warnings** - detect when a combination of settings (UAC low + SmartScreen off + Defender wide open) creates a security hole | Security bundle combinations | Scope 5 (use reporting + suppress for now) |
| I13 | **Declarative abstraction for binary registry values** - YAML `capslock: ctrl` compiled to binary scancode map | Scancode map format | Out of scope (Mi-Ke) |
| I14 | **Ownership escalation audit trail** - explicit, auditable registry key ownership taking | TrustedInstaller-owned keys | Scope 3 (carry forward from PowerShell implementation) |

_Reference sources are documented in [Research Priority](#research-priority-interfacing-with-existing-structured-data) at the top of this document._

---

## Session Summary

**Session completed:** 2026-02-14

**Key Achievements:**
- **207 unique settings** inventoried across 21 themes, each with registry path and applicability tags
- **5-tier priority assignment** (P1: 47, P2: 72, P3: 59, P4: 31, P5: 3)
- **Research strategy** identified: interface with ADMX, WinUtil, and Sophia Script to populate the Perch gallery at scale
- **14 infrastructure requirements** validated through chaos analysis, prioritized into must-have / should-have / deferred
- **2 meta-patterns** surfaced: setting bundles (privacy, dev, dual-boot, etc.) and conditional per-machine profiles
- **10 community catalogue candidates** identified for the GitHub Pages gallery
- **Keyboard remapping** (#50, #51) removed from scope - handled by Mi-Ke

**This document feeds into:** PRD Scope 3 (registry management), Perch gallery architecture, machine-profile layered config design