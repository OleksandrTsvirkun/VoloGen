namespace VoloGen;

/// <summary>
/// Marks a partial struct or class for automatic comparison code generation.
/// The source generator emits <see cref="System.IComparable{T}"/>,
/// <see cref="System.IComparable"/>, and the full set of comparison operators
/// (<c>&lt;</c>, <c>&gt;</c>, <c>&lt;=</c>, <c>&gt;=</c>, <c>==</c>, <c>!=</c>).
/// </summary>
/// <remarks>
/// <para>
/// The annotated type <b>must</b> be declared as <see langword="partial"/> and <b>must not</b>
/// be <see langword="static"/>.
/// </para>
/// <para>
/// The type must provide the following user-defined method that the generated code delegates to:
/// <list type="bullet">
///   <item><description><c>static int Compare(T left, T right)</c> — core comparison logic.</description></item>
/// </list>
/// </para>
/// <para>
/// When combined with <c>[AutoEquality]</c>, this generator takes ownership
/// of the <c>==</c> and <c>!=</c> operators so they are not emitted twice.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [AutoComparable]
/// public partial struct Amount
/// {
///     private readonly decimal _value;
///
///     public static int Compare(Amount left, Amount right)
///     {
///         return left._value.CompareTo(right._value);
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="AutoComparableAttribute"/>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class AutoComparableAttribute : Attribute;

