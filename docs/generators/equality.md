---
title: AutoEquality
permalink: /generators/equality/
---

# AutoEquality

Generates `IEquatable<T>`, `Equals(object?)`, `GetHashCode()`, and `==` / `!=` operators from a user-defined core equality method.

---

## Installation

```bash
dotnet add package VoloGen.Equality
dotnet add package VoloGen.Equality.Generator
```

## Requirements

The annotated type **must**:

- Be declared as `partial`
- Not be `static`
- Define two core methods:
  - `static bool Equal(T left, T right)` -- your equality logic
  - `override int GetHashCode()` -- your hash code computation

## What Gets Generated

| Generated Member | Delegates To |
|:-----------------|:-------------|
| `bool Equals(T other)` | `Equal(this, other)` |
| `override bool Equals(object? obj)` | `Equal(this, (T)obj)` |
| `operator ==(T, T)` | `Equal(left, right)` |
| `operator !=(T, T)` | `!Equal(left, right)` |

> If `[AutoComparable]` is also present on the same type, the `==` and `!=` operators are emitted by the comparable generator instead to avoid duplicate declarations.

## Example

### Struct

```csharp
using VoloGen;

[AutoEquality]
public partial struct UserId
{
    private readonly int _value;

    public UserId(int value)
    {
        _value = value;
    }

    public static bool Equal(UserId left, UserId right)
    {
        return left._value == right._value;
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }
}
```

### Class

```csharp
using VoloGen;

[AutoEquality]
public partial class Product
{
    private readonly string _name;
    private readonly decimal _price;

    public static bool Equal(Product? left, Product? right)
    {
        return left._name == right._name && left._price == right._price;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return _name.GetHashCode() ^ _price.GetHashCode();
        }
    }
}
```

### Usage

```csharp
var id1 = new UserId(42);
var id2 = new UserId(42);
var id3 = new UserId(99);

Console.WriteLine(id1 == id2);          // True
Console.WriteLine(id1 != id3);          // True
Console.WriteLine(id1.Equals(id2));     // True
Console.WriteLine(id1.GetHashCode());   // consistent with Equal
```

## Diagnostics

| ID | Trigger |
|:---|:--------|
| [VG0002]({% link diagnostics.md %}#vg0002) | Type is not `partial` |
| [VG0003]({% link diagnostics.md %}#vg0003) | Type is `static` |
| [VG0006]({% link diagnostics.md %}#vg0006) | Missing `Equal` and/or `GetHashCode` |
