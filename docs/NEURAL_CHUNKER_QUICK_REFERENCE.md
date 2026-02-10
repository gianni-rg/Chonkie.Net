# NeuralChunker ONNX - Quick Reference

## 5-Minute Setup

### 1. Convert Model (One-time, ~5-10 minutes)

```bash
# Requires: Python 3.10+, pip install torch transformers optimum[onnxruntime]
python scripts/convert_neural_models.py --all
# Creates: ./models/distilbert/, ./models/modernbert-base/, ./models/modernbert-large/
```

### 2. Use in C#

```csharp
using Chonkie.Chunkers;
using Chonkie.Tokenizers;

var tokenizer = new CharacterTokenizer();
var chunker = new NeuralChunker(tokenizer, "./models/distilbert");

var chunks = chunker.Chunk("Your long text...");
foreach (var chunk in chunks)
{
    Console.WriteLine($"{chunk.TokenCount} tokens: {chunk.Text}");
}
```

## Common Patterns

### Check if ONNX is Active

```csharp
if (chunker.UseOnnx)
{
    Console.WriteLine("Neural ONNX mode");
}
else
{
    Console.WriteLine("Fallback recursive mode");
}
```

### Enable ONNX After Creation

```csharp
var chunker = new NeuralChunker(tokenizer);
if (!chunker.InitializeOnnxModel("./models/distilbert"))
{
    Console.WriteLine("Failed to load model");
}
```

### With Logging

```csharp
using Microsoft.Extensions.Logging;

ILogger<NeuralChunker> logger = ...;
var chunker = new NeuralChunker(tokenizer, modelPath, logger: logger);
```

## Model Comparison

| Use Case | Model | Why |
|----------|-------|-----|
| **Real-time, resource-constrained** | DistilBERT | Fastest, smallest |
| **Balanced (recommended)** | ModernBERT Base | 2x faster than Large, better than DistilBERT |
| **Maximum accuracy** | ModernBERT Large | Best results, slower |

## Troubleshooting

| Problem | Solution |
|---------|----------|
| `DirectoryNotFoundException` | Run conversion script, check path |
| Falls back to recursive | Check model directory exists and contains model.onnx |
| Slow inference | Use DistilBERT or enable GPU (ONNX Runtime detects automatically) |
| High memory usage | Use DistilBERT or quantized model |

## File Locations

```
models/
├── distilbert/
│   ├── model.onnx          ← Used by C#
│   └── tokenizer.json      ← Used by C#
├── modernbert-base/
│   ├── model.onnx
│   └── tokenizer.json
└── modernbert-large/
    ├── model.onnx
    └── tokenizer.json
```

## Parameters

```csharp
new NeuralChunker(
    tokenizer,              // Any ITokenizer
    modelPath,              // Path to model dir
    chunkSize: 2048,        // Max tokens per chunk
    minCharactersPerChunk: 10, // Min chunk size
    logger: null            // Optional ILogger
)
```

## Environment Variables

```bash
# Optional: Specify device
export CUDA_VISIBLE_DEVICES=0  # Use GPU 0
```

## API Reference

### Constructor Options

```csharp
// Fallback mode
new NeuralChunker(ITokenizer tokenizer, int chunkSize = 2048)

// ONNX mode
new NeuralChunker(ITokenizer tokenizer, string modelPath, int chunkSize = 2048, ...)

// Initialize later
public bool InitializeOnnxModel(string modelPath)
```

### Main Method

```csharp
public override IReadOnlyList<Chunk> Chunk(string text)
```

### Properties

```csharp
public int ChunkSize { get; }        // Configured chunk size
public bool UseOnnx { get; }         // Is ONNX active?
```

### Static Properties

```csharp
public static readonly string[] SupportedModels = [
    "mirth/chonky_distilbert_base_uncased_1",
    "mirth/chonky_modernbert_base_1",
    "mirth/chonky_modernbert_large_1"
];
```

## Example: Full End-to-End

```csharp
using Chonkie.Chunkers;
using Chonkie.Tokenizers;
using Microsoft.Extensions.Logging;

// Setup logging
ILoggerFactory loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Debug));
var logger = loggerFactory.CreateLogger<NeuralChunker>();

// Create chunker with ONNX
var tokenizer = new CharacterTokenizer();
var chunker = new NeuralChunker(
    tokenizer,
    modelPath: "./models/distilbert",
    chunkSize: 2048,
    minCharactersPerChunk: 10,
    logger: logger
);

// Verify mode
Console.WriteLine($"Using ONNX: {chunker.UseOnnx}");  // true
Console.WriteLine(chunker);  // NeuralChunker(chunk_size=2048, mode=onnx, min_chars=10)

// Chunk text
string longText = File.ReadAllText("document.txt");
var chunks = chunker.Chunk(longText);

// Process results
foreach (var chunk in chunks)
{
    Console.WriteLine($"Chunk: {chunk.TokenCount} tokens, {chunk.Text.Length} chars");
    Console.WriteLine($"  Text: {chunk.Text.Substring(0, Math.Min(50, chunk.Text.Length))}...");
    Console.WriteLine();
}
```

## Model Conversion Examples

```bash
# Convert single model
python scripts/convert_neural_models.py \
    --model mirth/chonky_distilbert_base_uncased_1 \
    --output ./models/chonky_distilbert

# Convert all defaults
python scripts/convert_neural_models.py --all

# List available models
python scripts/convert_neural_models.py --list-models

# Manual export (no Optimum)
python scripts/convert_neural_models.py \
    --model mirth/chonky_modernbert_base_1 \
    --no-optimum \
    --output ./models/modernbert
```

## Performance Tips

1. **Batch if possible**: Convert multiple documents in one session
2. **Use appropriate model**: DistilBERT for speed, Large for accuracy
3. **Enable GPU**: Set CUDA_VISIBLE_DEVICES for automatic acceleration
4. **Adjust chunk size**: Larger values = fewer, bigger chunks
5. **Monitor memory**: Use Task Manager to track memory during inference

## Key Points

✓ **Backward compatible** - Falls back to RecursiveChunker automatically  
✓ **Fast** - 2-10x faster than transformers library  
✓ **Small** - ONNX models are lightweight and portable  
✓ **Flexible** - Works with any ITokenizer implementation  
✓ **Production-ready** - Error handling, logging, validation  

## Additional Resources

- Full guide: `docs/NEURAL_CHUNKER_ONNX_GUIDE.md`
- Implementation: `docs/NEURAL_CHUNKER_ONNX_IMPLEMENTATION.md`
- Example models: Convert with `scripts/convert_neural_models.py`
- Python Chonkie: https://github.com/chonkie-inc/chonkie
- ONNX Runtime: https://onnxruntime.ai/
