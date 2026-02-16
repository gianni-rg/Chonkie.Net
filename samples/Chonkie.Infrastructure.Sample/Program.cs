// Copyright 2025-2026 Gianni Rosa Gallina and Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Security.Cryptography;
using System.Text;
using Chonkie.Fetchers;
using Chonkie.Chefs;
using Chonkie.Refineries;
using Chonkie.Porters;
using Chonkie.Chunkers;
using Chonkie.Tokenizers;
using Chonkie.Core.Types;
using Chonkie.Embeddings.Azure;
using Chonkie.Embeddings.Interfaces;
using Chonkie.Embeddings.OpenAI;
using Chonkie.Embeddings.SentenceTransformers;
using Chonkie.Handshakes;
using Qdrant.Client.Grpc;

namespace Chonkie.Infrastructure.Sample;

/// <summary>
/// Sample application demonstrating the Chonkie.Net infrastructure pipeline components.
/// This showcases the complete text processing workflow: Fetch → Preprocess → Chunk → Refine → Export
/// </summary>
class Program
{
    /// <summary>
    /// Entry point for infrastructure and vector database demos.
    /// </summary>
    /// <param name="args">Command-line arguments (use --vector-db to enable vector demos).</param>
    static async Task Main(string[] args)
    {
        var runVectorDb = args.Any(arg => arg.Equals("--vector-db", StringComparison.OrdinalIgnoreCase) ||
                                          arg.Equals("vector-db", StringComparison.OrdinalIgnoreCase));
        var runPipeline = !runVectorDb || args.Any(arg => arg.Equals("--all", StringComparison.OrdinalIgnoreCase));

        if (runPipeline)
        {
            await RunInfrastructurePipelineAsync();
        }

        if (runVectorDb)
        {
            await RunVectorDatabaseDemoAsync(args);
        }
    }

    /// <summary>
    /// Runs the infrastructure pipeline demo: Fetch → Preprocess → Chunk → Refine → Export.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    static async Task RunInfrastructurePipelineAsync()
    {
        Console.WriteLine("=== Chonkie.Net Infrastructure Pipeline Demo ===\n");

        // Create sample data directory and files for demonstration
        await CreateSampleDataAsync();

        Console.WriteLine("Pipeline: Fetch → Preprocess → Chunk → Refine → Export\n");
        Console.WriteLine(new string('=', 80) + "\n");

        // Step 1: FETCH - Load text files from directory
        Console.WriteLine("Step 1: FETCH - Loading text files from directory...");
        var fetcher = new FileFetcher();
        var files = await fetcher.FetchAsync("./sample_data", "*.txt");
        Console.WriteLine($"   ✓ Fetched {files.Count} file(s)\n");

        // Step 2: PREPROCESS - Clean and normalize text
        Console.WriteLine("Step 2: PREPROCESS - Cleaning and normalizing text...");
        var chef = new TextChef();
        var processedTexts = new List<string>();
        foreach (var file in files)
        {
            var processed = await chef.ProcessAsync(file.Content);
            processedTexts.Add(processed);
            Console.WriteLine($"   ✓ Processed: {Path.GetFileName(file.Path)}");
        }
        Console.WriteLine();

        // Step 3: CHUNK - Split text into manageable chunks
        Console.WriteLine("Step 3: CHUNK - Splitting text into chunks...");
        var tokenizer = new WordTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 50, chunkOverlap: 10);

        var allChunks = new List<Chunk>();
        foreach (var text in processedTexts)
        {
            var chunks = chunker.Chunk(text);
            allChunks.AddRange(chunks);
        }
        Console.WriteLine($"   ✓ Created {allChunks.Count} chunks\n");

        // Step 4: REFINE - Post-process chunks (merge overlaps)
        Console.WriteLine("Step 4: REFINE - Optimizing chunks...");
        var refinery = new OverlapRefinery(minOverlap: 8);
        var refinedChunks = await refinery.RefineAsync(allChunks);
        Console.WriteLine($"   ✓ Refined to {refinedChunks.Count} optimized chunks\n");

