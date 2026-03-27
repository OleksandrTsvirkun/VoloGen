## [1.0.0] - 2026-03-27

## What's Changed
* chore: add Husky.NET git hooks and commit message linting by @OleksandrTsvirkun in https://github.com/OleksandrTsvirkun/VoloGen/pull/2
* test: add unit tests for all generators and combination tests by @OleksandrTsvirkun in https://github.com/OleksandrTsvirkun/VoloGen/pull/1
* feat: add Roslyn source generators for Comparable, Equality, Formattable, and Parsable by @OleksandrTsvirkun in https://github.com/OleksandrTsvirkun/VoloGen/pull/3
* build: add project infrastructure and build configuration by @OleksandrTsvirkun in https://github.com/OleksandrTsvirkun/VoloGen/pull/4
* feat(samples): add end-to-end usage examples for all generators by @OleksandrTsvirkun in https://github.com/OleksandrTsvirkun/VoloGen/pull/5
* docs: add README and community files (CONTRIBUTING, CODE_OF_CONDUCT, SECURITY, CHANGELOG) by @OleksandrTsvirkun in https://github.com/OleksandrTsvirkun/VoloGen/pull/7
* ci: add GitHub Actions CI, community templates, and Copilot instructions by @OleksandrTsvirkun in https://github.com/OleksandrTsvirkun/VoloGen/pull/6
* ci: add GitHub Pages documentation deployment workflow by @OleksandrTsvirkun in https://github.com/OleksandrTsvirkun/VoloGen/pull/8
* docs: add GitHub Pages documentation site with Just the Docs theme by @OleksandrTsvirkun in https://github.com/OleksandrTsvirkun/VoloGen/pull/9
* ci: add NuGet publish and GitHub release workflow on tag push by @OleksandrTsvirkun in https://github.com/OleksandrTsvirkun/VoloGen/pull/10
* fix(build): include root README.md in NuGet packages by @OleksandrTsvirkun in https://github.com/OleksandrTsvirkun/VoloGen/pull/11
* fix(build): suppress NU5128 for analyzer-only generator packages by @OleksandrTsvirkun in https://github.com/OleksandrTsvirkun/VoloGen/pull/12

## New Contributors
* @OleksandrTsvirkun made their first contribution in https://github.com/OleksandrTsvirkun/VoloGen/pull/2

**Full Changelog**: https://github.com/OleksandrTsvirkun/VoloGen/commits/v1.0.0
# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com),
and this project adheres to [Semantic Versioning](https://semver.org).

## [Unreleased]

### Added

- `[AutoParsable]` source generator -- emits `IParsable<T>`, `ISpanParsable<T>`, and optional `IUtf8SpanParsable<T>` implementations
- `[AutoFormattable]` source generator -- emits `IFormattable`, `ISpanFormattable`, and optional `IUtf8SpanFormattable` implementations
- `[AutoComparable]` source generator -- emits `IComparable<T>`, `IComparable`, and all comparison operators
- `[AutoEquality]` source generator -- emits `IEquatable<T>`, `Equals`, `GetHashCode`, and equality operators
- Skip-if-exists detection -- generators skip methods the user already provides
- Cross-attribute coordination -- `[AutoEquality]` defers `==`/`!=` when `[AutoComparable]` is present
- `ImplementUtf8` flag for Parsable and Formattable generators
- `ImplementExact` flag for `ParseExact`/`TryParseExact` overloads
- `DefaultFormat` and `AllowNullFormatProvider` flags for Formattable generator
- 8 compile-time diagnostics (VG0001--VG0008) with localized messages
- Strong-name signed assemblies
- GitHub Pages documentation site
- 73 tests across 5 test projects (unit, edge-case, and combination tests)
- 4 sample projects with multiple value-object examples
- CI workflow (build + test on push/PR)
- NuGet publish workflow (tag-triggered)
- Conventional Commits enforcement via Husky.NET + CommitLint.NET
