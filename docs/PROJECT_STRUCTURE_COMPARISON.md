# Chonkie.Net Project Structure vs Python Chonkie

**Purpose:** Show the actual folder/file organization and what's implemented

---

## Chonkie.Net (.NET 10/C#14) Structure

```
src/
├── Chonkie.Core/
│   ├── Types/
│   │   ├── Chunk.cs              ✅ Core chunk type
│   │   ├── Document.cs           ✅ Core document type
│   │   └── Metadata.cs           ✅ Metadata types
│   ├── Pipeline/
│   │   ├── Pipeline.cs           ✅ Main orchestrator
│   │   └── Builders/             ✅ Fluent builders
│   └── Interfaces/
│       └── IChunker.cs           ✅ Base interface
│
├── Chonkie.Chunkers/
│   ├── BaseChunker.cs            ✅ Abstract base
│   ├── TokenChunker.cs           ✅ Token-based
│   ├── SentenceChunker.cs        ✅ Sentence-based
│   ├── RecursiveChunker.cs       ✅ Hierarchical
│   ├── SemanticChunker.cs        ✅ Semantic (ML)
│   ├── CodeChunker.cs            ✅ Code-aware
│   ├── LateChunker.cs            ✅ Embed-then-chunk
│   ├── SlumberChunker.cs         ✅ Progressive window
│   ├── TableChunker.cs           ✅ Table-aware
│   └── NeuralChunker.cs          ✅ ONNX-enhanced
│       └── Onnx/
│           ├── ModelManager.cs   ✅ Model loading
│           └── PoolingUtils.cs   ✅ Pooling operations
│
├── Chonkie.Embeddings/
│   ├── Base/
│   │   └── IEmbeddings.cs        ✅ Interface
│   ├── OpenAI/
│   │   └── OpenAIEmbeddings.cs   ✅ OpenAI API
│   ├── Azure/
│   │   └── AzureOpenAIEmbeddings.cs ✅ Azure OpenAI
│   ├── Gemini/
│   │   └── GeminiEmbeddings.cs   ✅ Google Gemini
│   ├── Jina/
│   │   └── JinaEmbeddings.cs     ✅ Jina AI
│   ├── Cohere/
│   │   └── CohereEmbeddings.cs   ✅ Cohere
│   ├── VoyageAI/
│   │   └── VoyageAIEmbeddings.cs ✅ VoyageAI
│   ├── SentenceTransformers/
│   │   ├── SentenceTransformerEmbeddings.cs  ✅ Local ONNX
│   │   ├── ModelConfig.cs        ✅ Config
│   │   └── PoolingUtilities.cs   ✅ Pooling
│   ├── AutoEmbeddings.cs         🟡 STUB (not functional)
│   ├── VectorMath.cs             ✅ Math utilities
│   └── Extensions/               ✅ Helper methods
│
├── Chonkie.Chefs/
│   ├── Base/
│   │   └── IChef.cs              ✅ Interface
│   ├── TextChef.cs               ✅ Plain text
│   ├── MarkdownChef.cs           ✅ Markdown
│   ├── CodeChef.cs               ✅ Source code
│   └── TableChef.cs              ✅ Structured data
│
├── Chonkie.Fetchers/
│   ├── IFetcher.cs               ✅ Interface
│   └── FileFetcher.cs            ✅ File loading
│   ❌ WebFetcher.cs              (MISSING)
│   ❌ S3Fetcher.cs               (MISSING)
│   ❌ AzureBlobFetcher.cs        (MISSING)
│
├── Chonkie.Refineries/
│   ├── IRefinery.cs              ✅ Interface
│   ├── OverlapRefinery.cs        ✅ Remove overlaps
│   └── EmbeddingsRefinery.cs     ✅ Similarity filter
│   ❌ LengthRefinery.cs          (MISSING)
│   ❌ DuplicateRefinery.cs       (MISSING)
│
├── Chonkie.Porters/
│   ├── IPorter.cs                ✅ Interface
│   └── JsonPorter.cs             ✅ JSON export
│   ❌ CsvPorter.cs               (MISSING)
│   ❌ ParquetPorter.cs           (MISSING)
│   ❌ DatasetsPorter.cs          (MISSING)
│
├── Chonkie.Pipeline/
│   ├── Pipeline.cs               ✅ Orchestrator
│   └── PipelineBuilder.cs        ✅ Fluent API
│
├── Chonkie.Tokenizers/
│   ├── ITokenizer.cs             ✅ Interface
│   ├── CharacterTokenizer.cs     ✅ Char-based
│   ├── WordTokenizer.cs          ✅ Word-based
│   └── ModelTokenizer.cs         🟡 PARTIAL

❌ Chonkie.Genies/ (MISSING ENTIRELY)
   ├── IGenie.cs                  (MISSING)
   ├── OpenAIGenie.cs             (MISSING)
   ├── GeminiGenie.cs             (MISSING)
   ├── AzureOpenAIGenie.cs        (MISSING)
   └── LiteLLMGenie.cs            (MISSING)

❌ Chonkie.Handshakes/ (MISSING ENTIRELY)
   ├── IHandshake.cs              (MISSING)
   ├── QdrantHandshake.cs         (MISSING)
   ├── ChromaHandshake.cs         (MISSING)
   ├── PineconeHandshake.cs       (MISSING)
   ├── WeaviateHandshake.cs       (MISSING)
   ├── ElasticsearchHandshake.cs  (MISSING)
   ├── MilvusHandshake.cs         (MISSING)
   ├── MongoDBHandshake.cs        (MISSING)
   ├── PgvectorHandshake.cs       (MISSING)
   ├── TurbopufferHandshake.cs    (MISSING)
   └── ... (11 total)

tests/
├── Chonkie.Chunkers.Tests/       ✅ Chunker tests
├── Chonkie.Embeddings.Tests/     ✅ Embedding tests
├── Chonkie.Pipeline.Tests/       ✅ Pipeline tests
├── Chonkie.Chefs.Tests/          ✅ Chef tests
└── ...
```

