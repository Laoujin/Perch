---
stepsCompleted: [1, 2, 3]
inputDocuments:
  - '_bmad-output/planning-artifacts/prd.md'
  - '_bmad-output/planning-artifacts/prd-validation-report.md'
  - '_bmad-output/planning-artifacts/architecture.md'
  - '_bmad-output/planning-artifacts/epics.md'
  - '_bmad-output/planning-artifacts/competitive-research.md'
  - '_bmad-output/planning-artifacts/chezmoi-comparison.md'
---

# UX Design Specification Perch

**Author:** Wouter
**Date:** 2026-02-16

---

<!-- UX design content will be appended sequentially through collaborative workflow steps -->

## Executive Summary

### Project Vision

Perch Desktop is a WPF desktop application for managing dotfiles and application settings on Windows. It replaces the previous Avalonia prototype with a polished, visually appealing experience built on a modern WPF component library (WPF UI, HandyControl, or similar). The app serves two modes: a streamlined onboarding wizard for first-run setup, and a drift-focused dashboard for ongoing config health monitoring — both sharing the same reusable view components backed by the Perch.Core engine.

### Target Users

- **Developers:** Primary audience. Interested in dotfiles (.gitconfig, .vimrc), editor settings, shell configs. Shown the full experience including dotfiles management.
- **Power Users:** System tweakers managing app settings, Windows registry, terminal configs. Similar to developers but less focused on dotfiles.
- **Gamers / Casual Users:** Simplified experience. Dotfiles step hidden. Focus on app settings and system tweaks.

Profile selection at onboarding (with Midjourney-generated hero image cards) drives which views and content are shown throughout the app.

### Key Design Decisions

- **WPF over Avalonia:** Richer component library ecosystem, better designer tooling, modern Fluent Design libraries available. Windows-only is acceptable — the CLI handles cross-platform.
- **Component library:** Evaluating WPF UI (lepoco/wpfui), UI.WPF.Modern (iNKORE-NET), and HandyControl. Need sidebar NavigationView, card controls, step indicators, badges.
- **Drift-focused dashboard:** The daily-driver view. Shows config health at a glance — what's linked, what's broken, what needs attention. Not a settings panel; a verification tool.
- **Reusable views:** Dotfiles, Applications, and System Tweaks are UserControls used in both wizard (step-by-step with stepper) and dashboard (standalone via sidebar navigation) contexts.
- **Smart detection:** App and dotfile cards are populated by scanning the system. Detected items shown prominently with status ribbons. Gallery items shown as suggestions below. Full gallery searchable.
- **Visual polish:** Midjourney-generated hero images for profile cards. Real app icons/logos for app cards. Perch logo (bird silhouette concept) in sidebar. Dark theme.
- **Startup loading:** Module/drift state loaded once at startup, no real-time file watching needed.

### Key Design Challenges

- Bridging a power-user domain (symlinks, dotfiles, platform paths) with an approachable visual interface
- Card information density: status ribbon, app icon, name, description, and action menu must be scannable without clutter
- Wizard-to-dashboard transition: same components must feel natural in both guided (wizard) and standalone (dashboard) contexts
- Gallery dependency: detection-first design must degrade gracefully when the app gallery is unavailable or incomplete

### Design Opportunities

- Profile-driven filtering eliminates irrelevant options and reduces cognitive load
- Drift dashboard as a "config health control center" — unique value no CLI can replicate
- Detection-first card layout ("here's what you have") feels intelligent rather than asking users to configure from scratch
- Card-based UI is inherently visual and can showcase the Midjourney/icon artwork

## Core User Experience

### Defining Experience

Perch Desktop serves two distinct but equally important experiences:

**The Wizard (first run, potentially the only run):** A guided onboarding flow where users select their profile(s), see their system reflected back as smart-detected cards, toggle what they want managed, and deploy. For many users, this is the complete Perch Desktop experience — run once, configs linked, done. It must feel polished and complete as a standalone experience, not merely a stepping stone to the dashboard.

**The Dashboard (ongoing, for users who return):** A drift-focused config health overview. Open Perch, see what's correct, what's changed, what needs attention. Quick actions to fix issues. Sidebar navigation into the same views used by the wizard for deeper management.

Both experiences share the same card-based view components and the same Perch.Core engine.

### Platform Strategy

- **WPF, Windows-only.** The CLI remains cross-platform. The desktop app targets Windows where the richest component library ecosystem exists.
- **Mouse/keyboard primary.** Dark theme. Modern Fluent Design aesthetic via WPF UI, HandyControl, or similar library.
- **Offline-capable.** Engine and detection work locally. App gallery requires network for initial download but caches locally afterward.
- **Startup loading.** Module and drift state loaded once at app launch. No real-time file watchers or polling.

### Effortless Interactions

- **Profile selection is multi-select.** Users pick all profiles that apply (e.g., Developer + Power User + Gamer). The union of selected profiles determines which views and content are shown. Cards represent profiles with Midjourney hero images — aspirational, not clinical.
- **Detection is automatic and invisible.** System scan runs behind the scenes; results appear as cards. No "scan now" button or loading spinner dominating the experience.
- **Card actions are one-click.** "Link this", "Fix this", "Unlink", "Ignore" — no confirmation dialogs for safe operations.
- **Profile-driven filtering is seamless.** Selecting Developer + Gamer shows developer dotfiles, gaming app configs, and system tweaks — but hides nothing the user explicitly selected.
- **Sidebar navigation** puts every view one click away in dashboard mode.

### Critical Success Moments

1. **Profile selection (wizard step 1):** Visually striking cards make the user feel understood — "this app gets who I am." Multi-select feels empowering, not restrictive.
2. **Smart detection (wizard step 2-3):** "It already found my .gitconfig, my VS Code settings, my terminal config." The system reflects the user's machine back at them. This is the moment that builds trust.
3. **Deploy completion (wizard final step):** Clear confirmation that configs are linked. For users who never return, this is the lasting impression — it must feel conclusive and satisfying.
4. **Dashboard first load (returning users):** Instant visual summary of config health. Green = confidence. Yellow/red = clear path to resolution.
5. **Drift resolution:** See a problem card, click it, fix it. Minimal friction from detection to resolution.

### Experience Principles

1. **Detection-first, not configuration-first.** Show users what they already have. Don't ask them to build from scratch.
2. **The wizard is the product for most users.** Design it as a complete, standalone experience — not a setup screen for the dashboard.
3. **Visual over textual.** Cards with icons, status ribbons, and images over lists of file paths. The UI should feel like an app store, not a file manager.
4. **Progressive disclosure.** Simple card toggles by default. Details, options, and advanced actions available on click — never in the way.
5. **Respect the one-time user.** Every wizard step must earn its existence. No unnecessary steps, no "we'll use this later" justifications.
