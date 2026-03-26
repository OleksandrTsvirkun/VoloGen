using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using VoloGen.Common;

namespace VoloGen.Formattable;

/// <summary>
/// Roslyn incremental source generator that processes types annotated with
/// <see cref="AutoFormattableAttribute"/> and emits <see cref="System.IFormattable"/>,
/// <c>ISpanFormattable</c>, and optionally
/// <c>IUtf8SpanFormattable</c> implementations.
/// </summary>
/// <remarks>
/// <para>
/// Generated <c>TryFormat</c> and <c>ToString</c> overloads delegate to the user-supplied
/// <c>bool TryFormat(Span&lt;char&gt;, out int, ReadOnlySpan&lt;char&gt;, IFormatProvider?)</c> core method.
/// </para>
/// <para>
/// <c>ToString</c> generation requires either a user-defined
/// <c>string ToString(string?, IFormatProvider?)</c> method <b>or</b> a
/// <c>const int MaxBufferSize</c> field; the latter enables an auto-generated stackalloc /
/// <see cref="System.Buffers.ArrayPool{T}"/>-backed implementation.
/// </para>
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class FormattableGenerator : IIncrementalGenerator
{
    /// <summary>Fully-qualified metadata name of the <c>[AutoFormattable]</c> marker attribute.</summary>
    private const string AutoFormattableAttributeName = "VoloGen.AutoFormattableAttribute";
    /// <summary>Constant name looked up on the target type to determine buffer sizing for <c>ToString</c> generation.</summary>
    private const string MaxBufferSizeConstantName = "MaxBufferSize";
    /// <summary>Maximum buffer size (in chars) that may be allocated on the stack via <c>stackalloc</c>.</summary>
    private const int StackallocThreshold = 256;

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AutoFormattableAttributeName,
                predicate: static (node, _) => node is TypeDeclarationSyntax,
                transform: static (ctx, _) => GetTargetInfo(ctx))
            .Where(static t => t is not null)
            .Select(static (t, _) => t!.Value);

        context.RegisterSourceOutput(targets, static (spc, target) => Execute(spc, target));
    }

    /// <summary>
    /// Extracts metadata from the <see cref="GeneratorAttributeSyntaxContext"/> for a type
    /// annotated with <c>[AutoFormattable]</c>.
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
        string? defaultFormat = GetNamedStringArgument(context.Attributes[0], "DefaultFormat");
        bool allowNullFormatProvider = GetNamedBoolArgument(context.Attributes[0], "AllowNullFormatProvider", true);

        bool hasTryFormat = HasTryFormatCharProvider(typeSymbol);
        bool hasTryFormatCharFormatOnly = HasTryFormatCharFormatOnly(typeSymbol);
        bool hasTryFormatCharProviderOnly = HasTryFormatCharProviderOnly(typeSymbol);
        bool hasTryFormatCharNoFormatNoProvider = HasTryFormatCharNoFormatNoProvider(typeSymbol);
        bool hasToStringFormatProvider = HasToStringFormatProvider(typeSymbol);
        bool hasToStringProviderOnly = HasToStringProviderOnly(typeSymbol);
        bool hasToStringFormatOnly = HasToStringFormatOnly(typeSymbol);
        bool hasToStringNoArgs = HasToStringNoArgs(typeSymbol);
        bool hasTryFormatUtf8Provider = HasTryFormatUtf8Provider(typeSymbol);
        bool hasTryFormatUtf8FormatOnly = HasTryFormatUtf8FormatOnly(typeSymbol);
        bool hasTryFormatUtf8ProviderOnly = HasTryFormatUtf8ProviderOnly(typeSymbol);
        bool hasTryFormatUtf8NoArgs = HasTryFormatUtf8NoArgs(typeSymbol);
        var (hasMaxBufferSizeConstant, maxBufferSizeConstantValue) = GetMaxBufferSizeConstant(typeSymbol);

        var attributeLocation = context.Attributes[0].ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? syntax.GetLocation();

        return new TargetInfo(
            typeName,
            fullNamespace,
            typeKeyword,
            isPartial,
            isStatic,
            hasTryFormat,
            hasTryFormatCharFormatOnly,
            hasTryFormatCharProviderOnly,
            hasTryFormatCharNoFormatNoProvider,
            hasToStringFormatProvider,
            hasToStringProviderOnly,
            hasToStringFormatOnly,
            hasToStringNoArgs,
            implementUtf8,
            defaultFormat,
            allowNullFormatProvider,
            hasMaxBufferSizeConstant,
            maxBufferSizeConstantValue,
            hasTryFormatUtf8Provider,
            hasTryFormatUtf8FormatOnly,
            hasTryFormatUtf8ProviderOnly,
            hasTryFormatUtf8NoArgs,
            attributeLocation);
    }

    /// <summary>
    /// Reads a named <see cref="bool"/> argument from an <see cref="AttributeData"/>.
    /// </summary>
    /// <param name="attribute">The attribute data to inspect.</param>
    /// <param name="name">The name of the argument to read.</param>
    /// <param name="defaultValue">Value returned when the argument is not present.</param>
    /// <returns>The argument value, or <paramref name="defaultValue"/> if not found.</returns>
    private static bool GetNamedBoolArgument(AttributeData attribute, string name, bool defaultValue = false)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (argument.Key == name && argument.Value.Value is bool value)
            {
                return value;
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Reads a named <see cref="string"/> argument from an <see cref="AttributeData"/>.
    /// </summary>
    /// <param name="attribute">The attribute data to inspect.</param>
    /// <param name="name">The name of the argument to read.</param>
    /// <returns>The argument value, or <see langword="null"/> if not found.</returns>
    private static string? GetNamedStringArgument(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (argument.Key == name)
            {
                return argument.Value.Value as string;
            }
        }

        return null;
    }

    private static bool HasTryFormatCharProvider(INamedTypeSymbol typeSymbol)
    {
        return HasInstanceTryFormat(typeSymbol, "System.Span<char>", hasFormat: true, hasProvider: true);
    }

    private static bool HasTryFormatCharFormatOnly(INamedTypeSymbol typeSymbol)
    {
        return HasInstanceTryFormat(typeSymbol, "System.Span<char>", hasFormat: true, hasProvider: false);
    }

    private static bool HasTryFormatCharProviderOnly(INamedTypeSymbol typeSymbol)
    {
        return HasInstanceTryFormat(typeSymbol, "System.Span<char>", hasFormat: false, hasProvider: true);
    }

    private static bool HasTryFormatCharNoFormatNoProvider(INamedTypeSymbol typeSymbol)
    {
        return HasInstanceTryFormat(typeSymbol, "System.Span<char>", hasFormat: false, hasProvider: false);
    }

    private static bool HasTryFormatUtf8Provider(INamedTypeSymbol typeSymbol)
    {
        return HasInstanceTryFormat(typeSymbol, "System.Span<byte>", hasFormat: true, hasProvider: true);
    }

    private static bool HasTryFormatUtf8FormatOnly(INamedTypeSymbol typeSymbol)
    {
        return HasInstanceTryFormat(typeSymbol, "System.Span<byte>", hasFormat: true, hasProvider: false);
    }

    private static bool HasTryFormatUtf8ProviderOnly(INamedTypeSymbol typeSymbol)
    {
        return HasInstanceTryFormat(typeSymbol, "System.Span<byte>", hasFormat: false, hasProvider: true);
    }

    private static bool HasTryFormatUtf8NoArgs(INamedTypeSymbol typeSymbol)
    {
        return HasInstanceTryFormat(typeSymbol, "System.Span<byte>", hasFormat: false, hasProvider: false);
    }

    /// <summary>
    /// Looks for a <c>const int MaxBufferSize</c> field on the target type and returns its value.
    /// </summary>
    /// <param name="typeSymbol">The type to inspect.</param>
    /// <returns>A tuple indicating whether the constant exists and its positive integer value.</returns>
    private static (bool HasValue, int Value) GetMaxBufferSizeConstant(INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers(MaxBufferSizeConstantName))
        {
            if (member is not IFieldSymbol field)
            {
                continue;
            }

            if (!field.IsConst || field.Type.SpecialType != SpecialType.System_Int32)
            {
                continue;
            }

            if (field.ConstantValue is not int value || value <= 0)
            {
                continue;
            }

            return (true, value);
        }

        return (false, 0);
    }

    private static bool HasInstanceTryFormat(INamedTypeSymbol typeSymbol, string destinationType, bool hasFormat, bool hasProvider)
    {
        foreach (var member in typeSymbol.GetMembers("TryFormat"))
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (IsValidTryFormatSignature(method, destinationType, hasFormat, hasProvider))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsValidTryFormatSignature(IMethodSymbol method, string destinationType, bool hasFormat, bool hasProvider)
    {
        int expectedParameters = 2 + (hasFormat ? 1 : 0) + (hasProvider ? 1 : 0);
        if (method.IsStatic || method.ReturnType.SpecialType != SpecialType.System_Boolean || method.Parameters.Length != expectedParameters)
        {
            return false;
        }

        if (method.Parameters[0].Type.ToDisplayString() != destinationType)
        {
            return false;
        }

        if (method.Parameters[1].RefKind != RefKind.Out || method.Parameters[1].Type.SpecialType != SpecialType.System_Int32)
        {
            return false;
        }

        int index = 2;
        if (hasFormat)
        {
            if (method.Parameters[index].Type.ToDisplayString() != "System.ReadOnlySpan<char>")
            {
                return false;
            }

            index++;
        }

        if (hasProvider)
        {
            string providerDisplay = method.Parameters[index].Type.ToDisplayString();
            if (providerDisplay != "System.IFormatProvider" && providerDisplay != "System.IFormatProvider?")
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasToStringFormatProvider(INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers("ToString"))
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (method.IsStatic
                || method.ReturnType.SpecialType != SpecialType.System_String
                || method.Parameters.Length != 2
                || method.Parameters[0].Type.SpecialType != SpecialType.System_String)
            {
                continue;
            }

            string providerDisplay = method.Parameters[1].Type.ToDisplayString();
            if (providerDisplay == "System.IFormatProvider" || providerDisplay == "System.IFormatProvider?")
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasToStringProviderOnly(INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers("ToString"))
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (method.IsStatic
                || method.ReturnType.SpecialType != SpecialType.System_String
                || method.Parameters.Length != 1)
            {
                continue;
            }

            string providerDisplay = method.Parameters[0].Type.ToDisplayString();
            if (providerDisplay == "System.IFormatProvider" || providerDisplay == "System.IFormatProvider?")
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasToStringFormatOnly(INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers("ToString"))
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (!method.IsStatic
                && method.ReturnType.SpecialType == SpecialType.System_String
                && method.Parameters.Length == 1
                && method.Parameters[0].Type.SpecialType == SpecialType.System_String)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasToStringNoArgs(INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers("ToString"))
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (!method.IsStatic
                && method.ReturnType.SpecialType == SpecialType.System_String
                && method.Parameters.Length == 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates the target and emits the generated formattable source file,
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
                "AutoFormattable"));
            return;
        }

        if (target.IsStatic)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.CannotBeStatic,
                target.Location,
                target.TypeName,
                "AutoFormattable"));
            return;
        }

        if (!target.HasTryFormat)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.MissingTryFormatMethod,
                target.Location,
                target.TypeName));
            return;
        }

        if (!target.HasExistingToStringFormatProvider && !target.HasMaxBufferSizeConstant)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.MissingToStringOrMaxBufferSize,
                target.Location,
                target.TypeName));
            return;
        }

        string source = GenerateSource(target);
        if (string.IsNullOrEmpty(source))
        {
            return;
        }

        context.AddSource($"{target.TypeName}.Formattable.g.cs", source);
    }

    /// <summary>
    /// Builds the complete generated source text for the formattable partial declaration,
    /// including char, UTF-8, and ToString overloads as configured.
    /// </summary>
    /// <param name="target">The collected metadata for the annotated type.</param>
    /// <returns>The generated C# source code, or <see cref="string.Empty"/> if all members already exist.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3776", Justification = "Source-template assembly requires explicit conditional method emission blocks.")]
    private static string GenerateSource(TargetInfo target)
    {
        string ns = BuildNamespacePrefix(target);
        string utf8Interface = BuildUtf8InterfaceSuffix(target);
        string defaultFormatLiteral = BuildDefaultFormatLiteral(target);
        string defaultFormatSpan = BuildDefaultFormatSpan(target);
        string providerGuard = BuildProviderGuard(target);
        string generatedTypeHeader = BuildGeneratedTypeHeader(target, ns, utf8Interface);
        bool hasToStringCore = HasToStringCore(target);

        StringBuilder members = new();

        AppendToStringFormatProviderIfMissing(target, providerGuard, members);
        AppendTryFormatCharFormatIfMissing(target, members);
        AppendTryFormatCharProviderIfMissing(target, defaultFormatSpan, providerGuard, members);
        AppendTryFormatCharNoArgsIfMissing(target, members);
        AppendToStringProviderIfMissing(target, defaultFormatLiteral, hasToStringCore, members);
        AppendToStringFormatIfMissing(target, hasToStringCore, members);
        AppendToStringNoArgsIfMissing(target, defaultFormatLiteral, hasToStringCore, members);
        AppendTryFormatUtf8ProviderIfMissing(target, providerGuard, hasToStringCore, members);
        AppendTryFormatUtf8FormatIfMissing(target, members);
        AppendTryFormatUtf8ProviderOnlyIfMissing(target, defaultFormatSpan, providerGuard, members);
        AppendTryFormatUtf8NoArgsIfMissing(target, members);

        if (members.Length == 0)
        {
            return string.Empty;
        }

        string throwHelper = string.Empty;
        if (!target.AllowNullFormatProvider)
        {
            throwHelper = BuildThrowHelperClass();
        }

        return generatedTypeHeader + members + "}\n" + throwHelper;
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
}
""";
    }

    private static bool HasToStringCore(TargetInfo target)
    {
        if (target.HasExistingToStringFormatProvider)
        {
            return true;
        }

        if (target.HasMaxBufferSizeConstant)
        {
            return true;
        }

        return false;
    }

    private static string BuildNamespacePrefix(TargetInfo target)
    {
        if (string.IsNullOrEmpty(target.Namespace))
        {
            return string.Empty;
        }

        return $"namespace {target.Namespace};";
    }

    private static string BuildUtf8InterfaceSuffix(TargetInfo target)
    {
        if (!target.ImplementUtf8)
        {
            return string.Empty;
        }

        return ", System.IUtf8SpanFormattable";
    }

    private static string BuildDefaultFormatLiteral(TargetInfo target)
    {
        if (target.DefaultFormat is null)
        {
            return "null";
        }

        return $"\"{EscapeString(target.DefaultFormat)}\"";
    }

    private static string BuildDefaultFormatSpan(TargetInfo target)
    {
        if (target.DefaultFormat is null)
        {
            return "default";
        }

        return $"\"{EscapeString(target.DefaultFormat)}\".AsSpan()";
    }

    private static string BuildProviderGuard(TargetInfo target)
    {
        if (target.AllowNullFormatProvider)
        {
            return string.Empty;
        }

        return "        if (provider is null)\n"
            + "        {\n"
            + "            ValuGenThrowHelper.ThrowArgumentNullException(nameof(provider));\n"
            + "        }\n\n";
    }

    private static string BuildGeneratedTypeHeader(TargetInfo target, string ns, string utf8Interface)
    {
        return $$"""
