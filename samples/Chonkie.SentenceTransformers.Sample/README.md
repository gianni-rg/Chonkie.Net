# Sentence Transformer Embeddings Sample

This sample demonstrates how to use ONNX-based Sentence Transformer embeddings in Chonkie.Net.

## Prerequisites

1. **Convert a model to ONNX format:**

   Use the provided conversion script to convert a Sentence Transformer model from Hugging Face to ONNX format compatible with Chonkie.Net. For example, to convert the `all-MiniLM-L6-v2` model:

   ```shell
   uv run ./scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2
   ```

   See [scripts/README.md](../../scripts/README.md) for more details and options.

2. **Build the sample:**

```shell
cd samples/Chonkie.SentenceTransformers.Sample
dotnet build
```

### How Models are Selected

The sample automatically selects the appropriate tokenizer based on available model files:

- **BertTokenizer** (BERT-derived models): Used if `vocab.txt` exists for WordPiece tokenization
  - Optimal for: BERT, DistilBERT, RoBERTa-based Sentence Transformer models
- **SentenceTransformerTokenizer**: Fallback if BertTokenizer is unavailable
  - Optimal for: Models shipped with custom tokenizer configurations

**Important**: Different tokenizers can produce different token IDs and thus different embeddings. Ensure your model conversion matches the tokenizer type selected by the library. If you're using a BERT-derived model, ensure `vocab.txt` is included during conversion.

## Running the Sample

```bash
# Run with the converted model
dotnet run ../../models/all-MiniLM-L6-v2

# Or specify a different model path
dotnet run /path/to/your/model
```

## What the Sample Demonstrates

### 1. Model Validation and Metadata

- Validates that the model directory contains all required files
- Extracts and displays model metadata (type, dimensions, vocabulary size, etc.)

### 2. Single Text Embedding

- Embeds a single sentence
- Shows token count and embedding dimensions
- Displays sample embedding values

### 3. Batch Embedding

- Efficiently embeds multiple texts in a single operation
- Demonstrates proper batch processing

### 4. Semantic Similarity

- Computes cosine similarity between query and documents
- Ranks documents by relevance to the query

## Expected Output

```text
=== Chonkie.Net - Sentence Transformer Embeddings Example ===

Validating model...
✓ Model validated successfully

Model Metadata:
  Type: bert
  Hidden Size: 384
  Embedding Dimension: 384
  Max Position Embeddings: 512
  Vocabulary Size: 30522
  Pooling Mode: Mean

Loading model...
✓ Model loaded successfully
  Name: sentence-transformers
  Dimension: 384
  Max Sequence Length: 256

=== Example 1: Single Text Embedding ===
Text: "The quick brown fox jumps over the lazy dog."
Token count: 11
Embedding dimension: 384
First 5 values: [0.0234, -0.0456, 0.0123, -0.0789, 0.0345]
Norm: 1.0000

=== Example 2: Batch Embedding ===
Embedding 4 texts...

Text 1: "Machine learning is a subset of artificial intelligence."
  Embedding dimension: 384
  First 3 values: [0.0156, -0.0234, 0.0567]

[...more output...]

=== Example 3: Semantic Similarity ===
Query: "What is artificial intelligence?"

Documents:
  1. "Artificial intelligence is the simulation of human intelligence by machines."
  2. "Machine learning is a method of data analysis that automates analytical model building."
  3. "Python is a high-level programming language."
  4. "The weather today is sunny and warm."

Similarity scores:
  1. 0.7854 - Artificial intelligence is the simulation of human intelligence by machines.
  2. 0.6123 - Machine learning is a method of data analysis that automates analytical model building.
  3. 0.2341 - Python is a high-level programming language.
  4. 0.1234 - The weather today is sunny and warm.

=== Example 4: Finding Most Similar Documents ===
Documents ranked by relevance:
  1. [0.7854] Artificial intelligence is the simulation of human intelligence by machines.
  2. [0.6123] Machine learning is a method of data analysis that automates analytical model building.
  3. [0.2341] Python is a high-level programming language.
  4. [0.1234] The weather today is sunny and warm.

=== Example completed successfully! ===
```

## Troubleshooting

### Model Not Found

If you see "Invalid model directory", ensure you've converted the model:

```shell
uv run ../../scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2
```

### Missing Dependencies

If you encounter errors about missing packages:

```shell
dotnet restore
dotnet build
```

### ONNX Runtime Errors

Ensure the ONNX model was properly converted. Try reconverting:

