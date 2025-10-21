# Chonkie.Net Infrastructure Pipeline Sample

This sample demonstrates the complete text processing pipeline using Chonkie.Net's infrastructure components.

## Pipeline Components

The sample showcases a **5-stage pipeline** for processing text documents:

```
Fetch → Preprocess → Chunk → Refine → Export
```

### 1. **Fetcher** (Data Ingestion)
- **FileFetcher**: Loads text files from directories
- Supports file filtering (e.g., `*.txt`, `*.md`)
- Handles both single files and recursive directory scanning

### 2. **Chef** (Text Preprocessing)
- **TextChef**: Cleans and normalizes text
  - Removes control characters
  - Normalizes whitespace
  - Trims unnecessary spacing
- **MarkdownChef**: Processes Markdown documents
- **TableChef**: Extracts tables from text

### 3. **Chunker** (Text Segmentation)
- Splits preprocessed text into manageable chunks
- Uses tokenizers to control chunk size
- Supports overlap between chunks for context preservation

### 4. **Refinery** (Post-Processing)
- **OverlapRefinery**: Merges overlapping or similar chunks
- **EmbeddingsRefinery**: Adds vector embeddings to chunks
- Optimizes chunk organization for downstream tasks

### 5. **Porter** (Export)
- **JsonPorter**: Exports chunks to JSON format
- Preserves metadata (token counts, indices, embeddings)
- Suitable for storage or further processing

## Running the Sample

From the solution root:

```bash
dotnet run --project samples/Chonkie.Infrastructure.Sample/Chonkie.Infrastructure.Sample.csproj
```

Or from the sample directory:

```bash
cd samples/Chonkie.Infrastructure.Sample
dotnet run
```

## What It Does

The sample application:

1. Creates sample data files in `./sample_data/` directory
2. Fetches all `.txt` files from the directory
3. Preprocesses each file (cleans, normalizes)
4. Chunks the text using TokenChunker with:
   - Chunk size: 50 tokens
   - Overlap: 10 tokens
5. Refines chunks by merging overlaps
6. Exports the final chunks to `chunked_output.json`

## Sample Output

The application displays progress for each stage:

```
Step 1: FETCH - Loading text files from directory...
   ✓ Fetched 3 file(s)

Step 2: PREPROCESS - Cleaning and normalizing text...
   ✓ Processed: ai_fundamentals.txt
   ✓ Processed: future_tech.txt
   ✓ Processed: ml_overview.txt

Step 3: CHUNK - Splitting text into chunks...
   ✓ Created 4 chunks

Step 4: REFINE - Optimizing chunks...
   ✓ Refined to 1 optimized chunks

Step 5: EXPORT - Saving results...
   ✓ Exported to: chunked_output.json
```

## Customizing the Pipeline

You can customize each stage by:

### Using Different Components

```csharp
// Use MarkdownChef instead of TextChef
var chef = new MarkdownChef();

// Use SentenceChunker instead of TokenChunker
var chunker = new SentenceChunker(tokenizer, chunkSize: 100);

// Add embeddings to chunks
var embeddings = new OpenAIEmbeddings(apiKey: "your-key");
var embeddingsRefinery = new EmbeddingsRefinery(embeddings);
```

### Adjusting Parameters

```csharp
// Change chunk size and overlap
var chunker = new TokenChunker(tokenizer, chunkSize: 100, chunkOverlap: 20);

// Adjust overlap merge threshold
var refinery = new OverlapRefinery(minOverlap: 16);
```

### Processing Different File Types

```csharp
// Fetch only Markdown files
var files = await fetcher.FetchAsync("./docs", "*.md");

// Process specific file
var files = await fetcher.FetchAsync("./document.txt");
```

## Use Cases

This pipeline pattern is ideal for:

- **Document Preprocessing**: Preparing documents for RAG (Retrieval Augmented Generation)
- **Data Ingestion**: Building knowledge bases from file collections
- **Text Analytics**: Processing large document sets for analysis
- **Search Indexing**: Creating searchable text chunks with embeddings
- **Content Migration**: Transforming and restructuring document collections

## Learn More

- **Core Chunking**: See [Chonkie.Sample](../Chonkie.Sample/) for basic chunking examples
- **Embeddings**: See integration tests for embedding provider examples
- **API Documentation**: Check XML documentation in source files

## Architecture

The infrastructure components follow a clean, composable design:

```
IFetcher    → Loads raw data
IChef       → Preprocesses text
IChunker    → Segments into chunks
IRefinery   → Post-processes chunks
IPorter     → Exports results
```

Each interface can be implemented independently, allowing custom components to be plugged into the pipeline.
