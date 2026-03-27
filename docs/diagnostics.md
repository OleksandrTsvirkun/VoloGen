---
title: Diagnostics
permalink: /diagnostics/
---

# Diagnostics

All VoloGen diagnostics use the **VoloGen** category and are **errors** by default. Diagnostic messages are localized via `.resx` resource files.

---

## VG0001

**MissingTryParseMethod**

**Trigger:** `[AutoParsable]` is applied to a type that does not define the required core method.

**Required signature:**

```csharp
public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out T result)
```

**Fix:** Add the `TryParse` method with the exact signature shown above to the annotated type.

---

## VG0002

**MustBePartial**

**Trigger:** Any VoloGen attribute is applied to a type that is not declared as `partial`.

**Fix:** Add the `partial` modifier to the type declaration.

```diff
- [AutoEquality]
- public struct UserId { ... }
+ [AutoEquality]
+ public partial struct UserId { ... }
```

---

## VG0003

**CannotBeStatic**

**Trigger:** Any VoloGen attribute is applied to a `static` type.

**Fix:** Remove the `static` modifier. Source generators add instance members that cannot exist on static types.

```diff
- [AutoEquality]
- public static partial class Utilities { ... }
+ [AutoEquality]
+ public partial class Utilities { ... }
```

---

## VG0004

**CannotBeAbstract**

**Trigger:** `[AutoParsable]` is applied to an `abstract` type.

**Fix:** Remove the `abstract` modifier or use a concrete type.

---

## VG0005

**MissingComparableField**

**Trigger:** `[AutoComparable]`

**Required signature:**

```csharp
public static int Compare(T left, T right)
```

**Fix:** Add the `Compare` method with the exact signature shown above to the annotated type.

---

## VG0006

**MissingEquatableField**

**Trigger:** `[AutoEquality]`

**Required signatures:**

```csharp
public static bool Equal(T left, T right)
public override int GetHashCode()
```

**Fix:** Ensure both methods are present on the annotated type.

---

## VG0007

**MissingTryFormatMethod**

**Trigger:** `[AutoFormattable]`

**Required signature:**

```csharp
public bool TryFormat(Span<char> destination, out int charsWritten,
    ReadOnlySpan<char> format, IFormatProvider? provider)
```

**Fix:** Add the `TryFormat` method with the exact signature shown above to the annotated type.

---

## VG0008

**MissingToStringOrMaxBufferSize**

**Trigger:** `[AutoFormattable]` is applied to a type that has `TryFormat` but provides neither a custom `ToString(string?, IFormatProvider?)` method nor a `const int MaxBufferSize` field.

The generator needs one of these to produce the `ToString` overload:

- **`const int MaxBufferSize`** -- the generator creates a `stackalloc` / `ArrayPool<char>`-backed `ToString`
- **`string ToString(string?, IFormatProvider?)`** -- the generator uses your method directly

**Fix:** Add either a `MaxBufferSize` constant or a `ToString(string?, IFormatProvider?)` method.

```csharp
// Option A: MaxBufferSize
public const int MaxBufferSize = 64;

// Option B: Custom ToString
public string ToString(string? format, IFormatProvider? provider)
{
    return _value.ToString(format, provider);
}
```

---

## Localization

All diagnostic messages are localized via `.resx` resource files in the `VoloGen.Common` project. Currently supported locales include:

- English (default)
- German (de)
- Spanish (es)
- French (fr)
- Japanese (ja)
- Korean (ko)
- Portuguese -- Brazil (pt-BR)
- Ukrainian (uk)
- Chinese -- Simplified (zh-Hans)
