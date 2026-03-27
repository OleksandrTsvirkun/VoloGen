namespace VoloGen;

/// <summary>
/// Marks a partial struct or class for automatic formatting code generation.
/// The source generator emits <see cref="System.IFormattable"/> and
/// <c>ISpanFormattable</c> implementations.
/// </summary>
/// <remarks>
/// <para>
/// The annotated type <b>must</b> be declared as <see langword="partial"/> and <b>must not</b>
/// be <see langword="static"/>.
/// </para>
/// <para>
/// The type must provide the following user-defined method that the generated code delegates to:
/// <list type="bullet">
///   <item><description><c>bool TryFormat(Span&lt;char&gt; destination, out int charsWritten, ReadOnlySpan&lt;char&gt; format, IFormatProvider? provider)</c> — core formatting logic.</description></item>
/// </list>
/// </para>
/// <para>
/// For <c>ToString</c> generation the type must either supply its own
/// <c>string ToString(string?, IFormatProvider?)</c> method <b>or</b> declare a
/// <c>const int MaxBufferSize</c> field so the generator can produce a
/// stackalloc / <c>ArrayPool&lt;char&gt;</c>-backed implementation.
/// </para>
/// <para>
/// When <see cref="ImplementUtf8"/> is <see langword="true"/>, the generator additionally emits
/// <c>IUtf8SpanFormattable</c> overloads that encode the formatted output to UTF-8.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [AutoFormattable(DefaultFormat = "G", ImplementUtf8 = true)]
/// public partial struct Amount
/// {
///     private readonly decimal _value;
///     public const int MaxBufferSize = 64;
///
///     public bool TryFormat(Span&lt;char&gt; destination, out int charsWritten,
///         ReadOnlySpan&lt;char&gt; format, IFormatProvider? provider)
///     {
///         return _value.TryFormat(destination, out charsWritten, format, provider);
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="AutoFormattableAttribute"/>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class AutoFormattableAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a value indicating whether <c>IUtf8SpanFormattable</c>
    /// overloads should be generated. Defaults to <see langword="false"/>.
    /// </summary>
    /// <value><see langword="true"/> to generate UTF-8 formatting overloads; otherwise, <see langword="false"/>.</value>
    public bool ImplementUtf8 { get; set; } = false;

    /// <summary>
    /// Gets or sets the default format string passed to overloads that omit the <c>format</c> parameter.
    /// Defaults to <see langword="null"/>, which leaves the format unspecified.
    /// </summary>
    /// <value>
    /// A format string (e.g., <c>"G"</c>, <c>"N2"</c>) or <see langword="null"/>.
    /// </value>
    public string? DefaultFormat { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether a <see langword="null"/>
    /// <see cref="System.IFormatProvider"/> is acceptable.
    /// When <see langword="false"/>, generated overloads throw
    /// <see cref="System.ArgumentNullException"/> if the provider is <see langword="null"/>.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    /// <value><see langword="true"/> to allow null providers; <see langword="false"/> to enforce non-null providers.</value>
    public bool AllowNullFormatProvider { get; set; } = true;

}

