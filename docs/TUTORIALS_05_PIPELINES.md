# 🔗 Tutorial: Advanced Pipeline Configuration

**Time to complete:** 25-35 minutes  
**Level:** Advanced  
**What you'll learn:** Build complex, production-ready pipelines for document processing

---

## 🏗️ What is CHOMP?

**CHOMP** is Chonkie's standardized architecture for document processing:

```text
┌─────────────────────────────────────────────────────────────────┐
│                     CHOMP Architecture                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Fetcher → Chef → Chunker → Refinery → Porter/Handshake         │
│     ↓        ↓        ↓        ↓              ↓                 │
│   Input  Process  Split   Enhance         Output                │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘

1. Fetcher: Get documents from files, APIs, databases
2. Chef: Parse and convert to standardized Document format
3. Chunker: Split documents into manageable chunks
4. Refinery: Post-process, enhance, filter chunks
5. Porter/Handshake: Export or store results
```text

**Key:** Pipeline components automatically reorder to follow CHOMP! ✨

---

## 🚀 Basic Pipeline (Minimal)

The simplest pipeline - just chunking:

```csharp
using Chonkie.Pipeline;
using Chonkie.Chunkers;
using Chonkie.Tokenizers;

var result = await FluentPipeline.Create()
    .WithText("Your document text...")
    .ChunkWith(new RecursiveChunker(new WordTokenizer(), chunkSize: 512))
    .RunAsync();

// result.Chunks contains the output
foreach (var chunk in result.Chunks)
{
    Console.WriteLine(chunk.Text);
}
```text

---

## 📂 Fetcher Patterns

### Pattern 1: Single File

```csharp
var result = await FluentPipeline.Create()
    .FetchFrom("file.txt")
    .ChunkWith(new RecursiveChunker(new WordTokenizer(), 512))
    .RunAsync();
```text

### Pattern 2: Multiple Files

```csharp
var files = Directory.GetFiles("./documents", "*.txt");

foreach (var file in files)
{
    var result = await FluentPipeline.Create()
        .FetchFrom(file)
        .ChunkWith(new RecursiveChunker(new WordTokenizer(), 512))
        .RunAsync();

    Console.WriteLine($"Processed {file}: {result.Chunks.Count} chunks");
}
```

### Pattern 3: Directory Processing

```csharp
var result = await FluentPipeline.Create()
    .FetchFrom(new DirectoryInfo("./documents"))
    .ChunkWith(new RecursiveChunker(new WordTokenizer(), 512))
    .RunAsync();

// result.Chunks contains all chunks from all files
Console.WriteLine($"Total chunks: {result.Chunks.Count}");
```

### Pattern 4: Direct Text (No Fetcher)

```csharp
var textData = new List<string>
{
    "First document text...",
    "Second document text...",
    "Third document text..."
};

var result = await FluentPipeline.Create()
    .WithText(textData)
    .ChunkWith(new RecursiveChunker(new WordTokenizer(), 512))
    .RunAsync();
```

---

## 👨‍🍳 Chef Patterns

**Chefs** transform raw documents into standardized format:

```text
Raw File → Chef → Document
  │       ├─ TextChef: Plain text
  │       ├─ MarkdownChef: Markdown with structure
  │       ├─ CodeChef: Source code
  │       └─ TableChef: CSV, TSV, structured data
  ↓
Document {
    RawContent: string,
    Type: DocumentType,
    Metadata: Dictionary,
    Lines: List<DocumentLine>
}
```

### Pattern 1: Text Chef (Default)

```csharp
var result = await FluentPipeline.Create()
    .FetchFrom("document.txt")
    .ProcessWith(new TextChef())  // Explicit (optional - already default)
    .ChunkWith(new RecursiveChunker(new WordTokenizer(), 512))
    .RunAsync();
```

### Pattern 2: Markdown Chef

```csharp
var result = await FluentPipeline.Create()
    .FetchFrom("README.md")
    .ProcessWith(new MarkdownChef())  // Preserves headers, code blocks
    .ChunkWith(new RecursiveChunker(new WordTokenizer(), 512))
    .RunAsync();
```

### Pattern 3: Code Chef

```csharp
var result = await FluentPipeline.Create()
    .FetchFrom("Program.cs")
    .ProcessWith(new CodeChef())  // Syntax highlighting preserved
    .ChunkWith(new CodeChunker(new WordTokenizer(), chunkSize: 512))
    .RunAsync();
```

