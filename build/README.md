# Build Scripts

This directory contains build automation scripts.

## Changelog Generation

```bash
git-cliff -o CHANGELOG.md
```

## Local Build

```bash
dotnet build --configuration Release
```

## Local Test

```bash
dotnet test --configuration Release --verbosity normal
```
