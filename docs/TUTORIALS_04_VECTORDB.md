# 🗄️ Tutorial: Vector Database Integration

**Time to complete:** 20-30 minutes  
**Level:** Intermediate  
**What you'll learn:** How to store and retrieve embeddings with different
vector databases

---

## 📚 What is a Vector Database?

A **vector database** is a specialized database that stores and searches
**embeddings** (vectors of numbers):

```text
Regular Database:          Vector Database:
┌──────────────┐          ┌──────────────────┐
│ ID | Name    │          │ ID | Text | Vec  │
├────┼─────────┤          ├────┼──────┼──────┤
│ 1  │ Alice   │          │ 1  │ Cats │[0.1, │
│ 2  │ Bob     │          │    │      │0.2]  │
└──────────────┘          │ 2  │ Dogs │[0.3, │
                          │    │      │0.4]  │
Search: WHERE id = 1      └──────────────────┘
                          Search: NEAREST([0.15, 0.25])
                          Result: ID 1 (Cats)
```

**Key Operations:**

1. **Write** - Store embeddings + metadata
2. **Search** - Find similar embeddings (vectors close in space)
3. **Delete** - Remove outdated documents

---

## 🏢 Vector Databases in Chonkie.Net

Chonkie.Net implements **9 vector database integrations** (Handshakes):

| Database | Best For | Deployment | Cost |
| -------- | -------- | ---------- | ---- |
| **Qdrant** | Modern, local-first | Local, Cloud | Free (local) |
| **Pinecone** | Fully managed, serverless | SaaS | $$$ |
| **Weaviate** | Open-source, flexible | Self-hosted, Cloud | Free |
| **Chrome** | Simple, lightweight | Local with server | Free |
| **MongoDB** | MongoDB ecosystem | Atlas, Self-hosted | $$ |
| **PostgreSQL (pgvector)** | SQL + vectors | Self-hosted | $ |
| **Elasticsearch** | Search-optimized | Self-hosted, Cloud | $$ |
| **Milvus** | High performance | Self-hosted | Free |
| **Turbopuffer** | Real-time, edge | Cloud | $$ |

---

## 🚀 Pattern: Universal Handshake Interface

All vector database integrations use the same interface:

```csharp
public interface IHandshake
{
    // Store embeddings
    Task WriteAsync(
        List<EmbeddingRecord> records,
        string collectionName
    );

    // Search for similar vectors
    Task<List<EmbeddingRecord>> SearchAsync(
        float[] embedding,
        string collectionName,
        int topK = 10
    );

    // Delete collection for cleanup
    Task DeleteCollectionAsync(string collectionName);
}
```

**This means:** Switch databases by changing **ONE line of code!**

```csharp
// Using Qdrant
IHandshake db = new QdrantHandshake("localhost:6333");

// Switch to Pinecone - just change this line!
IHandshake db = new PineconeHandshake(apiKey);
```

---

## 1️⃣ Qdrant - Modern Vector Database (⭐ Recommended for Local)

**Best for:** Development, self-hosted, modern architecture

### Setup

#### Option A: Docker (Fastest)

```bash
docker run -p 6333:6333 qdrant/qdrant
```

#### Option B: Local Binary

```bash
# Download from https://qdrant.tech/documentation/quick-start/
./qdrant --storage-path ./qdrant-storage
```

### Basic Usage

```csharp
using Chonkie.Handshakes.Qdrant;
using Chonkie.Core.Types;

var handshake = new QdrantHandshake(
    host: "localhost",
    port: 6333,
    apiKey: "default"  // Optional, usually not needed for local
);

// Store embeddings
var records = new List<EmbeddingRecord>
{
    new EmbeddingRecord
    {
        Id = "chunk-1",
        Text = "Machine learning is transformative.",
        Embedding = new float[] { 0.1f, 0.2f, .../* 1536 dims */ },
        Metadata = new Dictionary<string, string>
        {
            ["source"] = "document.txt",
            ["page"] = "1"
        }
    },
    // More records...
};

await handshake.WriteAsync(records, collectionName: "documents");

// Search for similar content
float[] queryEmbedding = new float[] { 0.15f, 0.25f, .../* same dims */ };

var results = await handshake.SearchAsync(
    embedding: queryEmbedding,
    collectionName: "documents",
    topK: 5
);

foreach (var result in results)
{
    Console.WriteLine($"Score: {result.Score:F3}");
    Console.WriteLine($"Text: {result.Text}");
    Console.WriteLine($"Source: {result.Metadata?["source"]}");
}

// Clean up
await handshake.DeleteCollectionAsync("documents");
```

