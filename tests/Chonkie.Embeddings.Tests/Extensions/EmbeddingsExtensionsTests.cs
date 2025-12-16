using Chonkie.Embeddings.Extensions;
using Chonkie.Embeddings.Interfaces;
using Xunit;

namespace Chonkie.Embeddings.Tests.Extensions;

/// <summary>
/// Tests for IEmbeddings extension members (C# 14 feature).
/// </summary>
public class EmbeddingsExtensionsTests
{
    /// <summary>
    /// Tests that ProviderType property returns the provider name without "Embeddings" suffix.
    /// </summary>
    [Fact]
    public void ProviderType_ReturnsProviderName()
    {
        // Arrange
        var embeddings = new TestEmbeddings("Test", 128);

        // Act
        var providerType = embeddings.ProviderType;

        // Assert
        Assert.Equal("Test", providerType);
    }

    /// <summary>
    /// Tests that IsNormalized returns true for a unit vector (magnitude = 1).
    /// </summary>
    [Fact]
    public void IsNormalized_WithUnitVector_ReturnsTrue()
    {
        // Arrange
        var embeddings = new TestEmbeddings("Test", 2);
        var unitVector = new[] { 0.6f, 0.8f }; // Magnitude = 1

        // Act
        var isNormalized = embeddings.IsNormalized(unitVector);

        // Assert
        Assert.True(isNormalized);
    }

    /// <summary>
    /// Tests that IsNormalized returns false for a non-unit vector.
    /// </summary>
    [Fact]
    public void IsNormalized_WithNonUnitVector_ReturnsFalse()
    {
        // Arrange
        var embeddings = new TestEmbeddings("Test", 2);
        var nonUnitVector = new[] { 1.0f, 1.0f }; // Magnitude = sqrt(2)

        // Act
        var isNormalized = embeddings.IsNormalized(nonUnitVector);

        // Assert
        Assert.False(isNormalized);
    }

    /// <summary>
    /// Tests that Magnitude calculates the L2 norm correctly using TensorPrimitives.
    /// </summary>
    [Fact]
    public void Magnitude_CalculatesCorrectly()
    {
        // Arrange
        var embeddings = new TestEmbeddings("Test", 2);
        var vector = new[] { 3.0f, 4.0f }; // Magnitude = 5

        // Act
        var magnitude = embeddings.Magnitude(vector);

        // Assert
        Assert.Equal(5.0f, magnitude, precision: 4);
    }

    /// <summary>
    /// Tests that Distance calculates Euclidean distance correctly using TensorPrimitives.
    /// </summary>
    [Fact]
    public void Distance_CalculatesEuclideanDistance()
    {
        // Arrange
        var embeddings = new TestEmbeddings("Test", 2);
        var u = new[] { 0.0f, 0.0f };
        var v = new[] { 3.0f, 4.0f }; // Distance = 5

        // Act
        var distance = embeddings.Distance(u, v);

        // Assert
        Assert.Equal(5.0f, distance, precision: 4);
    }

    /// <summary>
    /// Tests that DefaultDimension static property returns 384 (default embedding dimension).
    /// </summary>
    [Fact]
    public void DefaultDimension_ReturnsPositiveValue()
    {
        // Act
        var defaultDim = IEmbeddings.DefaultDimension;

        // Assert
        Assert.True(defaultDim > 0);
        Assert.Equal(384, defaultDim);
    }

    /// <summary>
    /// Tests that Zero factory method creates a zero vector of specified dimension.
    /// </summary>
    [Fact]
    public void Zero_ReturnsZeroVector()
    {
        // Act
        var zeroVector = IEmbeddings.Zero(3);

        // Assert
        Assert.Equal(3, zeroVector.Length);
        Assert.All(zeroVector, value => Assert.Equal(0.0f, value));
    }

    /// <summary>
    /// Tests that CosineSimilarity returns 1.0 for identical vectors using TensorPrimitives.
    /// </summary>
    [Fact]
    public void CosineSimilarity_WithIdenticalVectors_ReturnsOne()
    {
        // Arrange
        var embeddings = new TestEmbeddings("Test", 3);
        var v = new[] { 1.0f, 2.0f, 3.0f };

        // Act
        var similarity = embeddings.CosineSimilarity(v, v);

        // Assert
        Assert.Equal(1.0f, similarity, precision: 4);
    }

