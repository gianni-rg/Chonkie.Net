using Xunit;
using Chonkie.Embeddings;

namespace Chonkie.Embeddings.Tests
{
    public class AutoEmbeddingsTests
    {
        [Fact]
        public void RegisterProvider_AddsProviderToDictionary()
        {
            // Arrange
            var providerName = "test-provider";
            var mockProvider = new MockEmbeddings();

            // Act
            AutoEmbeddings.RegisterProvider(providerName, () => mockProvider);
            var providers = AutoEmbeddings.ListProviders();

            // Assert
            Assert.Contains(providerName, providers);
        }

        [Fact]
        public void GetProvider_ThrowsException_WhenProviderNotFound()
        {
            // Arrange
            var nonExistentProvider = "non-existent-provider";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => AutoEmbeddings.GetProvider(nonExistentProvider));
        }

        [Fact]
        public void ListProviders_ReturnsAllRegisteredProviders()
        {
            // Act
            var providers = AutoEmbeddings.ListProviders();

            // Assert
            Assert.NotEmpty(providers);
            Assert.Contains("openai", providers);
            Assert.Contains("azure-openai", providers);
            Assert.Contains("sentence-transformers", providers);
            Assert.Contains("cohere", providers);
            Assert.Contains("gemini", providers);
            Assert.Contains("jina", providers);
            Assert.Contains("voyage", providers);
        }

        private class MockEmbeddings : Base.BaseEmbeddings
        {
            public override string Name => "mock";
            public override int Dimension => 128;

            public override Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new float[Dimension]);
            }
        }
    }
}