using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Embeddings.Base;
using Chonkie.Embeddings.Exceptions;

namespace Chonkie.Embeddings.OpenAI
{
    /// <summary>
    /// Embedding provider for OpenAI API.
    /// </summary>
    public class OpenAIEmbeddings : BaseEmbeddings
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly HttpClient _httpClient;

        /// <inheritdoc />
        public override string Name => "openai";

        /// <inheritdoc />
        public override int Dimension { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAIEmbeddings"/> class.
        /// </summary>
        /// <param name="apiKey">The OpenAI API key.</param>
        /// <param name="model">The model name to use.</param>
        /// <param name="dimension">The dimension of the embedding vectors.</param>
        public OpenAIEmbeddings(string apiKey, string model = "text-embedding-ada-002", int dimension = 1536)
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
                    input = text,
                    model = _model
                };
                var content = new StringContent(JsonSerializer.Serialize(requestBody));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/embeddings", content, cancellationToken);

                await HandleHttpResponseAsync(response);

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
                    $"Network error occurred while calling OpenAI embeddings API: {ex.Message}",
                    ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new EmbeddingNetworkException(
                    "Request to OpenAI embeddings API timed out",
                    ex);
            }
            catch (JsonException ex)
            {
                throw new EmbeddingInvalidResponseException(
                    $"Failed to parse OpenAI API response: {ex.Message}",
                    ex);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EmbeddingInvalidResponseException(
                    "OpenAI API response missing expected 'data' or 'embedding' property",
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
                    $"Unexpected error during OpenAI embedding: {ex.Message}",
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

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/embeddings", content, cancellationToken);

                await HandleHttpResponseAsync(response);

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
                    $"Network error occurred while calling OpenAI embeddings API: {ex.Message}",
                    ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new EmbeddingNetworkException(
                    "Request to OpenAI embeddings API timed out",
                    ex);
            }
            catch (JsonException ex)
            {
                throw new EmbeddingInvalidResponseException(
                    $"Failed to parse OpenAI API response: {ex.Message}",
                    ex);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EmbeddingInvalidResponseException(
                    "OpenAI API response missing expected 'data' or 'embedding' property",
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
                    $"Unexpected error during OpenAI batch embedding: {ex.Message}",
                    ex);
            }
        }

        private static async Task HandleHttpResponseAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            var statusCode = (int)response.StatusCode;
            var responseContent = await response.Content.ReadAsStringAsync();

            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    throw new EmbeddingAuthenticationException(
                        $"Authentication failed with OpenAI API (HTTP {statusCode}): {responseContent}");

                case HttpStatusCode.TooManyRequests:
                    // Try to extract retry-after header
                    int? retryAfter = null;
                    if (response.Headers.RetryAfter?.Delta.HasValue == true)
                    {
                        retryAfter = (int)response.Headers.RetryAfter.Delta.Value.TotalSeconds;
                    }
                    throw new EmbeddingRateLimitException(
                        $"Rate limit exceeded for OpenAI API (HTTP {statusCode}). {responseContent}",
                        retryAfter);

                case HttpStatusCode.BadRequest:
                    throw new EmbeddingInvalidResponseException(
                        $"Bad request to OpenAI API (HTTP {statusCode}): {responseContent}",
                        statusCode);

                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.GatewayTimeout:
                case HttpStatusCode.BadGateway:
                    throw new EmbeddingNetworkException(
                        $"OpenAI API service unavailable (HTTP {statusCode}): {responseContent}");

                default:
                    throw new EmbeddingException(
                        $"OpenAI API request failed (HTTP {statusCode}): {responseContent}");
            }
        }
    }
}

