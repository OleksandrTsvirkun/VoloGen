using System.Globalization;
using VoloGen;

namespace VoloGen.Samples.Formattable;

/// <summary>
/// A currency value object demonstrating [AutoFormattable].
/// The generator produces IFormattable, ISpanFormattable, ToString overloads,
/// and TryFormat convenience overloads.
/// </summary>
[AutoFormattable(DefaultFormat = "N2")]
public partial struct Currency
{
    private readonly decimal _value;

    /// <summary>
    /// Maximum buffer size for stackalloc-based ToString generation.
    /// </summary>
    public const int MaxBufferSize = 64;

    public Currency(decimal value)
    {
        _value = value;
    }

    /// <summary>
    /// Core TryFormat logic -- the generator delegates all overloads to this method.
    /// </summary>
    public bool TryFormat(Span<char> destination, out int charsWritten,
        ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return _value.TryFormat(destination, out charsWritten, format, provider ?? CultureInfo.InvariantCulture);
    }
}
