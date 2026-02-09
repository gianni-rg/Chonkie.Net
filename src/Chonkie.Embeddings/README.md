# Chonkie.Embeddings

The `Chonkie.Embeddings` library provides a unified interface for working with multiple embedding providers in .NET. It supports both cloud-based APIs and local ONNX models for generating text embeddings.

## Features

- **Unified Interface**: Single `IEmbeddings` interface for all providers
- **Multiple Providers**: OpenAI, Azure OpenAI, Cohere, Gemini, Jina AI, Voyage AI, and Sentence Transformers (ONNX)
- **Batch Processing**: Efficient batch embedding generation
- **Auto Provider Selection**: Factory pattern for automatic provider loading
- **Extensible**: Easy to add custom embedding providers

## Installation

```bash
dotnet add package Chonkie.Embeddings
```

## Quick Start

### Using OpenAI Embeddings

```csharp
using Chonkie.Embeddings.OpenAI;

var embeddings = new OpenAIEmbeddings(apiKey: "your-api-key");
var embedding = await embeddings.EmbedAsync("Hello, world!");
Console.WriteLine($"Embedding dimension: {embedding.Length}");
```

### Using Azure OpenAI Embeddings

```csharp
using Chonkie.Embeddings.Azure;

var embeddings = new AzureOpenAIEmbeddings(
    endpoint: "https://your-resource.openai.azure.com",
    apiKey: "your-api-key",
    deploymentName: "your-deployment-name"
);
var embedding = await embeddings.EmbedAsync("Hello, world!");
```

### Using Sentence Transformers (ONNX)

First, convert a model to ONNX format (see [ONNX Model Conversion Guide](../../docs/ONNX_MODEL_CONVERSION_GUIDE.md)):

```bash
# Install conversion tools
pip install optimum[onnxruntime] transformers

# Convert a model
python scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2
```

Then use it in your .NET application:

```csharp
using Chonkie.Embeddings.SentenceTransformers;

// Load the converted model
var embeddings = new SentenceTransformerEmbeddings(
    modelPath: "./models/all-MiniLM-L6-v2"
);

// Embed text
var embedding = await embeddings.EmbedAsync("Hello, world!");
Console.WriteLine($"Embedding dimension: {embedding.Length}");

// Advanced configuration
var customEmbeddings = new SentenceTransformerEmbeddings(
    modelPath: "./models/all-MiniLM-L6-v2",
    poolingMode: PoolingMode.Mean,  // Mean, Cls, Max, LastToken
    normalize: true,                 // L2 normalization (recommended)
    maxLength: 512                   // Max sequence length
);
```

### Using AutoEmbeddings Factory

```csharp
using Chonkie.Embeddings;

// Set environment variable for API key
Environment.SetEnvironmentVariable("OPENAI_API_KEY", "your-api-key");

// Get provider by name
var embeddings = AutoEmbeddings.GetProvider("openai");
var embedding = await embeddings.EmbedAsync("Hello, world!");

// List all available providers
var providers = AutoEmbeddings.ListProviders();
Console.WriteLine($"Available providers: {string.Join(", ", providers)}");
```

## Batch Processing

All embedding providers support batch processing for efficiency:

```csharp
var texts = new[] { "First text", "Second text", "Third text" };
var embeddings = await provider.EmbedBatchAsync(texts);

foreach (var embedding in embeddings)
{
    Console.WriteLine($"Embedding dimension: {embedding.Length}");
}
```

## Supported Providers

### Cloud APIs

| Provider | Class | Environment Variable | Default Model | Dimension |
|----------|-------|---------------------|---------------|-----------|
| OpenAI | `OpenAIEmbeddings` | `OPENAI_API_KEY` | text-embedding-ada-002 | 1536 |
| Azure OpenAI | `AzureOpenAIEmbeddings` | `AZURE_OPENAI_API_KEY` | (deployment-based) | 1536 |
| Cohere | `CohereEmbeddings` | `COHERE_API_KEY` | embed-english-v3.0 | 1024 |
| Gemini | `GeminiEmbeddings` | `GEMINI_API_KEY` | embedding-001 | 768 |
| Jina AI | `JinaEmbeddings` | `JINA_API_KEY` | jina-embeddings-v2-base-en | 768 |
| Voyage AI | `VoyageAIEmbeddings` | `VOYAGE_API_KEY` | voyage-2 | 1024 |

### Local Models

| Provider | Class | Requirements | Features |
|----------|-------|-------------|----------|
| Sentence Transformers | `SentenceTransformerEmbeddings` | ONNX model file | Proper tokenization, pooling strategies, batch processing, offline inference |

#### Sentence Transformers Features

