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
using Npgsql;
using NpgsqlTypes;
using Pgvector;
using Pgvector.Npgsql;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Chonkie.Handshakes;

/// <summary>
/// Handshake implementation for PostgreSQL with the pgvector extension.
/// Provides functionality to write chunks with embeddings to a PostgreSQL collection table.
/// </summary>
public sealed class PgvectorHandshake : BaseHandshake
{
    private static readonly Regex CollectionNamePattern = new("^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);
    private readonly NpgsqlDataSource _dataSource;
    private readonly string _collectionName;
    private readonly IEmbeddings _embeddingModel;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _initialized;
    private int? _vectorDimensions;

    /// <summary>
    /// Gets the collection (table) name.
    /// </summary>
    public string CollectionName => _collectionName;

    /// <summary>
    /// Gets the vector dimensions.
    /// </summary>
    public int VectorDimensions => _vectorDimensions ?? 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="PgvectorHandshake"/> class using connection options.
    /// </summary>
    /// <param name="options">The pgvector handshake configuration options.</param>
    /// <param name="embeddingModel">The embedding model to use for generating vectors.</param>
    /// <param name="logger">Optional logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when required arguments are null.</exception>
    /// <exception cref="ArgumentException">Thrown when the collection name is invalid.</exception>
    public PgvectorHandshake(
        PgvectorHandshakeOptions options,
        IEmbeddings embeddingModel,
        ILogger? logger = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(embeddingModel);

        ValidateCollectionName(options.CollectionName);

        _embeddingModel = embeddingModel;
        _vectorDimensions = options.VectorDimensions ?? (embeddingModel.Dimension > 0 ? embeddingModel.Dimension : null);
        _collectionName = options.CollectionName;

        var resolvedConnectionString = string.IsNullOrWhiteSpace(options.ConnectionString)
            ? BuildConnectionString(options)
            : options.ConnectionString;

        _dataSource = CreateDataSource(resolvedConnectionString);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PgvectorHandshake"/> class with a custom data source.
    /// </summary>
    /// <param name="dataSource">The Npgsql data source to use.</param>
    /// <param name="options">The pgvector handshake configuration options.</param>
    /// <param name="embeddingModel">The embedding model to use for generating vectors.</param>
    /// <param name="logger">Optional logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when required arguments are null.</exception>
    /// <exception cref="ArgumentException">Thrown when the collection name is invalid.</exception>
    public PgvectorHandshake(
        NpgsqlDataSource dataSource,
        PgvectorHandshakeOptions options,
        IEmbeddings embeddingModel,
        ILogger? logger = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(embeddingModel);

        ValidateCollectionName(options.CollectionName);

        _dataSource = dataSource;
        _collectionName = options.CollectionName;
        _embeddingModel = embeddingModel;
        _vectorDimensions = options.VectorDimensions ?? (embeddingModel.Dimension > 0 ? embeddingModel.Dimension : null);
    }

    /// <inheritdoc />
    protected override async Task<object> WriteInternalAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);

        Logger.LogDebug("Writing {ChunkCount} chunks to Pgvector collection: {CollectionName}", chunks.Count, _collectionName);

        var texts = chunks.Select(chunk => chunk.Text).ToList();
        var embeddings = await _embeddingModel.EmbedBatchAsync(texts, cancellationToken);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        for (var index = 0; index < chunks.Count; index++)
        {
            var chunk = chunks[index];
            var embedding = embeddings[index];
            var chunkId = GenerateChunkId(index, chunk);
            var metadataJson = JsonSerializer.Serialize(GenerateMetadata(chunk));

            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"INSERT INTO {_collectionName} (id, embedding, metadata) VALUES (@id, @embedding, @metadata) " +
                                  "ON CONFLICT (id) DO UPDATE SET embedding = EXCLUDED.embedding, metadata = EXCLUDED.metadata;";
            command.Parameters.AddWithValue("id", chunkId);
            command.Parameters.AddWithValue("embedding", new Vector(embedding));
            command.Parameters.Add("metadata", NpgsqlDbType.Jsonb).Value = metadataJson;

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        Logger.LogInformation("Successfully wrote {ChunkCount} chunks to Pgvector collection: {CollectionName}", chunks.Count, _collectionName);

        return new
        {
            Success = true,
            Count = chunks.Count,
            CollectionName = _collectionName
        };
    }

    /// <summary>
    /// Searches for similar chunks in the collection using vector similarity.
    /// </summary>
    /// <param name="query">The query text to search for.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="filters">Optional metadata filters represented as key/value pairs.</param>
    /// <param name="includeMetadata">Whether to include metadata in the results.</param>
    /// <param name="includeValue">Whether to include similarity scores in the results.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of search results with metadata and similarity scores.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
    public async Task<IReadOnlyList<Dictionary<string, object?>>> SearchAsync(
        string query,
        int limit = 5,
        IReadOnlyDictionary<string, object?>? filters = null,
        bool includeMetadata = true,
        bool includeValue = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        await EnsureInitializedAsync(cancellationToken);

        Logger.LogDebug("Searching Pgvector collection: {CollectionName} with limit={Limit}", _collectionName, limit);

        var queryEmbedding = await _embeddingModel.EmbedAsync(query, cancellationToken);
        var filterClause = filters is { Count: > 0 } ? "WHERE metadata @> @filters" : string.Empty;

        var sql = $"SELECT id, embedding <-> @query AS distance, metadata " +
                  $"FROM {_collectionName} {filterClause} " +
                  "ORDER BY embedding <-> @query LIMIT @limit;";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("query", new Vector(queryEmbedding));
        command.Parameters.AddWithValue("limit", limit);

        if (filters is { Count: > 0 })
        {
            command.Parameters.Add("filters", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(filters);
        }

        var results = new List<Dictionary<string, object?>>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var result = new Dictionary<string, object?>
            {
                ["id"] = reader.GetString(0)
            };

            if (includeValue)
            {
                result["similarity"] = reader.GetDouble(1);
            }

            if (includeMetadata)
            {
                var metadataJson = reader.GetString(2);
                var metadata = ParseMetadata(metadataJson);
                foreach (var pair in metadata)
                {
                    result[pair.Key] = pair.Value;
                }
            }

            results.Add(result);
        }

        Logger.LogInformation("Search complete: found {ResultCount} matching chunks", results.Count);

        return results;
    }

    /// <summary>
    /// Creates a vector index to speed up similarity searches.
    /// </summary>
    /// <param name="method">Index method to use ("hnsw" or "ivfflat").</param>
    /// <param name="distanceOperator">Distance operator class (e.g., "vector_cosine_ops").</param>
    /// <param name="indexOptions">Optional index configuration parameters. Valid keys depend on the method:
    /// For HNSW: m (int), ef_construction (int). For IVFFlat: lists (int), probes (int).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when the method, operator, or index options are invalid.</exception>
    public async Task CreateIndexAsync(
        string method = "hnsw",
        string distanceOperator = "vector_cosine_ops",
        IReadOnlyDictionary<string, int>? indexOptions = null,
        CancellationToken cancellationToken = default)
    {
        // Validate all input parameters before attempting any database operations.
        if (!string.Equals(method, "hnsw", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(method, "ivfflat", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Index method must be 'hnsw' or 'ivfflat'.", nameof(method));
        }

        if (!string.Equals(distanceOperator, "vector_cosine_ops", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(distanceOperator, "vector_l2_ops", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(distanceOperator, "vector_ip_ops", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Distance operator must be vector_cosine_ops, vector_l2_ops, or vector_ip_ops.", nameof(distanceOperator));
        }

        // Validate index options using an allowlist pattern to prevent SQL injection.
        ValidateIndexOptions(indexOptions, method);

        await EnsureInitializedAsync(cancellationToken);

        var indexName = $"{_collectionName}_embedding_idx";
        var optionsClause = indexOptions is { Count: > 0 }
            ? "WITH (" + string.Join(", ", indexOptions.Select(option => $"{option.Key}={option.Value}")) + ")"
            : string.Empty;

        var sql = $"CREATE INDEX IF NOT EXISTS {indexName} ON {_collectionName} USING {method} (embedding {distanceOperator}) {optionsClause};";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);

        Logger.LogInformation("Created {Method} index on Pgvector collection: {CollectionName}", method, _collectionName);
    }

    /// <summary>
    /// Deletes the collection (table) from PostgreSQL.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteCollectionAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP TABLE IF EXISTS {_collectionName};";
        await command.ExecuteNonQueryAsync(cancellationToken);

        Logger.LogInformation("Deleted Pgvector collection: {CollectionName}", _collectionName);
    }

    /// <summary>
    /// Gets basic information about the collection.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Collection metadata including name, dimension, and row count.</returns>
    public async Task<Dictionary<string, object?>> GetCollectionInfoAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {_collectionName};";
        var count = (long)(await command.ExecuteScalarAsync(cancellationToken) ?? 0L);

        return new Dictionary<string, object?>
        {
            ["name"] = _collectionName,
            ["dimension"] = _vectorDimensions,
            ["count"] = count
        };
    }

    /// <summary>
    /// Returns a string representation of the handshake.
    /// </summary>
    /// <returns>String representation including collection name and dimensions.</returns>
    public override string ToString()
    {
        return $"PgvectorHandshake(collection_name={_collectionName}, vector_dimensions={_vectorDimensions})";
    }

    private static string BuildConnectionString(PgvectorHandshakeOptions options)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = options.Host,
            Port = options.Port,
            Database = options.Database,
            Username = options.Username,
            Password = options.Password
        };

        return builder.ConnectionString;
    }

    private static NpgsqlDataSource CreateDataSource(string connectionString)
    {
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        builder.UseVector();
        return builder.Build();
    }

    private static void ValidateCollectionName(string collectionName)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
        {
            throw new ArgumentException("Collection name cannot be empty or whitespace.", nameof(collectionName));
        }

        if (!CollectionNamePattern.IsMatch(collectionName))
        {
            throw new ArgumentException("Collection name must contain only letters, numbers, and underscores.", nameof(collectionName));
        }
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return;
            }

