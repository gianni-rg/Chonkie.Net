# Chonkie.Net Fluent Pipeline API Sample

This sample demonstrates Chonkie.Net's fluent, chainable pipeline API for building text processing workflows in a clean, declarative style - similar to Python's Pipeline class.

## What is a Fluent Pipeline?

A fluent pipeline provides a **method chaining interface** that reads like natural language and composes complex operations elegantly:

```csharp
var result = await FluentPipeline.Create()
    .FetchFrom(fetcher, path)
    .ProcessWith(chef)
    .ChunkWith(chunker)
    .RefineWith(refinery)
    .ExportTo(porter, outputPath)
    .RunAsync();
```

## Pipeline Stages

The pipeline follows the **CHOMP architecture**:

```
Fetch → Process → Chunk → Refine → Export
```

### 1. **Fetch** (Optional - can use direct text)
Load data from files, directories, or other sources
```csharp
.FetchFrom(new FileFetcher(), "document.txt")
.FetchFrom(new FileFetcher(), "./docs", "*.md")
```

### 2. **Process** (Optional)
Clean and preprocess text
```csharp
.ProcessWith(new TextChef())
.ProcessWith(new MarkdownChef())
```

### 3. **Chunk** (Required)
Split text into chunks
```csharp
.ChunkWith(new TokenChunker(tokenizer, chunkSize: 512))
.ChunkWith(new RecursiveChunker(tokenizer, chunkSize: 512))
```

### 4. **Refine** (Optional)
Post-process and optimize chunks
```csharp
.RefineWith(new OverlapRefinery(minOverlap: 8))
.RefineWith(new EmbeddingsRefinery(embeddings))
```

### 5. **Export** (Optional)
Save results to files or databases
```csharp
.ExportTo(new JsonPorter(), "output.json")
```

## Running the Sample

From the solution root:

```bash
dotnet run --project samples/Chonkie.Pipeline.Sample/Chonkie.Pipeline.Sample.csproj
```

Or from the sample directory:

```bash
cd samples/Chonkie.Pipeline.Sample
dotnet run
```

## Sample Demonstrations

### Demo 1: Simple Pipeline - Direct Text

Process text without file I/O:

```csharp
var result = await FluentPipeline.Create()
    .WithText("Your text here...")
    .ChunkWith(new TokenChunker(tokenizer, chunkSize: 50))
    .RunAsync();
```

**Use Case**: Quick text processing, API responses, in-memory data

### Demo 2: File-Based Pipeline

Load and process a single file:

```csharp
var result = await FluentPipeline.Create()
    .FetchFrom(new FileFetcher(), "document.txt")
    .ProcessWith(new TextChef())
    .ChunkWith(new SentenceChunker(tokenizer, chunkSize: 15))
    .RunAsync();
```

**Use Case**: Document processing, report generation, content analysis

### Demo 3: Complete RAG Pipeline

Full pipeline with all stages:

```csharp
var result = await FluentPipeline.Create()
    .FetchFrom(new FileFetcher(), "./docs", "*.txt")
    .ProcessWith(new TextChef())
    .ChunkWith(new RecursiveChunker(tokenizer, chunkSize: 20))
    .RefineWith(new OverlapRefinery(minOverlap: 5))
    .ExportTo(new JsonPorter(), "output.json")
    .RunAsync();
```

**Use Case**: RAG knowledge base ingestion, search indexing, content migration

### Demo 4: Semantic Pipeline

Pattern for semantic chunking (expandable with embeddings):

```csharp
var result = await FluentPipeline.Create()
    .WithText(text)
    .ProcessWith(new TextChef())
    .ChunkWith(new RecursiveChunker(tokenizer, chunkSize: 15))
    .RunAsync();
```

**Use Case**: Semantic search, similarity-based chunking, content clustering

### Demo 5: CHOMP Pipeline (String-Based API)

Demonstrates the built-in `Chonkie.Pipeline.Pipeline` API with string aliases:

```csharp
var result = await new Pipeline()
    .FetchFrom("file", new { path = "doc.txt" })
    .ProcessWith("text")
    .ChunkWith("recursive", new { chunk_size = 40 })
    .RefineWith("overlap", new { context_size = 8 })
    .ExportWith("json", new { path = "chomp_output.json" })
    .RunAsync();
```

**Use Case**: Configuration-driven pipelines, parity with Python CHOMP examples

### Demo 6: RAG Tutorial Walkthrough (Optional)

Run with `--rag` to enable the end-to-end RAG flow:

```bash
dotnet run --project samples/Chonkie.Pipeline.Sample/Chonkie.Pipeline.Sample.csproj -- --rag
```

The RAG demo covers:

- Chunking documents with `RecursiveChunker`
- Generating embeddings
- Writing to Qdrant
- Retrieving top-k context
- Generating an answer with OpenAI/Azure OpenAI Genies (optional)

### Embeddings Configuration

Configure one of the following:

```text
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_API_KEY=your-key
AZURE_OPENAI_EMBEDDINGS_DEPLOYMENT=text-embedding-3-small
```

```text
OPENAI_API_KEY=your-key
OPENAI_EMBEDDINGS_MODEL=text-embedding-3-small
```

```text
CHONKIE_SENTENCE_TRANSFORMER_MODEL_PATH=./models/all-MiniLM-L6-v2
```

### Qdrant Configuration (RAG Demo)

```text
CHONKIE_QDRANT_URL=http://localhost:6333
CHONKIE_QDRANT_COLLECTION=rag_tutorial
```

## Benefits of Fluent API

