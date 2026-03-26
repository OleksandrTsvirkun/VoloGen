using System.Resources;
using Microsoft.CodeAnalysis;

namespace VoloGen.Common;

/// <summary>
/// Central registry of all <see cref="DiagnosticDescriptor"/> instances emitted by VoloGen generators.
/// Each descriptor corresponds to a specific code analysis rule (VG0001–VG0008).
/// </summary>
public static class DiagnosticDescriptors
{
    /// <summary>Shared diagnostic category applied to every descriptor.</summary>
    private const string Category = "VoloGen";

    /// <summary>
    /// Resource manager used to load localized title, message, and description strings.
    /// </summary>
    private static readonly ResourceManager _resourceManager =
        new("VoloGen.Common.Diagnostics", typeof(DiagnosticDescriptors).Assembly);

    /// <summary>
    /// Creates a <see cref="LocalizableResourceString"/> for the given resource key.
    /// </summary>
    /// <param name="name">The resource key (e.g., <c>VG0001_Title</c>).</param>
    /// <returns>A localizable string backed by the embedded <c>Diagnostics.resx</c> resource.</returns>
    private static LocalizableResourceString Localize(string name)
    {
        return new LocalizableResourceString(name, _resourceManager, typeof(DiagnosticDescriptors));
    }

    /// <summary>
    /// <b>VG0001</b> — The type is annotated with <c>[AutoParsable]</c> but does not define the required
    /// <c>static bool TryParse(ReadOnlySpan&lt;char&gt;, IFormatProvider?, out T)</c> core method.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingTryParseMethod = new(
        id: "VG0001",
        title: Localize("VG0001_Title"),
        messageFormat: Localize("VG0001_MessageFormat"),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Localize("VG0001_Description"));

    /// <summary>
    /// <b>VG0002</b> — The type is not declared as <see langword="partial"/>.
    /// All VoloGen attributes require the type to be partial so generated code can
    /// be added as another partial declaration.
    /// </summary>
    public static readonly DiagnosticDescriptor MustBePartial = new(
        id: "VG0002",
        title: Localize("VG0002_Title"),
        messageFormat: Localize("VG0002_MessageFormat"),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Localize("VG0002_Description"));

    /// <summary>
    /// <b>VG0003</b> — The type is declared as <see langword="static"/>.
    /// VoloGen generators cannot emit instance members for static types.
    /// </summary>
    public static readonly DiagnosticDescriptor CannotBeStatic = new(
        id: "VG0003",
        title: Localize("VG0003_Title"),
        messageFormat: Localize("VG0003_MessageFormat"),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Localize("VG0003_Description"));

    /// <summary>
    /// <b>VG0004</b> — The type is declared as <see langword="abstract"/>.
    /// VoloGen generators cannot produce concrete interface implementations for abstract types.
    /// </summary>
    public static readonly DiagnosticDescriptor CannotBeAbstract = new(
        id: "VG0004",
        title: Localize("VG0004_Title"),
        messageFormat: Localize("VG0004_MessageFormat"),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Localize("VG0004_Description"));

    /// <summary>
    /// <b>VG0005</b> — The type is annotated with <c>[AutoComparable]</c> but does not define
    /// the required <c>static int Compare(T, T)</c> core method.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingComparableField = new(
        id: "VG0005",
        title: Localize("VG0005_Title"),
        messageFormat: Localize("VG0005_MessageFormat"),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Localize("VG0005_Description"));

    /// <summary>
    /// <b>VG0006</b> — The type is annotated with <c>[AutoEquality]</c> but does not define both
    /// <c>static bool Equal(T, T)</c> and <c>override int GetHashCode()</c>.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingEquatableField = new(
        id: "VG0006",
        title: Localize("VG0006_Title"),
        messageFormat: Localize("VG0006_MessageFormat"),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Localize("VG0006_Description"));

    /// <summary>
    /// <b>VG0007</b> — The type is annotated with <c>[AutoFormattable]</c> but does not define
    /// the required <c>bool TryFormat(Span&lt;char&gt;, out int, ReadOnlySpan&lt;char&gt;, IFormatProvider?)</c> method.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingTryFormatMethod = new(
        id: "VG0007",
        title: Localize("VG0007_Title"),
        messageFormat: Localize("VG0007_MessageFormat"),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Localize("VG0007_Description"));

    /// <summary>
    /// <b>VG0008</b> — The type is annotated with <c>[AutoFormattable]</c> but defines neither
    /// <c>string ToString(string?, IFormatProvider?)</c> nor a <c>const int MaxBufferSize</c> field.
    /// At least one is required to generate <c>ToString</c> overloads.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingToStringOrMaxBufferSize = new(
        id: "VG0008",
        title: Localize("VG0008_Title"),
        messageFormat: Localize("VG0008_MessageFormat"),
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Localize("VG0008_Description"));
}