// <auto-generated/>
#nullable enable
using System;

{{ns}}

partial {{target.TypeKeyword}} {{target.TypeName}} : System.IFormattable, System.ISpanFormattable{{utf8Interface}}
{
""";
    }

    private static void AppendToStringFormatProviderIfMissing(TargetInfo target, string providerGuard, StringBuilder members)
    {
        if (target.HasExistingToStringFormatProvider)
        {
            return;
        }

        if (!target.HasMaxBufferSizeConstant)
        {
            return;
        }

        members.AppendLine($$"""

    public string ToString(string? format, System.IFormatProvider? provider)
    {
{{providerGuard}}        int maxBufferSize = {{target.MaxBufferSizeConstantValue}};
        if (maxBufferSize <= {{StackallocThreshold}})
        {
            System.Span<char> stackBuffer = stackalloc char[maxBufferSize];
            if (!TryFormat(stackBuffer, out int charsWritten, format.AsSpan(), provider))
            {
                return string.Empty;
            }

            return stackBuffer[..charsWritten].ToString();
        }

        char[] rented = System.Buffers.ArrayPool<char>.Shared.Rent(maxBufferSize);
        try
        {
            System.Span<char> pooledBuffer = rented.AsSpan(0, maxBufferSize);
            if (!TryFormat(pooledBuffer, out int charsWritten, format.AsSpan(), provider))
            {
                return string.Empty;
            }

            return pooledBuffer[..charsWritten].ToString();
        }
        finally
        {
            System.Buffers.ArrayPool<char>.Shared.Return(rented);
        }
    }
""");
    }

    private static void AppendTryFormatCharFormatIfMissing(TargetInfo target, StringBuilder members)
    {
        if (target.HasExistingTryFormatCharFormatOnly)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(System.Span<char> destination, out int charsWritten, System.ReadOnlySpan<char> format)
    {
        return TryFormat(destination, out charsWritten, format, (System.IFormatProvider?)null);
    }
""");
    }

    private static void AppendTryFormatCharProviderIfMissing(TargetInfo target, string defaultFormatSpan, string providerGuard, StringBuilder members)
    {
        if (target.HasExistingTryFormatCharProviderOnly)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(System.Span<char> destination, out int charsWritten, System.IFormatProvider? provider)
    {
{{providerGuard}}        return TryFormat(destination, out charsWritten, {{defaultFormatSpan}}, provider);
    }
""");
    }

    private static void AppendTryFormatCharNoArgsIfMissing(TargetInfo target, StringBuilder members)
    {
        if (target.HasExistingTryFormatCharNoFormatNoProvider)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(System.Span<char> destination, out int charsWritten)
    {
        return TryFormat(destination, out charsWritten, (System.IFormatProvider?)null);
    }
""");
    }

    private static void AppendToStringProviderIfMissing(TargetInfo target, string defaultFormatLiteral, bool hasToStringCore, StringBuilder members)
    {
        if (target.HasExistingToStringProviderOnly)
        {
            return;
        }

        if (!hasToStringCore)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public string ToString(System.IFormatProvider? provider)
    {
        return ToString({{defaultFormatLiteral}}, provider);
    }
""");
    }

    private static void AppendToStringFormatIfMissing(TargetInfo target, bool hasToStringCore, StringBuilder members)
    {
        if (target.HasExistingToStringFormatOnly)
        {
            return;
        }

        if (!hasToStringCore)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public string ToString(string? format)
    {
        return ToString(format, null);
    }
""");
    }

    private static void AppendToStringNoArgsIfMissing(TargetInfo target, string defaultFormatLiteral, bool hasToStringCore, StringBuilder members)
    {
        if (target.HasExistingToStringNoArgs)
        {
            return;
        }

        if (!hasToStringCore)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return ToString({{defaultFormatLiteral}}, null);
    }
