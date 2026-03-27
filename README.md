# VoloGen

[![CI](https://github.com/OleksandrTsvirkun/VoloGen/actions/workflows/ci.yml/badge.svg)](https://github.com/OleksandrTsvirkun/VoloGen/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/VoloGen.Parsable?color=blue&label=NuGet)](https://www.nuget.org/profiles/oleksandr.tsvirkun)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Docs](https://img.shields.io/badge/docs-GitHub%20Pages-blue)](https://oleksandrtsvirkun.github.io/VoloGen/)

## What is VoloGen?

**VoloGen** is a collection of **Roslyn incremental source generators** that produce standard method overloads, interface implementations, and operators from core logic **you write yourself**.

> **VoloGen is not a value-object generator.**
> It does not create types for you, impose wrapper structures, or dictate how your domain model looks.
> Instead, it takes the **one method you already wrote** — the single source of truth for parsing, formatting, comparing, or equality — and generates every remaining overload, interface member, and operator that the .NET type system expects.

### The idea

Every .NET interface like `IParsable<T>`, `IComparable<T>`, or `IEquatable<T>` demands multiple method signatures that ultimately delegate to the same core logic. Writing them by hand is tedious, error-prone, and produces hundreds of lines of boilerplate that obscure the real business rules.

VoloGen flips this around:

1. **You** define the core behavior — the *"how"* — in a single method.
2. **The generator** produces all the standard overloads that delegate to your implementation.
3. **You stay in control** — every generated method can be overridden by simply declaring it yourself.

```
┌──────────────────────────────────────────────────────────────────┐
│  Your code (the rule)         │  Generated code (the overloads) │
│───────────────────────────────│─────────────────────────────────│
│  static bool TryParse(...)    │  Parse(string, IFormatProvider?) │
│                               │  Parse(ReadOnlySpan<char>, ...)  │
│                               │  TryParse(string?, ...)          │
│                               │  IParsable<T>.Parse(...)         │
│                               │  ISpanParsable<T>.Parse(...)     │
│                               │  IUtf8SpanParsable<T>.*(...)     │
└──────────────────────────────────────────────────────────────────┘
```

Each generator ships as an **independent NuGet package** — use only what you need.

**[Full Documentation →](https://oleksandrtsvirkun.github.io/VoloGen/)**

---

## How it works

| You implement | Generator produces |
|---|---|
| **One core method** with your business logic | All standard overloads delegating to it |
| Nothing else | Interface implementations (`IParsable<T>`, `IFormattable`, etc.) |
| | Operators (`==`, `!=`, `<`, `>`, `<=`, `>=`) |
| | Convenience overloads (string, span, UTF-8) |

The generator **never invents behavior** — every generated method is a thin wrapper that calls your code. If you already declared a specific overload, the generator skips it (*skip-if-exists*).

---

## Features

- **You define the rules** — generators only produce overloads, never business logic.
- **Zero runtime dependency** — generated code is plain C#, no base classes or helper libraries.
- **Incremental generation** — only re-generates when the annotated type changes.
- **Skip-if-exists** — already have a custom `Equals` or `Parse`? The generator won't overwrite it.
- **Cross-generator coordination** — `[AutoComparable]` and `[AutoEquality]` cooperate on `==`/`!=`.
- **Strong-name signed** — all assemblies are signed with `VoloGen.snk`.
- **Localized diagnostics** — error messages are translated via `.resx` resource files.

---

## Packages

| Package | Attribute | What you write | What gets generated |
|---------|-----------|----------------|---------------------|
| `VoloGen.Parsable` | `[AutoParsable]` | `TryParse(ReadOnlySpan<char>, ...)` | `Parse`, `TryParse` overloads, `IParsable<T>`, `ISpanParsable<T>`, opt. `IUtf8SpanParsable<T>` |
| `VoloGen.Formattable` | `[AutoFormattable]` | `TryFormat(Span<char>, ...)` | `ToString`, `TryFormat` overloads, `IFormattable`, `ISpanFormattable`, opt. `IUtf8SpanFormattable` |
| `VoloGen.Comparable` | `[AutoComparable]` | `static int Compare(T, T)` | `CompareTo` overloads, `IComparable<T>`, `IComparable`, all comparison operators |
| `VoloGen.Equality` | `[AutoEquality]` | `static bool Equal(T, T)` + `GetHashCode()` | `Equals` overloads, `IEquatable<T>`, `==`/`!=` operators |

---

## Quick Start

### 1. Install

```bash
dotnet add package VoloGen.Parsable
dotnet add package VoloGen.Parsable.Generator
```

### 2. Write your core logic

```csharp
using VoloGen;

[AutoParsable]
public partial struct Amount
{
    private readonly decimal _value;

    // This is the only method you write — the single source of truth.
    // The generator will produce every other Parse/TryParse overload from it.
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Amount result)
    {
        if (decimal.TryParse(s, provider, out var value))
        {
            result = new Amount { _value = value };
            return true;
        }
        result = default;
        return false;
    }
}
```

### 3. Use the generated API

```csharp
// All of these are generated — each one delegates to your TryParse above.
var a = Amount.Parse("42.5");
var b = Amount.Parse("42.5".AsSpan());
bool ok = Amount.TryParse("abc", out _);                       // false
bool ok2 = Amount.TryParse("100", CultureInfo.InvariantCulture, out _); // true
```

---

## Core method → Generated overloads

### AutoEquality

| You write | Generated |
|---|---|
| `static bool Equal(T left, T right)` | `bool Equals(T)` — `IEquatable<T>` |
| `override int GetHashCode()` | `override bool Equals(object?)` |
| | `operator ==`, `operator !=` |

> When `[AutoComparable]` is also present, `==`/`!=` are emitted by the comparable generator to ensure consistency.

### AutoComparable

| You write | Generated |
|---|---|
| `static int Compare(T left, T right)` | `int CompareTo(T)` — `IComparable<T>` |
| | `int IComparable.CompareTo(object?)` |
| | `<`, `>`, `<=`, `>=`, `==`, `!=` operators |

### AutoParsable

| You write | Generated |
|---|---|
| `static bool TryParse(ReadOnlySpan<char>, IFormatProvider?, out T)` | `Parse(string, IFormatProvider?)` |
| | `Parse(ReadOnlySpan<char>, IFormatProvider?)` |
| | `TryParse(string?, IFormatProvider?, out T)` |
| | Providerless & spanless convenience overloads |
| **Optional flag:** `ImplementUtf8 = true` | `IUtf8SpanParsable<T>` overloads |
| **Optional flag:** `ImplementExact = true` | `ParseExact` / `TryParseExact` overloads |

### AutoFormattable

| You write | Generated |
|---|---|
| `bool TryFormat(Span<char>, out int, ReadOnlySpan<char>, IFormatProvider?)` | `IFormattable` — `ToString(string?, IFormatProvider?)` |
| `const int MaxBufferSize` *or* `ToString(string?, IFormatProvider?)` | `ISpanFormattable` convenience overloads |
| | `override string ToString()` |
| **Optional flag:** `ImplementUtf8 = true` | `IUtf8SpanFormattable` overloads |
| **Optional flag:** `DefaultFormat = "G"` | Default format for formatless overloads |
| **Optional flag:** `AllowNullFormatProvider = false` | Null-provider guard with `ArgumentNullException` |

---

## Diagnostics

All diagnostics use the **VoloGen** category and are errors by default.

| ID | Name | Trigger |
|---|---|---|
| **VG0001** | `MissingTryParseMethod` | `[AutoParsable]` without core `TryParse` |
| **VG0002** | `MustBePartial` | Type is not `partial` |
| **VG0003** | `CannotBeStatic` | Type is `static` |
| **VG0004** | `CannotBeAbstract` | Type is `abstract` |
| **VG0005** | `MissingComparableField` | `[AutoComparable]` without `Compare` |
| **VG0006** | `MissingEquatableField` | `[AutoEquality]` without `Equal` / `GetHashCode` |
| **VG0007** | `MissingTryFormatMethod` | `[AutoFormattable]` without `TryFormat` |
| **VG0008** | `MissingToStringOrMaxBufferSize` | `[AutoFormattable]` without `ToString` or `MaxBufferSize` |

---

## Project Structure

```
VoloGen/
+-- .github/                  # CI workflows, issue templates, Copilot instructions
+-- samples/                  # End-to-end usage examples
|   +-- VoloGen.Samples.Equality/
|   +-- VoloGen.Samples.Comparable/
|   +-- VoloGen.Samples.Parsable/
|   +-- VoloGen.Samples.Formattable/
+-- src/
|   +-- VoloGen.Common/       # Shared diagnostics, helpers, localized .resx
|   +-- VoloGen.Equality/     # [AutoEquality] attribute
|   +-- VoloGen.Equality.Generator/
|   +-- VoloGen.Comparable/   # [AutoComparable] attribute
|   +-- VoloGen.Comparable.Generator/
|   +-- VoloGen.Parsable/     # [AutoParsable] attribute
|   +-- VoloGen.Parsable.Generator/
|   +-- VoloGen.Formattable/  # [AutoFormattable] attribute
|   +-- VoloGen.Formattable.Generator/
+-- tests/
|   +-- VoloGen.Equality.Tests/
|   +-- VoloGen.Comparable.Tests/
|   +-- VoloGen.Parsable.Tests/
|   +-- VoloGen.Formattable.Tests/
|   +-- VoloGen.Combinations.Tests/  # Multi-attribute combination tests
+-- docs/                     # GitHub Pages documentation (Jekyll)
+-- build/                    # Build scripts and documentation
+-- .editorconfig             # Code style rules
+-- Directory.Build.props     # Shared MSBuild properties
+-- VoloGen.snk               # Strong-name signing key
+-- VoloGen.slnx              # Solution file
```

---

## Development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- Git

### Build & Test

```bash
git clone https://github.com/OleksandrTsvirkun/VoloGen.git
cd VoloGen
dotnet tool restore
dotnet husky install
dotnet build
dotnet test
```

### Run Samples

```bash
dotnet run --project samples/VoloGen.Samples.Parsable
dotnet run --project samples/VoloGen.Samples.Formattable
dotnet run --project samples/VoloGen.Samples.Comparable
dotnet run --project samples/VoloGen.Samples.Equality
```

### Commit Policy

This project uses [Conventional Commits](https://www.conventionalcommits.org/) enforced by **Husky.NET** + **CommitLint.NET**.

**Format:** `<type>(<scope>): <description>`

**Types:** `feat`, `fix`, `refactor`, `docs`, `test`, `build`, `ci`, `chore`, `perf`, `style`

**Scopes:** `parsable`, `formattable`, `comparable`, `equality`, `common`, `ci`, `readme`, `samples`

---

## Contributing

Contributions are welcome! Please read the [Contributing Guide](CONTRIBUTING.md) before submitting a PR.

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md).

## Security

To report a vulnerability, please see our [Security Policy](SECURITY.md).

## Acknowledgements

Inspired by the excellent work in [Vogen](https://github.com/SteveDunn/Vogen) by Steve Dunn
and [StronglyTypedId](https://github.com/andrewlock/StronglyTypedId) by Andrew Lock.

## License

[MIT](LICENSE) -- Copyright (c) 2026 Oleksandr Tsvirkun


