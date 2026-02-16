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
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;

namespace Chonkie.Genies;

/// <summary>
/// OpenAI Genie for LLM generation using GPT models.
/// Supports both text and structured JSON generation.
/// </summary>
/// <remarks>
/// Default model: gpt-4o (fastest GPT-4 class model with vision).
/// Supports gpt-4o, gpt-4-turbo, gpt-3.5-turbo, and other ChatGPT models.
/// Uses Microsoft.Extensions.AI for unified AI abstractions.
/// </remarks>
public class OpenAIGenie : BaseGenie
{
    private const string DefaultModel = "gpt-4o";

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenAIGenie"/> class.
    /// </summary>
    /// <param name="apiKey">The OpenAI API key.</param>
    /// <param name="model">The model to use (default: gpt-4o).</param>
    /// <param name="baseUrl">Optional custom base URL (for OpenAI-compatible endpoints).</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when apiKey is null.</exception>
    /// <exception cref="ArgumentException">Thrown when apiKey is empty or whitespace.</exception>
    public OpenAIGenie(
        string apiKey,
        string? model = null,
        string? baseUrl = null,
        ILogger<OpenAIGenie>? logger = null)
        : base(
            CreateChatClient(apiKey, model ?? DefaultModel, baseUrl),
            new GenieOptions
            {
                ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey)),
                Model = model ?? DefaultModel,
                Endpoint = string.IsNullOrWhiteSpace(baseUrl) ? null : new Uri(baseUrl)
            },
            logger)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("API key cannot be empty or whitespace.", nameof(apiKey));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenAIGenie"/> class using configuration options.
    /// </summary>
    /// <param name="options">The genie configuration options.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when options is null or options.ApiKey is null.</exception>
    /// <exception cref="ArgumentException">Thrown when options.ApiKey is empty or whitespace.</exception>
    public OpenAIGenie(GenieOptions options, ILogger<OpenAIGenie>? logger = null)
        : base(
            CreateChatClient(
                options?.ApiKey ?? throw new ArgumentNullException(nameof(options)),
                options.Model ?? DefaultModel,
                options.Endpoint?.ToString()),
            options,
            logger)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new ArgumentException("API key cannot be empty or whitespace.", nameof(options));
        }
    }

    /// <summary>
    /// Creates an OpenAIGenie instance from environment variable OPENAI_API_KEY.
    /// </summary>
    /// <param name="model">Optional model name (default: gpt-4o).</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <returns>A configured OpenAIGenie instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when OPENAI_API_KEY environment variable is not set.</exception>
    public static OpenAIGenie FromEnvironment(string? model = null, ILogger<OpenAIGenie>? logger = null)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "OPENAI_API_KEY environment variable is not set. " +
                "Please set it or use the constructor with an explicit API key.");
        }

        return new OpenAIGenie(apiKey, model, null, logger);
    }

    private static IChatClient CreateChatClient(string apiKey, string model, string? baseUrl)
    {
        var credential = new System.ClientModel.ApiKeyCredential(apiKey);
        var client = string.IsNullOrWhiteSpace(baseUrl)
            ? new OpenAIClient(credential)
            : new OpenAIClient(credential, new OpenAIClientOptions { Endpoint = new Uri(baseUrl) });

        return client.GetChatClient(model).AsIChatClient();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var model = Options.Model ?? DefaultModel;
        var endpoint = Options.Endpoint?.ToString() ?? "api.openai.com";
        return $"OpenAIGenie(model={model}, endpoint={endpoint})";
    }
}
