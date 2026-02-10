# 📚 Chonkie.Net - Complete Tutorial Guide

Welcome! This guide will help you learn Chonkie.Net from basics to advanced use cases.

---

## 🎯 Quick Navigation

### For Absolute Beginners (10 minutes)
Start here to understand the basics:
- **[Quick-Start Guide](TUTORIALS_01_QUICK_START.md)** - Your first chunking example
  - Install Chonkie.Net
  - CHONK! - Create your first chunks
  - Understand tokenizers
  - Try different chunker types
  - Introduction to pipelines

### For Building RAG Systems (1-2 hours)
Complete walk-through of Retrieval-Augmented Generation:
- **[RAG Tutorial](TUTORIALS_02_RAG.md)** - Build a complete RAG system
  - What is RAG and why it matters
  - CHOMP architecture overview
  - Document ingestion (chunking)
  - Embedding generation
  - Vector database storage
  - Retrieval at query time
  - LLM-based answer generation
  - Complete working example
  - Configuration tuning
  - Common patterns and pitfalls

### For Deep Diving into Chunkers (30-45 minutes)
Understand when and how to use each chunker:
- **[Chunkers Tutorial](TUTORIALS_03_CHUNKERS.md)** - Master all 10 chunkers
  - Overview of all chunkers
  - TokenChunker (simplest)
  - SentenceChunker (sentence boundaries)
  - RecursiveChunker (⭐ recommended)
  - SemanticChunker (meaning-aware)
  - CodeChunker (source code)
  - TableChunker (structured data)
  - MarkdownChunker (markdown)
  - LateChunker (two-stage)
  - NeuralChunker (local semantic)
  - SlumberChunker (complex docs)
  - FastChunker (high-speed)
  - Decision matrix (which to use?)
  - Performance comparison

### For Vector Database Integration (30-45 minutes)
Store and search your embeddings:
- **[Vector Database Tutorial](TUTORIALS_04_VECTORDB.md)** - All 9 databases
  - What is a vector database?
  - Qdrant (local/cloud)
  - Pinecone (fully managed)
  - Weaviate (open-source)
  - Chroma (lightweight)
  - PostgreSQL + pgvector (SQL)
  - MongoDB (document + vectors)
  - Elasticsearch (search-optimized)
  - Milvus (high-performance)
  - Turbopuffer (edge/real-time)
  - Universal Handshake interface
  - Search patterns & advanced queries
  - Database selection guide

### For Advanced Pipeline Configuration (40-60 minutes)
Build production-ready data processing pipelines:
- **[Pipeline Tutorial](TUTORIALS_05_PIPELINES.md)** - Master the CHOMP architecture
  - CHOMP architecture explained
  - Fetchers (data sources)
  - Chefs (preprocessing)
  - Chunkers (splitting)
  - Refineries (post-processing)
  - Porters (export)
  - Handshakes (vector DB storage)
  - Complete production example
  - Pipeline validation
  - Performance optimization
  - Troubleshooting common issues

---

## 📊 Recommended Learning Paths

### Path 1: I Just Want to Chunk Text (30 minutes)
```
1. Quick-Start Guide (10 min)
   └─ Learn basic chunking
2. Chunkers Tutorial (20 min)
   └─ Choose the right chunker
```

**Result:** You can chunk documents effectively.

---

### Path 2: I Want to Build a RAG System (2-3 hours)
```
1. Quick-Start Guide (10 min)
   └─ Understand basics
2. RAG Tutorial (45 min)
   └─ Learn full RAG flow
3. Chunkers Tutorial (25 min)
   └─ Choose optimal chunker
4. Vector Database Tutorial (30 min)
   └─ Pick and configure database
5. Build your first system!
```

**Result:** You can build a complete Q&A system over your documents.

---

