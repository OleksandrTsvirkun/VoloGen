using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Text;

using VoloGen.Common;

namespace VoloGen.Parsable;

/// <summary>
/// Roslyn incremental source generator that processes types annotated with
/// <see cref="AutoParsableAttribute"/> and emits <c>IParsable&lt;TSelf&gt;</c>,
/// <c>ISpanParsable&lt;TSelf&gt;</c>, and optionally
/// <c>IUtf8SpanParsable&lt;TSelf&gt;</c> implementations.
/// </summary>
/// <remarks>
/// <para>
/// Generated <c>Parse</c> and <c>TryParse</c> overloads delegate to the user-supplied
/// <c>static bool TryParse(ReadOnlySpan&lt;char&gt;, IFormatProvider?, out T)</c> core method.
/// </para>
/// <para>
/// When <see cref="AutoParsableAttribute.ImplementExact"/> is set, the generator also
/// emits <c>ParseExact</c> / <c>TryParseExact</c> overloads that delegate to
/// <c>static bool TryParseExact(ReadOnlySpan&lt;char&gt;, ReadOnlySpan&lt;char&gt;, IFormatProvider?, out T)</c>.
/// </para>
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class ParsableGenerator : IIncrementalGenerator
{
    /// <summary>Fully-qualified metadata name of the <c>[AutoParsable]</c> marker attribute.</summary>
    private const string AutoParsableAttributeName = "VoloGen.AutoParsableAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AutoParsableAttributeName,
                predicate: static (node, _) => node is TypeDeclarationSyntax,
                transform: static (ctx, _) => GetTargetInfo(ctx))
            .Where(static t => t is not null)
            .Select(static (t, _) => t!.Value);

        context.RegisterSourceOutput(targets, static (spc, target) => Execute(spc, target));
    }

    /// <summary>
    /// Extracts metadata from the <see cref="GeneratorAttributeSyntaxContext"/> for a type
    /// annotated with <c>[AutoParsable]</c>.
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
        bool isAbstract = typeSymbol.IsAbstract;
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

        bool implementUtf8 = GetNamedBoolArgument(context.Attributes[0], "ImplementUtf8");
        bool implementExact = GetNamedBoolArgument(context.Attributes[0], "ImplementExact");

        string typeDisplayName = typeSymbol.ToDisplayString();

        bool hasTryParse = HasTryParseSpanProvider(typeSymbol, typeDisplayName);
        bool hasTryParseSpanNoProvider = HasTryParseSpanNoProvider(typeSymbol, typeDisplayName);
        bool hasTryParseStringProvider = HasTryParseStringProvider(typeSymbol, typeDisplayName);
        bool hasTryParseStringNoProvider = HasTryParseStringNoProvider(typeSymbol, typeDisplayName);
        bool hasParseStringProvider = HasParseStringProvider(typeSymbol);
        bool hasParseSpanProvider = HasParseSpanProvider(typeSymbol);
        bool hasParseStringNoProvider = HasParseStringNoProvider(typeSymbol);
        bool hasParseSpanNoProvider = HasParseSpanNoProvider(typeSymbol);

        bool hasTryParseExactCore = HasTryParseExactSpanProvider(typeSymbol, typeDisplayName);
        bool hasTryParseExactSpanNoProvider = HasTryParseExactSpanNoProvider(typeSymbol, typeDisplayName);
        bool hasTryParseExactStringProvider = HasTryParseExactStringProvider(typeSymbol, typeDisplayName);
        bool hasTryParseExactStringNoProvider = HasTryParseExactStringNoProvider(typeSymbol, typeDisplayName);
        bool hasParseExactStringProvider = HasParseExactStringProvider(typeSymbol);
        bool hasParseExactSpanProvider = HasParseExactSpanProvider(typeSymbol);
        bool hasParseExactStringNoProvider = HasParseExactStringNoProvider(typeSymbol);
        bool hasParseExactSpanNoProvider = HasParseExactSpanNoProvider(typeSymbol);

        bool hasTryParseUtf8Provider = HasTryParseUtf8Provider(typeSymbol, typeDisplayName);
        bool hasTryParseUtf8NoProvider = HasTryParseUtf8NoProvider(typeSymbol, typeDisplayName);
        bool hasParseUtf8Provider = HasParseUtf8Provider(typeSymbol);
        bool hasParseUtf8NoProvider = HasParseUtf8NoProvider(typeSymbol);

        var attributeLocation = context.Attributes[0].ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? syntax.GetLocation();

        return new TargetInfo(
            typeName,
            fullNamespace,
            typeKeyword,
            isPartial,
            isStatic,
            isAbstract,
            implementUtf8,
            implementExact,
            hasTryParse,
            hasTryParseSpanNoProvider,
            hasTryParseStringProvider,
            hasTryParseStringNoProvider,
            hasParseStringProvider,
            hasParseSpanProvider,
            hasParseStringNoProvider,
            hasParseSpanNoProvider,
            hasTryParseExactCore,
            hasTryParseExactSpanNoProvider,
            hasTryParseExactStringProvider,
            hasTryParseExactStringNoProvider,
            hasParseExactStringProvider,
            hasParseExactSpanProvider,
            hasParseExactStringNoProvider,
            hasParseExactSpanNoProvider,
            hasTryParseUtf8Provider,
            hasTryParseUtf8NoProvider,
            hasParseUtf8Provider,
            hasParseUtf8NoProvider,
            attributeLocation);
    }

    /// <summary>
    /// Reads a named <see cref="bool"/> argument from an <see cref="AttributeData"/>.
    /// </summary>
    /// <param name="attribute">The attribute data to inspect.</param>
    /// <param name="name">The name of the argument to read.</param>
    /// <returns>The argument value, or <see langword="false"/> if not found.</returns>
    private static bool GetNamedBoolArgument(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (argument.Key == name && argument.Value.Value is bool value)
            {
                return value;
            }
        }

        return false;
    }

    private static bool HasTryParseSpanProvider(INamedTypeSymbol typeSymbol, string typeName)
    {
        return HasBoolMethod(
            typeSymbol,
            "TryParse",
            [
                ("System.ReadOnlySpan<char>", RefKind.None),
                ("System.IFormatProvider", RefKind.None),
                (typeName, RefKind.Out)
            ]);
    }

    private static bool HasTryParseSpanNoProvider(INamedTypeSymbol typeSymbol, string typeName)
    {
        return HasBoolMethod(
            typeSymbol,
            "TryParse",
            [
                ("System.ReadOnlySpan<char>", RefKind.None),
                (typeName, RefKind.Out)
            ]);
    }

    private static bool HasTryParseStringProvider(INamedTypeSymbol typeSymbol, string typeName)
    {
        return HasBoolMethod(
            typeSymbol,
            "TryParse",
            [
                ("System.String", RefKind.None),
                ("System.IFormatProvider", RefKind.None),
                (typeName, RefKind.Out)
            ]);
    }

    private static bool HasTryParseStringNoProvider(INamedTypeSymbol typeSymbol, string typeName)
    {
        return HasBoolMethod(
            typeSymbol,
            "TryParse",
            [
                ("System.String", RefKind.None),
                (typeName, RefKind.Out)
            ]);
    }

    private static bool HasParseStringProvider(INamedTypeSymbol typeSymbol)
    {
        return HasTypeReturningMethod(
            typeSymbol,
            "Parse",
            [
                ("System.String", RefKind.None),
                ("System.IFormatProvider", RefKind.None)
            ]);
    }

    private static bool HasParseSpanProvider(INamedTypeSymbol typeSymbol)
    {
        return HasTypeReturningMethod(
            typeSymbol,
            "Parse",
            [
                ("System.ReadOnlySpan<char>", RefKind.None),
                ("System.IFormatProvider", RefKind.None)
            ]);
    }

    private static bool HasParseStringNoProvider(INamedTypeSymbol typeSymbol)
    {
        return HasTypeReturningMethod(
            typeSymbol,
            "Parse",
            [
                ("System.String", RefKind.None)
            ]);
    }

    private static bool HasParseSpanNoProvider(INamedTypeSymbol typeSymbol)
    {
        return HasTypeReturningMethod(
            typeSymbol,
            "Parse",
            [
                ("System.ReadOnlySpan<char>", RefKind.None)
            ]);
    }

    private static bool HasTryParseExactSpanProvider(INamedTypeSymbol typeSymbol, string typeName)
    {
        return HasBoolMethod(
            typeSymbol,
            "TryParseExact",
            [
                ("System.ReadOnlySpan<char>", RefKind.None),
                ("System.ReadOnlySpan<char>", RefKind.None),
                ("System.IFormatProvider", RefKind.None),
                (typeName, RefKind.Out)
            ]);
    }

    private static bool HasTryParseExactSpanNoProvider(INamedTypeSymbol typeSymbol, string typeName)
    {
        return HasBoolMethod(
            typeSymbol,
            "TryParseExact",
            [
                ("System.ReadOnlySpan<char>", RefKind.None),
                ("System.ReadOnlySpan<char>", RefKind.None),
                (typeName, RefKind.Out)
            ]);
    }

    private static bool HasTryParseExactStringProvider(INamedTypeSymbol typeSymbol, string typeName)
    {
        return HasBoolMethod(
            typeSymbol,
            "TryParseExact",
            [
                ("System.String", RefKind.None),
                ("System.ReadOnlySpan<char>", RefKind.None),
                ("System.IFormatProvider", RefKind.None),
                (typeName, RefKind.Out)
            ]);
    }

    private static bool HasTryParseExactStringNoProvider(INamedTypeSymbol typeSymbol, string typeName)
    {
        return HasBoolMethod(
            typeSymbol,
            "TryParseExact",
            [
                ("System.String", RefKind.None),
                ("System.ReadOnlySpan<char>", RefKind.None),
                (typeName, RefKind.Out)
            ]);
    }

    private static bool HasParseExactStringProvider(INamedTypeSymbol typeSymbol)
    {
        return HasTypeReturningMethod(
            typeSymbol,
            "ParseExact",
            [
                ("System.String", RefKind.None),
                ("System.ReadOnlySpan<char>", RefKind.None),
                ("System.IFormatProvider", RefKind.None)
            ]);
    }

    private static bool HasParseExactSpanProvider(INamedTypeSymbol typeSymbol)
    {
        return HasTypeReturningMethod(
            typeSymbol,
            "ParseExact",
            [
                ("System.ReadOnlySpan<char>", RefKind.None),
                ("System.ReadOnlySpan<char>", RefKind.None),
                ("System.IFormatProvider", RefKind.None)
            ]);
    }

    private static bool HasParseExactStringNoProvider(INamedTypeSymbol typeSymbol)
    {
        return HasTypeReturningMethod(
            typeSymbol,
            "ParseExact",
            [
                ("System.String", RefKind.None),
                ("System.ReadOnlySpan<char>", RefKind.None)
            ]);
    }

    private static bool HasParseExactSpanNoProvider(INamedTypeSymbol typeSymbol)
    {
        return HasTypeReturningMethod(
            typeSymbol,
            "ParseExact",
            [
                ("System.ReadOnlySpan<char>", RefKind.None),
                ("System.ReadOnlySpan<char>", RefKind.None)
            ]);
    }

    private static bool HasTryParseUtf8Provider(INamedTypeSymbol typeSymbol, string typeName)
    {
        return HasBoolMethod(
            typeSymbol,
            "TryParse",
            [
                ("System.ReadOnlySpan<byte>", RefKind.None),
                ("System.IFormatProvider", RefKind.None),
                (typeName, RefKind.Out)
            ]);
    }

    private static bool HasTryParseUtf8NoProvider(INamedTypeSymbol typeSymbol, string typeName)
    {
        return HasBoolMethod(
            typeSymbol,
            "TryParse",
            [
                ("System.ReadOnlySpan<byte>", RefKind.None),
                (typeName, RefKind.Out)
            ]);
    }

    private static bool HasParseUtf8Provider(INamedTypeSymbol typeSymbol)
    {
        return HasTypeReturningMethod(
            typeSymbol,
            "Parse",
            [
                ("System.ReadOnlySpan<byte>", RefKind.None),
                ("System.IFormatProvider", RefKind.None)
            ]);
    }

    private static bool HasParseUtf8NoProvider(INamedTypeSymbol typeSymbol)
    {
        return HasTypeReturningMethod(
            typeSymbol,
            "Parse",
            [
                ("System.ReadOnlySpan<byte>", RefKind.None)
            ]);
    }

    /// <summary>
    /// Checks whether <paramref name="typeSymbol"/> declares a static method with the given name,
    /// a <see cref="bool"/> return type, and the specified parameter signature.
    /// </summary>
    /// <param name="typeSymbol">The type to inspect.</param>
    /// <param name="methodName">The method name to search for.</param>
    /// <param name="parameterTypes">Expected parameter type names and ref kinds.</param>
    private static bool HasBoolMethod(
        INamedTypeSymbol typeSymbol,
        string methodName,
        ReadOnlySpan<(string TypeName, RefKind RefKind)> parameterTypes)
    {
        foreach (var member in typeSymbol.GetMembers(methodName))
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (!method.IsStatic
                || method.ReturnType.SpecialType != SpecialType.System_Boolean
                || method.Parameters.Length != parameterTypes.Length)
            {
                continue;
            }

            if (AreParametersMatching(method, parameterTypes))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether <paramref name="typeSymbol"/> declares a static method with the given name
    /// that returns the type itself, with the specified parameter signature.
    /// </summary>
    /// <param name="typeSymbol">The type to inspect.</param>
    /// <param name="methodName">The method name to search for.</param>
    /// <param name="parameterTypes">Expected parameter type names and ref kinds.</param>
    private static bool HasTypeReturningMethod(
        INamedTypeSymbol typeSymbol,
        string methodName,
        ReadOnlySpan<(string TypeName, RefKind RefKind)> parameterTypes)
    {
        foreach (var member in typeSymbol.GetMembers(methodName))
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (!method.IsStatic
                || !SymbolEqualityComparer.Default.Equals(method.ReturnType, typeSymbol)
                || method.Parameters.Length != parameterTypes.Length)
            {
                continue;
            }

            if (AreParametersMatching(method, parameterTypes))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates that each parameter of <paramref name="method"/> matches the expected type name
    /// and <see cref="RefKind"/>. Handles nullable <see cref="System.IFormatProvider"/> matching.
    /// </summary>
    private static bool AreParametersMatching(
        IMethodSymbol method,
        ReadOnlySpan<(string TypeName, RefKind RefKind)> parameterTypes)
    {
        for (int i = 0; i < parameterTypes.Length; i++)
        {
            if (method.Parameters[i].RefKind != parameterTypes[i].RefKind)
            {
                return false;
            }

            if (parameterTypes[i].TypeName == "System.IFormatProvider")
            {
                string display = method.Parameters[i].Type.ToDisplayString();
                if (display != "System.IFormatProvider" && display != "System.IFormatProvider?")
                {
                    return false;
                }

                continue;
            }

            if (method.Parameters[i].Type.ToDisplayString() != parameterTypes[i].TypeName)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Validates the target and emits the generated parsable source file,
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
                "AutoParsable"));
            return;
        }

        if (target.IsStatic)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.CannotBeStatic,
                target.Location,
                target.TypeName,
                "AutoParsable"));
            return;
        }

        if (target.IsAbstract)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.CannotBeAbstract,
                target.Location,
                target.TypeName,
                "AutoParsable"));
            return;
        }

        if (!target.HasTryParseCore)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.MissingTryParseMethod,
                target.Location,
                target.TypeName));
            return;
        }

        if (target.ImplementExact && !target.HasTryParseExactCore)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.MissingTryParseMethod,
                target.Location,
                target.TypeName));
            return;
        }

        string source = GenerateSource(target);
        if (string.IsNullOrEmpty(source))
        {
            return;
        }

        context.AddSource($"{target.TypeName}.Parsable.g.cs", source);
    }

    /// <summary>
    /// Builds the complete generated source text for the parsable partial declaration,
    /// including standard, exact-format, and UTF-8 overloads as configured.
    /// </summary>
    /// <param name="target">The collected metadata for the annotated type.</param>
    /// <returns>The generated C# source code, or <see cref="string.Empty"/> if all members already exist.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3776", Justification = "Source-template assembly requires explicit conditional method emission blocks.")]
    private static string GenerateSource(TargetInfo target)
    {
        string ns = string.Empty;
        if (!string.IsNullOrEmpty(target.Namespace))
        {
            ns = $"namespace {target.Namespace};\n\n";
        }

        string utf8Interface = string.Empty;
        if (target.ImplementUtf8)
        {
            utf8Interface = $", System.IUtf8SpanParsable<{target.TypeName}>";
        }

        StringBuilder members = new();

        string invalidFormatMessage = ExceptionMessages.GetInvalidFormatMessage(target.TypeName);

        if (!target.HasExistingParseStringProvider)
        {
            members.AppendLine($$"""
    public static {{target.TypeName}} Parse(string s, System.IFormatProvider? provider)
    {
        if (s is null)
        {
            ValuGenThrowHelper.ThrowArgumentNullException(nameof(s));
        }

        if (!TryParse(s.AsSpan(), provider, out var result))
        {
            ValuGenThrowHelper.ThrowFormatException("{{invalidFormatMessage}}");
        }

        return result;
    }
""");
        }

        if (!target.HasExistingParseSpanProvider)
        {
            members.AppendLine($$"""
    public static {{target.TypeName}} Parse(System.ReadOnlySpan<char> s, System.IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var result))
        {
            ValuGenThrowHelper.ThrowFormatException("{{invalidFormatMessage}}");
        }

        return result;
    }
""");
        }

        if (!target.HasExistingTryParseStringProvider)
        {
            members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(string? s, System.IFormatProvider? provider, out {{target.TypeName}} result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }
""");
        }

        if (!target.HasExistingParseStringNoProvider)
        {
            members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static {{target.TypeName}} Parse(string s)
    {
        return Parse(s, null);
    }
""");
        }

        if (!target.HasExistingParseSpanNoProvider)
        {
            members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static {{target.TypeName}} Parse(System.ReadOnlySpan<char> s)
    {
        return Parse(s, null);
    }
""");
        }

        if (!target.HasExistingTryParseStringNoProvider)
        {
            members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(string? s, out {{target.TypeName}} result)
    {
        return TryParse(s, null, out result);
    }
""");
        }

        if (!target.HasExistingTryParseSpanNoProvider)
        {
            members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(System.ReadOnlySpan<char> s, out {{target.TypeName}} result)
    {
        return TryParse(s, null, out result);
    }
""");
        }

        if (target.ImplementExact && !target.HasExistingParseExactStringProvider)
        {
            members.AppendLine($$"""
    public static {{target.TypeName}} ParseExact(string s, System.ReadOnlySpan<char> format, System.IFormatProvider? provider)
    {
        if (s is null)
        {
            ValuGenThrowHelper.ThrowArgumentNullException(nameof(s));
        }

        if (!TryParseExact(s.AsSpan(), format, provider, out var result))
        {
            ValuGenThrowHelper.ThrowFormatException("{{invalidFormatMessage}}");
        }

        return result;
    }
""");
        }

        if (target.ImplementExact && !target.HasExistingParseExactSpanProvider)
        {
            members.AppendLine($$"""
    public static {{target.TypeName}} ParseExact(System.ReadOnlySpan<char> s, System.ReadOnlySpan<char> format, System.IFormatProvider? provider)
    {
        if (!TryParseExact(s, format, provider, out var result))
        {
            ValuGenThrowHelper.ThrowFormatException("{{invalidFormatMessage}}");
        }

        return result;
    }
""");
        }

        if (target.ImplementExact && !target.HasExistingTryParseExactStringProvider)
        {
            members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool TryParseExact(string? s, System.ReadOnlySpan<char> format, System.IFormatProvider? provider, out {{target.TypeName}} result)
    {
        return TryParseExact(s.AsSpan(), format, provider, out result);
    }
""");
        }

        if (target.ImplementExact && !target.HasExistingParseExactStringNoProvider)
        {
            members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static {{target.TypeName}} ParseExact(string s, System.ReadOnlySpan<char> format)
    {
        return ParseExact(s, format, null);
    }
""");
        }

        if (target.ImplementExact && !target.HasExistingParseExactSpanNoProvider)
        {
            members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static {{target.TypeName}} ParseExact(System.ReadOnlySpan<char> s, System.ReadOnlySpan<char> format)
    {
        return ParseExact(s, format, null);
    }
""");
        }

        if (target.ImplementExact && !target.HasExistingTryParseExactStringNoProvider)
        {
            members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool TryParseExact(string? s, System.ReadOnlySpan<char> format, out {{target.TypeName}} result)
    {
        return TryParseExact(s, format, null, out result);
    }
""");
        }

        if (target.ImplementExact && !target.HasExistingTryParseExactSpanNoProvider)
        {
            members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool TryParseExact(System.ReadOnlySpan<char> s, System.ReadOnlySpan<char> format, out {{target.TypeName}} result)
    {
        return TryParseExact(s, format, null, out result);
    }
""");
        }

        if (target.ImplementUtf8 && !target.HasExistingTryParseUtf8Provider)
        {
            members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(System.ReadOnlySpan<byte> utf8Text, System.IFormatProvider? provider, out {{target.TypeName}} result)
    {
        return TryParse(System.Text.Encoding.UTF8.GetString(utf8Text).AsSpan(), provider, out result);
    }
""");
        }

        if (target.ImplementUtf8 && !target.HasExistingParseUtf8Provider)
        {
            members.AppendLine($$"""
    public static {{target.TypeName}} Parse(System.ReadOnlySpan<byte> utf8Text, System.IFormatProvider? provider)
    {
        if (!TryParse(utf8Text, provider, out var result))
        {
            ValuGenThrowHelper.ThrowFormatException("{{invalidFormatMessage}}");
        }

        return result;
    }
""");
        }

        if (target.ImplementUtf8 && !target.HasExistingTryParseUtf8NoProvider)
        {
            members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(System.ReadOnlySpan<byte> utf8Text, out {{target.TypeName}} result)
    {
        return TryParse(utf8Text, null, out result);
    }
""");
        }

        if (target.ImplementUtf8 && !target.HasExistingParseUtf8NoProvider)
        {
            members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static {{target.TypeName}} Parse(System.ReadOnlySpan<byte> utf8Text)
    {
        return Parse(utf8Text, null);
    }
""");
        }

        if (members.Length == 0)
        {
            return string.Empty;
        }

        string generatedSource = $$"""
// <auto-generated/>
#nullable enable
using System;

{{ns}}partial {{target.TypeKeyword}} {{target.TypeName}} : System.IParsable<{{target.TypeName}}>, System.ISpanParsable<{{target.TypeName}}>{{utf8Interface}}
{
{{members}}
}
""";

        return generatedSource + BuildThrowHelperClass();
    }

    private static string BuildThrowHelperClass()
    {
        return """

file static class ValuGenThrowHelper
{
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    internal static void ThrowArgumentNullException(string paramName)
    {
        throw new System.ArgumentNullException(paramName);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    internal static void ThrowFormatException(string message)
    {
        throw new System.FormatException(message);
    }
}
""";
    }

    /// <summary>
    /// Immutable snapshot of all metadata the generator needs about a single
    /// <c>[AutoParsable]</c>-annotated type, including feature flags and existing method presence.
    /// </summary>
    private readonly record struct TargetInfo(
        string TypeName,
        string Namespace,
        string TypeKeyword,
        bool IsPartial,
        bool IsStatic,
        bool IsAbstract,
        bool ImplementUtf8,
        bool ImplementExact,
        bool HasTryParseCore,
        bool HasExistingTryParseSpanNoProvider,
        bool HasExistingTryParseStringProvider,
        bool HasExistingTryParseStringNoProvider,
        bool HasExistingParseStringProvider,
        bool HasExistingParseSpanProvider,
        bool HasExistingParseStringNoProvider,
        bool HasExistingParseSpanNoProvider,
        bool HasTryParseExactCore,
        bool HasExistingTryParseExactSpanNoProvider,
        bool HasExistingTryParseExactStringProvider,
        bool HasExistingTryParseExactStringNoProvider,
        bool HasExistingParseExactStringProvider,
        bool HasExistingParseExactSpanProvider,
        bool HasExistingParseExactStringNoProvider,
        bool HasExistingParseExactSpanNoProvider,
        bool HasExistingTryParseUtf8Provider,
        bool HasExistingTryParseUtf8NoProvider,
        bool HasExistingParseUtf8Provider,
        bool HasExistingParseUtf8NoProvider,
        Location Location);
}

