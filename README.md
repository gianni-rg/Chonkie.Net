# 🦛 Chonkie.NET ✨

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/github/license/gianni-rg/Chonkie.Net.svg)](https://github.com/gianni-rg/Chonkie.Net/blob/main/LICENSE)
[![Status](https://img.shields.io/badge/status-phase_11_in_progress-green)](STATUS_DASHBOARD.md)
[![Tests](https://img.shields.io/badge/tests-739_passing-brightgreen)](tests/)

> **Status:** ✅ Core implementation complete | 🟡 Phase 11 in progress (docs + release)  
> **Latest:** Optional chunkers, genies, and handshakes are complete with full test coverage

A .NET port of [Chonkie](https://github.com/chonkie-inc/chonkie) - the no-nonsense, ultra-lightweight text chunking library for RAG applications.

## 📋 About This Port

Chonkie.NET is a faithful port of the Python Chonkie library to .NET/C#, bringing powerful text chunking capabilities to the .NET ecosystem with:

- ✨ **Feature Parity** - Chunkers, pipelines, and integrations aligned with Python behavior
- 🚀 **High Performance** - .NET 10 + C# 14 (Span<T>, SIMD, extension members, stack allocation)
- 🪶 **Lightweight** - Modular packages, minimal dependencies
- 🔌 **32+ Integrations** - Vector DBs, embeddings, LLM providers
- 💪 **Strongly Typed** - Idiomatic C# APIs and DI-friendly design
- ⚡ **Optimized** - TensorPrimitives SIMD operations and low allocations

## 🚀 Quick Start

### Install

**NuGet packages:** Coming in Phase 11.  
For now, build from source and reference the projects you need.

```powershell
git clone https://github.com/gianni-rg/Chonkie.Net.git
cd Chonkie.Net
dotnet build Chonkie.Net.sln
```

### CHONK! 🦛✨

This mirrors the Python quick-start: create a chunker, pass text, and read chunks.

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
    .WithText("Chonkie is the goodest boi!")
    .ChunkWith(new RecursiveChunker(tokenizer, chunkSize: 128))
    .RefineWith(new OverlapRefinery(minOverlap: 8))
    .RunAsync();
```

## 📚 Documentation

### 🎓 **[Complete Tutorial Guide](docs/TUTORIALS_INDEX.md)** ← Start Here!
Learn Chonkie.Net step-by-step with interactive tutorials:
- [Quick-Start Guide](docs/TUTORIALS_01_QUICK_START.md) (10 min) - Your first chunks
- [RAG Tutorial](docs/TUTORIALS_02_RAG.md) (45 min) - Build a complete RAG system
- [Chunkers Deep Dive](docs/TUTORIALS_03_CHUNKERS.md) (30 min) - Master all 10 chunkers
- [Vector Database Guide](docs/TUTORIALS_04_VECTORDB.md) (30 min) - Store & search embeddings
- [Pipeline Configuration](docs/TUTORIALS_05_PIPELINES.md) (60 min) - Advanced workflows

### 📊 Project Documentation
- **[Status Dashboard](STATUS_DASHBOARD.md)** - Current status and sprint tracking
- **[Master Roadmap](MASTER_ROADMAP.md)** - Consolidated roadmap and progress tracking
- **[Implementation Quickstart](IMPLEMENTATION_QUICKSTART.md)** - Implementation guide
- **[Changelog](CHANGELOG.md)** - Version history
- **Original Chonkie** - [GitHub](https://github.com/chonkie-inc/chonkie) | [Docs](https://docs.chonkie.ai)

## 🧩 Component Overview

### Chunkers
`TokenChunker`, `SentenceChunker`, `RecursiveChunker`, `SemanticChunker`, `LateChunker`,
`CodeChunker`, `TableChunker`, `NeuralChunker`, `SlumberChunker`, `FastChunker`

### Tokenizers
Character, Word, HuggingFace (ML.NET), tiktoken (SharpToken)

### Embeddings
OpenAI, Azure OpenAI, Cohere, Gemini, Jina AI, Voyage AI, Sentence Transformers (ONNX)

### Vector Databases
ChromaDB, Qdrant, Pinecone, Weaviate, PostgreSQL (pgvector), MongoDB, Elasticsearch, Turbopuffer

### LLM Providers
OpenAI, Azure OpenAI, Gemini, Groq, Cerebras

## 🎯 Project Goals

1. **Feature Parity** with Python Chonkie
2. **Native .NET Experience** - Idiomatic C# APIs, DI support, IOptions pattern
3. **Performance** - Match or exceed Python performance
4. **Excellent Documentation** - XML docs, samples, migration guides

## 🗓️ Timeline

**Current Phase:** Phase 11 - Polish & Release (in progress)  
**Core Implementation:** ✅ Complete (Phases 1-10)  
**Release Target:** After documentation + packaging are finalized

See [MASTER_ROADMAP.md](MASTER_ROADMAP.md) for the complete plan.

## 🏗️ Project Structure

```
Chonkie.Net/
├── src/                      # Source code
│   ├── Chonkie.Core/         # Core types and interfaces
│   ├── Chonkie.Chunkers/     # Chunker implementations
│   ├── Chonkie.Embeddings/   # Embedding providers
│   └── ...
├── tests/                    # Unit and integration tests
├── benchmarks/               # Performance benchmarks
├── samples/                  # Usage examples
├── docs/                     # Documentation
└── MASTER_ROADMAP.md         # Consolidated roadmap
```

## 🤝 Contributing

Contributions are welcome while we finalize Phase 11 (docs + release prep).

If you'd like to help:
1. Check the [STATUS_DASHBOARD.md](STATUS_DASHBOARD.md) for current priorities
2. Look for issues labeled `help-wanted` or `good-first-issue`
3. Join the discussion in issues and pull requests

## 📄 License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgements

- **Original Chonkie** - Created by [Bhavnick Minhas](https://github.com/bhavnicksm) and [Shreyash Nigam](https://github.com/Primus)
- **Python Chonkie** - [GitHub](https://github.com/chonkie-inc/chonkie)
- Inspired by the amazing Python community and the need for a .NET equivalent

## 📞 Contact

- **Repository:** [github.com/gianni-rg/Chonkie.Net](https://github.com/gianni-rg/Chonkie.Net)
- **Issues:** [GitHub Issues](https://github.com/gianni-rg/Chonkie.Net/issues)
- **Original Chonkie Discord:** [Join](https://discord.gg/vH3SkRqmUz)

---

**Note:** This is an independent port and is not officially affiliated with the original Chonkie project. All credit for the original design and implementation goes to the Chonkie team.
