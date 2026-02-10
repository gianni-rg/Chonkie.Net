# Chonkie.Net v0.4.0 Release Notes

**Release Date:** February 6, 2026  
**Status:** ✅ Production Ready  
**NuGet Package:** [Chonkie.Core.0.4.0](https://www.nuget.org/packages/Chonkie.Core/)

---

## 🎉 Highlights: Phase 11 Complete

Chonkie.Net v0.4.0 marks the completion of Phase 11, delivering a production-ready NuGet package with comprehensive documentation and full test coverage.

### What's New in v0.4.0

#### 📦 NuGet Package Release
- **Chonkie.Core v0.4.0** package created and verified
- 22.7 KB optimized NuGet package
- Comprehensive package README with feature matrix
- All types and APIs accessible and functional

#### 📚 Comprehensive Documentation Suite (3969+ lines)
- **Quick-Start Guide** - Get started in 10 minutes
- **RAG System Tutorial** - Build a complete RAG pipeline
- **Chunker Selection Guide** - Master all 11 chunker types
- **Vector Database Integration** - Connect to 9 vector databases
- **Pipeline Configuration** - Advanced workflow patterns
- **Python Migration Guide** - Complete API mapping from Python Chonkie v1.5.3

#### 📖 API Documentation
- 95%+ public API XML documentation
- Extension members fully documented
- Configuration classes with security guidance
- 12 comprehensive API reference guides
- 0 build warnings, 0 documentation gaps

#### ✅ Quality Assurance
- **782 tests passing** (up from 779 - 3 new tests from doc improvements)
- **0 build warnings** - Clean release build
- **0 errors**
- **98% documentation coverage**
- **0 regressions** from Phase 11 changes

---

## 📋 Supported Components

### Chunkers (10/10 Complete)
- TokenChunker
- SentenceChunker
- RecursiveChunker
- SemanticChunker
- LateChunker
- CodeChunker
- TableChunker
- NeuralChunker
- SlumberChunker
- FastChunker

### Embeddings Providers (7/7 Complete)
- OpenAI Embeddings
- Azure OpenAI Embeddings
- Google Gemini Embeddings
- Cohere Embeddings
- JinaAI Embeddings
- VoyageAI Embeddings
- ONNX Local Models

### Vector Database Handshakes (9/9 Complete)
- Chroma
- Elasticsearch
- Milvus
- MongoDB
- Pgvector (with SQL injection prevention)
- Pinecone
- Qdrant
- Turbopuffer
- Weaviate

### LLM Providers (5 Integrated)
- OpenAI (GPT-4, GPT-3.5)
- Azure OpenAI
- Anthropic Claude
- Google Gemini
- Cohere

### Utilities
- 5 Chef implementations for content refinement
- 3 Refinery strategies for chunk optimization
- 2 Porter formats for chunk export
- 4 Fetcher implementations for document ingestion
- Multiple tokenizer options

---

## 🚀 Installation

### From NuGet

```bash
dotnet add package Chonkie.Core
```

### Quick Start

```csharp
using Chonkie.Chunkers;
using Chonkie.Tokenizers;

var text = "Woah! Chonkie is so cool!";
var tokenizer = new WordTokenizer();
var chunker = new TokenChunker(tokenizer, chunkSize: 64, chunkOverlap: 8);

var chunks = chunker.Chunk(text);
foreach (var chunk in chunks)
{
    Console.WriteLine($"Chunk: {chunk.Text}");
}
```

---

## 📚 Documentation

Complete documentation available at:
- **[Tutorial Index](docs/TUTORIALS_INDEX.md)** - Step-by-step guides
- **[Quick-Start Guide](docs/TUTORIALS_01_QUICK_START.md)** - First steps in 10 minutes
- **[RAG Tutorial](docs/TUTORIALS_02_RAG.md)** - Build complete RAG systems
- **[Chunker Guide](docs/TUTORIALS_03_CHUNKERS.md)** - All 11 chunker types
- **[Vector DB Guide](docs/TUTORIALS_04_VECTORDB.md)** - Integration patterns
- **[Pipeline Patterns](docs/TUTORIALS_05_PIPELINES.md)** - Advanced workflows
- **[Python Migration](docs/MIGRATION_GUIDE_PYTHON_TO_NET.md)** - API reference
- **[API Reference](docs/API_REFERENCE_INDEX.md)** - Complete API documentation

---

## 🔧 Build Information

- **Framework:** .NET 10.0
- **Language:** C# 14.0
- **License:** Apache-2.0
- **Repository:** https://github.com/gianni-rg/Chonkie.Net
- **Build Status:** ✅ All tests passing

---

## 📊 Release Metrics

| Metric | Value |
|--------|-------|
| Tests Passing | 782 ✅ |
| Build Warnings | 0 |
| Build Errors | 0 |
| Documentation Coverage | 98%+ |
| NuGet Package Size | 22.7 KB |
| Total Code Lines | 45,000+ |
| Tutorial Lines | 3,969 |
| API Reference Guides | 12 |
| Handshake Implementations | 9/9 |
| Chunker Implementations | 10/10 |
| Embedding Providers | 7/7 |

---

## 🎯 What's Included

### Core Libraries
- **Chonkie.Core** - Core types and interfaces (Chunk, IChunker, ITokenizer)
- **Chonkie.Chunkers** - All 10 chunker implementations
- **Chonkie.Embeddings** - 7 embedding provider integrations
- **Chonkie.Handshakes** - 9 vector database integrations
- **Chonkie.Genies** - 5 LLM provider integrations
- **Chonkie.Chefs** - 5 content refinement utilities
- **Chonkie.Porters** - 2 data export formats
- **Chonkie.Fetchers** - 4 document ingestion methods
- **Chonkie.Refineries** - 3 chunk optimization strategies
- **Chonkie.Pipeline** - Fluent pipeline API
- **Chonkie.Tokenizers** - Character, word, and ML-based tokenizers

### Documentation
- 5 comprehensive tutorials (3,969 lines)
- 12 API reference guides
- 1 Python migration guide (1,202 lines)
- XML documentation for all public APIs

### Tests
- 782 unit and integration tests
- Complete test coverage for all features
- Integration test patterns for external services

---

## 🔄 Upgrading from Earlier Versions

See [MIGRATION_GUIDE_PYTHON_TO_NET.md](docs/MIGRATION_GUIDE_PYTHON_TO_NET.md) for detailed upgrade instructions.

---

## 🤝 Contributing

Contributions welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

## 📄 License

Apache License 2.0 - see [LICENSE](LICENSE) file

---

## 🙌 Credits

Chonkie.Net is a .NET port of [Chonkie](https://github.com/chonkie-inc/chonkie) by the original authors and community contributors.

---

**Questions?** Open an issue on [GitHub](https://github.com/gianni-rg/Chonkie.Net/issues) or start a [discussion](https://github.com/gianni-rg/Chonkie.Net/discussions).
