using Xunit;
using Chonkie.Embeddings.OpenAI;

namespace Chonkie.Embeddings.Tests.Providers
{
    /// <summary>
    /// Unit tests for the <see cref="OpenAIEmbeddings"/> provider.
    /// </summary>
    public class OpenAIEmbeddingsTests
    {
        /// <summary>
        /// Tests that the constructor initializes properties correctly.
        /// </summary>
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange & Act
            var embeddings = new OpenAIEmbeddings("test-api-key", "text-embedding-ada-002", 1536);

            // Assert
            Assert.Equal("openai", embeddings.Name);
            Assert.Equal(1536, embeddings.Dimension);
        }

        /// <summary>
        /// Tests that the constructor throws an exception when API key is null.
        /// </summary>
        [Fact]
        public void Constructor_ThrowsException_WhenApiKeyIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new OpenAIEmbeddings(null!));
        }

        // Note: Integration tests for actual API calls should be in a separate test suite
        // and run only when API keys are available
    }
}
