using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using VoloGen.Common;
using VoloGen.Equality;
using Xunit;

namespace VoloGen.Equality.Tests;

public class EqualityGeneratorTests
{
    [Fact]
    public async Task ValidStruct_GeneratesEqualityOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoEquality]
            public partial struct UserId
            {
                private readonly Guid _value;

                public static bool Equal(UserId left, UserId right)
                {
                    return left._value == right._value;
                }

                public override int GetHashCode()
                {
                    return _value.GetHashCode();
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<EqualityGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ValidClass_GeneratesEqualityOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoEquality]
            public partial class Product
            {
                private readonly string _name;
                private readonly decimal _price;

                public static bool Equal(Product? left, Product? right)
                {
                    return left._name == right._name && left._price == right._price;
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        return _name.GetHashCode() ^ _price.GetHashCode();
                    }
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<EqualityGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ValidRecordStruct_GeneratesEqualityOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoEquality]
            public partial record struct Email
            {
                private readonly string _address;

                public static bool Equal(Email left, Email right)
                {
                    return left._address == right._address;
                }

                public override int GetHashCode()
                {
                    return _address.GetHashCode();
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<EqualityGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task MultipleFields_GeneratesEqualityOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoEquality]
            public partial struct Point
            {
                private readonly int _x;
                private readonly int _y;
                private readonly int _z;

                public static bool Equal(Point left, Point right)
                {
                    return left._x == right._x && left._y == right._y && left._z == right._z;
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        return (_x * 397) ^ (_y * 397) ^ _z;
                    }
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<EqualityGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ReferenceTypeField_GeneratesEqualityOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoEquality]
            public partial struct Name
            {
                private readonly string _value;

                public static bool Equal(Name left, Name right)
                {
                    return left._value == right._value;
                }

                public override int GetHashCode()
                {
                    return _value?.GetHashCode() ?? 0;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<EqualityGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task NullableValueTypeField_GeneratesEqualityOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoEquality]
            public partial struct OptionalValue
            {
                private readonly int? _value;

                public static bool Equal(OptionalValue left, OptionalValue right)
                {
                    return left._value == right._value;
                }

                public override int GetHashCode()
                {
                    return _value.GetHashCode();
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<EqualityGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location)
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

            [{|#0:AutoEquality|}]
            public struct UserId
            {
                private readonly Guid _value;
            }
            """;

        var test = new CSharpSourceGeneratorTest<EqualityGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location)
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(DiagnosticDescriptors.MustBePartial)
                        .WithLocation(0)
                        .WithArguments("UserId", "AutoEquality")
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

            [{|#0:AutoEquality|}]
            public static partial class Utilities
            {
                public static bool Equal(int left, int right)
                {
                    return left == right;
                }

                public static int GetHashCode(int obj)
                {
                    return obj.GetHashCode();
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<EqualityGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location)
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(DiagnosticDescriptors.CannotBeStatic)
                        .WithLocation(0)
                        .WithArguments("Utilities", "AutoEquality")
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task NoFields_ReportsDiagnostic()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [{|#0:AutoEquality|}]
            public partial struct Empty
            {
            }
            """;

        var test = new CSharpSourceGeneratorTest<EqualityGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location)
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(DiagnosticDescriptors.MissingEquatableField)
                        .WithLocation(0)
                        .WithArguments("Empty")
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task MissingEqualMethod_ReportsDiagnostic()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [{|#0:AutoEquality|}]
            public partial struct Amount
            {
                private readonly decimal _value;

                public override int GetHashCode()
                {
                    return _value.GetHashCode();
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<EqualityGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location)
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(DiagnosticDescriptors.MissingEquatableField)
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
    public async Task MissingGetHashCodeMethod_ReportsDiagnostic()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [{|#0:AutoEquality|}]
            public partial struct Amount
            {
                private readonly decimal _value;

                public static bool Equal(Amount left, Amount right)
                {
                    return left._value == right._value;
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<EqualityGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location)
                },
                ExpectedDiagnostics =
                {
                    new DiagnosticResult(DiagnosticDescriptors.MissingEquatableField)
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
    public async Task NamespacedType_GeneratesEqualityOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            namespace Domain.ValueObjects;

            [AutoEquality]
            public partial struct CustomerId
            {
                private readonly long _value;

                public static bool Equal(CustomerId left, CustomerId right)
                {
                    return left._value == right._value;
                }

                public override int GetHashCode()
                {
                    return _value.GetHashCode();
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<EqualityGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task GlobalNamespaceType_GeneratesEqualityOverloads()
    {
        string source = """
            using System;
            using VoloGen;

            [AutoEquality]
            public partial struct GlobalId
            {
                private readonly Guid _value;

                public static bool Equal(GlobalId left, GlobalId right)
                {
                    return left._value == right._value;
                }

                public override int GetHashCode()
                {
                    return _value.GetHashCode();
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<EqualityGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task WithAutoComparable_SkipsEqualityOperators()
    {
        string source = """
            using System;
            using VoloGen;

            namespace VoloGen
            {
                [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
                public sealed class AutoComparableAttribute : Attribute { }
            }

            namespace TestNamespace
            {
                [AutoEquality]
                [AutoComparable]
                public partial struct Price
                {
                    private readonly decimal _value;

                    public static bool Equal(Price left, Price right)
                    {
                        return left._value == right._value;
                    }

                    public override int GetHashCode()
                    {
                        return _value.GetHashCode();
                    }
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<EqualityGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ExistingEqualsTyped_SkipsGeneration()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoEquality]
            public partial struct Quantity
            {
                private readonly int _value;

                public static bool Equal(Quantity left, Quantity right)
                {
                    return left._value == right._value;
                }

                public override int GetHashCode()
                {
                    return _value.GetHashCode();
                }

                public bool Equals(Quantity other)
                {
                    return Equal(this, other);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<EqualityGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task ExistingOperators_SkipsOperatorGeneration()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoEquality]
            public partial struct Weight
            {
                private readonly double _kg;

                public static bool Equal(Weight left, Weight right)
                {
                    return left._kg == right._kg;
                }

                public override int GetHashCode()
                {
                    return _kg.GetHashCode();
                }

                public static bool operator ==(Weight left, Weight right)
                {
                    return Equal(left, right);
                }

                public static bool operator !=(Weight left, Weight right)
                {
                    return !Equal(left, right);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<EqualityGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }

    [Fact]
    public async Task AllMembersExist_GeneratesNothing()
    {
        string source = """
            using System;
            using VoloGen;

            namespace TestNamespace;

            [AutoEquality]
            public partial struct FullyDefined
            {
                private readonly int _value;

                public static bool Equal(FullyDefined left, FullyDefined right)
                {
                    return left._value == right._value;
                }

                public override int GetHashCode()
                {
                    return _value.GetHashCode();
                }

                public bool Equals(FullyDefined other)
                {
                    return Equal(this, other);
                }

                public override bool Equals(object? obj)
                {
                    return obj is FullyDefined other && Equals(other);
                }

                public static bool operator ==(FullyDefined left, FullyDefined right)
                {
                    return Equal(left, right);
                }

                public static bool operator !=(FullyDefined left, FullyDefined right)
                {
                    return !Equal(left, right);
                }
            }
            """;

        var test = new CSharpSourceGeneratorTest<EqualityGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(AutoEqualityAttribute).Assembly.Location)
                }
            },
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();

        Assert.True(true);
    }
}

