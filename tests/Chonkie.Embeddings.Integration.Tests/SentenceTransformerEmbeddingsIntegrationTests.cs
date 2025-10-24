using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Chonkie.Embeddings.SentenceTransformers;
using Xunit;

namespace Chonkie.Embeddings.Integration.Tests;

/// <summary>
/// Integration tests for Sentence Transformer embeddings provider
/// These tests require a valid ONNX model file path to be set
/// </summary>
public class SentenceTransformerEmbeddingsIntegrationTests
{
    private const string ModelPathEnvVar = "SENTENCE_TRANSFORMER_MODEL_PATH";

    /// <summary>
    /// Tests embedding with real model returns a valid embedding.
    /// </summary>
    [SkippableFact]
    public async Task EmbedAsync_WithRealModel_ReturnsValidEmbedding()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            throw new Xunit.SkipException($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);
        var text = "This is a test sentence for embedding.";

        // Act
        var result = await embeddings.EmbedAsync(text);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        Assert.All(result, value => Assert.True(float.IsFinite(value), "All values should be finite"));
    }

    /// <summary>
    /// Tests batch embedding with multiple texts returns valid embeddings.
    /// </summary>
    [SkippableFact]
    public async Task EmbedBatchAsync_WithMultipleTexts_ReturnsValidEmbeddings()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            throw new Xunit.SkipException($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);
        var texts = new[] { "First sentence.", "Second sentence.", "Third sentence." };

        // Act
        var results = await embeddings.EmbedBatchAsync(texts);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count);
        Assert.All(results, embedding =>
        {
            Assert.True(embedding.Length > 0);
            Assert.All(embedding, value => Assert.True(float.IsFinite(value), "All values should be finite"));
        });
    }

    /// <summary>
    /// Tests that similar texts produce similar embeddings.
    /// </summary>
    [SkippableFact]
    public async Task EmbedAsync_SimilarTexts_ProduceSimilarEmbeddings()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            throw new Xunit.SkipException($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);
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
    }

    /// <summary>
    /// Tests embedding with empty string returns a valid embedding.
    /// </summary>
    [SkippableFact]
    public async Task EmbedAsync_EmptyString_ReturnsValidEmbedding()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            throw new Xunit.SkipException($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);

        // Act
        var result = await embeddings.EmbedAsync(string.Empty);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    /// <summary>
    /// Tests that disposing releases resources without throwing exceptions.
    /// </summary>
    [SkippableFact]
    public void Dispose_ReleasesResources_NoExceptionThrown()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            throw new Xunit.SkipException($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);

        // Act & Assert
        embeddings.Dispose();
        embeddings.Dispose(); // Second dispose should not throw
    }

    /// <summary>
    /// Tests that large batch embedding handles chunking properly.
    /// </summary>
    [SkippableFact]
    public async Task EmbedBatchAsync_LargeBatch_HandlesChunking()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            throw new Xunit.SkipException($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);
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
            Assert.All(embedding, value => Assert.True(float.IsFinite(value), "All values should be finite"));
        });
    }
}

