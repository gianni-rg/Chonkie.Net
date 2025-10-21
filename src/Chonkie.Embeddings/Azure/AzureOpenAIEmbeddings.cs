using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Chonkie.Embeddings.Base;

namespace Chonkie.Embeddings.Azure
{
    /// <summary>
    /// Embedding provider for Azure OpenAI Service.
    /// </summary>
    public class AzureOpenAIEmbeddings : BaseEmbeddings
    {
        private readonly AzureOpenAIClient _client;
        private readonly string _deploymentName;
        
        /// <inheritdoc />
        public override string Name => "azure-openai";
        
        /// <inheritdoc />
        public override int Dimension { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureOpenAIEmbeddings"/> class.
        /// </summary>
        /// <param name="endpoint">The Azure OpenAI endpoint URL.</param>
        /// <param name="apiKey">The API key for authentication.</param>
        /// <param name="deploymentName">The deployment name.</param>
        /// <param name="dimension">The dimension of the embedding vectors.</param>
        public AzureOpenAIEmbeddings(string endpoint, string apiKey, string deploymentName, int dimension = 1536)
        {
            var credential = new AzureKeyCredential(apiKey);
            _client = new AzureOpenAIClient(new Uri(endpoint), credential);
            _deploymentName = deploymentName;
            Dimension = dimension;
        }

        /// <inheritdoc />
        public override async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        {
            var embeddingClient = _client.GetEmbeddingClient(_deploymentName);
            var response = await embeddingClient.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
            var embedding = response.Value.ToFloats();
            return embedding.ToArray();
        }

        /// <inheritdoc />
        public override async Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
        {
            var embeddingClient = _client.GetEmbeddingClient(_deploymentName);
            var response = await embeddingClient.GenerateEmbeddingsAsync(texts, cancellationToken: cancellationToken);
            var results = new List<float[]>(response.Value.Count);
            foreach (var item in response.Value)
            {
                var floats = item.ToFloats().ToArray();
                results.Add(floats);
            }
            return results;
        }
    }
}