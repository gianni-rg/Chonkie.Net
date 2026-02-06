# 🦛 Chonkie.Net - The Lightweight RAG Ingestion Library

[![GitHub](https://img.shields.io/badge/GitHub-gianni--rg%2FChonkie.Net-blue?logo=github)](https://github.com/gianni-rg/Chonkie.Net)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=.net)](https://dotnet.microsoft.com)
[![C# 14](https://img.shields.io/badge/C%23-14-239120?logo=csharp)](https://docs.microsoft.com/en-us/csharp)

**Chonkie.Net** is a production-ready C# port of Python Chonkie, providing fast, efficient, and robust text chunking for Retrieval-Augmented Generation (RAG) systems.

## ✨ Key Features

- 🚀 **Fast & Efficient** - 10-100x faster than Python implementations
- 🔀 **11 Specialized Chunkers** - Choose the right chunker for your data type
- 🧠 **7+ Embedding Providers** - OpenAI, Azure, Gemini, Cohere, VoyageAI, Jina, and more
- 🗄️ **9 Vector Database Integrations** - Pinecone, Qdrant, Chroma, Weaviate, MongoDB, Pgvector, Elasticsearch, Milvus, Turbopuffer
- 🤖 **5 LLM Providers** - OpenAI, Azure, Groq, Cerebras, Gemini
- ⚡ **ONNX Support** - Local embeddings with SentenceTransformers
- 🔗 **Complete RAG Pipeline** - End-to-end document processing for RAG
- 📦 **No Dependencies Bloat** - Minimal, modular architecture
- 🎯 **Type-Safe** - Full C# 14 nullable reference types support
- ✅ **779+ Tests** - Comprehensive unit and integration test suite

## 🚀 Quick Start

### Installation

```bash
dotnet add package Chonkie.Net
```

### Basic Chunking (30 seconds)

```csharp
using Chonkie.Chunkers;
using Chonkie.Tokenizers;

// Create a chunker
var chunker = new RecursiveChunker(
    tokenizer: new WordTokenizer(),
    chunkSize: 512,
    maxOverlap: 50
);

// Chunk your text
var text = "Your document here...";
var chunks = chunker.Chunk(text);

// Use the chunks
foreach (var chunk in chunks)
{
    Console.WriteLine($"Text: {chunk.Text}");
    Console.WriteLine($"Tokens: {chunk.TokenCount}");
}
```

### With Embeddings & Vector Database

```csharp
using Chonkie.Embeddings;
using Chonkie.Handshakes;

// Create embeddings
var embeddings = new OpenAIEmbeddings(
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!
);

// Create vector database connection
var vectorDb = new PineconeHandshake(
    apiKey: "your-pinecone-key",
    indexName: "my-index",
    embeddingModel: embeddings
);

// Embed and store chunks
var embeddedChunks = await embeddings.EmbedBatchAsync(chunks);
await vectorDb.WriteAsync(embeddedChunks);
```

## 📚 Documentation

- **[Quick Start Guide](https://github.com/gianni-rg/Chonkie.Net/blob/main/docs/TUTORIALS_01_QUICK_START.md)** - Get started in 5 minutes
- **[RAG System Tutorial](https://github.com/gianni-rg/Chonkie.Net/blob/main/docs/TUTORIALS_02_RAG.md)** - Build a complete RAG system
- **[Chunker Selection Guide](https://github.com/gianni-rg/Chonkie.Net/blob/main/docs/TUTORIALS_03_CHUNKERS.md)** - Choose the right chunker
- **[Vector Database Integration](https://github.com/gianni-rg/Chonkie.Net/blob/main/docs/TUTORIALS_04_VECTORDB.md)** - Connect to any vector DB
- **[Python Migration Guide](https://github.com/gianni-rg/Chonkie.Net/blob/main/docs/MIGRATION_GUIDE_PYTHON_TO_NET.md)** - Coming from Python Chonkie?
- **[Full Documentation](https://github.com/gianni-rg/Chonkie.Net)** - Complete API reference and guides

## 🔀 Chunkers (11 Types)

| Chunker | Best For | Speed |
|---------|----------|-------|
| **TokenChunker** | Simple, fast splitting | ⚡⚡⚡ |
| **RecursiveChunker** | Natural documents (RECOMMENDED) | ⚡⚡ |
| **SentenceChunker** | Sentence boundaries | ⚡⚡ |
| **SemanticChunker** | Meaning-aware grouping | ⚡ |
| **CodeChunker** | Source code | ⚡⚡ |
| **TableChunker** | Structured data | ⚡⚡ |
| **MarkdownChunker** | Markdown documents | ⚡⚡ |
| **LateChunker** | Two-stage processing | ⚡ |
| **NeuralChunker** | ONNX embeddings | ⚡ |
| **SlumberChunker** | Complex documents | ⚡ |
| **FastChunker** | High-speed splitting | ⚡⚡⚡ |

## 🧠 Embeddings (7+ Providers)

- OpenAI (text-embedding-3-small, text-embedding-3-large)
- Azure OpenAI
- Google Gemini
- Cohere
- VoyageAI
- Jina
- Local ONNX (SentenceTransformers)

## 🤖 LLM Providers (5 Types)

- OpenAI (GPT-4, GPT-3.5-turbo)
- Azure OpenAI
- Groq (fast inference)
- Cerebras (ultra-fast)
- Google Gemini

## 🗄️ Vector Databases (9 Integrations)

- **Pinecone** - Fully managed serverless
- **Qdrant** - Open-source vector search
- **Chroma** - Lightweight local embedding DB
- **Weaviate** - Open-source, flexible
- **MongoDB** - MongoDB Atlas Vector Search
- **PostgreSQL** - pgvector extension
- **Elasticsearch** - Search-optimized
- **Milvus** - High-performance distributed
- **Turbopuffer** - Real-time, edge-optimized

## 💡 Common Use Cases

### 1. Document Ingestion for RAG
```csharp
// Chunk documents, embed, and store in vector DB
var chunks = chunker.Chunk(document);
await vectorDb.WriteAsync(chunks);
```

### 2. Code Analysis
```csharp
var codeChunker = new CodeChunker(language: "csharp", chunkSize: 1024);
var chunks = codeChunker.Chunk(sourceCode);
```

### 3. Semantic Search
```csharp
var semanticChunker = new SemanticChunker(embeddings, threshold: 0.5f);
var chunks = semanticChunker.Chunk(text);
// Chunks grouped by semantic meaning
```

### 4. RAG Pipeline
```csharp
var pipeline = new Pipeline()
    .ProcessWith("text")
    .ChunkWith("recursive", new { chunk_size = 1024 })
    .RunAsync(texts: documentText);
```

## 🎯 Why Chonkie.Net?

✅ **Performance** - 10-100x faster than Python alternatives  
✅ **Type Safety** - Full C# 14 nullable reference support  
✅ **Dependency Minimal** - Modular, only install what you need  
✅ **Production Ready** - 779+ tests, zero warnings  
✅ **Well Documented** - 3969 lines of tutorials and guides  
✅ **Complete Features** - All major RAG components included  
✅ **Active Development** - Continuously updated with new features  

## 📋 Minimum Requirements

- **.NET 10.0** or higher
- **C# 14** features enabled
- Windows, Linux, or macOS

## 🤝 Contributing

Contributions are welcome! Please visit [GitHub Repository](https://github.com/gianni-rg/Chonkie.Net).

## 📄 License

Licensed under Apache License 2.0. See [LICENSE](LICENSE) for details.

## 🎓 Learn More

- **Official Repo:** https://github.com/gianni-rg/Chonkie.Net
- **Python Chonkie:** https://github.com/chonkie-inc/chonkie
- **Documentation:** Check the `/docs` folder in the repository

---

**Ready to build amazing RAG systems?** Start with the [Quick Start Guide](https://github.com/gianni-rg/Chonkie.Net/blob/main/docs/TUTORIALS_01_QUICK_START.md)! 🚀
