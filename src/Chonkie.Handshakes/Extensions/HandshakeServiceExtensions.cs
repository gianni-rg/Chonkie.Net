using Chonkie.Embeddings.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
}