""");
    }

    private static void AppendTryFormatUtf8ProviderIfMissing(TargetInfo target, string providerGuard, bool hasToStringCore, StringBuilder members)
    {
        if (!target.ImplementUtf8)
        {
            return;
        }

        if (!hasToStringCore)
        {
            return;
        }

        if (target.HasExistingTryFormatUtf8Provider)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(System.Span<byte> destination, out int bytesWritten, System.ReadOnlySpan<char> format, System.IFormatProvider? provider)
    {
{{providerGuard}}        string value = ToString(format.ToString(), provider);
        int requiredBytes = System.Text.Encoding.UTF8.GetByteCount(value);
        if (requiredBytes > destination.Length)
        {
            bytesWritten = 0;
            return false;
        }

        bytesWritten = System.Text.Encoding.UTF8.GetBytes(value.AsSpan(), destination);
        return true;
    }
""");
    }

    private static void AppendTryFormatUtf8FormatIfMissing(TargetInfo target, StringBuilder members)
    {
        if (!target.ImplementUtf8)
        {
            return;
        }

        if (target.HasExistingTryFormatUtf8FormatOnly)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(System.Span<byte> destination, out int bytesWritten, System.ReadOnlySpan<char> format)
    {
        return TryFormat(destination, out bytesWritten, format, (System.IFormatProvider?)null);
    }
""");
    }

    private static void AppendTryFormatUtf8ProviderOnlyIfMissing(TargetInfo target, string defaultFormatSpan, string providerGuard, StringBuilder members)
    {
        if (!target.ImplementUtf8)
        {
            return;
        }

        if (target.HasExistingTryFormatUtf8ProviderOnly)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(System.Span<byte> destination, out int bytesWritten, System.IFormatProvider? provider)
    {
{{providerGuard}}        return TryFormat(destination, out bytesWritten, {{defaultFormatSpan}}, provider);
    }
""");
    }

    private static void AppendTryFormatUtf8NoArgsIfMissing(TargetInfo target, StringBuilder members)
    {
        if (!target.ImplementUtf8)
        {
            return;
        }

        if (target.HasExistingTryFormatUtf8NoArgs)
        {
            return;
        }

        members.AppendLine($$"""
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(System.Span<byte> destination, out int bytesWritten)
    {
        return TryFormat(destination, out bytesWritten, (System.IFormatProvider?)null);
    }
""");
    }

    /// <summary>
    /// Escapes backslashes and double-quotes in <paramref name="value"/> for safe embedding
    /// inside a C# string literal.
    /// </summary>
    private static string EscapeString(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    /// <summary>
    /// Immutable snapshot of all metadata the generator needs about a single
    /// <c>[AutoFormattable]</c>-annotated type, including feature flags and existing method presence.
    /// </summary>
    private readonly record struct TargetInfo(
        string TypeName,
        string Namespace,
        string TypeKeyword,
        bool IsPartial,
        bool IsStatic,
        bool HasTryFormat,
        bool HasExistingTryFormatCharFormatOnly,
        bool HasExistingTryFormatCharProviderOnly,
        bool HasExistingTryFormatCharNoFormatNoProvider,
        bool HasExistingToStringFormatProvider,
        bool HasExistingToStringProviderOnly,
        bool HasExistingToStringFormatOnly,
        bool HasExistingToStringNoArgs,
        bool ImplementUtf8,
        string? DefaultFormat,
        bool AllowNullFormatProvider,
        bool HasMaxBufferSizeConstant,
        int MaxBufferSizeConstantValue,
        bool HasExistingTryFormatUtf8Provider,
        bool HasExistingTryFormatUtf8FormatOnly,
        bool HasExistingTryFormatUtf8ProviderOnly,
        bool HasExistingTryFormatUtf8NoArgs,
        Location Location);
}