### Docker Compose Example

```yaml
version: '3.8'
services:
  qdrant:
    image: qdrant/qdrant
    ports:
      - "6333:6333"
    volumes:
      - ./qdrant_storage:/qdrant/storage
    environment:
      - QDRANT_API_KEY=your-api-key  # Optional
```

---

## 2️⃣ Pinecone - Fully Managed (⭐ Recommended for Production)

**Best for:** Production, fully managed, no ops burden

### Pinecone Setup

```csharp
// Get API key from https://app.pinecone.io/
var apiKey = Environment.GetEnvironmentVariable("PINECONE_API_KEY")!;

var handshake = new PineconeHandshake(
    apiKey: apiKey,
    indexName: "documents",
    namespace: "default"
);
```

### Example: Full Workflow

```csharp
using Chonkie.Handshakes.Pinecone;

var pinecone = new PineconeHandshake(apiKey: "your-key");

// Store
var records = new List<EmbeddingRecord>
{
    new EmbeddingRecord
    {
        Id = "doc-1",
        Text = "Pinecone is serverless.",
        Embedding = embeddings,
        Metadata = new Dictionary<string, string>
        {
            ["type"] = "article"
        }
    }
};

await pinecone.WriteAsync(records, "documents");

// Search
var results = await pinecone.SearchAsync(embedding, "documents", topK: 10);

Console.WriteLine($"Found {results.Count} similar documents");
```

### Cost Estimation

```text
Pinecone Serverless:
- Write: $0.25 per 1M vectors stored/month
- Query: $0.24 per 1M queries
- 100,000 vectors: ~$2.50/month
- 1M vectors: ~$25/month
```

---

## 3️⃣ Weaviate - Open Source Vector Database

**Best for:** Open-source projects, self-hosted, machine learning workflows

### Docker Setup

```bash
docker run -p 8080:8080 -p 50051:50051 semitechnologies/weaviate:latest
```

### Usage

```csharp
using Chonkie.Handshakes.Weaviate;

var handshake = new WeaviateHandshake(
    host: "http://localhost:8080",
    apiKey: null  // Optional
);

// Store embeddings
await handshake.WriteAsync(records, "documents");

// Search
var results = await handshake.SearchAsync(embedding, "documents", topK: 5);

// Delete when done
await handshake.DeleteCollectionAsync("documents");
```

### Key Features

- Open-source (AGPLv3)
- No cost for self-hosting
- Good for learning and development

---

## 4️⃣ Chroma - Simple & Lightweight

**Best for:** Prototyping, lightweight deployments, simplicity

### Chroma Setup

```bash
# Option 1: Docker
docker run -p 8000:8000 chromadb/chroma:latest

# Option 2: In-process (no server)
var handshake = new ChromaHandshake(url: "http://localhost:8000");
```

### Chroma Usage

```csharp
using Chonkie.Handshakes.Chroma;

var handshake = new ChromaHandshake(url: "http://localhost:8000");

// Store
await handshake.WriteAsync(records, "documents");

// Search with metadata filtering
var results = await handshake.SearchAsync(
    embedding: embedding,
    collectionName: "documents",
    topK: 5
);
```

---

## 5️⃣ PostgreSQL + pgvector - SQL + Vectors

**Best for:** Existing PostgreSQL databases, hybrid SQL+vector queries

### PostgreSQL Setup

```sql
-- Enable pgvector extension
CREATE EXTENSION vector;

-- Create table
CREATE TABLE embeddings (
    id SERIAL PRIMARY KEY,
    text TEXT NOT NULL,
    embedding vector(1536),
    metadata JSONB,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Create index for fast search
CREATE INDEX ON embeddings USING ivfflat (embedding vector_cosine_ops);
```

### PostgreSQL Usage

