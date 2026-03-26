namespace VoloGen;

/// <summary>
/// Marks a partial struct or class for automatic parsing code generation.
/// The source generator emits all remaining <c>IParsable&lt;TSelf&gt;</c>
/// and <c>ISpanParsable&lt;TSelf&gt;</c> overloads from a single
/// user-defined <c>TryParse(ReadOnlySpan&lt;char&gt;, IFormatProvider?, out T)</c> method.
/// </summary>
/// <remarks>
/// <para>
/// The annotated type <b>must</b> be declared as <see langword="partial"/> and <b>must not</b>
/// be <see langword="static"/> or <see langword="abstract"/>.
/// </para>
/// <para>
/// The type must provide the following user-defined method that the generated code delegates to:
/// <list type="bullet">
///   <item><description><c>static bool TryParse(ReadOnlySpan&lt;char&gt; s, IFormatProvider? provider, out T result)</c> — core parsing logic.</description></item>
/// </list>
/// </para>
/// <para>
/// When <see cref="ImplementExact"/> is <see langword="true"/>, the type must also provide:
/// <list type="bullet">
///   <item><description><c>static bool TryParseExact(ReadOnlySpan&lt;char&gt; s, ReadOnlySpan&lt;char&gt; format, IFormatProvider? provider, out T result)</c></description></item>
/// </list>
/// </para>
/// <para>
/// When <see cref="ImplementUtf8"/> is <see langword="true"/>, the generator additionally emits
/// <c>IUtf8SpanParsable&lt;TSelf&gt;</c> overloads that decode UTF-8 input before
/// delegating to the char-based core.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [AutoParsable(ImplementUtf8 = true)]
/// public partial struct Amount
/// {
///     private readonly decimal _value;
///
///     public static bool TryParse(ReadOnlySpan&lt;char&gt; s, IFormatProvider? provider, out Amount result)
///     {
///         if (decimal.TryParse(s, provider, out var value))
///         {
///             result = new Amount { _value = value };
///             return true;
///         }
///         result = default;
///         return false;
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="AutoParsableAttribute"/>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class AutoParsableAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a value indicating whether <c>IUtf8SpanParsable&lt;TSelf&gt;</c>
    /// overloads should be generated. Defaults to <see langword="false"/>.
    /// </summary>
    /// <value><see langword="true"/> to generate UTF-8 parsing overloads; otherwise, <see langword="false"/>.</value>
    public bool ImplementUtf8 { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether format-exact parsing overloads
    /// (<c>ParseExact</c> / <c>TryParseExact</c>) should be generated.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    /// <value><see langword="true"/> to generate exact-format parsing overloads; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    /// When enabled, the type must define a core
    /// <c>static bool TryParseExact(ReadOnlySpan&lt;char&gt;, ReadOnlySpan&lt;char&gt;, IFormatProvider?, out T)</c>
    /// method.
    /// </remarks>
    public bool ImplementExact { get; set; }
}