    /// <summary>
    /// Tests that CosineSimilarity returns 0.0 for orthogonal vectors using TensorPrimitives.
    /// </summary>
    [Fact]
    public void CosineSimilarity_WithOrthogonalVectors_ReturnsZero()
    {
        // Arrange
        var embeddings = new TestEmbeddings("Test", 2);
        var u = new[] { 1.0f, 0.0f };
        var v = new[] { 0.0f, 1.0f };

        // Act
        var similarity = embeddings.CosineSimilarity(u, v);

        // Assert
        Assert.Equal(0.0f, similarity, precision: 4);
    }

    /// <summary>
    /// Tests that CosineSimilarity returns -1.0 for opposite direction vectors using TensorPrimitives.
    /// </summary>
    [Fact]
    public void CosineSimilarity_WithOppositeVectors_ReturnsNegativeOne()
    {
        // Arrange
        var embeddings = new TestEmbeddings("Test", 3);
        var u = new[] { 1.0f, 2.0f, 3.0f };
        var v = new[] { -1.0f, -2.0f, -3.0f };

        // Act
        var similarity = embeddings.CosineSimilarity(u, v);

        // Assert
        Assert.Equal(-1.0f, similarity, precision: 4);
    }

    /// <summary>
    /// Tests that NormalizeInPlace creates a unit vector with magnitude 1.0 using TensorPrimitives.
    /// </summary>
    [Fact]
    public void NormalizeInPlace_CreatesUnitVector()
    {
        // Arrange
        var embeddings = new TestEmbeddings("Test", 2);
        var vector = new[] { 3.0f, 4.0f }; // Magnitude = 5

        // Act
        embeddings.NormalizeInPlace(vector);

        // Assert
        var magnitude = embeddings.Magnitude(vector);
        Assert.Equal(1.0f, magnitude, precision: 4);
        Assert.Equal(0.6f, vector[0], precision: 4);
        Assert.Equal(0.8f, vector[1], precision: 4);
    }

    /// <summary>
    /// Tests that NormalizeInPlace handles zero vector correctly (remains zero).
    /// </summary>
    [Fact]
    public void NormalizeInPlace_WithZeroVector_RemainsZero()
    {
        // Arrange
        var embeddings = new TestEmbeddings("Test", 3);
        var vector = new[] { 0.0f, 0.0f, 0.0f };

        // Act
        embeddings.NormalizeInPlace(vector);

        // Assert
        Assert.All(vector, value => Assert.Equal(0.0f, value));
    }

    /// <summary>
    /// Tests that BatchCosineSimilarity calculates similarities for all candidate vectors.
    /// </summary>
    [Fact]
    public void BatchCosineSimilarity_CalculatesAllSimilarities()
    {
        // Arrange
        var embeddings = new TestEmbeddings("Test", 2);
        var query = new[] { 1.0f, 0.0f };
        var candidates = new[]
        {
            new[] { 1.0f, 0.0f },  // Similar
            new[] { 0.0f, 1.0f },  // Orthogonal
            new[] { -1.0f, 0.0f }  // Opposite
        };

        // Act
        var similarities = embeddings.BatchCosineSimilarity(query, candidates);

        // Assert
        Assert.Equal(3, similarities.Length);
        Assert.Equal(1.0f, similarities[0], precision: 4);
        Assert.Equal(0.0f, similarities[1], precision: 4);
        Assert.Equal(-1.0f, similarities[2], precision: 4);
    }

    /// <summary>
    /// Tests that FindMostSimilar returns the correct index and similarity score of the most similar vector.
    /// </summary>
    [Fact]
    public void FindMostSimilar_ReturnsCorrectIndexAndScore()
    {
        // Arrange
        var embeddings = new TestEmbeddings("Test", 2);
        var query = new[] { 1.0f, 0.0f };
        var candidates = new[]
        {
            new[] { 0.5f, 0.5f },  // 0.707
            new[] { 1.0f, 0.0f },  // 1.0 (most similar)
            new[] { 0.0f, 1.0f }   // 0.0
        };

        // Act
        var (index, similarity) = embeddings.FindMostSimilar(query, candidates);

        // Assert
        Assert.Equal(1, index);
        Assert.Equal(1.0f, similarity, precision: 4);
    }

