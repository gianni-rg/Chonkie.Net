# 🦛 Chonkie.NET

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/github/license/gianni-rg/Chonkie.Net.svg)](https://github.com/gianni-rg/Chonkie.Net/blob/main/LICENSE)
[![Status](https://img.shields.io/badge/status-experimental-green)](STATUS_DASHBOARD.md)
[![Tests](https://img.shields.io/badge/tests-932_passing-brightgreen)](tests/)

A .NET port of [Chonkie](https://github.com/chonkie-inc/chonkie), the lightweight ingestion Python library for fast, efficient and robust RAG pipelines.

> *This is an independent port and is not officially affiliated with the original Chonkie project.  
All credit for the original design and implementation goes to the Chonkie team.*

Chonkie.NET is a somewhat faithful port of the Python Chonkie library to .NET/C#, bringing powerful text chunking capabilities to the .NET ecosystem with:

- **Feature Parity**: Chunkers, Tokenizers, Embeddings, Chefs, Genies, Pipelines, Porters, Handshakes, and integrations aligned with Python version as of v1.5.4 ([commit cd8bd64](https://github.com/chonkie-inc/chonkie/commit/cd8bd643bd7045686f0a8b73a64f1c9296c0dae2))
- **High Performance**: .NET 10, C# 14 features, and optimizations to possibly match or even exceed Python performance
- **Lightweight**: Modular packages, minimal dependencies
- **Strongly Typed**: Idiomatic C# APIs and DI-friendly design, with parts of the API designed to leverage the .NET AI ecosystem ([Microsoft.Extensions.AI](https://github.com/dotnet/extensions/tree/main/src/Libraries/Microsoft.Extensions.AI), [Semantic Kernel](https://github.com/microsoft/semantic-kernel), and [ONNX Runtime](https://github.com/microsoft/onnxruntime))
- **Local-First**: Support for local models and on-device processing with ONNX Runtime
- **Documentation**: Comprehensive documentation with examples, usage patterns, and best practices

## About this project

The project is part of an *experimental* journey with [GitHub Copilot](https://github.com/features/copilot) and coding agents in Visual Studio, Visual Studio Code and CLI, in order to evaluate the feasibility in terms of quality, efforts, and resources for translating a quite complex Python codebase into C#, with careful attention to maintain the same behavior, and explore the capabilities of (semi)autonomous AI-assisted coding.

**It is a work in progress, built in my spare time for fun and learning.**

The project initially *was* not intended for production use, but rather as a demonstration of the capabilities of AI-assisted coding. The results have been *very promising*, with a high degree of feature parity achieved, and the project has evolved into a fully functional .NET port of the original library, that might eventually be used in production scenarios.

> *Please keep in mind that the project should be still considered in early stages and experimental, and while the core implementation is complete, it still needs deep testing, optimizations and polishing here and there, before considering it stable*.

## Getting Started

### Project Organization

The project is organized in a monorepo structure, with multiple projects for different components of the library.

```text
Chonkie.Net/
├── .github/                  # CI/CD pipelines
├── src/                      # Source code
│   ├── Chonkie.Core/         # Core types and interfaces
│   ├── Chonkie.Tokenizers/   # Tokenizer implementations
│   ├── Chonkie.Chunkers/     # Chunker implementations
│   ├── Chonkie.Embeddings/   # Embedding providers
│   ├── Chonkie.Chefs/        # Content processing specialization
│   ├── Chonkie.Refineries/   # Post-chunking refinement
│   ├── Chonkie.Genies/       # Intelligent text generation
│   ├── Chonkie.Pipeline/     # Fluent pipeline API
│   ├── Chonkie.Porters/      # Content conversion and export
│   ├── Chonkie.Fetchers/     # Content retrieval
│   └── Chonkie.Handshakes/   # Handshake protocols
├── tests/                    # Unit and integration tests
├── samples/                  # Usage examples
├── benchmarks/               # Performance benchmarks
├── docs/                     # Technical documentation
├── models/                   # AI models (ONNX, etc.)
├── scripts/                  # Automation scripts
├── .editorconfig             # Code style configuration
├── Directory.Build.props     # Shared MSBuild properties
├── Chonkie.Net.sln           # Visual Studio solution
└── test.runsettings          # Test execution settings
```

### Setup a local copy

Clone the repository and build. You should be able to generate the library and use it in your own projects.

```powershell
git clone https://github.com/gianni-rg/Chonkie.Net.git
cd Chonkie.Net
dotnet build
```

As alternative, you can get a pre-compiled binary version of the library on [NuGet](https://www.nuget.org/packages/Chonkie.NET/). Remember to thick 'Include prerelease' when looking for the `Chonkie.NET` library.

### Let's chunk

If you are familiar with the Python library, this mirrors the Python quick-start: create a new console application, create a chunker, pass some text, and read chunks.

```csharp
using Chonkie.Chunkers;
using Chonkie.Tokenizers;

var text = "Woah! Chonkie, the chunking library is so cool!";

var tokenizer = new WordTokenizer();
var chunker = new TokenChunker(tokenizer, chunkSize: 64, chunkOverlap: 8);

var chunks = chunker.Chunk(text);
foreach (var chunk in chunks)
{
    Console.WriteLine($"Chunk: {chunk.Text}");
    Console.WriteLine($"Tokens: {chunk.TokenCount}");
}
```

### Pipeline (Optional)

```csharp
using Chonkie.Chunkers;
using Chonkie.Pipeline;
using Chonkie.Refineries;
using Chonkie.Tokenizers;

var tokenizer = new WordTokenizer();

var result = await FluentPipeline.Create()
    .WithText("Chonkie is the best tool!")
    .ChunkWith(new RecursiveChunker(tokenizer, chunkSize: 128))
    .RefineWith(new OverlapRefinery(minOverlap: 8))
    .RunAsync();
```

## Documentation

You can find all the technical documentation in the [docs/](docs/) folder. There are some getting started guides, and in the [samples/](/samples) folder you can find more complete examples of how to use the library and its features.

If you want to have a look at the original Chonkie and its documentation, you can find it here:

- [https://github.com/chonkie-inc/chonkie](https://github.com/chonkie-inc/chonkie)
- [https://docs.chonkie.ai](https://docs.chonkie.ai)

## Contribution

The project is constantly evolving and contributions are warmly welcomed.

I'm more than happy to receive any kind of contribution to this experimental project: from helpful feedbacks to bug reports, documentation, usage examples, feature requests, or directly code contribution for bug fixes and new and/or improved features.

Feel free to file issues and pull requests on the repository and I'll address them as much as I can, *with a best effort approach during my spare time*.

> Development is mainly done on Windows, so other platforms are not directly developed, tested or supported (but the CI/CD pipeline builds and tests for macOS and Linux as well).  
> An help is kindly appreciated in make the application work on other platforms as well.

Check out [CONTRIBUTING.md](CONTRIBUTING.md) to get started!

## Acknowledgements

Inspired by the amazing Python community and the need for a .NET equivalent, this project could not have been possible without their work: the original Chonkie has been created by [Bhavnick Minhas](https://github.com/bhavnicksm) and [Shreyash Nigam](https://github.com/Primus). Please check out the original project for more details and to support their work on [GitHub](https://github.com/chonkie-inc/chonkie).

## License

You may find specific license information for third party software in the [third-party-programs.txt](./third-party-programs.txt) file.  
Where not otherwise specified, everything is licensed under the [APACHE 2.0 License](./LICENSE).

Copyright (C) 2025-2026 Gianni Rosa Gallina.
