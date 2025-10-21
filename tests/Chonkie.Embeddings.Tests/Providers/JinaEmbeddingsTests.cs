using Xunit;
using Chonkie.Embeddings.Jina;

namespace Chonkie.Embeddings.Tests.Providers
{
    public class JinaEmbeddingsTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange & Act
            var embeddings = new JinaEmbeddings("test-api-key", "jina-embeddings-v2-base-en", 768);

            // Assert
            Assert.Equal("jina", embeddings.Name);
            Assert.Equal(768, embeddings.Dimension);
        }

        [Fact]
        public void Constructor_ThrowsException_WhenApiKeyIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new JinaEmbeddings(null!));
        }

        [Fact]
        public void Constructor_UsesDefaultModel()
        {
            // Arrange & Act
            var embeddings = new JinaEmbeddings("test-api-key");

            // Assert
            Assert.Equal("jina", embeddings.Name);
            Assert.Equal(768, embeddings.Dimension);
        }

        [Fact]
        public void DimensionProperty_ReturnsCorrectValue()
        {
            // Arrange
            var embeddings = new JinaEmbeddings("test-api-key", "jina-embeddings-v2-base-en", 512);

            // Act
            var dimension = embeddings.Dimension;

            // Assert
            Assert.Equal(512, dimension);
        }

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var embeddings = new JinaEmbeddings("test-api-key");

            // Act
            var result = embeddings.ToString();

            // Assert
            Assert.Contains("JinaEmbeddings", result);
            Assert.Contains("jina", result);
        }

        // Note: Integration tests for actual API calls should be in a separate test suite
        // and run only when API keys are available
    }
}
