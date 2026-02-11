using System.Security.Cryptography;
using System.Text;
using Chonkie.Chefs;
using Chonkie.Chunkers;
using Chonkie.Core.Types;
using Chonkie.Embeddings.Azure;
using Chonkie.Embeddings.Interfaces;
using Chonkie.Embeddings.OpenAI;
using Chonkie.Embeddings.SentenceTransformers;
using Chonkie.Tokenizers;

namespace Chonkie.Sample;

/// <summary>
/// Sample application demonstrating tutorial-aligned chunking workflows.
/// </summary>
class Program
{
    private const string SampleText = "Artificial Intelligence has revolutionized many aspects of modern technology. " +
                                      "Machine learning algorithms can now process vast amounts of data to identify patterns and make predictions. " +
                                      "Natural Language Processing enables computers to understand and generate human language. " +
                                      "Deep learning models, inspired by the human brain, have achieved remarkable results in image recognition, " +
                                      "speech synthesis, and game playing. The field continues to evolve rapidly with new breakthroughs emerging regularly.";

    private const string CodeSample = """
namespace Demo;

public class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }

    public int Multiply(int a, int b)
    {
        return a * b;
    }
}
""";

    private const string MarkdownSample = """
# Shipping Manifest

## Overview
Chonkie samples demonstrate how to chunk markdown with headings and code blocks.

```csharp
public record Shipment(string Id, int Packages);
```

| Id | Packages | Destination |
|----|----------|-------------|
| A1 | 12       | Seattle     |
| B2 | 7        | Oslo        |
| C3 | 4        | Denver      |
""";

    private const string TableSample = """
| Product | Price | Stock |
|---------|-------|-------|
| Apple   | $1.50 | 100   |
| Banana  | $0.75 | 150   |
| Orange  | $1.25 | 80    |
| Grape   | $2.00 | 60    |
| Mango   | $1.80 | 40    |
""";

    /// <summary>
    /// Entry point for the tutorial-aligned sample run.
    /// </summary>
    /// <param name="args">Command-line arguments (currently unused).</param>
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Chonkie.Net Tutorial Samples ===\n");

        DemoQuickStart(SampleText);
        DemoTokenizers(SampleText);

        DemoTokenChunker(SampleText);
        DemoSentenceChunker(SampleText);
        DemoRecursiveChunker(SampleText);
        DemoFastChunker(SampleText);

        DemoCodeChunker(CodeSample);
        DemoTableChunker(TableSample);

        DemoMarkdownProcessing(MarkdownSample);
        DemoSemanticChunker(SampleText);
        DemoLateChunker(SampleText);
        DemoNeuralChunker(SampleText);
        DemoSlumberChunker(SampleText);

        DemoMarkdownDocument();
        DemoInvalidTableLogging();
        DemoHeaderRepetition();

