using Chonkie.Core.Types;
using Chonkie.Embeddings.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chonkie.Handshakes;

/// <summary>
/// Handshake implementation for Turbopuffer vector database via REST API.
/// Provides functionality to write chunks with embeddings to a Turbopuffer namespace.
/// </summary>
/// <remarks>
/// Turbopuffer is a managed vector database with Redis-like simplicity.
/// This implementation communicates with Turbopuffer via HTTP REST API.
/// Requires TURBOPUFFER_API_KEY environment variable or explicit API key parameter.
/// </remarks>
/// <example>
/// <code>
/// var embeddings = new SentenceTransformerEmbeddings("all-MiniLM-L6-v2");
/// var handshake = new TurbopufferHandshake(
///     embeddingModel: embeddings,
///     apiKey: "your-api-key"
/// );
/// 
/// var chunks = new[] { new Chunk { Text = "Hello world", StartIndex = 0, EndIndex = 11, TokenCount = 2 } };
/// await handshake.WriteAsync(chunks);
/// </code>
/// </example>
public class TurbopufferHandshake : BaseHandshake
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _namespace;
    private readonly IEmbeddings _embeddingModel;
    private const string DefaultApiUrl = "https://api.turbopuffer.com/v1";

    /// <summary>
    /// Gets the namespace name in Turbopuffer.
    /// </summary>
    public string Namespace => _namespace;

    /// <summary>
    /// Gets the API key (masked for logging).
    /// </summary>
    public string ApiKeyMasked => $"***{_apiKey.Substring(Math.Max(0, _apiKey.Length - 4))}";

    /// <summary>
    /// Initializes a new instance of the <see cref="TurbopufferHandshake"/> class.
    /// </summary>
    /// <param name="embeddingModel">The embedding model to use for generating vectors from chunk text.</param>
    /// <param name="apiKey">Optional. The Turbopuffer API key. If not provided, uses TURBOPUFFER_API_KEY environment variable.</param>
    /// <param name="namespaceName">The Turbopuffer namespace. Use "random" to generate a random name.</param>
    /// <param name="httpClient">Optional. An existing HttpClient instance. If not provided, a new one is created.</param>
    /// <param name="logger">Optional logger instance.</param>
    public TurbopufferHandshake(
        IEmbeddings embeddingModel,
        string? apiKey = null,
        string namespaceName = "random",
        HttpClient? httpClient = null,
        ILogger? logger = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(embeddingModel);

        _embeddingModel = embeddingModel;

        // Get API key from parameter or environment variable
        _apiKey = apiKey ?? Environment.GetEnvironmentVariable("TURBOPUFFER_API_KEY")
            ?? throw new InvalidOperationException(
                "Turbopuffer API key not provided. Set TURBOPUFFER_API_KEY environment variable or pass apiKey parameter.");

        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new ArgumentException("API key cannot be empty", nameof(apiKey));

        _httpClient = httpClient ?? new HttpClient();

        // Configure HTTP client with authentication header
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        // Handle namespace name
        if (namespaceName == "random")
        {
            _namespace = GenerateRandomNamespace();
            Logger.LogInformation("Generated random namespace: {Namespace}", _namespace);
        }
        else
        {
            _namespace = namespaceName;
        }

        Logger.LogInformation("Initializing Turbopuffer handshake for namespace: {Namespace}", _namespace);
    }

    /// <inheritdoc/>
    protected override async Task<object> WriteInternalAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Writing {ChunkCount} chunks to Turbopuffer namespace: {Namespace}", chunks.Count, _namespace);

        // Get embeddings for all chunks
        var texts = chunks.Select(c => c.Text).ToList();
        var embeddings = await _embeddingModel.EmbedBatchAsync(texts, cancellationToken);

        try
        {
            // Prepare vectors in columnar format for Turbopuffer
            var vectors = new List<object>();
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                vectors.Add(new
                {
                    id = GenerateId(i, chunk),
                    vector = embeddings[i],
                    attributes = new
                    {
                        text = chunk.Text,
                        start_index = chunk.StartIndex,
                        end_index = chunk.EndIndex,
                        token_count = chunk.TokenCount
                    }
                });
            }

            // Upsert vectors to Turbopuffer
            var upsertRequest = new { vectors };
            var json = JsonSerializer.Serialize(upsertRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{DefaultApiUrl}/namespaces/{Uri.EscapeDataString(_namespace)}/vectors/upsert";
            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Turbopuffer API returned status {response.StatusCode}: {errorContent}");
            }

            Logger.LogInformation("Successfully wrote {ChunkCount} chunks to Turbopuffer namespace", chunks.Count);

            return new
            {
                Success = true,
                Count = chunks.Count,
                Namespace = _namespace,
                VectorCount = vectors.Count
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to write chunks to Turbopuffer");
            throw new InvalidOperationException(
                $"Failed to write {chunks.Count} chunks to Turbopuffer namespace '{_namespace}'",
                ex);
        }
    }

    /// <summary>
    /// Generates a unique deterministic ID for a chunk using UUID5.
    /// </summary>
    private string GenerateId(int index, Chunk chunk)
    {
        var input = $"{_namespace}::chunk-{index}:{chunk.Text}";
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
    /// Searches the Turbopuffer namespace for chunks similar to the query.
    /// </summary>
    /// <param name="query">The query string to search for.</param>
    /// <param name="limit">The maximum number of results to return. Defaults to 5.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of search results with metadata and similarity scores.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public async Task<IReadOnlyList<Dictionary<string, object?>>> SearchAsync(
        string query,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        Logger.LogDebug("Searching Turbopuffer namespace: {Namespace} with limit={Limit}", _namespace, limit);

        try
        {
            // Get the embedding for the query
            var queryEmbedding = await _embeddingModel.EmbedAsync(query, cancellationToken);

            // Prepare the query request in Turbopuffer format
            var queryRequest = new
            {
                rank_by = new object[]
                {
                    "vector",
                    "ANN",
                    queryEmbedding.ToList()
                },
                top_k = limit,
                include_attributes = new[] { "text", "start_index", "end_index", "token_count" }
            };

            var json = JsonSerializer.Serialize(queryRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{DefaultApiUrl}/namespaces/{Uri.EscapeDataString(_namespace)}/vectors/query";
            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Turbopuffer API returned status {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            using var jsonDoc = JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;

            var results = new List<Dictionary<string, object?>>();

            // Parse Turbopuffer response format
            if (root.TryGetProperty("results", out var resultsElement) && resultsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var result in resultsElement.EnumerateArray())
                {
                    var resultDict = new Dictionary<string, object?>();

                    // Extract id
                    if (result.TryGetProperty("id", out var idElement))
                    {
                        resultDict["id"] = idElement.GetString();
                    }

                    // Convert distance to similarity: 1.0 - distance
                    if (result.TryGetProperty("$dist", out var distElement))
                    {
                        double distance = distElement.GetDouble();
                        resultDict["similarity"] = 1.0 - distance;
                        resultDict["distance"] = distance;
                    }

                    // Extract attributes
                    if (result.TryGetProperty("text", out var textElement))
                    {
                        resultDict["text"] = textElement.GetString();
                    }

                    if (result.TryGetProperty("start_index", out var startElement))
                    {
                        resultDict["start_index"] = startElement.GetInt32();
                    }

                    if (result.TryGetProperty("end_index", out var endElement))
                    {
                        resultDict["end_index"] = endElement.GetInt32();
                    }

                    if (result.TryGetProperty("token_count", out var tokenElement))
                    {
                        resultDict["token_count"] = tokenElement.GetInt32();
                    }

                    results.Add(resultDict);
                }
            }
            // Alternative response format with rows array
            else if (root.TryGetProperty("rows", out var rowsElement) && rowsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var row in rowsElement.EnumerateArray())
                {
                    var resultDict = new Dictionary<string, object?>();

                    // Extract id
                    if (row.TryGetProperty("id", out var idElement))
                    {
                        resultDict["id"] = idElement.GetString();
                    }

                    // Convert distance to similarity
                    if (row.TryGetProperty("$dist", out var distElement))
                    {
                        double distance = distElement.GetDouble();
                        resultDict["similarity"] = 1.0 - distance;
                        resultDict["distance"] = distance;
                    }

                    // Extract all other attributes
                    foreach (var prop in row.EnumerateObject())
                    {
                        if (prop.Name is not "id" and not "$dist")
                        {
                            if (prop.Value.ValueKind == JsonValueKind.Number)
                            {
                                resultDict[prop.Name] = prop.Value.GetInt32();
                            }
                            else if (prop.Value.ValueKind == JsonValueKind.String)
                            {
                                resultDict[prop.Name] = prop.Value.GetString();
                            }
                            else
                            {
                                resultDict[prop.Name] = prop.Value.GetRawText();
                            }
                        }
                    }

                    results.Add(resultDict);
                }
            }

            Logger.LogInformation("Found {ResultCount} similar chunks in Turbopuffer namespace", results.Count);
            return results.AsReadOnly();
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "Failed to parse Turbopuffer search response");
            throw new InvalidOperationException(
                $"Failed to parse search response from Turbopuffer for namespace '{_namespace}'",
                ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to search Turbopuffer namespace");
            throw new InvalidOperationException(
                $"Failed to search namespace '{_namespace}' in Turbopuffer",
                ex);
        }
    }

    /// <summary>
    /// Generates a random namespace name for Turbopuffer.
    /// </summary>
    private string GenerateRandomNamespace() =>
        $"ns_{Guid.NewGuid().ToString("N").Substring(0, 24)}";

    /// <summary>
    /// Returns a string representation of this handshake instance.
    /// </summary>
    public override string ToString() => $"TurbopufferHandshake(namespace={_namespace}, api_key={ApiKeyMasked})";
}

