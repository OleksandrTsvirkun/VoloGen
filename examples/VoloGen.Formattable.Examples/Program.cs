using System.Globalization;
using System.Text;

using VoloGen;

#pragma warning disable S3903, S1144

Console.WriteLine("Attribute-configured buffer:");
var attributeSized = new AttributeSizedAmount(1234.567m);
Console.WriteLine(attributeSized.ToString());
Console.WriteLine(FormatToSpan(attributeSized, "F1"));

Console.WriteLine();
Console.WriteLine("Legacy constant fallback:");
var legacySized = new LegacySizedAmount(42.5m);
Console.WriteLine(legacySized.ToString("C", CultureInfo.GetCultureInfo("en-US")));

Console.WriteLine();
Console.WriteLine("Custom stack buffer threshold:");
var customThreshold = new CustomThresholdAmount(9876.543m);
Console.WriteLine(customThreshold.ToString("N2", CultureInfo.InvariantCulture));

Console.WriteLine();
Console.WriteLine("Class with attribute buffer:");
var customerCode = new CustomerCode("CUS-2026-001");
Console.WriteLine(customerCode);

Console.WriteLine();
Console.WriteLine("Custom ToString implementation:");
var distance = new Distance(12.5m);
Console.WriteLine(distance.ToString("F1", CultureInfo.InvariantCulture));

Console.WriteLine();
Console.WriteLine("UTF-8 formatting:");
var utf8Amount = new Utf8Amount(987.65m);
Span<byte> utf8Destination = stackalloc byte[64];
if (utf8Amount.TryFormat(utf8Destination, out int bytesWritten, "N2", CultureInfo.InvariantCulture))
{
    Console.WriteLine(Encoding.UTF8.GetString(utf8Destination[..bytesWritten]));
}

Console.WriteLine();
Console.WriteLine("Non-null format provider:");
var strictAmount = new StrictAmount(42.5m);
Console.WriteLine(strictAmount.ToString(CultureInfo.InvariantCulture));

static string FormatToSpan<T>(T value, ReadOnlySpan<char> format)
    where T : ISpanFormattable
{
    Span<char> destination = stackalloc char[64];
    return value.TryFormat(destination, out int charsWritten, format, CultureInfo.InvariantCulture)
        ? destination[..charsWritten].ToString()
        : string.Empty;
}

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