### Pattern 4: Table Chef

```csharp
var result = await FluentPipeline.Create()
    .FetchFrom("data.csv")
    .ProcessWith(new TableChef())  // Treats as structured data
    .ChunkWith(new TableChunker(new WordTokenizer(), chunkSize: 512))
    .RunAsync();
```

---

## ✂️ Chunker Patterns

### Pattern 1: Simple Chunking

```csharp
await FluentPipeline.Create()
    .WithText(text)
    .ChunkWith(new TokenChunker(tokenizer, chunkSize: 512))
    .RunAsync();
```

### Pattern 2: Semantic Chunking

```csharp
using Chonkie.Embeddings.OpenAI;

var embeddings = new OpenAIEmbeddings(apiKey: "sk-...");

await FluentPipeline.Create()
    .WithText(text)
    .ChunkWith(new SemanticChunker(tokenizer, embeddings, threshold: 0.8f))
    .RunAsync();
```

### Pattern 3: Specialized Chunking

```csharp
// For code files
await FluentPipeline.Create()
    .FetchFrom("main.py")
    .ProcessWith(new CodeChef())
    .ChunkWith(new CodeChunker(tokenizer))
    .RunAsync();

// For markdown
await FluentPipeline.Create()
    .FetchFrom("docs.md")
    .ProcessWith(new MarkdownChef())
    .ChunkWith(new MarkdownChunker(tokenizer))
    .RunAsync();
```

---

## 🔧 Refinery Patterns

**Refineries** post-process chunks (filter, enhance, clean):

Available refineries:

- `OverlapRefinery` - Ensure minimum overlap between chunks
- `LengthRefinery` - Filter by chunk length
- `DuplicateRefinery` - Remove duplicate chunks
- Custom refineries

### Pattern 1: Overlap Refinery

```csharp
using Chonkie.Refineries;

await FluentPipeline.Create()
    .WithText(text)
    .ChunkWith(new RecursiveChunker(tokenizer, chunkSize: 512, chunkOverlap: 25))
    .RefineWith(new OverlapRefinery(minOverlap: 50))  // Ensure minimum overlap
    .RunAsync();
```

### Pattern 2: Multiple Refineries

```csharp
// Chain refineries
await FluentPipeline.Create()
    .WithText(text)
    .ChunkWith(new RecursiveChunker(tokenizer, 512))
    .RefineWith(new OverlapRefinery(minOverlap: 50))
    .RefineWith(new LengthRefinery(minLength: 100))  // Remove tiny chunks
    .RefineWith(new DuplicateRefinery())  // Remove duplicates
    .RunAsync();
```

### Pattern 3: Custom Refinery

```csharp
public class CustomRefinery : IRefinery
{
    public List<Chunk> Refine(List<Chunk> chunks)
    {
        // Filter chunks that contain "TODO"
        return chunks
            .Where(c => !c.Text.Contains("TODO"))
            .ToList();
    }
}

// Use it
await FluentPipeline.Create()
    .WithText(text)
    .ChunkWith(chunker)
    .RefineWith(new CustomRefinery())
    .RunAsync();
```

---

## 📤 Porter Patterns

**Porters** export chunks to different formats:

Available porters:

- `JsonPorter` - Export to JSON
- Custom porters for CSV, Parquet, etc.

### Pattern 1: Export to JSON

```csharp
using Chonkie.Porters.Json;

var result = await FluentPipeline.Create()
    .WithText(text)
    .ChunkWith(new RecursiveChunker(tokenizer, 512))
    .ExportWith(new JsonPorter(outputPath: "./output.json"))
    .RunAsync();

// Creates ./output.json with all chunks
```

### Pattern 2: Export Multiple Formats

```csharp
var result = await FluentPipeline.Create()
    .WithText(text)
    .ChunkWith(new RecursiveChunker(tokenizer, 512))
    .ExportWith(new JsonPorter("./output.json"))
    .RunAsync();

// Then manually export to CSV using extension methods
var csvContent = result.Chunks.ToCsvFormat();
File.WriteAllText("./output.csv", csvContent);
```

---

## 🤝 Handshake Patterns

**Handshakes** store chunks in vector databases:

### Pattern 1: Store in Qdrant