- ✅ Proper BERT-style tokenization with special tokens
- ✅ Multiple pooling strategies (Mean, CLS, Max, LastToken)
- ✅ L2 normalization for cosine similarity
- ✅ True batch processing with dynamic padding
- ✅ Model validation and metadata extraction
- ✅ Efficient tensor operations with ONNX Runtime
- ✅ Compatible with HuggingFace Sentence Transformers

**Recommended Models:**
- `sentence-transformers/all-MiniLM-L6-v2` (384 dim, fast)
- `sentence-transformers/all-mpnet-base-v2` (768 dim, high quality)
- `sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2` (384 dim, multilingual)

See the [ONNX Model Conversion Guide](../../docs/ONNX_MODEL_CONVERSION_GUIDE.md) for detailed instructions.

## Custom Providers

You can create custom embedding providers by implementing the `IEmbeddings` interface:

```csharp
using Chonkie.Embeddings.Interfaces;

public class MyCustomEmbeddings : IEmbeddings
{
    public string Name => "my-custom";
    public int Dimension => 512;

    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        // Your custom embedding logic
        return new float[Dimension];
    }

    public async Task<IReadOnlyList<float[]>> EmbedBatchAsync(
        IEnumerable<string> texts,
        CancellationToken cancellationToken = default)
    {
        // Your custom batch embedding logic
        var results = new List<float[]>();
        foreach (var text in texts)
        {
            results.Add(await EmbedAsync(text, cancellationToken));
        }
        return results;
    }
}

// Register with AutoEmbeddings
AutoEmbeddings.RegisterProvider("my-custom", () => new MyCustomEmbeddings());
```

## Configuration

### Using Environment Variables

The `AutoEmbeddings` factory automatically reads API keys from environment variables:

```bash
# Windows PowerShell
$env:OPENAI_API_KEY="your-key"
$env:AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com"
$env:AZURE_OPENAI_API_KEY="your-key"
$env:AZURE_OPENAI_DEPLOYMENT="your-deployment"
$env:COHERE_API_KEY="your-key"
$env:GEMINI_API_KEY="your-key"
$env:JINA_API_KEY="your-key"
$env:VOYAGE_API_KEY="your-key"
$env:CHONKIE_SENTENCE_TRANSFORMERS_MODEL_PATH="path/to/model.onnx"
```

### Using Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using Chonkie.Embeddings.Interfaces;
using Chonkie.Embeddings.OpenAI;

var services = new ServiceCollection();
services.AddSingleton<IEmbeddings>(sp =>
    new OpenAIEmbeddings(apiKey: "your-api-key"));

var serviceProvider = services.BuildServiceProvider();
var embeddings = serviceProvider.GetRequiredService<IEmbeddings>();
```

## Performance Considerations

### Batch Processing

Always use `EmbedBatchAsync` when processing multiple texts for better performance:

```csharp
// ❌ Slower - multiple API calls
foreach (var text in texts)
{
    var embedding = await embeddings.EmbedAsync(text);
}

// ✅ Faster - single API call
var embeddings = await provider.EmbedBatchAsync(texts);
```

### Local vs Cloud

- **Cloud APIs**: Best for production with varying workloads, no local resources required
- **Local ONNX**: Best for high-throughput scenarios, offline processing, or cost optimization

### Cancellation Support

All methods support cancellation tokens for responsive applications:

```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(10));

try
{
    var embedding = await embeddings.EmbedAsync("text", cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation was cancelled");
}
```

## Error Handling

All providers may throw exceptions for:
- Invalid API keys
- Network errors
- Rate limiting
- Model not found (ONNX)

```csharp
try
{
    var embedding = await embeddings.EmbedAsync("text");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid configuration: {ex.Message}");
}
```

## Examples

### Semantic Search

```csharp
var embeddings = new OpenAIEmbeddings(apiKey: "your-key");

// Embed query and documents
var queryEmbedding = await embeddings.EmbedAsync("What is the capital of France?");
var documents = new[] { "Paris is the capital of France", "London is the capital of the UK" };
var docEmbeddings = await embeddings.EmbedBatchAsync(documents);

// Compute cosine similarity
float CosineSimilarity(float[] a, float[] b)
{
    var dot = 0f;
    var magA = 0f;
    var magB = 0f;
    for (int i = 0; i < a.Length; i++)
    {
        dot += a[i] * b[i];
        magA += a[i] * a[i];
        magB += b[i] * b[i];
    }
    return dot / (MathF.Sqrt(magA) * MathF.Sqrt(magB));
}

for (int i = 0; i < documents.Length; i++)
{
    var similarity = CosineSimilarity(queryEmbedding, docEmbeddings[i]);
    Console.WriteLine($"{documents[i]}: {similarity:F4}");
}
```

## License

Apache-2.0

## Contributing

Contributions are welcome! Please see the [CONTRIBUTING.md](../CONTRIBUTING.md) file for details.
