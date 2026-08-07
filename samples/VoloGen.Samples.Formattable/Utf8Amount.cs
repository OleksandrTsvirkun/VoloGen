using VoloGen;

namespace VoloGen.Samples.Formattable;

[AutoFormattable(ImplementUtf8 = true, DefaultFormat = "N2", MaxBufferSize = 256)]
public partial struct Utf8Amount
{
    private readonly decimal _value;

    public Utf8Amount(decimal value)
    {
        _value = value;
    }

    public bool TryFormat(Span<char> destination, out int charsWritten,
        ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return _value.TryFormat(destination, out charsWritten, format, provider);
    }
}
