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
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace Chonkie.Handshakes;

/// <summary>
/// Handshake implementation for MongoDB vector database.
/// Provides functionality to write chunks with embeddings to a MongoDB collection.
/// </summary>
/// <remarks>
/// MongoDB stores chunks as documents with embeddings. Supports flexible connection options
/// including URI, username/password, and hostname/port combinations.
/// </remarks>
/// <example>
/// <code>
/// var embeddings = new OpenAIEmbeddings("your-api-key");
/// var handshake = new MongoDBHandshake(
///     hostname: "localhost",
///     port: 27017,
///     embeddingModel: embeddings
/// );
/// 
/// var chunks = new[] { new Chunk { Text = "Hello world", StartIndex = 0, EndIndex = 11, TokenCount = 2 } };
/// await handshake.WriteAsync(chunks);
/// </code>
/// </example>
public class MongoDBHandshake : BaseHandshake
{
    private const string RandomNameToken = "random";
    private readonly IMongoCollection<BsonDocument> _collection;
    private readonly string _databaseName;
    private readonly string _collectionName;
    private readonly IEmbeddings _embeddingModel;
    private readonly int _dimension;

    /// <summary>
    /// Gets the name of the MongoDB database.
    /// </summary>
    public string DatabaseName => _databaseName;

    /// <summary>
    /// Gets the name of the MongoDB collection.
    /// </summary>
    public string CollectionName => _collectionName;

    /// <summary>
    /// Gets the embedding dimension.
    /// </summary>
    public int Dimension => _dimension;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDBHandshake"/> class with hostname and port.
    /// </summary>
    /// <param name="embeddingModel">The embedding model to use for generating vectors from chunk text.</param>
    /// <param name="hostname">The MongoDB hostname. Defaults to "localhost".</param>
    /// <param name="port">The MongoDB port. Defaults to 27017.</param>
    /// <param name="username">Optional username for MongoDB authentication.</param>
    /// <param name="password">Optional password for MongoDB authentication.</param>
    /// <param name="databaseName">The database name. Use "random" to generate a random name.</param>
    /// <param name="collectionName">The collection name. Use "random" to generate a random name.</param>
    /// <param name="logger">Optional logger instance.</param>
    public MongoDBHandshake(
        IEmbeddings embeddingModel,
        string? hostname = null,
        int? port = null,
        string? username = null,
        string? password = null,
        string databaseName = "chonkie_db",
        string collectionName = "chonkie_collection",
        ILogger? logger = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(embeddingModel);

        _embeddingModel = embeddingModel;
        _dimension = embeddingModel.Dimension;

        // Build connection string
        hostname = hostname ?? "localhost";
        var actualPort = port ?? 27017;

        var uri = BuildConnectionUri(username, password, hostname, actualPort);

        var client = new MongoClient(uri);

        // Handle database name
        if (databaseName == RandomNameToken)
        {
            _databaseName = GenerateRandomName();
            Logger.LogInformation("Created random MongoDB database: {DatabaseName}", _databaseName);
        }
        else
        {
            _databaseName = databaseName;
        }

        var database = client.GetDatabase(_databaseName);

        // Handle collection name
        if (collectionName == RandomNameToken)
        {
            _collectionName = GenerateRandomName();
            Logger.LogInformation("Created random MongoDB collection: {CollectionName}", _collectionName);
        }
        else
        {
            _collectionName = collectionName;
        }

        _collection = database.GetCollection<BsonDocument>(_collectionName);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDBHandshake"/> class with a connection URI.
    /// </summary>
    /// <param name="uri">The MongoDB connection URI.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <param name="databaseName">The database name.</param>
    /// <param name="collectionName">The collection name.</param>
    /// <param name="logger">Optional logger instance.</param>
    public MongoDBHandshake(
        string uri,
        IEmbeddings embeddingModel,
        string databaseName = "chonkie_db",
        string collectionName = "chonkie_collection",
        ILogger? logger = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(embeddingModel);

        _embeddingModel = embeddingModel;
        _dimension = embeddingModel.Dimension;

        var client = new MongoClient(uri);

        _databaseName = databaseName == RandomNameToken ? GenerateRandomName() : databaseName;
        var database = client.GetDatabase(_databaseName);

        _collectionName = collectionName == RandomNameToken ? GenerateRandomName() : collectionName;
        _collection = database.GetCollection<BsonDocument>(_collectionName);

        Logger.LogInformation("Connected to MongoDB database: {DatabaseName}, collection: {CollectionName}",
            _databaseName, _collectionName);
    }

    /// <summary>
    /// Initializes a new instance with an existing MongoDB client.
    /// </summary>
    /// <param name="client">The MongoDB client instance.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <param name="databaseName">The database name.</param>
    /// <param name="collectionName">The collection name.</param>
    /// <param name="logger">Optional logger instance.</param>
    public MongoDBHandshake(
        IMongoClient client,
        IEmbeddings embeddingModel,
        string databaseName = "chonkie_db",
        string collectionName = "chonkie_collection",
        ILogger? logger = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(embeddingModel);

        _embeddingModel = embeddingModel;
        _dimension = embeddingModel.Dimension;

        _databaseName = databaseName == RandomNameToken ? GenerateRandomName() : databaseName;
        var database = client.GetDatabase(_databaseName);

        _collectionName = collectionName == RandomNameToken ? GenerateRandomName() : collectionName;
        _collection = database.GetCollection<BsonDocument>(_collectionName);
    }

    /// <inheritdoc/>
    protected override async Task<object> WriteInternalAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Writing {ChunkCount} chunks to MongoDB collection: {CollectionName}", chunks.Count, _collectionName);

        // Get embeddings for all chunks
        var texts = chunks.Select(c => c.Text).ToList();
        var embeddings = await _embeddingModel.EmbedBatchAsync(texts, cancellationToken);

        // Create documents
        var documents = new List<BsonDocument>(chunks.Count);
        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var embedding = embeddings[i];

            var doc = new BsonDocument
            {
                ["_id"] = GenerateId(i, chunk),
                ["text"] = chunk.Text,
                ["start_index"] = chunk.StartIndex,
                ["end_index"] = chunk.EndIndex,
                ["token_count"] = chunk.TokenCount,
                ["embedding"] = new BsonArray(embedding)
            };

            documents.Add(doc);
        }

