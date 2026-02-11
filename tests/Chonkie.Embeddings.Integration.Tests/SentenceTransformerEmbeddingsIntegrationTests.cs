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
    private const string ModelPathEnvVar = "CHONKIE_SENTENCE_TRANSFORMERS_MODEL_PATH";

    /// <summary>
    /// Tests embedding with real model returns a valid embedding.
    /// </summary>
    [Fact]
    public async Task EmbedAsync_WithRealModel_ReturnsValidEmbedding()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");

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
    [Fact]
    public async Task EmbedBatchAsync_WithMultipleTexts_ReturnsValidEmbeddings()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");

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
    [Fact]
    public async Task EmbedAsync_SimilarTexts_ProduceSimilarEmbeddings()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");

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
    [Fact]
    public async Task EmbedAsync_EmptyString_ReturnsValidEmbedding()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");

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
    [Fact]
    public void Dispose_ReleasesResources_NoExceptionThrown()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);

        // Act & Assert
        embeddings.Dispose();
        embeddings.Dispose(); // Second dispose should not throw
    }

    /// <summary>
    /// Tests that large batch embedding handles chunking properly.
    /// </summary>
    [Fact]
    public async Task EmbedBatchAsync_LargeBatch_HandlesChunking()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
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

    /// <summary>
    /// Tests Dimension property returns positive value.
    /// </summary>
    [Fact]
    public void Dimension_ReturnsPositiveValue()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);

        // Act & Assert
        Assert.True(embeddings.Dimension > 0, "Dimension should be positive");
        // Different models have different dimensions (e.g., 384, 768, 1024)
        Assert.InRange(embeddings.Dimension, 1, 2048);
    }

    /// <summary>
    /// Tests MaxSequenceLength property returns positive value.
    /// </summary>
    [Fact]
    public void MaxSequenceLength_ReturnsPositiveValue()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);

        // Act & Assert
        Assert.True(embeddings.MaxSequenceLength > 0, "MaxSequenceLength should be positive");
    }

    /// <summary>
    /// Tests CountTokens returns expected token count.
    /// </summary>
    [Fact]
    public void CountTokens_ReturnsExpectedCount()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);
        var text = "This is a test sentence.";

        // Act
        var tokenCount = embeddings.CountTokens(text);

        // Assert
        Assert.True(tokenCount > 0, "Token count should be positive");
        Assert.InRange(tokenCount, 1, 100); // Reasonable range for a short sentence
    }

    /// <summary>
    /// Tests CountTokens with empty string returns zero or minimal count.
    /// </summary>
    [Fact]
    public void CountTokens_EmptyString_ReturnsMinimalCount()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);

        // Act
        var tokenCount = embeddings.CountTokens(string.Empty);

        // Assert
        Assert.True(tokenCount >= 0, "Token count should be non-negative");
    }

    /// <summary>
    /// Tests CountTokensBatch returns expected token counts for multiple texts.
    /// </summary>
    [Fact]
    public void CountTokensBatch_ReturnsExpectedCounts()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);
        var texts = new[] { "Short text.", "This is a longer sentence with more words.", "Medium length." };

        // Act
        var tokenCounts = embeddings.CountTokensBatch(texts);

        // Assert
        Assert.NotNull(tokenCounts);
        Assert.Equal(3, tokenCounts.Count);
        Assert.All(tokenCounts, count => Assert.True(count > 0, "All token counts should be positive"));

        // Verify that longer text has more tokens
        Assert.True(tokenCounts[1] > tokenCounts[0], "Longer text should have more tokens");
        Assert.True(tokenCounts[1] > tokenCounts[2], "Longer text should have more tokens");
    }

    /// <summary>
    /// Tests CountTokensBatch with empty list returns empty list.
    /// </summary>
    [Fact]
    public void CountTokensBatch_EmptyList_ReturnsEmptyList()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);
        var texts = Array.Empty<string>();

        // Act
        var tokenCounts = embeddings.CountTokensBatch(texts);

        // Assert
        Assert.NotNull(tokenCounts);
        Assert.Empty(tokenCounts);
    }

    /// <summary>
    /// Tests ToString returns formatted string.
    /// </summary>
    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);

        // Act
        var result = embeddings.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("SentenceTransformerEmbeddings", result);
        Assert.Contains("sentence-transformers", result);
        Assert.Contains("dimension=", result);
    }

    /// <summary>
    /// Tests Name property returns expected value.
    /// </summary>
    [Fact]
    public void Name_ReturnsExpectedValue()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);

        // Act & Assert
        Assert.Equal("sentence-transformers", embeddings.Name);
    }

    /// <summary>
    /// Tests embedding normalization when enabled.
    /// </summary>
    [Fact]
    public async Task EmbedAsync_WithNormalization_ProducesUnitVectors()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath, normalize: true);
        var text = "Test sentence for normalization.";

        // Act
        var embedding = await embeddings.EmbedAsync(text);

        // Assert
        var magnitude = MathF.Sqrt(embedding.Sum(x => x * x));
        Assert.Equal(1.0f, magnitude, precision: 5);
    }

    /// <summary>
    /// Tests batch embedding produces consistent results with single embeddings.
    /// </summary>
    [Fact]
    public async Task EmbedBatchAsync_ProducesConsistentResults()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);
        var text1 = "First test sentence.";
        var text2 = "Second test sentence.";

        // Act
        var singleEmbedding1 = await embeddings.EmbedAsync(text1);
        var singleEmbedding2 = await embeddings.EmbedAsync(text2);
        var batchEmbeddings = await embeddings.EmbedBatchAsync([text1, text2]);

        // Assert
        Assert.Equal(2, batchEmbeddings.Count);

        // Compare first embedding
        var similarity1 = embeddings.Similarity(singleEmbedding1, batchEmbeddings[0]);
        Assert.True(similarity1 > 0.99f, $"Batch and single embeddings should be nearly identical. Similarity: {similarity1}");

        // Compare second embedding
        var similarity2 = embeddings.Similarity(singleEmbedding2, batchEmbeddings[1]);
        Assert.True(similarity2 > 0.99f, $"Batch and single embeddings should be nearly identical. Similarity: {similarity2}");
    }

    /// <summary>
    /// Tests that embeddings have the correct dimension.
    /// </summary>
    [Fact]
    public async Task EmbedAsync_ReturnedEmbeddingHasCorrectDimension()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);
        var text = "Test sentence.";

        // Act
        var embedding = await embeddings.EmbedAsync(text);

        // Assert
        Assert.Equal(embeddings.Dimension, embedding.Length);
    }

    /// <summary>
    /// Tests batch embeddings all have the correct dimension.
    /// </summary>
    [Fact]
    public async Task EmbedBatchAsync_AllEmbeddingsHaveCorrectDimension()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);
        var texts = new[] { "First.", "Second.", "Third." };

        // Act
        var batchEmbeddings = await embeddings.EmbedBatchAsync(texts);

        // Assert
        Assert.All(batchEmbeddings, embedding =>
            Assert.Equal(embeddings.Dimension, embedding.Length));
    }

    /// <summary>
    /// Tests that identical texts produce identical embeddings.
    /// </summary>
    [Fact]
    public async Task EmbedAsync_IdenticalTexts_ProduceIdenticalEmbeddings()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);
        var text = "The cat sits on the mat.";

        // Act
        var embedding1 = await embeddings.EmbedAsync(text);
        var embedding2 = await embeddings.EmbedAsync(text);

        // Assert
        var similarity = embeddings.Similarity(embedding1, embedding2);
        Assert.Equal(1.0f, similarity, precision: 5);
    }

    /// <summary>
    /// Tests similarity method produces values in valid range.
    /// </summary>
    [Fact]
    public async Task Similarity_ProducesValidRange()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);
        var text1 = "The weather is nice.";
        var text2 = "It's raining heavily today.";

        // Act
        var embedding1 = await embeddings.EmbedAsync(text1);
        var embedding2 = await embeddings.EmbedAsync(text2);
        var similarity = embeddings.Similarity(embedding1, embedding2);

        // Assert - allow for floating point precision issues
        Assert.InRange(similarity, 0, 1.001);
    }

    /// <summary>
    /// Tests that long text is handled correctly.
    /// </summary>
    [Fact]
    public async Task EmbedAsync_LongText_HandlesCorrectly()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);
        // Create a long text that exceeds typical sequence lengths
        var longText = string.Join(" ", Enumerable.Repeat("This is a test sentence.", 100));

        // Act
        var embedding = await embeddings.EmbedAsync(longText);

        // Assert
        Assert.NotNull(embedding);
        Assert.Equal(embeddings.Dimension, embedding.Length);
        Assert.All(embedding, value => Assert.True(float.IsFinite(value)));
    }

    /// <summary>
    /// Tests CountTokens with various text lengths.
    /// </summary>
    [Fact]
    public void CountTokens_VariousLengths_ReturnsAppropriateCount()
    {
        // Arrange
        var modelPath = TestHelpers.GetEnvironmentVariableOrSkip(ModelPathEnvVar);
        if (!Directory.Exists(modelPath))
        {
            Assert.Skip($"Model directory not found at {modelPath}. Skipping integration test.");
        }

        using var embeddings = new SentenceTransformerEmbeddings(modelPath: modelPath);
        var shortText = "Hi";
        var mediumText = "This is a medium length sentence.";
        var longText = "This is a much longer sentence with many more words that should produce a higher token count.";

        // Act
        var shortCount = embeddings.CountTokens(shortText);
        var mediumCount = embeddings.CountTokens(mediumText);
        var longCount = embeddings.CountTokens(longText);

        // Assert
        Assert.True(shortCount < mediumCount, "Short text should have fewer tokens than medium");
        Assert.True(mediumCount < longCount, "Medium text should have fewer tokens than long");
    }
}

