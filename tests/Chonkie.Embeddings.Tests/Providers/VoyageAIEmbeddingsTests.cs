using Xunit;
using Chonkie.Embeddings.VoyageAI;

namespace Chonkie.Embeddings.Tests.Providers
{
    public class VoyageAIEmbeddingsTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange & Act
            var embeddings = new VoyageAIEmbeddings("test-api-key", "voyage-2", 1024);

            // Assert
            Assert.Equal("voyage", embeddings.Name);
            Assert.Equal(1024, embeddings.Dimension);
        }

        [Fact]
        public void Constructor_ThrowsException_WhenApiKeyIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new VoyageAIEmbeddings(null!));
        }

        [Fact]
        public void Constructor_UsesDefaultModel()
        {
            // Arrange & Act
            var embeddings = new VoyageAIEmbeddings("test-api-key");

            // Assert
            Assert.Equal("voyage", embeddings.Name);
            Assert.Equal(1024, embeddings.Dimension);
        }

        [Fact]
        public void DimensionProperty_ReturnsCorrectValue()
        {
            // Arrange
            var embeddings = new VoyageAIEmbeddings("test-api-key", "voyage-2", 768);

            // Act
            var dimension = embeddings.Dimension;

            // Assert
            Assert.Equal(768, dimension);
        }

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var embeddings = new VoyageAIEmbeddings("test-api-key");

            // Act
            var result = embeddings.ToString();

            // Assert
            Assert.Contains("VoyageAIEmbeddings", result);
            Assert.Contains("voyage", result);
        }

        // Note: Integration tests for actual API calls should be in a separate test suite
        // and run only when API keys are available
    }
}