### Path 3: I Need Production-Ready Code (4-5 hours)
```
1. Quick-Start Guide (10 min)
2. RAG Tutorial (45 min)
3. Chunkers Tutorial (25 min)
4. Vector Database Tutorial (30 min)
5. Pipeline Tutorial (60 min)
   └─ Advanced patterns
6. Implement production pipeline
7. Add monitoring & logging
8. Deploy!
```

**Result:** You have a scaled, monitored, production-ready system.

---

### Path 4: I'm Migrating from Python Chonkie (2-3 hours)
```
1. Quick-Start Guide (10 min)
   └─ See C# equivalents
2. Chunkers Tutorial (20 min)
   └─ Same classes, different syntax
3. RAG Tutorial (45 min)
   └─ See pattern differences
4. Vector Database Tutorial (30 min)
   └─ Same Handshakes interface
5. Migration Guide (coming soon)
   └─ API equivalents
```

**Result:** You know how to port Python code to C#.

---

## 🎓 Learning Tips

### 1. **Try Examples Incrementally**
Don't just read - actually run the code! Each tutorial has working examples you can copy.

```csharp
// Start with this
var chunker = new TokenChunker(new WordTokenizer(), chunkSize: 512);
var chunks = chunker.Chunk("Your text here");

// Then try this
var chunker = new RecursiveChunker(new WordTokenizer(), chunkSize: 512);
var chunks = chunker.Chunk("Your text here");

// See the difference!
```

---

### 2. **Experiment with Parameters**
The "right" settings depend on your data. Try different combinations:

```csharp
// Test different chunk sizes
foreach (var size in new[] { 128, 256, 512, 1024 })
{
    var chunker = new TokenChunker(tokenizer, chunkSize: size);
    var chunks = chunker.Chunk(yourText);
    Console.WriteLine($"Size {size}: {chunks.Count} chunks");
}
```

---

### 3. **Understand the "Why"**
Each tutorial explains not just *how* to use features, but *why* you'd use them.

- Why use TokenChunker? (Simple, fast)
- Why use SemanticChunker? (Better meaning preservation)
- Why use Qdrant? (Modern, free, local-first)
- Why use Pinecone? (Managed, no ops burden)

---

### 4. **Compare with Python**
Chonkie.Net is faithful to Python Chonkie. Compare side-by-side:

**Python:**
```python
chunker = RecursiveChunker(chunk_size=512, chunk_overlap=50)
chunks = chunker.chunk(text)
```

**C#:**
```csharp
var chunker = new RecursiveChunker(tokenizer, chunkSize: 512, chunkOverlap: 50);
var chunks = chunker.Chunk(text);
```

Notice: Same logic, similar API, C# benefits (types, compiled, faster).

---

## 🔗 Document Map

```
┌─────────────────────────────────────────────┐
│         Chonkie.Net Tutorials               │
├─────────────────────────────────────────────┤
│                                             │
│  TUTORIALS_01_QUICK_START.md      ← Start   │
│        ↓                                    │
│  TUTORIALS_02_RAG.md                        │
│        ↓                                    │
│  TUTORIALS_03_CHUNKERS.md                   │
│  TUTORIALS_04_VECTORDB.md    ← Pick path    │
│  TUTORIALS_05_PIPELINES.md                  │
│        ↓                                    │
│  Build Your System!                         │
│                                             │
└─────────────────────────────────────────────┘
```

---

## 📋 All Available Material

