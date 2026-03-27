namespace VoloGen;

/// <summary>
/// Marks a partial struct or class for automatic equality code generation.
/// The source generator emits <see cref="System.IEquatable{T}"/> implementation,
/// <see cref="object.Equals(object?)"/>, <see cref="object.GetHashCode"/>, and
/// <c>operator ==</c> / <c>operator !=</c> overloads.
/// </summary>
/// <remarks>
/// <para>
/// The annotated type <b>must</b> be declared as <see langword="partial"/> and <b>must not</b>
/// be <see langword="static"/>.
/// </para>
/// <para>
/// The type must provide the following user-defined methods that the generated code delegates to:
/// <list type="bullet">
///   <item><description><c>static bool Equal(T left, T right)</c> — core equality logic.</description></item>
///   <item><description><c>override int GetHashCode()</c> — hash code computation.</description></item>
/// </list>
/// </para>
/// <para>
/// If the type is also annotated with <c>[AutoComparable]</c>, the equality
/// operators (<c>==</c>, <c>!=</c>) are emitted by the comparable generator instead,
/// avoiding duplicate operator declarations.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [AutoEquality]
/// public partial struct UserId
/// {
///     private readonly int _value;
///
///     public static bool Equal(UserId left, UserId right)
///     {
///         return left._value == right._value;
///     }
///
///     public override int GetHashCode()
///     {
///         return _value.GetHashCode();
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="AutoEqualityAttribute"/>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class AutoEqualityAttribute : Attribute;

