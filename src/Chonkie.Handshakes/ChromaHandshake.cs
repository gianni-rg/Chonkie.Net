using Chonkie.Core.Types;
using Chonkie.Embeddings.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Chonkie.Handshakes;

/// <summary>
/// Handshake implementation for Chroma vector database via REST API.
/// Provides functionality to write chunks with embeddings to a Chroma collection.
/// </summary>
/// <remarks>
/// Communicates with Chroma server via HTTP REST API. Chroma server must be running
/// and accessible at the specified URL.
/// </remarks>
/// <example>
/// <code>
/// var embeddings = new SentenceTransformerEmbeddings("all-MiniLM-L6-v2");
/// var handshake = new ChromaHandshake(
///     serverUrl: "http://localhost:8000",
///     collectionName: "my-chunks",
///     embeddingModel: embeddings
/// );
/// 
/// var chunks = new[] { new Chunk { Text = "Hello world", StartIndex = 0, EndIndex = 11, TokenCount = 2 } };
/// await handshake.WriteAsync(chunks);
/// </code>
/// </example>
public class ChromaHandshake : BaseHandshake
{
    private const string ChromaServerUrlEnvironmentVariable = "CHONKIE_CHROMA_URL";
    private readonly HttpClient _httpClient;
    private readonly string _serverUrl;
    private readonly string _collectionName;
    private readonly IEmbeddings _embeddingModel;

    /// <summary>
    /// Gets the name of the Chroma collection.
    /// </summary>
    public string CollectionName => _collectionName;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromaHandshake"/> class.
    /// </summary>
    /// <param name="collectionName">The name of the collection. Use "random" to generate a random collection name.</param>
    /// <param name="embeddingModel">The embedding model to use for generating vectors from chunk text.</param>
    /// <param name="serverUrl">The URL to the Chroma server. If null, uses CHONKIE_CHROMA_URL.</param>
    /// <param name="httpClient">Optional. An existing HttpClient instance. If not provided, a new one is created.</param>
    /// <param name="logger">Optional logger instance.</param>
    public ChromaHandshake(
        string collectionName,
        IEmbeddings embeddingModel,
        string? serverUrl = null,
        HttpClient? httpClient = null,
        ILogger? logger = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(collectionName);
        ArgumentNullException.ThrowIfNull(embeddingModel);

        _embeddingModel = embeddingModel;
        var resolvedServerUrl = ResolveServerUrl(serverUrl);
        _serverUrl = resolvedServerUrl.TrimEnd('/');
        _httpClient = httpClient ?? new HttpClient();

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

        Logger.LogInformation("Initializing Chroma handshake for collection: {CollectionName} at {ServerUrl}",
            _collectionName, _serverUrl);
    }

    /// <inheritdoc/>
    protected override async Task<object> WriteInternalAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Writing {ChunkCount} chunks to Chroma collection: {CollectionName}", chunks.Count, _collectionName);

        // Get embeddings for all chunks
        var texts = chunks.Select(c => c.Text).ToList();
        var embeddings = await _embeddingModel.EmbedBatchAsync(texts, cancellationToken);

        // Prepare upsert request
        var ids = new List<string>(chunks.Count);
        var documents = new List<string>(chunks.Count);
        var metadatas = new List<Dictionary<string, object>>(chunks.Count);
        var vectors = new List<IEnumerable<float>>(chunks.Count);

        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            ids.Add(GenerateId(i, chunk));
            documents.Add(chunk.Text);
            vectors.Add(embeddings[i]);