            if (_vectorDimensions is null || _vectorDimensions <= 0)
            {
                var sampleEmbedding = await _embeddingModel.EmbedAsync("test", cancellationToken);
                _vectorDimensions = sampleEmbedding.Length;
            }

            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE EXTENSION IF NOT EXISTS vector; " +
                                  $"CREATE TABLE IF NOT EXISTS {_collectionName} (" +
                                  "id TEXT PRIMARY KEY, " +
                                  $"embedding vector({_vectorDimensions}), " +
                                  "metadata JSONB" +
                                  ");";

            await command.ExecuteNonQueryAsync(cancellationToken);
            _initialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    private string GenerateChunkId(int index, Chunk chunk)
    {
        var input = $"{_collectionName}::chunk-{index}:{chunk.Text}";
        return CreateUuid5(input);
    }

    private static string CreateUuid5(string name)
    {
        var namespaceBytes = new Guid("6ba7b812-9dad-11d1-80b4-00c04fd430c8").ToByteArray();
        var nameBytes = Encoding.UTF8.GetBytes(name);
        var data = new byte[namespaceBytes.Length + nameBytes.Length];

        Buffer.BlockCopy(namespaceBytes, 0, data, 0, namespaceBytes.Length);
        Buffer.BlockCopy(nameBytes, 0, data, namespaceBytes.Length, nameBytes.Length);

        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(data);

        hash[6] = (byte)((hash[6] & 0x0F) | (5 << 4));
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

        var guidBytes = new byte[16];
        Array.Copy(hash, 0, guidBytes, 0, 16);
        return new Guid(guidBytes).ToString();
    }

