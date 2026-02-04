using Chonkie.Core.Types;
using Chonkie.Embeddings.Interfaces;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Security.Cryptography;
using System.Text;

namespace Chonkie.Handshakes;

/// <summary>
/// Handshake implementation for Qdrant vector database.
/// Provides functionality to write chunks with embeddings to a Qdrant collection.
/// </summary>
/// <example>
/// <code>
/// var embeddings = new OpenAIEmbeddings("your-api-key");
/// var handshake = new QdrantHandshake(
///     url: "http://localhost:6333",
///     collectionName: "my-chunks",
///     embeddingModel: embeddings
/// );
/// 
/// var chunks = new[] { new Chunk { Text = "Hello world", StartIndex = 0, EndIndex = 11, TokenCount = 2 } };
/// await handshake.WriteAsync(chunks);
/// </code>
/// </example>
public class QdrantHandshake : BaseHandshake
{
    private readonly QdrantClient _client;
    private readonly string _collectionName;
    private readonly IEmbeddings _embeddingModel;
    private readonly uint _dimension;

    /// <summary>
    /// Gets the name of the Qdrant collection.
    /// </summary>
    public string CollectionName => _collectionName;

    /// <summary>
    /// Gets the embedding dimension.
    /// </summary>
    public uint Dimension => _dimension;

    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantHandshake"/> class with a URL.
    /// </summary>
    /// <param name="url">The URL to the Qdrant server (e.g., "http://localhost:6333").</param>
    /// <param name="collectionName">The name of the collection. Use "random" to generate a random collection name.</param>
    /// <param name="embeddingModel">The embedding model to use for generating vectors from chunk text.</param>
    /// <param name="apiKey">Optional API key for Qdrant Cloud authentication.</param>
    /// <param name="logger">Optional logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public QdrantHandshake(
        string url,
        string collectionName,
        IEmbeddings embeddingModel,
        string? apiKey = null,
        ILogger? logger = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(collectionName);
        ArgumentNullException.ThrowIfNull(embeddingModel);

        _embeddingModel = embeddingModel;
        _dimension = (uint)embeddingModel.Dimension;

        // Initialize Qdrant client
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _client = new QdrantClient(host: url, apiKey: apiKey);
        }
        else
        {
            _client = new QdrantClient(host: url);
        }

        // Handle random collection name
        if (collectionName == "random")
        {
            _collectionName = GenerateRandomCollectionName();
            Logger.LogInformation("Generated random collection name: {CollectionName}", _collectionName);
        }
        else
        {
            _collectionName = collectionName;
        }

        // Create collection if it doesn't exist
        EnsureCollectionExistsAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantHandshake"/> class with a custom client.
    /// </summary>
    /// <param name="client">The Qdrant client instance.</param>
    /// <param name="collectionName">The name of the collection.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <param name="logger">Optional logger instance.</param>
    public QdrantHandshake(
        QdrantClient client,
        string collectionName,
        IEmbeddings embeddingModel,
        ILogger? logger = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(collectionName);
        ArgumentNullException.ThrowIfNull(embeddingModel);

        _client = client;
        _collectionName = collectionName;
        _embeddingModel = embeddingModel;
        _dimension = (uint)embeddingModel.Dimension;

        // Create collection if it doesn't exist
        EnsureCollectionExistsAsync().GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    protected override async Task<object> WriteInternalAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Writing {ChunkCount} chunks to Qdrant collection: {CollectionName}", chunks.Count, _collectionName);

        // Generate embeddings for all chunks
        var texts = chunks.Select(c => c.Text).ToList();
        var embeddings = await _embeddingModel.EmbedBatchAsync(texts, cancellationToken);

        // Create points for Qdrant
        var points = new List<PointStruct>();
        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var embedding = embeddings[i];

            var pointId = GeneratePointId(i, chunk);
            var payload = new Dictionary<string, Value>
            {
                ["text"] = chunk.Text,
                ["start_index"] = chunk.StartIndex,
                ["end_index"] = chunk.EndIndex,
                ["token_count"] = chunk.TokenCount
            };

            points.Add(new PointStruct
            {
                Id = pointId,
                Vectors = embedding,
                Payload = { payload }
            });
        }

        // Upsert points to collection
        var upsertResponse = await _client.UpsertAsync(
            collectionName: _collectionName,
            points: points,
            cancellationToken: cancellationToken
        );

        Logger.LogInformation("Successfully wrote {ChunkCount} chunks to Qdrant collection: {CollectionName}", chunks.Count, _collectionName);

        return new
        {
            Success = upsertResponse.Status == UpdateStatus.Completed,
            Count = chunks.Count,
            CollectionName = _collectionName
        };
    }

    /// <summary>
    /// Searches for similar chunks in the collection using a query text.
    /// </summary>
    /// <param name="query">The query text to search for.</param>
    /// <param name="limit">The maximum number of results to return.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of scored points matching the query.</returns>
    public async Task<IReadOnlyList<ScoredPoint>> SearchAsync(string query, int limit = 5, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        Logger.LogDebug("Searching Qdrant collection: {CollectionName} with limit={Limit}", _collectionName, limit);

        // Generate embedding for query
        var queryEmbedding = await _embeddingModel.EmbedAsync(query, cancellationToken);

        // Search in Qdrant
        var searchResults = await _client.SearchAsync(
            collectionName: _collectionName,
            vector: queryEmbedding,
            limit: (ulong)limit,
            cancellationToken: cancellationToken
        );

        Logger.LogInformation("Search complete: found {ResultCount} matching chunks", searchResults.Count);

        return searchResults;
    }

    /// <summary>
    /// Ensures the collection exists, creating it if necessary.
    /// </summary>
    private async Task EnsureCollectionExistsAsync()
    {
        try
        {
            var exists = await _client.CollectionExistsAsync(_collectionName);
            if (!exists)
            {
                await _client.CreateCollectionAsync(
                    collectionName: _collectionName,
                    vectorsConfig: new VectorParams
                    {
                        Size = _dimension,
                        Distance = Distance.Cosine
                    }
                );
                Logger.LogInformation("Created new Qdrant collection: {CollectionName}", _collectionName);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to ensure collection {CollectionName} exists", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Generates a deterministic point ID from chunk index and content.
    /// Uses MD5 hash for consistent IDs across multiple writes of the same data.
    /// </summary>
    private PointId GeneratePointId(int index, Chunk chunk)
    {
        var input = $"{_collectionName}::chunk-{index}:{chunk.Text}";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        var guid = new Guid(hash);
        return new PointId { Uuid = guid.ToString() };
    }

    /// <summary>
    /// Generates a random collection name.
    /// </summary>
    private static string GenerateRandomCollectionName()
    {
        var adjectives = new[] { "happy", "clever", "swift", "bright", "calm", "eager", "gentle", "jolly", "kind", "noble" };
        var nouns = new[] { "vector", "chunk", "embed", "query", "search", "index", "store", "data", "point", "space" };
        var random = new Random();
        var adjective = adjectives[random.Next(adjectives.Length)];
        var noun = nouns[random.Next(nouns.Length)];
        var number = random.Next(1000, 9999);
        return $"chonkie-{adjective}-{noun}-{number}";
    }

    /// <summary>
    /// Returns a string representation of this handshake.
    /// </summary>
    public override string ToString()
    {
        return $"QdrantHandshake(collection_name={_collectionName}, dimension={_dimension})";
    }
}
