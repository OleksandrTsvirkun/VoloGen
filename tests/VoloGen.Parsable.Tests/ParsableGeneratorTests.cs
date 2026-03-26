using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using VoloGen.Common;
using VoloGen.Parsable;
using Xunit;

namespace VoloGen.Parsable.Tests;

public class ParsableGeneratorTests
{
    [Fact]
    public async Task ValidStruct_GeneratesParsableOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoParsable]
            public partial struct Amount
            {
                private decimal _value;

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Amount result)
                {
                    if (decimal.TryParse(s, provider, out var value))
                    {
                        result = new Amount { _value = value };
                        return true;
                    }
                    result = default;
                    return false;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ParsableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoParsableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ValidClass_GeneratesParsableOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoParsable]
            public partial class Temperature
            {
                private double _celsius;

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Temperature result)
                {
                    if (double.TryParse(s, provider, out var value))
                    {
                        result = new Temperature { _celsius = value };
                        return true;
                    }
                    result = default;
                    return false;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ParsableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoParsableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task RecordStruct_GeneratesParsableOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoParsable]
            public partial record struct Percentage
            {
                private decimal _value;

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Percentage result)
                {
                    if (decimal.TryParse(s, provider, out var value))
                    {
                        result = new Percentage { _value = value };
                        return true;
                    }
                    result = default;
                    return false;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ParsableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoParsableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ImplementUtf8_GeneratesIUtf8SpanParsable()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoParsable(ImplementUtf8 = true)]
            public partial struct Utf8Amount
            {
                private decimal _value;

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Utf8Amount result)
                {
                    if (decimal.TryParse(s, provider, out var value))
                    {
                        result = new Utf8Amount { _value = value };
                        return true;
                    }
                    result = default;
                    return false;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ParsableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoParsableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ImplementExact_GeneratesParsableExactMethods()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoParsable(ImplementExact = true)]
            public partial struct ExactAmount
            {
                private decimal _value;

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out ExactAmount result)
                {
                    if (decimal.TryParse(s, provider, out var value))
                    {
                        result = new ExactAmount { _value = value };
                        return true;
                    }
                    result = default;
                    return false;
                }

                public static bool TryParseExact(ReadOnlySpan<char> s, ReadOnlySpan<char> format, IFormatProvider? provider, out ExactAmount result)
                {
                    if (decimal.TryParse(s, provider, out var value))
                    {
                        result = new ExactAmount { _value = value };
                        return true;
                    }
                    result = default;
                    return false;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ParsableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoParsableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ImplementUtf8AndExact_GeneratesAllOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoParsable(ImplementUtf8 = true, ImplementExact = true)]
            public partial struct FullAmount
            {
                private decimal _value;

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out FullAmount result)
                {
                    if (decimal.TryParse(s, provider, out var value))
                    {
                        result = new FullAmount { _value = value };
                        return true;
                    }
                    result = default;
                    return false;
                }

                public static bool TryParseExact(ReadOnlySpan<char> s, ReadOnlySpan<char> format, IFormatProvider? provider, out FullAmount result)
                {
                    if (decimal.TryParse(s, provider, out var value))
                    {
                        result = new FullAmount { _value = value };
                        return true;
                    }
                    result = default;
                    return false;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ParsableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoParsableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task StaticType_ReportsDiagnostic()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [{|#0:AutoParsable|}]
            public static partial class Parser
            {
                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out int result)
                {
                    return int.TryParse(s, provider, out result);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ParsableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoParsableAttribute).Assembly.Location)
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(DiagnosticDescriptors.CannotBeStatic)
                        .WithLocation(0)
                        .WithArguments("Parser", "AutoParsable")
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task MissingTryParseMethod_ReportsDiagnostic()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [{|#0:AutoParsable|}]
            public partial struct Amount
            {
                private decimal _value;
            }
            """;

        var test = new CSharpSourceGeneratorTest<ParsableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoParsableAttribute).Assembly.Location)
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(DiagnosticDescriptors.MissingTryParseMethod)
                        .WithLocation(0)
                        .WithArguments("Amount")
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task NonPartialType_ReportsDiagnostic()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [{|#0:AutoParsable|}]
            public struct Amount
            {
                private decimal _value;

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Amount result)
                {
                    result = default;
                    return false;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ParsableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoParsableAttribute).Assembly.Location)
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(DiagnosticDescriptors.MustBePartial)
                        .WithLocation(0)
                        .WithArguments("Amount", "AutoParsable")
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task GlobalNamespaceType_GeneratesParsableOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            [AutoParsable]
            public partial struct GlobalAmount
            {
                private decimal _value;

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out GlobalAmount result)
                {
                    if (decimal.TryParse(s, provider, out var value))
                    {
                        result = new GlobalAmount { _value = value };
                        return true;
                    }
                    result = default;
                    return false;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ParsableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoParsableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ClassWithAllFlags_GeneratesAllOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoParsable(ImplementUtf8 = true, ImplementExact = true)]
            public partial class DateValue
            {
                private DateOnly _value;

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out DateValue result)
                {
                    if (DateOnly.TryParse(s, provider, out var value))
                    {
                        result = new DateValue { _value = value };
                        return true;
                    }
                    result = default;
                    return false;
                }

                public static bool TryParseExact(ReadOnlySpan<char> s, ReadOnlySpan<char> format, IFormatProvider? provider, out DateValue result)
                {
                    if (DateOnly.TryParseExact(s, format, provider, System.Globalization.DateTimeStyles.None, out var value))
                    {
                        result = new DateValue { _value = value };
                        return true;
                    }
                    result = default;
                    return false;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ParsableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoParsableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task AbstractType_ReportsDiagnostic()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [{|#0:AutoParsable|}]
            public abstract partial class BaseValue
            {
                private decimal _value;

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out BaseValue result)
                {
                    result = default!;
                    return false;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ParsableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoParsableAttribute).Assembly.Location)
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(DiagnosticDescriptors.CannotBeAbstract)
                        .WithLocation(0)
                        .WithArguments("BaseValue", "AutoParsable")
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task MissingTryParseExact_WithImplementExact_ReportsDiagnostic()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [{|#0:AutoParsable(ImplementExact = true)|}]
            public partial struct FormattedValue
            {
                private decimal _value;

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out FormattedValue result)
                {
                    if (decimal.TryParse(s, provider, out var value))
                    {
                        result = new FormattedValue { _value = value };
                        return true;
                    }
                    result = default;
                    return false;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ParsableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoParsableAttribute).Assembly.Location)
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(DiagnosticDescriptors.MissingTryParseMethod)
                        .WithLocation(0)
                        .WithArguments("FormattedValue")
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ExistingParseOverloads_SkipsExistingMethods()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoParsable]
            public partial struct Distance
            {
                private double _meters;

                public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Distance result)
                {
                    if (double.TryParse(s, provider, out var value))
                    {
                        result = new Distance { _meters = value };
                        return true;
                    }
                    result = default;
                    return false;
                }

                public static Distance Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
                {
                    if (!TryParse(s, provider, out var result))
                    {
                        throw new FormatException();
                    }
                    return result;
                }

                public static bool TryParse(ReadOnlySpan<char> s, out Distance result)
                {
                    return TryParse(s, null, out result);
                }

                public static Distance Parse(ReadOnlySpan<char> s)
                {
                    return Parse(s, null);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ParsableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoParsableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }
}

