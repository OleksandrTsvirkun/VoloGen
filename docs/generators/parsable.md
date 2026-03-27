---
title: AutoParsable
permalink: /generators/parsable/
---

Generates `IParsable<T>`

---

## Installation

```bash
dotnet add package VoloGen.Parsable
dotnet add package VoloGen.Parsable.Generator
```

## Requirements

The annotated type **must**:

- Be declared as `partial`
- Not be `static` or `abstract`
- Define one core method:
  - `static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out T result)` -- your parsing logic

## Attribute Properties

| Property | Type | Default | Description |
|:---------|:-----|:--------|:------------|
| `ImplementUtf8` | `bool` | `false` | Generate `IUtf8SpanParsable<T>` overloads |
| `ImplementExact` | `bool` | `false` | Generate `ParseExact` / `TryParseExact` overloads |

## What Gets Generated

### Base Overloads (always generated)

| Generated Member | Delegates To |
|:-----------------|:-------------|
| `static T Parse(string s, IFormatProvider? provider)` | `TryParse` -> throw on failure |
| `static T Parse(ReadOnlySpan<char> s, IFormatProvider? provider)` | `TryParse` -> throw on failure |
| `static T Parse(string s)` | `TryParse(s, null, ...)` |
| `static T Parse(ReadOnlySpan<char> s)` | `TryParse(s, null, ...)` |
| `static bool TryParse(string? s, IFormatProvider? provider, out T result)` | Core `TryParse` via `AsSpan()` |
| `static bool TryParse(string? s, out T result)` | `TryParse(s, null, ...)` |
| `static bool TryParse(ReadOnlySpan<char> s, out T result)` | `TryParse(s, null, ...)` |

### UTF-8 Overloads (`ImplementUtf8 = true`)

| Generated Member | Delegates To |
|:-----------------|:-------------|
| `static T Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider)` | Decodes UTF-8 -> `TryParse` |
| `static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, out T result)` | Decodes UTF-8 -> `TryParse` |

### Exact Overloads (`ImplementExact = true`)

When `ImplementExact` is enabled, you must also provide:

```csharp
public static bool TryParseExact(ReadOnlySpan<char> s, ReadOnlySpan<char> format,
    IFormatProvider? provider, out T result)
```

| Generated Member | Delegates To |
|:-----------------|:-------------|
| `static T ParseExact(string s, string format, IFormatProvider? provider)` | `TryParseExact` -> throw on failure |
| `static T ParseExact(ReadOnlySpan<char> s, ReadOnlySpan<char> format, IFormatProvider? provider)` | `TryParseExact` -> throw on failure |
| `static bool TryParseExact(string? s, string? format, IFormatProvider? provider, out T result)` | Core `TryParseExact` |
| Providerless and spanless convenience overloads | Delegate with `null` defaults |

## Example

### Basic

```csharp
using VoloGen;

[AutoParsable]
public partial struct Temperature
{
    private readonly double _celsius;

    public Temperature(double celsius)
    {
        _celsius = celsius;
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Temperature result)
    {
        if (double.TryParse(s, provider, out double value))
        {
            result = new Temperature(value);
            return true;
        }
        result = default;
        return false;
    }
}
```

### With UTF-8 Support

```csharp
[AutoParsable(ImplementUtf8 = true)]
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

    public static bool TryParse(ReadOnlySpan<byte> s, IFormatProvider? provider, out Amount result)
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

### With Exact Parsing

```csharp
[AutoParsable(ImplementExact = true)]
public partial struct Date
{
    private readonly DateOnly _value;

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Date result)
    {
        if (DateOnly.TryParse(s, provider, out var value))
        {
            result = new Date { _value = value };
            return true;
        }
        result = default;
        return false;
    }

    public static bool TryParseExact(ReadOnlySpan<char> s, ReadOnlySpan<char> format,
        IFormatProvider? provider, out Date result)
    {
        if (DateOnly.TryParseExact(s, format, provider, System.Globalization.DateTimeStyles.None, out var value))
        {
            result = new Date { _value = value };
            return true;
        }
        result = default;
        return false;
    }
}
```

### Usage

```csharp
// Parse from string
var temp = Temperature.Parse("36.6");

// TryParse with error handling
if (Temperature.TryParse("not-a-number", out var bad))
{
    Console.WriteLine(bad);
}
else
{
    Console.WriteLine("Failed to parse"); // this branch
}

// Parse with provider
var amount = Amount.Parse("1,234.56", CultureInfo.InvariantCulture);

// UTF-8 parsing (when ImplementUtf8 = true)
var utf8 = "42.5"u8;
var parsed = Amount.Parse(utf8, null);
```

## Diagnostics

| ID | Trigger |
|:---|:--------|
| [VG0001]({% link diagnostics.md %}#vg0001) | Missing core `TryParse` method |
| [VG0002]({% link diagnostics.md %}#vg0002) | Type is not `partial` |
| [VG0003]({% link diagnostics.md %}#vg0003) | Type is `static` |
| [VG0004]({% link diagnostics.md %}#vg0004) | Type is `abstract` |
