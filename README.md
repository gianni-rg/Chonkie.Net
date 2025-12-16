# 🦛 Chonkie.NET ✨

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/github/license/gianni-rg/Chonkie.Net.svg)](https://github.com/gianni-rg/Chonkie.Net/blob/main/LICENSE)
[![Status](https://img.shields.io/badge/status-phase_2_in_progress-green)](PORT_PLAN.md)
[![Tests](https://img.shields.io/badge/tests-50_passing-brightgreen)](tests/)

> **Status:** ✅ Phase 1 Complete - Foundation Established | 🚀 Phase 2 In Progress - Core Chunkers  
> **Latest:** 🔥 .NET 10 RTM & C# 14 enhancement plan ready - see [docs/DOTNET10_CSHARP14_ENHANCEMENT_PLAN.md](docs/DOTNET10_CSHARP14_ENHANCEMENT_PLAN.md)

A .NET port of [Chonkie](https://github.com/chonkie-inc/chonkie) - the no-nonsense, ultra-lightweight text chunking library for RAG applications.

## 📋 About This Port

Chonkie.NET is a faithful port of the Python Chonkie library to .NET/C#, bringing powerful text chunking capabilities to the .NET ecosystem with:

- ✨ **Feature Parity** - All chunking strategies from the Python version
- 🚀 **High Performance** - Leveraging .NET 10 & C# 14 (Span<T>, SIMD, extension members, stack allocation)
- 🪶 **Lightweight** - Minimal dependencies, modular NuGet packages
- 🔌 **32+ Integrations** - Vector databases, embedding providers, LLM services
- 💪 **Strongly Typed** - Full C# 14 type safety and modern language features
- ⚡ **Optimized** - Stack allocation, devirtualization, TensorPrimitives SIMD operations

## 🎯 Project Goals

1. **Complete Feature Parity** with Python Chonkie
2. **Native .NET Experience** - Idiomatic C# APIs, DI support, IOptions pattern
3. **Performance** - Match or exceed Python performance
4. **Excellent Documentation** - XML docs, samples, migration guides

## 📚 Documentation

- **[Port Plan](PORT_PLAN.md)** - Comprehensive migration plan and progress tracking
- **[.NET 10 & C# 14 Enhancements](docs/DOTNET10_CSHARP14_ENHANCEMENT_PLAN.md)** - Modern .NET features & optimizations
- **[Changelog](CHANGELOG.md)** - Project changes and version history
- **Original Chonkie** - [GitHub](https://github.com/chonkie-inc/chonkie) | [Docs](https://docs.chonkie.ai)

## 🚀 Planned Features

### Core Chunkers
- `TokenChunker` - Fixed-size token chunks
- `SentenceChunker` - Sentence-boundary aware
- `RecursiveChunker` - Hierarchical splitting
- `SemanticChunker` - Similarity-based chunking
- `LateChunker` - Embed-then-chunk
- `CodeChunker` - Code-aware chunking
- `NeuralChunker` - ML-based chunking
- `SlumberChunker` - LLM-guided chunking

### Integrations

**Tokenizers:** Character, Word, HuggingFace (ML.NET), tiktoken (SharpToken)

**Embeddings:** OpenAI, Azure OpenAI, Cohere, Gemini, Jina AI, Voyage AI, Sentence Transformers (ONNX)

**Vector Databases:** ChromaDB, Qdrant, Pinecone, Weaviate, PostgreSQL (pgvector), MongoDB, Elasticsearch, Turbopuffer

**LLM Providers:** OpenAI, Azure OpenAI, Gemini (+ OpenRouter compatible)

## 🗓️ Timeline

**Current Phase:** Phase 2 - Core Chunkers (In Progress)  
**Phase 1:** ✅ Complete - Foundation established with 50 passing tests  
**Estimated v1.0 Release:** Week 18

See [PORT_PLAN.md](PORT_PLAN.md) for the complete 10-phase migration plan.

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
└── PORT_PLAN.md             # Detailed port plan
```

## 🤝 Contributing

This project is in the early planning phase. Contributions will be welcome once the foundation is established. 

If you're interested in helping with the port:
1. Check the [PORT_PLAN.md](PORT_PLAN.md) for current status
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
