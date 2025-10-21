using Xunit;
using Chonkie.Embeddings.Gemini;

namespace Chonkie.Embeddings.Tests.Providers
{
    public class GeminiEmbeddingsTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange & Act
            var embeddings = new GeminiEmbeddings("test-api-key", "embedding-001", 768);

            // Assert
            Assert.Equal("gemini", embeddings.Name);
            Assert.Equal(768, embeddings.Dimension);
        }

        [Fact]
        public void Constructor_ThrowsException_WhenApiKeyIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new GeminiEmbeddings(null!));
        }

        [Fact]
        public void Constructor_UsesDefaultModel()
        {
            // Arrange & Act
            var embeddings = new GeminiEmbeddings("test-api-key");

            // Assert
            Assert.Equal("gemini", embeddings.Name);
            Assert.Equal(768, embeddings.Dimension);
        }

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
