using VoloGen.Samples.Equality;

// --- AutoEquality demo -----------------------------------------------
Console.WriteLine("=== VoloGen.Equality Sample ===");
Console.WriteLine();

// --- Simple value object: UserId ---
Console.WriteLine("-- UserId (single-field) --");
var id1 = new UserId(42);
var id2 = new UserId(42);
var id3 = new UserId(99);

Console.WriteLine($"id1 == id2 : {id1 == id2}");   // True
Console.WriteLine($"id1 != id3 : {id1 != id3}");   // True
Console.WriteLine($"id1.Equals(id2) : {id1.Equals(id2)}");   // True
Console.WriteLine($"id1.Equals((object)id3) : {id1.Equals((object)id3)}"); // False
Console.WriteLine($"id1.GetHashCode() == id2.GetHashCode() : {id1.GetHashCode() == id2.GetHashCode()}"); // True
Console.WriteLine();

// --- Multi-field value object: Email ---
Console.WriteLine("-- Email (multi-field, case-insensitive domain) --");
var email1 = new Email("user@Example.COM");
var email2 = new Email("user@example.com");
var email3 = new Email("other@example.com");

Console.WriteLine($"email1 == email2 : {email1 == email2}");  // True (domain case-insensitive)
Console.WriteLine($"email1 != email3 : {email1 != email3}");  // True
Console.WriteLine($"email1.Equals(email2) : {email1.Equals(email2)}");  // True
Console.WriteLine($"email1.Address : {email1.Address}");  // user@example.com
Console.WriteLine();

// --- HashSet deduplication ---
Console.WriteLine("-- HashSet deduplication --");
var set = new HashSet<UserId> { id1, id2, id3 };
Console.WriteLine($"Added 3, set contains: {set.Count}");  // 2

Console.WriteLine();
Console.WriteLine("Done.");
