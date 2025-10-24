# ONNX Sentence Transformer Model Conversion Guide

This guide explains how to convert Sentence Transformer models from HuggingFace to ONNX format for use with Chonkie.Net.

## Prerequisites

### Python Environment

Install the required Python packages:

```bash
pip install optimum[onnxruntime] transformers sentencepiece protobuf
```

### .NET Requirements

The Chonkie.Embeddings library already includes the necessary packages:
- Microsoft.ML.OnnxRuntime (v1.20.1)
- Microsoft.ML.Tokenizers (v0.22.0-preview)

## Quick Start

### Method 1: Using the Conversion Script (Recommended)

We provide a Python script to simplify the conversion process:

```bash
# Convert a model
python scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2

# Convert to a specific directory
python scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2 ./my-models/minilm

# List popular models
python scripts/convert_model.py --list
```

### Method 2: Manual Conversion with Optimum CLI

```bash
# Install optimum CLI
pip install optimum[onnxruntime]

# Convert a model
optimum-cli export onnx \
  --model sentence-transformers/all-MiniLM-L6-v2 \
  ./models/all-MiniLM-L6-v2

# The output directory will contain:
# - model.onnx (the ONNX model)
# - config.json (model configuration)
# - tokenizer.json (tokenizer configuration)
# - vocab.txt (vocabulary)
# - special_tokens_map.json (special tokens)
```

### Method 3: Programmatic Conversion (Python)

```python
from optimum.onnxruntime import ORTModelForFeatureExtraction
from transformers import AutoTokenizer

model_name = "sentence-transformers/all-MiniLM-L6-v2"
output_dir = "./models/all-MiniLM-L6-v2"

# Export model to ONNX
model = ORTModelForFeatureExtraction.from_pretrained(
    model_name,
    export=True
)
model.save_pretrained(output_dir)

# Save tokenizer
tokenizer = AutoTokenizer.from_pretrained(model_name)
tokenizer.save_pretrained(output_dir)

print(f"Model converted and saved to {output_dir}")
```

## Recommended Models

### General Purpose

| Model | Embedding Dim | Max Seq Length | Size | Description |
|-------|---------------|----------------|------|-------------|
| `sentence-transformers/all-MiniLM-L6-v2` | 384 | 256 | ~90 MB | Fast and lightweight, good for most tasks |
| `sentence-transformers/all-mpnet-base-v2` | 768 | 384 | ~420 MB | Higher quality, better performance |
| `sentence-transformers/all-MiniLM-L12-v2` | 384 | 256 | ~120 MB | Balanced speed and quality |

### Multilingual

| Model | Embedding Dim | Languages | Size | Description |
|-------|---------------|-----------|------|-------------|
| `sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2` | 384 | 50+ | ~470 MB | Good for multilingual applications |
| `sentence-transformers/paraphrase-multilingual-mpnet-base-v2` | 768 | 50+ | ~1.1 GB | High quality multilingual embeddings |

### Specialized

| Model | Embedding Dim | Use Case | Size |
|-------|---------------|----------|------|
| `sentence-transformers/msmarco-distilbert-base-v4` | 768 | Information retrieval | ~250 MB |
| `sentence-transformers/stsb-roberta-large` | 1024 | Semantic similarity | ~1.3 GB |

## Using Converted Models in Chonkie.Net

### Basic Usage

```csharp
using Chonkie.Embeddings.SentenceTransformers;

// Load a converted model
var embeddings = new SentenceTransformerEmbeddings(
    modelPath: "./models/all-MiniLM-L6-v2"
);

// Embed a single text
var embedding = await embeddings.EmbedAsync("This is a test sentence.");
Console.WriteLine($"Embedding dimension: {embedding.Length}");

// Embed multiple texts (batch processing)
var texts = new[]
{
    "First sentence",
    "Second sentence",
    "Third sentence"
};
var batchEmbeddings = await embeddings.EmbedBatchAsync(texts);
Console.WriteLine($"Generated {batchEmbeddings.Count} embeddings");
```

