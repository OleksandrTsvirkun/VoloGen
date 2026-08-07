using VoloGen;

namespace VoloGen.Samples.Formattable;

[AutoFormattable(DefaultFormat = "N2", MaxBufferSize = 256, MaxStackBufferSize = 256)]
public partial struct AttributeSizedAmount
{
    private readonly decimal _value;

    public AttributeSizedAmount(decimal value)
    {
        _value = value;
    }

    public bool TryFormat(Span<char> destination, out int charsWritten,
        ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return _value.TryFormat(destination, out charsWritten, format, provider);
    }
}
