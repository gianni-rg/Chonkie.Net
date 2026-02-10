using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Embeddings.Base;
using Chonkie.Embeddings.Exceptions;

namespace Chonkie.Embeddings.Gemini
{
    /// <summary>
    /// Embedding provider for Google Gemini API.
    /// </summary>
    public class GeminiEmbeddings : BaseEmbeddings
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly HttpClient _httpClient;

        /// <inheritdoc />
        public override string Name => "gemini";

        /// <inheritdoc />
        public override int Dimension { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeminiEmbeddings"/> class.
        /// </summary>
        /// <param name="apiKey">The Google Gemini API key.</param>
        /// <param name="model">The model name to use.</param>
        /// <param name="dimension">The dimension of the embedding vectors.</param>
        public GeminiEmbeddings(string apiKey, string model = "embedding-001", int dimension = 768)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _model = model;
            Dimension = dimension;
            _httpClient = new HttpClient();
        }

        /// <inheritdoc />
        public override async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        {
            try
            {
                var requestBody = new
                {
                    content = new { parts = new[] { new { text } } }
                };
                var content = new StringContent(JsonSerializer.Serialize(requestBody));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:embedContent?key={_apiKey}";
                var response = await _httpClient.PostAsync(url, content, cancellationToken);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(responseJson);
                var embedding = doc.RootElement.GetProperty("embedding").GetProperty("values");
                var floats = new List<float>(Dimension);
                foreach (var value in embedding.EnumerateArray())
                    floats.Add(value.GetSingle());
                return floats.ToArray();
            }
            catch (HttpRequestException ex)
            {
                throw new EmbeddingNetworkException(
                    $"Network error occurred while calling Gemini embeddings API: {ex.Message}",
                    ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new EmbeddingNetworkException(
                    "Request to Gemini embeddings API timed out",
                    ex);
            }
            catch (JsonException ex)
            {
                throw new EmbeddingInvalidResponseException(
                    $"Failed to parse Gemini API response: {ex.Message}",
                    ex);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EmbeddingInvalidResponseException(
                    "Gemini API response missing expected 'embedding' or 'values' property",
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
                    $"Unexpected error during Gemini embedding: {ex.Message}",
                    ex);
            }
        }

        /// <inheritdoc />
        public override async Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
        {
            try
            {
                var results = new List<float[]>();
                foreach (var text in texts)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    results.Add(await EmbedAsync(text, cancellationToken));
                }
                return results;
            }
            catch (EmbeddingException)
            {
                // Re-throw our own exceptions
                throw;
            }
            catch (Exception ex)
            {
                throw new EmbeddingException(
                    $"Unexpected error during Gemini batch embedding: {ex.Message}",
                    ex);
            }
        }
    }
}
