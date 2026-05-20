---
title: Generators
permalink: /generators/
---

VoloGen provides four independent source generators. Each one targets a specific value-object concern and ships as its own NuGet package.

| Generator | You Provide | Generator Emits |
|:----------|:------------|:----------------|
| [AutoEquality]({% link generators/equality.md %}) | `Equal(T, T)` + `GetHashCode()` | `IEquatable<T>`, `Equals(object?)`, `==`, `!=` |
| [AutoComparable]({% link generators/comparable.md %}) | `Compare(T, T)` | `IComparable<T>`, `IComparable`, `<`, `>`, `<=`, `>=`, `==`, `!=` |
| [AutoParsable]({% link generators/parsable.md %}) | `TryParse(ReadOnlySpan<char>, ...)` | `IParsable<T>`, `ISpanParsable<T>`, all `Parse`/`TryParse` overloads |
| [AutoFormattable]({% link generators/formattable.md %}) | `TryFormat(Span<char>, ...)` | `IFormattable`, `ISpanFormattable`, all `ToString`/`TryFormat` overloads |

---

## Common Rules

All generators share the following requirements:

1. **The type must be `partial`** -- the generator adds members to a partial declaration
2. **The type must not be `static`** -- source generation targets instance types
3. **The type must not be `abstract`** -- concrete implementations are required

Violating these rules produces a compile-time diagnostic error. See the [Diagnostics]({% link diagnostics.md %}) page for details.

## Skip-If-Exists

Every generator checks whether an identical overload already exists on the target type before emitting it. This means you can always override any generated method by defining your own -- the generator will silently skip it.

## Operator Coordination

When both `[AutoEquality]` and `[AutoComparable]` are applied to the same type:

- `[AutoComparable]` takes ownership of `==` and `!=` operators (delegating to `Compare`)
- `[AutoEquality]` skips those operators to avoid duplicate declarations

This coordination is automatic -- no configuration needed.
