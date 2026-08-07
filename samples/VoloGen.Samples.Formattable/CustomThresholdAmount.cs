using VoloGen;

namespace VoloGen.Samples.Formattable;

[AutoFormattable(MaxBufferSize = 512, MaxStackBufferSize = 128)]
public partial struct CustomThresholdAmount
{
    private readonly decimal _value;

    public CustomThresholdAmount(decimal value)
    {
        _value = value;
    }

    public bool TryFormat(Span<char> destination, out int charsWritten,
        ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return _value.TryFormat(destination, out charsWritten, format, provider);
    }
}
