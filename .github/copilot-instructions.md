# VoloGen — Copilot Instructions

> These instructions guide GitHub Copilot (and other AI assistants) when working
> inside the **VoloGen** repository. Follow them unless the user explicitly asks
> for something different.

---

## 1  Project Overview

VoloGen is a collection of **Roslyn incremental source generators** that
eliminate boilerplate for value-object patterns in .NET.

| Package | Attribute | Generates |
|---|---|---|
| `VoloGen.Equality` | `[AutoEquality]` | `IEquatable<T>`, `Equals`, `GetHashCode`, `==`/`!=` |
| `VoloGen.Comparable` | `[AutoComparable]` | `IComparable<T>`, `IComparable`, `<`/`>`/`<=`/`>=`/`==`/`!=` |
| `VoloGen.Parsable` | `[AutoParsable]` | `IParsable<T>`, `ISpanParsable<T>`, optional `IUtf8SpanParsable<T>` |
| `VoloGen.Formattable` | `[AutoFormattable]` | `IFormattable`, `ISpanFormattable`, optional `IUtf8SpanFormattable` |

### Targets

- **Attribute packages** target `netstandard2.0` (consumed by any .NET version).
- **Generator packages** target `netstandard2.0` (Roslyn requirement).
- **Test projects** target `net10.0`.
- **Sample projects** target `net10.0`.

---

## 2  Code Style Rules

These rules are enforced by `.editorconfig` and must be followed at all times.

### Braces & Control Flow

- **Always** use curly braces `{ }` for `if`, `else`, `for`, `foreach`, `while`, `using`, etc.
- **Never** use single-line braceless control flow.

### Methods & Expression Bodies

- **Do not** use expression-bodied methods (`=>`). Use block bodies with `{ }`.
- Expression-bodied **getters** and **accessors** are allowed.
- **Do not** use expression-bodied constructors or operators.

### Templates & String Building

- **Do not** use `StringBuilder` for code-generation templates.
- Use raw string literals (`$$"""..."""`) for generated source templates.
- Use `StringBuilder` **only** for aggregating generated member blocks (not for templating).

### Collections

- Prefer `List<string>` over `string[]` for generated member collections.
- Use explicit `Add(...)` calls instead of collection expressions.

### Ternary Operators

- **Do not** use ternary operators (`? :`) in generator logic.
- Use explicit `if`/`else` blocks to decide whether to emit generated methods.

### Naming

- `PascalCase` for types, methods, properties, constants.
- `_camelCase` with underscore prefix for private fields.
- `I` prefix for interfaces.
- File-scoped namespaces everywhere.

### Formatting

- 4-space indentation (2 spaces for XML/JSON/YAML/csproj).
- UTF-8 encoding, LF line endings.
- Sort `System` usings first.

---

## 3  Generator Design Patterns

### Core Method Delegation

Each generator follows the same pattern:

1. The **user** implements a single core method manually:
   - `Equality` → `static bool Equal(T, T)` + `override int GetHashCode()`
   - `Comparable` → `static int Compare(T, T)`
   - `Parsable` → `static bool TryParse(ReadOnlySpan<char>, IFormatProvider?, out T)`
   - `Formattable` → `bool TryFormat(Span<char>, out int, ReadOnlySpan<char>, IFormatProvider?)`

2. The **generator** emits all remaining overloads that delegate to the core method.

### Skip-If-Exists

- Generate a method **only** when an identical overload is missing on the target type.
- Check by method name, parameter types, ref kinds, and return type.

### Operator Coordination

- `[AutoComparable]` generates `==`/`!=` operators (via `Compare`).
- `[AutoEquality]` skips `==`/`!=` when `[AutoComparable]` is also present.

### AggressiveInlining

- All generated thin-wrapper overloads are annotated with
  `[MethodImpl(MethodImplOptions.AggressiveInlining)]`.
- Core delegation methods and heavier logic (e.g., `Parse` with throw) are **not** inlined.

### Feature Flags

- `ImplementUtf8` → emits UTF-8 interface overloads.
- `ImplementExact` → emits `ParseExact`/`TryParseExact` overloads.
- `DefaultFormat` → default format string for formatless overloads.
- `AllowNullFormatProvider` → guards against null providers.

---

## 4  Diagnostics

All diagnostics use the `VoloGen` category and are errors by default.

| ID | Name | Trigger |
|---|---|---|
| VG0001 | `MissingTryParseMethod` | `[AutoParsable]` without core `TryParse` |
| VG0002 | `MustBePartial` | Type is not `partial` |
| VG0003 | `CannotBeStatic` | Type is `static` |
| VG0004 | `CannotBeAbstract` | Type is `abstract` |
| VG0005 | `MissingComparableField` | `[AutoComparable]` without `Compare` |
| VG0006 | `MissingEquatableField` | `[AutoEquality]` without `Equal`/`GetHashCode` |
| VG0007 | `MissingTryFormatMethod` | `[AutoFormattable]` without `TryFormat` |
| VG0008 | `MissingToStringOrMaxBufferSize` | `[AutoFormattable]` without `ToString` or `MaxBufferSize` |

Diagnostic messages are localized via `.resx` files in `VoloGen.Common`.

---

## 5  Testing Guidelines

- Use **xUnit** for all tests.
- Each generator has a dedicated test project under `tests/`.
- Test categories:
  - ✅ Successful generation with expected output
  - ❌ Diagnostic reporting for each error condition
  - 🔄 Edge cases (nested types, global namespace, record structs)
- Use `CSharpGeneratorTest` from `Microsoft.CodeAnalysis.Testing`.

---

## 6  Commit & PR Convention

Follow [Conventional Commits](https://www.conventionalcommits.org/).

**Format:** `<type>(<scope>): <description>`

**Types:** `feat`, `fix`, `refactor`, `docs`, `test`, `build`, `ci`, `chore`, `perf`, `style`

**Scopes:** `parsable`, `formattable`, `comparable`, `equality`, `common`, `ci`, `readme`, `samples`

---

## 7  Repository Structure

```
VoloGen/
├── .github/              # CI, issue templates, copilot instructions
├── samples/              # End-to-end usage examples
├── src/
│   ├── VoloGen.Common/           # Shared diagnostics, helpers, resx
│   ├── VoloGen.Equality/         # [AutoEquality] attribute
│   ├── VoloGen.Equality.Generator/
│   ├── VoloGen.Comparable/       # [AutoComparable] attribute
│   ├── VoloGen.Comparable.Generator/
│   ├── VoloGen.Parsable/         # [AutoParsable] attribute
│   ├── VoloGen.Parsable.Generator/
│   ├── VoloGen.Formattable/      # [AutoFormattable] attribute
│   └── VoloGen.Formattable.Generator/
├── tests/
│   ├── VoloGen.Equality.Tests/
│   ├── VoloGen.Comparable.Tests/
│   ├── VoloGen.Parsable.Tests/
│   └── VoloGen.Formattable.Tests/
├── .editorconfig
├── Directory.Build.props
├── VoloGen.snk
└── VoloGen.slnx
```

