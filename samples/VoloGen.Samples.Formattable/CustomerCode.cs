using VoloGen;

namespace VoloGen.Samples.Formattable;

[AutoFormattable(MaxBufferSize = 64)]
public partial class CustomerCode
{
    private readonly string _value;

    public CustomerCode(string value)
    {
        _value = value;
    }

    public bool TryFormat(Span<char> destination, out int charsWritten,
        ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        ReadOnlySpan<char> value = _value.AsSpan();
        if (value.TryCopyTo(destination))
        {
            charsWritten = value.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }
}
