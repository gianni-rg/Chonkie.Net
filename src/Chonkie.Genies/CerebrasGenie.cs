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

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;

namespace Chonkie.Genies;

/// <summary>
/// Genie implementation for Cerebras's ultra-fast LLM inference API.
/// Cerebras provides the fastest inference speed available, optimized for production workloads.
/// </summary>
/// <example>
/// <code>
/// var apiKey = Environment.GetEnvironmentVariable("CEREBRAS_API_KEY");
/// var genie = new CerebrasGenie(apiKey);
/// 
/// // Generate text
/// var response = await genie.GenerateAsync("Explain machine learning");
/// 
/// // Generate structured JSON
/// var data = await genie.GenerateJsonAsync&lt;MyDataClass&gt;("Generate sample product data");
/// </code>
/// </example>
public class CerebrasGenie : BaseGenie
{
    private const string DefaultModel = "llama-3.3-70b";
    private const string DefaultEndpoint = "https://api.cerebras.ai/v1";

    /// <summary>
    /// Initializes a new instance of the <see cref="CerebrasGenie"/> class.
    /// </summary>
    /// <param name="apiKey">The Cerebras API key for authentication.</param>
    /// <param name="model">The model name to use. Defaults to llama-3.3-70b.</param>
    /// <param name="endpoint">The API endpoint URL. Defaults to Cerebras's standard endpoint.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    public CerebrasGenie(
        string apiKey,
        string? model,
        string? endpoint,
        ILogger<CerebrasGenie>? logger = null)
        : base(
            CreateChatClient(apiKey, model ?? DefaultModel, endpoint ?? DefaultEndpoint),
            new GenieOptions
            {
                ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey)),
                Model = model ?? DefaultModel,
                Endpoint = new Uri(endpoint ?? DefaultEndpoint)
            },
            logger)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CerebrasGenie"/> class with custom options.
    /// </summary>
    /// <param name="options">Configuration options for the Genie.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    public CerebrasGenie(GenieOptions options, ILogger<CerebrasGenie>? logger = null)
        : base(
            CreateChatClient(
                options?.ApiKey ?? throw new ArgumentNullException(nameof(options)),
                options.Model,
                options.Endpoint?.ToString() ?? DefaultEndpoint),
            options,
            logger)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new ArgumentException("API key cannot be null or empty", nameof(options));
        }
    }

    /// <summary>
    /// Creates a CerebrasGenie instance using the CEREBRAS_API_KEY environment variable.
    /// </summary>
    /// <param name="model">The model name to use. Defaults to llama-3.3-70b.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A configured CerebrasGenie instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the CEREBRAS_API_KEY environment variable is not set.</exception>
    public static CerebrasGenie FromEnvironment(string? model = null, ILogger<CerebrasGenie>? logger = null)
    {
        var apiKey = Environment.GetEnvironmentVariable("CEREBRAS_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "CEREBRAS_API_KEY environment variable is not set. " +
                "Please set it to your Cerebras API key.");
        }

        return new CerebrasGenie(apiKey, model, null, logger);
    }

    private static IChatClient CreateChatClient(string apiKey, string model, string endpoint)
    {
        // Cerebras is OpenAI-compatible, so we use the OpenAI client
        var openAIClient = new OpenAI.OpenAIClient(
            credential: new System.ClientModel.ApiKeyCredential(apiKey),
            options: new OpenAI.OpenAIClientOptions
            {
                Endpoint = new Uri(endpoint)
            });

        return openAIClient.GetChatClient(model).AsIChatClient();
    }
}
