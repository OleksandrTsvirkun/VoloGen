---
title: Contributing
layout: default
nav_order: 5
---

# Contributing

Thank you for your interest in contributing to VoloGen! This page covers everything you need to get started.

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- Git

### Setup

```bash
git clone https://github.com/OleksandrTsvirkun/VoloGen.git
cd VoloGen
dotnet tool restore
dotnet husky install
dotnet build
dotnet test
```

## Branch Strategy

We use a simplified GitFlow model with `main` as the sole long-lived branch:

| Branch Pattern | Purpose |
|:---------------|:--------|
| `main` | Always deployable, protected |
| `feature/<scope>-<description>` | New functionality |
| `fix/<scope>-<description>` | Bug fixes |
| `docs/<description>` | Documentation only |
| `chore/<description>` | Maintenance tasks |

### Example

```bash
git checkout -b feature/parsable-utf8-overloads
# ... make changes ...
git commit -m "feat(parsable): add IUtf8SpanParsable support"
git push origin feature/parsable-utf8-overloads
# Open a Pull Request -> main
```

## Commit Convention

All commits **must** follow [Conventional Commits](https://www.conventionalcommits.org/). This is enforced by a `commit-msg` git hook via **Husky.NET** + **CommitLint.NET**.

**Format:** `<type>(<scope>): <description>`

### Types

| Type | When to use |
|:-----|:------------|
| `feat` | A new feature or capability |
| `fix` | A bug fix |
| `refactor` | Code change that neither fixes a bug nor adds a feature |
| `docs` | Documentation only |
| `test` | Adding or updating tests |
| `build` | Build system or external dependencies |
| `ci` | CI configuration |
| `chore` | Maintenance, tooling, configs |
| `perf` | Performance improvement |
| `style` | Formatting, white-space, missing semicolons |

### Scopes

`parsable`, `formattable`, `comparable`, `equality`, `common`, `ci`, `readme`, `samples`

### Examples

```
feat(parsable): add span-based TryParse generation
fix(equality): handle null in Equals(object)
refactor(common): extract ThrowHelper
docs(readme): update usage section
test(parsable): add providerless overload coverage
```

## Code Style

All code must follow the `.editorconfig` rules in the repository:

- **4-space indentation** (2 spaces for XML/JSON/YAML/csproj)
- **File-scoped namespaces** everywhere
- **`_camelCase`** for private fields
- **PascalCase** for types, methods, properties, constants
- **Always use `{ }` braces** for control flow statements
- **Block bodies** for methods (no expression-bodied methods)
- **Raw string literals** (`$$"""..."""`) for code generation templates
- **No `StringBuilder`** for templates; use `StringBuilder` only for aggregating member blocks
- **No ternary operators** in generator logic

## Testing

All generators must have tests for:

- Successful generation with expected output
- Diagnostic reporting for each error condition
- Edge cases (nested types, global namespace, record structs)

```bash
dotnet test
```

## Adding a New Generator

1. Create `src/VoloGen.<Name>/` -- marker attribute (target `netstandard2.0`)
2. Create `src/VoloGen.<Name>.Generator/` -- source generator (target `netstandard2.0`)
3. Add diagnostic descriptors in `src/VoloGen.Common/DiagnosticDescriptors.cs`
4. Add localized messages in `Diagnostics.resx` + language variants
5. Create `tests/VoloGen.<Name>.Tests/` with xUnit tests
6. Add all projects to `VoloGen.slnx`
7. Update the README packages table
8. Add a documentation page under `docs/generators/`

## Pull Requests

1. One logical change per PR
2. All CI checks must pass
3. Squash-merge into `main`
4. PR title follows commit convention: `feat(parsable): add UTF-8 support`

---

## Resources

- [Code of Conduct](https://github.com/OleksandrTsvirkun/VoloGen/blob/main/CODE_OF_CONDUCT.md)
- [Security Policy](https://github.com/OleksandrTsvirkun/VoloGen/blob/main/SECURITY.md)
- [MIT License](https://github.com/OleksandrTsvirkun/VoloGen/blob/main/LICENSE)
