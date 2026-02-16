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

using Chonkie.Embeddings.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pinecone;
using Qdrant.Client;
using Weaviate.Client;
using System;

namespace Chonkie.Handshakes;

/// <summary>
/// Extension methods for registering handshakes with dependency injection.
/// </summary>
public static class HandshakeServiceExtensions
{
    private const string ChromaServerUrlEnvironmentVariable = "CHONKIE_CHROMA_URL";
    private const string MilvusServerUrlEnvironmentVariable = "CHONKIE_MILVUS_URL";
    private const string ElasticsearchServerUrlEnvironmentVariable = "CHONKIE_ELASTICSEARCH_URL";

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
    /// <param name="serverUrl">The Chroma server URL. If null, uses CHONKIE_CHROMA_URL.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddChromaHandshake(
        this IServiceCollection services,
        string collectionName,
        IEmbeddings embeddingModel,
        string? serverUrl = null)
    {
        services.AddSingleton<IHandshake>(sp =>
        {
            var logger = sp.GetService<ILogger<ChromaHandshake>>();
            var resolvedServerUrl = ResolveRequiredSetting(serverUrl, ChromaServerUrlEnvironmentVariable, nameof(serverUrl));
            return new ChromaHandshake(collectionName, embeddingModel, resolvedServerUrl, logger: logger);
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
        string? serverUrl = null,
        string collectionName = "random")
    {
        services.AddSingleton<IHandshake>(sp =>
        {
            var logger = sp.GetService<ILogger<MilvusHandshake>>();
            var resolvedServerUrl = ResolveRequiredSetting(serverUrl, MilvusServerUrlEnvironmentVariable, nameof(serverUrl));
            return new MilvusHandshake(embeddingModel, serverUrl: resolvedServerUrl, collectionName: collectionName, logger: logger);
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
        string? serverUrl = null,
        string indexName = "random",
        string? apiKey = null)
    {
        services.AddSingleton<IHandshake>(sp =>
        {
            var logger = sp.GetService<ILogger<ElasticsearchHandshake>>();
            var resolvedServerUrl = ResolveRequiredSetting(serverUrl, ElasticsearchServerUrlEnvironmentVariable, nameof(serverUrl));
            return new ElasticsearchHandshake(embeddingModel, resolvedServerUrl, indexName, apiKey, logger: logger);
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
    }

    private static string ResolveRequiredSetting(string? value, string environmentVariableName, string parameterName)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var environmentValue = Environment.GetEnvironmentVariable(environmentVariableName);
        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            return environmentValue;
        }

        throw new InvalidOperationException(
            $"Missing required setting for {parameterName}. Set {environmentVariableName} or pass {parameterName} explicitly.");
    }
}
