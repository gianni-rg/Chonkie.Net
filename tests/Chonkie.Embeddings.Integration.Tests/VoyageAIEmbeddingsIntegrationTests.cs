using System;
using System.Linq;
using System.Threading.Tasks;
using Chonkie.Embeddings.VoyageAI;
using Xunit;

namespace Chonkie.Embeddings.Integration.Tests;

/// <summary>
/// Integration tests for Voyage AI embeddings provider
/// These tests require VOYAGE_API_KEY environment variable to be set
/// </summary>
public class VoyageAIEmbeddingsIntegrationTests
{
    private const string ApiKeyEnvVar = "VOYAGE_API_KEY";

    /// <summary>
    /// Tests embedding with real API returns a valid embedding.
    /// </summary>
    
    public async Task EmbedAsync_WithRealAPI_ReturnsValidEmbedding()
    {
        // Arrange
        var apiKey = TestHelpers.GetEnvironmentVariableOrSkip(ApiKeyEnvVar);
        var embeddings = new VoyageAIEmbeddings(apiKey: apiKey);
        var text = "This is a test sentence for embedding.";

        // Act
        var result = await embeddings.EmbedAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        Assert.All(result, value => Assert.InRange(value, -1f, 1f));
    }

    /// <summary>
    /// Tests embedding with custom model returns a valid embedding.
    /// </summary>
    
    public async Task EmbedAsync_WithCustomModel_ReturnsValidEmbedding()
    {
        // Arrange
        var apiKey = TestHelpers.GetEnvironmentVariableOrSkip(ApiKeyEnvVar);
        var embeddings = new VoyageAIEmbeddings(apiKey: apiKey, model: "voyage-2");
        var text = "Testing custom model.";

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
    
    public async Task EmbedBatchAsync_WithMultipleTexts_ReturnsValidEmbeddings()
    {
        // Arrange
        var apiKey = TestHelpers.GetEnvironmentVariableOrSkip(ApiKeyEnvVar);
        var embeddings = new VoyageAIEmbeddings(apiKey: apiKey);
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
    
    public async Task EmbedAsync_SimilarTexts_ProduceSimilarEmbeddings()
    {
        // Arrange
        var apiKey = TestHelpers.GetEnvironmentVariableOrSkip(ApiKeyEnvVar);
        var embeddings = new VoyageAIEmbeddings(apiKey: apiKey);
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
    /// Tests embedding with long text returns a valid embedding.
    /// </summary>
    
    public async Task EmbedAsync_LongText_ReturnsValidEmbedding()
    {
        // Arrange
        var apiKey = TestHelpers.GetEnvironmentVariableOrSkip(ApiKeyEnvVar);
        var embeddings = new VoyageAIEmbeddings(apiKey: apiKey);
        var longText = string.Join(" ", Enumerable.Repeat("This is a test sentence.", 100));

        // Act
        var result = await embeddings.EmbedAsync(longText);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        Assert.All(result, value => Assert.InRange(value, -1f, 1f));
    }

    /// <summary>
    /// Tests embedding with empty string returns a valid embedding.
    /// </summary>
    
    public async Task EmbedAsync_EmptyString_ReturnsValidEmbedding()
    {
        // Arrange
        var apiKey = TestHelpers.GetEnvironmentVariableOrSkip(ApiKeyEnvVar);
        var embeddings = new VoyageAIEmbeddings(apiKey: apiKey);

        // Act
        var result = await embeddings.EmbedAsync(string.Empty);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    /// <summary>
    /// Tests that large batch embedding handles chunking properly.
    /// </summary>
    
    public async Task EmbedBatchAsync_LargeBatch_HandlesChunking()
    {
        // Arrange
        var apiKey = TestHelpers.GetEnvironmentVariableOrSkip(ApiKeyEnvVar);
        var embeddings = new VoyageAIEmbeddings(apiKey: apiKey);
        // Create a batch larger than typical API limits
        var texts = Enumerable.Range(0, 150)
            .Select(i => $"Test sentence number {i}.")
            .ToArray();

        // Act
        var results = await embeddings.EmbedBatchAsync(texts);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(150, results.Count);
        Assert.All(results, embedding =>
        {
            Assert.True(embedding.Length > 0);
            Assert.All(embedding, value => Assert.InRange(value, -1f, 1f));
        });
    }
}

