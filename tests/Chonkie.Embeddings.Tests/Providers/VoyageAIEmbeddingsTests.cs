using Xunit;
using Chonkie.Embeddings.VoyageAI;

namespace Chonkie.Embeddings.Tests.Providers
{
    /// <summary>
    /// Unit tests for the <see cref="VoyageAIEmbeddings"/> provider.
    /// </summary>
    public class VoyageAIEmbeddingsTests
    {
        /// <summary>
        /// Tests that the constructor initializes properties correctly.
        /// </summary>
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange & Act
            var embeddings = new VoyageAIEmbeddings("test-api-key", "voyage-2", 1024);

            // Assert
            Assert.Equal("voyage", embeddings.Name);
            Assert.Equal(1024, embeddings.Dimension);
        }

        /// <summary>
        /// Tests that the constructor throws an exception when API key is null.
        /// </summary>
        [Fact]
        public void Constructor_ThrowsException_WhenApiKeyIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new VoyageAIEmbeddings(null!));
        }

        /// <summary>
        /// Tests that the constructor uses default model.
        /// </summary>
        [Fact]
        public void Constructor_UsesDefaultModel()
        {
            // Arrange & Act
            var embeddings = new VoyageAIEmbeddings("test-api-key");

            // Assert
            Assert.Equal("voyage", embeddings.Name);
            Assert.Equal(1024, embeddings.Dimension);
        }

        /// <summary>
        /// Tests that the Dimension property returns the correct value.
        /// </summary>
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

        /// <summary>
        /// Tests that ToString returns a formatted string.
        /// </summary>
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
