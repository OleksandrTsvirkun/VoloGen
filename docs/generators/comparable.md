---
title: AutoComparable
permalink: /generators/comparable/
---

Generates `IComparable<T>`

---

## Installation

```bash
dotnet add package VoloGen.Comparable
dotnet add package VoloGen.Comparable.Generator
```

## Requirements

The annotated type **must**:

- Be declared as `partial`
- Not be `static`
- Define one core method:
  - `static int Compare(T left, T right)` -- your comparison logic

## What Gets Generated

| Generated Member | Delegates To |
|:-----------------|:-------------|
| `int CompareTo(T other)` | `Compare(this, other)` |
| `int IComparable.CompareTo(object? obj)` | `Compare(this, (T)obj)` with type check |
| `operator <(T, T)` | `Compare(left, right) < 0` |
| `operator >(T, T)` | `Compare(left, right) > 0` |
| `operator <=(T, T)` | `Compare(left, right) <= 0` |
| `operator >=(T, T)` | `Compare(left, right) >= 0` |
| `operator ==(T, T)` | `Compare(left, right) == 0` |
| `operator !=(T, T)` | `Compare(left, right) != 0` |

> When `[AutoEquality]` is also present, this generator takes ownership of `==` and `!=` so they delegate to `Compare` rather than `Equal`. This avoids duplicate operator declarations.

## Example

### Struct

```csharp
using VoloGen;

[AutoComparable]
public partial struct Amount
{
    private readonly decimal _value;

    public Amount(decimal value)
    {
        _value = value;
    }

    public static int Compare(Amount left, Amount right)
    {
        return left._value.CompareTo(right._value);
    }

    public override bool Equals(object? obj)
    {
        return obj is Amount other && _value == other._value;
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }
}
```

### Multi-Field Comparison

```csharp
using VoloGen;

[AutoComparable]
public partial struct Coordinate
{
    private readonly int _x;
    private readonly int _y;

    public static int Compare(Coordinate left, Coordinate right)
    {
        var xCompare = left._x.CompareTo(right._x);
        if (xCompare != 0)
        {
            return xCompare;
        }
        return left._y.CompareTo(right._y);
    }
}
```

### Usage

```csharp
var a = new Amount(10.00m);
var b = new Amount(30.00m);
var c = new Amount(50.00m);

Console.WriteLine(a < b);   // True
Console.WriteLine(c > b);   // True
Console.WriteLine(a <= a);  // True
Console.WriteLine(a == a);  // True

// IComparable<T> -- works with sorting
var amounts = new[] { c, a, b };
Array.Sort(amounts);
// amounts: 10, 30, 50
```

## Diagnostics

| ID | Trigger |
|:---|:--------|
| [VG0002]({% link diagnostics.md %}#vg0002) | Type is not `partial` |
| [VG0003]({% link diagnostics.md %}#vg0003) | Type is `static` |
| [VG0005]({% link diagnostics.md %}#vg0005) | Missing `Compare` method |
