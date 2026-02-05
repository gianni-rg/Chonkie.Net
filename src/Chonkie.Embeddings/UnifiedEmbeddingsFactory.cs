using System;
using Azure;
using Azure.AI.OpenAI;
using Chonkie.Embeddings.Interfaces;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using OpenAI;
using OpenAI.Embeddings;  // For AsIEmbeddingGenerator extension

namespace Chonkie.Embeddings
{
    /// <summary>
    /// Factory for creating UnifiedEmbeddings instances for various providers.
    /// Provides simplified creation of embedding generators using Microsoft.Extensions.AI standard.
    /// </summary>
    /// <remarks>
    /// Supported providers:
    /// - OpenAI (text-embedding-ada-002, text-embedding-3-small, text-embedding-3-large)
    /// - Azure OpenAI (same models, Azure-hosted)
    /// - Ollama (local models: all-minilm, mxbai-embed-large, nomic-embed-text, etc.)
    /// 
    /// For other providers (Google Gemini, MistralAI, HuggingFace, ONNX), use Semantic Kernel's
    /// dependency injection registration pattern with AddGoogleAIEmbeddingGeneration(), AddMistralAIEmbeddingGeneration(), etc.
    /// </remarks>
    public static class UnifiedEmbeddingsFactory
    {
        /// <summary>
        /// Creates an OpenAI embedding provider.
        /// </summary>
        /// <param name="apiKey">OpenAI API key.</param>
        /// <param name="model">Model name (default: text-embedding-ada-002).</param>
        /// <param name="dimension">Embedding dimension (default: 1536 for ada-002).</param>
        /// <param name="logger">Optional logger.</param>
        /// <returns>IEmbeddings instance wrapping OpenAI.</returns>
        public static IEmbeddings CreateOpenAI(
            string apiKey,
            string model = "text-embedding-ada-002",
            int dimension = 1536,
            ILogger? logger = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be null or whitespace.", nameof(apiKey));

            var client = new OpenAIClient(apiKey);
            var embeddingClient = client.GetEmbeddingClient(model);
            var generator = embeddingClient.AsIEmbeddingGenerator();
            
            return new UnifiedEmbeddings(generator, "openai", dimension, logger);
        }

        /// <summary>
        /// Creates an Azure OpenAI embedding provider.
        /// </summary>
        /// <param name="endpoint">Azure OpenAI endpoint URL.</param>
        /// <param name="apiKey">Azure OpenAI API key.</param>
        /// <param name="deploymentName">Deployment name.</param>
        /// <param name="dimension">Embedding dimension (default: 1536).</param>
        /// <param name="logger">Optional logger.</param>
        /// <returns>IEmbeddings instance wrapping Azure OpenAI.</returns>
        public static IEmbeddings CreateAzureOpenAI(
            string endpoint,
            string apiKey,
            string deploymentName,
            int dimension = 1536,
            ILogger? logger = null)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Endpoint cannot be null or whitespace.", nameof(endpoint));
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be null or whitespace.", nameof(apiKey));
            if (string.IsNullOrWhiteSpace(deploymentName))
                throw new ArgumentException("Deployment name cannot be null or whitespace.", nameof(deploymentName));

            var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            var embeddingClient = client.GetEmbeddingClient(deploymentName);
            var generator = embeddingClient.AsIEmbeddingGenerator();
            
            return new UnifiedEmbeddings(generator, "azure-openai", dimension, logger);
        }

        /// <summary>
        /// Creates an Ollama embedding provider for local models.
        /// </summary>
        /// <param name="model">Model name (e.g., "all-minilm", "mxbai-embed-large", "nomic-embed-text").</param>
        /// <param name="dimension">Embedding dimension (model-specific: all-minilm=384, mxbai=1024, nomic=768).</param>
        /// <param name="endpoint">Ollama endpoint URL (default: http://localhost:11434).</param>
        /// <param name="logger">Optional logger.</param>
        /// <returns>IEmbeddings instance wrapping Ollama.</returns>
        public static IEmbeddings CreateOllama(
            string model,
            int dimension,
            string endpoint = "http://localhost:11434",
            ILogger? logger = null)
        {
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be null or whitespace.", nameof(model));
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Endpoint cannot be null or whitespace.", nameof(endpoint));

            // OllamaApiClient implements IEmbeddingGenerator<string, Embedding<float>> directly
            var ollamaClient = new OllamaApiClient(new Uri(endpoint), model);
            IEmbeddingGenerator<string, Embedding<float>> generator = 
                (IEmbeddingGenerator<string, Embedding<float>>)ollamaClient;
            
            return new UnifiedEmbeddings(generator, "ollama", dimension, logger);
        }

        /// <summary>
        /// Creates an embedding provider from environment variables.
        /// Supports: OPENAI_API_KEY, AZURE_OPENAI_ENDPOINT/API_KEY/DEPLOYMENT, OLLAMA_MODEL.
        /// </summary>
        /// <param name="providerName">Provider name ("openai", "azure", or "ollama").</param>
        /// <param name="model">Model name (not used for Azure, uses deployment instead).</param>
        /// <param name="dimension">Embedding dimension.</param>
        /// <param name="logger">Optional logger.</param>
        /// <returns>IEmbeddings instance for the specified provider.</returns>
        public static IEmbeddings CreateFromEnvironment(
            string providerName,
            string model,
            int dimension,
            ILogger? logger = null)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("Provider name cannot be null or whitespace.", nameof(providerName));

            return providerName.ToLowerInvariant() switch
            {
                "openai" => CreateOpenAI(
                    Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
                        ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable not set"),
                    model,
                    dimension,
                    logger),

                "azure" or "azure-openai" => CreateAzureOpenAI(
                    Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") 
                        ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT environment variable not set"),
                    Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") 
                        ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY environment variable not set"),
                    Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") 
                        ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT environment variable not set"),
                    dimension,
                    logger),

                "ollama" => CreateOllama(
                    model,
                    dimension,
                    Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? "http://localhost:11434",
                    logger),

                _ => throw new ArgumentException($"Unsupported provider: {providerName}. Supported: openai, azure, ollama", nameof(providerName))
            };
        }
    }
}

