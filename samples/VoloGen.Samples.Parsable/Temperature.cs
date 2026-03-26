using VoloGen;

namespace VoloGen.Samples.Parsable;

/// <summary>
/// A temperature value object demonstrating [AutoParsable].
/// The generator produces Parse, TryParse, and all IParsable/ISpanParsable overloads.
/// </summary>
[AutoParsable]
public partial struct Temperature
{
    private readonly double _celsius;

    public Temperature(double celsius)
    {
        _celsius = celsius;
    }

    public double Celsius
    {
        get { return _celsius; }
    }

    /// <summary>
    /// Core TryParse logic -- the generator delegates all overloads to this method.
    /// </summary>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Temperature result)
    {
        if (double.TryParse(s, provider, out double value))
        {
            result = new Temperature(value);
            return true;
        }

        result = default;
        return false;
    }

    public override string ToString()
    {
        return $"{_celsius:F1} deg C";
    }
}
