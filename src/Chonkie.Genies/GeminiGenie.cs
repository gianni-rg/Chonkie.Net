using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Chonkie.Genies;

/// <summary>
/// Google Gemini Genie for LLM generation using Gemini models.
/// Supports both text and structured JSON generation.
/// </summary>
/// <remarks>
/// Default model: gemini-2.0-flash-exp (fastest Gemini 2.0 model).
/// Supports gemini-2.0-flash-exp, gemini-1.5-pro, gemini-1.5-flash, and other Gemini models.
/// Uses Google AI REST API through Microsoft.Extensions.AI abstractions.
/// </remarks>
public class GeminiGenie : BaseGenie
{
    private const string DefaultModel = "gemini-2.0-flash-exp";

    /// <summary>
    /// Initializes a new instance of the <see cref="GeminiGenie"/> class.
    /// </summary>
    /// <param name="apiKey">The Gemini API key.</param>
    /// <param name="model">The model to use (default: gemini-2.0-flash-exp).</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when apiKey is null.</exception>
    /// <exception cref="ArgumentException">Thrown when apiKey is empty or whitespace.</exception>
    public GeminiGenie(
        string apiKey,
        string? model = null,
        ILogger<GeminiGenie>? logger = null)
        : base(
            CreateChatClient(apiKey, model ?? DefaultModel),
            new GenieOptions
            {
                ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey)),
                Model = model ?? DefaultModel
            },
            logger)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be empty or whitespace.", nameof(apiKey));
    }

    private static IChatClient CreateChatClient(string apiKey, string model)
    {
        var httpClient = new HttpClient();
        return new GeminiChatClient(apiKey, model, httpClient);
    }

    /// <summary>
    /// Creates a GeminiGenie instance from environment variable GEMINI_API_KEY.
    /// </summary>
    /// <param name="model">Optional model name (default: gemini-2.0-flash-exp).</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <returns>A configured GeminiGenie instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when GEMINI_API_KEY environment variable is not set.</exception>
    public static GeminiGenie FromEnvironment(string? model = null, ILogger<GeminiGenie>? logger = null)
    {
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "GEMINI_API_KEY environment variable is not set. " +
                "Please set it or use the constructor with an explicit API key.");
        }

        return new GeminiGenie(apiKey, model, logger);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"GeminiGenie(model={Options.Model})";
    }

    /// <summary>
    /// Custom IChatClient implementation for Gemini API.
    /// This is a temporary wrapper until Microsoft.Extensions.AI adds native Gemini support.
    /// </summary>
    private class GeminiChatClient : IChatClient
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly HttpClient _httpClient;

        public GeminiChatClient(string apiKey, string model, HttpClient httpClient)
        {
            _apiKey = apiKey;
            _model = model;
            _httpClient = httpClient;
        }

        public ChatClientMetadata Metadata => new("gemini", new Uri("https://ai.google.dev"), _model);

        public async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> chatMessages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            // Build Gemini request format
            var messageList = chatMessages.ToList();
            var parts = messageList.Select(m => new { text = m.Text }).ToArray();
            var requestBody = new { contents = new[] { new { parts } } };

            var content = new StringContent(JsonSerializer.Serialize(requestBody));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(responseJson);

            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";

            return new ChatResponse(new ChatMessage(ChatRole.Assistant, text));
        }

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> chatMessages,
            ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Simplified: return single chunk (Gemini streaming requires SSE support)
            var response = await GetResponseAsync(chatMessages, options, cancellationToken);
            var message = response.Messages.Single();
            yield return new ChatResponseUpdate(ChatRole.Assistant, message.Text);
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public TService? GetService<TService>(object? key = null) where TService : class => null;

        public void Dispose() { }
    }
}