        Console.WriteLine("\n=== Sample Application Complete ===");
    }

    /// <summary>
    /// Shows the quick-start chunking flow and core chunk metadata.
    /// </summary>
    /// <param name="text">The sample text to chunk.</param>
    private static void DemoQuickStart(string text)
    {
        Console.WriteLine("1. Quick Start - CHONK!\n");

        var tokenizer = new WordTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 40, chunkOverlap: 6);
        var chunks = chunker.Chunk(text);

        Console.WriteLine($"   Created {chunks.Count} chunk(s)");
        if (chunks.Count > 0)
        {
            var chunk = chunks[0];
            Console.WriteLine("   First chunk preview:");
            Console.WriteLine($"   {chunk.Text.Substring(0, Math.Min(80, chunk.Text.Length))}...");
            Console.WriteLine($"   Tokens: {chunk.TokenCount}, Range: [{chunk.StartIndex}, {chunk.EndIndex}]");
        }

        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Demonstrates the built-in tokenizers available in Chonkie.Net.
    /// </summary>
    /// <param name="text">The text to tokenize.</param>
    private static void DemoTokenizers(string text)
    {
        Console.WriteLine("2. Tokenizers Overview\n");

        var wordTokenizer = new WordTokenizer();
        var characterTokenizer = new CharacterTokenizer();
        var autoTokenizer = AutoTokenizer.Create("word");

        Console.WriteLine($"   Word tokens: {wordTokenizer.CountTokens(text)}");
        Console.WriteLine($"   Character tokens: {characterTokenizer.CountTokens(text)}");
        Console.WriteLine($"   AutoTokenizer (word) tokens: {autoTokenizer.CountTokens(text)}");
        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Demonstrates token-based chunking with overlap.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    private static void DemoTokenChunker(string text)
    {
        Console.WriteLine("3. TokenChunker\n");

        var chunker = new TokenChunker(new WordTokenizer(), chunkSize: 20, chunkOverlap: 5);
        var chunks = chunker.Chunk(text);

        PrintChunkPreview(chunks, 2);
        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Demonstrates sentence-aware chunking.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    private static void DemoSentenceChunker(string text)
    {
        Console.WriteLine("4. SentenceChunker\n");

        var chunker = new SentenceChunker(new WordTokenizer(), chunkSize: 50, chunkOverlap: 10);
        var chunks = chunker.Chunk(text);

        PrintChunkPreview(chunks, 2);
        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Demonstrates hierarchical recursive chunking.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    private static void DemoRecursiveChunker(string text)
    {
        Console.WriteLine("5. RecursiveChunker\n");

        var chunker = new RecursiveChunker(new WordTokenizer(), chunkSize: 30);
        var chunks = chunker.Chunk(text);

        PrintChunkPreview(chunks, 2);
        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Demonstrates high-speed character-based chunking.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    private static void DemoFastChunker(string text)
    {
        Console.WriteLine("6. FastChunker\n");

        var chunker = new FastChunker(chunkSize: 120, chunkOverlap: 20);
        var chunks = chunker.Chunk(text);

        PrintChunkPreview(chunks, 2);
        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Demonstrates code-aware chunking.
    /// </summary>
    /// <param name="code">The source code to chunk.</param>
    private static void DemoCodeChunker(string code)
    {
        Console.WriteLine("7. CodeChunker\n");

        var chunker = new CodeChunker(new WordTokenizer(), chunkSize: 40);
        var chunks = chunker.Chunk(code);

        PrintChunkPreview(chunks, 3);
        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Demonstrates table-aware chunking for markdown tables.
    /// </summary>
    /// <param name="table">The markdown table text.</param>
    private static void DemoTableChunker(string table)
    {
        Console.WriteLine("8. TableChunker\n");

        var chunker = new TableChunker(new WordTokenizer(), chunkSize: 40, repeatHeaders: false);
        var chunks = chunker.Chunk(table);

        PrintChunkPreview(chunks, 2);
        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Demonstrates markdown preprocessing with MarkdownChef and recursive chunking.
    /// </summary>
    /// <param name="markdown">The markdown text to preprocess and chunk.</param>
    private static void DemoMarkdownProcessing(string markdown)
    {
        Console.WriteLine("9. Markdown Processing (MarkdownChef + RecursiveChunker)\n");

        var chef = new MarkdownChef();
        var html = chef.Process(markdown);
        var chunker = new RecursiveChunker(new WordTokenizer(), chunkSize: 50);
        var chunks = chunker.Chunk(html);

        PrintChunkPreview(chunks, 2);
        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Demonstrates semantic chunking with embeddings.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    private static void DemoSemanticChunker(string text)
    {
        Console.WriteLine("10. SemanticChunker\n");

        using var embeddingsHandle = CreateEmbeddingsHandle("semantic");
        var chunker = new SemanticChunker(
            tokenizer: new WordTokenizer(),
            embeddingModel: embeddingsHandle.Embeddings,
            threshold: 0.75f,
            chunkSize: 60);

        var chunks = chunker.Chunk(text);
        Console.WriteLine($"   Embeddings source: {embeddingsHandle.Source}");
        PrintChunkPreview(chunks, 2);
        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Demonstrates late interaction chunking with embeddings.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    private static void DemoLateChunker(string text)
    {
        Console.WriteLine("11. LateChunker\n");

        using var embeddingsHandle = CreateEmbeddingsHandle("late");
        var chunker = new LateChunker(new WordTokenizer(), embeddingsHandle.Embeddings, chunkSize: 40);
        var chunks = chunker.Chunk(text);

        Console.WriteLine($"   Embeddings source: {embeddingsHandle.Source}");
        PrintChunkPreview(chunks, 2);
        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Demonstrates neural chunking with optional ONNX model support.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    private static void DemoNeuralChunker(string text)
    {
        Console.WriteLine("12. NeuralChunker (fallback-aware)\n");

        var modelPath = Environment.GetEnvironmentVariable("CHONKIE_NEURAL_MODEL_PATH");
        var chunker = string.IsNullOrWhiteSpace(modelPath)
            ? new NeuralChunker(new WordTokenizer(), chunkSize: 60)
            : new NeuralChunker(new WordTokenizer(), modelPath, chunkSize: 60);

        var chunks = chunker.Chunk(text);
        Console.WriteLine($"   ONNX enabled: {chunker.UseOnnx}");
        PrintChunkPreview(chunks, 2);
        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Demonstrates the LLM-guided SlumberChunker fallback behavior.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    private static void DemoSlumberChunker(string text)
    {
        Console.WriteLine("13. SlumberChunker (fallback)\n");

        var chunker = new SlumberChunker(new WordTokenizer(), chunkSize: 60);
        var chunks = chunker.Chunk(text);

        PrintChunkPreview(chunks, 2);
        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Demonstrates the MarkdownDocument type and metadata.
    /// </summary>
    private static void DemoMarkdownDocument()
    {
        Console.WriteLine("14. MarkdownDocument Type\n");

        var doc = new MarkdownDocument
        {
            Content = MarkdownSample,
            Source = "manifest.md",
            Tables =
            [
                new MarkdownTable
                {
                    Content = "| Id | Packages | Destination |\n|----|----------|-------------|\n| A1 | 12 | Seattle |",
                    StartIndex = 86,
                    EndIndex = 160
                }
            ],
            Code =
            [
                new MarkdownCode
                {
                    Content = "public record Shipment(string Id, int Packages);",
                    Language = "csharp",
                    StartIndex = 60,
                    EndIndex = 110
                }
            ]
        };

        Console.WriteLine($"   Document ID: {doc.Id}");
        Console.WriteLine($"   Source: {doc.Source}");
        Console.WriteLine($"   Tables: {doc.Tables.Count}");
        Console.WriteLine($"   Code blocks: {doc.Code.Count}");
        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Demonstrates how invalid tables are handled by TableChunker.
    /// </summary>
    private static void DemoInvalidTableLogging()
    {
        Console.WriteLine("15. Invalid Table Logging\n");

        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 100);
        var invalidTable = "| Name | Value |\n|------|-------|";

        var chunks = chunker.Chunk(invalidTable);
        Console.WriteLine($"   Result: Treated as normal text, {chunks.Count} chunk(s) created");
        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Demonstrates header repetition options in TableChunker.
    /// </summary>
    private static void DemoHeaderRepetition()
    {
        Console.WriteLine("16. Header Repetition Mode\n");

        var tokenizer = new CharacterTokenizer();

        var chunkerNoRepeat = new TableChunker(tokenizer, chunkSize: 80, repeatHeaders: false);
        var chunksNoRepeat = chunkerNoRepeat.Chunk(TableSample);

        Console.WriteLine("   A. Without Header Repetition:");
        PrintChunkPreview(chunksNoRepeat, 2);

        var chunkerRepeat = new TableChunker(tokenizer, chunkSize: 80, repeatHeaders: true);
        var chunksRepeat = chunkerRepeat.Chunk(TableSample);

        Console.WriteLine("   B. With Header Repetition:");
        PrintChunkPreview(chunksRepeat, 2);
        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Prints a short preview for a chunk list.
    /// </summary>
    /// <param name="chunks">The chunks to preview.</param>
    /// <param name="maxChunks">The maximum number of chunks to print.</param>
    private static void PrintChunkPreview(IReadOnlyList<Chunk> chunks, int maxChunks)
    {
        Console.WriteLine($"   Created {chunks.Count} chunk(s)");
        var displayCount = Math.Min(maxChunks, chunks.Count);

        for (var i = 0; i < displayCount; i++)
        {
            var preview = chunks[i].Text;
            preview = preview.Replace("\n", " ");
            if (preview.Length > 80)
            {
                preview = preview.Substring(0, 80) + "...";
            }
            Console.WriteLine($"   Chunk {i + 1}: {preview}");
        }
    }

    /// <summary>
    /// Creates an embeddings provider for samples using environment configuration.
    /// </summary>
    /// <param name="purpose">The demo purpose for diagnostics.</param>
    /// <returns>A handle that manages embeddings lifetime.</returns>
    private static EmbeddingsHandle CreateEmbeddingsHandle(string purpose)
    {
        if (IsOfflineMode())
        {
            return new EmbeddingsHandle(new DemoEmbeddings(), "demo (offline mode)");
        }

        if (TryCreateSentenceTransformerEmbeddings(out var local, out var localSource))
        {
            return local;
        }

        if (TryCreateAzureEmbeddings(out var azure, out var azureSource))
        {
            return new EmbeddingsHandle(azure, azureSource);
        }

        if (TryCreateOpenAiEmbeddings(out var openAi, out var openAiSource))
        {
            return new EmbeddingsHandle(openAi, openAiSource);
        }

        Console.WriteLine($"   No external embeddings configured for {purpose}, using demo embeddings.");
        return new EmbeddingsHandle(new DemoEmbeddings(), "demo (no provider configured)");
    }

    /// <summary>
    /// Determines whether the sample should avoid external calls.
    /// </summary>
    /// <returns>True when offline mode is enabled.</returns>
    private static bool IsOfflineMode()
    {
        var offline = Environment.GetEnvironmentVariable("CHONKIE_SAMPLE_OFFLINE");
        return string.Equals(offline, "true", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(offline, "1", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Attempts to create Azure OpenAI embeddings from environment variables.
    /// </summary>
    /// <param name="embeddings">The embeddings provider when created.</param>
    /// <param name="source">The provider source label.</param>
    /// <returns>True if a provider was created.</returns>
    private static bool TryCreateAzureEmbeddings(out IEmbeddings embeddings, out string source)
    {
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_EMBEDDINGS_DEPLOYMENT");

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(deployment))
        {
            embeddings = null!;
            source = string.Empty;
            return false;
        }

        embeddings = new AzureOpenAIEmbeddings(endpoint, apiKey, deployment);
        source = $"azure-openai:{deployment}";
        return true;
    }

    /// <summary>
    /// Attempts to create OpenAI embeddings from environment variables.
    /// </summary>
    /// <param name="embeddings">The embeddings provider when created.</param>
    /// <param name="source">The provider source label.</param>
    /// <returns>True if a provider was created.</returns>
    private static bool TryCreateOpenAiEmbeddings(out IEmbeddings embeddings, out string source)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            embeddings = null!;
            source = string.Empty;
            return false;
        }

        var model = Environment.GetEnvironmentVariable("OPENAI_EMBEDDINGS_MODEL") ?? "text-embedding-3-small";
        var dimension = Environment.GetEnvironmentVariable("OPENAI_EMBEDDINGS_DIMENSION");
        var dimensionValue = int.TryParse(dimension, out var parsed) ? parsed : 1536;

        embeddings = new OpenAIEmbeddings(apiKey, model, dimensionValue);
        source = $"openai:{model}";
        return true;
    }

    /// <summary>
    /// Attempts to create local Sentence Transformer embeddings from environment variables.
    /// </summary>
    /// <param name="handle">The embeddings handle when created.</param>
    /// <param name="source">The provider source label.</param>
    /// <returns>True if a provider was created.</returns>
    private static bool TryCreateSentenceTransformerEmbeddings(out EmbeddingsHandle handle, out string source)
    {
        var modelPath = Environment.GetEnvironmentVariable("CHONKIE_SENTENCE_TRANSFORMERS_MODEL_PATH");
        if (string.IsNullOrWhiteSpace(modelPath))
        {
            handle = null!;
            source = string.Empty;
            return false;
        }

        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        source = $"sentence-transformers:{Path.GetFileName(modelPath)}";
        handle = new EmbeddingsHandle(embeddings, source, embeddings);
        return true;
    }
}

/// <summary>
/// Wraps an embeddings provider and handles cleanup when needed.
/// </summary>
sealed class EmbeddingsHandle : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingsHandle"/> class.
    /// </summary>
    /// <param name="embeddings">The embeddings provider.</param>
    /// <param name="source">The source label for diagnostics.</param>
    /// <param name="disposable">Optional disposable resource for cleanup.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="embeddings"/> is null.</exception>
    public EmbeddingsHandle(IEmbeddings embeddings, string source, IDisposable? disposable = null)
    {
        Embeddings = embeddings ?? throw new ArgumentNullException(nameof(embeddings));
        Source = source;
        _disposable = disposable;
    }

    /// <summary>
    /// Gets the embeddings provider.
    /// </summary>
    public IEmbeddings Embeddings { get; }

    /// <summary>
    /// Gets the provider source label.
    /// </summary>
    public string Source { get; }

    private readonly IDisposable? _disposable;

    /// <summary>
    /// Disposes any underlying resources if required.
    /// </summary>
    public void Dispose()
    {
        _disposable?.Dispose();
    }
}

/// <summary>
/// A deterministic, offline embeddings provider for samples without external dependencies.
/// </summary>
sealed class DemoEmbeddings : IEmbeddings
{
    private readonly int _dimension;

    /// <summary>
    /// Initializes a new instance of the <see cref="DemoEmbeddings"/> class.
    /// </summary>
    /// <param name="dimension">The embedding vector dimension.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dimension"/> is not positive.</exception>
    public DemoEmbeddings(int dimension = 384)
    {
        if (dimension <= 0)
        {
            throw new ArgumentException("Dimension must be positive.", nameof(dimension));
        }

        _dimension = dimension;
    }

    /// <inheritdoc />
    public string Name => "demo-embeddings";

    /// <inheritdoc />
    public int Dimension => _dimension;

    /// <inheritdoc />
    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateEmbedding(text));
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        var results = texts.Select(CreateEmbedding).ToList();
        return Task.FromResult((IReadOnlyList<float[]>)results);
    }

    /// <summary>
    /// Computes cosine similarity for demo embeddings.
    /// </summary>
    /// <param name="u">The first vector.</param>
    /// <param name="v">The second vector.</param>
    /// <returns>The cosine similarity value.</returns>
    /// <exception cref="ArgumentException">Thrown when vector dimensions do not match.</exception>
    public float Similarity(float[] u, float[] v)
    {
        if (u.Length != v.Length)
        {
            throw new ArgumentException("Vectors must have the same dimension.", nameof(v));
        }

        double dot = 0;
        double normU = 0;
        double normV = 0;

        for (var i = 0; i < u.Length; i++)
        {
            dot += u[i] * v[i];
            normU += u[i] * u[i];
            normV += v[i] * v[i];
        }

        var denom = Math.Sqrt(normU) * Math.Sqrt(normV);
        return denom <= 0 ? 0 : (float)(dot / denom);
    }

    /// <summary>
    /// Creates a deterministic embedding vector from a text input.
    /// </summary>
    /// <param name="text">The text to embed.</param>
    /// <returns>A normalized embedding vector.</returns>
    private float[] CreateEmbedding(string text)
    {
        var input = Encoding.UTF8.GetBytes(text ?? string.Empty);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(input);

        var vector = new float[_dimension];
        for (var i = 0; i < _dimension; i++)
        {
            var value = hash[i % hash.Length];
            vector[i] = (value - 128) / 128f;
        }

        Normalize(vector);
        return vector;
    }

    /// <summary>
    /// Normalizes a vector in-place to unit length.
    /// </summary>
    /// <param name="vector">The vector to normalize.</param>
    private static void Normalize(float[] vector)
    {
        double sum = 0;
        foreach (var value in vector)
        {
            sum += value * value;
        }

        var norm = Math.Sqrt(sum);
        if (norm <= 0)
        {
            return;
        }

        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] = (float)(vector[i] / norm);
        }
    }
}
