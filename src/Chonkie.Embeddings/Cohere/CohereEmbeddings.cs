using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Embeddings.Base;

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
        public override string Name => "cohere";
        public override int Dimension { get; }

        public CohereEmbeddings(string apiKey, string model = "embed-english-v3.0", int dimension = 1024)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _model = model;
            Dimension = dimension;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public override async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        {
            var requestBody = new
            {
                texts = new[] { text },
                model = _model,
                input_type = "search_document"
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.PostAsync("https://api.cohere.ai/v1/embed", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(responseJson);
            var embeddings = doc.RootElement.GetProperty("embeddings")[0];
            var floats = new List<float>(Dimension);
            foreach (var value in embeddings.EnumerateArray())
                floats.Add(value.GetSingle());
            return floats.ToArray();
        }

        public override async Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
        {
            var requestBody = new
            {
                texts = texts,
                model = _model,
                input_type = "search_document"
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.PostAsync("https://api.cohere.ai/v1/embed", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(responseJson);
            var embeddingsArray = doc.RootElement.GetProperty("embeddings");
            var results = new List<float[]>(embeddingsArray.GetArrayLength());
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