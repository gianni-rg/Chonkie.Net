using Xunit;
using Chonkie.Embeddings.Base;

namespace Chonkie.Embeddings.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="BaseEmbeddings"/> class.
    /// </summary>
    public class BaseEmbeddingsTests
    {
        /// <summary>
        /// Tests that embedding async returns an embedding array.
        /// </summary>
        [Fact]
        public async Task EmbedAsync_ReturnsEmbeddingArray()
        {
            // Arrange
            var embeddings = new MockEmbeddings();
            var text = "Test text";

            // Act
            var result = await embeddings.EmbedAsync(text);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(embeddings.Dimension, result.Length);
        }

        /// <summary>
        /// Tests that EmbedBatchAsync returns multiple embeddings.
        /// </summary>
        [Fact]
        public async Task EmbedBatchAsync_ReturnsMultipleEmbeddings()
        {
            // Arrange
            var embeddings = new MockEmbeddings();
            var texts = new[] { "Text 1", "Text 2", "Text 3" };

            // Act
            var results = await embeddings.EmbedBatchAsync(texts);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(3, results.Count);
            foreach (var result in results)
            {
                Assert.Equal(embeddings.Dimension, result.Length);
            }
        }

        /// <summary>
        /// Tests that EmbedBatchAsync handles empty input.
        /// </summary>
        [Fact]
        public async Task EmbedBatchAsync_HandlesEmptyInput()
        {
            // Arrange
            var embeddings = new MockEmbeddings();
            var texts = Array.Empty<string>();

            // Act
            var results = await embeddings.EmbedBatchAsync(texts);

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that Similarity computes cosine similarity.
        /// </summary>
        [Fact]
        public async Task Similarity_ComputesCosineSimilarity()
        {
            // Arrange
            var embeddings = new MockEmbeddings();
            var texts = new[] { "Text 1", "Text 2" };
            var results = await embeddings.EmbedBatchAsync(texts);

            // Act
            var similarity = embeddings.Similarity(results[0], results[1]);

            // Assert - Allow for floating point precision issues
            Assert.InRange(similarity, -0.01f, 1.01f);
            Assert.True(similarity >= 0.0f || MathF.Abs(similarity) < 0.01f);
        }

        /// <summary>
        /// Tests that Similarity returns a perfect score for identical vectors.
        /// </summary>
        [Fact]
        public async Task Similarity_ReturnsPerfectScoreForIdenticalVectors()
        {
            // Arrange
            var embeddings = new MockEmbeddings();
            var embedding = await embeddings.EmbedAsync("Test");

            // Act
            var similarity = embeddings.Similarity(embedding, embedding);

            // Assert
            Assert.Equal(1.0f, similarity, precision: 5);
        }

        /// <summary>
        /// Tests that Similarity throws an exception when the vector is null.
        /// </summary>
        [Fact]
        public void Similarity_ThrowsException_WhenVectorIsNull()
        {
            // Arrange
            var embeddings = new MockEmbeddings();
            var validVector = new float[] { 1.0f, 2.0f, 3.0f };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => embeddings.Similarity(null!, validVector));
            Assert.Throws<ArgumentNullException>(() => embeddings.Similarity(validVector, null!));
        }

        /// <summary>
        /// Tests that Similarity throws an exception when vectors have different lengths.
        /// </summary>
        [Fact]
        public void Similarity_ThrowsException_WhenVectorsHaveDifferentLengths()
        {
            // Arrange
            var embeddings = new MockEmbeddings();
            var vector1 = new float[] { 1.0f, 2.0f, 3.0f };
            var vector2 = new float[] { 1.0f, 2.0f };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => embeddings.Similarity(vector1, vector2));
        }

        /// <summary>
        /// Tests that ToString returns a formatted string.
        /// </summary>
        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var embeddings = new MockEmbeddings();

            // Act
            var result = embeddings.ToString();

            // Assert
            Assert.Contains("MockEmbeddings", result);
            Assert.Contains("name=mock", result);
            Assert.Contains("dimension=128", result);
        }

        /// <summary>
        /// Tests that the Dimension property returns a positive value.
        /// </summary>
        [Fact]
        public void DimensionProperty_ReturnsPositiveValue()
        {
            // Arrange
            var embeddings = new MockEmbeddings();

            // Act
            var dimension = embeddings.Dimension;

            // Assert
            Assert.True(dimension > 0);
            Assert.Equal(128, dimension);
        }

        /// <summary>
        /// Tests that the Name property returns the provider name.
        /// </summary>
        [Fact]
        public void NameProperty_ReturnsProviderName()
        {
            // Arrange
            var embeddings = new MockEmbeddings();

            // Act
            var name = embeddings.Name;

            // Assert
            Assert.Equal("mock", name);
        }

        private class MockEmbeddings : BaseEmbeddings
        {
            public override string Name => "mock";
            public override int Dimension => 128;

            public override Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
            {
                var embedding = new float[Dimension];
                for (int i = 0; i < Dimension; i++)
                {
                    embedding[i] = (float)i / Dimension;
                }
                return Task.FromResult(embedding);
            }
        }
    }
}