        // Step 5: EXPORT - Save results to JSON
        Console.WriteLine("Step 5: EXPORT - Saving results...");
        var porter = new JsonPorter();
        var outputPath = "chunked_output.json";
        await porter.ExportAsync(refinedChunks, outputPath);
        Console.WriteLine($"   ✓ Exported to: {Path.GetFullPath(outputPath)}\n");

        // Display summary
        Console.WriteLine(new string('=', 80));
        Console.WriteLine("Pipeline Summary:");
        Console.WriteLine($"  Files processed:  {files.Count}");
        Console.WriteLine($"  Initial chunks:   {allChunks.Count}");
        Console.WriteLine($"  Refined chunks:   {refinedChunks.Count}");
        Console.WriteLine($"  Output file:      {outputPath}");
        Console.WriteLine(new string('=', 80));

        // Display sample chunk
        if (refinedChunks.Count > 0)
        {
            Console.WriteLine("\nSample Chunk Preview:");
            var sample = refinedChunks[0];
            var preview = sample.Text.Length > 150 ? sample.Text.Substring(0, 150) + "..." : sample.Text;
            Console.WriteLine($"  Text: {preview}");
            Console.WriteLine($"  Tokens: {sample.TokenCount}");
            Console.WriteLine($"  Range: [{sample.StartIndex}, {sample.EndIndex}]");
        }

