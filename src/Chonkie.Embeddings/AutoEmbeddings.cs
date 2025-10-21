using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Embeddings.Interfaces;
using Chonkie.Embeddings.Base;

namespace Chonkie.Embeddings
{
    /// <summary>
    /// Factory for automatic embedding provider selection.
    /// </summary>
    public static class AutoEmbeddings
    {
        private static readonly Dictionary<string, Func<IEmbeddings>> Providers = new()
        {
            { "openai", () => new OpenAI.OpenAIEmbeddings(Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "") },
            { "azure-openai", () => new Azure.AzureOpenAIEmbeddings(
                Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "",
                Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? "",
                Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? ""
            ) },
            { "sentence-transformers", () => new SentenceTransformers.SentenceTransformerEmbeddings(
                Environment.GetEnvironmentVariable("SENTENCE_TRANSFORMERS_MODEL_PATH") ?? ""
            ) },
            { "cohere", () => new Cohere.CohereEmbeddings(Environment.GetEnvironmentVariable("COHERE_API_KEY") ?? "") },
            { "gemini", () => new Gemini.GeminiEmbeddings(Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "") },
            { "jina", () => new Jina.JinaEmbeddings(Environment.GetEnvironmentVariable("JINA_API_KEY") ?? "") },
            { "voyage", () => new VoyageAI.VoyageAIEmbeddings(Environment.GetEnvironmentVariable("VOYAGE_API_KEY") ?? "") }
        };

        /// <summary>
        /// Registers a new embedding provider.
        /// </summary>
        public static void RegisterProvider(string name, Func<IEmbeddings> factory)
        {
            Providers[name.ToLowerInvariant()] = factory;
        }

        /// <summary>
        /// Gets an embedding provider by name.
        /// </summary>
        public static IEmbeddings GetProvider(string name)
        {
            if (Providers.TryGetValue(name.ToLowerInvariant(), out var factory))
                return factory();
            throw new ArgumentException($"Embedding provider '{name}' not found.");
        }

        /// <summary>
        /// Lists all registered providers.
        /// </summary>
        public static IReadOnlyList<string> ListProviders() => Providers.Keys.ToList();
    }
}