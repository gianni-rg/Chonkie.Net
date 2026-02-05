using System;
using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Chonkie.Genies;

/// <summary>
/// Azure OpenAI Genie for LLM generation using Azure-hosted GPT models.
/// Supports both text and structured JSON generation.
/// </summary>
/// <remarks>
/// Uses Azure OpenAI Service deployments.
/// Supports API key authentication and Azure Entra ID (managed identity).
/// Uses Microsoft.Extensions.AI for unified AI abstractions.
/// </remarks>
public class AzureOpenAIGenie : BaseGenie
{
    private const string DefaultApiVersion = "2024-10-21";

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureOpenAIGenie"/> class with API key authentication.
    /// </summary>
    /// <param name="endpoint">The Azure OpenAI endpoint (e.g., https://myresource.openai.azure.com).</param>
    /// <param name="apiKey">The Azure OpenAI API key.</param>
    /// <param name="deploymentName">The deployment name.</param>
    /// <param name="apiVersion">The API version (default: 2024-10-21).</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="ArgumentException">Thrown when parameters are empty or whitespace.</exception>
    public AzureOpenAIGenie(
        string endpoint,
        string apiKey,
        string deploymentName,
        string? apiVersion = null,
        ILogger<AzureOpenAIGenie>? logger = null)
        : base(
            ValidateAndCreateChatClient(endpoint, apiKey, deploymentName, apiVersion ?? DefaultApiVersion),
            new GenieOptions
            {
                ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey)),
                Model = deploymentName ?? throw new ArgumentNullException(nameof(deploymentName)),
                Endpoint = string.IsNullOrWhiteSpace(endpoint) ? null : new Uri(endpoint)
            },
            logger)
    {
    }

    private static IChatClient ValidateAndCreateChatClient(string endpoint, string apiKey, string deploymentName, string apiVersion)
    {
        // Validate parameters before attempting to create the client
        // This ensures ArgumentException is thrown instead of TypeLoadException from SDK initialization
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be empty or whitespace.", nameof(endpoint));

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be empty or whitespace.", nameof(apiKey));

        if (string.IsNullOrWhiteSpace(deploymentName))
            throw new ArgumentException("Deployment name cannot be empty or whitespace.", nameof(deploymentName));

        return CreateChatClient(endpoint, apiKey, deploymentName, apiVersion);
    }

    private static IChatClient CreateChatClient(string endpoint, string apiKey, string deploymentName, string apiVersion)
    {
        var azureClient = new AzureOpenAIClient(
            new Uri(endpoint),
            new Azure.AzureKeyCredential(apiKey));

        return azureClient.GetChatClient(deploymentName).AsIChatClient();
    }

    /// <summary>
    /// Creates an AzureOpenAIGenie instance from environment variables.
    /// Requires AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_API_KEY, and AZURE_OPENAI_DEPLOYMENT.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <returns>A configured AzureOpenAIGenie instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required environment variables are not set.</exception>
    public static AzureOpenAIGenie FromEnvironment(ILogger<AzureOpenAIGenie>? logger = null)
    {
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException(
                "AZURE_OPENAI_ENDPOINT environment variable is not set. " +
                "Please set it or use the constructor with an explicit endpoint.");
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "AZURE_OPENAI_API_KEY environment variable is not set. " +
                "Please set it or use the constructor with an explicit API key.");
        }

        if (string.IsNullOrWhiteSpace(deployment))
        {
            throw new InvalidOperationException(
                "AZURE_OPENAI_DEPLOYMENT environment variable is not set. " +
                "Please set it or use the constructor with an explicit deployment name.");
        }

        return new AzureOpenAIGenie(endpoint, apiKey, deployment, null, logger);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"AzureOpenAIGenie(deployment={Options.Model}, endpoint={Options.Endpoint})";
    }
}
