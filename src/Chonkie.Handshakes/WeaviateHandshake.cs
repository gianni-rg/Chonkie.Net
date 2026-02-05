using Chonkie.Core.Types;
using Chonkie.Embeddings.Interfaces;
using Microsoft.Extensions.Logging;
using Weaviate.Client;
using Weaviate.Client.Models;
using Weaviate.Client.Typed;

namespace Chonkie.Handshakes;

/// <summary>
/// Handshake implementation for Weaviate vector database.
/// Weaviate is an open-source vector database that stores objects and vectors for semantic search.
/// </summary>
public class WeaviateHandshake : BaseHandshake
{
    private readonly WeaviateClient _client;
    private readonly IEmbeddings _embeddingModel;
    private readonly string _className;
    private readonly int _dimension;
    private TypedCollectionClient<Dictionary<string, object>>? _collection;

    /// <summary>
    /// Gets the Weaviate class (collection) name.
    /// </summary>
    public string ClassName => _className;

    /// <summary>
    /// Gets the embedding dimension.
    /// </summary>
    public int Dimension => _dimension;

    /// <summary>
    /// Lazily gets or initializes the collection.
    /// </summary>
    private TypedCollectionClient<Dictionary<string, object>> GetCollection() => _collection ??= _client.Collections.Use<Dictionary<string, object>>(_className);

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateHandshake"/> class for Weaviate Cloud.
    /// </summary>
    /// <param name="url">The Weaviate Cloud URL.</param>
    /// <param name="apiKey">The Weaviate Cloud API key.</param>
    /// <param name="className">The name of the Weaviate class (collection). Must exist before use.</param>
    /// <param name="embeddingModel">The embedding model to use for generating vectors from chunk text.</param>
    /// <param name="logger">Optional logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public static async Task<WeaviateHandshake> CreateCloudAsync(
        string url,
        string apiKey,
        string className,
        IEmbeddings embeddingModel,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(apiKey);
        ArgumentNullException.ThrowIfNull(className);
        ArgumentNullException.ThrowIfNull(embeddingModel);

        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty or whitespace.", nameof(url));
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be empty or whitespace.", nameof(apiKey));
        if (string.IsNullOrWhiteSpace(className))
            throw new ArgumentException("Class name cannot be empty or whitespace.", nameof(className));

        var client = await Connect.Cloud(url, apiKey);
        return new WeaviateHandshake(client, className, embeddingModel, logger);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateHandshake"/> class with a custom client.
    /// </summary>
    /// <param name="client">The Weaviate client instance.</param>
    /// <param name="className">The name of the Weaviate class.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <param name="logger">Optional logger instance.</param>
    public WeaviateHandshake(
        WeaviateClient client,
        string className,
        IEmbeddings embeddingModel,
        ILogger? logger = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(className);
        ArgumentNullException.ThrowIfNull(embeddingModel);

        if (string.IsNullOrWhiteSpace(className))
            throw new ArgumentException("Class name cannot be empty or whitespace.", nameof(className));

        _client = client;
        _className = className;
        _embeddingModel = embeddingModel;
        _dimension = embeddingModel.Dimension;

        Logger.LogInformation("Initialized WeaviateHandshake for class: {ClassName}", _className);
    }

    /// <inheritdoc/>
    protected override async Task<object> WriteInternalAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Writing {ChunkCount} chunks to Weaviate class:{ClassName}", chunks.Count, _className);

        // Generate embeddings for all chunks
        var texts = chunks.Select(c => c.Text).ToList();
        var embeddings = await _embeddingModel.EmbedBatchAsync(texts, cancellationToken);

        // Create batch insert requests with both properties and vectors
        var requests = new List<BatchInsertRequest>();
        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var embedding = embeddings[i];

            // Create properties dictionary
            var properties = new Dictionary<string, object>
            {
                ["text"] = chunk.Text,
                ["start_index"] = chunk.StartIndex,
                ["end_index"] = chunk.EndIndex,
                ["token_count"] = chunk.TokenCount,
                ["chunk_index"] = i
            };

            // Create vectors with embedding
            var vectors = new Vectors();
            vectors.Add("default", embedding);

            // Create request using the static factory method
            var request = BatchInsertRequest.Create(properties, uuid: null, vectors: vectors);
            requests.Add(request);
        }

        // Batch insert using InsertMany
        var collection = GetCollection();
        var insertResults = await collection.Data.InsertMany(
            requests,
            cancellationToken: cancellationToken
        );

        // Check for errors
        var errorCount = insertResults.Count(r => r.Error != null);
        if (errorCount > 0)
        {
            Logger.LogWarning("Batch insert completed with {ErrorCount} errors out of {TotalCount}", errorCount, chunks.Count);

            // Log first few errors for debugging
            foreach (var error in insertResults.Where(r => r.Error != null).Take(3))
            {
                Logger.LogError("Insert error at index {Index}: {ErrorMessage}", error.Index, error.Error?.Message);
            }
        }

        var successCount = insertResults.Count(r => r.Error == null);
        Logger.LogInformation("Successfully wrote {ChunkCount} chunks to Weaviate class: {ClassName}", successCount, _className);

        return new
        {
            Success = successCount > 0,
            Count = successCount,
            Errors = errorCount,
            ClassName = _className,
            Uuids = insertResults.Where(r => r.UUID.HasValue).Select(r => r.UUID!.Value).ToList()
        };
    }

    /// <summary>
    /// Searches for similar chunks in the class using a query text.
    /// </summary>
    /// <param name="query">The query text to search for.</param>
    /// <param name="limit">The maximum number of results to return.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Query results containing matching objects and their properties.</returns>
    public async Task<object> SearchAsync(string query, int limit = 5, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        Logger.LogDebug("Searching Weaviate class: {ClassName} with limit={Limit}", _className, limit);

        // Generate embedding for query
        var queryEmbedding = await _embeddingModel.EmbedAsync(query, cancellationToken);

        // Search in Weaviate using NearVector
        var collection = GetCollection();
        var searchResults = await collection.Query.NearVector(
            input: new NearVectorInput(queryEmbedding),
            limit: (uint)limit,
            cancellationToken: cancellationToken
        );

        Logger.LogInformation("Search complete: found {ResultCount} matching chunks", searchResults.Objects.Count);

        return new
        {
            Count = searchResults.Objects.Count,
            Objects = searchResults.Objects,
            ClassName = _className
        };
    }

    /// <summary>
    /// Returns a string representation of this handshake.
    /// </summary>
    public override string ToString()
    {
        return $"WeaviateHandshake(ClassName={_className}, Dimension={_dimension})";
    }
}
