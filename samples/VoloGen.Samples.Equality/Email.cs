using VoloGen;

namespace VoloGen.Samples.Equality;

/// <summary>
/// An email address value object with case-insensitive equality.
/// Demonstrates multi-field equality with custom comparison logic.
/// </summary>
[AutoEquality]
public partial struct Email
{
    private readonly string _localPart;
    private readonly string _domain;

    public Email(string address)
    {
        var parts = address.Split('@');
        _localPart = parts[0];
        _domain = parts[1].ToLowerInvariant();
    }

    /// <summary>
    /// Equality logic: local part is case-sensitive per RFC 5321,
    /// but domain is case-insensitive.
    /// </summary>
    public static bool Equal(Email left, Email right)
    {
        return left._localPart == right._localPart
            && string.Equals(left._domain, right._domain, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (_localPart?.GetHashCode() ?? 0) * 397
                ^ StringComparer.OrdinalIgnoreCase.GetHashCode(_domain ?? "");
        }
    }

    public override string ToString()
    {
        return _localPart + "@" + _domain;
    }
}
