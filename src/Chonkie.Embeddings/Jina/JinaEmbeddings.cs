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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Embeddings.Base;
using Chonkie.Embeddings.Exceptions;

namespace Chonkie.Embeddings.Jina
{
    /// <summary>
    /// Embedding provider for Jina AI API.
    /// </summary>
    public class JinaEmbeddings : BaseEmbeddings
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly HttpClient _httpClient;

        /// <inheritdoc />
        public override string Name => "jina";

        /// <inheritdoc />
        public override int Dimension { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JinaEmbeddings"/> class.
        /// </summary>
        /// <param name="apiKey">The Jina AI API key.</param>
        /// <param name="model">The model name to use.</param>
        /// <param name="dimension">The dimension of the embedding vectors.</param>
        public JinaEmbeddings(string apiKey, string model = "jina-embeddings-v2-base-en", int dimension = 768)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _model = model;
            Dimension = dimension;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        /// <inheritdoc />
        public override async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        {
            try
            {
                var requestBody = new
                {
                    input = new[] { text },
                    model = _model
                };
                var content = new StringContent(JsonSerializer.Serialize(requestBody));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await _httpClient.PostAsync("https://api.jina.ai/v1/embeddings", content, cancellationToken);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(responseJson);
                var embedding = doc.RootElement.GetProperty("data")[0].GetProperty("embedding");
                var floats = new List<float>(Dimension);
                foreach (var value in embedding.EnumerateArray())
                    floats.Add(value.GetSingle());
                return floats.ToArray();
            }
            catch (HttpRequestException ex)
            {
                throw new EmbeddingNetworkException(
                    $"Network error occurred while calling Jina AI embeddings API: {ex.Message}",
                    ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new EmbeddingNetworkException(
                    "Request to Jina AI embeddings API timed out",
                    ex);
            }
            catch (JsonException ex)
            {
                throw new EmbeddingInvalidResponseException(
                    $"Failed to parse Jina AI API response: {ex.Message}",
                    ex);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EmbeddingInvalidResponseException(
                    "Jina AI API response missing expected 'data' or 'embedding' property",
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
                    $"Unexpected error during Jina AI embedding: {ex.Message}",
                    ex);
            }
        }

        /// <inheritdoc />
        public override async Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
        {
            try
            {
                var requestBody = new
                {
                    input = texts,
                    model = _model
                };
                var content = new StringContent(JsonSerializer.Serialize(requestBody));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await _httpClient.PostAsync("https://api.jina.ai/v1/embeddings", content, cancellationToken);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(responseJson);
                var data = doc.RootElement.GetProperty("data");
                var results = new List<float[]>(data.GetArrayLength());
                foreach (var item in data.EnumerateArray())
                {
                    var embedding = item.GetProperty("embedding");
                    var floats = new List<float>(Dimension);
                    foreach (var value in embedding.EnumerateArray())
                        floats.Add(value.GetSingle());
                    results.Add(floats.ToArray());
                }
                return results;
            }
            catch (HttpRequestException ex)
            {
                throw new EmbeddingNetworkException(
                    $"Network error occurred while calling Jina AI embeddings API: {ex.Message}",
                    ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new EmbeddingNetworkException(
                    "Request to Jina AI embeddings API timed out",
                    ex);
            }
            catch (JsonException ex)
            {
                throw new EmbeddingInvalidResponseException(
                    $"Failed to parse Jina AI API response: {ex.Message}",
                    ex);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EmbeddingInvalidResponseException(
                    "Jina AI API response missing expected 'data' or 'embedding' property",
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
                    $"Unexpected error during Jina AI batch embedding: {ex.Message}",
                    ex);
            }
        }
    }
}