        // Insert documents
        try
        {
            await _collection.InsertManyAsync(documents, cancellationToken: cancellationToken);

            Logger.LogInformation("Successfully wrote {ChunkCount} chunks to MongoDB collection", chunks.Count);

            return new
            {
                Success = true,
                Count = chunks.Count,
                DatabaseName = _databaseName,
                CollectionName = _collectionName
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to insert chunks to MongoDB");
            throw new InvalidOperationException(
                $"Failed to write {chunks.Count} chunks to MongoDB collection '{_collectionName}'",
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
    /// Builds a MongoDB connection URI from components.
    /// </summary>
    private static string BuildConnectionUri(
        string? username,
        string? password,
        string hostname,
        int port)
    {
        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
        {
            return $"mongodb://{Uri.EscapeDataString(username)}:{Uri.EscapeDataString(password)}@{hostname}:{port}";
        }

        return $"mongodb://{hostname}:{port}";
    }

    /// <summary>
    /// Searches for similar chunks in the MongoDB collection using vector similarity (brute force).
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

        Logger.LogDebug("Searching MongoDB collection: {CollectionName} with limit={Limit}", _collectionName, limit);

        try
        {
            // Get the embedding for the query
            var queryEmbedding = await _embeddingModel.EmbedAsync(query, cancellationToken);
            var queryEmbeddingList = queryEmbedding.ToList();

            // Brute force search: retrieve all documents and compute similarity scores
            var allDocs = await _collection.Find(new BsonDocument()).ToListAsync(cancellationToken);

            var results = new List<Dictionary<string, object?>>();

            foreach (var doc in allDocs)
            {
                if (doc.TryGetValue("embedding", out var embeddingValue))
                {
                    // Extract embedding from BSON array
                    var docEmbedding = new List<double>();
                    if (embeddingValue.IsBsonArray)
                    {
                        foreach (var element in embeddingValue.AsBsonArray)
                        {
                            docEmbedding.Add(element.AsDouble);
                        }
                    }

                    if (docEmbedding.Count > 0)
                    {
                        // Calculate cosine similarity
                        var queryAsDouble = queryEmbeddingList.ConvertAll(x => (double)x);
                        var similarity = CosineSimilarity(queryAsDouble, docEmbedding);

                        var result = new Dictionary<string, object?>
                        {
                            ["id"] = doc.GetValue("_id").ToString(),
                            ["text"] = doc.GetValue("text", ""),
                            ["similarity"] = similarity,
                            ["start_index"] = doc.GetValue("start_index", 0),
                            ["end_index"] = doc.GetValue("end_index", 0),
                            ["token_count"] = doc.GetValue("token_count", 0)
                        };

                        results.Add(result);
                    }
                }
            }

            // Sort by similarity descending and take top limit
            results = results
                .OrderByDescending(r => Convert.ToDouble(r["similarity"]))
                .Take(limit)
                .ToList();

            Logger.LogInformation("Search complete: found {ResultCount} matching chunks", results.Count);

            return results;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to search MongoDB collection");
            throw new InvalidOperationException($"Failed to search MongoDB collection '{_collectionName}'", ex);
        }
    }

    /// <summary>
    /// Calculates cosine similarity between two vectors.
    /// </summary>
    private static double CosineSimilarity(List<double> vec1, List<double> vec2)
    {
        if (vec1.Count != vec2.Count)
        {
            throw new ArgumentException("Vectors must have the same length");
        }

        double dotProduct = 0.0;
        double mag1 = 0.0;
        double mag2 = 0.0;

        for (int i = 0; i < vec1.Count; i++)
        {
            dotProduct += vec1[i] * vec2[i];
            mag1 += vec1[i] * vec1[i];
            mag2 += vec2[i] * vec2[i];
        }

        mag1 = Math.Sqrt(mag1);
        mag2 = Math.Sqrt(mag2);

        const double magnitudeEpsilon = 1e-12;
        if (mag1 < magnitudeEpsilon || mag2 < magnitudeEpsilon)
        {
            return 0.0;
        }

        return dotProduct / (mag1 * mag2);
    }

    /// <summary>
    /// Generates a random collection/database name.
    /// </summary>
    private static string GenerateRandomName() => $"chonkie_{Guid.NewGuid():N}";

    /// <summary>
    /// Returns a string representation of this handshake instance.
    /// </summary>
    public override string ToString() =>
        $"MongoDBHandshake(db_name={_databaseName}, collection_name={_collectionName})";
}
