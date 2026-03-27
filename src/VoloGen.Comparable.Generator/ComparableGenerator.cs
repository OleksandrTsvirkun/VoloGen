using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Text;

using VoloGen.Common;

namespace VoloGen.Comparable;

/// <summary>
/// Roslyn incremental source generator that processes types annotated with
/// <see cref="AutoComparableAttribute"/> and emits <see cref="System.IComparable{T}"/>,
/// <see cref="System.IComparable"/>, and comparison / equality operator overloads.
/// </summary>
/// <remarks>
/// Generated members delegate to the user-supplied <c>static int Compare(T, T)</c> method.
/// This generator also emits <c>==</c> and <c>!=</c> operators, taking precedence over
/// the equality generator when both attributes are applied.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class ComparableGenerator : IIncrementalGenerator
{
    /// <summary>Fully-qualified metadata name of the <c>[AutoComparable]</c> marker attribute.</summary>
    private const string AutoComparableAttributeName = "VoloGen.AutoComparableAttribute";
    /// <summary>Fully-qualified type name for <see cref="object"/> used in <c>IComparable.CompareTo(object)</c> checks.</summary>
    private const string ObjectTypeName = "System.Object";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AutoComparableAttributeName,
                predicate: static (node, _) => node is TypeDeclarationSyntax,
                transform: static (ctx, _) => GetTargetInfo(ctx))
            .Where(static t => t is not null)
            .Select(static (t, _) => t!.Value);

        context.RegisterSourceOutput(targets, static (spc, target) => Execute(spc, target));
    }

    /// <summary>
    /// Extracts metadata from the <see cref="GeneratorAttributeSyntaxContext"/> for a type
    /// annotated with <c>[AutoComparable]</c>.
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

        bool hasCompare = HasStaticCompareMethod(typeSymbol);
        bool hasCompareTo = HasInstanceCompareTo(typeSymbol);
        bool hasObjectCompareTo = HasObjectCompareTo(typeSymbol);
        bool hasOperatorLessThan = HasBinaryBoolOperator(typeSymbol, "op_LessThan");
        bool hasOperatorGreaterThan = HasBinaryBoolOperator(typeSymbol, "op_GreaterThan");
        bool hasOperatorLessThanOrEqual = HasBinaryBoolOperator(typeSymbol, "op_LessThanOrEqual");
        bool hasOperatorGreaterThanOrEqual = HasBinaryBoolOperator(typeSymbol, "op_GreaterThanOrEqual");
        bool hasOperatorEquality = HasBinaryBoolOperator(typeSymbol, "op_Equality");
        bool hasOperatorInequality = HasBinaryBoolOperator(typeSymbol, "op_Inequality");

        var attributeLocation = context.Attributes[0].ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? syntax.GetLocation();

        return new TargetInfo(
            typeName,
            fullNamespace,
            typeKeyword,
            isPartial,
            isStatic,
            isValueType,
            hasCompare,
            hasCompareTo,
            hasObjectCompareTo,
            hasOperatorLessThan,
            hasOperatorGreaterThan,
            hasOperatorLessThanOrEqual,
            hasOperatorGreaterThanOrEqual,
            hasOperatorEquality,
            hasOperatorInequality,
            attributeLocation);
    }

    /// <summary>
    /// Checks whether <paramref name="typeSymbol"/> declares a
    /// <c>static int Compare(T, T)</c> method.
    /// </summary>
    private static bool HasStaticCompareMethod(INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers("Compare"))
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (!method.IsStatic
                || method.ReturnType.SpecialType != SpecialType.System_Int32
                || method.Parameters.Length != 2)
            {
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, typeSymbol)
                || !SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, typeSymbol)
                || method.Parameters[0].RefKind != RefKind.None
                || method.Parameters[1].RefKind != RefKind.None)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks whether <paramref name="typeSymbol"/> already declares an
    /// instance <c>int CompareTo(T)</c> method.
    /// </summary>
    private static bool HasInstanceCompareTo(INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers("CompareTo"))
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (method.IsStatic
                || method.ReturnType.SpecialType != SpecialType.System_Int32
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
    /// instance <c>int CompareTo(object)</c> method for non-generic <see cref="System.IComparable"/>.
    /// </summary>
    private static bool HasObjectCompareTo(INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (method.MethodKind == MethodKind.PropertyGet || method.MethodKind == MethodKind.PropertySet)
            {
                continue;
            }

            if (method.ReturnType.SpecialType != SpecialType.System_Int32
                || method.Parameters.Length != 1
                || method.Parameters[0].RefKind != RefKind.None)
            {
                continue;
            }

            if (method.Parameters[0].Type.ToDisplayString() == ObjectTypeName)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether <paramref name="typeSymbol"/> already declares a
    /// <c>static bool operator</c> with the specified metadata name.
    /// </summary>
    /// <param name="typeSymbol">The type to inspect.</param>
    /// <param name="operatorMetadataName">The CLR metadata name (e.g., <c>op_LessThan</c>).</param>
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
    /// Validates the target and emits the generated comparable source file,
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
                "AutoComparable"));
            return;
        }

        if (target.IsStatic)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.CannotBeStatic,
                target.Location,
                target.TypeName,
                "AutoComparable"));
            return;
        }

        if (!target.HasCompare)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.MissingComparableField,
                target.Location,
                target.TypeName));
            return;
        }

        string source = GenerateSource(target);
        if (string.IsNullOrEmpty(source))
        {
            return;
        }

        context.AddSource($"{target.TypeName}.Comparable.g.cs", source);
    }

    /// <summary>
    /// Builds the complete generated source text for the comparable partial declaration.
    /// </summary>
    /// <param name="target">The collected metadata for the annotated type.</param>
    /// <returns>The generated C# source code, or <see cref="string.Empty"/> if all members already exist.</returns>
    private static string GenerateSource(TargetInfo target)
    {
        string nullableAnnotation = GetNullableAnnotation(target);
        string ns = BuildNamespacePrefix(target);
        string generatedTypeHeader = BuildGeneratedTypeHeader(target, ns);
        string nullCheckCompareTo = BuildNullCheckCompareTo(target);

        StringBuilder members = new();

        AppendCompareToIfMissing(target, nullableAnnotation, nullCheckCompareTo, members);
        AppendObjectCompareToIfMissing(target, members);
        AppendOperatorLessThanIfMissing(target, members);
        AppendOperatorGreaterThanIfMissing(target, members);
        AppendOperatorLessThanOrEqualIfMissing(target, members);
        AppendOperatorGreaterThanOrEqualIfMissing(target, members);
        AppendOperatorEqualityIfMissing(target, members);
        AppendOperatorInequalityIfMissing(target, members);

        if (members.Length == 0)
        {
            return string.Empty;
        }

        return generatedTypeHeader + members + "}\n" + BuildThrowHelperClass();
    }

    private static string BuildThrowHelperClass()
    {
        return """

file static class ValuGenThrowHelper
{
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    internal static void ThrowArgumentException(string message, string paramName)
    {
        throw new System.ArgumentException(message, paramName);
    }
}
""";
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

partial {{target.TypeKeyword}} {{target.TypeName}} : System.IComparable<{{target.TypeName}}>, System.IComparable
{
""";
    }

    private static string BuildNullCheckCompareTo(TargetInfo target)
    {
        if (target.IsValueType)
        {
            return string.Empty;
        }

        return """
                if (other is null)
                {
                    return 1;
                }

            """;
    }

    private static void AppendCompareToIfMissing(TargetInfo target, string nullableAnnotation, string nullCheckCompareTo, StringBuilder members)
    {
        if (target.HasCompareTo)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public int CompareTo({{target.TypeName}}{{nullableAnnotation}} other)
    {
{{nullCheckCompareTo}}        return Compare(this, other);
    }
""");
    }

    private static void AppendObjectCompareToIfMissing(TargetInfo target, StringBuilder members)
    {
        if (target.HasObjectCompareToMember)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    int System.IComparable.CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        if (obj is {{target.TypeName}} other)
        {
            return CompareTo(other);
        }

        ValuGenThrowHelper.ThrowArgumentException($"Object must be of type {{target.TypeName}}.", nameof(obj));
        return 0;
    }
