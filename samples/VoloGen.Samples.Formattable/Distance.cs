using VoloGen;

namespace VoloGen.Samples.Formattable;

[AutoFormattable]
public partial struct Distance
{
    private readonly decimal _value;

    public Distance(decimal value)
    {
        _value = value;
    }

    public bool TryFormat(Span<char> destination, out int charsWritten,
        ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return _value.TryFormat(destination, out charsWritten, format, provider);
    }

    public string ToString(string? format, IFormatProvider? provider)
    {
        return $"{_value.ToString(format, provider)} km";
    }
}
