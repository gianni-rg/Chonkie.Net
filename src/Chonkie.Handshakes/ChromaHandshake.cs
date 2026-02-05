using Chonkie.Core.Types;
using Chonkie.Embeddings.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    /// <param name="serverUrl">The URL to the Chroma server. Defaults to "http://localhost:8000".</param>
    /// <param name="httpClient">Optional. An existing HttpClient instance. If not provided, a new one is created.</param>
    /// <param name="logger">Optional logger instance.</param>
    public ChromaHandshake(
        string collectionName,
        IEmbeddings embeddingModel,
        string serverUrl = "http://localhost:8000",
        HttpClient? httpClient = null,
        ILogger? logger = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(collectionName);
        ArgumentNullException.ThrowIfNull(embeddingModel);
        ArgumentNullException.ThrowIfNull(serverUrl);

        _embeddingModel = embeddingModel;
        _serverUrl = serverUrl.TrimEnd('/');
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
    /// Generates a random collection name.
    /// </summary>
    protected string GenerateRandomCollectionName() =>
        $"collection_{Guid.NewGuid().ToString("N").AsSpan(0, 24)}".ToString();

    /// <summary>
    /// Returns a string representation of this handshake instance.
    /// </summary>
    public override string ToString() => $"ChromaHandshake(collection_name={_collectionName})";
}

