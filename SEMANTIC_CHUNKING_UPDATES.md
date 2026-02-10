# Semantic Chunking Pipeline Sample Update

## Summary

The Pipeline sample has been extended to support **full semantic chunking with embeddings**. This includes a complete semantic RAG (Retrieval Augmented Generation) pipeline demonstration.

## New Features Added

### Demo 4: Enhanced Semantic Pipeline with Embeddings
- **File**: [samples/Chonkie.Pipeline.Sample/Program.cs](samples/Chonkie.Pipeline.Sample/Program.cs#L195)
- **What's New**: 
  - Now uses `SemanticChunker` with actual embeddings instead of a placeholder
  - Configures embeddings from available providers (SentenceTransformers, Azure OpenAI, OpenAI)
  - Demonstrates semantic boundary detection using similarity thresholds
  - Includes error handling and fallback to recursive chunking

**Key Features**:
- Configurable similarity threshold (0.75f for semantic sensitivity)
- Adjustable similarity window for boundary detection
- Automatic embeddings provider selection
- Graceful fallback if embeddings fail

### Demo 6 (NEW): Semantic RAG Pipeline
- **File**: [samples/Chonkie.Pipeline.Sample/Program.cs](samples/Chonkie.Pipeline.Sample/Program.cs#L307)
- **What's New**: Complete semantic RAG demonstration

**Pipeline Steps**:

1. **Semantic Chunking**
   - Processes multiple documents with semantic boundaries
   - Uses SemanticChunker for coherent chunk boundaries
   - Refines chunks with OverlapRefinery for context preservation

2. **Semantic Indexing**
   - Generates embeddings for all chunks (384-dimensional vectors for SentenceTransformers)
   - Builds in-memory semantic index for demonstration

3. **Semantic Retrieval**
   - User provides a natural language query
   - Query is embedded using the same model
   - Cosine similarity search finds most relevant chunks
   - Returns top 3 most similar chunks with relevance scores

4. **Generation** (when LLM configured)
   - Combines retrieved context with query
   - Generates grounded answer using LLM (OpenAI or Azure OpenAI)
   - Falls back gracefully when no LLM is configured

**Example Output**:
```
--- Demo 6: Semantic RAG Pipeline ---

✓ Step 1: Semantic chunking
  - Processed 3 documents
  - Created N semantic chunks
  - Exported to semantic_chunks.json

✓ Step 2: Building semantic index
  - Generated embeddings for N chunks

✓ Step 3: Semantic retrieval
  - Query: "How do transformers and attention work in NLP?"
  - Searching N semantically-chunked documents
  - Retrieved 3 most relevant chunks:

  [1] Relevance: 78.5%
      "Transformers use attention mechanisms..."
```

### Demo 7: RAG Tutorial (Unchanged)
- Full walkthrough using Qdrant vector database
- Requires `--rag` flag to run (needs external dependencies)
- Integrates with Qdrant for persistent vector storage

## Embeddings Provider Support

The sample automatically detects and uses the first available embeddings provider:

1. **SentenceTransformers** (Local, Offline)
   - Environment variable: `CHONKIE_SENTENCE_TRANSFORMER_MODEL_PATH`
   - Default model: `all-MiniLM-L6-v2` (384 dimensions)
   - Best for: Development, offline use, privacy-sensitive applications

2. **Azure OpenAI**
   - Environment variables:
     - `AZURE_OPENAI_ENDPOINT`
     - `AZURE_OPENAI_API_KEY`
     - `AZURE_OPENAI_EMBEDDINGS_DEPLOYMENT`

3. **OpenAI**
   - Environment variable: `OPENAI_API_KEY`
   - Default model: `text-embedding-3-small`

4. **Demo Embeddings** (Fallback)
   - Deterministic, offline embeddings for demonstration
   - Uses SHA256 hashing for reproducible embeddings

## Semantic Chunker Configuration

```csharp
var semanticChunker = new SemanticChunker(
    tokenizer: new WordTokenizer(),
    embeddingModel: embeddings,
    threshold: 0.75f,           // Semantic similarity threshold (0-1)
    chunkSize: 50,              // Max tokens per chunk
    similarityWindow: 2,        // Sentences to consider for similarity
    minSentencesPerChunk: 1,    // Minimum sentences per chunk
    minCharactersPerSentence: 24
);
```

## Cosine Similarity Calculation

The sample includes a utility method for computing cosine similarity between embedding vectors:

```csharp
static float ComputeCosineSimilarity(float[] u, float[] v)
{
    // Normalized dot product
    // Range: 0.0 (completely different) to 1.0 (identical)
}
```

## Running the Samples

```bash
# Run all demos (1-6)
dotnet run

# Run with full RAG tutorial (demos 1-7)
dotnet run -- --rag
```

## Configuration for LLM Generation

To enable answer generation in Demo 6, configure one of:

### OpenAI
```bash
set OPENAI_API_KEY=sk-...
```

### Azure OpenAI
```bash
set AZURE_OPENAI_ENDPOINT=https://your-instance.openai.azure.com/
set AZURE_OPENAI_API_KEY=your-key
set AZURE_OPENAI_DEPLOYMENT_EMBEDDINGS=your-deployment-name
```

## Output Files

Demo 6 creates the following files (automatically cleaned up):
- `semantic_chunks.json` - Exported chunks in JSON format
- `semantic_rag_docs/` - Temporary directory with sample documents

## Key Improvements

✅ **Full Semantic Chunking**: Uses similarity-based peak detection for meaningful boundaries  
✅ **Multiple Embeddings Providers**: Flexible configuration for different use cases  
✅ **Complete RAG Pipeline**: Demonstrates end-to-end semantic retrieval  
✅ **Error Handling**: Graceful fallbacks when external services unavailable  
✅ **Configurable Parameters**: Adjust thresholds and windows for different content types  
✅ **Example Output**: Clear demonstration of relevance scoring and retrieval  

## Related Components

- [SemanticChunker](src/Chonkie.Chunkers/SemanticChunker.cs)
- [Embeddings Interfaces](src/Chonkie.Embeddings/Interfaces/)
- [FluentPipeline Class](samples/Chonkie.Pipeline.Sample/Program.cs#L720)
- [IEmbeddings Interface](src/Chonkie.Embeddings/Interfaces/IEmbeddings.cs)
