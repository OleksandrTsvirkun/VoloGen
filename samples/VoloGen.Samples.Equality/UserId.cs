using VoloGen;

namespace VoloGen.Samples.Equality;

/// <summary>
/// A simple user ID value object demonstrating [AutoEquality].
/// The generator produces IEquatable&lt;UserId&gt;, Equals(object), and == / != operators.
/// </summary>
[AutoEquality]
public partial struct UserId
{
    private readonly int _value;

    public UserId(int value)
    {
        _value = value;
    }

    /// <summary>
    /// Core equality logic -- the generator delegates all overloads to this method.
    /// </summary>
    public static bool Equal(UserId left, UserId right)
    {
        return left._value == right._value;
    }

    /// <summary>
    /// Hash code computation required by [AutoEquality].
    /// </summary>
    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }
}
