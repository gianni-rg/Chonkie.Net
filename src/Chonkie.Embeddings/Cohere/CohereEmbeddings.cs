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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Embeddings.Base;
using Chonkie.Embeddings.Exceptions;

namespace Chonkie.Embeddings.Cohere
{
    /// <summary>
    /// Embedding provider for Cohere API.
    /// </summary>
    public class CohereEmbeddings : BaseEmbeddings
    {
        private readonly string _apiUrl;
        private readonly string _model;
        private readonly HttpClient _httpClient;

        /// <inheritdoc />
        public override string Name => "cohere";

        /// <inheritdoc />
        public override int Dimension { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CohereEmbeddings"/> class.
        /// </summary>
        /// <param name="apiKey">The Cohere API key.</param>
        /// <param name="model">The model name to use.</param>
        /// <param name="dimension">The dimension of the embedding vectors.</param>
        /// <param name="apiUrl">The Cohere API endpoint URL (optional, defaults to official Cohere endpoint).</param>
        public CohereEmbeddings(string apiKey, string model = "embed-english-v3.0", int dimension = 1024, string? apiUrl = null)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            _model = model;
            Dimension = dimension;
            _apiUrl = apiUrl ?? GetDefaultApiUrl();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        private static string GetDefaultApiUrl()
        {
            // Use environment variable if available, otherwise use default endpoint
            return Environment.GetEnvironmentVariable("COHERE_API_URL") ?? "https://api.cohere.com/v1/embed";
        }

        /// <inheritdoc />
        public override async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        {
            try
            {
                // Cohere API rejects empty strings, so use a space instead
                var textToEmbed = string.IsNullOrEmpty(text) ? " " : text;

                var requestBody = new
                {
                    model = _model,
                    texts = new[] { textToEmbed },
                    input_type = "search_document",
                    truncate = "END"
                };
                var content = new StringContent(JsonSerializer.Serialize(requestBody));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await _httpClient.PostAsync(_apiUrl, content, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}). Error: {errorBody}");
                }
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(responseJson);
                var embeddings = doc.RootElement.GetProperty("embeddings")[0];
                var floats = new List<float>(Dimension);
                foreach (var value in embeddings.EnumerateArray())
                    floats.Add(value.GetSingle());
                return floats.ToArray();
            }
            catch (HttpRequestException ex)
            {
                throw new EmbeddingNetworkException(
                    $"Network error occurred while calling Cohere embeddings API: {ex.Message}",
                    ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new EmbeddingNetworkException(
                    "Request to Cohere embeddings API timed out",
                    ex);
            }
            catch (JsonException ex)
            {
                throw new EmbeddingInvalidResponseException(
                    $"Failed to parse Cohere API response: {ex.Message}",
                    ex);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EmbeddingInvalidResponseException(
                    "Cohere API response missing expected 'embeddings' property",
                    ex);
            }
            catch (EmbeddingException)
            {
                // Re-throw our own exceptions
                throw;
            }
            catch (Exception ex)
            {
                throw new EmbeddingException(
                    $"Unexpected error during Cohere embedding: {ex.Message}",
                    ex);
            }
        }

        /// <inheritdoc />
        public override async Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
        {
            try
            {
                // Cohere API rejects empty strings, so replace them with spaces
                var textsToEmbed = texts.Select(t => string.IsNullOrEmpty(t) ? " " : t).ToList();

                // Cohere API limits batch size to 96 texts per request
                const int maxBatchSize = 96;
                var allResults = new List<float[]>();

                // Process texts in batches
                for (int i = 0; i < textsToEmbed.Count; i += maxBatchSize)
                {
                    var batch = textsToEmbed.Skip(i).Take(maxBatchSize).ToList();
                    var batchResults = await ProcessBatchAsync(batch, cancellationToken);
                    allResults.AddRange(batchResults);
                }

                return allResults;
            }
            catch (HttpRequestException ex)
            {
                throw new EmbeddingNetworkException(
                    $"Network error occurred while calling Cohere embeddings API: {ex.Message}",
                    ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new EmbeddingNetworkException(
                    "Request to Cohere embeddings API timed out",
                    ex);
            }
            catch (JsonException ex)
            {
                throw new EmbeddingInvalidResponseException(
                    $"Failed to parse Cohere API response: {ex.Message}",
                    ex);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EmbeddingInvalidResponseException(
                    "Cohere API response missing expected 'embeddings' property",
                    ex);
            }
            catch (EmbeddingException)
            {
                // Re-throw our own exceptions
                throw;
            }
            catch (Exception ex)
            {
                throw new EmbeddingException(
                    $"Unexpected error during Cohere batch embedding: {ex.Message}",
                    ex);
            }
        }

        private async Task<IReadOnlyList<float[]>> ProcessBatchAsync(List<string> batch, CancellationToken cancellationToken)
        {
            var requestBody = new
            {
                model = _model,
                texts = batch,
                input_type = "search_document",
                truncate = "END"
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.PostAsync(_apiUrl, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}). Error: {errorBody}");
            }
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(responseJson);
            var embeddingsArray = doc.RootElement.GetProperty("embeddings");
            var results = new List<float[]>();
            foreach (var embedding in embeddingsArray.EnumerateArray())
            {
                var floats = new List<float>(Dimension);
                foreach (var value in embedding.EnumerateArray())
                    floats.Add(value.GetSingle());
                results.Add(floats.ToArray());
            }
            return results;
        }
    }
}
