# Sentence Transformer Embeddings Sample

This sample demonstrates how to use ONNX-based Sentence Transformer embeddings in Chonkie.Net.

## Prerequisites

1. **Convert a model to ONNX format:**

```bash
# Install conversion tools
pip install optimum[onnxruntime] transformers sentencepiece protobuf

# Convert a model
python ../../scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2

# Or use a specific output directory
python ../../scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2 ./models/minilm
```

2. **Build the sample:**

```bash
cd samples/Chonkie.SentenceTransformers.Sample
dotnet build
```

## Running the Sample

```bash
# Run with the converted model
dotnet run ./models/all-MiniLM-L6-v2

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

```
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
```bash
python ../../scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2
```

### Missing Dependencies
If you encounter errors about missing packages:
```bash
dotnet restore
dotnet build
```

### ONNX Runtime Errors
Ensure the ONNX model was properly converted. Try reconverting:
```bash
python ../../scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2 --force
```

## Learn More

- [ONNX Model Conversion Guide](../../docs/ONNX_MODEL_CONVERSION_GUIDE.md)
- [Sentence Transformers Documentation](https://www.sbert.net/)
- [Chonkie.Embeddings README](../../src/Chonkie.Embeddings/README.md)

## Advanced Usage

You can customize the embeddings behavior:

```csharp
using Chonkie.Embeddings.SentenceTransformers;

// Custom pooling mode
var embeddings = new SentenceTransformerEmbeddings(
    modelPath: "./models/all-MiniLM-L6-v2",
    poolingMode: PoolingMode.Mean,  // or Cls, Max, LastToken
    normalize: true,                 // L2 normalization
    maxLength: 256                   // Max sequence length
);

// Count tokens before embedding
var text = "Very long text that might exceed the maximum length...";
var tokenCount = embeddings.CountTokens(text);
if (tokenCount > embeddings.MaxSequenceLength)
{
    Console.WriteLine($"Warning: Text has {tokenCount} tokens, will be truncated to {embeddings.MaxSequenceLength}");
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

## Performance Tips

1. **Use batch processing** for multiple texts:
   ```csharp
   var embeddings = await provider.EmbedBatchAsync(texts);
   ```

2. **Reduce max sequence length** if you have short texts:
   ```csharp
   var embeddings = new SentenceTransformerEmbeddings(modelPath, maxLength: 128);
   ```

3. **Choose the right model** for your use case:
   - Fast: `all-MiniLM-L6-v2` (384 dim)
   - Balanced: `all-MiniLM-L12-v2` (384 dim)
   - High quality: `all-mpnet-base-v2` (768 dim)

4. **Reuse the embeddings object**:
   ```csharp
   using var embeddings = new SentenceTransformerEmbeddings(modelPath);
   // Use for multiple operations
   ```
