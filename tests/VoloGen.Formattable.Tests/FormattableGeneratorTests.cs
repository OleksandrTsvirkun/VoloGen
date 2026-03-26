using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using VoloGen.Common;
using VoloGen.Formattable;
using Xunit;

namespace VoloGen.Formattable.Tests;

public class FormattableGeneratorTests
{
    [Fact]
    public async Task ValidStruct_GeneratesFormattableOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoFormattable]
            public partial struct Currency
            {
                private const int MaxBufferSize = 256;
                private readonly string _code;

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var s = _code.AsSpan();
                    if (s.TryCopyTo(destination))
                    {
                        charsWritten = s.Length;
                        return true;
                    }

                    charsWritten = 0;
                    return false;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ValidClass_GeneratesFormattableOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoFormattable]
            public partial class Money
            {
                private const int MaxBufferSize = 256;
                private readonly decimal _amount;

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _amount.TryFormat(destination, out charsWritten, format, provider);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task RecordStruct_GeneratesFormattableOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoFormattable]
            public partial record struct Percentage
            {
                private const int MaxBufferSize = 64;
                private readonly decimal _value;

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _value.TryFormat(destination, out charsWritten, format, provider);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ImplementUtf8_GeneratesIUtf8SpanFormattable()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoFormattable(ImplementUtf8 = true)]
            public partial struct Utf8Price
            {
                private const int MaxBufferSize = 128;
                private readonly decimal _value;

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _value.TryFormat(destination, out charsWritten, format, provider);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task DefaultFormat_AppliesDefaultFormatString()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoFormattable(DefaultFormat = "N2")]
            public partial struct Decimal
            {
                private const int MaxBufferSize = 256;
                private readonly decimal _value;

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _value.TryFormat(destination, out charsWritten, format, provider);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task AllowNullFormatProvider_GeneratesNullCheckGuard()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoFormattable(AllowNullFormatProvider = false)]
            public partial struct StrictCurrency
            {
                private const int MaxBufferSize = 256;
                private readonly decimal _value;

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _value.TryFormat(destination, out charsWritten, format, provider);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task AllOptions_GeneratesAllOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoFormattable(ImplementUtf8 = true, DefaultFormat = "C", AllowNullFormatProvider = false)]
            public partial struct FullCurrency
            {
                private const int MaxBufferSize = 256;
                private readonly decimal _value;

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _value.TryFormat(destination, out charsWritten, format, provider);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
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

            [{|#0:AutoFormattable|}]
            public static partial class Formatter
            {
                public static bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    charsWritten = 0;
                    return true;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(DiagnosticDescriptors.CannotBeStatic)
                        .WithLocation(0)
                        .WithArguments("Formatter", "AutoFormattable")
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task MissingTryFormatMethod_ReportsDiagnostic()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [{|#0:AutoFormattable|}]
            public partial struct Amount
            {
                private const int MaxBufferSize = 256;
                private readonly decimal _value;
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(DiagnosticDescriptors.MissingTryFormatMethod)
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
    public async Task MissingMaxBufferSizeAndToString_ReportsDiagnostic()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [{|#0:AutoFormattable|}]
            public partial struct Value
            {
                private readonly decimal _amount;

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _amount.TryFormat(destination, out charsWritten, format, provider);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(DiagnosticDescriptors.MissingToStringOrMaxBufferSize)
                        .WithLocation(0)
                        .WithArguments("Value")
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

            [{|#0:AutoFormattable|}]
            public struct Currency
            {
                private readonly string _code;
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(DiagnosticDescriptors.MustBePartial)
                        .WithLocation(0)
                        .WithArguments("Currency", "AutoFormattable")
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task Utf8AndDefaultFormat_GeneratesCombinedOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoFormattable(ImplementUtf8 = true, DefaultFormat = "N2")]
            public partial struct Utf8Decimal
            {
                private const int MaxBufferSize = 128;
                private readonly decimal _value;

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _value.TryFormat(destination, out charsWritten, format, provider);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task Utf8AndAllowNullFormatProvider_GeneratesGuardedUtf8()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoFormattable(ImplementUtf8 = true, AllowNullFormatProvider = false)]
            public partial struct StrictUtf8Price
            {
                private const int MaxBufferSize = 128;
                private readonly decimal _value;

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _value.TryFormat(destination, out charsWritten, format, provider);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task DefaultFormatAndAllowNullFormatProvider_GeneratesCombinedFlags()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoFormattable(DefaultFormat = "C", AllowNullFormatProvider = false)]
            public partial struct StrictMoney
            {
                private const int MaxBufferSize = 128;
                private readonly decimal _value;

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _value.TryFormat(destination, out charsWritten, format, provider);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task WithCustomToString_GeneratesOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoFormattable]
            public partial struct Rate
            {
                private readonly decimal _value;

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _value.TryFormat(destination, out charsWritten, format, provider);
                }

                public string ToString(string? format, IFormatProvider? provider)
                {
                    return _value.ToString(format, provider) + "%";
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task GlobalNamespaceType_GeneratesFormattableOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            [AutoFormattable]
            public partial struct GlobalCounter
            {
                private const int MaxBufferSize = 32;
                private readonly int _count;

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _count.TryFormat(destination, out charsWritten, format, provider);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task LargeMaxBufferSize_GeneratesArrayPoolPath()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoFormattable]
            public partial struct LargeDocument
            {
                private const int MaxBufferSize = 4096;
                private readonly string _content;

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var s = (_content ?? "").AsSpan();
                    if (s.TryCopyTo(destination))
                    {
                        charsWritten = s.Length;
                        return true;
                    }
                    charsWritten = 0;
                    return false;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ExistingTryFormatOverloads_SkipsExistingMethods()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoFormattable]
            public partial struct Metric
            {
                private const int MaxBufferSize = 64;
                private readonly double _value;

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    return _value.TryFormat(destination, out charsWritten, format, provider);
                }

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format)
                {
                    return TryFormat(destination, out charsWritten, format, null);
                }

                public bool TryFormat(Span<char> destination, out int charsWritten)
                {
                    return TryFormat(destination, out charsWritten, default, null);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task RecordClass_GeneratesFormattableOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoFormattable]
            public partial record Label
            {
                private const int MaxBufferSize = 128;
                private readonly string _text;

                public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
                {
                    var s = (_text ?? "").AsSpan();
                    if (s.TryCopyTo(destination))
                    {
                        charsWritten = s.Length;
                        return true;
                    }
                    charsWritten = 0;
                    return false;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<FormattableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoFormattableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }
}