```csharp
using Chonkie.Handshakes.Pgvector;

var handshake = new PgvectorHandshake(
    connectionString: "Host=localhost;Database=vectors;User Id=postgres;Password=password"
);

// Store
await handshake.WriteAsync(records, "embeddings_table");

// Search
var results = await handshake.SearchAsync(embedding, "embeddings_table", topK: 10);

// SQL query alongside vector search
// Powerful combination!
```

### Hybrid Query Example

```sql
-- Find documents about "AI" that are recent
SELECT id, text, (embedding <-> $1) as distance
FROM embeddings
WHERE metadata->>'topic' = 'AI'
  AND created_at > NOW() - INTERVAL '1 week'
ORDER BY distance
LIMIT 10;
```

---

## 6️⃣ MongoDB + Vector Search

**Best for:** MongoDB ecosystem, document storage + vectors

### MongoDB Setup

```csharp
using Chonkie.Handshakes.MongoDB;

var handshake = new MongoDBHandshake(
    connectionString: "mongodb+srv://user:pass@cluster.mongodb.net/database",
    databaseName: "documents"
);
```

### MongoDB Usage

```csharp
// Store
await handshake.WriteAsync(records, "chunks_collection");

// Search vectors
var results = await handshake.SearchAsync(
    embedding: embedding,
    collectionName: "chunks_collection",
    topK: 5
);
```

### Benefits

- Combines document storage + vector search
- BSON document flexibility
- Good for content + metadata

---

## 7️⃣ Elasticsearch - Search-Optimized

**Best for:** Full-text search + vector search combined

### Elasticsearch Setup

```bash
docker run -p 9200:9200 -e "discovery.type=single-node" docker.elastic.co/elasticsearch/elasticsearch:latest
```

### Elasticsearch Usage

```csharp
using Chonkie.Handshakes.Elasticsearch;

var handshake = new ElasticsearchHandshake(
    nodes: new[] { "http://localhost:9200" }
);

// Store
await handshake.WriteAsync(records, "documents_index");

// Search
var results = await handshake.SearchAsync(embedding, "documents_index", topK: 10);
```

### Elasticsearch Hybrid Search

```csharp
// Elasticsearch excels at combining:
// 1. Full-text search (keywords)
// 2. Vector similarity (meaning)
// 3. Filtering (metadata)

var elasticsearchQuery = new
{
    bool = new
    {
        must = new[] {
            new { match = new { text = "machine learning" } }  // Text search
        },
        filter = new { term = new { status = "published" } },  // Metadata filter
        should = new[] {
            new { knn = new { embedding = embedding } }  // Vector search
        }
    }
};
```

---

## 8️⃣ Milvus - High Performance

**Best for:** High-scale deployments, performance-critical applications

### Milvus Docker Setup

```bash
docker run -p 19530:19530 -p 9091:9091 milvusdb/milvus:latest
```

### Milvus Usage

```csharp
using Chonkie.Handshakes.Milvus;

var handshake = new MilvusHandshake(
    host: "localhost",
    port: 19530
);

// Store
await handshake.WriteAsync(records, "documents");

// Search
var results = await handshake.SearchAsync(embedding, "documents", topK: 5);
```

---

## 9️⃣ Turbopuffer - Edge & Real-time

**Best for:** Edge computing, real-time latency, distributed deployments

### Turbopuffer Setup

```csharp
using Chonkie.Handshakes.Turbopuffer;

var handshake = new TurbopufferHandshake(
    apiKey: Environment.GetEnvironmentVariable("TURBOPUFFER_API_KEY")!,
    namespace: "documents"
);

// Store
await handshake.WriteAsync(records, "collection");

// Search
var results = await handshake.SearchAsync(embedding, "collection", topK: 5);
```

---

## 🔄 Complete Workflow: Local Development to Production

### Phase 1: Development (Local)

```csharp
// Start with free, local qdrant
var db = new QdrantHandshake("localhost:6333");

// Your code
await ProcessDocuments(db);  // Same code...
```

### Phase 2: Scale Up (Still Local)

```csharp
// Switch to self-hosted Weaviate
var db = new WeaviateHandshake("http://localhost:8080");

await ProcessDocuments(db);  // ...exact same interface!
```

### Phase 3: Production (Cloud)