### ✅ Readable Code
```csharp
// Clear, self-documenting pipeline
var result = await FluentPipeline.Create()
    .FetchFrom(fetcher, path)
    .ProcessWith(chef)
    .ChunkWith(chunker)
    .RunAsync();
```

### ✅ Composable
```csharp
// Build reusable pipeline templates
var basePipeline = FluentPipeline.Create()
    .ProcessWith(new TextChef())
    .ChunkWith(new TokenChunker(tokenizer, 512));

// Extend for different use cases
var ragPipeline = basePipeline.RefineWith(new OverlapRefinery());
var exportPipeline = basePipeline.ExportTo(porter, path);
```

### ✅ Declarative
```csharp
// Describe WHAT you want, not HOW to do it
Pipeline.Create()
    .FetchFrom(fetcher, "./docs")
    .ChunkWith(chunker)
    .ExportTo(porter, "output.json");
```

### ✅ Type-Safe
Full IntelliSense support and compile-time checking

## Pipeline Result

The `PipelineResult` object provides detailed metrics:

```csharp
public class PipelineResult
{
    public int SourceCount { get; set; }           // Files/texts processed
    public int InitialChunkCount { get; set; }     // Chunks before refinement
    public List<Chunk> FinalChunks { get; set; }   // Final chunks
    public string? ExportPath { get; set; }        // Where exported
}
```

Access the results:

```csharp
var result = await pipeline.RunAsync();

Console.WriteLine($"Processed {result.SourceCount} sources");
Console.WriteLine($"Created {result.InitialChunkCount} initial chunks");
Console.WriteLine($"Refined to {result.FinalChunks.Count} final chunks");

foreach (var chunk in result.FinalChunks)
{
    Console.WriteLine($"Chunk: {chunk.Text}");
    Console.WriteLine($"Tokens: {chunk.TokenCount}");
}
```

## Comparison with Python

This .NET implementation mirrors Python's Pipeline API:

**Python:**
```python
doc = (Pipeline()
    .fetch_from("file", path="doc.txt")
    .process_with("text")
    .chunk_with("recursive", chunk_size=512)
    .refine_with("overlap", context_size=50)
    .run())
```

**.NET:**
```csharp
var result = await FluentPipeline.Create()
    .FetchFrom(new FileFetcher(), "doc.txt")
    .ProcessWith(new TextChef())
    .ChunkWith(new RecursiveChunker(tokenizer, chunkSize: 512))
    .RefineWith(new OverlapRefinery(minOverlap: 50))
    .RunAsync();
```

## Customizing Pipelines

### Skip Optional Stages

```csharp
// Direct text input - no fetch or process needed
var result = await FluentPipeline.Create()
    .WithText("Your text")
    .ChunkWith(chunker)
    .RunAsync();
```

### Chain Multiple Refinements

```csharp
// (Future) Multiple refinement stages
var result = await pipeline
    .RefineWith(new OverlapRefinery())
    .RefineWith(new EmbeddingsRefinery(embeddings))
    .RunAsync();
```

### Different Chunking Strategies

```csharp
// Token-based
.ChunkWith(new TokenChunker(tokenizer, chunkSize: 512))

// Sentence-based
.ChunkWith(new SentenceChunker(tokenizer, chunkSize: 100))

// Recursive with custom separators
.ChunkWith(new RecursiveChunker(
    tokenizer, 
    chunkSize: 512,
    separators: ["\n\n", "\n", ". ", " "]))

// Semantic (when available)
.ChunkWith(new SemanticChunker(tokenizer, embeddings))
```

## Error Handling

Pipelines validate configuration:

```csharp
try
{
    var result = await FluentPipeline.Create()
        // Missing chunker - will throw
        .WithText("text")
        .RunAsync();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Pipeline error: {ex.Message}");
}
```

**Required**: At least one chunker
**Required**: Input source (FetchFrom or WithText)

## Use Cases

### RAG Knowledge Base Ingestion
```csharp
await FluentPipeline.Create()
    .FetchFrom(new FileFetcher(), "./knowledge_base", "*.md")
    .ProcessWith(new MarkdownChef())
    .ChunkWith(new RecursiveChunker(tokenizer, 512))
    .RefineWith(new OverlapRefinery(minOverlap: 50))
    .ExportTo(new JsonPorter(), "knowledge.json")
    .RunAsync();
```

### Document Analysis
```csharp
await FluentPipeline.Create()
    .FetchFrom(new FileFetcher(), "./reports")
    .ProcessWith(new TextChef())
    .ChunkWith(new SentenceChunker(tokenizer, 100))
    .RunAsync();
```

### Content Migration
```csharp
await FluentPipeline.Create()
    .FetchFrom(new FileFetcher(), "./old_content")
    .ProcessWith(new TextChef())
    .ChunkWith(new TokenChunker(tokenizer, 512))
    .ExportTo(new JsonPorter(), "migrated.json")
    .RunAsync();
```

## Learn More

- **Core Chunking**: See [Chonkie.Sample](../Chonkie.Sample/) for basic chunking
- **Infrastructure**: See [Chonkie.Infrastructure.Sample](../Chonkie.Infrastructure.Sample/) for component-based approach
- **Python Pipeline**: See [Python Pipeline Docs](https://docs.chonkie.ai/oss/pipelines) for original implementation

## Future Enhancements

This fluent pipeline API can be extended with:

- [ ] Multiple refinery chaining
- [ ] Handshake support (vector database connectors)
- [ ] Async streaming for large datasets
- [ ] Pipeline configuration serialization/deserialization
- [ ] Progress reporting and cancellation tokens
- [ ] Pipeline recipes (pre-configured templates)
