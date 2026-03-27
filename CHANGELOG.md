# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com),
and this project adheres to [Semantic Versioning](https://semver.org).

## [1.0.0] - 2026-03-27

### Added

- `[AutoParsable]` source generator — emits `IParsable<T>`, `ISpanParsable<T>`, and optional `IUtf8SpanParsable<T>`
- `[AutoFormattable]` source generator — emits `IFormattable`, `ISpanFormattable`, and optional `IUtf8SpanFormattable`
- `[AutoComparable]` source generator — emits `IComparable<T>`, `IComparable`, and all comparison operators
- `[AutoEquality]` source generator — emits `IEquatable<T>`, `Equals`, `GetHashCode`, and equality operators
- Skip-if-exists detection — generators skip methods the user already provides
- Cross-attribute coordination — `[AutoEquality]` defers `==`/`!=` when `[AutoComparable]` is present
- `ImplementUtf8` flag for Parsable and Formattable generators
- `ImplementExact` flag for `ParseExact`/`TryParseExact` overloads
- `DefaultFormat` and `AllowNullFormatProvider` flags for Formattable generator
- 8 compile-time diagnostics (VG0001–VG0008) with localized messages
- Strong-name signed assemblies
- GitHub Pages documentation site
- 73 unit tests across 5 test projects
- 4 sample projects with end-to-end usage examples
- CI workflow (build + test on 3 OS)
- NuGet publish workflow (tag-triggered)
- Conventional Commits enforcement via Husky.NET + CommitLint.NET
