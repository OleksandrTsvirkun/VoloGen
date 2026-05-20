---
title: FAQ
permalink: /faq/
---

Frequently asked questions about VoloGen.

---

## General

### What .NET versions are supported?

VoloGen attribute packages target `netstandard2.0`, making them compatible with any .NET version. The generators require a Roslyn version that supports incremental source generators, which means **.NET 6+** is recommended for the best experience.

### Does VoloGen add any runtime dependency?

**No.** The generated code is plain C# with no external dependencies. The generator and attribute packages are compile-time only -- they are not included in your application's output.

### Can I use multiple generators on the same type?

**Yes.** You can apply `[AutoEquality]` and `[AutoComparable]` to the same type. The generators coordinate automatically -- `[AutoComparable]` takes ownership of `==`/`!=` operators to avoid conflicts.

### Does it work with record types?

**Yes.** VoloGen works with `struct`, `class`, `record struct`, and `record class` types. The only requirement is that the type is `partial`.

---

## Code Generation

### What if I already have a method the generator would emit?

The generator uses a **skip-if-exists** pattern. Before emitting any method, it checks whether an identical overload (same name, parameter types, ref kinds, and return type) already exists. If it does, the generator silently skips it.

### Why are generated methods marked `AggressiveInlining`?

Thin wrapper overloads -- methods that just adapt parameters and delegate -- are marked with `[MethodImpl(MethodImplOptions.AggressiveInlining)]` to hint the JIT compiler. This eliminates call overhead for trivial delegations.

Core methods and heavier logic (e.g., `Parse` with exception throwing) are **not** inlined.

### Can I see the generated code?

Yes! Add this to your `.csproj` to emit the generated source files to disk:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)\GeneratedFiles</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

The generated files will appear in `obj/GeneratedFiles/`.

---

## Diagnostics

### Why am I getting VG0002 (MustBePartial)?

You need to add the `partial` modifier to your type declaration:

```diff
- [AutoEquality]
- public struct UserId { ... }
+ [AutoEquality]
+ public partial struct UserId { ... }
```

### Why am I getting VG0008 (MissingToStringOrMaxBufferSize)?

The `[AutoFormattable]` generator needs a way to produce `ToString`. Provide either:

- A `const int MaxBufferSize` field (the generator will use `stackalloc`/`ArrayPool`)
- A `string ToString(string?, IFormatProvider?)` method (the generator will use it directly)

### Can I suppress diagnostics?

VoloGen diagnostics are errors by default for safety. You can suppress them with `#pragma warning disable VGxxxx` if needed, but this is not recommended -- the diagnostics exist because the generated code would not work correctly without the required methods.

---

## Packaging

### Why are there two packages per generator?

The architecture splits each generator into:

1. **Attribute package** (e.g., `VoloGen.Parsable`) -- contains the marker attribute
2. **Generator package** (e.g., `VoloGen.Parsable.Generator`) -- contains the source generator

This ensures the attribute package has zero dependencies and maximum compatibility, while the generator package is loaded only at compile time.

### Are the assemblies strong-name signed?

**Yes.** All VoloGen assemblies are signed with `VoloGen.snk` (RSA-2048). This is required for consuming projects that are also strong-name signed.

### What is the minimum Roslyn version?

VoloGen targets `netstandard2.0` for the generator packages, which is compatible with the Roslyn version included in the .NET 6 SDK and later.
