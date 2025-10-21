using System;
using System.Linq;
using System.Threading.Tasks;
using Chonkie.Embeddings.Cohere;
using Xunit;

namespace Chonkie.Embeddings.Integration.Tests;

/// <summary>
/// Integration tests for Cohere embeddings provider
/// These tests require COHERE_API_KEY environment variable to be set
/// </summary>
public class CohereEmbeddingsIntegrationTests
{
    private const string ApiKeyEnvVar = "COHERE_API_KEY";

    [SkippableFact]
    public async Task EmbedAsync_WithRealAPI_ReturnsValidEmbedding()
    {
        // Arrange
        var apiKey = TestHelpers.GetEnvironmentVariableOrSkip(ApiKeyEnvVar);
        var embeddings = new CohereEmbeddings(apiKey: apiKey);
        var text = "This is a test sentence for embedding.";

        // Act
        var result = await embeddings.EmbedAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        Assert.All(result, value => Assert.InRange(value, -1f, 1f));
    }

    [SkippableFact]
    public async Task EmbedAsync_WithCustomModel_ReturnsValidEmbedding()
    {
        // Arrange
        var apiKey = TestHelpers.GetEnvironmentVariableOrSkip(ApiKeyEnvVar);
        var embeddings = new CohereEmbeddings(apiKey: apiKey, model: "embed-english-light-v3.0");
        var text = "Testing custom model.";

        // Act
        var result = await embeddings.EmbedAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        Assert.All(result, value => Assert.InRange(value, -1f, 1f));
    }

    [SkippableFact]
    public async Task EmbedBatchAsync_WithMultipleTexts_ReturnsValidEmbeddings()
    {
        // Arrange
        var apiKey = TestHelpers.GetEnvironmentVariableOrSkip(ApiKeyEnvVar);
        var embeddings = new CohereEmbeddings(apiKey: apiKey);
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

    [SkippableFact]
    public async Task EmbedAsync_SimilarTexts_ProduceSimilarEmbeddings()
    {
        // Arrange
        var apiKey = TestHelpers.GetEnvironmentVariableOrSkip(ApiKeyEnvVar);
        var embeddings = new CohereEmbeddings(apiKey: apiKey);
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

    [SkippableFact]
    public async Task EmbedAsync_LongText_ReturnsValidEmbedding()
    {
        // Arrange
        var apiKey = TestHelpers.GetEnvironmentVariableOrSkip(ApiKeyEnvVar);
        var embeddings = new CohereEmbeddings(apiKey: apiKey);
        var longText = string.Join(" ", Enumerable.Repeat("This is a test sentence.", 100));

        // Act
        var result = await embeddings.EmbedAsync(longText);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        Assert.All(result, value => Assert.InRange(value, -1f, 1f));
    }
}
