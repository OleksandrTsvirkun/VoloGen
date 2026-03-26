---
title: Installation
layout: default
nav_order: 4
---

# Installation
{: .no_toc }

## Table of contents
{: .no_toc .text-delta }

1. TOC
{:toc}

---

## NuGet Packages

Each generator ships as **two packages**: a lightweight attribute package and the generator itself. Both are required.

| Feature | Attribute Package | Generator Package |
|:--------|:------------------|:------------------|
| Equality | `VoloGen.Equality` | `VoloGen.Equality.Generator` |
| Comparable | `VoloGen.Comparable` | `VoloGen.Comparable.Generator` |
| Parsable | `VoloGen.Parsable` | `VoloGen.Parsable.Generator` |
| Formattable | `VoloGen.Formattable` | `VoloGen.Formattable.Generator` |

## .NET CLI

```bash
# Equality
dotnet add package VoloGen.Equality
dotnet add package VoloGen.Equality.Generator

# Comparable
dotnet add package VoloGen.Comparable
dotnet add package VoloGen.Comparable.Generator

# Parsable
dotnet add package VoloGen.Parsable
dotnet add package VoloGen.Parsable.Generator

# Formattable
dotnet add package VoloGen.Formattable
dotnet add package VoloGen.Formattable.Generator
```

## PackageReference (csproj)

```xml
<ItemGroup>
  <!-- Pick the generators you need -->
  <PackageReference Include="VoloGen.Parsable" Version="*" />
  <PackageReference Include="VoloGen.Parsable.Generator" Version="*"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

{: .note }
The generator package must be referenced with `OutputItemType="Analyzer"` and `ReferenceOutputAssembly="false"` so it runs at compile time without being included as a runtime dependency.

## Compatibility

| Component | Target Framework |
|:----------|:-----------------|
| Attribute packages | `netstandard2.0` |
| Generator packages | `netstandard2.0` |
| Your project | Any .NET version that supports source generators (`.NET 6+` recommended) |

The attribute packages target `netstandard2.0` for maximum compatibility -- they can be consumed by any .NET project. The generator packages also target `netstandard2.0` as required by the Roslyn analyzer infrastructure.

## Strong-Name Signing

All VoloGen assemblies are **strong-name signed** with `VoloGen.snk`. This ensures:

- Assembly binding predictability
- Compatibility with strong-name-signed consuming projects
- Supply-chain integrity verification