            metadatas.Add(new Dictionary<string, object>
            {
                ["start_index"] = chunk.StartIndex,
                ["end_index"] = chunk.EndIndex,
                ["token_count"] = chunk.TokenCount
            });
        }

        try
        {
            var upsertRequest = new
            {
                ids,
                documents,
                metadatas,
                embeddings = vectors
            };

            var json = JsonSerializer.Serialize(upsertRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{_serverUrl}/api/v1/collections/{Uri.EscapeDataString(_collectionName)}/upsert";
            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Chroma server returned status {response.StatusCode}: {errorContent}");
            }

            Logger.LogInformation("Successfully wrote {ChunkCount} chunks to Chroma collection", chunks.Count);

            return new
            {
                Success = true,
                Count = chunks.Count,
                CollectionName = _collectionName
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to write chunks to Chroma");
            throw new InvalidOperationException(
                $"Failed to write {chunks.Count} chunks to Chroma collection '{_collectionName}'",
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
    /// Searches for similar chunks in the Chroma collection using vector similarity.
    /// Returns results ranked by cosine similarity to the query text.
    /// </summary>
    /// <param name="query">The query text to search for. Will be embedded using the configured embedding model.</param>
    /// <param name="limit">Maximum number of results to return. Default is 5. Must be positive.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of search results as dictionaries containing documents, embeddings, metadatas, and distances.</returns>
    /// <remarks>
    /// This method:
    /// 1. Embeds the query text using the configured IEmbeddings provider
    /// 2. Sends the query embedding to the Chroma collection
    /// 3. Returns chunks ranked by cosine similarity (lower distance = higher similarity)
    /// 
    /// Results include all metadata stored during WriteAsync (start_index, end_index, token_count).
    /// The "distances" field contains cosine distance (0 = identical, 1 = orthogonal, 2 = opposite).
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when Chroma server returns an error or connection fails.</exception>
    /// <example>
    /// <code>
    /// var handshake = new ChromaHandshake("my-collection", embeddings);
    /// var results = await handshake.SearchAsync("Find documentation about API", limit: 5);
    /// 
    /// foreach (var result in results)
    /// {
    ///     var document = (result["documents"] as List&lt;string&gt;)?[0];
    ///     var distance = ((List&lt;float&gt;?)result["distances"])?[0];
    ///     Console.WriteLine($"Document: {document}, Similarity: {1 - distance:F3}");
    /// }
    /// </code>
    /// </example>
    public async Task<IReadOnlyList<Dictionary<string, object?>>> SearchAsync(
        string query,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        Logger.LogDebug("Searching Chroma collection: {CollectionName} with limit={Limit}", _collectionName, limit);

        try
        {
            // Get the embedding for the query
            var queryEmbedding = await _embeddingModel.EmbedAsync(query, cancellationToken);

            // Prepare the query request
            var queryRequest = new
            {
                query_embeddings = new[] { queryEmbedding },
                n_results = limit,
                include = new[] { "embeddings", "metadatas", "documents", "distances" }
            };

            var json = JsonSerializer.Serialize(queryRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{_serverUrl}/api/v1/collections/{Uri.EscapeDataString(_collectionName)}/query";
            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Chroma server returned status {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var results = ParseChromaResults(responseContent);

            Logger.LogInformation("Search complete: found {ResultCount} matching chunks", results.Count);

            return results;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to search Chroma collection");
            throw new InvalidOperationException($"Failed to search Chroma collection '{_collectionName}'", ex);
        }
    }

    /// <summary>
    /// Generates a random collection name.
    /// </summary>
    protected static string GenerateRandomCollectionName() =>
        $"collection_{Guid.NewGuid().ToString("N").AsSpan(0, 24)}".ToString();

    /// <summary>
    /// Returns a string representation of this handshake instance.
    /// </summary>
    public override string ToString() => $"ChromaHandshake(collection_name={_collectionName})";

    private static string ResolveServerUrl(string? serverUrl)
    {
        if (!string.IsNullOrWhiteSpace(serverUrl))
        {
            return serverUrl;
        }

        var environmentValue = Environment.GetEnvironmentVariable(ChromaServerUrlEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            return environmentValue;
        }

        throw new InvalidOperationException(
            $"Chroma server URL not provided. Set {ChromaServerUrlEnvironmentVariable} or pass serverUrl.");
    }

    private static List<Dictionary<string, object?>> ParseChromaResults(string responseContent)
    {
        using var jsonDoc = JsonDocument.Parse(responseContent);
        var root = jsonDoc.RootElement;

        if (!root.TryGetProperty("ids", out var idsElement) || idsElement.ValueKind != JsonValueKind.Array)
        {
            return new List<Dictionary<string, object?>>();
        }

        var idsList = idsElement.EnumerateArray().ToList();
        var distancesList = root.GetProperty("distances").EnumerateArray().ToList();
        var documentsList = root.GetProperty("documents").EnumerateArray().ToList();
        var metadatasList = root.GetProperty("metadatas").EnumerateArray().ToList();

        var results = new List<Dictionary<string, object?>>();

        for (int i = 0; i < idsList.Count; i++)
        {
            if (idsList[i].ValueKind != JsonValueKind.Array || idsList[i].GetArrayLength() == 0)
            {
                continue;
            }

            var idArray = idsList[i].EnumerateArray().ToList();
            var distanceArray = distancesList[i].EnumerateArray().ToList();
            var documentArray = documentsList[i].EnumerateArray().ToList();
            var metadataArray = metadatasList[i].EnumerateArray().ToList();

            AddResultsFromGroup(results, idArray, distanceArray, documentArray, metadataArray);
        }

        return results;
    }

    private static void AddResultsFromGroup(
        List<Dictionary<string, object?>> results,
        List<JsonElement> idArray,
        List<JsonElement> distanceArray,
        List<JsonElement> documentArray,
        List<JsonElement> metadataArray)
    {
        for (int j = 0; j < idArray.Count; j++)
        {
            var result = new Dictionary<string, object?>
            {
                ["id"] = idArray[j].GetString(),
                ["text"] = documentArray[j].GetString(),
                ["similarity"] = 1.0 - distanceArray[j].GetDouble()
            };

            if (metadataArray[j].ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in metadataArray[j].EnumerateObject())
                {
                    result[prop.Name] = prop.Value.Deserialize<object>();
                }
            }

            results.Add(result);
        }
    }
}

