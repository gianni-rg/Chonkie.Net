namespace Chonkie.Genies;

/// <summary>
/// Configuration options for Genie instances (LLM services).
/// Controls authentication, model selection, and behavior parameters.
/// </summary>
/// <remarks>
/// This class is used to configure how a Genie instance communicates with language model APIs.
/// Different Genie implementations (GroqGenie, OpenAIGenie, etc.) use these options to initialize
/// their underlying chat clients and control generation behavior.
/// </remarks>
/// <example>
/// <code>
/// var options = new GenieOptions
/// {
///     ApiKey = "sk-...",
///     Model = "gpt-4o",
///     Temperature = 0.7f,
///     MaxTokens = 2048,
///     MaxRetries = 3
/// };
/// var genie = new OpenAIGenie(options);
/// </code>
/// </example>
public class GenieOptions
{
    /// <summary>
    /// Gets or sets the API key for authentication.
    /// Required for all Genie implementations.
    /// </summary>
    /// <remarks>
    /// This should be loaded from secure configuration (environment variables, Azure Key Vault, etc.)
    /// and never hard-coded. Check the specific Genie implementation for the expected environment variable name.
    /// </remarks>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model name to use for generation.
    /// Examples: "gpt-4o" (OpenAI), "llama-3.3-70b-versatile" (Groq), "gemini-2.0-flash-exp" (Google).
    /// </summary>
    /// <remarks>
    /// The model name must be supported by the Genie implementation being used.
    /// Check the service provider's documentation for available models and their capabilities.
    /// </remarks>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base endpoint URL for the API.
    /// Optional for services with public endpoints; required for self-hosted or custom endpoints.
    /// </summary>
    /// <remarks>
    /// For services like OpenAI, this defaults to their public endpoint (https://api.openai.com/v1).
    /// For self-hosted or OpenAI-compatible services, set this to your custom endpoint.
    /// </remarks>
    public Uri? Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed requests.
    /// Default is 5.
    /// </summary>
    /// <remarks>
    /// Retries use exponential backoff: 2s, 4s, 8s, 16s, 32s (up to MaxRetryDelaySeconds).
    /// Useful for handling temporary API failures or rate limiting. Set to 0 to disable retries.
    /// </remarks>
    public int MaxRetries { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum retry delay in seconds.
    /// Default is 60 seconds.
    /// </summary>
    /// <remarks>
    /// Combined with MaxRetries to cap how long retry backoff can wait.
    /// For example, with MaxRetries=5 and MaxRetryDelaySeconds=60, the backoff sequence
    /// will be: 2s, 4s, 8s, 16s, 32s (4th retry is capped at 60s).
    /// </remarks>
    public int MaxRetryDelaySeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the timeout for requests in seconds.
    /// Default is 120 seconds.
    /// </summary>
    /// <remarks>
    /// Applied to individual HTTP requests. Increase for complex prompts or large context windows.
    /// Some advanced models may require longer timeouts (5-10 minutes) for complex reasoning.
    /// </remarks>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Gets or sets the temperature for generation (0.0 to 2.0).
    /// Default is 0.7.
    /// </summary>
    /// <remarks>
    /// Temperature controls randomness in responses:
    /// - 0.0: Deterministic, always the same output
    /// - 0.7: Balanced (default, recommended for most use cases)
    /// - 1.5+: Very creative, may be unrelated or nonsensical
    /// For factual/structured tasks, use lower values (0.0-0.3).
    /// For creative tasks, use higher values (0.8-1.5).
    /// </remarks>
    public float Temperature { get; set; } = 0.7f;

    /// <summary>
    /// Gets or sets the maximum number of tokens to generate.
    /// Default is 1024.
    /// </summary>
    /// <remarks>
    /// Tokens are typically ~4 characters of English text. For most use cases, 256-2048 is appropriate.
    /// Longer generations may result in higher costs and slower responses.
    /// The actual maximum depends on the model (GPT-4 supports up to 8192 tokens in total context).
    /// </remarks>
    public int MaxTokens { get; set; } = 1024;
}
