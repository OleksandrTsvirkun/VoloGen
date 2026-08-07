using VoloGen;

namespace VoloGen.Samples.Formattable;

[AutoFormattable(AllowNullFormatProvider = false, MaxBufferSize = 256)]
public partial struct StrictAmount
{
    private readonly decimal _value;

    public StrictAmount(decimal value)
    {
        _value = value;
    }

    public bool TryFormat(Span<char> destination, out int charsWritten,
        ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        return _value.TryFormat(destination, out charsWritten, format, provider);
    }
}
