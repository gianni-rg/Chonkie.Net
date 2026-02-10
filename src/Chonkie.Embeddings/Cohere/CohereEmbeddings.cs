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
        private readonly string _apiKey;
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
        public CohereEmbeddings(string apiKey, string model = "embed-english-v3.0", int dimension = 1024)
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

                var response = await _httpClient.PostAsync("https://api.cohere.com/v1/embed", content, cancellationToken);
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

                    var requestBody = new
                    {
                        model = _model,
                        texts = batch,
                        input_type = "search_document",
                        truncate = "END"
                    };
                    var content = new StringContent(JsonSerializer.Serialize(requestBody));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await _httpClient.PostAsync("https://api.cohere.com/v1/embed", content, cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                        throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}). Error: {errorBody}");
                    }
                    var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                    using var doc = JsonDocument.Parse(responseJson);
                    var embeddingsArray = doc.RootElement.GetProperty("embeddings");
                    foreach (var embedding in embeddingsArray.EnumerateArray())
                    {
                        var floats = new List<float>(Dimension);
                        foreach (var value in embedding.EnumerateArray())
                            floats.Add(value.GetSingle());
                        allResults.Add(floats.ToArray());
                    }
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
    }
}
