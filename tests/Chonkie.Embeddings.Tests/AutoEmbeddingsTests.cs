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
            var nonExistentProvider = "non-existent-provider-12345";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => AutoEmbeddings.GetProvider(nonExistentProvider));
            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public void GetProvider_IsCaseInsensitive()
        {
            // Act & Assert - Should not throw
            var provider1 = AutoEmbeddings.GetProvider("openai");
            var provider2 = AutoEmbeddings.GetProvider("OpenAI");
            var provider3 = AutoEmbeddings.GetProvider("OPENAI");

            // All should return instances
            Assert.NotNull(provider1);
            Assert.NotNull(provider2);
            Assert.NotNull(provider3);
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

        [Fact]
        public void ListProviders_ReturnsAtLeastSevenProviders()
        {
            // Act
            var providers = AutoEmbeddings.ListProviders();

            // Assert
            Assert.True(providers.Count >= 7, $"Expected at least 7 providers, but found {providers.Count}");
        }

        [Fact]
        public void RegisterProvider_OverwritesExistingProvider()
        {
            // Arrange
            var providerName = "test-overwrite";
            var mockProvider1 = new MockEmbeddings();
            var mockProvider2 = new MockEmbeddings();

            // Act
            AutoEmbeddings.RegisterProvider(providerName, () => mockProvider1);
            AutoEmbeddings.RegisterProvider(providerName, () => mockProvider2);
            var provider = AutoEmbeddings.GetProvider(providerName);

            // Assert
            Assert.NotNull(provider);
        }

        [Fact]
        public void GetProvider_OpenAI_ReturnsOpenAIEmbeddings()
        {
            // Act
            var provider = AutoEmbeddings.GetProvider("openai");

            // Assert
            Assert.NotNull(provider);
            Assert.IsType<OpenAI.OpenAIEmbeddings>(provider);
        }

        [Fact]
        public void GetProvider_AzureOpenAI_ReturnsAzureOpenAIEmbeddings()
        {
            // Skip if Azure credentials are not set
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")) ||
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")) ||
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT")))
            {
                return; // Skip test
            }

            // Act
            var provider = AutoEmbeddings.GetProvider("azure-openai");

            // Assert
            Assert.NotNull(provider);
            Assert.IsType<Azure.AzureOpenAIEmbeddings>(provider);
        }

        [Fact]
        public void GetProvider_Cohere_ReturnsCohereEmbeddings()
        {
            // Act
            var provider = AutoEmbeddings.GetProvider("cohere");

            // Assert
            Assert.NotNull(provider);
            Assert.IsType<Cohere.CohereEmbeddings>(provider);
        }

        [Fact]
        public void GetProvider_Gemini_ReturnsGeminiEmbeddings()
        {
            // Act
            var provider = AutoEmbeddings.GetProvider("gemini");

            // Assert
            Assert.NotNull(provider);
            Assert.IsType<Gemini.GeminiEmbeddings>(provider);
        }

        [Fact]
        public void GetProvider_Jina_ReturnsJinaEmbeddings()
        {
            // Act
            var provider = AutoEmbeddings.GetProvider("jina");

            // Assert
            Assert.NotNull(provider);
            Assert.IsType<Jina.JinaEmbeddings>(provider);
        }

        [Fact]
        public void GetProvider_Voyage_ReturnsVoyageAIEmbeddings()
        {
            // Act
            var provider = AutoEmbeddings.GetProvider("voyage");

            // Assert
            Assert.NotNull(provider);
            Assert.IsType<VoyageAI.VoyageAIEmbeddings>(provider);
        }

        [Fact]
        public void GetProvider_SentenceTransformers_ReturnsType()
        {
            // Skip if model path is not set
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SENTENCE_TRANSFORMERS_MODEL_PATH")))
            {
                return; // Skip test - model file not available
            }

            // Act
            var provider = AutoEmbeddings.GetProvider("sentence-transformers");

            // Assert
            Assert.NotNull(provider);
            Assert.IsType<SentenceTransformers.SentenceTransformerEmbeddings>(provider);
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