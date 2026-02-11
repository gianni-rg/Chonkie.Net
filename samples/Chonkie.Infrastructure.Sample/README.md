# Chonkie.Net Infrastructure Pipeline Sample

This sample demonstrates the complete text processing pipeline using Chonkie.Net's infrastructure components, showcasing both the traditional 5-stage pipeline infrastructure and vector database integration (Handshakes) for storing and searching embeddings.

## What This Sample Covers

The sample includes two main demonstrations:

1. **Infrastructure Pipeline** (Default) - The 5-stage text processing workflow: Fetch → Preprocess → Chunk → Refine → Export
2. **Vector Database Integration** (Optional) - Storing chunks with embeddings in 9 different vector databases using the unified Handshake interface

Both can be run independently or together.

## Pipeline Components

The sample showcases a **5-stage pipeline** for processing text documents:

```text
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

**By default**, running without arguments executes the infrastructure pipeline demo. To run the vector database demo, see the [Vector Database Demo](#vector-database-demo-optional) section below.

## What Are Handshakes?

**Handshakes** are Chonkie.Net's unified abstraction for vector database integrations. They provide a consistent interface (`IHandshake`) for storing and searching embeddings across different vector databases:

```csharp
public interface IHandshake
{
    Task WriteAsync(IEnumerable<Chunk> chunks);       // Store embeddings
    Task SearchAsync(string query, int limit = 5);    // Find similar vectors
}
```

**The key advantage:** Switch between databases by changing **one line of code**:

```csharp
// Using Qdrant
IHandshake db = new QdrantHandshake("http://localhost:6333", "chonkie_samples", embeddings);

// Switch to Pinecone - just change this line!
IHandshake db = new PineconeHandshake(apiKey, indexName, embeddings);
```

Chonkie.Net supports 9 vector databases out of the box. See [API Reference - Handshakes](../../docs/API_REFERENCE_HANDSHAKES.md) for full interface details.

## Vector Database Demo (Optional)

> Running this demo requires running a local or cloud instance of one of the supported vector databases.
Use the provided `docker-compose.handshakes.yml` file to quickly spin up local instances of Qdrant, Chroma, Milvus, mongodb, postgres, weaviate, and/or Elasticsearch.

Run the vector database demo with:

```bash
dotnet run --project samples/Chonkie.Infrastructure.Sample/Chonkie.Infrastructure.Sample.csproj -- --vector-db --provider qdrant
```

You can also run both pipeline and vector DB demo:

```bash
dotnet run --project samples/Chonkie.Infrastructure.Sample/Chonkie.Infrastructure.Sample.csproj -- --all --vector-db --provider qdrant
```

Optional flags:

- `--provider` (qdrant, pinecone, weaviate, chroma, mongodb, pgvector, elasticsearch, milvus, turbopuffer) - **default: qdrant**
- `--query` (custom search query) - **default: "How does chunking help RAG?"**
- `--topk` (number of results) - **default: 3**

### Provider Configuration

Set the environment variables for your chosen provider:

```text
# Qdrant
CHONKIE_QDRANT_URL=http://localhost:6333
CHONKIE_QDRANT_COLLECTION=chonkie_samples

# Pinecone
PINECONE_API_KEY=your-key
PINECONE_INDEX=your-index
PINECONE_NAMESPACE=default

# Weaviate (cloud)
WEAVIATE_URL=https://your-instance.weaviate.network
WEAVIATE_API_KEY=your-key
WEAVIATE_CLASS=ChonkieChunks

# Chroma
CHONKIE_CHROMA_URL=http://localhost:8000
CHONKIE_CHROMA_COLLECTION=chonkie_samples

# MongoDB
CHONKIE_MONGODB_URI=mongodb://localhost:27017
CHONKIE_MONGODB_DB=chonkie_db
CHONKIE_MONGODB_COLLECTION=chonkie_collection

# pgvector
CHONKIE_PGVECTOR_CONNECTION_STRING=Host=localhost;Database=postgres;Username=postgres;Password=postgres
CHONKIE_PGVECTOR_COLLECTION=chonkie_chunks
CHONKIE_PGVECTOR_DIMENSIONS=1536

# Elasticsearch (8+)
CHONKIE_ELASTICSEARCH_URL=http://localhost:9200
CHONKIE_ELASTICSEARCH_INDEX=chonkie_samples
CHONKIE_ELASTICSEARCH_API_KEY=your-key

# Milvus
CHONKIE_MILVUS_URL=http://localhost:19530
CHONKIE_MILVUS_COLLECTION=chonkie_samples

# Turbopuffer
TURBOPUFFER_API_KEY=your-key
TURBOPUFFER_API_URL=https://api.turbopuffer.com (optional, defaults to official cloud)
TURBOPUFFER_NAMESPACE=chonkie_samples
```

### Embeddings Configuration

The sample automatically detects embeddings providers in this order (falls back to next if not configured):

**1. Local Sentence Transformers (Recommended for offline/local development)**

```text
CHONKIE_SENTENCE_TRANSFORMERS_MODEL_PATH=./models/all-MiniLM-L6-v2
```

**2. Azure OpenAI**

```text
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_API_KEY=your-key
AZURE_OPENAI_EMBEDDINGS_DEPLOYMENT=text-embedding-3-small
```

**3. OpenAI**

```text
OPENAI_API_KEY=your-key
OPENAI_EMBEDDINGS_MODEL=text-embedding-3-small
OPENAI_EMBEDDINGS_DIMENSION=1536 (optional)
```

**Fallback: Demo Embeddings**

If no embeddings provider is configured, the sample uses a deterministic demo embeddings generator (SHA256-based, no external dependencies). This is useful for testing without API keys, but produces non-semantic embeddings.

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

### Infrastructure Pipeline Output

When running the default pipeline:

```text
=== Chonkie.Net Infrastructure Pipeline Demo ===

Pipeline: Fetch → Preprocess → Chunk → Refine → Export

================================================================================

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

================================================================================
Pipeline Summary:
  Files processed:  3
  Initial chunks:   4
  Refined chunks:   1
  Output file:      chunked_output.json
================================================================================

Sample Chunk Preview:
  Text: Artificial Intelligence represents the simulation of human intelligence...
  Tokens: 45
  Range: [0, 275]

=== Infrastructure Pipeline Complete ===
```

### Vector Database Demo Output

When running with `--vector-db --provider qdrant`:

```text
=== Chonkie.Net Vector Database Demo ===

Provider: qdrant
Embeddings: sentence-transformers:all-MiniLM-L6-v2

✓ Stored sample chunks in vector database
✓ Qdrant returned 3 result(s)
Query: "How does chunking help RAG?"
 - Chunking breaks long documents into manageable pieces. (score: 0.85)
 - Embeddings capture semantic meaning for retrieval. (score: 0.78)
 - RAG combines retrieval and generation for grounded answers. (score: 0.72)

=== Vector Database Demo Complete ===
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

- **Core Chunking**: See [Chonkie.Sample](../Chonkie.Sample/) for basic chunking examples and all chunker types
- **Fluent Pipelines**: See [Chonkie.Pipeline.Sample](../Chonkie.Pipeline.Sample/) for chainable pipeline construction patterns
- **Visualization**: See [Chonkie.Visualizer.Sample](../Chonkie.Visualizer.Sample/) for console visualization utilities

## Architecture

The infrastructure components follow a clean, composable design:

```text
IFetcher    → Loads raw data
IChef       → Preprocesses text
IChunker    → Segments into chunks
IRefinery   → Post-processes chunks
IPorter     → Exports results
```

Each interface can be implemented independently, allowing custom components to be plugged into the pipeline.
