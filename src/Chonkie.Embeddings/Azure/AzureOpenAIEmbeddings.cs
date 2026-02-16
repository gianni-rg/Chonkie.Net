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
            // Azure OpenAI API doesn't accept empty strings, return zero vector
            if (string.IsNullOrEmpty(text))
            {
                return new float[Dimension];
            }

            var embeddingClient = _client.GetEmbeddingClient(_deploymentName);
            var response = await embeddingClient.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
            var embedding = response.Value.ToFloats();
            return embedding.ToArray();
        }

        /// <inheritdoc />
        public override async Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
        {
            var textList = texts.ToList();
            var results = new List<float[]>(textList.Count);

            // Separate empty and non-empty texts
            var nonEmptyIndices = new List<int>();
            var nonEmptyTexts = new List<string>();

            for (int i = 0; i < textList.Count; i++)
            {
                if (string.IsNullOrEmpty(textList[i]))
                {
                    results.Add(new float[Dimension]);
                }
                else
                {
                    nonEmptyIndices.Add(i);
                    nonEmptyTexts.Add(textList[i]);
                    results.Add(null!); // Placeholder
                }
            }

            // Process non-empty texts if any
            if (nonEmptyTexts.Count > 0)
            {
                var embeddingClient = _client.GetEmbeddingClient(_deploymentName);
                var response = await embeddingClient.GenerateEmbeddingsAsync(nonEmptyTexts, cancellationToken: cancellationToken);

                for (int i = 0; i < response.Value.Count; i++)
                {
                    var floats = response.Value[i].ToFloats().ToArray();
                    results[nonEmptyIndices[i]] = floats;
                }
            }

            return results;
        }
    }
}
