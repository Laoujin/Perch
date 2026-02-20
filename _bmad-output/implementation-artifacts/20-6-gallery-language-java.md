# Story 20.6: Gallery Language — Java Ecosystem

Status: ready-for-dev

## Story

As a Perch Desktop user browsing the Languages page,
I want the Java ecosystem to have complete gallery entries covering JDKs, build tools, IDEs, and config files,
so that the Java ecosystem detail page shows my full Java toolchain.

## Design Source

`_bmad-output/design-thinking-2026-02-19.md` — Languages page, unified gallery architecture.

## Current Gallery State

In `../perch-gallery/catalog/apps/java/`:
- `temurin-jdk.yaml` — runtime with suggests for corretto-jdk, maven, gradle, intellij-idea-community
- `corretto-jdk.yaml`, `maven.yaml`, `gradle.yaml`, `intellij-idea-community.yaml`

## What Needs Verification / Enrichment

### Sub-categories Expected
1. **JDKs / Runtimes**: Eclipse Temurin, Amazon Corretto, Oracle JDK, GraalVM
2. **Build Tools**: Maven, Gradle, Ant
3. **IDEs**: IntelliJ IDEA Community, IntelliJ IDEA Ultimate, Eclipse
4. **Global Tools**: common Maven/Gradle plugins, JMH
5. **Configuration Files**: settings.xml (Maven), gradle.properties

### Per-Entry Checklist
- [ ] `temurin-jdk.yaml`: verify kind: runtime, suggests covers full ecosystem
- [ ] Alternatives: Temurin ↔ Corretto ↔ Oracle JDK as JDK alternatives; Maven ↔ Gradle as build tool alternatives; IntelliJ ↔ Eclipse as IDE alternatives
- [ ] Config files: Maven settings.xml (`%USERPROFILE%\.m2\settings.xml` / `$HOME/.m2/settings.xml`), gradle.properties

## Acceptance Criteria

1. **Complete ecosystem.** Java runtime's `suggests` list references all ecosystem tools.
2. **Sub-category coverage.** At least one entry per: JDKs, Build Tools, IDEs.
3. **Alternatives set.** JDKs, build tools, and IDEs have alternatives defined.
4. **Config file entries.** Maven settings.xml has kind: dotfile entry with config.links.
5. **Install IDs verified.**
6. **Gallery index regenerated.**

## Tasks / Subtasks

- [ ] Task 1: Audit existing Java gallery entries
- [ ] Task 2: Create missing entries (additional JDKs, build tools, config files)
- [ ] Task 3: Verify/update suggests, requires, alternatives
- [ ] Task 4: Add config.links for Java config files
- [ ] Task 5: Verify install IDs
- [ ] Task 6: Regenerate gallery index

## Constraints

- Changes are in `../perch-gallery/` repo.
- Follow existing YAML schema conventions.