### Getting Started
- [Quick-Start Guide](TUTORIALS_01_QUICK_START.md) - First steps (10 min)
- [Installation](TUTORIALS_01_QUICK_START.md#-installation) - Setup guide

### Core Concepts
- [RAG System Tutorial](TUTORIALS_02_RAG.md) - Complete RAG (45 min)
- [Chunkers Deep Dive](TUTORIALS_03_CHUNKERS.md) - All 10 chunkers (30 min)
- [Vector Databases](TUTORIALS_04_VECTORDB.md) - All 9 databases (30 min)
- [Pipelines](TUTORIALS_05_PIPELINES.md) - Advanced workflows (60 min)

### Additional Resources (Coming Soon)
- Migration Guide (Python → C#)
- Production Deployment Guide
- Performance Tuning Guide
- Troubleshooting FAQ

### API References (Coming Soon)
- Chunkers API Reference
- Tokenizers API Reference
- Embeddings API Reference
- Vector Databases API Reference

---

## ❓ Frequently Asked Questions

### Q: Which tutorial should I start with?
**A:** Start with [Quick-Start Guide](TUTORIALS_01_QUICK_START.md) - it takes 10 minutes and gets you chunking immediately.

---

### Q: Can I skip tutorials?
**A:** Yes! But it's not recommended. Each tutorial builds on previous concepts:
- Skip Quick-Start only if you already understand tokenization
- RAG builds on Quick-Start concepts
- Chunkers deep dive helps optimize RAG
- Vector DB shows where chunks go
- Pipelines orchestrates everything

---

### Q: Are there code samples I can copy?
**A:** Yes! Every tutorial has complete, working code examples you can copy directly.

---

### Q: What if my use case isn't covered?
**A:** The tutorials cover 95% of common use cases. For advanced scenarios:
1. Check the RAG tutorial (most comprehensive)
2. Check the Pipelines tutorial (most flexible)
3. See the [Advanced Git Discussions](https://github.com/gianni-rg/Chonkie.Net/discussions)

---

### Q: Do I need an API key to start?
**A:** No! You can learn with:
- `TokenChunker` - no API needed
- `RecursiveChunker` - no API needed
- Local Qdrant - no API needed
- Local ONNX embeddings - no API needed

Only advanced tutorials (SemanticChunker, Pinecone, OpenAI) need API keys.

---

### Q: How long does it take to build a RAG system?
**A:**
- Quick prototype: 30 minutes (follow RAG tutorial)
- Production-ready: 2-3 hours (add pipelines, error handling, logging)
- Fully optimized: 1-2 weeks (benchmarking, tuning, monitoring)

---

## 🚀 Getting Help

- **Stuck in a tutorial?** - Check the Troubleshooting section at the end
- **Code isn't working?** - Try the [Quick-Start](TUTORIALS_01_QUICK_START.md#-troubleshooting) troubleshooting guide
- **Questions?** - Open an issue on [GitHub](https://github.com/gianni-rg/Chonkie.Net/issues)

---

## 📞 Next Steps After Tutorials

Once you've completed the tutorials:

1. **Build Something** - Apply what you learned to your data
2. **Benchmark** - Test different configs on your dataset
3. **Optimize** - Tune parameters for your use case
4. **Scale** - Move from local to production deployment
5. **Monitor** - Add logging and observability

---

## 📖 Table of Contents by Topic

### Chunking
- [Quick-Start: Different Chunker Types](TUTORIALS_01_QUICK_START.md#-different-chunker-types)
- [Chunkers Deep Dive](TUTORIALS_03_CHUNKERS.md)
- [RAG: Choosing Chunking Strategy](TUTORIALS_02_RAG.md#-configuration-tuning)

### Vectorization  
- [Quick-Start: Tokenizers](TUTORIALS_01_QUICK_START.md#-tokenizers-explained)
- [RAG: Embedding Generation](TUTORIALS_02_RAG.md#step-2-embedding-generation)

### Storage & Search
- [Vector Database Tutorial](TUTORIALS_04_VECTORDB.md)
- [RAG: Store & Retrieve](TUTORIALS_02_RAG.md#step-3-store-in-vector-database)

### Workflows
- [Pipeline Tutorial](TUTORIALS_05_PIPELINES.md)
- [RAG: Complete Architecture](TUTORIALS_02_RAG.md#-rag-architecture)

---

**Happy Learning! 🚀 Start with [Quick-Start Guide](TUTORIALS_01_QUICK_START.md) →**
