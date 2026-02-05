using Chonkie.Embeddings.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pinecone;
using Qdrant.Client;
using Weaviate.Client;

namespace Chonkie.Handshakes;

/// <summary>
/// Extension methods for registering handshakes with dependency injection.
/// </summary>
public static class HandshakeServiceExtensions
{
    /// <summary>
    /// Adds a QdrantHandshake to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="url">The Qdrant server URL.</param>
    /// <param name="collectionName">The collection name.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <param name="apiKey">Optional API key for Qdrant Cloud.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddQdrantHandshake(
        this IServiceCollection services,
        string url,
        string collectionName,
        IEmbeddings embeddingModel,
        string? apiKey = null)
    {
        services.AddSingleton<IHandshake>(sp =>
        {
            var logger = sp.GetService<ILogger<QdrantHandshake>>();
            return new QdrantHandshake(url, collectionName, embeddingModel, apiKey, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds a QdrantHandshake to the service collection with a custom client.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="client">The Qdrant client.</param>
    /// <param name="collectionName">The collection name.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddQdrantHandshake(
        this IServiceCollection services,
        QdrantClient client,
        string collectionName,
        IEmbeddings embeddingModel)
    {
        services.AddSingleton<IHandshake>(sp =>
        {
            var logger = sp.GetService<ILogger<QdrantHandshake>>();
            return new QdrantHandshake(client, collectionName, embeddingModel, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds a PineconeHandshake to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiKey">The Pinecone API key.</param>
    /// <param name="indexName">The Pinecone index name.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <param name="namespace">Optional namespace for vectors.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPineconeHandshake(
        this IServiceCollection services,
        string apiKey,
        string indexName,
        IEmbeddings embeddingModel,
        string? @namespace = null)
    {
        services.AddSingleton<IHandshake>(sp =>
        {
            var logger = sp.GetService<ILogger<PineconeHandshake>>();
            return new PineconeHandshake(apiKey, indexName, embeddingModel, @namespace, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds a PineconeHandshake to the service collection with a custom client.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="client">The Pinecone client.</param>
    /// <param name="indexName">The Pinecone index name.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <param name="namespace">Optional namespace for vectors.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPineconeHandshake(
        this IServiceCollection services,
        PineconeClient client,
        string indexName,
        IEmbeddings embeddingModel,
        string? @namespace = null)
    {
        services.AddSingleton<IHandshake>(sp =>
        {
            var logger = sp.GetService<ILogger<PineconeHandshake>>();
            return new PineconeHandshake(client, indexName, embeddingModel, @namespace, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds a WeaviateHandshake to the service collection using the factory method for Weaviate Cloud.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="url">The Weaviate Cloud URL.</param>
    /// <param name="apiKey">The Weaviate Cloud API key.</param>
    /// <param name="className">The Weaviate class (collection) name.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWeaviateHandshake(
        this IServiceCollection services,
        string url,
        string apiKey,
        string className,
        IEmbeddings embeddingModel)
    {
        services.AddSingleton<IHandshake>(sp =>
        {
            var logger = sp.GetService<ILogger<WeaviateHandshake>>();
            // Since CreateCloudAsync is async, we need to use Task.Run or call it synchronously
            // For serviceCollection registration, we'll block here (acceptable pattern for DI)
            return WeaviateHandshake.CreateCloudAsync(url, apiKey, className, embeddingModel, logger).GetAwaiter().GetResult();
        });

        return services;
    }

    /// <summary>
    /// Adds a WeaviateHandshake to the service collection with a custom client.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="client">The Weaviate client.</param>
    /// <param name="className">The Weaviate class (collection) name.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWeaviateHandshake(
        this IServiceCollection services,
        WeaviateClient client,
        string className,
        IEmbeddings embeddingModel)
    {
        services.AddSingleton<IHandshake>(sp =>
        {
            var logger = sp.GetService<ILogger<WeaviateHandshake>>();
            return new WeaviateHandshake(client, className, embeddingModel, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds a PgvectorHandshake to the service collection using connection options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The pgvector handshake configuration options.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPgvectorHandshake(
        this IServiceCollection services,
        PgvectorHandshakeOptions options,
        IEmbeddings embeddingModel)
    {
        services.AddSingleton<IHandshake>(sp =>
        {
            var logger = sp.GetService<ILogger<PgvectorHandshake>>();
            return new PgvectorHandshake(options, embeddingModel, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds a PgvectorHandshake to the service collection with a custom data source.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="dataSource">The Npgsql data source.</param>
    /// <param name="options">The pgvector handshake configuration options.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPgvectorHandshake(
        this IServiceCollection services,
        NpgsqlDataSource dataSource,
        PgvectorHandshakeOptions options,
        IEmbeddings embeddingModel)
    {
        services.AddSingleton<IHandshake>(sp =>
        {
            var logger = sp.GetService<ILogger<PgvectorHandshake>>();
            return new PgvectorHandshake(dataSource, options, embeddingModel, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds a ChromaHandshake to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="collectionName">The name of the Chroma collection.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddChromaHandshake(
        this IServiceCollection services,
        string collectionName,
        IEmbeddings embeddingModel)
    {
        services.AddSingleton<IHandshake>(sp =>
        {
            var logger = sp.GetService<ILogger<ChromaHandshake>>();
            return new ChromaHandshake(collectionName, embeddingModel, logger: logger);
        });

        return services;
    }

    /// <summary>
    /// Adds a MongoDBHandshake to the service collection using hostname and port.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <param name="hostname">The MongoDB hostname.</param>
    /// <param name="port">The MongoDB port.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMongoDBHandshake(
        this IServiceCollection services,
        IEmbeddings embeddingModel,
        string hostname = "localhost",
        int port = 27017)
    {
        services.AddSingleton<IHandshake>(sp =>
        {
            var logger = sp.GetService<ILogger<MongoDBHandshake>>();
            return new MongoDBHandshake(embeddingModel, hostname: hostname, port: port, logger: logger);
        });

        return services;
    }

    /// <summary>
    /// Adds a MilvusHandshake to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <param name="serverUrl">The Milvus server URL.</param>
    /// <param name="collectionName">The collection name.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMilvusHandshake(
        this IServiceCollection services,
        IEmbeddings embeddingModel,
        string serverUrl = "http://localhost:19530",
        string collectionName = "random")
    {
        services.AddSingleton<IHandshake>(sp =>
        {
            var logger = sp.GetService<ILogger<MilvusHandshake>>();
            return new MilvusHandshake(embeddingModel, serverUrl: serverUrl, collectionName: collectionName, logger: logger);
        });

        return services;
    }

    /// <summary>
    /// Adds an ElasticsearchHandshake to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <param name="serverUrl">The Elasticsearch server URL.</param>
    /// <param name="indexName">The Elasticsearch index name.</param>
    /// <param name="apiKey">Optional API key for authentication.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddElasticsearchHandshake(
        this IServiceCollection services,
        IEmbeddings embeddingModel,
        string serverUrl = "http://localhost:9200",
        string indexName = "random",
        string? apiKey = null)
    {
        services.AddSingleton<IHandshake>(sp =>
        {
            var logger = sp.GetService<ILogger<ElasticsearchHandshake>>();
            return new ElasticsearchHandshake(embeddingModel, serverUrl, indexName, apiKey, logger: logger);
        });

        return services;
    }

    /// <summary>
    /// Adds a TurbopufferHandshake to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="embeddingModel">The embedding model to use.</param>
    /// <param name="apiKey">The Turbopuffer API key. If null, uses TURBOPUFFER_API_KEY environment variable.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTurbopufferHandshake(
        this IServiceCollection services,
        IEmbeddings embeddingModel,
        string? apiKey = null)
    {
        services.AddSingleton<IHandshake>(sp =>
        {
            var logger = sp.GetService<ILogger<TurbopufferHandshake>>();
            return new TurbopufferHandshake(embeddingModel, apiKey: apiKey, logger: logger);
        });

        return services;
    }}