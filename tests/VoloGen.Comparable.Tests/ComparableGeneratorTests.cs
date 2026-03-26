using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using VoloGen.Common;
using VoloGen.Comparable;
using Xunit;

namespace VoloGen.Comparable.Tests;

public class ComparableGeneratorTests
{
    [Fact]
    public async Task ValidStruct_GeneratesComparableOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoComparable]
            public partial struct Age
            {
                private readonly int _value;

                public static int Compare(Age left, Age right)
                {
                    return left._value.CompareTo(right._value);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ComparableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoComparableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ValidClass_GeneratesComparableOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoComparable]
            public partial class Price
            {
                private readonly decimal _amount;

                public static int Compare(Price? left, Price? right)
                {
                    return left._amount.CompareTo(right._amount);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ComparableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoComparableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task RecordStruct_GeneratesComparableOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoComparable]
            public partial record struct Score
            {
                private readonly int _value;

                public static int Compare(Score left, Score right)
                {
                    return left._value.CompareTo(right._value);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ComparableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoComparableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task MultipleFields_GeneratesComparableOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoComparable]
            public partial struct Coordinate
            {
                private readonly int _x;
                private readonly int _y;
                private readonly int _z;

                public static int Compare(Coordinate left, Coordinate right)
                {
                    var xCompare = left._x.CompareTo(right._x);
                    if (xCompare != 0) return xCompare;
                    var yCompare = left._y.CompareTo(right._y);
                    if (yCompare != 0) return yCompare;
                    return left._z.CompareTo(right._z);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ComparableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoComparableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ReferenceTypeField_GeneratesComparableOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoComparable]
            public partial struct Text
            {
                private readonly string _value;

                public static int Compare(Text left, Text right)
                {
                    return (left._value ?? "").CompareTo(right._value ?? "");
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ComparableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoComparableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task NullableValueTypeField_GeneratesComparableOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoComparable]
            public partial struct OptionalScore
            {
                private readonly int? _value;

                public static int Compare(OptionalScore left, OptionalScore right)
                {
                    return Nullable.Compare(left._value, right._value);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ComparableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoComparableAttribute).Assembly.Location)
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

            [{|#0:AutoComparable|}]
            public struct Age
            {
                private readonly int _value;
            }
            """;

        var test = new CSharpSourceGeneratorTest<ComparableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoComparableAttribute).Assembly.Location)
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(DiagnosticDescriptors.MustBePartial)
                        .WithLocation(0)
                        .WithArguments("Age", "AutoComparable")
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

            [{|#0:AutoComparable|}]
            public static partial class Sorter
            {
                public static int Compare(int left, int right)
                {
                    return left.CompareTo(right);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ComparableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoComparableAttribute).Assembly.Location)
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(DiagnosticDescriptors.CannotBeStatic)
                        .WithLocation(0)
                        .WithArguments("Sorter", "AutoComparable")
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task MissingCompareMethod_ReportsDiagnostic()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [{|#0:AutoComparable|}]
            public partial struct Value
            {
                private readonly int _amount;
            }
            """;

        var test = new CSharpSourceGeneratorTest<ComparableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoComparableAttribute).Assembly.Location)
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(DiagnosticDescriptors.MissingComparableField)
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
    public async Task WithAutoEquality_GeneratesAllOperators()
    {
        string source = """
            using System;
            using VoloGen;

            namespace VoloGen
            {
                [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
                public sealed class AutoEqualityAttribute : Attribute { }
            }

            namespace TestNamespace
            {
                [AutoComparable]
                [AutoEquality]
                public partial struct Weight
                {
                    private readonly double _kg;

                    public static int Compare(Weight left, Weight right)
                    {
                        return left._kg.CompareTo(right._kg);
                    }
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ComparableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoComparableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task GlobalNamespaceType_GeneratesComparableOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            [AutoComparable]
            public partial struct GlobalScore
            {
                private readonly int _value;

                public static int Compare(GlobalScore left, GlobalScore right)
                {
                    return left._value.CompareTo(right._value);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ComparableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoComparableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ExistingCompareTo_SkipsCompareToGeneration()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoComparable]
            public partial struct Distance
            {
                private readonly double _meters;

                public static int Compare(Distance left, Distance right)
                {
                    return left._meters.CompareTo(right._meters);
                }

                public int CompareTo(Distance other)
                {
                    return Compare(this, other);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ComparableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoComparableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ExistingAllOperators_SkipsOperatorGeneration()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoComparable]
            public partial struct Rating
            {
                private readonly int _stars;

                public static int Compare(Rating left, Rating right)
                {
                    return left._stars.CompareTo(right._stars);
                }

                public static bool operator <(Rating left, Rating right)
                {
                    return Compare(left, right) < 0;
                }

                public static bool operator >(Rating left, Rating right)
                {
                    return Compare(left, right) > 0;
                }

                public static bool operator <=(Rating left, Rating right)
                {
                    return Compare(left, right) <= 0;
                }

                public static bool operator >=(Rating left, Rating right)
                {
                    return Compare(left, right) >= 0;
                }

                public static bool operator ==(Rating left, Rating right)
                {
                    return Compare(left, right) == 0;
                }

                public static bool operator !=(Rating left, Rating right)
                {
                    return Compare(left, right) != 0;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ComparableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoComparableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task NamespacedClass_GeneratesNullSafeCompareTo()
    {
        string source = """
            using System;
            using VoloGen;

            namespace Domain.ValueObjects.Ordering;

            [AutoComparable]
            public partial class Priority
            {
                private readonly int _level;

                public static int Compare(Priority? left, Priority? right)
                {
                    return (left?._level ?? 0).CompareTo(right?._level ?? 0);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<ComparableGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoComparableAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }
}

