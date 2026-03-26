using VoloGen.Samples.Comparable;

// --- AutoComparable demo ---------------------------------------------
Console.WriteLine("=== VoloGen.Comparable Sample ===");
Console.WriteLine();

// --- Simple value object: Amount ---
Console.WriteLine("-- Amount (single-field) --");
var a = new Amount(10.00m);
var b = new Amount(25.50m);
var c = new Amount(10.00m);

Console.WriteLine($"a < b  : {a < b}");    // True
Console.WriteLine($"a > b  : {a > b}");    // False
Console.WriteLine($"a <= c : {a <= c}");   // True
Console.WriteLine($"a >= c : {a >= c}");   // True
Console.WriteLine($"a == c : {a == c}");   // True
Console.WriteLine($"a != b : {a != b}");   // True
Console.WriteLine($"a.CompareTo(b) : {a.CompareTo(b)}"); // Negative
Console.WriteLine();

var amounts = new[] { new Amount(50m), new Amount(10m), new Amount(30m) };
Array.Sort(amounts);
Console.WriteLine("Sorted: " + string.Join(", ", amounts.Select(x => x.ToString())));
Console.WriteLine();

// --- Multi-field value object: DateRange ---
Console.WriteLine("-- DateRange (multi-field comparison) --");
var q1 = new DateRange(new DateOnly(2025, 1, 1), new DateOnly(2025, 3, 31));
var q2 = new DateRange(new DateOnly(2025, 4, 1), new DateOnly(2025, 6, 30));
var shortJan = new DateRange(new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));

Console.WriteLine($"q1 < q2       : {q1 < q2}");         // True (earlier start)
Console.WriteLine($"shortJan < q1 : {shortJan < q1}");   // True (same start, earlier end)
Console.WriteLine($"q1 == q1      : {q1 == q1}");         // True
Console.WriteLine();

var ranges = new[] { q2, q1, shortJan };
Array.Sort(ranges);
Console.WriteLine("Sorted ranges:");
foreach (var r in ranges)
{
    Console.WriteLine($"  {r}");
}

Console.WriteLine();
Console.WriteLine("Done.");
