# Contributing to Chonkie.Net

First off, thank you for considering a contribution to Chonkie.Net! It's people like you that make Chonkie.Net such a great tool.

## Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the issue list as you might find out that you don't need to create one. When you are creating a bug report, please include as many details as possible:

* **Use a clear and descriptive title** for the issue to identify the problem.
* **Describe the exact steps which reproduce the problem** in as much detail as possible.
* **Provide specific examples to demonstrate the steps.** Include links to files or GitHub projects, or copy/pasteable snippets, which you use in those examples.
* **Describe the behavior you observed after following the steps** and point out what exactly is the problem with that behavior.
* **Explain which behavior you expected to see instead and why.**
* **Include screenshots and animated GIFs if possible** which show you following the described steps and/or demonstrate the problem.
* **Include your environment details:** .NET version, operating system, any relevant dependencies, etc.

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, please include:

* **Use a clear and descriptive title** for the issue to identify the suggestion.
* **Provide a step-by-step description of the suggested enhancement** in as much detail as possible.
* **Provide specific examples to demonstrate the steps.**
* **Describe the current behavior** and **the suggested behavior.**
* **Explain why this enhancement would be useful** to most Chonkie.Net users.

### Pull Requests

* Fill in the required template
* Follow the C# style-guides (see below)
* Include appropriate test cases
* Document new code with XML comments
* Ensure all tests pass locally before submitting
* End all files with a newline

## Development Setup

### Prerequisites

* .NET 10 SDK or later
* Visual Studio Code or Visual Studio 2026 (recommended)
* PowerShell 7+ (for script execution on Windows)

### Getting Started

1. Fork the repository
2. Clone your fork:

   ```powershell
   git clone https://github.com/YOUR-USERNAME/Chonkie.Net.git
   cd Chonkie.Net
   ```

3. Add upstream remote:

   ```powershell
   git remote add upstream https://github.com/gianni-rg/Chonkie.Net.git
   ```

4. Create a topic branch from `main`:

   ```powershell
   git checkout -b feature/my-feature main
   ```

5. Build the solution:

   ```powershell
   dotnet build
   ```

6. Run tests:

   ```powershell
   dotnet test
   ```

### Making Your Changes

* Commit your changes in logical chunks
* Use clear, descriptive commit messages
* Reference any related issues in your commit messages (e.g., `Fixes #123`)
* Ensure your code follows the project's coding standards

## Style-guides

### C# Style-guide

This project follows the guidelines documented in [AGENTS.md](AGENTS.md), which includes:

* **Naming Conventions:**

  - PascalCase for public members, classes, methods
  - camelCase for private fields and local variables
  - Prefix private fields with `_` (e.g., `_userService`)
  - Prefix interfaces with `I` (e.g., `IUserService`)

* **Formatting:**

  - Follow `.editorconfig` rules (automatically enforced in most IDEs)
  - Use spaces for indentation (no tabs)
  - Insert a newline before opening curly braces
  - Use file-scoped namespaces and single-line using directives

* **Code Quality:**

  - Use modern C# 14 features where appropriate
  - Use `nameof()` instead of string literals for member names
  - Use pattern matching and switch expressions
  - Use null-conditional operators (`?.`) and null-coalescing operators (`??`)
  - Use `is null` / `is not null` instead of `== null` / `!= null`
  - Add XML doc comments for all public APIs
  - Handle edge cases and write clear exception handling

* **Performance:**

  - Avoid unnecessary allocations
  - Use structural equality where appropriate
  - Profile performance-critical code

### Commit Message Convention

Use clear, concise commit messages that follow this pattern:

```
[Type] Short description

Longer explanation if needed. This should explain the why, not the what.

Fixes #123
```

**Type** can be:

* `feat:` A new feature
* `fix:` A bug fix
* `docs:` Documentation changes
* `style:` Code style changes (formatting, missing semicolons, etc.)
* `refactor:` Code refactoring without feature changes or bug fixes
* `perf:` Performance improvements
* `test:` Adding or updating tests
* `chore:` Build process, dependency updates, etc.

Example:

```
feat: Add support for custom chunking strategies

Implement the strategy pattern to allow users to define custom chunking
algorithms. This provides more flexibility for domain-specific use cases.

Fixes #456
```

### Documentation Style-guide

* Use Markdown for documentation
* Reference code elements with backticks: `ClassName.MethodName()`
* Include code examples where applicable
* Keep lines at a reasonable length for readability
* Use clear, concise language

## Testing Guidelines

* Write tests for new functionality and bug fixes
* Use xUnit v3 for test frameworks (or the existing test framework in the project)
* Follow the AAA (Arrange-Act-Assert) pattern
* Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`
* Use `[Fact]` for invariant tests and `[Theory]` with `[InlineData]` for parameterized tests
* Mock external dependencies (databases, APIs, etc.) using NSubstitute
* Use Shouldly for readable assertions
* Ensure tests are independent and can run in any order

Example:

```csharp
[Fact]
public void Chunk_EmptyText_ReturnsEmptyCollection()
{
    // Arrange
    var chunker = new TokenChunker(new WordTokenizer());
    var text = "";

    // Act
    var chunks = chunker.Chunk(text);

    // Assert
    chunks.Should().BeEmpty();
}
```

## Licensing

By contributing to Chonkie.Net, you agree that your contributions will be licensed under its Apache License 2.0.

## Attribution

Contributors will be recognized in:

* The [CONTRIBUTORS.md](CONTRIBUTORS.md) file
* Release notes for significant contributions
* Project documentation where appropriate

## Review Process

1. Maintainers will review your pull request
2. Changes may be requested or suggestions provided
3. Once approved, your pull request will be merged
4. Your contribution will be attributed in release notes

## Questions?

Feel free to open an issue with the `question` label or contact the maintainers directly.

Thank you for contributing!
