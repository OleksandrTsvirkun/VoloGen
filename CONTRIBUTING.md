# Contributing to VoloGen

Thank you for your interest in contributing! This document explains the workflow, conventions, and guidelines for the project.

## Getting Started

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

- **`main`** — always deployable, protected
- **`feature/<scope>-<description>`** — new functionality
- **`fix/<scope>-<description>`** — bug fixes
- **`docs/<description>`** — documentation only
- **`chore/<description>`** — maintenance tasks

### Example

```bash
git checkout -b feature/parsable-utf8-overloads
# ... work ...
git commit -m "feat(parsable): add IUtf8SpanParsable support"
git push origin feature/parsable-utf8-overloads
# Open a Pull Request → main
```

## Commit Messages

All commits **must** follow [Conventional Commits](https://www.conventionalcommits.org/). This is enforced by a `commit-msg` git hook (Husky.NET + CommitLint.NET).

### Format

```
<type>(<scope>): <short description>

[optional body]

[optional footer(s)]
```

### Allowed Types

| Type | When to use |
|------|-------------|
| `feat` | A new feature or capability |
| `fix` | A bug fix |
| `refactor` | Code change that neither fixes a bug nor adds a feature |
| `docs` | Documentation only |
| `test` | Adding or updating tests |
| `build` | Build system or external dependencies |
| `ci` | CI configuration |
| `chore` | Maintenance, tooling, configs |
| `perf` | Performance improvement |
| `style` | Formatting, white-space, missing semi-colons |

### Allowed Scopes

`parsable`, `formattable`, `comparable`, `equality`, `common`, `abstractions`, `ci`, `readme`

### Good Examples

```
feat(parsable): add span-based TryParse generation
fix(equality): handle null in Equals(object)
refactor(common): extract ThrowHelper
docs(readme): update usage section
test(parsable): add providerless overload coverage
build(ci): add GitHub Actions workflow
```

### Bad Examples

```
updated stuff               # no type, no scope, vague
fix: thing                  # missing scope
FEAT(Parsable): Add thing   # type must be lowercase
```

## Adding a New Generator

1. Create a new project under `src/VoloGen.<Name>/`
2. Add a marker attribute in `VoloGen.Abstractions`
3. Implement `IIncrementalGenerator`
4. Add diagnostic descriptors in `VoloGen.Common`
5. Create `tests/VoloGen.<Name>.Tests/` with xUnit tests
6. Add both projects to `VoloGen.slnx`
7. Update the README table

## Code Style

- Follow `.editorconfig` rules
- File-scoped namespaces
- `_camelCase` for private fields
- XML doc on public API
- No `TODO` comments in merged code

## Testing

- All generators must have tests for:
  - Successful code generation
  - Each diagnostic (missing partial, static types, etc.)
  - Edge cases (empty types, nested types, generic types)

Run tests:

```bash
dotnet test
```

## Pull Requests

1. One logical change per PR
2. All CI checks must pass
3. Squash-merge into `main`
4. PR title follows commit convention: `feat(parsable): add UTF-8 support`

