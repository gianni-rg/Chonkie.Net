using System;
using System.Threading.Tasks;
using Chonkie.Embeddings.Azure;
using Xunit;

namespace Chonkie.Embeddings.Integration.Tests;

/// <summary>
/// Integration tests for Azure OpenAI embeddings provider.
/// These tests require AZURE_OPENAI_API_KEY and AZURE_OPENAI_ENDPOINT environment variables to be set.
/// </summary>
public class AzureOpenAIEmbeddingsIntegrationTests
{
    private const string ApiKeyEnvVar = "AZURE_OPENAI_API_KEY";
    private const string EndpointEnvVar = "AZURE_OPENAI_ENDPOINT";
    private const string DeploymentEnvVar = "AZURE_OPENAI_DEPLOYMENT";

    /// <summary>
    /// Tests embedding with real API returns a valid embedding.
    /// </summary>
    [Fact]
    public async Task EmbedAsync_WithRealAPI_ReturnsValidEmbedding()
    {
        // Arrange
        var config = TestHelpers.GetEnvironmentVariablesOrSkip(ApiKeyEnvVar, EndpointEnvVar, DeploymentEnvVar);
        var embeddings = new AzureOpenAIEmbeddings(
            endpoint: config[EndpointEnvVar],
            apiKey: config[ApiKeyEnvVar],
            deploymentName: config[DeploymentEnvVar]);
        var text = "This is a test sentence for embedding.";

        // Act
        var result = await embeddings.EmbedAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        Assert.All(result, value => Assert.InRange(value, -1f, 1f));
    }

    /// <summary>
    /// Tests batch embedding with multiple texts returns valid embeddings.
    /// </summary>
    [Fact]
    public async Task EmbedBatchAsync_WithMultipleTexts_ReturnsValidEmbeddings()
    {
        // Arrange
        var config = TestHelpers.GetEnvironmentVariablesOrSkip(ApiKeyEnvVar, EndpointEnvVar, DeploymentEnvVar);
        var embeddings = new AzureOpenAIEmbeddings(
            endpoint: config[EndpointEnvVar],
            apiKey: config[ApiKeyEnvVar],
            deploymentName: config[DeploymentEnvVar]);
        var texts = new[] { "First sentence.", "Second sentence.", "Third sentence." };

        // Act
        var results = await embeddings.EmbedBatchAsync(texts);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count);
        Assert.All(results, embedding =>
        {
            Assert.True(embedding.Length > 0);
            Assert.All(embedding, value => Assert.InRange(value, -1f, 1f));
        });
    }

    /// <summary>
    /// Tests that similar texts produce similar embeddings.
    /// </summary>
    [Fact]
    public async Task EmbedAsync_SimilarTexts_ProduceSimilarEmbeddings()
    {
        // Arrange
        var config = TestHelpers.GetEnvironmentVariablesOrSkip(ApiKeyEnvVar, EndpointEnvVar, DeploymentEnvVar);
        var embeddings = new AzureOpenAIEmbeddings(
            endpoint: config[EndpointEnvVar],
            apiKey: config[ApiKeyEnvVar],
            deploymentName: config[DeploymentEnvVar]);
        var text1 = "The cat sits on the mat.";
        var text2 = "A cat is sitting on a mat.";
        var text3 = "The weather is sunny today.";

        // Act
        var embedding1 = await embeddings.EmbedAsync(text1);
        var embedding2 = await embeddings.EmbedAsync(text2);
        var embedding3 = await embeddings.EmbedAsync(text3);

        var similarity12 = embeddings.Similarity(embedding1, embedding2);
        var similarity13 = embeddings.Similarity(embedding1, embedding3);

        // Assert
        Assert.True(similarity12 > similarity13,
            $"Similar texts should have higher similarity. Got {similarity12} vs {similarity13}");
        Assert.InRange(similarity12, 0.7f, 1.0f);
    }

    /// <summary>
    /// Tests embedding with empty string returns a valid embedding.
    /// </summary>
    [Fact]
    public async Task EmbedAsync_EmptyString_ReturnsValidEmbedding()
    {
        // Arrange
        var config = TestHelpers.GetEnvironmentVariablesOrSkip(ApiKeyEnvVar, EndpointEnvVar, DeploymentEnvVar);
        var embeddings = new AzureOpenAIEmbeddings(
            endpoint: config[EndpointEnvVar],
            apiKey: config[ApiKeyEnvVar],
            deploymentName: config[DeploymentEnvVar]);

        // Act
        var result = await embeddings.EmbedAsync(string.Empty);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    /// <summary>
    /// Tests that large batch embedding handles chunking properly.
    /// </summary>
    [Fact]
    public async Task EmbedBatchAsync_LargeBatch_HandlesChunking()
    {
        // Arrange
        var config = TestHelpers.GetEnvironmentVariablesOrSkip(ApiKeyEnvVar, EndpointEnvVar, DeploymentEnvVar);
        var embeddings = new AzureOpenAIEmbeddings(
            endpoint: config[EndpointEnvVar],
            apiKey: config[ApiKeyEnvVar],
            deploymentName: config[DeploymentEnvVar]);

        // Create a batch to test batch processing
        var texts = Enumerable.Range(0, 50)
            .Select(i => $"Test sentence number {i}.")
            .ToArray();

        // Act
        var results = await embeddings.EmbedBatchAsync(texts);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(50, results.Count);
        Assert.All(results, embedding =>
        {
            Assert.True(embedding.Length > 0);
            Assert.All(embedding, value => Assert.InRange(value, -1f, 1f));
        });
    }
}

