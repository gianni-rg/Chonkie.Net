using Chonkie.Core.Types;
using Chonkie.Embeddings.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chonkie.Handshakes;

/// <summary>
/// Handshake implementation for Milvus vector database via REST API.
/// Provides functionality to write chunks with embeddings to a Milvus collection.
/// </summary>
/// <remarks>
/// Communicates with Milvus server via HTTP REST API. Milvus server must be running
/// and accessible at the specified URL.
/// </remarks>
/// <example>
/// <code>
/// var embeddings = new SentenceTransformerEmbeddings("all-MiniLM-L6-v2");
/// var handshake = new MilvusHandshake(
///     embeddingModel: embeddings,
///     serverUrl: "http://localhost:19530"
/// );
/// 
/// var chunks = new[] { new Chunk { Text = "Hello world", StartIndex = 0, EndIndex = 11, TokenCount = 2 } };
/// await handshake.WriteAsync(chunks);
/// </code>
/// </example>
public class MilvusHandshake : BaseHandshake
{
    private readonly HttpClient _httpClient;
    private readonly string _serverUrl;
    private readonly string _collectionName;
    private readonly IEmbeddings _embeddingModel;
    private readonly int _dimension;

    /// <summary>
    /// Gets the name of the Milvus collection.
    /// </summary>
    public string CollectionName => _collectionName;

    /// <summary>
    /// Gets the embedding dimension.
    /// </summary>
    public int Dimension => _dimension;

    /// <summary>
    /// Initializes a new instance of the <see cref="MilvusHandshake"/> class.
    /// </summary>
    /// <param name="embeddingModel">The embedding model to use for generating vectors from chunk text.</param>
    /// <param name="serverUrl">The URL to the Milvus server. Defaults to "http://localhost:19530".</param>
    /// <param name="collectionName">The collection name. Use "random" to generate a random name.</param>
    /// <param name="httpClient">Optional. An existing HttpClient instance. If not provided, a new one is created.</param>
    /// <param name="logger">Optional logger instance.</param>
    public MilvusHandshake(
        IEmbeddings embeddingModel,
        string serverUrl = "http://localhost:19530",
        string collectionName = "random",
        HttpClient? httpClient = null,
        ILogger? logger = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(embeddingModel);
        ArgumentNullException.ThrowIfNull(serverUrl);

        _embeddingModel = embeddingModel;
        _dimension = embeddingModel.Dimension;
        _serverUrl = serverUrl.TrimEnd('/');
        _httpClient = httpClient ?? new HttpClient();

        // Handle collection name
        if (collectionName == "random")
        {
            _collectionName = GenerateRandomCollectionName();
            Logger.LogInformation("Generated random collection name: {CollectionName}", _collectionName);
        }
        else
        {
            _collectionName = collectionName;
        }

        Logger.LogInformation("Initializing Milvus handshake for collection: {CollectionName} at {ServerUrl}",
            _collectionName, _serverUrl);
    }

    /// <inheritdoc/>
    protected override async Task<object> WriteInternalAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Writing {ChunkCount} chunks to Milvus collection: {CollectionName}", chunks.Count, _collectionName);

        // Get embeddings for all chunks
        var texts = chunks.Select(c => c.Text).ToList();
        var embeddings = await _embeddingModel.EmbedBatchAsync(texts, cancellationToken);

        // Prepare data in columnar format for Milvus
        var textList = chunks.Select(c => c.Text).ToList();
        var startIndices = chunks.Select(c => (long)c.StartIndex).ToList();
        var endIndices = chunks.Select(c => (long)c.EndIndex).ToList();
        var tokenCounts = chunks.Select(c => (long)c.TokenCount).ToList();

