using VoloGen;

namespace VoloGen.Samples.Comparable;

/// <summary>
/// A money amount value object demonstrating [AutoComparable].
/// The generator produces IComparable&lt;Amount&gt;, IComparable,
/// and all comparison / equality operators.
/// </summary>
[AutoComparable]
public partial struct Amount
{
    private readonly decimal _value;

    public Amount(decimal value)
    {
        _value = value;
    }

    /// <summary>
    /// Core comparison logic -- the generator delegates all overloads to this method.
    /// </summary>
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

    public override string ToString()
    {
        return _value.ToString("C");
    }
}
