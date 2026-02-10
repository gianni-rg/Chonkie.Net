# API Reference - Handshakes
**Scope:** Vector database integrations and DI helpers.

## Python Reference
- [chonkie/docs/oss/handshakes/overview.mdx](chonkie/docs/oss/handshakes/overview.mdx)

## Chonkie.Handshakes

### IHandshake
Interface for vector DB writers.

Members:
- Methods: `Task<object> WriteAsync(IEnumerable<Chunk> chunks, CancellationToken cancellationToken = default)`

### BaseHandshake
Base class providing logging and error handling.

Members:
- Constructors: `protected BaseHandshake(ILogger? logger = null)`
- Methods: `Task<object> WriteAsync(IEnumerable<Chunk> chunks, CancellationToken cancellationToken = default)`
- Methods (protected): `abstract Task<object> WriteInternalAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken)`

### PgvectorHandshakeOptions
Options for `PgvectorHandshake`.

Members:
- Properties: `string Host { get; init; }`, `int Port { get; init; }`, `string Database { get; init; }`, `string Username { get; init; }`, `string Password { get; init; }`, `string? ConnectionString { get; init; }`, `string CollectionName { get; init; }`, `int? VectorDimensions { get; init; }`

### ChromaHandshake
REST-based Chroma integration.

Members:
- Constructors: `ChromaHandshake(string collectionName, IEmbeddings embeddingModel, string? serverUrl = null, HttpClient? httpClient = null, ILogger? logger = null)`
- Properties: `string CollectionName { get; }`
- Methods: `Task<IReadOnlyList<Dictionary<string, object?>>> SearchAsync(string query, int limit = 5, CancellationToken cancellationToken = default)`, `override string ToString()`

### ElasticsearchHandshake
Elasticsearch 8+ integration.

Members:
- Constructors: `ElasticsearchHandshake(IEmbeddings embeddingModel, string? serverUrl = null, string indexName = "random", string? apiKey = null, ElasticsearchClient? client = null, ILogger? logger = null)`
- Properties: `string IndexName { get; }`, `int Dimension { get; }`
- Methods: `Task<IReadOnlyList<Dictionary<string, object?>>> SearchAsync(string query, int limit = 5, CancellationToken cancellationToken = default)`, `override string ToString()`

### MilvusHandshake
REST-based Milvus integration.

Members:
- Constructors: `MilvusHandshake(IEmbeddings embeddingModel, string? serverUrl = null, string collectionName = "random", HttpClient? httpClient = null, ILogger? logger = null)`
- Properties: `string CollectionName { get; }`, `int Dimension { get; }`
- Methods: `Task<IReadOnlyList<Dictionary<string, object?>>> SearchAsync(string query, int limit = 5, CancellationToken cancellationToken = default)`, `override string ToString()`

### MongoDBHandshake
MongoDB vector storage integration.

Members:
- Constructors: `MongoDBHandshake(IEmbeddings embeddingModel, string? hostname = null, int? port = null, string? username = null, string? password = null, string databaseName = "chonkie_db", string collectionName = "chonkie_collection", ILogger? logger = null)`, `MongoDBHandshake(string uri, IEmbeddings embeddingModel, string databaseName = "chonkie_db", string collectionName = "chonkie_collection", ILogger? logger = null)`, `MongoDBHandshake(IMongoClient client, IEmbeddings embeddingModel, string databaseName = "chonkie_db", string collectionName = "chonkie_collection", ILogger? logger = null)`
- Properties: `string DatabaseName { get; }`, `string CollectionName { get; }`, `int Dimension { get; }`
- Methods: `Task<IReadOnlyList<Dictionary<string, object?>>> SearchAsync(string query, int limit = 5, CancellationToken cancellationToken = default)`, `override string ToString()`

### PgvectorHandshake
PostgreSQL + pgvector integration.

Members:
- Constructors: `PgvectorHandshake(PgvectorHandshakeOptions options, IEmbeddings embeddingModel, ILogger? logger = null)`, `PgvectorHandshake(NpgsqlDataSource dataSource, PgvectorHandshakeOptions options, IEmbeddings embeddingModel, ILogger? logger = null)`
- Properties: `string CollectionName { get; }`, `int VectorDimensions { get; }`
- Methods: `Task<IReadOnlyList<Dictionary<string, object?>>> SearchAsync(string query, int limit = 5, IReadOnlyDictionary<string, object?>? filters = null, bool includeMetadata = true, bool includeValue = true, CancellationToken cancellationToken = default)`, `Task CreateIndexAsync(string method = "hnsw", string distanceOperator = "vector_cosine_ops", IReadOnlyDictionary<string, int>? indexOptions = null, CancellationToken cancellationToken = default)`, `Task DeleteCollectionAsync(CancellationToken cancellationToken = default)`, `Task<Dictionary<string, object?>> GetCollectionInfoAsync(CancellationToken cancellationToken = default)`, `override string ToString()`

