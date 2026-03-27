using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

using VoloGen.Comparable;
using VoloGen.Equality;
using VoloGen.Formattable;
using VoloGen.Parsable;

using Xunit;

namespace VoloGen.Combinations.Tests;

public class CombinationTests
{
    private static async Task<(Compilation OutputCompilation, GeneratorDriverRunResult RunResult)> RunGeneratorsAsync(
        string source,
        params IIncrementalGenerator[] generators)
    {
        var referenceAssemblies = await ReferenceAssemblies.Net.Net90.ResolveAsync(
            LanguageNames.CSharp, CancellationToken.None);

        var references = referenceAssemblies
            .Add(MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(AutoComparableAttribute).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(AutoParsableAttribute).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location));

        var compilation = CSharpCompilation.Create(
            "CombinationTestAssembly",
            new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest)) },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generators);
        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation, out var outputCompilation, out _);

        var runResult = driver.GetRunResult();
        return (outputCompilation, runResult);
    }

    private static void AssertNoErrors(
        Compilation compilation,
        GeneratorDriverRunResult runResult)
    {
        var generatorErrors = runResult.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(generatorErrors);

        var compilationErrors = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(compilationErrors);
    }

    [Fact]
    public async Task EqualityAndComparable_Struct_SkipsEqualityOperators()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoEquality]
            [AutoComparable]
            public partial struct Amount
            {
                private readonly decimal _value;

                public static bool Equal(Amount left, Amount right)
                {
                    return left._value == right._value;
                }

                public override int GetHashCode()
                {
                    return _value.GetHashCode();
                }

                public static int Compare(Amount left, Amount right)
                {
                    return left._value.CompareTo(right._value);
                }
            }
            """;

        var (output, runResult) = await RunGeneratorsAsync(
            source,
            new EqualityGenerator(),
            new ComparableGenerator());

        AssertNoErrors(output, runResult);
        Assert.Equal(2, runResult.GeneratedTrees.Length);
    }

    [Fact]
    public async Task EqualityAndParsable_Struct_GeneratesIndependently()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoEquality]
            [AutoParsable]
            public partial struct ItemId
            {
                private readonly int _value;

                private ItemId(int value) { _value = value; }

                public static bool Equal(ItemId left, ItemId right)
                {
                    return left._value == right._value;
                }

                public override int GetHashCode()
                {
                    return _value.GetHashCode();
                }

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out ItemId result)
                {
                    if (int.TryParse(s, provider, out var value))
                    {
                        result = new ItemId(value);
                        return true;
                    }
                    result = default;
                    return false;
                }
            }
            """;

        var (output, runResult) = await RunGeneratorsAsync(
            source,
            new EqualityGenerator(),
            new ParsableGenerator());

        AssertNoErrors(output, runResult);
        Assert.Equal(2, runResult.GeneratedTrees.Length);
    }

    [Fact]
    public async Task ComparableAndFormattable_Struct_GeneratesIndependently()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoComparable]
            [AutoFormattable(DefaultFormat = "G")]
            public partial struct Temperature
            {
                private readonly double _value;
                public const int MaxBufferSize = 32;

                public static int Compare(Temperature left, Temperature right)
                {
                    return left._value.CompareTo(right._value);
                }

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _value.TryFormat(destination, out charsWritten, format, provider);
                }
            }
            """;

        var (output, runResult) = await RunGeneratorsAsync(
            source,
            new ComparableGenerator(),
            new FormattableGenerator());

        AssertNoErrors(output, runResult);
        Assert.Equal(2, runResult.GeneratedTrees.Length);
    }

    [Fact]
    public async Task ParsableAndFormattable_Struct_GeneratesRoundtripInterfaces()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoParsable]
            [AutoFormattable(DefaultFormat = "G")]
            public partial struct Measurement
            {
                private readonly double _value;
                public const int MaxBufferSize = 32;

                private Measurement(double value) { _value = value; }

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Measurement result)
                {
                    if (double.TryParse(s, provider, out var value))
                    {
                        result = new Measurement(value);
                        return true;
                    }
                    result = default;
                    return false;
                }

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _value.TryFormat(destination, out charsWritten, format, provider);
                }
            }
            """;

        var (output, runResult) = await RunGeneratorsAsync(
            source,
            new ParsableGenerator(),
            new FormattableGenerator());

        AssertNoErrors(output, runResult);
        Assert.Equal(2, runResult.GeneratedTrees.Length);
    }

    [Fact]
    public async Task AllFour_Struct_GeneratesAllInterfaces()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoEquality]
            [AutoComparable]
            [AutoParsable]
            [AutoFormattable(DefaultFormat = "G")]
            public partial struct Money
            {
                private readonly decimal _value;
                public const int MaxBufferSize = 64;

                private Money(decimal value) { _value = value; }

                public static bool Equal(Money left, Money right)
                {
                    return left._value == right._value;
                }

                public override int GetHashCode()
                {
                    return _value.GetHashCode();
                }

                public static int Compare(Money left, Money right)
                {
                    return left._value.CompareTo(right._value);
                }

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Money result)
                {
                    if (decimal.TryParse(s, provider, out var value))
                    {
                        result = new Money(value);
                        return true;
                    }
                    result = default;
                    return false;
                }

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _value.TryFormat(destination, out charsWritten, format, provider);
                }
            }
            """;

        var (output, runResult) = await RunGeneratorsAsync(
            source,
            new EqualityGenerator(),
            new ComparableGenerator(),
            new ParsableGenerator(),
            new FormattableGenerator());

        AssertNoErrors(output, runResult);
        Assert.Equal(4, runResult.GeneratedTrees.Length);
    }

    [Fact]
    public async Task AllFour_Class_GeneratesNullSafeOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoEquality]
            [AutoComparable]
            [AutoParsable]
            [AutoFormattable(DefaultFormat = "G")]
            public partial class Score
            {
                private readonly int _value;
                public const int MaxBufferSize = 32;

                private Score(int value) { _value = value; }

                public static bool Equal(Score? left, Score? right)
                {
                    if (ReferenceEquals(left, right))
                    {
                        return true;
                    }
                    if (left is null || right is null)
                    {
                        return false;
                    }
                    return left._value == right._value;
                }

                public override int GetHashCode()
                {
                    return _value.GetHashCode();
                }

                public static int Compare(Score? left, Score? right)
                {
                    if (ReferenceEquals(left, right))
                    {
                        return 0;
                    }
                    if (left is null)
                    {
                        return -1;
                    }
                    if (right is null)
                    {
                        return 1;
                    }
                    return left._value.CompareTo(right._value);
                }

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Score result)
                {
                    if (int.TryParse(s, provider, out var value))
                    {
                        result = new Score(value);
                        return true;
                    }
                    result = default!;
                    return false;
                }

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _value.TryFormat(destination, out charsWritten, format, provider);
                }
            }
            """;

        var (output, runResult) = await RunGeneratorsAsync(
            source,
            new EqualityGenerator(),
            new ComparableGenerator(),
            new ParsableGenerator(),
            new FormattableGenerator());

        AssertNoErrors(output, runResult);
        Assert.Equal(4, runResult.GeneratedTrees.Length);
    }

    [Fact]
    public async Task AllFour_RecordStruct_EqualitySkipsAllMembers()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoEquality]
            [AutoComparable]
            [AutoParsable]
            [AutoFormattable]
            public partial record struct Percentage
            {
                private readonly double _value;
                public const int MaxBufferSize = 16;

                private Percentage(double value) { _value = value; }

                public static bool Equal(Percentage left, Percentage right)
                {
                    return left._value == right._value;
                }

                public override int GetHashCode()
                {
                    return _value.GetHashCode();
                }

                public static int Compare(Percentage left, Percentage right)
                {
                    return left._value.CompareTo(right._value);
                }

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Percentage result)
                {
                    if (double.TryParse(s, provider, out var value))
                    {
                        result = new Percentage(value);
                        return true;
                    }
                    result = default;
                    return false;
                }

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _value.TryFormat(destination, out charsWritten, format, provider);
                }
            }
            """;

        var (output, runResult) = await RunGeneratorsAsync(
            source,
            new EqualityGenerator(),
            new ComparableGenerator(),
            new ParsableGenerator(),
            new FormattableGenerator());

        AssertNoErrors(output, runResult);
        // EqualityGenerator produces nothing for record struct (Equals, operators already synthesized)
        Assert.Equal(3, runResult.GeneratedTrees.Length);
    }

    [Fact]
    public async Task EqualityComparableParsable_Struct_ThreeWayCombination()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoEquality]
            [AutoComparable]
            [AutoParsable]
            public partial struct Priority
            {
                private readonly int _value;

                private Priority(int value) { _value = value; }

                public static bool Equal(Priority left, Priority right)
                {
                    return left._value == right._value;
                }

                public override int GetHashCode()
                {
                    return _value.GetHashCode();
                }

                public static int Compare(Priority left, Priority right)
                {
                    return left._value.CompareTo(right._value);
                }

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Priority result)
                {
                    if (int.TryParse(s, provider, out var value))
                    {
                        result = new Priority(value);
                        return true;
                    }
                    result = default;
                    return false;
                }
            }
            """;

        var (output, runResult) = await RunGeneratorsAsync(
            source,
            new EqualityGenerator(),
            new ComparableGenerator(),
            new ParsableGenerator());

        AssertNoErrors(output, runResult);
        Assert.Equal(3, runResult.GeneratedTrees.Length);
    }

    [Fact]
    public async Task ParsableAndFormattable_WithUtf8_GeneratesCombinedInterfaces()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoParsable(ImplementUtf8 = true)]
            [AutoFormattable(ImplementUtf8 = true, DefaultFormat = "G")]
            public partial struct Quantity
            {
                private readonly int _value;
                public const int MaxBufferSize = 32;

                private Quantity(int value) { _value = value; }

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Quantity result)
                {
                    if (int.TryParse(s, provider, out var value))
                    {
                        result = new Quantity(value);
                        return true;
                    }
                    result = default;
                    return false;
                }

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _value.TryFormat(destination, out charsWritten, format, provider);
                }
            }
            """;

        var (output, runResult) = await RunGeneratorsAsync(
            source,
            new ParsableGenerator(),
            new FormattableGenerator());

        AssertNoErrors(output, runResult);
        Assert.Equal(2, runResult.GeneratedTrees.Length);
    }
}
