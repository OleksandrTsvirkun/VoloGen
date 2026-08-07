using VoloGen;

namespace VoloGen.Samples.Formattable;

[AutoFormattable]
public partial struct LegacySizedAmount
{
    private const int MaxBufferSize = 256;
    private readonly decimal _value;

    public LegacySizedAmount(decimal value)
    {
        _value = value;
    }

    public bool TryFormat(Span<char> destination, out int charsWritten,
        ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return _value.TryFormat(destination, out charsWritten, format, provider);
    }
}
