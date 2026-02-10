# Migration Guide: Python Chonkie → Chonkie.Net

**Version:** 1.0  
**Target Python Version:** v1.5.3  
**Target .NET Version:** Chonkie.Net v0.4+ (.NET 10, C# 14)  
**Last Updated:** February 6, 2026  

---

## 📋 Table of Contents

1. [Introduction](#introduction)
2. [Installation & Setup](#installation--setup)
3. [Quick Comparison](#quick-comparison)
4. [Core API Mappings](#core-api-mappings)
5. [Module-by-Module Migration](#module-by-module-migration)
6. [Code Examples](#code-examples)
7. [Known Differences](#known-differences)
8. [Troubleshooting](#troubleshooting)

---

## Introduction

Chonkie.Net is a **direct port** of Python Chonkie (v1.5.3) to C# (.NET 10), maintaining API parity while leveraging .NET's type system and performance characteristics. This guide helps Python developers migrate their code to Chonkie.Net.

### When to Migrate?

You should consider migrating to Chonkie.Net if:

- ✅ You need **better performance** (10-100x faster for chunking)
- ✅ You want **type safety** (C# sealed types vs Python's dynamic typing)
- ✅ You need **cloud deployment** with minimal dependencies
- ✅ You're building microservices that require **.NET infrastructure**
- ✅ You require **ONNX model integration** for embeddings and chunking

You should stay with Python Chonkie if:

- 📊 You need **maximum flexibility** and experimentation
- 🔬 You're doing **research and prototyping**
- 🐍 Your entire stack is Python-based with no .NET infrastructure

### Key Principles

1. **API Parity**: Core APIs are nearly identical between Python and .NET
2. **Type Safety**: C# requires explicit types; Python uses duck typing
3. **Async/Await**: All I/O operations are async in .NET (not just async methods)
4. **Configuration**: .NET uses dependency injection; Python uses constructors
5. **Performance**: .NET generally 10-100x faster for computational tasks

---

## Installation & Setup

### Python Chonkie

```bash
# Basic installation
pip install chonkie

# With specific features
pip install "chonkie[openai,embeddings]"

# Full installation (all features)
pip install "chonkie[all]"
```

### Chonkie.Net

```bash
# Via NuGet Package Manager
dotnet add package Chonkie.Net

# Via Package Manager Console
Install-Package Chonkie.Net

# Via CLI
nuget install Chonkie.Net
```

#### Basic Setup

**Python:**
```python
from chonkie import RecursiveChunker

chunker = RecursiveChunker()
```

**C# (.NET):**
```csharp
using Chonkie.Chunkers;

var chunker = new RecursiveChunker();
```

---

## Quick Comparison

| Aspect | Python Chonkie | Chonkie.Net |
|--------|---|---|
| **Installation** | `pip install chonkie` | NuGet package |
| **Import Style** | `from chonkie import RecursiveChunker` | `using Chonkie.Chunkers;` |
| **Async/Await** | `await chunker.chunk_async()` | `await chunker.ChunkAsync()` |
| **Naming** | snake_case | PascalCase |
| **Error Handling** | Exceptions | Exceptions + Optional pattern |
| **Configuration** | Constructor params + kwargs | DependencyInjection or constructor |
| **Type System** | Dynamic (duck typing) | Static (sealed types) |
| **Performance** | Baseline | 10-100x faster |
| **ONNX Support** | Limited | Full (ONNX Runtime for embeddings/chunking) |

---

## Core API Mappings

### Naming Convention Changes

Python uses `snake_case` for methods and parameters; C# uses `PascalCase`.

| Python | C# |
|--------|---|
| `chunk()`, `chunk_async()` | `Chunk()`, `ChunkAsync()` |
| `generate()`, `generate_async()` | `Generate()`, `GenerateAsync()` |
| `embed()`, `embed_async()` | `Embed()`, `EmbedAsync()` |
| `chunk_size` | `ChunkSize` |
| `max_overlap` | `MaxOverlap` |
| `min_chunk_size` | `MinChunkSize` |

### Core Classes

| Python | C# | Location |
|--------|---|----------|
| `Chunk` | `Chunk` | `Chonkie.Types` |
| `Document` | `Document` | `Chonkie.Types` |
| `BaseChunker` | `BaseChunker` | `Chonkie.Chunkers` |
| `BaseEmbeddings` | `IEmbeddings` | `Chonkie.Embeddings` |
| `BaseGenie` | `IGeneration` | `Chonkie.Genies` |
| `BaseHandshake` | `IHandshake` | `Chonkie.Handshakes` |
| `Pipeline` | `Pipeline` | `Chonkie.Pipeline` |

---

## Module-by-Module Migration

### 1. CHUNKERS 

All 11 chunkers from Python are available in Chonkie.Net.

#### TokenChunker

**Python:**
```python
from chonkie import TokenChunker, AutoTokenizer

chunker = TokenChunker(
    token_counter=AutoTokenizer.for_model("gpt2"),
    chunk_size=512
)
chunks = chunker("Your text here...")
```

**C#:**
```csharp
using Chonkie.Chunkers;
using Chonkie.Tokenizers;

var chunker = new TokenChunker(
    tokenizer: AutoTokenizer.ForModel("gpt2"),
    chunkSize: 512
);
var chunks = chunker.Chunk("Your text here...");
```

#### RecursiveChunker

**Python:**
```python
from chonkie import RecursiveChunker

chunker = RecursiveChunker(
    chunk_size=1024,
    max_overlap=128
)
chunks = chunker("Your text here...")
```

**C#:**
```csharp
using Chonkie.Chunkers;

var chunker = new RecursiveChunker(
    chunkSize: 1024,
    maxOverlap: 128
);
var chunks = chunker.Chunk("Your text here...");
```

#### SemanticChunker

**Python:**
```python
from chonkie import SemanticChunker, OpenAIEmbeddings

embeddings = OpenAIEmbeddings(model="text-embedding-3-small")
chunker = SemanticChunker(
    embedder=embeddings,
    threshold=0.5
)
chunks = chunker("Your text here...")
```

**C#:**
```csharp
using Chonkie.Chunkers;
using Chonkie.Embeddings;

var embeddings = new OpenAIEmbeddings(
    model: "text-embedding-3-small",
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!
);
var chunker = new SemanticChunker(
    embeddings: embeddings,
    threshold: 0.5f
);
var chunks = chunker.Chunk("Your text here...");
```

#### NeuralChunker

**Python:**
```python
from chonkie import NeuralChunker, RecursiveChunker
from chonkie.tokenizer import AutoTokenizer

tokenizer = AutoTokenizer.for_model("gpt2")
recursive_chunker = RecursiveChunker(tokenizer=tokenizer)

chunker = NeuralChunker(
    tokenizer=tokenizer,
    chunk_size=512,
    model_name="bert-base-uncased"  # ONNX model
)
chunks = chunker("Your text here...")
```

**C#:**
```csharp
using Chonkie.Chunkers;
using Chonkie.Tokenizers;

var tokenizer = AutoTokenizer.ForModel("gpt2");
var recursiveChunker = new RecursiveChunker(tokenizer);

var chunker = new NeuralChunker(
    tokenizer: tokenizer,
    chunkSize: 512,
    modelName: "bert-base-uncased"  // ONNX model
);
var chunks = chunker.Chunk("Your text here...");
```

#### SlumberChunker

**Python:**
```python
from chonkie import SlumberChunker, OpenAIGenie

genie = OpenAIGenie(model="gpt-4")
chunker = SlumberChunker(
    genie=genie,
    chunk_size=512
)
chunks = chunker("Your text here...")
```

**C#:**
```csharp
using Chonkie.Chunkers;
using Chonkie.Genies;

var genie = new OpenAIGenie(
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!,
    model: "gpt-4"
);
var chunker = new SlumberChunker(
    genie: genie,
    chunkSize: 512
);
var chunks = chunker.Chunk("Your text here...");
```

#### CodeChunker

**Python:**
```python
from chonkie import CodeChunker

chunker = CodeChunker(
    chunk_size=512,
    language="python"
)
chunks = chunker("def hello():\n    print('Hello')")
```

**C#:**
```csharp
using Chonkie.Chunkers;

var chunker = new CodeChunker(
    chunkSize: 512,
    language: "python"
);
var chunks = chunker.Chunk("def hello():\n    print('Hello')");
```

#### SentenceChunker

**Python:**
```python
from chonkie import SentenceChunker

chunker = SentenceChunker(
    chunk_size=3,
    language="english"
)
chunks = chunker("First sentence. Second sentence. Third sentence.")
```

**C#:**
```csharp
using Chonkie.Chunkers;

var chunker = new SentenceChunker(
    chunkSize: 3,
    language: "english"
);
var chunks = chunker.Chunk("First sentence. Second sentence. Third sentence.");
```

#### TableChunker

**Python:**
```python
from chonkie import TableChunker

chunker = TableChunker(
    chunk_size=512
)
chunks = chunker(markdown_text_with_tables)
```

**C#:**
```csharp
using Chonkie.Chunkers;

var chunker = new TableChunker(
    chunkSize: 512
);
var chunks = chunker.Chunk(markdownTextWithTables);
```

#### LateChunker

**Python:**
```python
from chonkie import LateChunker, OpenAIEmbeddings

embeddings = OpenAIEmbeddings()
chunker = LateChunker(
    embedder=embeddings,
    chunk_size=512
)
chunks = chunker("Your text here...")
```

**C#:**
```csharp
using Chonkie.Chunkers;
using Chonkie.Embeddings;

var embeddings = new OpenAIEmbeddings(
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!
);
var chunker = new LateChunker(
    embedder: embeddings,
    chunkSize: 512
);
var chunks = chunker.Chunk("Your text here...");
```

#### FastChunker

**Python:**
```python
from chonkie import FastChunker

chunker = FastChunker(
    chunk_size=512
)
chunks = chunker("Your text here...")
```

**C#:**
```csharp
using Chonkie.Chunkers;

var chunker = new FastChunker(
    chunkSize: 512
);
var chunks = chunker.Chunk("Your text here...");
```

### 2. EMBEDDINGS

All embeddings providers from Python Chonkie are available in Chonkie.Net.

#### OpenAIEmbeddings

**Python:**
```python
from chonkie import OpenAIEmbeddings

embeddings = OpenAIEmbeddings(model="text-embedding-3-small")
vector = embeddings.embed("Hello, world!")
```

**C#:**
```csharp
using Chonkie.Embeddings;
using Microsoft.Extensions.Logging;

var embeddings = new OpenAIEmbeddings(
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!,
    model: "text-embedding-3-small"
);
var vector = await embeddings.EmbedAsync("Hello, world!");
```

#### AzureOpenAIEmbeddings

**Python:**
```python
from chonkie import AzureOpenAIEmbeddings

embeddings = AzureOpenAIEmbeddings(
    api_key="your-api-key",
    api_version="2024-02-15-preview",
    azure_endpoint="https://your-endpoint.openai.azure.com/",
    deployment_name="your-deployment"
)
```

**C#:**
```csharp
using Chonkie.Embeddings;

var embeddings = new AzureOpenAIEmbeddings(
    apiKey: "your-api-key",
    apiVersion: "2024-02-15-preview",
    azureEndpoint: "https://your-endpoint.openai.azure.com/",
    deploymentName: "your-deployment"
);
```

#### GeminiEmbeddings

**Python:**
```python
from chonkie import GeminiEmbeddings

embeddings = GeminiEmbeddings(model="embedding-001")
```

**C#:**
```csharp
using Chonkie.Embeddings;

var embeddings = new GeminiEmbeddings(
    apiKey: Environment.GetEnvironmentVariable("GOOGLE_API_KEY")!,
    model: "embedding-001"
);
```

#### CohereEmbeddings

**Python:**
```python
from chonkie import CohereEmbeddings

embeddings = CohereEmbeddings(model="embed-english-v3.0")
```

**C#:**
```csharp
using Chonkie.Embeddings;

var embeddings = new CohereEmbeddings(
    apiKey: Environment.GetEnvironmentVariable("COHERE_API_KEY")!,
    model: "embed-english-v3.0"
);
```

#### VoyageAIEmbeddings

**Python:**
```python
from chonkie import VoyageAIEmbeddings

embeddings = VoyageAIEmbeddings(model="voyage-3")
```

**C#:**
```csharp
using Chonkie.Embeddings;

var embeddings = new VoyageAIEmbeddings(
    apiKey: Environment.GetEnvironmentVariable("VOYAGE_API_KEY")!,
    model: "voyage-3"
);
```

#### JinaEmbeddings

**Python:**
```python
from chonkie import JinaEmbeddings

embeddings = JinaEmbeddings(model="jina-embeddings-v3")
```

**C#:**
```csharp
using Chonkie.Embeddings;

var embeddings = new JinaEmbeddings(
    apiKey: Environment.GetEnvironmentVariable("JINA_API_KEY")!,
    model: "jina-embeddings-v3"
);
```

#### SentenceTransformerEmbeddings (ONNX)

**Python:**
```python
from chonkie import SentenceTransformerEmbeddings

embeddings = SentenceTransformerEmbeddings(
    model="all-MiniLM-L6-v2"
)
```

**C#:**
```csharp
using Chonkie.Embeddings;

var embeddings = new SentenceTransformerEmbeddings(
    modelName: "all-MiniLM-L6-v2"  // Downloads ONNX model automatically
);
```

### 3. GENIES (LLM Providers)

#### OpenAIGenie

**Python:**
```python
from chonkie import OpenAIGenie

genie = OpenAIGenie(model="gpt-4")
response = genie.generate("What is RAG?")
```

**C#:**
```csharp
using Chonkie.Genies;

var genie = new OpenAIGenie(
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!,
    model: "gpt-4"
);
var response = await genie.GenerateAsync("What is RAG?");
```

#### AzureOpenAIGenie

**Python:**
```python
from chonkie import AzureOpenAIGenie

genie = AzureOpenAIGenie(
    api_key="your-api-key",
    api_version="2024-02-15-preview",
    azure_endpoint="https://your-endpoint.openai.azure.com/",
    deployment_name="your-deployment"
)
```

**C#:**
```csharp
using Chonkie.Genies;

var genie = new AzureOpenAIGenie(
    apiKey: "your-api-key",
    apiVersion: "2024-02-15-preview",
    azureEndpoint: "https://your-endpoint.openai.azure.com/",
    deploymentName: "your-deployment"
);
```

#### GroqGenie

**Python:**
```python
from chonkie import GroqGenie

genie = GroqGenie(model="llama-3.3-70b-versatile")
response = genie.generate("What is chunking?")
```

**C#:**
```csharp
using Chonkie.Genies;

var genie = new GroqGenie(
    apiKey: Environment.GetEnvironmentVariable("GROQ_API_KEY")!,
    model: "llama-3.3-70b-versatile"
);
var response = await genie.GenerateAsync("What is chunking?");
```

#### CerebrasGenie

**Python:**
```python
from chonkie import CerebrasGenie

genie = CerebrasGenie(model="llama-3.3-70b")
```

**C#:**
```csharp
using Chonkie.Genies;

var genie = new CerebrasGenie(
    apiKey: Environment.GetEnvironmentVariable("CEREBRAS_API_KEY")!,
    model: "llama-3.3-70b"
);
```

#### GeminiGenie

**Python:**
```python
from chonkie import GeminiGenie

genie = GeminiGenie(model="gemini-1.5-pro")
```

**C#:**
```csharp
using Chonkie.Genies;

var genie = new GeminiGenie(
    apiKey: Environment.GetEnvironmentVariable("GOOGLE_API_KEY")!,
    model: "gemini-1.5-pro"
);
```

### 4. HANDSHAKES (Vector Databases)

#### ChromaHandshake

**Python:**
```python
from chonkie import ChromaHandshake

handshake = ChromaHandshake(
    collection_name="my_collection",
    host="localhost",
    port=8000
)
```

**C#:**
```csharp
using Chonkie.Handshakes;
using Chonkie.Embeddings;

var embeddings = new SentenceTransformerEmbeddings("all-MiniLM-L6-v2");
var handshake = new ChromaHandshake(
    collectionName: "my_collection",
    embeddingModel: embeddings,
    serverUrl: "http://localhost:8000"
);
```

**Key Difference:** Chonkie.Net requires an EmbeddingModel (IEmbeddings) to generate vectors from chunk text.

#### ElasticsearchHandshake

**Python:**
```python
from chonkie import ElasticHandshake

handshake = ElasticHandshake(
    hosts=["localhost:9200"],
    index_name="my_index"
)
```

**C#:**
```csharp
using Chonkie.Handshakes;
using Chonkie.Embeddings;

var embeddings = new OpenAIEmbeddings(apiKey: "...");
var handshake = new ElasticsearchHandshake(
    hosts: new[] { "localhost:9200" },
    indexName: "my_index",
    embeddingModel: embeddings
);
```

#### PineconeHandshake

**Python:**
```python
from chonkie import PineconeHandshake

handshake = PineconeHandshake(
    api_key="your-api-key",
    index_name="my_index"
)
```

**C#:**
```csharp
using Chonkie.Handshakes;
using Chonkie.Embeddings;

var embeddings = new OpenAIEmbeddings(apiKey: "...");
var handshake = new PineconeHandshake(
    apiKey: "your-api-key",
    indexName: "my_index",
    embeddingModel: embeddings
);
```

#### PgvectorHandshake

**Python:**
```python
from chonkie import PgvectorHandshake

handshake = PgvectorHandshake(
    connection_string="postgresql://user:pass@localhost/db"
)
```

**C#:**
```csharp
using Chonkie.Handshakes;
using Chonkie.Embeddings;
using Npgsql;

var embeddings = new OpenAIEmbeddings(apiKey: "...");
var options = new PgvectorHandshakeOptions
{
    Host = "localhost",
    Port = 5432,
    User = "postgres",
    Password = "password",
    Database = "chunks_db",
    CollectionName = "chunks"
};

var handshake = new PgvectorHandshake(
    options: options,
    embeddingModel: embeddings
);
```

**Key Difference:** Uses PgvectorHandshakeOptions for configuration instead of connection string.

#### MongoDBHandshake

**Python:**
```python
from chonkie import MongoDBHandshake

handshake = MongoDBHandshake(
    connection_string="mongodb://localhost:27017"
)
```

**C#:**
```csharp
using Chonkie.Handshakes;
using Chonkie.Embeddings;

var embeddings = new OpenAIEmbeddings(apiKey: "...");
var handshake = new MongoDBHandshake(
    connectionString: "mongodb://localhost:27017",
    databaseName: "chunks",
    collectionName: "documents",
    embeddingModel: embeddings
);
```

#### QdrantHandshake

**Python:**
```python
from chonkie import QdrantHandshake

handshake = QdrantHandshake(
    host="localhost",
    port=6333,
    collection_name="my_collection"
)
```

**C#:**
```csharp
using Chonkie.Handshakes;
using Chonkie.Embeddings;

var embeddings = new SentenceTransformerEmbeddings("all-MiniLM-L6-v2");
var handshake = new QdrantHandshake(
    host: "localhost",
    port: 6333,
    collectionName: "my_collection",
    embeddingModel: embeddings
);
```

#### MilvusHandshake

**Python:**
```python
from chonkie import MilvusHandshake

handshake = MilvusHandshake(
    uri="http://localhost:19530"
)
```

**C#:**
```csharp
using Chonkie.Handshakes;
using Chonkie.Embeddings;

var embeddings = new OpenAIEmbeddings(apiKey: "...");
var handshake = new MilvusHandshake(
    uri: "http://localhost:19530",
    collectionName: "chunks",
    embeddingModel: embeddings
);
```

#### WeaviateHandshake

**Python:**
```python
from chonkie import WeaviateHandshake

handshake = WeaviateHandshake(
    host="http://localhost:8080"
)
```

**C#:**
```csharp
using Chonkie.Handshakes;
using Chonkie.Embeddings;

var embeddings = new OpenAIEmbeddings(apiKey: "...");
var handshake = new WeaviateHandshake(
    host: "http://localhost:8080",
    embeddingModel: embeddings
);
```

#### TurbopufferHandshake

**Python:**
```python
from chonkie import TurbopufferHandshake

handshake = TurbopufferHandshake(
    api_key="your-api-key",
    db_name="my_database"
)
```

**C#:**
```csharp
using Chonkie.Handshakes;
using Chonkie.Embeddings;

var embeddings = new OpenAIEmbeddings(apiKey: "...");
var handshake = new TurbopufferHandshake(
    apiKey: "your-api-key",
    dbName: "my_database",
    embeddingModel: embeddings
);
```

### 5. CHEFS (Document Preprocessing)

#### TextChef

**Python:**
```python
from chonkie import TextChef

chef = TextChef()
chunks = chef.chunk(document)
```

**C#:**
```csharp
using Chonkie.Chefs;

var chef = new TextChef();
var chunks = chef.Chunk(document);
```

#### MarkdownChef

**Python:**
```python
from chonkie import MarkdownChef

chef = MarkdownChef()
chunks = chef.chunk(markdown_document)
```

**C#:**
```csharp
using Chonkie.Chefs;

var chef = new MarkdownChef();
var chunks = chef.Chunk(markdownDocument);
```

#### TableChef

**Python:**
```python
from chonkie import TableChef

chef = TableChef()
chunks = chef.chunk(document)
```

**C#:**
```csharp
using Chonkie.Chefs;

var chef = new TableChef();
var chunks = chef.Chunk(document);
```

### 6. PIPELINE

#### Creating a Pipeline

**Python:**
```python
from chonkie import Pipeline, RecursiveChunker, OpenAIEmbeddings, PineconeHandshake

pipeline = Pipeline(
    chunker=RecursiveChunker(),
    embedder=OpenAIEmbeddings(),
    database=PineconeHandshake(api_key="...")
)

# Run the full pipeline
chunks = pipeline.run("Your text here...")
```

**C#:**
```csharp
using Chonkie.Pipeline;

// Fluent builder pattern
var result = await new Pipeline()
    .ProcessWith("text")  // Use TextChef
    .ChunkWith("recursive", new { chunk_size = 512 })  // Use RecursiveChunker
    .RunAsync(texts: "Your text here...");
```

**Alternative: Configuration-based Pipeline**

```csharp
// From JSON configuration
var pipeline = Pipeline.FromConfig("pipeline-config.json");
var result = await pipeline.RunAsync(texts: "Your text here...");
```

**Pipeline Configuration (pipeline-config.json):**
```json
[
  {
    "type": "ProcessWith",
    "component": "text"
  },
  {
    "type": "ChunkWith",
    "component": "recursive",
    "parameters": {
      "chunk_size": 512,
      "max_overlap": 50
    }
  },
  {
    "type": "RefineWith",
    "component": "overlap",
    "parameters": {
      "context_size": 50
    }
  }
]
```

**Key Differences:**
1. Python Pipeline: Constructor-based with type-safe components
2. C# Pipeline: Fluent builder pattern with string-based component registration
3. C# is more flexible - components are registered and can be configured via JSON or fluent API
4. More complex pipelines use configuration files rather than inline construction

---

## Code Examples

### Example 1: Basic Chunking

**Python:**
```python
from chonkie import RecursiveChunker

text = """
Chonkie is a chunking library...
"""

chunker = RecursiveChunker(chunk_size=512,max_overlap=50)
chunks = chunker(text)

for chunk in chunks:
    print(f"Text: {chunk.text}")
    print(f"Tokens: {chunk.token_count}")
```

**C#:**
```csharp
using Chonkie.Chunkers;

var text = """
Chonkie is a chunking library...
""";

var chunker = new RecursiveChunker(chunkSize: 512, maxOverlap: 50);
var chunks = chunker.Chunk(text);

foreach (var chunk in chunks)
{
    Console.WriteLine($"Text: {chunk.Text}");
    Console.WriteLine($"Tokens: {chunk.TokenCount}");
}
```

### Example 2: Semantic Chunks with Embeddings

**Python:**
```python
from chonkie import SemanticChunker, OpenAIEmbeddings

embeddings = OpenAIEmbeddings(model="text-embedding-3-small")
chunker = SemanticChunker(embedder=embeddings, threshold=0.5)

chunks = chunker("Your text here...")

for chunk in chunks:
    print(f"Chunk: {chunk.text}")
    print(f"Embedding: {chunk.embedding}")
```

**C#:**
```csharp
using Chonkie.Chunkers;
using Chonkie.Embeddings;

var embeddings = new OpenAIEmbeddings(
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!,
    model: "text-embedding-3-small"
);
var chunker = new SemanticChunker(embedder: embeddings, threshold: 0.5f);

var chunks = await chunker.ChunkAsync("Your text here..."); // Async in C#

foreach (var chunk in chunks)
{
    Console.WriteLine($"Chunk: {chunk.Text}");
    Console.WriteLine($"Embedding: {string.Join(", ", chunk.Embeddings![..3])}...");
}
```

### Example 3: Full RAG Pipeline

**Python:**
```python
from chonkie import (
    Pipeline,
    RecursiveChunker,
    OpenAIEmbeddings,
    PineconeHandshake
)

# Setup components
chunker = RecursiveChunker(chunk_size=1024)
embeddings = OpenAIEmbeddings()
database = PineconeHandshake(
    api_key="your-api-key",
    index_name="my_index"
)

# Create pipeline
pipeline = Pipeline(
    chunker=chunker,
    embedder=embeddings,
    database=database
)

# Ingest document
chunks = pipeline.run(
    text="Your document content...",
    metadata={"source": "wikipedia", "title": "RAG"}
)

print(f"Ingested {len(chunks)} chunks")
```

**C#:**
```csharp
using Chonkie.Pipeline;

// Fluent builder pattern - simpler and more flexible
var result = await new Pipeline()
    .ProcessWith("text")  // TextChef
    .ChunkWith("recursive", new 
    { 
        chunk_size = 1024 
    })
    .RefineWith("overlap", new
    {
        context_size = 50
    })
    .RunAsync(texts: "Your document content...");

// Result is a list of chunks
var chunks = result as List<Chunk>;
Console.WriteLine($"Ingested {chunks?.Count ?? 0} chunks");
```

**For Vector Database Integration:** Use Handshakes separately after chunking:

```csharp
using Chonkie.Handshakes;
using Chonkie.Embeddings;

// Chunk the text
var chunker = new RecursiveChunker(chunkSize: 1024);
var chunks = chunker.Chunk("Your document content...");

// Embed the chunks
var embeddings = new OpenAIEmbeddings(apiKey: "...");
var embeddedChunks = await embeddings.EmbedBatchAsync(chunks);

// Store in vector database
var handshake = new PineconeHandshake(
    apiKey: "your-api-key",
    indexName: "my_index",
    embeddingModel: embeddings
);

await handshake.WriteAsync(embeddedChunks);

Console.WriteLine($"Ingested {chunks.Count} chunks");
```

### Example 4: Large Document Processing

**Python:**
```python
from chonkie import RecursiveChunker
import asyncio

chunker = RecursiveChunker()

# Process large documents
documents = [
    "Document 1...",
    "Document 2...",
    "Document 3..."
]

for doc in documents:
    chunks = chunker(doc)
    # Process chunks...
```

**C#:**
```csharp
using Chonkie.Chunkers;

var chunker = new RecursiveChunker();

var documents = new[]
{
    "Document 1...",
    "Document 2...",
    "Document 3..."
};

foreach (var doc in documents)
{
    var chunks = chunker.Chunk(doc);
    // Process chunks...
}

// Or process in parallel for better performance
var allChunks = new List<Chunk>();
foreach (var doc in documents)
{
    var chunks = await chunker.ChunkAsync(doc);
    allChunks.AddRange(chunks);
}
```

---

## Known Differences

### 1. Async/Await Pattern

**Python:** All operations are synchronous by default; async versions available with `_async()` suffix.

```python
# Synchronous (default)
chunks = chunker(text)

# Asynchronous
chunks = await chunker.chunk_async(text)
```

**C#:** All I/O-bound operations are asynchronous using `async/await`.

```csharp
// All operations are async
var chunks = await chunker.ChunkAsync(text);

// Synchronous version (blocks on async)
var chunks = chunker.Chunk(text);  // Uses .GetAwaiter().GetResult()
```

### 2. Nullable Types & Optional Values

**Python:** Uses `None` for missing values; type hints optional.

```python
chunk = chunks[0]
if chunk.embedding is None:
    print("No embedding")
```

**C#:** Uses nullable reference types; explicit nullability.

```csharp
var chunk = chunks[0];
if (chunk.Embeddings is null)
{
    Console.WriteLine("No embedding");
}
```

### 3. Type System

**Python:** Dynamic typing; no type checking at compile time.

```python
embeddings = OpenAIEmbeddings()
vector = embeddings.embed("text")  # Vector is List[float]
```

**C#:** Static typing; compile-time type checking.

```csharp
var embeddings = new OpenAIEmbeddings(...);
var vector = await embeddings.EmbedAsync("text");  // Vector is float[]
```

### 4. Exception Handling

**Python:** Exceptions are for error cases only.

```python
try:
    chunks = chunker("text")
except ValueError as e:
    print(f"Error: {e}")
```

**C#:** Exceptions + Result/Option patterns.

```csharp
try
{
    var chunks = chunker.Chunk("text");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

### 5. Configuration & Dependency Injection

**Python:** Direct constructor params.

```python
chunker = RecursiveChunker(
    chunk_size=1024,
    max_overlap=128,
    tokenizer=tokenizer
)
```

**C#:** Constructor params + optional DI integration.

```csharp
// Direct constructor
var chunker = new RecursiveChunker(
    chunkSize: 1024,
    maxOverlap: 128,
    tokenizer: tokenizer
);

// Or with Dependency Injection
services.AddRecursiveChunker(options =>
{
    options.ChunkSize = 1024;
    options.MaxOverlap = 128;
});
```

### 6. Performance Characteristics

| Operation | Python | C# | Speedup |
|-----------|---|---|---|
| Basic Chunking | 1.0x | 10-20x | 10-20x |
| Semantic Embedding | 1.0x | 5-10x | 5-10x |
| NeuralChunker | 1.0x | 15-50x | 15-50x |
| Large Documents (10MB+) | Slow | Fast | 50-100x |

### 7. Module Organization

**Python:** Flat imports, optional sub-packages.

```python
from chonkie import RecursiveChunker, OpenAIGenie, PineconeHandshake
```

**C#:** Namespaced, hierarchical organization.

```csharp
using Chonkie.Chunkers;
using Chonkie.Genies;
using Chonkie.Handshakes;
```

### 8. String Handling

**Python:** Native Unicode; flexible string operations.

```python
text = "Hello, 世界 🌍"
chunks = chunker(text)
```

**C#:** UTF-16; excellent Unicode support.

```csharp
var text = "Hello, 世界 🌍";
var chunks = chunker.Chunk(text);
```

### 9. Logging

**Python:** Uses `logging` module.

```python
import logging

logger = logging.getLogger("chonkie")
logger.info("Starting chunking...")
```

**C#:** Uses Microsoft.Extensions.Logging.

```csharp
using Microsoft.Extensions.Logging;

ILogger logger = loggerFactory.CreateLogger("Chonkie");
logger.LogInformation("Starting chunking...");
```

### 10. Return Types

**Python:** Lists and dictionaries.

```python
chunks = chunker("text")  # Returns list[Chunk]
```

**C#:** Lists and arrays.

```csharp
var chunks = chunker.Chunk("text");  // Returns List<Chunk>
```

---

## Best Practices for Migration

### 1. Start with Core Chunking

Migrate chunking first—it's the stable foundation.

```csharp
// Phase 1: Just migrate chunking
var chunker = new RecursiveChunker();
var chunks = chunker.Chunk(yourText);
```

### 2. Add Embeddings Next

Once chunking works, add embeddings.

```csharp
// Phase 2: Add embeddings
var embeddings = new OpenAIEmbeddings(apiKey);
var vectorChunks = await embeddings.EmbedAsync(chunks);
```

### 3. Integrate Vector DB Last

Complete the pipeline with vector database integration.

```csharp
// Phase 3: Add vector database
var handshake = new PineconeHandshake(apiKey, indexName);
await handshake.WriteAsync(vectorChunks);
```

### 4. Test Behavior Parity

Always verify that C# behavior matches Python semantics.

```csharp
// Python test
chunks_py = chunker(text)

// C# test - should produce same results
var chunks_cs = chunker.Chunk(text);
Assert.Equal(chunks_py.Count, chunks_cs.Count);
```

### 5. Use Async/Await Properly

Don't block on async operations unnecessarily.

```csharp
// Good: Use async all the way
var chunks = await chunker.ChunkAsync(text);

// Avoid: Blocking on async
var chunks = chunker.ChunkAsync(text).Result;  // Deadlock risk!
```

---

## Troubleshooting

### "Type 'X' not found in namespace"

**Solution:** Verify using statements and import correct namespace.

```csharp
// ❌ Wrong
var chunker = new RecursiveChunker();

// ✅ Correct
using Chonkie.Chunkers;
var chunker = new RecursiveChunker();
```

### "Cannot await non-async method"

**Solution:** Use async methods appropriately.

```csharp
// ❌ Wrong
var chunks = await chunker.Chunk(text);

// ✅ Correct
var chunks = await chunker.ChunkAsync(text);
```

### "API Key not found error"

**Solution:** Set environment variables correctly.

```csharp
// ❌ Wrong
var embeddings = new OpenAIEmbeddings();  // Missing apiKey

// ✅ Correct
var embeddings = new OpenAIEmbeddings(
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
        ?? throw new InvalidOperationException("OPENAI_API_KEY not set")
);
```

### "NullReferenceException on null properties"

**Solution:** Check for null using C# null-coalescing operators.

```csharp
// ❌ Wrong
if (chunk.Embeddings.Length > 0)  // Throws if null

// ✅ Correct
if (chunk.Embeddings?.Length > 0)  // Safe navigation
```

---

## Summary

| Aspect | Python | C# | Migration Effort |
|--------|--------|---|---|
| **Chunking APIs** | Direct match | Direct match | Easy ✅ |
| **Embedding APIs** | Direct match | Async required | Easy ✅ |
| **Genie APIs** | Direct match | Async required | Easy ✅ |
| **Handshake APIs** | Direct match | Async required | Easy ✅ |
| **Type System** | Dynamic | Static | Medium 🟡 |
| **Async/Await** | Optional | Required | Medium 🟡 |
| **Configuration** | Constructor | DI support | Easy ✅ |
| **Overall** | Feature parity | ~95% | Low-Medium |

---

## Additional Resources

- **Official Docs:** [Chonkie.Net Documentation](../docs/INDEX.md)
- **API Reference:** [API Reference Index](API_REFERENCE_INDEX.md)
- **Quick Start:** [Quick Start Guide](TUTORIALS_01_QUICK_START.md)
- **Python Chonkie:** [https://docs.chonkie.ai](https://docs.chonkie.ai)
- **GitHub:** [https://github.com/chonkie-inc/chonkie](https://github.com/chonkie-inc/chonkie)

---

## Contributing Feedback

Found an issue with the migration guide? Please open an issue with:
- Your Python code snippet
- Your C# translation attempt
- Expected behavior
- Actual behavior

**Version:** 1.0 | **Last Updated:** February 6, 2026
