---
title: Samples
permalink: /samples/
---

The repository includes runnable sample projects under `samples/` that demonstrate each generator in action.

---

## Running the Samples

```bash
git clone https://github.com/OleksandrTsvirkun/VoloGen.git
cd VoloGen
dotnet build

# Run any sample
dotnet run --project samples/VoloGen.Samples.Equality
dotnet run --project samples/VoloGen.Samples.Comparable
dotnet run --project samples/VoloGen.Samples.Parsable
dotnet run --project samples/VoloGen.Samples.Formattable
```

---

## Equality Sample

**Project:** `samples/VoloGen.Samples.Equality/`

A `UserId` value object with `[AutoEquality]` that generates `IEquatable<UserId>`, `Equals(object?)`, and `==`/`!=` operators.

```csharp
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

**Output:**

```
id1 == id2: True
id1 != id3: True
id1.Equals(id2): True
id1.GetHashCode() == id2.GetHashCode(): True
```

---

## Comparable Sample

**Project:** `samples/VoloGen.Samples.Comparable/`

An `Amount` value object with `[AutoComparable]` that generates `IComparable<Amount>`, `IComparable`, and all comparison operators.

```csharp
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

**Output:**

```
a < b: True
c > b: True
a <= a: True
a == a: True
Sorted: $10.00, $30.00, $50.00
```

---

## Parsable Sample

**Project:** `samples/VoloGen.Samples.Parsable/`

A `Temperature` value object with `[AutoParsable]` that generates `IParsable<Temperature>`, `ISpanParsable<Temperature>`, and all `Parse`/`TryParse` overloads.

```csharp
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

**Output:**

```
Parsed: 36.6 deg C
Boiling: 100.0 deg C
'not-a-number' failed to parse as expected.
```

---

## Formattable Sample

**Project:** `samples/VoloGen.Samples.Formattable/`

A `Currency` value object with `[AutoFormattable]` that generates `IFormattable`, `ISpanFormattable`, and all `ToString`/`TryFormat` overloads.

```csharp
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
        return _value.TryFormat(destination, out charsWritten, format,
            provider ?? System.Globalization.CultureInfo.InvariantCulture);
    }
}
```

**Output:**

```
Default: 1,234.57
Format C: $1,234.57
TryFormat: 1234.567
Interpolated: 1,235
```
