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
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Text.Json;

namespace Chonkie.Genies;

/// <summary>
/// Base class for all Genie implementations.
/// Provides common functionality including retry logic, error handling, and JSON parsing.
/// </summary>
public abstract class BaseGenie : IGeneration
{
    private readonly IChatClient _chatClient;
    private readonly ILogger _logger;
    private readonly GenieOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseGenie"/> class.
    /// </summary>
    /// <param name="chatClient">The chat client for LLM interactions.</param>
    /// <param name="options">Configuration options for the Genie.</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    protected BaseGenie(IChatClient chatClient, GenieOptions options, ILogger? logger = null)
    {
        _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Gets the configured options for this Genie.
    /// </summary>
    protected GenieOptions Options => _options;

    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    protected ILogger Logger => _logger;

    /// <inheritdoc/>
    public virtual async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentNullException(nameof(prompt), "Prompt cannot be null or empty");
        }

        return await ExecuteWithRetryAsync(
            async ct =>
            {
                var messages = new List<ChatMessage>
                {
                    new(ChatRole.User, prompt)
                };

                var chatOptions = new ChatOptions
                {
                    Temperature = _options.Temperature,
                    MaxOutputTokens = _options.MaxTokens
                };

                var response = await _chatClient.GetResponseAsync(messages, chatOptions, ct);

                if (string.IsNullOrWhiteSpace(response?.Text))
                {
                    throw new GenieException("Received null or empty response from the model");
                }

                return response.Text;
            },
            cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<T> GenerateJsonAsync<T>(string prompt, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentNullException(nameof(prompt), "Prompt cannot be null or empty");
        }

        return await ExecuteWithRetryAsync(
            async ct =>
            {
                // Enhance prompt with JSON instruction
                var jsonPrompt = $"{prompt}\n\nRespond with valid JSON only.";

                var messages = new List<ChatMessage>
                {
                    new(ChatRole.User, jsonPrompt)
                };

                var chatOptions = new ChatOptions
                {
                    Temperature = _options.Temperature,
                    MaxOutputTokens = _options.MaxTokens,
                    ResponseFormat = ChatResponseFormat.Json
                };

                var response = await _chatClient.GetResponseAsync(messages, chatOptions, ct);

                if (string.IsNullOrWhiteSpace(response?.Text))
                {
                    throw new GenieException("Received null or empty response from the model");
                }

                try
                {
                    var result = JsonSerializer.Deserialize<T>(response.Text, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result is null)
                    {
                        throw new JsonParsingException($"Failed to deserialize JSON response to type {typeof(T).Name}");
                    }

                    return result;
                }
                catch (JsonException ex)
                {
                    throw new JsonParsingException($"Failed to parse JSON response: {ex.Message}", ex);
                }
            },
            cancellationToken);
    }

    /// <summary>
    /// Executes an operation with exponential backoff retry logic.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The result of the operation.</returns>
    protected async Task<T> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        var attempt = 0;
        var delay = TimeSpan.FromSeconds(1);

        while (true)
        {
            attempt++;

            try
            {
                return await operation(cancellationToken);
            }
            catch (HttpRequestException ex) when (attempt < _options.MaxRetries && ShouldRetry(ex))
            {
                _logger.LogWarning(ex, "Request failed on attempt {Attempt}/{MaxRetries}. Retrying after {Delay}s...",
                    attempt, _options.MaxRetries, delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);

                // Exponential backoff with max cap
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, _options.MaxRetryDelaySeconds));
            }
            catch (HttpRequestException ex) when (IsRateLimitError(ex))
            {
                throw new RateLimitException("API rate limit exceeded", ex);
            }
            catch (HttpRequestException ex) when (IsAuthenticationError(ex))
            {
                throw new AuthenticationException("Authentication failed. Please check your API key", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new GenieException($"HTTP request failed: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new GenieException("Request timed out", ex);
            }
        }
    }

    /// <summary>
    /// Determines if an HTTP exception should trigger a retry.
    /// </summary>
    private static bool ShouldRetry(HttpRequestException ex)
    {
        return ex.StatusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;
    }

    /// <summary>
    /// Determines if an HTTP exception is a rate limit error.
    /// </summary>
    private static bool IsRateLimitError(HttpRequestException ex)
    {
        return ex.StatusCode is HttpStatusCode.TooManyRequests;
    }

    /// <summary>
    /// Determines if an HTTP exception is an authentication error.
    /// </summary>
    private static bool IsAuthenticationError(HttpRequestException ex)
    {
        return ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;
    }
}
