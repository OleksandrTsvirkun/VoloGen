---
title: Design Principles
permalink: /design/
---

This page explains the architectural decisions behind VoloGen's source generators.

---

## Core Method Delegation

Every generator follows the same fundamental pattern:

1. **The user** implements a single core method that contains the business logic
2. **The generator** emits all remaining overloads that delegate to that core method

This design ensures:

- **Single source of truth** -- the logic lives in one place
- **Complete interface compliance** -- all interface members are generated correctly
- **Minimal user effort** -- you write the interesting part, the generator handles the rest

### Core Methods by Generator

| Generator | Core Method |
|:----------|:------------|
| `[AutoEquality]` | `static bool Equal(T, T)` + `override int GetHashCode()` |
| `[AutoComparable]` | `static int Compare(T, T)` |
| `[AutoParsable]` | `static bool TryParse(ReadOnlySpan<char>, IFormatProvider?, out T)` |
| `[AutoFormattable]` | `bool TryFormat(Span<char>, out int, ReadOnlySpan<char>, IFormatProvider?)` |

---

## Skip-If-Exists

Before emitting any method, each generator checks whether an identical overload already exists on the target type. The check matches:

- Method name
- Parameter types and ref kinds
- Return type

If a match is found, the generator silently skips that method. This means you can always override any generated method by defining your own implementation.

> This design makes VoloGen safe to adopt incrementally. You can start with full generation and later customize individual methods without breaking anything.

---

## Operator Coordination

When both `[AutoEquality]` and `[AutoComparable]` are applied to the same type, there's a potential conflict: both generators would want to emit `==` and `!=` operators.

VoloGen resolves this automatically:

- `[AutoComparable]` **always** emits `==` and `!=` (delegating to `Compare`)
- `[AutoEquality]` **skips** `==` and `!=` when it detects `[AutoComparable]` is present

This coordination requires no user configuration.

---

## AggressiveInlining

All generated thin-wrapper overloads -- methods that do nothing more than delegate to another method with minor parameter adaptation -- are annotated with:

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
```

Core delegation methods and heavier logic (e.g., `Parse` with throw) are **not** inlined, allowing the JIT compiler to make its own decisions.

---

## Incremental Generation

VoloGen uses the [Roslyn Incremental Source Generator](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md) API (`IIncrementalGenerator`). This means:

- The generator pipeline is declared as a series of transforms
- Roslyn caches intermediate results and only re-runs transforms when inputs change
- Editing unrelated files does **not** trigger regeneration

The typical pipeline:

```
ForAttributeWithMetadataName -> GetTargetInfo -> Where/Select -> RegisterSourceOutput
```

Each step produces an immutable value that Roslyn can compare efficiently.

---

## Package Architecture

Each generator is split into two NuGet packages:

| Package Type | Target | Purpose |
|:-------------|:-------|:--------|
| **Attribute** (`VoloGen.Parsable`) | `netstandard2.0` | Marker attribute consumed by user code |
| **Generator** (`VoloGen.Parsable.Generator`) | `netstandard2.0` | Source generator loaded by the compiler |

This split ensures:

- **Attribute packages** have zero dependencies and maximum compatibility
- **Generator packages** are compile-time only -- they don't ship with your application
- **Users can reference attribute packages** from any .NET version

A shared `VoloGen.Common` project provides diagnostics, localized messages, and helper utilities used by all generators.

---

## Localized Diagnostics

All diagnostic messages are stored in `.resx` resource files within `VoloGen.Common`:

- `Diagnostics.resx` -- English (default)
- `Diagnostics.de.resx` -- German
- `Diagnostics.es.resx` -- Spanish
- `Diagnostics.fr.resx` -- French
- `Diagnostics.ja.resx` -- Japanese
- `Diagnostics.ko.resx` -- Korean
- `Diagnostics.pt-BR.resx` -- Portuguese (Brazil)
- `Diagnostics.uk.resx` -- Ukrainian
- `Diagnostics.zh-Hans.resx` -- Chinese (Simplified)

Diagnostic descriptors are centralized in `DiagnosticDescriptors.cs` and reference the resource manager for message resolution. This ensures developers see errors in their IDE's language.

---

## Feature Flags

Some generators support attribute properties that enable optional features:

| Flag | Generator | Effect |
|:-----|:----------|:-------|
| `ImplementUtf8` | Parsable, Formattable | Emits UTF-8 interface overloads |
| `ImplementExact` | Parsable | Emits `ParseExact` / `TryParseExact` overloads |
| `DefaultFormat` | Formattable | Sets default format string for formatless overloads |
| `AllowNullFormatProvider` | Formattable | Controls null-provider guard generation |

Feature flags are read from the attribute's named arguments during the `GetTargetInfo` transform phase and stored in the immutable `TargetInfo` record for downstream processing.