```csharp
using Chonkie.Handshakes.Qdrant;
using Chonkie.Embeddings.OpenAI;

var embeddings = new OpenAIEmbeddings(apiKey: "sk-...");
var vectorDb = new QdrantHandshake("localhost:6333");

var result = await FluentPipeline.Create()
    .WithText(text)
    .ChunkWith(new RecursiveChunker(tokenizer, 512))
    .RunAsync();

// Manually embed and store
var vectors = await embeddings.EmbedAsync(
    result.Chunks.Select(c => c.Text).ToList()
);

for (int i = 0; i < result.Chunks.Count; i++)
{
    var record = new EmbeddingRecord
    {
        Id = Guid.NewGuid().ToString(),
        Text = result.Chunks[i].Text,
        Embedding = vectors[i],
        Metadata = new Dictionary<string, string>
        {
            ["chunk_index"] = i.ToString()
        }
    };

    await vectorDb.WriteAsync(new[] { record }, "documents");
}
```

### Pattern 2: Store in Pinecone

```csharp
using Chonkie.Handshakes.Pinecone;

var vectorDb = new PineconeHandshake(apiKey: "your-key");

// Same flow as Qdrant - interface is identical!
```

---

## 🎯 Complete Production Pipeline

Here's a real-world, production-ready pipeline:

```csharp
public class ProductionDocumentProcessor
{
    private readonly ITokenizer _tokenizer;
    private readonly IEmbeddingsModel _embeddings;
    private readonly IHandshake _vectorDb;
    private readonly ILogger _logger;

    public ProductionDocumentProcessor(
        ITokenizer tokenizer,
        IEmbeddingsModel embeddings,
        IHandshake vectorDb,
        ILogger logger)
    {
        _tokenizer = tokenizer;
        _embeddings = embeddings;
        _vectorDb = vectorDb;
        _logger = logger;
    }

    public async Task ProcessDocumentsAsync(
        string documentsPath,
        string collectionName,
        int batchSize = 50)
    {
        try
        {
            _logger.Information("Starting document processing from {path}", documentsPath);

            // Step 1: Fetch and chunk
            var documents = Directory.GetFiles(documentsPath, "*.txt");
            var allChunks = new List<Chunk>();

            foreach (var doc in documents)
            {
                var result = await FluentPipeline.Create()
                    .FetchFrom(doc)
                    .ProcessWith(new TextChef())
                    .ChunkWith(new RecursiveChunker(
                        _tokenizer,
                        chunkSize: 512,
                        chunkOverlap: 50))
                    .RefineWith(new OverlapRefinery(minOverlap: 25))
                    .RefineWith(new LengthRefinery(minLength: 10))
                    .RunAsync();

                allChunks.AddRange(result.Chunks);
                _logger.Information(
                    "Processed {file}: {count} chunks",
                    Path.GetFileName(doc),
                    result.Chunks.Count);
            }

            _logger.Information("Total chunks created: {count}", allChunks.Count);

            // Step 2: Embed in batches
            _logger.Information("Generating embeddings...");

            for (int i = 0; i < allChunks.Count; i += batchSize)
            {
                var batch = allChunks.Skip(i).Take(batchSize).ToList();
                var texts = batch.Select(c => c.Text).ToList();

                _logger.Debug("Embedding batch {batch}/{total}", 
                    i / batchSize + 1, (allChunks.Count + batchSize - 1) / batchSize);

                var embeddings = await _embeddings.EmbedAsync(texts);

                // Step 3: Store in vector database
                var records = batch
                    .Select((chunk, idx) => new EmbeddingRecord
                    {
                        Id = $"{collectionName}_{Guid.NewGuid()}",
                        Text = chunk.Text,
                        Embedding = embeddings[idx],
                        Metadata = new Dictionary<string, string>
                        {
                            ["start_index"] = chunk.StartIndex.ToString(),
                            ["end_index"] = chunk.EndIndex.ToString(),
                            ["token_count"] = chunk.TokenCount.ToString()
                        }
                    })
                    .ToList();

                await _vectorDb.WriteAsync(records, collectionName);
                _logger.Debug("Stored {count} vectors", records.Count);
            }

            _logger.Information("✅ Document processing complete!");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "❌ Document processing failed");
            throw;
        }
    }
}

// Usage
var processor = new ProductionDocumentProcessor(
    tokenizer: new WordTokenizer(),
    embeddings: new OpenAIEmbeddings(apiKey: "sk-..."),
    vectorDb: new QdrantHandshake("localhost:6333"),
    logger: logger
);

await processor.ProcessDocumentsAsync(
    documentsPath: "./documents",
    collectionName: "knowledge-base"
);
```

