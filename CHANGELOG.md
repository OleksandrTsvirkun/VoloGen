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
