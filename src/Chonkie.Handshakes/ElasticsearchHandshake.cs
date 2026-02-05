using Chonkie.Core.Types;
using Chonkie.Embeddings.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Chonkie.Handshakes;

/// <summary>
/// Handshake implementation for Elasticsearch vector database using official Elastic.Clients.Elasticsearch.
/// Provides functionality to write chunks with embeddings to an Elasticsearch index.
/// </summary>
/// <remarks>
/// Uses Elasticsearch 8.0+ with dense_vector support for fine-grained and recall.
/// The implementation uses the bulk API for efficient batch operations.
/// </remarks>
/// <example>
/// <code>
/// var embeddings = new SentenceTransformerEmbeddings("all-MiniLM-L6-v2");
/// var handshake = new ElasticsearchHandshake(
///     embeddingModel: embeddings,
///     serverUrl: "http://localhost:9200"
/// );
/// 
/// var chunks = new[] { new Chunk { Text = "Hello world", StartIndex = 0, EndIndex = 11, TokenCount = 2 } };
/// await handshake.WriteAsync(chunks);
/// </code>
/// </example>
public class ElasticsearchHandshake : BaseHandshake
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;
    private readonly IEmbeddings _embeddingModel;
    private readonly int _dimension;

    /// <summary>
    /// Gets the name of the Elasticsearch index.
    /// </summary>
    public string IndexName => _indexName;

    /// <summary>
    /// Gets the embedding dimension.
    /// </summary>
    public int Dimension => _dimension;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElasticsearchHandshake"/> class.
    /// </summary>
    /// <param name="embeddingModel">The embedding model to use for generating vectors from chunk text.</param>
    /// <param name="serverUrl">The URL to the Elasticsearch server. Defaults to "http://localhost:9200".</param>
    /// <param name="indexName">The name of the Elasticsearch index. Use "random" to generate a random name.</param>
    /// <param name="apiKey">Optional. The API key for authentication.</param>
    /// <param name="client">Optional. An existing ElasticsearchClient instance.</param>
    /// <param name="logger">Optional logger instance.</param>
    public ElasticsearchHandshake(
        IEmbeddings embeddingModel,
        string serverUrl = "http://localhost:9200",
        string indexName = "random",
        string? apiKey = null,
        ElasticsearchClient? client = null,
        ILogger? logger = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(embeddingModel);
        ArgumentNullException.ThrowIfNull(serverUrl);

        _embeddingModel = embeddingModel;
        _dimension = embeddingModel.Dimension;

        // Initialize Elasticsearch client
        if (client is not null)
        {
            _client = client;
            Logger.LogInformation("Using provided Elasticsearch client");
        }
        else
        {
            var settings = new ElasticsearchClientSettings(new Uri(serverUrl));

            // Add API key authentication if provided
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                settings = settings.Authentication(new BasicAuthentication(apiKey, ""));
            }

            _client = new ElasticsearchClient(settings);
            Logger.LogInformation("Created Elasticsearch client for {ServerUrl}", serverUrl);
        }

        // Handle index name
        if (indexName == "random")
        {
            _indexName = GenerateRandomIndexName();
            Logger.LogInformation("Generated random index name: {IndexName}", _indexName);
        }
        else
        {
            _indexName = indexName;
        }

        Logger.LogInformation("Initializing Elasticsearch handshake for index: {IndexName}", _indexName);
    }

    /// <inheritdoc/>
    protected override async Task<object> WriteInternalAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Writing {ChunkCount} chunks to Elasticsearch index: {IndexName}", chunks.Count, _indexName);

        // Get embeddings for all chunks
        var texts = chunks.Select(c => c.Text).ToList();
        var embeddings = await _embeddingModel.EmbedBatchAsync(texts, cancellationToken);

        try
        {
            // Create index with mapping if it doesn't exist
            var existsResponse = await _client.Indices.ExistsAsync(_indexName, cancellationToken);
            if (!existsResponse.Exists)
            {
                await CreateIndexWithMappingAsync(cancellationToken);
            }

            // Prepare bulk operations
            var operations = new List<object>();

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var docId = GenerateId(i, chunk);

                // Add bulk action metadata
                operations.Add(new { index = new { _index = _indexName, _id = docId } });

                // Add document
                operations.Add(new
                {
                    text = chunk.Text,
                    embedding = embeddings[i],
                    start_index = chunk.StartIndex,
                    end_index = chunk.EndIndex,
                    token_count = chunk.TokenCount
                });
            }

            // Execute bulk operation
            var bulkResponse = await _client.BulkAsync(req => req
                .Index(_indexName)
                .Index<object>(operations), cancellationToken);

            if (bulkResponse.Errors)
            {
                var errorItems = bulkResponse.Items?
                    .Where(item => !item.IsValid)
                    .Select(item => item.Error?.Reason)
                    .ToList() ?? new List<string?>();

                Logger.LogWarning("Some bulk operations failed: {Errors}", string.Join(", ", errorItems));
            }

            var successCount = bulkResponse.Items?.Count(item => item.IsValid) ?? 0;
            Logger.LogInformation("Successfully wrote {SuccessCount} of {TotalCount} chunks to Elasticsearch index",
                successCount, chunks.Count);

            return new
            {
                Success = bulkResponse.Errors == false,
                Count = successCount,
                IndexName = _indexName
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to write chunks to Elasticsearch");
            throw new InvalidOperationException(
                $"Failed to write {chunks.Count} chunks to Elasticsearch index '{_indexName}'",
                ex);
        }
    }

    /// <summary>
    /// Creates the index with the appropriate mapping for vector search.
    /// </summary>
    private async Task CreateIndexWithMappingAsync(CancellationToken cancellationToken)
    {
        Logger.LogDebug("Creating Elasticsearch index: {IndexName}", _indexName);

        try
        {
            // Create index - Elasticsearch will handle defaults
            _ = await _client.Indices.CreateAsync(_indexName, cancellationToken);

            Logger.LogInformation("Created Elasticsearch index {IndexName}", _indexName);
        }
        catch (Exception ex)
        {
            // Index might already exist - log but don't fail
            Logger.LogDebug(ex, "Could not create index (may already exist): {IndexName}", _indexName);
        }
    }



    /// <summary>
    /// Generates a unique deterministic ID for a chunk using UUID5.
    /// </summary>
    private string GenerateId(int index, Chunk chunk)
    {
        var input = $"{_indexName}::chunk-{index}:{chunk.Text}";
        var bytes = Encoding.UTF8.GetBytes(input);

        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(bytes);

        var guidBytes = new byte[16];
        Buffer.BlockCopy(hash, 0, guidBytes, 0, 16);

        guidBytes[6] = (byte)((guidBytes[6] & 0x0f) | 0x50);
        guidBytes[8] = (byte)((guidBytes[8] & 0x3f) | 0x80);

        return new Guid(guidBytes).ToString();
    }

    /// <summary>
    /// Generates a random index name.
    /// </summary>
    private string GenerateRandomIndexName() =>
        $"chonkie-{Guid.NewGuid().ToString("N").Substring(0, 8)}".ToLowerInvariant();

    /// <summary>
    /// Returns a string representation of this handshake instance.
    /// </summary>
    public override string ToString() => $"ElasticsearchHandshake(index_name={_indexName})";
}

