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

using Chonkie.Core.Types;
using Chonkie.Embeddings.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

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
    private const string MilvusServerUrlEnvironmentVariable = "CHONKIE_MILVUS_URL";
    private const string TextFieldName = "text";
    private const string StartIndexFieldName = "start_index";
    private const string EndIndexFieldName = "end_index";
    private const string TokenCountFieldName = "token_count";
    private const string EmbeddingFieldName = "embedding";
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
    /// <param name="serverUrl">The URL to the Milvus server. If null, uses CHONKIE_MILVUS_URL.</param>
    /// <param name="collectionName">The collection name. Use "random" to generate a random name.</param>
    /// <param name="httpClient">Optional. An existing HttpClient instance. If not provided, a new one is created.</param>
    /// <param name="logger">Optional logger instance.</param>
    public MilvusHandshake(
        IEmbeddings embeddingModel,
        string? serverUrl = null,
        string collectionName = "random",
        HttpClient? httpClient = null,
        ILogger? logger = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(embeddingModel);

        _embeddingModel = embeddingModel;
        _dimension = embeddingModel.Dimension;
        var resolvedServerUrl = ResolveServerUrl(serverUrl);
        _serverUrl = resolvedServerUrl.TrimEnd('/');
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
                    new { field_name = TextFieldName, field_values = textList },
                    new { field_name = StartIndexFieldName, field_values = startIndices },
                    new { field_name = EndIndexFieldName, field_values = endIndices },
                    new { field_name = TokenCountFieldName, field_values = tokenCounts },
                    new { field_name = EmbeddingFieldName, field_values = embeddings }
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
            var searchRequest = BuildSearchRequest(queryEmbedding, limit);

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
            var results = ParseSearchResults(responseContent);

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
    private static string GenerateRandomCollectionName() =>
        $"collection_{Guid.NewGuid().ToString("N").Substring(0, 24)}";

    /// <summary>
    /// Returns a string representation of this handshake instance.
    /// </summary>
    public override string ToString() => $"MilvusHandshake(collection_name={_collectionName})";

    private static string ResolveServerUrl(string? serverUrl)
    {
        if (!string.IsNullOrWhiteSpace(serverUrl))
        {
            return serverUrl;
        }

        var environmentValue = Environment.GetEnvironmentVariable(MilvusServerUrlEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            return environmentValue;
        }

        throw new InvalidOperationException(
            $"Milvus server URL not provided. Set {MilvusServerUrlEnvironmentVariable} or pass serverUrl.");
    }

    private object BuildSearchRequest(IEnumerable<float> queryEmbedding, int limit)
    {
        return new
        {
            collection_name = _collectionName,
            search_params = new
            {
                metric_type = "L2"
            },
            anns_field = EmbeddingFieldName,
            limit = limit,
            output_fields = new[] { TextFieldName, StartIndexFieldName, EndIndexFieldName, TokenCountFieldName },
            vectors = new[] { queryEmbedding }
        };
    }

    private static List<Dictionary<string, object?>> ParseSearchResults(string responseContent)
    {
        using var jsonDoc = JsonDocument.Parse(responseContent);
        var root = jsonDoc.RootElement;

        if (!root.TryGetProperty("results", out var resultsElement) || resultsElement.ValueKind != JsonValueKind.Array)
        {
            return new List<Dictionary<string, object?>>();
        }

        var results = new List<Dictionary<string, object?>>();

        foreach (var resultItem in resultsElement.EnumerateArray())
        {
            if (!resultItem.TryGetProperty("result", out var resultData) || resultData.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            AddResultItems(results, resultData);
        }

        return results;
    }

    private static void AddResultItems(List<Dictionary<string, object?>> results, JsonElement resultData)
    {
        foreach (var hit in resultData.EnumerateArray())
        {
            var result = new Dictionary<string, object?>
            {
                ["id"] = hit.GetProperty("id").GetInt64().ToString()
            };

            if (hit.TryGetProperty("entity", out var entity) && entity.ValueKind == JsonValueKind.Object)
            {
                AddEntityFields(result, entity);
            }

            if (hit.TryGetProperty("distance", out var distanceElement))
            {
                result["similarity"] = 1.0 / (1.0 + distanceElement.GetDouble());
            }

            results.Add(result);
        }
    }

    private static void AddEntityFields(Dictionary<string, object?> result, JsonElement entity)
    {
        foreach (var prop in entity.EnumerateObject())
        {
            if (prop.Name == TextFieldName)
            {
                result[TextFieldName] = prop.Value.GetString();
            }
            else if (prop.Name == StartIndexFieldName)
            {
                result[StartIndexFieldName] = prop.Value.GetInt64();
            }
            else if (prop.Name == EndIndexFieldName)
            {
                result[EndIndexFieldName] = prop.Value.GetInt64();
            }
            else if (prop.Name == TokenCountFieldName)
            {
                result[TokenCountFieldName] = prop.Value.GetInt64();
            }
        }
    }
}
