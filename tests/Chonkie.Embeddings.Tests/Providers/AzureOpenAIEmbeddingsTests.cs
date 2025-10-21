using Xunit;
using Chonkie.Embeddings.Azure;

namespace Chonkie.Embeddings.Tests.Providers
{
    public class AzureOpenAIEmbeddingsTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange & Act
            var embeddings = new AzureOpenAIEmbeddings(
                "https://test.openai.azure.com",
                "test-api-key",
                "test-deployment",
                1536
            );

            // Assert
            Assert.Equal("azure-openai", embeddings.Name);
            Assert.Equal(1536, embeddings.Dimension);
        }

        // Note: Integration tests for actual API calls should be in a separate test suite
        // and run only when Azure credentials are available
    }
}