---
stepsCompleted: [1, 2, 3, 4, 5, 6]
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

## Desired Emotional Response

### Primary Emotional Goals

- **Confidence:** "My configs are handled. I don't need to worry about this." The wizard delivers this at completion; the dashboard sustains it on every return visit. This is the foundational emotion — Perch exists to eliminate config anxiety.
- **Impressed:** "This is way more polished than I expected for a dotfiles tool." The visual quality (Midjourney cards, Fluent Design, app-store-like browsing) signals that this is a premium product in a space dominated by bare CLI tools. This is the emotion that makes users tell others about it.

### Emotional Journey Mapping

| Stage | Emotion | Trigger |
|---|---|---|
| Wizard launch | Curiosity + Trust | Professional visual design, polished UI — "this isn't hacked together" |
| Profile selection | Identity + Empowerment | Aspirational hero images, multi-select — "it gets me, and I'm not being boxed in" |
| System detection | Relief + Surprise | Cards appear for detected configs — "it already found my stuff without me doing anything" |
| Card browsing | Familiarity + Comfort | App-store-like layout — browsing feels natural, not technical |
| Deploy completion | Accomplishment + Closure | Clear success confirmation — "done, my machine is set up." Conclusive, not transitional |
| Dashboard first load | Calm reassurance | Green status across modules — "everything is as expected" |
| Drift detected | Clarity + Control | Yellow/red cards with obvious actions — "I see what's wrong and how to fix it" |
| Error states | Understanding | Clear explanation + next steps — no anxiety, no dead ends |

### Micro-Emotions

- **Confidence over Confusion:** Every screen communicates what it is and what to do next. No ambiguous states.
- **Trust over Skepticism:** Detection results are transparent — show what was found and where. Don't hide the technical details, just layer them behind progressive disclosure.
- **Accomplishment over Frustration:** Deploy completion is celebrated, not buried. The wizard ending should feel like finishing something meaningful.
- **Delight over mere Satisfaction:** The visual quality — profile card artwork, smooth animations, polished card layouts — elevates the experience beyond "it works" to "this is nice."

### Design Implications

- **Confidence** → Status ribbons on every card. Green/yellow/red at a glance. No ambiguous "unknown" states — always show a clear status.
- **Impressed** → Midjourney hero images, real app icons, consistent Fluent Design styling. Every pixel should feel intentional. No default gray borders or unstyled controls.
- **Relief (detection)** → System scan results appear naturally as cards populating the view. No "scanning..." modal blocking the UI — detection should feel effortless, almost instant.
- **Closure (deploy)** → Wizard completion screen with clear summary: "X configs linked, X apps configured." A satisfying endpoint, not a redirect to another view.
- **No anxiety** → Destructive actions (unlink, remove) require confirmation. Safe actions (link, fix) are one-click. Error messages always include a recovery path.

### Emotional Design Principles

1. **Status is always visible.** Users should never wonder "did it work?" Every action produces immediate visual feedback.
2. **Celebrate completion.** The wizard end and deploy success are achievement moments — design them as such.
3. **Errors are guidance, not dead ends.** Every error state tells users what happened, why, and what to do next.
4. **Visual quality is the trust signal.** In a space of terminal tools, the polish of the UI is itself a feature — it communicates "this was built with care."
5. **Never make users feel dumb.** Technical concepts (symlinks, dotfiles, registry) are presented through familiar metaphors (cards, toggles, status indicators) — not jargon.

## UX Pattern Analysis & Inspiration

### Inspiring Products Analysis

**ASUS Armoury Crate 6 (Primary Inspiration)**
The central visual and interaction reference for Perch Desktop. Key UX elements:
- **Sidebar navigation** with icon + label, section-based (Devices, Playground, Assistant). Clean hierarchy — one click to any section.
- **Dashboard with metrics front and center.** Performance stats (GPU, CPU, fans, memory) visible immediately without navigation. Maps directly to Perch's drift summary on the home screen.
- **Modular architecture.** Users install only the modules they need (Playground for RGB, Assistant for performance). Parallels Perch's profile-driven content filtering.
- **Dark theme, premium aesthetic.** ROG's gaming visual identity — polished, not utilitarian. The bar Perch should aim for in a developer/power-user context.
- **Reduced cognitive load.** Version 6 explicitly removed visual clutter from v5. "Cleaner, more modern look" with "far less visually busy" presentation. Settings consolidated under logical sections instead of scattered across dashboard.
- **Device-centric navigation.** Click a device image to see its settings. Maps to Perch's card-click-to-detail pattern — click an app card, see its config and actions.

**VS Code Extensions Panel (Card Grid Pattern)**
The reference for Perch's app browsing experience:
- Card grid layout with icon, name, brief description, and action button per card
- Tabs: Installed / Recommended / Search results
- Search bar with instant filtering
- Category sidebar for browsing
- Maps to: Perch's "Your apps" / "Suggested" / "Other apps" three-tier layout

**GitHub Desktop (Status-Focused Dashboard)**
The reference for Perch's drift concept:
- Home screen answers "what changed?" at a glance
- File-level diff indicators (added, modified, deleted) map to Perch's card status ribbons (linked, drifted, missing, broken)
- Minimal chrome — the status IS the interface
- One-click actions from the status view (commit, push) map to Perch's one-click fix/link actions

### Transferable UX Patterns

**Navigation:**
- Armoury Crate's sidebar → Perch sidebar (Home, Dotfiles, Apps, System Tweaks, Settings)
- Armoury Crate's device-click-to-detail → Perch card-click-to-detail