```shell
uv run ../../scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2 --force
```

## Advanced Usage

### Pooling Modes

Pooling converts token-level embeddings to sentence-level embeddings. Choose the mode that best matches your model's training strategy:

- **Mean** (default): Averages all token embeddings, weighted by attention mask
  - **Use when**: Building general-purpose semantic search
  - **Pros**: Captures information from all tokens, balanced performance
  - **Cons**: May dilute rare but important tokens

- **Cls**: Uses the [CLS] token embedding (first token)
  - **Use when**: Working with BERT-style models optimized for classification
  - **Pros**: Single dedicated token trained for sentence representation
  - **Cons**: Requires model trained with CLS pooling

- **Max**: Maximum pooling across all dimensions for each token
  - **Use when**: Capturing the most important semantic features
  - **Pros**: Preserves strongest signals, good for keyword matching
  - **Cons**: Can miss subtle patterns in lower-magnitude values

- **LastToken**: Uses the final token embedding
  - **Use when**: Working with sequential models that accumulate information at the end
  - **Pros**: Captures accumulated context, suited for left-to-right models
  - **Cons**: Ignores earlier tokens, less common for sentence embeddings

Example with different pooling modes:

```csharp
using Chonkie.Embeddings.SentenceTransformers;

// Mean pooling (general purpose)
var meanEmb = new SentenceTransformerEmbeddings(
    modelPath: "./models/all-MiniLM-L6-v2",
    poolingMode: PoolingMode.Mean
);

// CLS pooling (BERT-optimized)
var clsEmb = new SentenceTransformerEmbeddings(
    modelPath: "./models/all-MiniLM-L6-v2",
    poolingMode: PoolingMode.Cls
);

// Max pooling (feature extraction)
var maxEmb = new SentenceTransformerEmbeddings(
    modelPath: "./models/all-MiniLM-L6-v2",
    poolingMode: PoolingMode.Max
);

// Compare embeddings for the same text
var text = "Machine learning is transforming AI.";
var emb1 = await meanEmb.EmbedAsync(text);
var emb2 = await clsEmb.EmbedAsync(text);
var emb3 = await maxEmb.EmbedAsync(text);

Console.WriteLine($"Mean similarity: {meanEmb.Similarity(emb1, emb2):F4}");
Console.WriteLine($"Max similarity: {meanEmb.Similarity(emb1, emb3):F4}");
```

### Model Caching

`ModelManager` includes utilities for managing cached models locally:

```csharp
using Chonkie.Embeddings.SentenceTransformers;

var modelName = "sentence-transformers/all-MiniLM-L6-v2";

// Get the local cache path for a model
var localPath = ModelManager.GetModelPath(modelName);
Console.WriteLine($"Cache location: {localPath}");

// Check if a model is already cached and valid
if (ModelManager.IsModelCached(modelName))
{
    Console.WriteLine("Model is cached and ready to use");
    var embeddings = new SentenceTransformerEmbeddings(localPath);
}
else
{
    Console.WriteLine("Model not cached. Convert it first using:");
    Console.WriteLine($"  python scripts/convert_model.py {modelName}");
}

// Get or create the cache directory
var cacheDir = ModelManager.EnsureCacheDirectory();
Console.WriteLine($"Default cache: {cacheDir}");
```

By default, models are cached in `~/.cache/chonkie/models`. You can customize this behavior by providing a different cache directory to the methods above.

### Customization and Configuration

```csharp
using Chonkie.Embeddings.SentenceTransformers;

var embeddings = new SentenceTransformerEmbeddings(
    modelPath: "./models/all-MiniLM-L6-v2",
    poolingMode: PoolingMode.Mean,              // Pooling strategy
    normalize: true,                             // L2 normalization (default)
    maxLength: 256,                              // Max sequence length override
    sessionOptions: null                         // Custom ONNX SessionOptions
);

// Count tokens before embedding to check length
var text = "Very long text that might exceed the maximum length...";
var tokenCount = embeddings.CountTokens(text);
Console.WriteLine($"Token count: {tokenCount} / {embeddings.MaxSequenceLength}");

if (tokenCount > embeddings.MaxSequenceLength)
{
    Console.WriteLine("Warning: Text will be truncated");
}

// Embed with proper error handling
try
{
    var embedding = await embeddings.EmbedAsync(text);
    Console.WriteLine($"Successfully embedded text: {embedding.Length} dimensions");
}
catch (Exception ex)
{
    Console.WriteLine($"Error embedding text: {ex.Message}");
}
```