```csharp
// Deploy to Pinecone
var db = new PineconeHandshake(apiKey);

await ProcessDocuments(db);  // Still works without code changes
```

**Same application code. Different deployment strategies.**

---

## 🔍 Advanced Search Patterns

### Pattern 1: Similarity Search

```csharp
// Find top-K most similar embeddings
var results = await db.SearchAsync(
    embedding: queryEmbedding,
    collectionName: "documents",
    topK: 5
);
```

### Pattern 2: Threshold-Based Search

```csharp
// Only return results above confidence threshold
var results = await db.SearchAsync(embedding, collection, topK: 100);

var confident = results
    .Where(r => r.Score > 0.7)  // Adjust threshold
    .Take(5)
    .ToList();
```

### Pattern 3: Metadata Filtering + Vector Search

```csharp
// Get similar documents from specific source
var results = await db.SearchAsync(embedding, collection, topK: 10);

var filtered = results
    .Where(r => r.Metadata?["source"] == "trusted-docs")
    .ToList();
```

### Pattern 4: Re-ranking

```csharp
// Get broad results, then re-rank
var initial = await db.SearchAsync(embedding, collection, topK: 20);

var reranked = initial
    .OrderByDescending(r => CalculateRelevance(query, r.Text))
    .Take(5)
    .ToList();
```

---

## 📊 Comparison: Choosing a Vector Database

```text
Architecture:
- Local development?           → Qdrant (Docker)
- Self-hosted scale?           → Weaviate, Milvus
- Fully managed?               → Pinecone
- Existing PostgreSQL?         → pgvector
- Existing MongoDB?            → MongoDB Vector Search
- Full-text + vectors?         → Elasticsearch

Budget:
- Free (Open Source)?          → Weaviate, Milvus, Qdrant
- Low cost (<$100/month)?      → pgvector (your server)
- Pay-as-you-go?               → Pinecone
- Fixed monthly?               → Weaviate Cloud, Qdrant Cloud

Latency Requirements:
- <50ms?                       → Pinecone, Turbopuffer
- <200ms?                      → Qdrant, Milvus
- Flexible?                    → All others

Scale Requirements:
- Billions of vectors?         → Milvus, Elasticsearch
- Millions of vectors?         → All options
- Thousands of vectors?        → Chroma, simple Qdrant
```

---

## 🚨 Common Issues & Solutions

### Issue 1: "Connection Refused"

```csharp
// ❌ Database not running
var db = new QdrantHandshake("localhost:6333");

// ✅ Start the database first
// docker run -p 6333:6333 qdrant/qdrant

// Then create client
var db = new QdrantHandshake("localhost:6333");
```

---

### Issue 2: "Collection Not Found"

```csharp
// ❌ Typo in collection name
await db.SearchAsync(embedding, "documents");  // Wrong spelling

// ✅ Use consistent names
const string COLLECTION = "documents";
await db.WriteAsync(records, COLLECTION);
await db.SearchAsync(embedding, COLLECTION);
```

---

### Issue 3: "Dimension Mismatch"

```csharp
// ❌ Embeddings of different sizes
var openAI = new OpenAIEmbeddings(); // 1536 dimensions
var sentence = new SentenceTransformerEmbeddings(); // 384 dimensions

// ✅ Use consistent embedding model
var embeddings = new OpenAIEmbeddings();
var emb1 = await embeddings.EmbedAsync(["text1"]);
var emb2 = await embeddings.EmbedAsync(["text2"]);
// Both are 1536 dimensions
```

---

## 🚀 Next Steps

1. **[Quick-Start Guide](TUTORIALS_01_QUICK_START.md)** - Basics
2. **[RAG Tutorial](TUTORIALS_02_RAG.md)** - Full RAG with vector DB
3. **[Chunkers](TUTORIALS_03_CHUNKERS.md)** - Before storing vectors
4. **[Pipelines](TUTORIALS_05_PIPELINES.md)** - Complex workflows

---

## 📚 Additional Resources

- **Qdrant Documentation:** <https://qdrant.tech/documentation/>
- **Pinecone Documentation:** <https://docs.pinecone.io/>
- **Weaviate Documentation:** <https://weaviate.io/developers/weaviate>
- **PostgreSQL pgvector:** <https://github.com/pgvector/pgvector>
