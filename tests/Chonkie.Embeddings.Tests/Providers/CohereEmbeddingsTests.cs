using Xunit;
using Chonkie.Embeddings.Cohere;

namespace Chonkie.Embeddings.Tests.Providers
{
    public class CohereEmbeddingsTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange & Act
            var embeddings = new CohereEmbeddings("test-api-key", "embed-english-v3.0", 1024);

            // Assert
            Assert.Equal("cohere", embeddings.Name);
            Assert.Equal(1024, embeddings.Dimension);
        }

        [Fact]
        public void Constructor_ThrowsException_WhenApiKeyIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new CohereEmbeddings(null!));
        }

        [Fact]
        public void Constructor_UsesDefaultModel()
        {
            // Arrange & Act
            var embeddings = new CohereEmbeddings("test-api-key");

            // Assert
            Assert.Equal("cohere", embeddings.Name);
            Assert.Equal(1024, embeddings.Dimension);
        }

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