### PineconeHandshake
Pinecone integration.

Members:
- Constructors: `PineconeHandshake(string apiKey, string indexName, IEmbeddings embeddingModel, string? @namespace = null, ILogger? logger = null)`, `PineconeHandshake(PineconeClient client, string indexName, IEmbeddings embeddingModel, string? @namespace = null, ILogger? logger = null)`
- Properties: `string IndexName { get; }`, `string Namespace { get; }`, `int Dimension { get; }`
- Methods: `Task<QueryResponse> SearchAsync(string query, int limit = 5, CancellationToken cancellationToken = default)`, `override string ToString()`

### QdrantHandshake
Qdrant integration.

Members:
- Constructors: `QdrantHandshake(string url, string collectionName, IEmbeddings embeddingModel, string? apiKey = null, ILogger? logger = null)`, `QdrantHandshake(QdrantClient client, string collectionName, IEmbeddings embeddingModel, ILogger? logger = null)`
- Properties: `string CollectionName { get; }`, `uint Dimension { get; }`
- Methods: `Task<IReadOnlyList<ScoredPoint>> SearchAsync(string query, int limit = 5, CancellationToken cancellationToken = default)`, `override string ToString()`

### TurbopufferHandshake
Turbopuffer integration.

Members:
- Constructors: `TurbopufferHandshake(IEmbeddings embeddingModel, string? apiKey = null, string namespaceName = "random", string? apiUrl = null, HttpClient? httpClient = null, ILogger? logger = null)`
- Properties: `string Namespace { get; }`, `string ApiKeyMasked { get; }`
- Methods: `Task<IReadOnlyList<Dictionary<string, object?>>> SearchAsync(string query, int limit = 5, CancellationToken cancellationToken = default)`, `override string ToString()`

### WeaviateHandshake
Weaviate integration.

Members:
- Constructors: `WeaviateHandshake(WeaviateClient client, string className, IEmbeddings embeddingModel, ILogger? logger = null)`
- Properties: `string ClassName { get; }`, `int Dimension { get; }`
- Methods: `static Task<WeaviateHandshake> CreateCloudAsync(string url, string apiKey, string className, IEmbeddings embeddingModel, ILogger? logger = null)`, `Task<object> SearchAsync(string query, int limit = 5, CancellationToken cancellationToken = default)`, `override string ToString()`

## Chonkie.Handshakes (DI)

### HandshakeServiceExtensions
DI registration helpers.

Members:
- Methods: `static IServiceCollection AddQdrantHandshake(this IServiceCollection services, string url, string collectionName, IEmbeddings embeddingModel, string? apiKey = null)`, `static IServiceCollection AddQdrantHandshake(this IServiceCollection services, QdrantClient client, string collectionName, IEmbeddings embeddingModel)`, `static IServiceCollection AddPineconeHandshake(this IServiceCollection services, string apiKey, string indexName, IEmbeddings embeddingModel, string? @namespace = null)`, `static IServiceCollection AddPineconeHandshake(this IServiceCollection services, PineconeClient client, string indexName, IEmbeddings embeddingModel, string? @namespace = null)`, `static IServiceCollection AddWeaviateHandshake(this IServiceCollection services, string url, string apiKey, string className, IEmbeddings embeddingModel)`, `static IServiceCollection AddWeaviateHandshake(this IServiceCollection services, WeaviateClient client, string className, IEmbeddings embeddingModel)`, `static IServiceCollection AddPgvectorHandshake(this IServiceCollection services, PgvectorHandshakeOptions options, IEmbeddings embeddingModel)`, `static IServiceCollection AddPgvectorHandshake(this IServiceCollection services, NpgsqlDataSource dataSource, PgvectorHandshakeOptions options, IEmbeddings embeddingModel)`, `static IServiceCollection AddChromaHandshake(this IServiceCollection services, string collectionName, IEmbeddings embeddingModel, string? serverUrl = null)`, `static IServiceCollection AddMongoDBHandshake(this IServiceCollection services, IEmbeddings embeddingModel, string hostname = "localhost", int port = 27017)`, `static IServiceCollection AddMilvusHandshake(this IServiceCollection services, IEmbeddings embeddingModel, string? serverUrl = null, string collectionName = "random")`, `static IServiceCollection AddElasticsearchHandshake(this IServiceCollection services, IEmbeddings embeddingModel, string? serverUrl = null, string indexName = "random", string? apiKey = null)`, `static IServiceCollection AddTurbopufferHandshake(this IServiceCollection services, IEmbeddings embeddingModel, string? apiKey = null)`
