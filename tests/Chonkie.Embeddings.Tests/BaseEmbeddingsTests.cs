using Xunit;
using Chonkie.Embeddings.Base;

namespace Chonkie.Embeddings.Tests
{
    public class BaseEmbeddingsTests
    {
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