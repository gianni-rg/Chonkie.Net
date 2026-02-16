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
using Pinecone;

namespace Chonkie.Handshakes;

/// <summary>
/// Handshake implementation for Pinecone vector database.
/// Provides functionality to write chunks with embeddings to a Pinecone index.
/// </summary>
/// <example>
/// <code>
/// var embeddings = new OpenAIEmbeddings("your-api-key");
/// var handshake = new PineconeHandshake(
///     apiKey: "your-pinecone-api-key",
///     indexName: "my-chunks",
///     embeddingModel: embeddings,
///     @namespace: "default"
/// );
/// 
/// var chunks = new[] { new Chunk { Text = "Hello world", StartIndex = 0, EndIndex = 11, TokenCount = 2 } };
/// await handshake.WriteAsync(chunks);
/// </code>
/// </example>
public class PineconeHandshake : BaseHandshake
{
    private readonly PineconeClient _client;
    private IndexClient? _index;
    private readonly string _indexName;
    private readonly string _namespace;
    private readonly IEmbeddings _embeddingModel;
    private readonly int _dimension;

    /// <summary>
    /// Gets the name of the Pinecone index.
    /// </summary>
    public string IndexName => _indexName;

    /// <summary>
    /// Gets the namespace for vectors.
    /// </summary>
    public string Namespace => _namespace;

    /// <summary>
    /// Gets the embedding dimension.
    /// </summary>
    public int Dimension => _dimension;

    /// <summary>
    /// Lazily gets or initializes the index client.
    /// </summary>
    private IndexClient GetIndexClient() => _index ??= _client.Index(_indexName);

    /// <summary>
    /// Initializes a new instance of the <see cref="PineconeHandshake"/> class.
    /// </summary>
    /// <param name="apiKey">The Pinecone API key.</param>
    /// <param name="indexName">The name of the Pinecone index. Must exist before use.</param>
    /// <param name="embeddingModel">The embedding model to use for generating vectors from chunk text.</param>
    /// <param name="namespace">Optional namespace for organizing vectors. Defaults to empty string (default namespace).</param>
    /// <param name="logger">Optional logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public PineconeHandshake(
        string apiKey,
        string indexName,
        IEmbeddings embeddingModel,
        string? @namespace = null,
        ILogger? logger = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
        ArgumentNullException.ThrowIfNull(indexName);
        ArgumentNullException.ThrowIfNull(embeddingModel);

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be empty or whitespace.", nameof(apiKey));
        if (string.IsNullOrWhiteSpace(indexName))
            throw new ArgumentException("Index name cannot be empty or whitespace.", nameof(indexName));

        _embeddingModel = embeddingModel;
        _dimension = embeddingModel.Dimension;
        _indexName = indexName;
        _namespace = @namespace ?? string.Empty;

        // Initialize Pinecone client (index is initialized lazily when first accessed)
        _client = new PineconeClient(apiKey);

        Logger.LogInformation("Initialized PineconeHandshake for index: {IndexName}, namespace: {Namespace}", _indexName, _namespace);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PineconeHandshake"/> class with a custom client.
    /// </summary>
    /// <param name="client">The Pinecone client instance.</param>
    /// <param name="indexName">The name of the Pinecone index.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <param name="namespace">Optional namespace for organizing vectors.</param>
    /// <param name="logger">Optional logger instance.</param>
    public PineconeHandshake(
        PineconeClient client,
        string indexName,
        IEmbeddings embeddingModel,
        string? @namespace = null,
        ILogger? logger = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(indexName);
        ArgumentNullException.ThrowIfNull(embeddingModel);

        _client = client;
        _indexName = indexName;
        _embeddingModel = embeddingModel;
        _dimension = embeddingModel.Dimension;
        _namespace = @namespace ?? string.Empty;

        Logger.LogInformation("Initialized PineconeHandshake for index: {IndexName}, namespace: {Namespace}", _indexName, _namespace);
    }

    /// <inheritdoc/>
    protected override async Task<object> WriteInternalAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Writing {ChunkCount} chunks to Pinecone index: {IndexName}, namespace: {Namespace}", chunks.Count, _indexName, _namespace);

        // Generate embeddings for all chunks
        var texts = chunks.Select(c => c.Text).ToList();
        var embeddings = await _embeddingModel.EmbedBatchAsync(texts, cancellationToken);

        // Create vectors for Pinecone
        var vectors = new List<Vector>();
        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var embedding = embeddings[i];

            // Generate deterministic ID
            var id = GenerateVectorId(i, chunk);

            // Create metadata
            var metadata = new Metadata
            {
                ["text"] = chunk.Text,
                ["start_index"] = chunk.StartIndex,
                ["end_index"] = chunk.EndIndex,
                ["token_count"] = chunk.TokenCount
            };

            vectors.Add(new Vector
            {
                Id = id,
                Values = embedding,
                Metadata = metadata
            });
        }

        // Upsert vectors to Pinecone
        var upsertRequest = new UpsertRequest
        {
            Vectors = vectors,
            Namespace = _namespace
        };

        var index = GetIndexClient();
        var upsertResponse = await index.UpsertAsync(upsertRequest);

        Logger.LogInformation("Successfully wrote {ChunkCount} chunks to Pinecone index: {IndexName}", chunks.Count, _indexName);

        return new
        {
            Success = true,
            Count = upsertResponse.UpsertedCount,
            IndexName = _indexName,
            Namespace = _namespace
        };
    }

    /// <summary>
    /// Searches for similar chunks in the index using a query text.
    /// </summary>
    /// <param name="query">The query text to search for.</param>
    /// <param name="limit">The maximum number of results to return.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Query results containing matching vectors and their metadata.</returns>
    public async Task<QueryResponse> SearchAsync(string query, int limit = 5, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        Logger.LogDebug("Searching Pinecone index: {IndexName}, namespace: {Namespace} with limit={Limit}", _indexName, _namespace, limit);

        // Generate embedding for query
        var queryEmbedding = await _embeddingModel.EmbedAsync(query, cancellationToken);

        // Search in Pinecone
        var searchRequest = new QueryRequest
        {
            Namespace = _namespace,
            Vector = queryEmbedding,
            TopK = (uint)limit,
            IncludeValues = true,
            IncludeMetadata = true
        };

        var index = GetIndexClient();
        var searchResults = await index.QueryAsync(searchRequest);

        var matchCount = searchResults.Matches?.Count() ?? 0;
        Logger.LogInformation("Search complete: found {ResultCount} matching chunks", matchCount);

        return searchResults;
    }

    /// <summary>
    /// Generates a deterministic vector ID from chunk index and content.
    /// Uses a simple hash-based approach for consistent IDs.
    /// </summary>
    private string GenerateVectorId(int index, Chunk chunk)
    {
        // Create a deterministic ID based on index name, namespace, index, and content hash
        var contentHash = chunk.Text.GetHashCode();
        return $"{_indexName}-{_namespace}-chunk-{index}-{contentHash:X8}";
    }

    /// <summary>
    /// Returns a string representation of this handshake.
    /// </summary>
    public override string ToString()
    {
        return $"PineconeHandshake(index_name={_indexName}, namespace={_namespace}, dimension={_dimension})";
    }
}