**Information Hierarchy:**
- Armoury Crate's "metrics front and center" → Perch drift summary as hero section on dashboard
- VS Code's Installed/Recommended/Search tabs → Perch's three-tier card layout (Your apps → Suggested → Other)

**Visual Design:**
- Armoury Crate's dark premium aesthetic → Perch dark theme with Fluent Design + Midjourney artwork
- VS Code's clean card grid → Perch app/dotfile cards with real icons and status ribbons

**Interaction:**
- GitHub Desktop's one-click actions from status → Perch one-click fix/link from drift cards
- Armoury Crate's modular install → Perch profile-driven view filtering

### Anti-Patterns to Avoid

- **The old Avalonia prototype:** 10-step wizard with bare checkboxes, no visual hierarchy, no component library, technical language. The explicit anti-reference.
- **Ninite's wall of checkboxes:** Functional but zero visual appeal. No cards, no images, no status indicators. Efficient but forgettable.
- **Settings panels disguised as dashboards:** If the home screen is just a list of toggles, it's a settings panel. The dashboard must communicate state, not offer configuration.
- **Information overload on first load:** Armoury Crate v5 was criticized for being visually busy. V6 fixed this by consolidating. Perch must launch clean — drift summary and attention items only, not every module.
- **Technical jargon in non-developer contexts:** The Apps view and System Tweaks should be free of symlink/junction/path terminology. Dotfiles views can be more technical when the user selected a Developer profile — that audience expects and prefers it. Where possible, even system configuration should use plain language ("Dark mode", "Context menu entries") rather than implementation details ("Registry key HKCU\...").

### Design Inspiration Strategy

**Adopt directly:**
- Sidebar navigation pattern (Armoury Crate)
- Dark theme with premium visual polish (Armoury Crate)
- Card grid with search for browsing content (VS Code)
- Status-focused home screen (GitHub Desktop)

**Adapt for Perch:**
- Armoury Crate's device images → Perch's Midjourney profile cards and real app icons
- VS Code's Installed/Recommended tabs → Perch's "Your apps" / "Suggested for you" / "Other apps" sections with detection-first ordering
- GitHub Desktop's file-level diffs → Perch's module-level drift cards with color-coded status ribbons

**Avoid:**
- Armoury Crate v5's visual clutter — keep the dashboard focused on drift, not configuration
- Ninite's checkbox grid — cards with images and status, never bare checkboxes
- Technical jargon in non-developer contexts — Apps and System Tweaks use plain language; Dotfiles views for Developer profiles can be technical

## Design System Foundation

### Design System Choice

**Primary:** WPF UI (lepoco/wpfui) — Fluent Design System for WPF
**Secondary:** HandyControl StepBar — wizard step indicator component
**MVVM:** CommunityToolkit.Mvvm — carried over from the Avalonia prototype

This is a **themeable established system** approach: WPF UI provides the Fluent 2 / Windows 11 design language as a foundation, with HandyControl filling specific component gaps. Custom styling on top for Perch's brand identity (logo, Midjourney artwork, color accents).

### Rationale for Selection

- **WPF UI** provides the Armoury Crate-like sidebar NavigationView, CardControl, CardAction, InfoBar, and Badge — the exact controls needed for dashboard and card-based views. Windows 11 Fluent aesthetic matches the premium, dark-theme visual goal. ~7k GitHub stars, MIT licensed, actively maintained through 2026.
- **HandyControl** adds the `StepBar` component for wizard step indicators — the one critical control WPF UI lacks. Also provides `SearchBar`, `Shield` (badges), and `CoverView` as backup options. ~6k GitHub stars, MIT licensed.
- **CommunityToolkit.Mvvm** is already proven in the codebase from the Avalonia prototype. `[ObservableProperty]`, `[RelayCommand]`, and `ObservableObject` carry over directly to WPF with zero changes.

### Implementation Approach

**Shell (WPF UI):**
- `NavigationView` for sidebar (Home, Dotfiles, Apps, System Tweaks, Settings)
- `CardControl` / `CardAction` for app cards, dotfile cards, profile cards
- Dark Fluent theme as the base aesthetic
- `InfoBar` for status messages and notifications
- `Badge` for drift status indicators on cards

**Wizard (HandyControl):**
- `StepBar` for step indicator (Profile → Dotfiles → Apps → System Tweaks → Deploy)
- Hosted in a dedicated wizard window/view, separate from the sidebar-based dashboard shell

**MVVM (CommunityToolkit.Mvvm):**
- `ObservableObject` base for all ViewModels
- `[ObservableProperty]` for bindable properties
- `[RelayCommand]` for command bindings
- ViewLocator pattern for ViewModel → View resolution (same approach as Avalonia prototype)

### Customization Strategy

- **Color palette:** Dark base from WPF UI Fluent theme. Custom accent color for Perch brand identity (to be defined — likely a nature-inspired tone to match the bird/perch concept).
- **Card backgrounds:** Profile cards use Midjourney hero images via `ImageBrush` with semi-transparent overlay for text readability. App cards use real app icons on a subtle gradient or solid background.
- **Typography:** Inter or Segoe UI Variable (Windows 11 system font) — clean, modern, readable at small sizes for card descriptions.
- **Status colors:** Green (linked/OK), Yellow (exists but not linked / needs attention), Red (broken/error), Blue (not installed / informational). Consistent across all views.
- **Logo:** Bird silhouette concept (perching bird on a branch or wire). Dark-theme-friendly, works at sidebar icon size (~24px) through splash screen size. Midjourney for concepts, vectorized for final asset.