""");
    }

    private static void AppendOperatorLessThanIfMissing(TargetInfo target, StringBuilder members)
    {
        if (target.HasOperatorLessThan)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator <({{target.TypeName}} left, {{target.TypeName}} right)
    {
        return Compare(left, right) < 0;
    }
""");
    }

    private static void AppendOperatorGreaterThanIfMissing(TargetInfo target, StringBuilder members)
    {
        if (target.HasOperatorGreaterThan)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator >({{target.TypeName}} left, {{target.TypeName}} right)
    {
        return Compare(left, right) > 0;
    }
""");
    }

    private static void AppendOperatorLessThanOrEqualIfMissing(TargetInfo target, StringBuilder members)
    {
        if (target.HasOperatorLessThanOrEqual)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator <=({{target.TypeName}} left, {{target.TypeName}} right)
    {
        return Compare(left, right) <= 0;
    }
""");
    }

    private static void AppendOperatorGreaterThanOrEqualIfMissing(TargetInfo target, StringBuilder members)
    {
        if (target.HasOperatorGreaterThanOrEqual)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator >=({{target.TypeName}} left, {{target.TypeName}} right)
    {
        return Compare(left, right) >= 0;
    }
""");
    }

    private static void AppendOperatorEqualityIfMissing(TargetInfo target, StringBuilder members)
    {
        if (target.HasOperatorEquality)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator ==({{target.TypeName}} left, {{target.TypeName}} right)
    {
        return Compare(left, right) == 0;
    }
""");
    }

    private static void AppendOperatorInequalityIfMissing(TargetInfo target, StringBuilder members)
    {
        if (target.HasOperatorInequality)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool operator !=({{target.TypeName}} left, {{target.TypeName}} right)
    {
        return Compare(left, right) != 0;
    }
""");
    }

    /// <summary>
    /// Immutable snapshot of all metadata the generator needs about a single
    /// <c>[AutoComparable]</c>-annotated type.
    /// </summary>
    private readonly record struct TargetInfo(
        string TypeName,
        string Namespace,
        string TypeKeyword,
        bool IsPartial,
        bool IsStatic,
        bool IsValueType,
        bool HasCompare,
        bool HasCompareTo,
        bool HasObjectCompareToMember,
        bool HasOperatorLessThan,
        bool HasOperatorGreaterThan,
        bool HasOperatorLessThanOrEqual,
        bool HasOperatorGreaterThanOrEqual,
        bool HasOperatorEquality,
        bool HasOperatorInequality,
        Location Location);
}

