using VoloGen;

namespace VoloGen.Samples.Parsable;

/// <summary>
/// An RGB hex color value object demonstrating custom parsing logic.
/// Accepts formats: "#RRGGBB", "RRGGBB", "#RGB".
/// </summary>
[AutoParsable]
public partial struct HexColor
{
    private readonly byte _r;
    private readonly byte _g;
    private readonly byte _b;

    public HexColor(byte r, byte g, byte b)
    {
        _r = r;
        _g = g;
        _b = b;
    }

    public byte R
    {
        get { return _r; }
    }

    public byte G
    {
        get { return _g; }
    }

    public byte B
    {
        get { return _b; }
    }

    /// <summary>
    /// Core TryParse: accepts "#RRGGBB", "RRGGBB", or "#RGB" shorthand.
    /// </summary>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out HexColor result)
    {
        result = default;

        if (s.Length > 0 && s[0] == '#')
        {
            s = s.Slice(1);
        }

        if (s.Length == 6)
        {
            if (byte.TryParse(s.Slice(0, 2), System.Globalization.NumberStyles.HexNumber, provider, out byte r)
                && byte.TryParse(s.Slice(2, 2), System.Globalization.NumberStyles.HexNumber, provider, out byte g)
                && byte.TryParse(s.Slice(4, 2), System.Globalization.NumberStyles.HexNumber, provider, out byte b))
            {
                result = new HexColor(r, g, b);
                return true;
            }
        }
        else if (s.Length == 3)
        {
            if (byte.TryParse(s.Slice(0, 1), System.Globalization.NumberStyles.HexNumber, provider, out byte r)
                && byte.TryParse(s.Slice(1, 1), System.Globalization.NumberStyles.HexNumber, provider, out byte g)
                && byte.TryParse(s.Slice(2, 1), System.Globalization.NumberStyles.HexNumber, provider, out byte b))
            {
                result = new HexColor((byte)(r * 17), (byte)(g * 17), (byte)(b * 17));
                return true;
            }
        }

        return false;
    }

    public override string ToString()
    {
        return $"#{_r:X2}{_g:X2}{_b:X2}";
    }
}
