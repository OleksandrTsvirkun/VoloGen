using System.Globalization;
using VoloGen;

namespace VoloGen.Samples.Formattable;

/// <summary>
/// A percentage value object demonstrating [AutoFormattable] with a custom
/// ToString method instead of MaxBufferSize.
/// </summary>
[AutoFormattable(DefaultFormat = "F1")]
public partial struct Percentage
{
    private readonly decimal _value;

    public Percentage(decimal value)
    {
        _value = value;
    }

    /// <summary>
    /// Core TryFormat logic -- formats the numeric value then appends '%'.
    /// </summary>
    public bool TryFormat(Span<char> destination, out int charsWritten,
        ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        charsWritten = 0;

        if (!_value.TryFormat(destination, out int numWritten, format, provider ?? CultureInfo.InvariantCulture))
        {
            return false;
        }

        if (numWritten + 1 > destination.Length)
        {
            charsWritten = 0;
            return false;
        }

        destination[numWritten] = '%';
        charsWritten = numWritten + 1;
        return true;
    }

    /// <summary>
    /// Custom ToString used by the generator instead of MaxBufferSize.
    /// </summary>
    public string ToString(string? format, IFormatProvider? provider)
    {
        return _value.ToString(format, provider ?? CultureInfo.InvariantCulture) + "%";
    }
}
