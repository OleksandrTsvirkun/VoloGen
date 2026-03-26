using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using VoloGen.Common;

namespace VoloGen.Equality;

/// <summary>
/// Roslyn incremental source generator that processes types annotated with
/// <see cref="AutoEqualityAttribute"/> and emits <see cref="System.IEquatable{T}"/>
/// implementation, <c>Equals(object)</c>, and equality operator overloads.
/// </summary>
/// <remarks>
/// <para>Generated members delegate to the user-supplied <c>static bool Equal(T, T)</c> method.</para>
/// <para>
/// If the target type is also annotated with <c>[AutoComparable]</c>, the <c>==</c> and <c>!=</c>
/// operators are <b>not</b> emitted by this generator to prevent duplicate declarations.
/// </para>
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class EqualityGenerator : IIncrementalGenerator
{
    /// <summary>Fully-qualified metadata name of the <c>[AutoEquality]</c> marker attribute.</summary>
    private const string AutoEqualityAttributeName = "VoloGen.AutoEqualityAttribute";
    /// <summary>Fully-qualified metadata name of the <c>[AutoComparable]</c> attribute used for operator dedup.</summary>
    private const string AutoComparableAttributeName = "VoloGen.AutoComparableAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AutoEqualityAttributeName,
                predicate: static (node, _) => node is TypeDeclarationSyntax,
                transform: static (ctx, _) => GetTargetInfo(ctx))
            .Where(static t => t is not null)
            .Select(static (t, _) => t!.Value);

        context.RegisterSourceOutput(targets, static (spc, target) => Execute(spc, target));
    }

    /// <summary>
    /// Extracts metadata from the <see cref="GeneratorAttributeSyntaxContext"/> for a type
    /// annotated with <c>[AutoEquality]</c>.
    /// </summary>
    /// <param name="context">The syntax context provided by the incremental pipeline.</param>
    /// <returns>A <see cref="TargetInfo"/> value, or <see langword="null"/> if the target is not a named type.</returns>
    private static TargetInfo? GetTargetInfo(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        var syntax = (TypeDeclarationSyntax)context.TargetNode;

        bool isPartial = syntax.Modifiers.Any(SyntaxKind.PartialKeyword);
        bool isStatic = typeSymbol.IsStatic;
        bool isValueType = typeSymbol.IsValueType;

        string typeName = typeSymbol.Name;
        string fullNamespace = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : typeSymbol.ContainingNamespace.ToDisplayString();

        string typeKeyword = syntax switch
        {
            StructDeclarationSyntax => "struct",
            ClassDeclarationSyntax => "class",
            RecordDeclarationSyntax r when r.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword) => "record struct",
            RecordDeclarationSyntax => "record",
            _ => "class"
        };

        bool hasEqual = HasStaticEqualMethod(typeSymbol);
        bool hasGetHashCode = HasGetHashCodeOverride(typeSymbol);
        bool hasEqualsType = HasEqualsTypeMethod(typeSymbol);
        bool hasEqualsObject = HasEqualsObjectMethod(typeSymbol);
        bool hasOperatorEquality = HasBinaryBoolOperator(typeSymbol, "op_Equality");
        bool hasOperatorInequality = HasBinaryBoolOperator(typeSymbol, "op_Inequality");
        bool hasComparableAttribute = HasComparableAttribute(typeSymbol);

        var attributeLocation = context.Attributes[0].ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? syntax.GetLocation();

        return new TargetInfo(
            typeName,
            fullNamespace,
            typeKeyword,
            isPartial,
            isStatic,
            isValueType,
            hasEqual,
            hasGetHashCode,
            hasEqualsType,
            hasEqualsObject,
            hasOperatorEquality,
            hasOperatorInequality,
            hasComparableAttribute,
            attributeLocation);
    }

    /// <summary>
    /// Checks whether <paramref name="typeSymbol"/> declares a
    /// <c>static bool Equal(T, T)</c> method.
    /// </summary>
    private static bool HasStaticEqualMethod(INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers("Equal"))
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (!method.IsStatic
                || method.ReturnType.SpecialType != SpecialType.System_Boolean
                || method.Parameters.Length != 2
                || method.Parameters[0].RefKind != RefKind.None
                || method.Parameters[1].RefKind != RefKind.None)
            {
                continue;
            }

            if (SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, typeSymbol)
                && SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, typeSymbol))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether <paramref name="typeSymbol"/> contains an
    /// <c>override int GetHashCode()</c> declaration.
    /// </summary>
    private static bool HasGetHashCodeOverride(INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers(nameof(GetHashCode)))
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (method.IsStatic
                || method.Parameters.Length != 0
                || method.ReturnType.SpecialType != SpecialType.System_Int32)
            {
                continue;
            }

            if (method.IsOverride)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether <paramref name="typeSymbol"/> already declares a typed
    /// <c>bool Equals(T)</c> instance method.
    /// </summary>
    private static bool HasEqualsTypeMethod(INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers("Equals"))
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (method.IsStatic
                || method.ReturnType.SpecialType != SpecialType.System_Boolean
                || method.Parameters.Length != 1
                || method.Parameters[0].RefKind != RefKind.None)
            {
                continue;
            }

            if (SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, typeSymbol))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether <paramref name="typeSymbol"/> already declares an
    /// <c>override bool Equals(object?)</c> method.
    /// </summary>
    private static bool HasEqualsObjectMethod(INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers("Equals"))
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (method.IsStatic
                || method.ReturnType.SpecialType != SpecialType.System_Boolean
                || method.Parameters.Length != 1
                || method.Parameters[0].RefKind != RefKind.None)
            {
                continue;
            }

            if (method.Parameters[0].Type.SpecialType == SpecialType.System_Object)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether <paramref name="typeSymbol"/> already declares a
    /// <c>static bool operator</c> with the specified metadata name (e.g., <c>op_Equality</c>).
    /// </summary>
    /// <param name="typeSymbol">The type to inspect.</param>
    /// <param name="operatorMetadataName">The CLR metadata name of the operator.</param>
    private static bool HasBinaryBoolOperator(INamedTypeSymbol typeSymbol, string operatorMetadataName)
    {
        foreach (var member in typeSymbol.GetMembers(operatorMetadataName))
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (!method.IsStatic
                || method.ReturnType.SpecialType != SpecialType.System_Boolean
                || method.Parameters.Length != 2
                || method.Parameters[0].RefKind != RefKind.None
                || method.Parameters[1].RefKind != RefKind.None)
            {
                continue;
            }

            if (SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, typeSymbol)
                && SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, typeSymbol))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether <paramref name="typeSymbol"/> is also annotated with
    /// <c>[AutoComparable]</c>, which affects equality operator generation.
    /// </summary>
    private static bool HasComparableAttribute(INamedTypeSymbol typeSymbol)
    {
        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == AutoComparableAttributeName)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates the target and emits the generated equality source file,
    /// or reports diagnostics if requirements are not met.
    /// </summary>
    /// <param name="context">The source production context for adding source or diagnostics.</param>
    /// <param name="target">The collected metadata for the annotated type.</param>
    private static void Execute(SourceProductionContext context, TargetInfo target)
    {
        if (!target.IsPartial)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.MustBePartial,
                target.Location,
                target.TypeName,
                "AutoEquality"));
            return;
        }

        if (target.IsStatic)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.CannotBeStatic,
                target.Location,
                target.TypeName,
                "AutoEquality"));
            return;
        }

        if (!target.HasEqual || !target.HasGetHashCode)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.MissingEquatableField,
                target.Location,
                target.TypeName));
            return;
        }

        string source = GenerateSource(target);
        if (string.IsNullOrEmpty(source))
        {
            return;
        }

        context.AddSource($"{target.TypeName}.Equality.g.cs", source);
    }

    /// <summary>
    /// Builds the complete generated source text for the equality partial declaration.
    /// </summary>
    /// <param name="target">The collected metadata for the annotated type.</param>
    /// <returns>The generated C# source code, or <see cref="string.Empty"/> if all members already exist.</returns>
    private static string GenerateSource(TargetInfo target)
    {
        string nullableAnnotation = GetNullableAnnotation(target);
        string ns = BuildNamespacePrefix(target);
        string generatedTypeHeader = BuildGeneratedTypeHeader(target, ns);
        string nullCheck = BuildNullCheckForEqualsType(target);
        bool shouldGenerateEqualityOperators = ShouldGenerateEqualityOperators(target);

        StringBuilder members = new();

        AppendEqualsTypeIfMissing(target, nullableAnnotation, nullCheck, members);
        AppendEqualsObjectIfMissing(target, members);
        AppendOperatorEqualityIfMissing(target, shouldGenerateEqualityOperators, members);
        AppendOperatorInequalityIfMissing(target, shouldGenerateEqualityOperators, members);

        if (members.Length == 0)
        {
            return string.Empty;
        }

        return generatedTypeHeader + members + "}\n";
    }

    private static string GetNullableAnnotation(TargetInfo target)
    {
        if (target.IsValueType)
        {
            return string.Empty;
        }

        return "?";
    }

    private static string BuildNamespacePrefix(TargetInfo target)
    {
        if (string.IsNullOrEmpty(target.Namespace))
        {
            return string.Empty;
        }

        return $"namespace {target.Namespace};";
    }

    private static string BuildGeneratedTypeHeader(TargetInfo target, string ns)
    {
        return $$"""
// <auto-generated/>
#nullable enable
using System;

{{ns}}

partial {{target.TypeKeyword}} {{target.TypeName}} : System.IEquatable<{{target.TypeName}}>
{
""";
    }

    private static string BuildNullCheckForEqualsType(TargetInfo target)
    {
        if (target.IsValueType)
        {
            return string.Empty;
        }

        return """
                if (other is null)
                {
                    return false;
                }

            """;
    }

    private static bool ShouldGenerateEqualityOperators(TargetInfo target)
    {
        if (target.HasComparableAttributeMarker)
        {
            return false;
        }

        return true;
    }

    private static void AppendEqualsTypeIfMissing(TargetInfo target, string nullableAnnotation, string nullCheck, StringBuilder members)
    {
        if (target.HasEqualsType)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool Equals({{target.TypeName}}{{nullableAnnotation}} other)
    {
{{nullCheck}}        return Equal(this, other);
    }
""");
    }

    private static void AppendEqualsObjectIfMissing(TargetInfo target, StringBuilder members)
    {
        if (target.HasEqualsObject)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        return obj is {{target.TypeName}} other && Equals(other);
    }
""");
    }

    private static void AppendOperatorEqualityIfMissing(TargetInfo target, bool shouldGenerateEqualityOperators, StringBuilder members)
    {
        if (!shouldGenerateEqualityOperators)
        {
            return;
        }

        if (target.HasOperatorEquality)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator ==({{target.TypeName}} left, {{target.TypeName}} right)
    {
        return Equal(left, right);
    }
""");
    }

    private static void AppendOperatorInequalityIfMissing(TargetInfo target, bool shouldGenerateEqualityOperators, StringBuilder members)
    {
        if (!shouldGenerateEqualityOperators)
        {
            return;
        }

        if (target.HasOperatorInequality)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator !=({{target.TypeName}} left, {{target.TypeName}} right)
    {
        return !Equal(left, right);
    }
""");
    }

    /// <summary>
    /// Immutable snapshot of all metadata the generator needs about a single
    /// <c>[AutoEquality]</c>-annotated type.
    /// </summary>
    private readonly record struct TargetInfo(
        string TypeName,
        string Namespace,
        string TypeKeyword,
        bool IsPartial,
        bool IsStatic,
        bool IsValueType,
        bool HasEqual,
        bool HasGetHashCode,
        bool HasEqualsType,
        bool HasEqualsObject,
        bool HasOperatorEquality,
        bool HasOperatorInequality,
        bool HasComparableAttributeMarker,
        Location Location);
}

