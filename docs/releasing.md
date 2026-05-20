---
title: Release Process
permalink: /releasing/
---

This page describes how VoloGen packages are versioned, tagged, and published to NuGet.

---

## Overview

Releases are fully automated through GitHub Actions.
A maintainer creates and pushes a version tag; the CI pipeline takes care of everything else.

```
git tag v1.2.3
git push origin v1.2.3
```

That single action triggers the [Publish workflow](#publish-workflow) which:

1. Runs the full build and test suite
2. Packs all NuGet packages with the version derived from the tag
3. Pushes the packages to [nuget.org](https://www.nuget.org/)
4. Generates release notes and creates a GitHub Release
5. Prepends a new entry to `CHANGELOG.md` on `main`

---

## Versioning

VoloGen follows [Semantic Versioning](https://semver.org/):

| Version segment | When to increment |
|:----------------|:------------------|
| **MAJOR** (`x.0.0`) | Breaking changes to public API or generated code shape |
| **MINOR** (`0.x.0`) | New generators, new attribute properties, new opt-in overloads |
| **PATCH** (`0.0.x`) | Bug fixes, diagnostics corrections, performance tweaks |

Tags must be prefixed with `v`, e.g. `v1.0.0`, `v2.3.1`.

---

## Commit Convention

All commits on `main` (and in PRs targeting `main`) must follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <description>
```

### Types

| Type | Purpose |
|:-----|:--------|
| `feat` | New feature or generator capability |
| `fix` | Bug fix |
| `refactor` | Internal restructuring without behaviour change |
| `perf` | Performance improvement |
| `docs` | Documentation-only changes |
| `test` | Adding or updating tests |
| `build` | Build system or dependency changes |
| `ci` | CI/CD workflow changes |
| `chore` | Routine maintenance |
| `style` | Formatting, whitespace |

### Scopes

Use the package name in lowercase: `parsable`, `formattable`, `comparable`, `equality`, `common`, `ci`, `readme`, `samples`.

### Examples

```
feat(parsable): add ThrowOnNull support to [AutoParsable]
fix(equality): emit == operator when AutoComparable is absent
docs(readme): add ThrowOnNull example
test(parsable): cover ThrowOnNull opt-out path
```

---

## Publish Workflow

The workflow lives at `.github/workflows/publish.yml` and is triggered by any `v*` tag push.

### Steps

| Step | What it does |
|:-----|:-------------|
| **Checkout** | Full clone (`fetch-depth: 0`) so tag history is available |
| **Setup .NET** | Installs the SDK version required by the repo |
| **Extract version** | Strips the `v` prefix from the tag to obtain the bare version string |
| **Restore** | `dotnet restore` |
| **Build** | `dotnet build --configuration Release` |
| **Test** | `dotnet test --configuration Release` — release is aborted on any failure |
| **Pack** | `dotnet pack -p:PackageVersion=<version>` — outputs `.nupkg` files to `./nupkgs/` |
| **Push to NuGet** | Uploads all `.nupkg` files; skips already-published versions (`--skip-duplicate`) |
| **Generate release notes** | Calls the GitHub API to auto-generate notes from merged PRs since the previous tag |
| **Update CHANGELOG** | Prepends the new version entry to `CHANGELOG.md` and pushes a `[skip ci]` commit to `main` |
| **Create GitHub Release** | Creates a GitHub Release associated with the tag, using the generated notes |

### Required secret

| Secret | Description |
|:-------|:------------|
| `NUGET_API_KEY` | NuGet.org API key with push permissions for the `VoloGen.*` package prefix |

---

## Step-by-step: Creating a Release

1. **Ensure `main` is green** — all CI checks on the target commit must pass.

2. **Update `CHANGELOG.md`** *(optional pre-tag entry)* — add a `## [Unreleased]` section summarising the changes if you want to provide hand-crafted notes; the automation will still prepend the auto-generated entry on tag push.

3. **Create and push the tag:**

   ```bash
   git checkout main
   git pull origin main
   git tag v1.2.3
   git push origin v1.2.3
   ```

4. **Monitor the workflow** at `Actions → Publish` on GitHub.

5. **Verify the release** — check [nuget.org](https://www.nuget.org/profiles/OleksandrTsvirkun) and the GitHub Releases page.

---

## Hotfix Releases

For urgent fixes against an older version:

1. Create a branch from the target tag:

   ```bash
   git checkout -b hotfix/v1.2.4 v1.2.3
   ```

2. Apply the fix, push the branch, and open a PR if review is needed.

3. Tag directly from the hotfix branch:

   ```bash
   git tag v1.2.4
   git push origin v1.2.4
   ```

   The Publish workflow fires on the tag regardless of the source branch.

4. Cherry-pick the fix onto `main` if applicable.

---

## Packages Published

Each release publishes the following NuGet packages:

| Package | Contains |
|:--------|:---------|
| `VoloGen.Equality` | `[AutoEquality]` attribute |
| `VoloGen.Equality.Generator` | Roslyn source generator |
| `VoloGen.Comparable` | `[AutoComparable]` attribute |
| `VoloGen.Comparable.Generator` | Roslyn source generator |
| `VoloGen.Parsable` | `[AutoParsable]` attribute |
| `VoloGen.Parsable.Generator` | Roslyn source generator |
| `VoloGen.Formattable` | `[AutoFormattable]` attribute |
| `VoloGen.Formattable.Generator` | Roslyn source generator |