    private static Dictionary<string, object?> GenerateMetadata(Chunk chunk)
    {
        var metadata = new Dictionary<string, object?>
        {
            ["text"] = chunk.Text,
            ["start_index"] = chunk.StartIndex,
            ["end_index"] = chunk.EndIndex,
            ["token_count"] = chunk.TokenCount,
            ["chunk_type"] = chunk.GetType().Name
        };

        if (!string.IsNullOrWhiteSpace(chunk.Context))
        {
            metadata["context"] = chunk.Context;
        }

        return metadata;
    }

    private static void ValidateIndexOptions(IReadOnlyDictionary<string, int>? indexOptions, string method)
    {
        if (indexOptions is null or { Count: 0 })
        {
            return;
        }

        // Allowlist of valid index parameter keys by method type.
        var validKeys = method.Equals("hnsw", StringComparison.OrdinalIgnoreCase)
            ? new[] { "m", "ef_construction" }
            : new[] { "lists", "probes" };

        foreach (var key in indexOptions.Keys)
        {
            if (!validKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Invalid index option '{key}' for method '{method}'. Valid options are: {string.Join(", ", validKeys)}.",
                    nameof(indexOptions));
            }

            var value = indexOptions[key];
            if (value <= 0)
            {
                throw new ArgumentException(
                    $"Index option '{key}' must be a positive integer, but got {value}.",
                    nameof(indexOptions));
            }
        }
    }

    private static Dictionary<string, object?> ParseMetadata(string json)
    {
        using var document = JsonDocument.Parse(json);
        var result = JsonElementToObject(document.RootElement) as Dictionary<string, object?>;
        return result ?? new Dictionary<string, object?>();
    }

    private static object? JsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(
                property => property.Name,
                property => JsonElementToObject(property.Value)),
            JsonValueKind.Array => element.EnumerateArray().Select(JsonElementToObject).ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var integer) ? integer : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }
}
