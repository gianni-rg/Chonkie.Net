using Xunit;
using Chonkie.Embeddings.Cohere;

namespace Chonkie.Embeddings.Tests.Providers
{
    /// <summary>
    /// Unit tests for the <see cref="CohereEmbeddings"/> provider.
    /// </summary>
    public class CohereEmbeddingsTests
    {
        /// <summary>
        /// Tests that the constructor initializes properties correctly.
        /// </summary>
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange & Act
            var embeddings = new CohereEmbeddings("test-api-key", "embed-english-v3.0", 1024);

            // Assert
            Assert.Equal("cohere", embeddings.Name);
            Assert.Equal(1024, embeddings.Dimension);
        }

        /// <summary>
        /// Tests that the constructor throws an exception when API key is null.
        /// </summary>
        [Fact]
        public void Constructor_ThrowsException_WhenApiKeyIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new CohereEmbeddings(null!));
        }

        /// <summary>
        /// Tests that the constructor uses default model.
        /// </summary>
        [Fact]
        public void Constructor_UsesDefaultModel()
        {
            // Arrange & Act
            var embeddings = new CohereEmbeddings("test-api-key");

            // Assert
            Assert.Equal("cohere", embeddings.Name);
            Assert.Equal(1024, embeddings.Dimension);
        }

        /// <summary>
        /// Tests that the Dimension property returns the correct value.
        /// </summary>
        [Fact]
        public void DimensionProperty_ReturnsCorrectValue()
        {
            // Arrange
            var embeddings = new CohereEmbeddings("test-api-key", "embed-english-v3.0", 768);

            // Act
            var dimension = embeddings.Dimension;

            // Assert
            Assert.Equal(768, dimension);
        }

        /// <summary>
        /// Tests that ToString returns a formatted string.
        /// </summary>
        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var embeddings = new CohereEmbeddings("test-api-key");

            // Act
            var result = embeddings.ToString();

            // Assert
            Assert.Contains("CohereEmbeddings", result);
            Assert.Contains("cohere", result);
        }

        // Note: Integration tests for actual API calls should be in a separate test suite
        // and run only when API keys are available
    }
}
