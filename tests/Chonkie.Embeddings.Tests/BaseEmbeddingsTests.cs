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