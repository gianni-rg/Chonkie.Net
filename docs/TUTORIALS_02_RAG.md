# 🧠 Tutorial: Building a RAG System with Chonkie.Net

**Time to complete:** 20-30 minutes  
**Level:** Intermediate  
**What you'll build:** A complete Retrieval-Augmented Generation system

---

## 📚 What is RAG?

**RAG (Retrieval-Augmented Generation)** combines:
1. **Retrieval** - Find relevant documents/chunks from a knowledge base
2. **Augmentation** - Add those chunks to your LLM prompt
3. **Generation** - Let the LLM generate an answer based on the augmented context

This lets LLMs answer questions about your **private data** without retraining.

### RAG vs. Fine-tuning

| Aspect | RAG | Fine-tuning |
|--------|-----|-----------|
| Update knowledge | ✅ Easy (add docs) | ❌ Expensive (retrain) |
| Private data | ✅ Safe (stays local) | ❌ Less safe |
| Speed | ✅ Fast | ❌ Slow |
| Cost | ✅ Low | ❌ High |
| Accuracy | 🟡 Depends on chunks | 🟡 Depends on data |

---

## 🏗️ RAG Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     RAG Pipeline                            │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Step 1: Document Processing                                │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ Raw Documents → Chunk → Embed → Vector DB Store     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                              │
│  Step 2: Retrieval (At Query Time)                          │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ User Question → Embed → Search Vector DB → Get Top-K│   │
│  └─────────────────────────────────────────────────────┘   │
│                                                              │
│  Step 3: Generation                                          │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ Question + Top-K Chunks → LLM → Answer              │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 🚀 Complete RAG Implementation

Here's a complete, working RAG system:

### Step 1: Document Ingestion **(Chunking)**

```csharp
using Chonkie.Chunkers;
using Chonkie.Pipeline;
using Chonkie.Tokenizers;

public class DocumentProcessor
{
    private readonly ITokenizer _tokenizer;

    public DocumentProcessor()
    {
        _tokenizer = new WordTokenizer();
    }

    public async Task<List<Chunk>> ChunkDocumentAsync(string documentPath)
    {
        // Read document
        string content = File.ReadAllText(documentPath);

        // Process with pipeline
        var result = await FluentPipeline.Create()
            .WithText(content)
            .ChunkWith(new RecursiveChunker(_tokenizer, chunkSize: 512, chunkOverlap: 50))
            .RunAsync();

        return result.Chunks;
    }

    public async Task<List<Chunk>> ChunkAllDocumentsAsync(string folderPath)
    {
        var allChunks = new List<Chunk>();

        foreach (var file in Directory.GetFiles(folderPath, "*.txt"))
        {
            var chunks = await ChunkDocumentAsync(file);
            allChunks.AddRange(chunks);
        }

        return allChunks;
    }
}
```

---

### Step 2: Embedding Generation

```csharp
using Chonkie.Core.Types;
using Chonkie.Embeddings.OpenAI;

public class EmbeddingGenerator
{
    private readonly IEmbeddingsModel _embeddingModel;
    private const int BatchSize = 10; // OpenAI rates limit

    public EmbeddingGenerator(string openAiApiKey)
    {
        _embeddingModel = new OpenAIEmbeddings(
            apiKey: openAiApiKey,
            model: "text-embedding-3-small"
        );
    }

    public async Task<Dictionary<Chunk, float[]>> GenerateEmbeddingsAsync(
        List<Chunk> chunks)
    {
        var result = new Dictionary<Chunk, float[]>();

        // Process in batches to respect rate limits
        for (int i = 0; i < chunks.Count; i += BatchSize)
        {
            var batch = chunks.Skip(i).Take(BatchSize).ToList();
            var texts = batch.Select(c => c.Text).ToList();

            var embeddings = await _embeddingModel.EmbedAsync(texts);

            for (int j = 0; j < batch.Count; j++)
            {
                result[batch[j]] = embeddings[j];
            }
        }

        return result;
    }
}
```

---

### Step 3: Store in Vector Database

```csharp
using Chonkie.Handshakes.Qdrant;  // Or any other vector DB

public class VectorStore
{
    private readonly IHandshake _handshake;

    public VectorStore(IHandshake handshake)
    {
        _handshake = handshake;
    }

    public async Task StoreChunksAsync(
        Dictionary<Chunk, float[]> chunkEmbeddings,
        string collectionName)
    {
        var records = chunkEmbeddings
            .Select(kvp => new EmbeddingRecord
            {
                Id = kvp.Key.GetHashCode().ToString(),
                Text = kvp.Key.Text,
                Embedding = kvp.Value,
                Metadata = new Dictionary<string, string>
                {
                    ["start_index"] = kvp.Key.StartIndex.ToString(),
                    ["end_index"] = kvp.Key.EndIndex.ToString()
                }
            })
            .ToList();

        // Write to vector database
        await _handshake.WriteAsync(records, collectionName);
    }
}
```

