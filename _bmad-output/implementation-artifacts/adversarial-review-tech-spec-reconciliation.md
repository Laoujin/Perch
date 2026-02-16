# Adversarial Review: Tech-Spec Full-Scope Reconciliation

**Reviewed:** 2026-02-16
**Source:** `tech-spec-perch-full-scope-reconciliation.md`
**Status:** 14 findings

---

1. **Enormous scope with no phasing or MVP definition.** Seven streams, 25+ tasks, four separate repos, zero milestones. There's no "what ships first and is independently useful." This is a big-bang rewrite masquerading as a feature spec. A single developer will drown in cross-repo context switches with no shippable increments along the way.

2. **Critical path is unmapped and dependencies are hand-waved.** "Stream A should complete before Stream E" hides a 6-task → 5-task waterfall. D3 depends on E4, which depends on E1-E2, which depends on A being done. The actual critical path is A1→A2→A3→A4→E1→E2→E3→E4→D3 -- a 9-task serial chain that isn't called out. No developer can plan their week from the dependency notes as written.

3. **packages.yaml migration has a gaping hole.** The spec acknowledges 109 packages need gallery entries and ~50+ require manual creation. But no task covers creating those 50+ manual entries. Task A5 (WinUtil converter) produces stubs for ~300 apps, but the mapping from "109 specific packages I actually use" to "gallery entries that exist" is completely unaddressed. Retiring packages.yaml (D3) before this is done = broken deploys on day one.

4. **Gallery/manifest merge strategy is acknowledged as high-risk, then left unresolved.** Task E3 says "manifest values win, arrays are additive unless explicitly overridden." What does "explicitly overridden" mean? How does a config say "remove this gallery-provided link"? There's no subtraction/negation syntax. The High-Risk Notes section raises this exact question and then the task ignores it. This is the single most architecturally complex piece and it has the least design.

5. **Template variable namespace and escaping are undefined.** `{{variable.path}}` has no namespace separation. MachineProfile.Variables priority over built-ins means built-ins (`machine.name`, `platform`) can be silently shadowed by a typo in profile YAML. No escaping mechanism for literal `{{` in config files -- any config file that legitimately contains double-braces (Mustache templates, Handlebars, GitHub Actions expressions) will be corrupted by the template engine.

6. **Clean filter cross-platform problem is identified and then punted.** The High-Risk Notes say PowerShell scripts need PS 7+ and bash doesn't work on Windows. The spec "considers" using Perch CLI as the filter binary but no task implements that. Tasks C1-C3 assume filter scripts exist and work. On a fresh Linux/macOS machine without PowerShell 7+, every clean filter silently fails with no error path defined.

7. **Git-Config submodule adds complexity for unclear benefit.** Task D1 replaces a simple `git/` folder with a submodule to justify the concept of "gallery starter templates" -- a concept that is never defined, has no other users, and has no task implementing the general mechanism. Meanwhile, the spec itself acknowledges submodules add clone friction and need auto-init during deploy. One folder gets special treatment that nothing else in the system uses.

8. **`install.yaml` format is severely underspecified.** Defined as `apps: [git, vscode, nvm, bun, ...]` -- a flat ID list. No version pinning, no conditional installation per machine profile, no grouping or ordering, no handling of "app ID not found in gallery." The current `packages.yaml` with 109 entries likely has implicit ordering. Replacing a working (if inelegant) format with an underspecified one is a regression waiting to happen.

9. **"Choose website stack" (Task F1) is a research task in a "ready-for-dev" spec.** A tech spec marked `status: ready-for-dev` should contain decisions, not defer them. F1 blocks F2 and F3, making all of Stream F unplannable and unestimable. Either make the decision now or remove Stream F from this spec.

10. **Zero rollback or transition strategy.** Multiple breaking changes land: packages.yaml removal, gallery overlay, install.yaml introduction, submodule migration. Can `perch deploy` work with both old and new formats during transition? What happens if someone pulls the config repo changes but hasn't updated the Perch binary? No feature flags, no format version detection, no graceful degradation. A half-migrated state is a broken state.

11. **Acceptance criteria are happy-path only with unresolved design choices baked in.** AC-B6 literally says "reports a warning and leaves the placeholder as-is *(or fails, depending on strict mode)*" -- an open design question in what should be a testable criterion. No ACs cover: converter error handling on malformed JSON (A5), deploy when gallery path doesn't exist (E1-E3), submodule not initialized (D1), registry keys that don't exist (E5), or template processing on a file with no placeholders (B3).

12. **`PerchSettings.GalleryPath` default assumes a fragile sibling-directory convention.** `<config-repo>/../perch-gallery/catalog` only works if the user clones both repos to adjacent folders. CI pipelines, Docker containers, and anyone who organizes repos differently will hit a silent "gallery not found" on first deploy. The spec doesn't define what happens when the gallery path is missing or invalid.

13. **The spec mixes "what the system does" with "what a human must manually curate" and doesn't distinguish them.** Task A6 (collect logos from official sites), Task A5 (manual selection of which WinUtil apps to import), Task D3 (mapping 109 packages to gallery entries) -- these are substantial manual curation efforts buried as single task bullets alongside automatable work. No time estimates, no tooling for the curation workflow, no "good enough" threshold.

14. **No consideration of the deploy-time performance impact.** Current deploy reads one `packages.yaml` and 13 manifests. The new deploy reads 13 manifests + gallery catalog (30+ YAML files) + `install.yaml` + performs merge logic per module + resolves templates + registers clean filters. The spec adds multiple YAML parse passes and a merge algorithm to every deploy with zero discussion of performance, caching, or whether this is acceptable for a CLI tool that should feel instant.