### Advanced Configuration

```csharp
using Chonkie.Embeddings.SentenceTransformers;

// Custom pooling mode and normalization
var embeddings = new SentenceTransformerEmbeddings(
    modelPath: "./models/all-MiniLM-L6-v2",
    poolingMode: PoolingMode.Mean,  // Mean, Cls, Max, or LastToken
    normalize: true,                 // Apply L2 normalization
    maxLength: 512                   // Override max sequence length
);

// Check model information
Console.WriteLine($"Model: {embeddings.Name}");
Console.WriteLine($"Dimension: {embeddings.Dimension}");
Console.WriteLine($"Max sequence length: {embeddings.MaxSequenceLength}");

// Count tokens
var tokenCount = embeddings.CountTokens("This is a test sentence.");
Console.WriteLine($"Token count: {tokenCount}");
```

### Using with AutoEmbeddings

```csharp
using Chonkie.Embeddings;

// AutoEmbeddings can automatically detect Sentence Transformer models
var embeddings = AutoEmbeddings.Create(
    provider: "sentence-transformers",
    modelPath: "./models/all-MiniLM-L6-v2"
);

var embedding = await embeddings.EmbedAsync("Test text");
```

### Error Handling and Validation

```csharp
using Chonkie.Embeddings.SentenceTransformers;

// Validate model before loading
var modelPath = "./models/all-MiniLM-L6-v2";
if (!ModelManager.ValidateModel(modelPath))
{
    Console.WriteLine("Invalid model directory!");
    return;
}

// Get model metadata
var metadata = ModelManager.GetModelMetadata(modelPath);
Console.WriteLine($"Model metadata: {metadata}");
Console.WriteLine($"  Type: {metadata.ModelType}");
Console.WriteLine($"  Hidden size: {metadata.HiddenSize}");
Console.WriteLine($"  Embedding dim: {metadata.EmbeddingDimension}");
Console.WriteLine($"  Max length: {metadata.MaxPositionEmbeddings}");
Console.WriteLine($"  Pooling mode: {metadata.PoolingMode}");

// Load model with error handling
try
{
    var embeddings = new SentenceTransformerEmbeddings(modelPath);
    var result = await embeddings.EmbedAsync("Test");
    Console.WriteLine($"Embedding generated successfully: {result.Length} dimensions");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"Model files not found: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Invalid model configuration: {ex.Message}");
}
```

## Troubleshooting

### Model Files Missing

**Problem:** `FileNotFoundException: model.onnx not found`

**Solution:** Ensure all required files are present:
```bash
ls ./models/all-MiniLM-L6-v2/
# Should contain:
# - model.onnx
# - config.json
# - vocab.txt
# - tokenizer_config.json
# - special_tokens_map.json
```

### Incorrect Output Dimensions

**Problem:** Embeddings have wrong dimensions

**Solution:** Check the pooling configuration:
```bash
# Check if 1_Pooling/config.json exists
cat ./models/all-MiniLM-L6-v2/1_Pooling/config.json
```

If missing, create it manually:
```json
{
  "word_embedding_dimension": 384,
  "pooling_mode_cls_token": false,
  "pooling_mode_mean_tokens": true,
  "pooling_mode_max_tokens": false,
  "pooling_mode_mean_sqrt_len_tokens": false
}
```

### Tokenization Issues

**Problem:** Tokenizer errors or incorrect token IDs

**Solution:** The library uses fallback tokenization if `vocab.txt` is missing. For best results:
1. Ensure `vocab.txt` exists in the model directory
2. Check that `tokenizer_config.json` is present
3. Verify special tokens are defined in `special_tokens_map.json`

### Performance Issues

**Problem:** Slow inference or high memory usage

**Solutions:**
1. Use batch processing for multiple texts:
   ```csharp
   var embeddings = await embeddings.EmbedBatchAsync(texts);
   ```

2. Use a smaller model (e.g., MiniLM instead of mpnet):
   ```bash
   python scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2
   ```

