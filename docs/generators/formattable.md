---
title: AutoFormattable
layout: default
parent: Generators
nav_order: 4
---

# AutoFormattable

Generates `IFormattable`, `ISpanFormattable`, and optionally `IUtf8SpanFormattable` from a user-defined core `TryFormat` method.

---

## Installation

```bash
dotnet add package VoloGen.Formattable
dotnet add package VoloGen.Formattable.Generator
```

## Requirements

The annotated type **must**:

- Be declared as `partial`
- Not be `static`
- Define one core method:
  - `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)` -- your formatting logic
- Provide **one** of the following for `ToString` generation:
  - `const int MaxBufferSize` -- the generator uses `stackalloc` / `ArrayPool<char>` to build the string
  - `string ToString(string?, IFormatProvider?)` -- your own conversion method

## Attribute Properties

| Property | Type | Default | Description |
|:---------|:-----|:--------|:------------|
| `ImplementUtf8` | `bool` | `false` | Generate `IUtf8SpanFormattable` overloads |
| `DefaultFormat` | `string?` | `null` | Default format string for formatless overloads |
| `AllowNullFormatProvider` | `bool` | `true` | When `false`, generated overloads throw `ArgumentNullException` for null providers |

## What Gets Generated

### Base Overloads (always generated)

| Generated Member | Delegates To |
|:-----------------|:-------------|
| `string ToString(string? format, IFormatProvider? provider)` | Via `MaxBufferSize` stackalloc or user's `ToString` |
| `override string ToString()` | `ToString(DefaultFormat, null)` |
| `bool ISpanFormattable.TryFormat(...)` | Core `TryFormat` |

### UTF-8 Overloads (`ImplementUtf8 = true`)

| Generated Member | Delegates To |
|:-----------------|:-------------|
| `bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)` | Formats to chars -> encodes to UTF-8 |

### Null Provider Guard (`AllowNullFormatProvider = false`)

When disabled, generated overloads that receive a null `IFormatProvider?` throw `ArgumentNullException` before delegating to your core method.

## Example

### With MaxBufferSize

```csharp
using VoloGen;

[AutoFormattable(DefaultFormat = "N2")]
public partial struct Currency
{
    private readonly decimal _value;

    public const int MaxBufferSize = 64;

    public Currency(decimal value)
    {
        _value = value;
    }

    public bool TryFormat(Span<char> destination, out int charsWritten,
        ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return _value.TryFormat(destination, out charsWritten, format, provider);
    }
}
```

### With Custom ToString

```csharp
using VoloGen;

[AutoFormattable]
public partial struct Percentage
{
    private readonly decimal _value;

    public bool TryFormat(Span<char> destination, out int charsWritten,
        ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return _value.TryFormat(destination, out charsWritten, format, provider);
    }

    public string ToString(string? format, IFormatProvider? provider)
    {
        return _value.ToString(format, provider) + "%";
    }
}
```

### With All Options

```csharp
using VoloGen;

[AutoFormattable(ImplementUtf8 = true, DefaultFormat = "C", AllowNullFormatProvider = false)]
public partial struct Money
{
    private readonly decimal _amount;
    public const int MaxBufferSize = 128;

    public bool TryFormat(Span<char> destination, out int charsWritten,
        ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return _amount.TryFormat(destination, out charsWritten, format, provider);
    }
}
```

### Usage

```csharp
var price = new Currency(1234.567m);

// ToString with default format
Console.WriteLine(price.ToString());            // "1,234.57" (DefaultFormat = "N2")

// ToString with explicit format
Console.WriteLine(price.ToString("C", CultureInfo.GetCultureInfo("en-US")));  // "$1,234.57"

// ISpanFormattable -- TryFormat to a span
Span<char> buffer = stackalloc char[64];
if (price.TryFormat(buffer, out int written, "F1", null))
{
    Console.WriteLine(buffer[..written]);       // "1234.6"
}

// String interpolation (leverages ISpanFormattable)
Console.WriteLine($"Total: {price:N0}");        // "Total: 1,235"
```

## Diagnostics

| ID | Trigger |
|:---|:--------|
| [VG0002]({% link diagnostics.md %}#vg0002) | Type is not `partial` |
| [VG0003]({% link diagnostics.md %}#vg0003) | Type is `static` |
| [VG0007]({% link diagnostics.md %}#vg0007) | Missing core `TryFormat` method |
| [VG0008]({% link diagnostics.md %}#vg0008) | Missing both `ToString(string?, IFormatProvider?)` and `MaxBufferSize` |
