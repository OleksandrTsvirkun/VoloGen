---
title: Home
layout: default
nav_order: 1
description: "VoloGen -- Roslyn incremental source generators that eliminate boilerplate for value-object patterns in .NET."
permalink: /
---

# VoloGen

Roslyn incremental source generators that eliminate boilerplate for value-object patterns in .NET.

[Get Started](#quick-start) \| [View on GitHub](https://github.com/OleksandrTsvirkun/VoloGen)

---

## What is VoloGen?

**VoloGen** is a collection of **Roslyn incremental source generators** that eliminate boilerplate for value-object patterns in .NET.
You implement one core method -- the generators produce all the remaining overloads, interface implementations, and operators.

Each generator ships as an **independent NuGet package** so you only pay for what you use.

## Features

- **Zero runtime dependency** -- generated code is plain C#, no base classes or helper libraries
- **Incremental generation** -- only re-generates when the annotated type changes
- **Strong-name signed** -- all assemblies are signed with `VoloGen.snk`
- **Localized diagnostics** -- error messages are translated via `.resx` resource files
- **Skip-if-exists** -- already have a custom `Equals`? The generator won't overwrite it
- **AggressiveInlining** -- thin wrapper overloads are annotated for optimal performance

## Packages

| Package | Attribute | Generates | Interfaces |
|:--------|:----------|:----------|:-----------|
| `VoloGen.Equality` | `[AutoEquality]` | `Equals`, `GetHashCode`, `==` / `!=` | `IEquatable<T>` |
| `VoloGen.Comparable` | `[AutoComparable]` | `CompareTo`, comparison & equality operators | `IComparable<T>`, `IComparable` |
| `VoloGen.Parsable` | `[AutoParsable]` | `Parse`, `TryParse` overloads | `IParsable<T>`, `ISpanParsable<T>`, opt. `IUtf8SpanParsable<T>` |
| `VoloGen.Formattable` | `[AutoFormattable]` | `ToString`, `TryFormat` overloads | `IFormattable`, `ISpanFormattable`, opt. `IUtf8SpanFormattable` |

---

## Quick Start

### 1. Install

```bash
dotnet add package VoloGen.Parsable
dotnet add package VoloGen.Parsable.Generator
```

### 2. Annotate your type

```csharp
using VoloGen;

[AutoParsable]
public partial struct Amount
{
    private readonly decimal _value;

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
var amount = Amount.Parse("42.5");
bool ok = Amount.TryParse("abc", out var bad); // false
```

interface members -- all delegating to your core `TryParse`.

---

## How It Works

Each generator follows the **core method delegation** pattern:

1. **You** implement a single core method (e.g., `TryParse`, `TryFormat`, `Equal`, `Compare`)
2. **The generator** emits all remaining overloads, interface implementations, and operators that delegate to your core method

This means you write the business logic once, and the generator handles the tedious boilerplate -- including null checks, span conversions, operator definitions, and interface compliance.

> All generated thin-wrapper overloads are annotated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]` for optimal performance.

---

## Next Steps

- **[Generators]({% link generators/index.md %})** -- detailed docs for each generator
- **[Diagnostics]({% link diagnostics.md %})** -- reference for all diagnostic codes
- **[Contributing]({% link contributing.md %})** -- how to contribute to the project
