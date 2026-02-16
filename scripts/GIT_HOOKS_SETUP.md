# Git Hooks Setup Guide

This document describes how to set up git hooks for local development to enforce license header consistency.

## Overview

The pre-commit hook validates that all staged files have proper Apache License headers before allowing commits. This ensures compliance and consistency across the codebase.

## Setup Instructions

### Windows (PowerShell)

```powershell
# Set git to use the scripts/hooks directory as hook location
git config core.hooksPath scripts\hooks

# Make the pre-commit hook executable (if using WSL)
# chmod +x scripts/hooks/pre-commit
```

### Linux/macOS

```bash
# Set git to use the scripts/hooks directory as hook location
git config core.hooksPath scripts/hooks

# Make the pre-commit hook executable
chmod +x scripts/hooks/pre-commit
```

## Validation Script

The validation is performed by `scripts/validate-headers.ps1`, which:

1. **Checks staged files** - Only validates files that are being committed
2. **Filters by extension** - Only checks `.cs`, `.py` files
3. **Validates headers** - Ensures each file starts with the appropriate copyright header
4. **Provides feedback** - Shows which files are missing headers and where to find templates

### Supported File Types

| Extension | Header Style            |
|-----------|-------------------------|
| `.cs`     | C# line comments (`//`) |
| `.py`     | Python comments (`#`)   |

## Header Templates

For header templates and detailed formatting guidelines, see [LICENSE_HEADERS.md](../LICENSE_HEADERS.md).

## Running the Hook

The hook runs automatically before each `git commit`. To bypass it (not recommended):

```bash
git commit --no-verify
```

## Manual Validation

To manually validate all source files:

```powershell
pwsh scripts/validate-headers.ps1
```

## GitHub Actions

A corresponding validation step is run in the GitHub Actions CI/CD pipeline (`code-quality` job):

- **Trigger**: Every push to `main`, and on all pull requests
- **Action**: Validates Apache License headers on all source files
- **Failure**: Blocks merge if headers are missing

## Troubleshooting

### Hook not running on Windows

If the pre-commit hook is not executing on Windows, ensure:

1. **Git version**: Update to the latest Git version (2.9+)
2. **PowerShell policy**: Check execution policy:

   ```powershell
   Get-ExecutionPolicy
   # If restricted, update: Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

3. **Core.hooksPath**: Verify it's set correctly:

   ```powershell
   git config core.hooksPath
   ```

### Hook bypassed on CI/CD

The GitHub Actions validation is independent of local hooks and will always run on all commits to `main`, and on all pull requests.

## Adding to New Files

When creating new source files, add the appropriate header based on the file type. See [LICENSE_HEADERS.md](../LICENSE_HEADERS.md) for templates.

## Questions or Issues?

Refer to [CONTRIBUTING.md](../CONTRIBUTING.md) for more information on contribution guidelines, or open an issue in the repository.
