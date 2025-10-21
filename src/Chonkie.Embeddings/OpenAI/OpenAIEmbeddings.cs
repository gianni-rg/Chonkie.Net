using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Embeddings.Base;

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
        public override string Name => "openai";
        public override int Dimension { get; }

        public OpenAIEmbeddings(string apiKey, string model = "text-embedding-ada-002", int dimension = 1536)
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
                input = text,
                model = _model
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/embeddings", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(responseJson);
            var embedding = doc.RootElement.GetProperty("data")[0].GetProperty("embedding");
            var floats = new List<float>(Dimension);
            foreach (var value in embedding.EnumerateArray())
                floats.Add(value.GetSingle());
            return floats.ToArray();
        }

        public override async Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
        {
            var requestBody = new
            {
                input = texts,
                model = _model
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/embeddings", content, cancellationToken);
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
    }
}