---

### Step 4: Retrieval at Query Time

```csharp
using Chonkie.Genies.OpenAI;

public class RAGRetriever
{
    private readonly IHandshake _vectorDb;
    private readonly IEmbeddingsModel _embeddings;
    private readonly int _topK;

    public RAGRetriever(
        IHandshake vectorDb,
        IEmbeddingsModel embeddings,
        int topK = 3)
    {
        _vectorDb = vectorDb;
        _embeddings = embeddings;
        _topK = topK;
    }

    public async Task<List<string>> RetrieveContextAsync(
        string query,
        string collectionName)
    {
        // Embed the query
        var queryEmbedding = await _embeddings.EmbedAsync(new[] { query });
        var embedding = queryEmbedding[0];

        // Search vector database
        var results = await _vectorDb.SearchAsync(
            embedding: embedding,
            collectionName: collectionName,
            topK: _topK
        );

        // Extract text from results
        return results
            .Select(r => r.Text ?? r.Metadata?["text"] ?? "")
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();
    }
}
```

---

### Step 5: LLM Generation

```csharp
using Chonkie.Genies.OpenAI;

public class RAGGenerator
{
    private readonly IGeneration _llm;

    public RAGGenerator(string openAiApiKey)
    {
        _llm = new OpenAIGenie(apiKey: openAiApiKey);
    }

    public async Task<string> GenerateAnswerAsync(
        string question,
        List<string> contextChunks)
    {
        // Build prompt with context
        var contextText = string.Join("\n\n", contextChunks);
        var prompt = $@"
Use the following context to answer the question.
If the context doesn't contain the answer, say ""I don't know"".

Context:
{contextText}

Question: {question}

Answer:";

        // Generate with LLM
        var answer = await _llm.GenerateAsync(prompt);
        return answer;
    }
}
```

---

## 🧩 Putting It All Together

```csharp
public class RAGApplication
{
    public static async Task Main()
    {
        const string OpenAiKey = "sk-...";
        const string DocumentFolder = "./documents";
        const string CollectionName = "my-documents";

        // 1. Process documents into chunks
        Console.WriteLine("📄 Processing documents...");
        var processor = new DocumentProcessor();
        var chunks = await processor.ChunkAllDocumentsAsync(DocumentFolder);
        Console.WriteLine($"✅ Created {chunks.Count} chunks");

        // 2. Generate embeddings
        Console.WriteLine("🔢 Generating embeddings...");
        var embeddingGen = new EmbeddingGenerator(OpenAiKey);
        var chunkEmbeddings = await embeddingGen.GenerateEmbeddingsAsync(chunks);
        Console.WriteLine($"✅ Generated {chunkEmbeddings.Count} embeddings");

        // 3. Store in vector database
        Console.WriteLine("💾 Storing in vector database...");
        var handshake = new QdrantHandshake(
            host: "localhost",
            port: 6333,
            apiKey: "default"
        );
        var vectorStore = new VectorStore(handshake);
        await vectorStore.StoreChunksAsync(chunkEmbeddings, CollectionName);
        Console.WriteLine("✅ Stored in Qdrant");

        // 4. Interactive queries
        Console.WriteLine("\n🚀 RAG System Ready! Ask questions:\n");
        var retriever = new RAGRetriever(handshake, embeddingGen._embeddingModel);
        var generator = new RAGGenerator(OpenAiKey);

        while (true)
        {
            Console.Write("Question: ");
            var question = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(question)) break;

            // Retrieve relevant chunks
            var context = await retriever.RetrieveContextAsync(question, CollectionName);

            // Generate answer
            var answer = await generator.GenerateAnswerAsync(question, context);

            Console.WriteLine($"\nAnswer: {answer}\n");
        }
    }
}
```

**Output:**
```
📄 Processing documents...
✅ Created 245 chunks

🔢 Generating embeddings...
✅ Generated 245 embeddings

💾 Storing in vector database...
✅ Stored in Qdrant

🚀 RAG System Ready! Ask questions:

Question: What is machine learning?
Answer: Machine learning is a subset of artificial intelligence that enables systems to learn and improve from experience without being explicitly programmed. Based on the provided context, it involves algorithms processing vast amounts of data to identify patterns and make predictions...

Question: How does semantic chunking work?
Answer: Semantic chunking groups semantically similar sentences together using embeddings. It respects meaning boundaries rather than just token limits, resulting in more coherent chunks that preserve context better than simple token-based chunking...
```

---

## ⚙️ Configuration Tuning

### Chunking Parameters

