using System.Globalization;
using VoloGen.Samples.Formattable;

// --- AutoFormattable demo --------------------------------------------
Console.WriteLine("=== VoloGen.Formattable Sample ===");
Console.WriteLine();

// --- Currency (MaxBufferSize approach) ---
Console.WriteLine("-- Currency (MaxBufferSize + DefaultFormat) --");
var price = new Currency(1234.567m);

// ToString() -- uses DefaultFormat "N2"
Console.WriteLine($"Default  : {price}");

// ToString(format)
Console.WriteLine($"Format C : {price.ToString("C", CultureInfo.GetCultureInfo("en-US"))}");
Console.WriteLine($"Format F4: {price.ToString("F4")}");

// TryFormat to span
Span<char> buffer = stackalloc char[64];
if (price.TryFormat(buffer, out int written))
{
    Console.WriteLine($"Span     : {buffer[..written]}");
}

// IFormattable via string interpolation
FormattableString fs = $"Interpolated: {price:N0}";
Console.WriteLine(fs.ToString(CultureInfo.InvariantCulture));
Console.WriteLine();

// --- Percentage (custom ToString approach) ---
Console.WriteLine("-- Percentage (custom ToString + DefaultFormat) --");
var rate = new Percentage(85.75m);

Console.WriteLine($"Default  : {rate}");           // 85.8% (DefaultFormat = "F1")
Console.WriteLine($"Format F3: {rate.ToString("F3")}");  // 85.750%

Span<char> pctBuffer = stackalloc char[32];
if (rate.TryFormat(pctBuffer, out int pctWritten, "N0", CultureInfo.InvariantCulture))
{
    Console.WriteLine($"Span     : {pctBuffer[..pctWritten]}");  // 86%
}

Console.WriteLine();
Console.WriteLine("Done.");