    /// <summary>
    /// Tests that FindMostSimilar throws ArgumentException when candidates array is empty.
    /// </summary>
    [Fact]
    public void FindMostSimilar_WithEmptyCandidates_ThrowsException()
    {
        // Arrange
        var embeddings = new TestEmbeddings("Test", 2);
        var query = new[] { 1.0f, 0.0f };
        var candidates = Array.Empty<float[]>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => embeddings.FindMostSimilar(query, candidates));
    }

    /// <summary>
    /// Tests that FindTopKSimilar returns top K results sorted by similarity in descending order.
    /// </summary>
    [Fact]
    public void FindTopKSimilar_ReturnsSortedResults()
    {
        // Arrange
        var embeddings = new TestEmbeddings("Test", 2);
        var query = new[] { 1.0f, 0.0f };
        var candidates = new[]
        {
            new[] { 0.5f, 0.5f },   // Index 0, similarity ~0.707
            new[] { 1.0f, 0.0f },   // Index 1, similarity 1.0
            new[] { 0.8f, 0.2f },   // Index 2, similarity ~0.98
            new[] { 0.0f, 1.0f }    // Index 3, similarity 0.0
        };

        // Act
        var topK = embeddings.FindTopKSimilar(query, candidates, k: 2);

        // Assert
        Assert.Equal(2, topK.Length);
        Assert.Equal(1, topK[0].Index);  // Most similar
        Assert.True(topK[0].Similarity > topK[1].Similarity);  // Descending order
    }

    /// <summary>
    /// Tests that FindTopKSimilar throws ArgumentException when K is invalid (zero or greater than candidates length).
    /// </summary>
    [Fact]
    public void FindTopKSimilar_WithInvalidK_ThrowsException()
    {
        // Arrange
        var embeddings = new TestEmbeddings("Test", 2);
        var query = new[] { 1.0f, 0.0f };
        var candidates = new[] { new[] { 1.0f, 0.0f } };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => embeddings.FindTopKSimilar(query, candidates, k: 0));
        Assert.Throws<ArgumentException>(() => embeddings.FindTopKSimilar(query, candidates, k: 2));
    }

    /// <summary>
    /// Tests that FindTopKSimilar throws ArgumentException when candidates array is empty.
    /// </summary>
    [Fact]
    public void FindTopKSimilar_WithEmptyCandidates_ThrowsException()
    {
        // Arrange
        var embeddings = new TestEmbeddings("Test", 2);
        var query = new[] { 1.0f, 0.0f };
        var candidates = Array.Empty<float[]>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => embeddings.FindTopKSimilar(query, candidates, k: 1));
    }

    // Test embeddings implementation
    private class TestEmbeddings : IEmbeddings
    {
        public TestEmbeddings(string name, int dimension)
        {
            Name = name + "Embeddings";
            Dimension = dimension;
        }

        public string Name { get; }
        public int Dimension { get; }

        public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        {
            var embedding = new float[Dimension];
            for (int i = 0; i < Dimension; i++)
            {
                embedding[i] = (float)text.Length / (i + 1);
            }
            return Task.FromResult(embedding);
        }

        public Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
        {
            var result = texts.Select(t => EmbedAsync(t, cancellationToken).Result).ToList();
            return Task.FromResult<IReadOnlyList<float[]>>(result);
        }

        public float Similarity(float[] u, float[] v)
        {
            var dotProduct = u.Zip(v, (a, b) => a * b).Sum();
            var magnitudeU = (float)Math.Sqrt(u.Sum(x => x * x));
            var magnitudeV = (float)Math.Sqrt(v.Sum(x => x * x));
            return dotProduct / (magnitudeU * magnitudeV);
        }
    }
}