---

## Python Chonkie Structure

```
src/chonkie/
├── chunker/
│   ├── __init__.py               ✅ Module init
│   ├── base.py                   ✅ BaseChunker
│   ├── token.py                  ✅ TokenChunker
│   ├── sentence.py               ✅ SentenceChunker
│   ├── recursive.py              ✅ RecursiveChunker
│   ├── semantic.py               ✅ SemanticChunker
│   ├── code.py                   ✅ CodeChunker
│   ├── late.py                   ✅ LateChunker
│   ├── slumber.py                ✅ SlumberChunker
│   ├── table.py                  ✅ TableChunker
│   └── neural.py                 ✅ NeuralChunker
│
├── embeddings/
│   ├── __init__.py               ✅ Module init
│   ├── base.py                   ✅ BaseEmbeddings
│   ├── auto.py                   ✅ AutoEmbeddings registry
│   ├── registry.py               ✅ Provider registry
│   ├── openai.py                 ✅ OpenAI
│   ├── azure_openai.py           ✅ Azure OpenAI
│   ├── gemini.py                 ✅ Gemini
│   ├── jina.py                   ✅ Jina
│   ├── cohere.py                 ✅ Cohere
│   ├── voyageai.py               ✅ VoyageAI
│   ├── sentence_transformer.py   ✅ Local ONNX
│   ├── litellm.py                ✅ LiteLLM (NEW)
│   ├── model2vec.py              ✅ Model2Vec (NEW)
│   └── catsu.py                  ✅ Catsu (NEW)
│
├── genie/
│   ├── __init__.py               ✅ Module init
│   ├── base.py                   ✅ BaseGenie
│   ├── openai.py                 ✅ OpenAIGenie
│   ├── gemini.py                 ✅ GeminiGenie
│   ├── azure_openai.py           ✅ AzureOpenAIGenie
│   └── litellm.py                ✅ LiteLLMGenie (NEW)
│
├── chef/
│   ├── __init__.py               ✅ Module init
│   ├── base.py                   ✅ BaseChef
│   ├── text.py                   ✅ TextChef
│   ├── markdown.py               ✅ MarkdownChef
│   ├── code.py                   ✅ CodeChef
│   └── table.py                  ✅ TableChef
│
├── fetcher/
│   ├── __init__.py               ✅ Module init
│   ├── base.py                   ✅ BaseFetcher
│   └── file.py                   ✅ FileFetcher
│
├── refinery/
│   ├── __init__.py               ✅ Module init
│   ├── base.py                   ✅ BaseRefinery
│   ├── overlap.py                ✅ OverlapRefinery
│   └── embedding.py              ✅ EmbeddingRefinery
│
├── porters/
│   ├── __init__.py               ✅ Module init
│   ├── base.py                   ✅ BasePorter
│   ├── json.py                   ✅ JsonPorter
│   └── datasets.py               ✅ DatasetsPorter
│
├── handshakes/
│   ├── __init__.py               ✅ Module init
│   ├── base.py                   ✅ BaseHandshake
│   ├── chroma.py                 ✅ Chroma
│   ├── qdrant.py                 ✅ Qdrant
│   ├── pinecone.py               ✅ Pinecone
│   ├── weaviate.py               ✅ Weaviate
│   ├── elastic.py                ✅ Elasticsearch
│   ├── milvus.py                 ✅ Milvus
│   ├── mongodb.py                ✅ MongoDB
│   ├── pgvector.py               ✅ Pgvector
│   ├── turbopuffer.py            ✅ Turbopuffer
│   └── utils.py                  ✅ Utilities
│
├── types/
│   ├── __init__.py               ✅ Module init
│   ├── chunk.py                  ✅ Chunk type
│   ├── document.py               ✅ Document type
│   └── ...
│
├── pipeline.py                   ✅ Pipeline orchestrator
├── tokenizer.py                  ✅ Tokenizer
├── logger.py                     ✅ Logging
│
├── utils/
│   └── ... (Various utilities)
│
├── cloud/
│   ├── chunker/
│   │   ├── base.py               ✅ Cloud base
│   │   ├── token.py              ✅ Cloud token chunker
│   │   └── ... (Cloud wrappers)
│   ├── embeddings/
│   └── ...
│
├── experimental/
│   └── ... (Experimental features)
│
└── visualizer/
    └── ... (Chunk visualization)
```