3. Reduce max sequence length:
   ```csharp
   var embeddings = new SentenceTransformerEmbeddings(
       modelPath: "./models/model",
       maxLength: 128  // Reduce from default 512
   );
   ```

## Model Conversion Best Practices

### 1. Choose the Right Model

- **For speed:** Use MiniLM models (L6 or L12)
- **For quality:** Use mpnet or RoBERTa models
- **For multilingual:** Use paraphrase-multilingual models
- **For specific domains:** Check Sentence Transformers documentation for specialized models

### 2. Verify Model After Conversion

```bash
# Check file sizes
ls -lh ./models/all-MiniLM-L6-v2/

# Verify ONNX model
python -c "
from optimum.onnxruntime import ORTModelForFeatureExtraction
model = ORTModelForFeatureExtraction.from_pretrained('./models/all-MiniLM-L6-v2')
print('Model loaded successfully')
"
```

### 3. Test with Python First

Before using in .NET, test the converted model with Python:

```python
from sentence_transformers import SentenceTransformer

model = SentenceTransformer('./models/all-MiniLM-L6-v2')
embeddings = model.encode(['Test sentence'])
print(f"Embedding shape: {embeddings.shape}")
print(f"Sample values: {embeddings[0][:5]}")
```

### 4. Compare Outputs

Compare .NET outputs with Python to ensure correctness:

```python
# Python
from sentence_transformers import SentenceTransformer
model = SentenceTransformer('./models/all-MiniLM-L6-v2')
embedding = model.encode(['This is a test.'])[0]
print(f"Python: {embedding[:5]}")
```

```csharp
// C#
var embeddings = new SentenceTransformerEmbeddings("./models/all-MiniLM-L6-v2");
var embedding = await embeddings.EmbedAsync("This is a test.");
Console.WriteLine($"C#: [{string.Join(", ", embedding.Take(5))}]");
```

The values should be very close (cosine similarity > 0.99).

## Advanced Topics

### Custom Pooling Strategies

The library supports multiple pooling modes:

- **Mean Pooling** (default): Average all token embeddings weighted by attention mask
- **CLS Pooling**: Use the [CLS] token embedding
- **Max Pooling**: Max over all token embeddings
- **Last Token Pooling**: Use the last non-padding token

```csharp
// Specify pooling mode
var embeddings = new SentenceTransformerEmbeddings(
    modelPath: "./models/model",
    poolingMode: PoolingMode.Mean
);
```

### L2 Normalization

Sentence Transformers typically apply L2 normalization to embeddings:

```csharp
// Normalization is enabled by default
var embeddings = new SentenceTransformerEmbeddings(
    modelPath: "./models/model",
    normalize: true  // Default
);

// Disable normalization if needed
var unnormalizedEmbeddings = new SentenceTransformerEmbeddings(
    modelPath: "./models/model",
    normalize: false
);
```

### Model Caching

Models can be cached for reuse:

```csharp
// Get default cache directory
var cacheDir = ModelManager.DefaultCacheDirectory;
Console.WriteLine($"Cache directory: {cacheDir}");

// Check if model is cached
var isCached = ModelManager.IsModelCached("sentence-transformers/all-MiniLM-L6-v2");
Console.WriteLine($"Model cached: {isCached}");

// Get cached model path
var modelPath = ModelManager.GetModelPath("sentence-transformers/all-MiniLM-L6-v2");
```

## Resources

- **Sentence Transformers Documentation:** https://www.sbert.net/
- **HuggingFace Models:** https://huggingface.co/sentence-transformers
- **Optimum Documentation:** https://huggingface.co/docs/optimum/
- **ONNX Runtime Documentation:** https://onnxruntime.ai/
- **Microsoft.ML.Tokenizers:** https://github.com/dotnet/machinelearning/tree/main/src/Microsoft.ML.Tokenizers

## Contributing

Found an issue or want to add support for more model types? Please contribute:

1. Fork the repository
2. Create a feature branch
3. Submit a pull request

See CONTRIBUTING.md for more details.
