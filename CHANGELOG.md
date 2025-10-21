# Changelog

All notable changes to the Chonkie.NET project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- Updated target framework from .NET 8.0 to .NET 10.0 (Preview/RC)
- Updated language version from C# 12 to C# 13
- All projects now inherit framework configuration from Directory.Build.props
- CI/CD pipeline updated to use .NET 10.0 SDK

### Added - Phase 1 Complete ✅
- .NET solution structure with Chonkie.Core and Chonkie.Tokenizers projects
- Core types: `Chunk`, `Document`, `Sentence`
- Tokenizer infrastructure:
  - `ITokenizer` interface
  - `CharacterTokenizer` - Character-level tokenization
  - `WordTokenizer` - Word-level tokenization
  - `AutoTokenizer` - Factory for creating tokenizers
- Microsoft.Extensions.Logging.Abstractions integration for logging
- Comprehensive test suite with 50 unit tests (100% passing)
- GitHub Actions CI/CD pipeline for automated builds and tests
- Directory.Build.props for centralized project configuration

### Changed
- Updated PORT_PLAN.md to reflect Phase 1 completion
- Updated project status from Planning to Phase 2

## Project Start - 2025-10-21

### Added
- `PORT_PLAN.md` - Comprehensive port plan from Python to .NET/C#
- `CHANGELOG.md` - This changelog file
- Project repository initialization

### Notes
- Project is currently in the planning phase
- Target framework: .NET 8.0 (LTS)
- Language: C# 12
- Planned release: Version 1.0.0

---

## Future Releases

### [1.0.0] - TBD (Week 18)
Target: First stable release with feature parity to Python Chonkie

**Planned Features:**
- Core types and interfaces
- 8+ chunker implementations
- Tokenizer abstractions and implementations
- Embedding provider integrations (8+ providers)
- Vector database handshakes (8+ databases)
- LLM genie integrations (3+ providers)
- Pipeline system with fluent API
- Comprehensive documentation
- NuGet packages

### [0.9.0] - TBD (Week 17)
Target: Release candidate for community testing

**Planned Features:**
- Beta-quality implementation of all features
- Complete test coverage
- Documentation and samples
- Performance benchmarks

### [0.5.0] - TBD (Week 11)
Target: Mid-project milestone

**Planned Features:**
- Core chunkers completed
- Pipeline system functional
- Embedding integrations started

### [0.1.0] - TBD (Week 2)
Target: Foundation milestone

**Planned Features:**
- Core types implemented
- Basic tokenizers
- Initial CI/CD pipeline

---

**Legend:**
- `Added` - New features
- `Changed` - Changes in existing functionality
- `Deprecated` - Soon-to-be removed features
- `Removed` - Removed features
- `Fixed` - Bug fixes
- `Security` - Security improvements