        Console.WriteLine("\n=== Infrastructure Pipeline Complete ===\n");
    }

    /// <summary>
    /// Runs the vector database tutorial demo when enabled.
    /// </summary>
    /// <param name="args">Command-line arguments used for configuration.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    static async Task RunVectorDatabaseDemoAsync(string[] args)
    {
        Console.WriteLine("=== Chonkie.Net Vector Database Demo ===\n");

        var provider = GetOption(args, "--provider") ??
                       Environment.GetEnvironmentVariable("CHONKIE_VECTOR_DB") ??
                       "qdrant";
        var query = GetOption(args, "--query") ?? "How does chunking help RAG?";
        var topK = GetOptionInt(args, "--topk", 3);

        using var embeddingsHandle = CreateEmbeddingsHandle();
        Console.WriteLine($"Provider: {provider}");
        Console.WriteLine($"Embeddings: {embeddingsHandle.Source}\n");

        try
        {
            var chunks = BuildSampleChunks();
            var handshake = await CreateHandshakeAsync(provider, embeddingsHandle.Embeddings);

            await handshake.WriteAsync(chunks);
            Console.WriteLine("✓ Stored sample chunks in vector database");

            await PrintSearchResultsAsync(handshake, query, topK);
            Console.WriteLine("\n=== Vector Database Demo Complete ===\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Vector DB demo failed: {ex.Message}");
            Console.WriteLine("Verify provider configuration and connectivity.");
        }
    }

    /// <summary>
    /// Creates a handshake for the requested vector database provider.
    /// </summary>
    /// <param name="provider">The provider identifier.</param>
    /// <param name="embeddings">Embeddings provider for vector generation.</param>
    /// <returns>A configured handshake instance.</returns>
    static async Task<IHandshake> CreateHandshakeAsync(string provider, IEmbeddings embeddings)
    {
        switch (provider.ToLowerInvariant())
        {
            case "qdrant":
                {
                    var url = Environment.GetEnvironmentVariable("CHONKIE_QDRANT_URL") ?? "http://localhost:6333";
                    var collection = Environment.GetEnvironmentVariable("CHONKIE_QDRANT_COLLECTION") ?? "chonkie_samples";
                    var apiKey = Environment.GetEnvironmentVariable("CHONKIE_QDRANT_API_KEY");
                    return new QdrantHandshake(url, collection, embeddings, apiKey);
                }
            case "pinecone":
                {
                    var apiKey = RequireEnv("PINECONE_API_KEY");
                    var indexName = RequireEnv("PINECONE_INDEX");
                    var @namespace = Environment.GetEnvironmentVariable("PINECONE_NAMESPACE") ?? "default";
                    return new PineconeHandshake(apiKey, indexName, embeddings, @namespace);
                }
            case "weaviate":
                {
                    var url = RequireEnv("WEAVIATE_URL");
                    var apiKey = RequireEnv("WEAVIATE_API_KEY");
                    var className = Environment.GetEnvironmentVariable("WEAVIATE_CLASS") ?? "ChonkieChunks";
                    return await WeaviateHandshake.CreateCloudAsync(url, apiKey, className, embeddings);
                }
            case "chroma":
                {
                    var collection = Environment.GetEnvironmentVariable("CHONKIE_CHROMA_COLLECTION") ?? "chonkie_samples";
                    var serverUrl = Environment.GetEnvironmentVariable("CHONKIE_CHROMA_URL");
                    return new ChromaHandshake(collection, embeddings, serverUrl);
                }
            case "mongodb":
                {
                    var uri = Environment.GetEnvironmentVariable("CHONKIE_MONGODB_URI");
                    var database = Environment.GetEnvironmentVariable("CHONKIE_MONGODB_DB") ?? "chonkie_db";
                    var collection = Environment.GetEnvironmentVariable("CHONKIE_MONGODB_COLLECTION") ?? "chonkie_collection";

                    if (!string.IsNullOrWhiteSpace(uri))
                    {
                        return new MongoDBHandshake(uri, embeddings, database, collection);
                    }

                    var host = Environment.GetEnvironmentVariable("CHONKIE_MONGODB_HOST");
                    var port = TryParseInt(Environment.GetEnvironmentVariable("CHONKIE_MONGODB_PORT"));
                    return new MongoDBHandshake(embeddings, host, port, databaseName: database, collectionName: collection);
                }
            case "pgvector":
                {
                    var port = TryParseInt(Environment.GetEnvironmentVariable("CHONKIE_PGVECTOR_PORT"));
                    var options = new PgvectorHandshakeOptions
                    {
                        ConnectionString = Environment.GetEnvironmentVariable("CHONKIE_PGVECTOR_CONNECTION_STRING"),
                        Host = Environment.GetEnvironmentVariable("CHONKIE_PGVECTOR_HOST") ?? "localhost",
                        Port = port ?? 5432,
                        Database = Environment.GetEnvironmentVariable("CHONKIE_PGVECTOR_DB") ?? "postgres",
                        Username = Environment.GetEnvironmentVariable("CHONKIE_PGVECTOR_USER") ?? "postgres",
                        Password = Environment.GetEnvironmentVariable("CHONKIE_PGVECTOR_PASSWORD") ?? "postgres",
                        CollectionName = Environment.GetEnvironmentVariable("CHONKIE_PGVECTOR_COLLECTION") ?? "chonkie_chunks",
                        VectorDimensions = TryParseInt(Environment.GetEnvironmentVariable("CHONKIE_PGVECTOR_DIMENSIONS"))
                    };

                    return new PgvectorHandshake(options, embeddings);
                }
            case "elasticsearch":
                {
                    var url = Environment.GetEnvironmentVariable("CHONKIE_ELASTICSEARCH_URL");
                    var indexName = Environment.GetEnvironmentVariable("CHONKIE_ELASTICSEARCH_INDEX") ?? "chonkie_samples";
                    var apiKey = Environment.GetEnvironmentVariable("CHONKIE_ELASTICSEARCH_API_KEY");
                    return new ElasticsearchHandshake(embeddings, url, indexName, apiKey);
                }
            case "milvus":
                {
                    var url = Environment.GetEnvironmentVariable("CHONKIE_MILVUS_URL");
                    var collection = Environment.GetEnvironmentVariable("CHONKIE_MILVUS_COLLECTION") ?? "chonkie_samples";
                    return new MilvusHandshake(embeddings, url, collection);
                }
            case "turbopuffer":
                {
                    var apiKey = RequireEnv("TURBOPUFFER_API_KEY");
                    var apiUrl = Environment.GetEnvironmentVariable("TURBOPUFFER_API_URL");
                    var namespaceName = Environment.GetEnvironmentVariable("TURBOPUFFER_NAMESPACE") ?? "chonkie_samples";
                    return new TurbopufferHandshake(embeddings, apiKey, namespaceName, apiUrl);
                }
            default:
                throw new ArgumentException($"Unknown vector DB provider: {provider}", nameof(provider));
        }
    }

    /// <summary>
    /// Builds sample chunks for vector database ingestion.
    /// </summary>
    /// <returns>The generated chunks.</returns>
    static List<Chunk> BuildSampleChunks()
    {
        var texts = new[]
        {
            "Chunking breaks long documents into manageable pieces.",
            "Embeddings capture semantic meaning for retrieval.",
            "Vector databases store embeddings for fast similarity search.",
            "RAG combines retrieval and generation for grounded answers."
        };

        var chunker = new RecursiveChunker(new WordTokenizer(), chunkSize: 40);
        return texts.SelectMany(chunker.Chunk).ToList();
    }

    /// <summary>
    /// Prints search results for the configured handshake provider.
    /// </summary>
    /// <param name="handshake">The handshake instance.</param>
    /// <param name="query">The query text.</param>
    /// <param name="topK">Number of results to return.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    static async Task PrintSearchResultsAsync(IHandshake handshake, string query, int topK)
    {
        switch (handshake)
        {
            case QdrantHandshake qdrant:
                {
                    var results = await qdrant.SearchAsync(query, topK);
                    Console.WriteLine($"✓ Qdrant returned {results.Count} result(s)");
                    break;
                }
            case PineconeHandshake pinecone:
                {
                    var results = await pinecone.SearchAsync(query, topK);
                    var matchCount = results.Matches?.Count() ?? 0;
                    Console.WriteLine($"✓ Pinecone returned {matchCount} match(es)");
                    break;
                }
            case WeaviateHandshake weaviate:
                {
                    var results = await weaviate.SearchAsync(query, topK);
                    Console.WriteLine($"✓ Weaviate search response type: {results.GetType().Name}");
                    break;
                }
            case ChromaHandshake chroma:
                {
                    var results = await chroma.SearchAsync(query, topK);
                    PrintDictionaryResults(results);
                    break;
                }
            case MongoDBHandshake mongo:
                {
                    var results = await mongo.SearchAsync(query, topK);
                    PrintDictionaryResults(results);
                    break;
                }
            case PgvectorHandshake pgvector:
                {
                    var results = await pgvector.SearchAsync(query, topK);
                    PrintDictionaryResults(results);
                    break;
                }
            case ElasticsearchHandshake elastic:
                {
                    var results = await elastic.SearchAsync(query, topK);
                    PrintDictionaryResults(results);
                    break;
                }
            case MilvusHandshake milvus:
                {
                    var results = await milvus.SearchAsync(query, topK);
                    PrintDictionaryResults(results);
                    break;
                }
            case TurbopufferHandshake turbopuffer:
                {
                    var results = await turbopuffer.SearchAsync(query, topK);
                    PrintDictionaryResults(results);
                    break;
                }
            default:
                Console.WriteLine("Search not supported for this provider.");
                break;
        }
    }

    /// <summary>
    /// Prints dictionary-based search results.
    /// </summary>
    /// <param name="results">The search results.</param>
    static void PrintDictionaryResults(IReadOnlyList<Dictionary<string, object?>> results)
    {
        Console.WriteLine($"✓ Retrieved {results.Count} result(s)");
        foreach (var result in results.Take(3))
        {
            var text = result.TryGetValue("text", out var value) ? value?.ToString() : null;
            var similarity = result.TryGetValue("similarity", out var score) ? score?.ToString() : null;
            Console.WriteLine($" - {text ?? "(no text)"} (score: {similarity ?? "n/a"})");
        }
    }

    /// <summary>
    /// Creates an embeddings provider based on environment variables or demo fallback.
    /// </summary>
    /// <returns>A handle that manages embeddings lifetime.</returns>
    static EmbeddingsHandle CreateEmbeddingsHandle()
    {
        if (TryCreateSentenceTransformerEmbeddings(out var local, out _))
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

        return new EmbeddingsHandle(new DemoEmbeddings(), "demo (no provider configured)");
    }

    /// <summary>
    /// Attempts to create Azure OpenAI embeddings from environment variables.
    /// </summary>
    /// <param name="embeddings">The embeddings provider when created.</param>
    /// <param name="source">The source label for diagnostics.</param>
    /// <returns>True if a provider was created.</returns>
    static bool TryCreateAzureEmbeddings(out IEmbeddings embeddings, out string source)
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
    /// <param name="source">The source label for diagnostics.</param>
    /// <returns>True if a provider was created.</returns>
    static bool TryCreateOpenAiEmbeddings(out IEmbeddings embeddings, out string source)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            embeddings = null!;
            source = string.Empty;
            return false;
        }

        var model = Environment.GetEnvironmentVariable("OPENAI_EMBEDDINGS_MODEL") ?? "text-embedding-3-small";
        embeddings = new OpenAIEmbeddings(apiKey, model);
        source = $"openai:{model}";
        return true;
    }

    /// <summary>
    /// Attempts to create local Sentence Transformer embeddings from environment variables.
    /// </summary>
    /// <param name="handle">The embeddings handle when created.</param>
    /// <param name="source">The source label for diagnostics.</param>
    /// <returns>True if a provider was created.</returns>
    static bool TryCreateSentenceTransformerEmbeddings(out EmbeddingsHandle handle, out string source)
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

    /// <summary>
    /// Gets an argument value from command-line input.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <param name="key">The argument key.</param>
    /// <returns>The argument value when found.</returns>
    static string? GetOption(string[] args, string key)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }

    /// <summary>
    /// Parses an integer option value or falls back to a default.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <param name="key">The argument key.</param>
    /// <param name="fallback">Fallback value when parsing fails.</param>
    /// <returns>The parsed value or fallback.</returns>
    static int GetOptionInt(string[] args, string key, int fallback)
    {
        var value = GetOption(args, key);
        return int.TryParse(value, out var parsed) ? parsed : fallback;
    }

    /// <summary>
    /// Parses an integer value from text.
    /// </summary>
    /// <param name="value">The input text.</param>
    /// <returns>The parsed value, or null if invalid.</returns>
    static int? TryParseInt(string? value)
    {
        return int.TryParse(value, out var parsed) ? parsed : null;
    }

    /// <summary>
    /// Retrieves a required environment variable value.
    /// </summary>
    /// <param name="name">The environment variable name.</param>
    /// <returns>The value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the variable is missing.</exception>
    static string RequireEnv(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Environment variable '{name}' is required for this provider.");
        }

        return value;
    }

    /// <summary>
    /// Creates sample data files for the infrastructure pipeline demo.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    static async Task CreateSampleDataAsync()
    {
        var dataDir = "sample_data";
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }

        // Create sample text files for demonstration
        var sampleTexts = new Dictionary<string, string>
        {
            ["ai_fundamentals.txt"] = @"Artificial Intelligence represents the simulation of human intelligence by machines. 
Machine learning is a subset of AI that enables systems to learn and improve from experience without being explicitly programmed. 
Deep learning uses neural networks with multiple layers to progressively extract higher-level features from raw input. 
Natural Language Processing allows computers to understand, interpret, and generate human language in a valuable way.",

            ["ml_overview.txt"] = @"Supervised learning algorithms learn from labeled training data to make predictions on unseen data. 
Unsupervised learning identifies patterns in data without pre-existing labels or categories. 
Reinforcement learning trains agents to make sequential decisions by rewarding desired behaviors. 
Feature engineering transforms raw data into features that better represent the underlying problem.",

            ["future_tech.txt"] = @"Quantum computing promises exponential speedups for certain computational problems. 
Edge AI brings artificial intelligence processing closer to data sources for faster response times. 
Explainable AI aims to make machine learning models more transparent and interpretable. 
Federated learning enables collaborative model training while keeping data decentralized and private."
        };

        foreach (var (filename, content) in sampleTexts)
        {
            var path = Path.Combine(dataDir, filename);
            if (!File.Exists(path))
            {
                await File.WriteAllTextAsync(path, content);
            }
        }
    }
}

/// <summary>
/// Wraps an embeddings provider and handles cleanup when needed.
/// </summary>
public sealed class EmbeddingsHandle : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingsHandle"/> class.
    /// </summary>
    /// <param name="embeddings">The embeddings provider.</param>
    /// <param name="source">The provider source label.</param>
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
public sealed class DemoEmbeddings : IEmbeddings
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
