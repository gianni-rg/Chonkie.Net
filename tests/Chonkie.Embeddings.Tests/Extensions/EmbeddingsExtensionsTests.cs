using Chonkie.Embeddings.Extensions;
using Chonkie.Embeddings.Interfaces;
using Xunit;

namespace Chonkie.Embeddings.Tests.Extensions;

/// <summary>
/// Tests for IEmbeddings extension members (C# 14 feature).
/// </summary>
public class EmbeddingsExtensionsTests
{
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

    [Fact]
    public void DefaultDimension_ReturnsPositiveValue()
    {
        // Act
        var defaultDim = IEmbeddings.DefaultDimension;

        // Assert
        Assert.True(defaultDim > 0);
        Assert.Equal(384, defaultDim);
    }

    [Fact]
    public void Zero_ReturnsZeroVector()
    {
        // Act
        var zeroVector = IEmbeddings.Zero(3);

        // Assert
        Assert.Equal(3, zeroVector.Length);
        Assert.All(zeroVector, value => Assert.Equal(0.0f, value));
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