---

## Key Differences in Organization

### Python Approach
- Flat namespace within each module (chunker/, embeddings/, etc.)
- Uses Python's `__init__.py` for exports
- Mix of module structure for core vs cloud
- Experimental features in separate namespace
- No project-level separation

### C# / .NET Approach
- Separate projects for logical boundaries
- Namespace structure mirrors folder hierarchy
- No Cloud APIs yet
- No experimental namespace yet
- Dependency injection integration
- Strong typing enforced

---

## What This Means

### Files Chonkie.Net has that Python doesn't need:
- `.csproj` project files (x9)
- Interface definitions (IChunker, IEmbeddings, etc.)
- Extension classes (for C# 14 extension members)
- Dependency injection setup

### Files/Folders Python has that Chonkie.Net lacks:
1. **genie/** - All 4 LLM provider implementations
2. **handshakes/** - All 11 vector database connectors
3. **cloud/** - REST API wrappers (3+ modules)
4. **experimental/** - Advanced/beta features
5. **visualizer/** - Visualization utilities
6. Additional **embeddings/** providers (3 missing)
7. Additional **porters/** (2 missing)
8. Additional **fetchers/** (5 missing)

---

## Implementation Roadmap by File Creation

To achieve parity, Chonkie.Net needs to create:

### Phase 1: Critical (Week 1-2)
```
NEW PROJECT: src/Chonkie.Genies/
├── IGenie.cs
├── OpenAIGenie.cs
├── GeminiGenie.cs
├── AzureOpenAIGenie.cs
└── LiteLLMGenie.cs

NEW PROJECT: src/Chonkie.Handshakes/
├── IHandshake.cs
├── QdrantHandshake.cs
└── ChromaHandshake.cs
```

### Phase 2: High Priority (Week 3-4)
```
IN: src/Chonkie.Embeddings/
├── LiteLLM/LiteLLMEmbeddings.cs
├── Catsu/CatsuEmbeddings.cs
└── Model2Vec/Model2VecEmbeddings.cs

IN: src/Chonkie.Handshakes/
├── PineconeHandshake.cs
├── WeaviateHandshake.cs
└── ... (7 more)

IN: src/Chonkie.Fetchers/
└── WebFetcher.cs

IN: src/Chonkie.Porters/
├── CsvPorter.cs
└── ParquetPorter.cs
```

### Phase 3: Nice-to-Have (Week 5+)
```
NEW PROJECT: src/Chonkie.Visualizer/
├── ChunkVisualizer.cs
├── HtmlRenderer.cs
└── TerminalRenderer.cs

NEW PROJECT: src/Chonkie.Cloud/
├── ChunkerAPI.cs
├── EmbeddingsAPI.cs
├── GenieAPI.cs
└── HandshakeAPI.cs
```

---

## Summary

| Aspect | Python | Chonkie.Net | Status |
|--------|--------|------------|--------|
| **Projects** | ~2 (src/, tests/) | 9 | .NET modular |
| **Chunkers** | 10 files | 10 files | ✅ SAME |
| **Embeddings** | 13 files | 7 files | 🟡 PARTIAL |
| **Genies** | 4 files | 0 files | ❌ MISSING |
| **Handshakes** | 11 files | 0 files | ❌ MISSING |
| **Chefs** | 4 files | 4 files | ✅ SAME |
| **Fetchers** | 1 file | 1 file | ✅ SAME |
| **Refineries** | 3 files | 2 files | 🟡 PARTIAL |
| **Porters** | 3 files | 1 file | 🟡 PARTIAL |
| **Total Core** | ~48 files | ~28 files | **58% complete** |

---

## File Count Impact

**Current Chonkie.Net:** ~28 implementation files  
**Full Python Parity:** ~70+ files needed

**Files to Create:**
- 30+ new files for Genies, Handshakes, additional providers
- 15+ new test files
- 5+ new utility/integration files

**Estimated Effort:** 1 file/day = ~7-8 weeks total