### Error Handling

Common errors and how to handle them:

```csharp
using Chonkie.Embeddings.SentenceTransformers;

try
{
    var embeddings = new SentenceTransformerEmbeddings(modelPath);
}
catch (ArgumentNullException)
{
    // Model path was null or empty
    Console.WriteLine("Error: Model path cannot be null or empty");
}
catch (DirectoryNotFoundException ex)
{
    // Model directory doesn't exist
    Console.WriteLine($"Error: Model directory not found at {ex.Message}");
    Console.WriteLine("Ensure you've converted the model using:");
    Console.WriteLine("  python ../../scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2");
}
catch (FileNotFoundException ex)
{
    // Required model files are missing
    Console.WriteLine($"Error: Missing required file - {ex.Message}");
    Console.WriteLine("Required files: model.onnx, config.json, vocab.txt");
}
catch (Exception ex)
{
    // Other ONNX Runtime errors
    Console.WriteLine($"Error loading model: {ex.Message}");
    Console.WriteLine("Ensure the ONNX model was properly converted and is compatible");
}
```

### Batch Processing

`EmbedBatchAsync()` processes multiple texts and returns embeddings for all of them:

```csharp
using Chonkie.Embeddings.SentenceTransformers;

var embeddings = new SentenceTransformerEmbeddings("./models/all-MiniLM-L6-v2");

var texts = new[]
{
    "Machine learning is a subset of artificial intelligence.",
    "Deep learning uses neural networks with multiple layers.",
    "Natural language processing enables computers to understand text.",
    "Computer vision allows machines to interpret visual information."
};

// Batch embedding (currently processes sequentially)
var batchEmbeddings = await embeddings.EmbedBatchAsync(texts);
Console.WriteLine($"Generated {batchEmbeddings.Count} embeddings");

// For high-throughput scenarios with many texts, consider parallel processing:
var parallelEmbeddings = await Task.WhenAll(
    texts.Select(t => embeddings.EmbedAsync(t))
);

// Token counting for batch
var tokenCounts = embeddings.CountTokensBatch(texts);
foreach (var (text, count) in texts.Zip(tokenCounts))
{
    Console.WriteLine($"{text}: {count} tokens");
}
```

## Performance Tips

1. **Reuse the embeddings object** for multiple operations:

   ```csharp
   using var embeddings = new SentenceTransformerEmbeddings(modelPath);
   // Initialize once, use for many operations
   var emb1 = await embeddings.EmbedAsync(text1);
   var emb2 = await embeddings.EmbedAsync(text2);
   ```

2. **Use batch processing** for multiple texts (minimizes overhead):

   ```csharp
   // Sequential batch processing
   var batchEmbeddings = await embeddings.EmbedBatchAsync(texts);

   // For high throughput, use parallel processing
   var parallelEmbeddings = await Task.WhenAll(
       texts.Select(t => embeddings.EmbedAsync(t))
   );
   ```

3. **Reduce max sequence length** for faster inference on short texts:

   ```csharp
   // Default is often 512 tokens, reduce if your texts are shorter
   var embeddings = new SentenceTransformerEmbeddings(
       modelPath,
       maxLength: 128  // Faster for short texts
   );
   ```

4. **Choose the right model** for your use case balancing speed vs quality:
   - **Fastest**: `all-MiniLM-L6-v2` (384 dims, ~100ms per text)
   - **Balanced**: `all-MiniLM-L12-v2` (384 dims, ~150ms per text)
   - **High quality**: `all-mpnet-base-v2` (768 dims, ~300ms per text)
   - **Large**: `all-mpnet-large-v2` (768 dims, high quality, slowest)

5. **Use appropriate pooling mode** for your use case:
   - Mean pooling: General semantic search (recommended)
   - Max pooling: Fast feature extraction, good for recall
   - CLS pooling: Models explicitly trained with it
   - LastToken pooling: Special use cases only

6. **Disable normalization if not needed**:

   ```csharp
   // L2 normalization adds slight overhead but ensures unit-length vectors
   // Skip if you're just using embeddings for ranking (not storing)
   var embeddings = new SentenceTransformerEmbeddings(
       modelPath,
       normalize: false  // Skip L2 normalization
   );
   ```

7. **Pre-check token counts** to avoid processing oversized texts:

   ```csharp
   var tokenCount = embeddings.CountTokens(text);
   if (tokenCount > embeddings.MaxSequenceLength)
   {
       // Truncate or skip
       continue;
   }
   ```