        try
        {
            // Insert data using columnar format
            var insertRequest = new
            {
                collection_name = _collectionName,
                records = new object[]
                {
                    new { field_name = "text", field_values = textList },
                    new { field_name = "start_index", field_values = startIndices },
                    new { field_name = "end_index", field_values = endIndices },
                    new { field_name = "token_count", field_values = tokenCounts },
                    new { field_name = "embedding", field_values = embeddings }
                }
            };

            var json = JsonSerializer.Serialize(insertRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{_serverUrl}/v1/insert";
            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Milvus server returned status {response.StatusCode}: {errorContent}");
            }

            // Flush to ensure data is searchable
            var flushRequest = new { collection_names = new[] { _collectionName } };
            var flushJson = JsonSerializer.Serialize(flushRequest);
            var flushContent = new StringContent(flushJson, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync($"{_serverUrl}/v1/flush", flushContent, cancellationToken);

            Logger.LogInformation("Successfully wrote {ChunkCount} chunks to Milvus collection", chunks.Count);

            return new
            {
                Success = true,
                Count = chunks.Count,
                CollectionName = _collectionName
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to write chunks to Milvus");
            throw new InvalidOperationException(
                $"Failed to write {chunks.Count} chunks to Milvus collection '{_collectionName}'",
                ex);
        }
    }

    /// <summary>
    /// Generates a unique deterministic ID for a chunk using UUID5.
    /// </summary>
    private string GenerateId(int index, Chunk chunk)
    {
        var input = $"{_collectionName}::chunk-{index}:{chunk.Text}";
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
    /// Searches for similar chunks in the Milvus collection using vector similarity.
    /// </summary>
    /// <param name="query">The query text to search for.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of search results with metadata and similarity scores.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public async Task<IReadOnlyList<Dictionary<string, object?>>> SearchAsync(
        string query,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        Logger.LogDebug("Searching Milvus collection: {CollectionName} with limit={Limit}", _collectionName, limit);

        try
        {
            // Get the embedding for the query
            var queryEmbedding = await _embeddingModel.EmbedAsync(query, cancellationToken);

            // Prepare the search request for Milvus REST API
            var searchRequest = new
            {
                collection_name = _collectionName,
                search_params = new
                {
                    metric_type = "L2"
                },
                anns_field = "embedding",
                limit = limit,
                output_fields = new[] { "text", "start_index", "end_index", "token_count" },
                vectors = new[] { queryEmbedding }
            };

            var json = JsonSerializer.Serialize(searchRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{_serverUrl}/v1/search";
            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Milvus server returned status {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            using var jsonDoc = JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;

            var results = new List<Dictionary<string, object?>>();

            if (root.TryGetProperty("results", out var resultsElement) && resultsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var resultItem in resultsElement.EnumerateArray())
                {
                    if (resultItem.TryGetProperty("result", out var resultData) && resultData.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var hit in resultData.EnumerateArray())
                        {
                            var result = new Dictionary<string, object?>
                            {
                                ["id"] = hit.GetProperty("id").GetInt64().ToString()
                            };

                            // Add fields from the document
                            if (hit.TryGetProperty("entity", out var entity) && entity.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var prop in entity.EnumerateObject())
                                {
                                    if (prop.Name == "text")
                                    {
                                        result["text"] = prop.Value.GetString();
                                    }
                                    else if (prop.Name == "start_index")
                                    {
                                        result["start_index"] = prop.Value.GetInt64();
                                    }
                                    else if (prop.Name == "end_index")
                                    {
                                        result["end_index"] = prop.Value.GetInt64();
                                    }
                                    else if (prop.Name == "token_count")
                                    {
                                        result["token_count"] = prop.Value.GetInt64();
                                    }
                                }
                            }

                            // Add distance (convert to similarity if needed)
                            if (hit.TryGetProperty("distance", out var distanceElement))
                            {
                                result["similarity"] = 1.0 / (1.0 + distanceElement.GetDouble());
                            }

                            results.Add(result);
                        }
                    }
                }
            }

            Logger.LogInformation("Search complete: found {ResultCount} matching chunks", results.Count);

            return results;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to search Milvus collection");
            throw new InvalidOperationException($"Failed to search Milvus collection '{_collectionName}'", ex);
        }
    }

    /// <summary>
    /// Generates a random collection name using underscore separator for Milvus compatibility.
    /// </summary>
    private string GenerateRandomCollectionName() =>
        $"collection_{Guid.NewGuid().ToString("N").Substring(0, 24)}";

    /// <summary>
    /// Returns a string representation of this handshake instance.
    /// </summary>
    public override string ToString() => $"MilvusHandshake(collection_name={_collectionName})";
}