```csharp
// For brief documents/FAQs
var chunker = new RecursiveChunker(tokenizer, chunkSize: 256, chunkOverlap: 25);

// For technical documentation  
var chunker = new RecursiveChunker(tokenizer, chunkSize: 512, chunkOverlap: 50);

// For long-form content (books, research papers)
var chunker = new RecursiveChunker(tokenizer, chunkSize: 1024, chunkOverlap: 100);

// For semantic understanding required
var chunker = new SemanticChunker(tokenizer, embeddings, threshold: 0.7f);
```

### Retrieval Parameters

```csharp
// Precise answers (fewer chunks, more focused)
var retriever = new RAGRetriever(vectorDb, embeddings, topK: 3);

// Comprehensive answers (more context)
var retriever = new RAGRetriever(vectorDb, embeddings, topK: 10);

// Balanced (most common)
var retriever = new RAGRetriever(vectorDb, embeddings, topK: 5);
```

---

## 🎯 RAG Patterns & Best Practices

### Pattern 1: Simple RAG (What we built above)
✅ Good for: Small to medium documents  
❌ Issues: Noise from irrelevant chunks

```csharp
// Just retrieve and generate
var context = await retriever.RetrieveContextAsync(question, collection);
var answer = await generator.GenerateAnswerAsync(question, context);
```

---

### Pattern 2: Re-ranking RAG
✅ Better relevance filtering before LLM

```csharp
var initialResults = await retriever.RetrieveContextAsync(question, collection);

// Re-rank by relevance (optional step)
var reranked = initialResults
    .OrderByDescending(r => CalculateRelevance(question, r))
    .Take(3)
    .ToList();

var answer = await generator.GenerateAnswerAsync(question, reranked);
```

---

### Pattern 3: Hybrid RAG
✅ Combines keyword search + semantic search

```csharp
// Semantic search
var semanticResults = await retriever.RetrieveContextAsync(question, collection);

// Plus keyword search (fallback)
var keywordResults = await keywordSearch.SearchAsync(question);

// Combine (with ranking)
var combined = semanticResults.Concat(keywordResults)
    .Distinct()
    .Take(5)
    .ToList();

var answer = await generator.GenerateAnswerAsync(question, combined);
```

---

## ⚠️ Common RAG Pitfalls & Solutions

### Problem 1: Chunks Too Small (Loss of Context)
```csharp
// ❌ Too small - loses context
var chunker = new TokenChunker(tokenizer, chunkSize: 50);

// ✅ Better - maintains context
var chunker = new TokenChunker(tokenizer, chunkSize: 512, chunkOverlap: 50);
```

---

### Problem 2: Chunks Too Large (Noise)
```csharp
// ❌ Too large - mixes unrelated topics
var chunker = new TokenChunker(tokenizer, chunkSize: 4096);

// ✅ Better - focused chunks
var chunker = new TokenChunker(tokenizer, chunkSize: 512);
```

---

### Problem 3: Irrelevant Retrieval
```csharp
// ❌ Just top-1, may miss context
var results = await vectorDb.SearchAsync(embedding, collection, topK: 1);

// ✅ Top-3 with re-ranking
var results = await vectorDb.SearchAsync(embedding, collection, topK: 5);
var reranked = results.Take(3).ToList();  // Re-rank logic here
```

---

### Problem 4: Stale Data
```csharp
// Update chunks periodically
public async Task RefreshDocumentsAsync()
{
    // Delete old collection
    await handshake.DeleteCollectionAsync("my-documents");
    
    // Re-process and store
    var chunks = await processor.ChunkAllDocumentsAsync("./documents");
    var embeddings = await embeddingGen.GenerateEmbeddingsAsync(chunks);
    await vectorStore.StoreChunksAsync(embeddings, "my-documents");
}
```

---

## 📊 Performance Considerations

| Operation | Time | Notes |
|-----------|------|-------|
| Chunk 1000 pages | ~5s | Fast, local |
| Embed 1000 chunks | ~30s | API calls, batch processing |
| Store 1000 vectors | ~2s | Vector DB insert |
| Retrieve (search) | ~100ms | Fast vector similarity |
| Generate answer | ~2-5s | LLM API call |

**Total per query:** ~3-8 seconds (mostly LLM generation)

---

## 🔗 Next Steps

1. **[RAG with Different Chunkers](TUTORIALS_02_CHUNKERS.md)** - Try different chunking strategies
2. **[Vector Database Guide](TUTORIALS_04_VECTORDB.md)** - Use different vector databases (Pinecone, Weaviate, etc.)
3. **[Advanced Pipelines](TUTORIALS_05_PIPELINES.md)** - Multi-step processing workflows
4. **[Production Patterns](ADVANCED_RAG_PATTERNS.md)** - Scaling, caching, monitoring (coming soon)

---

**Happy Building! 🚀**
