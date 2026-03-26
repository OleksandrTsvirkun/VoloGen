using VoloGen.Samples.Parsable;

// --- AutoParsable demo -----------------------------------------------
Console.WriteLine("=== VoloGen.Parsable Sample ===");
Console.WriteLine();

// --- Simple: Temperature ---
Console.WriteLine("-- Temperature (numeric parsing) --");
var temp = Temperature.Parse("36.6");
Console.WriteLine($"Parsed: {temp}");   // 36.6 deg C

var span = "100.0".AsSpan();
var boiling = Temperature.Parse(span, null);
Console.WriteLine($"Boiling: {boiling}");  // 100.0 deg C

if (Temperature.TryParse("-40", out var cold))
{
    Console.WriteLine($"Cold: {cold}");  // -40.0 deg C
}

if (!Temperature.TryParse("not-a-number", out _))
{
    Console.WriteLine("'not-a-number' failed to parse (expected)");
}
Console.WriteLine();

// --- Custom parsing: HexColor ---
Console.WriteLine("-- HexColor (custom format parsing) --");
var red = HexColor.Parse("#FF0000");
Console.WriteLine($"Red    : {red}");           // #FF0000

var green = HexColor.Parse("00FF00");
Console.WriteLine($"Green  : {green}");         // #00FF00

var white = HexColor.Parse("#FFF");
Console.WriteLine($"White  : {white}");         // #FFFFFF

Console.WriteLine($"R={white.R}, G={white.G}, B={white.B}");  // R=255, G=255, B=255

if (!HexColor.TryParse("not-a-color", out _))
{
    Console.WriteLine("'not-a-color' failed to parse (expected)");
}

Console.WriteLine();
Console.WriteLine("Done.");