---

## 🔍 Pipeline Validation

Validate your pipeline before running on large datasets:

```csharp
public async Task ValidatePipelineAsync(string testDocPath)
{
    Console.WriteLine("🧪 Validating pipeline...\n");

    // Test with single document
    var result = await FluentPipeline.Create()
        .FetchFrom(testDocPath)
        .ProcessWith(new TextChef())
        .ChunkWith(new RecursiveChunker(tokenizer, 512))
        .RefineWith(new OverlapRefinery(minOverlap: 25))
        .RunAsync();

    // Print diagnostics
    Console.WriteLine($"✓ Chunks created: {result.Chunks.Count}");
    var avgChunkSize = result.Chunks.Average(c => c.TokenCount);
    Console.WriteLine($"✓ Avg chunk size: {avgChunkSize:F0} tokens");
    Console.WriteLine($"✓ Min chunk: {result.Chunks.Min(c => c.TokenCount)} tokens");
    Console.WriteLine($"✓ Max chunk: {result.Chunks.Max(c => c.TokenCount)} tokens");

    if (result.Chunks.Any(c => c.TokenCount > 2000))
    {
        Console.WriteLine($"⚠️  Warning: Some chunks exceed 2000 tokens");
    }

    if (result.Chunks.Any(c => c.TokenCount < 10))
    {
        Console.WriteLine($"⚠️  Warning: Some chunks are very small (<10 tokens)");
    }

    Console.WriteLine("✅ Pipeline validation complete!\n");
}
```

---

## ⚠️ Common Pipeline Issues

### Issue 1: Memory Issues with Large Documents

```csharp
// ❌ Loading entire file at once
var text = File.ReadAllText("huge-file.txt");
await FluentPipeline.Create().WithText(text)...

// ✅ Process file directly (streaming)
await FluentPipeline.Create()
    .FetchFrom("huge-file.txt")  // Streams, doesn't load all at once
    .ChunkWith(chunker)
    .RunAsync();
```

---

### Issue 2: Slow Embedding Generation

```csharp
// ❌ Embedding one at a time
foreach (var chunk in chunks)
{
    var vec = await embeddings.EmbedAsync(new[] { chunk.Text });
}

// ✅ Batch embedding (much faster)
var texts = chunks.Select(c => c.Text).ToList();
var vecs = await embeddings.EmbedAsync(texts);
```

---

### Issue 3: Inconsistent Refineries

```csharp
// ❌ Refineries may remove chunks unexpectedly
await FluentPipeline.Create()
    .WithText(text)
    .ChunkWith(chunker)
    .RefineWith(new LengthRefinery(minLength: 500))  // Might remove chunks!
    .RunAsync();

// ✅ Validate refinery settings
Console.WriteLine($"Before refinery: {beforeRefinement.Count} chunks");
Console.WriteLine($"After refinery: {afterRefinement.Count} chunks");
var filteredCount = beforeRefinement.Count - afterRefinement.Count;
Console.WriteLine($"Filtered out: {filteredCount} chunks");
```

---

## 📈 Pipeline Performance Tips

1. **Use FastChunker for huge documents**

    ```csharp
   .ChunkWith(new FastChunker(tokenizer, 512))  // 3-5x faster
   ```

2. **Batch embeddings**

    ```csharp
   // Embed 100 at a time, not 1 at a time
   var vectors = await embeddings.EmbedAsync(texts.Take(100));
   ```

3. **Parallel document processing**

    ```csharp
   var files = Directory.GetFiles("./documents", "*.txt");

   await Parallel.ForEachAsync(files, async file =>
   {
       await ProcessSingle(file);
   });
   ```

4. **Skip unnecessary steps**

    ```csharp
   // Don't use semantic chunking if you don't need it
   // TokenChunker is 5x faster
   ```

---

## 🚀 Next Steps

1. **[Quick-Start](TUTORIALS_01_QUICK_START.md)** - Back to basics
2. **[RAG System](TUTORIALS_02_RAG.md)** - Complete example
3. **[Chunkers Deep Dive](TUTORIALS_03_CHUNKERS.md)** - Chunker details
4. **[Vector Database](TUTORIALS_04_VECTORDB.md)** - Storage options