/// <summary>
/// Factory class for Turbopuffer client operations using reflection to avoid compile-time dependencies.
/// </summary>
internal static class TurbopufferClientFactory
{
    /// <summary>
    /// Creates a Turbopuffer client with the specified API key and region.
    /// </summary>
    public static dynamic CreateClient(string apiKey, string region)
    {
        // Uses reflection to call: turbopuffer.Turbopuffer(api_key=apiKey, region=region)
        var type = System.Type.GetType("turbopuffer.Turbopuffer");
        if (type is null)
            throw new InvalidOperationException("Turbopuffer library not available");

        return System.Activator.CreateInstance(type, apiKey, region)
            ?? throw new InvalidOperationException("Failed to create Turbopuffer client");
    }

    /// <summary>
    /// Gets or creates a namespace in the Turbopuffer client.
    /// </summary>
    public static dynamic GetOrCreateNamespace(dynamic client, string namespaceName)
    {
        // Uses reflection to call: client.namespace(namespaceName)
        var method = client.GetType().GetMethod("Namespace") ?? client.GetType().GetMethod("namespace");
        if (method is null)
            throw new InvalidOperationException("Namespace method not found on Turbopuffer client");

        return method.Invoke(client, new object[] { namespaceName })
            ?? throw new InvalidOperationException("Failed to get or create namespace");
    }

    /// <summary>
    /// Gets the name of a namespace instance.
    /// </summary>
    public static string GetNamespaceName(dynamic @namespace)
    {
        try
        {
            var property = @namespace.GetType().GetProperty("Id") ?? @namespace.GetType().GetProperty("id");
            if (property is not null)
            {
                var value = property.GetValue(@namespace);
                return value?.ToString() ?? "unknown";
            }

            // Fallback to ToString
            return @namespace.ToString() ?? "unknown";
        }
        catch
        {
            return "unknown";
        }
    }

    /// <summary>
    /// Upserts data into a Turbopuffer namespace using columnar format.
    /// </summary>
    public static void Upsert(
        dynamic @namespace,
        Dictionary<string, object> data,
        string distanceMetric = "cosine_distance")
    {
        // Uses reflection to call: namespace.write(upsert_columns=data, distance_metric=distanceMetric)
        var method = @namespace.GetType().GetMethod("Write") ?? @namespace.GetType().GetMethod("write");
        if (method is null)
            throw new InvalidOperationException("Write method not found on Turbopuffer namespace");

        var args = new object[] { data, distanceMetric };
        var paramNames = new[] { "upsert_columns", "distance_metric" };

        method.Invoke(@namespace, args);
    }
}
