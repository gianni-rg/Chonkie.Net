using Xunit;
using Chonkie.Embeddings.Gemini;

namespace Chonkie.Embeddings.Tests.Providers
{
    /// <summary>
    /// Unit tests for the <see cref="GeminiEmbeddings"/> provider.
    /// </summary>
    public class GeminiEmbeddingsTests
    {
        /// <summary>
        /// Tests that the constructor initializes properties correctly.
        /// </summary>
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange & Act
            var embeddings = new GeminiEmbeddings("test-api-key", "embedding-001", 768);

            // Assert
            Assert.Equal("gemini", embeddings.Name);
            Assert.Equal(768, embeddings.Dimension);
        }

        /// <summary>
        /// Tests that the constructor throws an exception when API key is null.
        /// </summary>
        [Fact]
        public void Constructor_ThrowsException_WhenApiKeyIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new GeminiEmbeddings(null!));
        }

        /// <summary>
        /// Tests that the constructor uses default model.
        /// </summary>
        [Fact]
        public void Constructor_UsesDefaultModel()
        {
            // Arrange & Act
            var embeddings = new GeminiEmbeddings("test-api-key");

            // Assert
            Assert.Equal("gemini", embeddings.Name);
            Assert.Equal(768, embeddings.Dimension);
        }

        /// <summary>
        /// Tests that the Dimension property returns the correct value.
        /// </summary>
        [Fact]
        public void DimensionProperty_ReturnsCorrectValue()
        {
            // Arrange
            var embeddings = new GeminiEmbeddings("test-api-key", "embedding-001", 1024);

            // Act
            var dimension = embeddings.Dimension;

            // Assert
            Assert.Equal(1024, dimension);
        }

        /// <summary>
        /// Tests that ToString returns a formatted string.
        /// </summary>
        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var embeddings = new GeminiEmbeddings("test-api-key");

            // Act
            var result = embeddings.ToString();

            // Assert
            Assert.Contains("GeminiEmbeddings", result);
            Assert.Contains("gemini", result);
        }

        // Note: Integration tests for actual API calls should be in a separate test suite
        // and run only when API keys are available
    }
}
