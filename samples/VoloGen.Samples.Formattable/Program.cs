using System.Globalization;
using System.Text;

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
// --- Attribute-configured buffer ---
Console.WriteLine("-- Attribute-configured buffer --");
var attributeSized = new AttributeSizedAmount(1234.567m);
Console.WriteLine($"Default  : {attributeSized}");
Console.WriteLine($"Format F1: {FormatToSpan(attributeSized, "F1")}");
Console.WriteLine();

// --- Legacy constant fallback ---
Console.WriteLine("-- Legacy constant fallback --");
var legacySized = new LegacySizedAmount(42.5m);
Console.WriteLine($"Format C : {legacySized.ToString("C", CultureInfo.GetCultureInfo("en-US"))}");
Console.WriteLine();

// --- Custom stack buffer threshold ---
Console.WriteLine("-- Custom stack buffer threshold --");
var customThreshold = new CustomThresholdAmount(9876.543m);
Console.WriteLine($"Format N2: {customThreshold.ToString("N2", CultureInfo.InvariantCulture)}");
Console.WriteLine();

// --- Class with attribute buffer ---
Console.WriteLine("-- Class with attribute buffer --");
var customerCode = new CustomerCode("CUS-2026-001");
Console.WriteLine($"Code     : {customerCode}");
Console.WriteLine();

// --- Custom ToString implementation ---
Console.WriteLine("-- Custom ToString implementation --");
var distance = new Distance(12.5m);
Console.WriteLine($"Distance : {distance.ToString("F1", CultureInfo.InvariantCulture)}");
Console.WriteLine();

// --- UTF-8 formatting ---
Console.WriteLine("-- UTF-8 formatting --");
var utf8Amount = new Utf8Amount(987.65m);
Span<byte> utf8Destination = stackalloc byte[64];
if (utf8Amount.TryFormat(utf8Destination, out int bytesWritten, "N2".AsSpan(), CultureInfo.InvariantCulture))
{
    Console.WriteLine($"UTF-8    : {Encoding.UTF8.GetString(utf8Destination[..bytesWritten])}");
}

// --- Non-null format provider ---
Console.WriteLine("-- Non-null format provider --");
var strictAmount = new StrictAmount(42.5m);
Console.WriteLine($"Strict   : {strictAmount.ToString(CultureInfo.InvariantCulture)}");

Console.WriteLine();
Console.WriteLine("Done.");

static string FormatToSpan<T>(T value, ReadOnlySpan<char> format)
    where T : ISpanFormattable
{
    Span<char> destination = stackalloc char[64];
    if (!value.TryFormat(destination, out int charsWritten, format, CultureInfo.InvariantCulture))
    {
        return string.Empty;
    }

    return destination[..charsWritten].ToString();
}
