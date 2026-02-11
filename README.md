# 🦛 Chonkie.Net

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/github/license/gianni-rg/Chonkie.Net.svg)](https://github.com/gianni-rg/Chonkie.Net/blob/main/LICENSE)
[![Status](https://img.shields.io/badge/status-experimental-green)](STATUS_DASHBOARD.md)
[![Tests](https://img.shields.io/badge/tests-930_passing-brightgreen)](tests/)

A .NET port of [Chonkie](https://github.com/chonkie-inc/chonkie), the lightweight ingestion Python library for fast, efficient and robust RAG pipelines.

> *This is an independent port and is not officially affiliated with the original Chonkie project.  
All credit for the original design and implementation goes to the Chonkie team.*

Chonkie.Net is a somewhat faithful port of the Python Chonkie library to .NET/C#, bringing powerful text chunking capabilities to the .NET ecosystem with:

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
├── scripts/                  # Automation and helper scripts
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

As alternative, you can get a pre-compiled binary version of the library on [NuGet](https://www.nuget.org/packages/Chonkie.Net/). Remember to tick 'Include prerelease' when looking for the `Chonkie.Net` library.

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

## Tests

The project includes a comprehensive test suite with unit and integration tests. You can run the tests from Visual Studio Code or Visual Studio Test Explorer. As an alternative, you can run the tests with the .NET CLI, from the root of the repository:

```shell
dotnet test
```

### Integration test dependencies

> Generally integration tests should be able to run out of the box, but some of them require specific dependencies, such as ONNX models or API keys for third party services. If those dependencies are not present, the related tests will be *skipped* with a warning message.

`test.runsettings` file can be used to configure a set of variables for specifying paths and other settings. You can also override them with OS environment variables.

- **ONNX models**: Place required ONNX models in the `./models/` folder or set an environment variable pointing to a models directory (see examples below). Embedding integration tests iterate over the models present in that folder (see [./scripts/README.md](scripts/README.md) for details).

  To run embedding integration tests for all supported models, use the provided script:

  ```shell
  ./tests/Run-EmbeddingIntegration-AllModels.ps1
  ```

- **Third party services**: some tests require access to third party services, such as Azure OpenAI, Gemini, Cohere, etc. You can set the required API keys and endpoints as OS environment variables, or use the `test.runsettings` file to specify them. Tests that require those services will be skipped if the required variables are not set.

- **Local Infrastructure**: some tests require access to a local infrastructure, such as a local LLM server or a local vector database. You can set the required endpoints and credentials as OS environment variables, or use the `test.runsettings` file to specify them. Tests that require those services will be skipped if the required variables are not set. A preset docker compose file with all the required infrastructure for local dev/testing is available in `docker-compose.handshakes.yml`.

  Just run `docker compose -f docker-compose.handshakes.yml up -d` or `podman compose -f docker-compose.handshakes.yml up -d` to get everything up and running.

## ONNX Models

Some of the chunkers and embedding providers rely on neural models, that can be converted to ONNX format for runtime use in .NET with ONNX Runtime. You can find some scripts in the `scripts/` folder to convert HuggingFace models or local checkpoints to ONNX format, and then use them in .NET.

Please check the [scripts/README.md](scripts/README.md) for more details and examples.

## Documentation

You can find all the technical documentation in the [docs/](docs/) folder. There are some getting started guides, and in the [samples/](/samples) folder you can find more complete examples of how to use the library and its features.

If you want to have a look at the original Chonkie and its documentation, you can find it here:

- [https://github.com/chonkie-inc/chonkie](https://github.com/chonkie-inc/chonkie)
- [https://docs.chonkie.ai](https://docs.chonkie.ai)

## Contribution

The project is constantly evolving and contributions are warmly welcomed.

I'm more than happy to receive any kind of contribution to this experimental project: from helpful feedbacks to bug reports, documentation, usage examples, feature requests, or directly code contribution for bug fixes and new and/or improved features.

Feel free to file issues and pull requests on the repository and I'll address them as much as I can, *with a best effort approach during my spare time*.

> Development is mainly done on Windows, so other platforms are not directly developed, tested, or supported (although the CI/CD pipeline builds and tests for macOS and Linux as well). Help is kindly appreciated in making the libraries work on other platforms as well.

Check out [CONTRIBUTING.md](CONTRIBUTING.md) to get started!

## Acknowledgements

Inspired by the amazing Python community and the need for a .NET equivalent, this project could not have been possible without their work: the original Chonkie has been created by [Bhavnick Minhas](https://github.com/bhavnicksm) and [Shreyash Nigam](https://github.com/Primus). Please check out the original project for more details and to support their work on [GitHub](https://github.com/chonkie-inc/chonkie).

## License

This project, and where not otherwise specified, is licensed under the [APACHE 2.0 License](./LICENSE).

Copyright (C) 2025-2026 Gianni Rosa Gallina.
