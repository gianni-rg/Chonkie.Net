using Chonkie.Embeddings.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Qdrant.Client;

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
}
