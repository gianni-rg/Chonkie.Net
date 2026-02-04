namespace Chonkie.Genies;

/// <summary>
/// Options for configuring a Genie instance.
/// </summary>
public class GenieOptions
{
    /// <summary>
    /// Gets or sets the API key for authentication.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model name to use for generation.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base endpoint URL for the API.
    /// </summary>
    public Uri? Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed requests.
    /// Default is 5.
    /// </summary>
    public int MaxRetries { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum retry delay in seconds.
    /// Default is 60 seconds.
    /// </summary>
    public int MaxRetryDelaySeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the timeout for requests in seconds.
    /// Default is 120 seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Gets or sets the temperature for generation (0.0 to 2.0).
    /// Default is 0.7.
    /// </summary>
    public float Temperature { get; set; } = 0.7f;

    /// <summary>
    /// Gets or sets the maximum number of tokens to generate.
    /// Default is 1024.
    /// </summary>
    public int MaxTokens { get; set; } = 1024;
}
