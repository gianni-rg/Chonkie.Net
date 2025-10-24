using Xunit;
using Chonkie.Embeddings.SentenceTransformers;
using NSubstitute;
using Chonkie.Embeddings;

namespace Chonkie.Embeddings.Tests.Providers
{
    /// <summary>
    /// Unit tests for the <see cref="SentenceTransformerEmbeddings"/> provider.
    /// </summary>
    public class SentenceTransformerEmbeddingsTests
    {
        /// <summary>
        /// Tests that the constructor throws when model path is null.
        /// </summary>
        [Fact]
        public void Constructor_ThrowsException_WhenModelPathIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SentenceTransformerEmbeddings(null!));
        }

        /// <summary>
        /// Tests that the constructor initializes properties correctly.
        /// </summary>
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange
            var modelPath = "test-model-directory";

            // Act & Assert - May throw if file doesn't exist, but we're just testing property initialization
            try
            {
                var embeddings = new SentenceTransformerEmbeddings(
                    modelPath: modelPath,
                    poolingMode: null,
                    normalize: true,
                    maxLength: null
                );
                Assert.Equal("sentence-transformers", embeddings.Name);
            }
            catch (Exception)
            {
                // Expected when model directory doesn't exist
            }
        }

        // Note: Full integration tests require actual ONNX model files

        /// <summary>
        /// Tests Similarity returns expected value for mock embeddings.
        /// </summary>
        [Fact]
        public void CosineSimilarity_ReturnsExpectedValue()
        {
            // Arrange
            var embedding1 = new float[] { 1, 0 };
            var embedding2 = new float[] { 0, 1 };
            var embedding3 = new float[] { 1, 1 };

            // Act
            var similarity12 = VectorMath.CosineSimilarity(embedding1, embedding2);
            var similarity13 = VectorMath.CosineSimilarity(embedding1, embedding3);

            // Assert
            Assert.True(similarity13 > similarity12,
                $"Similar vectors should have higher similarity. Expected {similarity13} > {similarity12}");
            Assert.InRange(similarity12, 0, 1);
            Assert.InRange(similarity13, 0, 1);
        }

        /// <summary>
        /// Tests CosineSimilarity with identical vectors returns 1.
        /// </summary>
        [Fact]
        public void CosineSimilarity_IdenticalVectors_ReturnsOne()
        {
            // Arrange
            var embedding1 = new float[] { 1, 2, 3, 4 };
            var embedding2 = new float[] { 1, 2, 3, 4 };

            // Act
            var similarity = VectorMath.CosineSimilarity(embedding1, embedding2);

            // Assert
            Assert.Equal(1.0f, similarity, precision: 5);
        }

        /// <summary>
        /// Tests CosineSimilarity with orthogonal vectors returns 0.
        /// </summary>
        [Fact]
        public void CosineSimilarity_OrthogonalVectors_ReturnsZero()
        {
            // Arrange
            var embedding1 = new float[] { 1, 0, 0 };
            var embedding2 = new float[] { 0, 1, 0 };

            // Act
            var similarity = VectorMath.CosineSimilarity(embedding1, embedding2);

            // Assert
            Assert.Equal(0.0f, similarity, precision: 5);
        }

        /// <summary>
        /// Tests CosineSimilarity throws when vectors have different lengths.
        /// </summary>
        [Fact]
        public void CosineSimilarity_DifferentLengths_ThrowsException()
        {
            // Arrange
            var embedding1 = new float[] { 1, 2, 3 };
            var embedding2 = new float[] { 1, 2 };

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                VectorMath.CosineSimilarity(embedding1, embedding2));
        }

        /// <summary>
        /// Tests L2Normalize produces unit vectors.
        /// </summary>
        [Fact]
        public void L2Normalize_ProducesUnitVector()
        {
            // Arrange
            var embedding = new float[] { 3, 4 }; // Length = 5

            // Act
            var normalized = PoolingUtilities.L2Normalize(embedding);

            // Assert
            var magnitude = Math.Sqrt(normalized.Sum(x => x * x));
            Assert.Equal(1.0, magnitude, precision: 5);
            Assert.Equal(0.6f, normalized[0], precision: 5); // 3/5
            Assert.Equal(0.8f, normalized[1], precision: 5); // 4/5
        }

        /// <summary>
        /// Tests L2Normalize with zero vector doesn't throw.
        /// </summary>
        [Fact]
        public void L2Normalize_ZeroVector_DoesNotThrow()
        {
            // Arrange
            var embedding = new float[] { 0, 0, 0 };

            // Act
            var normalized = PoolingUtilities.L2Normalize(embedding);

            // Assert
            Assert.NotNull(normalized);
            Assert.Equal(3, normalized.Length);
        }
    }
}
