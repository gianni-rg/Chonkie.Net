using Xunit;
using Chonkie.Embeddings;

namespace Chonkie.Embeddings.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="AutoEmbeddings"/> class.
    /// </summary>
    public class AutoEmbeddingsTests
    {
        /// <summary>
        /// Tests that registering a provider adds it to the dictionary.
        /// </summary>
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

        /// <summary>
        /// Tests that GetProvider throws an exception when the provider is not found.
        /// </summary>
        [Fact]
        public void GetProvider_ThrowsException_WhenProviderNotFound()
        {
            // Arrange
            var nonExistentProvider = "non-existent-provider-12345";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => AutoEmbeddings.GetProvider(nonExistentProvider));
            Assert.Contains("not found", exception.Message);
        }

        /// <summary>
        /// Tests that GetProvider is case insensitive.
        /// </summary>
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

        /// <summary>
        /// Tests that ListProviders returns all registered providers.
        /// </summary>
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

        /// <summary>
        /// Tests that ListProviders returns at least seven providers.
        /// </summary>
        [Fact]
        public void ListProviders_ReturnsAtLeastSevenProviders()
        {
            // Act
            var providers = AutoEmbeddings.ListProviders();

            // Assert
            Assert.True(providers.Count >= 7, $"Expected at least 7 providers, but found {providers.Count}");
        }

        /// <summary>
        /// Tests that registering a provider overwrites an existing provider.
        /// </summary>
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

        /// <summary>
        /// Tests that GetProvider("OpenAI") returns an OpenAIEmbeddings instance.
        /// </summary>
        [Fact]
        public void GetProvider_OpenAI_ReturnsOpenAIEmbeddings()
        {
            // Act
            var provider = AutoEmbeddings.GetProvider("openai");

            // Assert
            Assert.NotNull(provider);
            Assert.IsType<OpenAI.OpenAIEmbeddings>(provider);
        }

        /// <summary>
        /// Tests that GetProvider("AzureOpenAI") returns an AzureOpenAIEmbeddings instance.
        /// </summary>
        [Fact]
        public void GetProvider_AzureOpenAI_ReturnsAzureOpenAIEmbeddings()
        {
            // Skip if Azure credentials are not set
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")) ||
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")) ||
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_EMBEDDINGS")))
            {
                Assert.Skip("Azure OpenAI credentials not configured");
            }

            // Act
            var provider = AutoEmbeddings.GetProvider("azure-openai");

            // Assert
            Assert.NotNull(provider);
            Assert.IsType<Azure.AzureOpenAIEmbeddings>(provider);
        }

        /// <summary>
        /// Tests that GetProvider("Cohere") returns a CohereEmbeddings instance.
        /// </summary>
        [Fact]
        public void GetProvider_Cohere_ReturnsCohereEmbeddings()
        {
            // Act
            var provider = AutoEmbeddings.GetProvider("cohere");

            // Assert
            Assert.NotNull(provider);
            Assert.IsType<Cohere.CohereEmbeddings>(provider);
        }

        /// <summary>
        /// Tests that GetProvider("Gemini") returns a GeminiEmbeddings instance.
        /// </summary>
        [Fact]
        public void GetProvider_Gemini_ReturnsGeminiEmbeddings()
        {
            // Act
            var provider = AutoEmbeddings.GetProvider("gemini");

            // Assert
            Assert.NotNull(provider);
            Assert.IsType<Gemini.GeminiEmbeddings>(provider);
        }

        /// <summary>
        /// Tests that GetProvider("Jina") returns a JinaEmbeddings instance.
        /// </summary>
        [Fact]
        public void GetProvider_Jina_ReturnsJinaEmbeddings()
        {
            // Act
            var provider = AutoEmbeddings.GetProvider("jina");

            // Assert
            Assert.NotNull(provider);
            Assert.IsType<Jina.JinaEmbeddings>(provider);
        }

        /// <summary>
        /// Tests that GetProvider("Voyage") returns a VoyageAIEmbeddings instance.
        /// </summary>
        [Fact]
        public void GetProvider_Voyage_ReturnsVoyageAIEmbeddings()
        {
            // Act
            var provider = AutoEmbeddings.GetProvider("voyage");

            // Assert
            Assert.NotNull(provider);
            Assert.IsType<VoyageAI.VoyageAIEmbeddings>(provider);
        }

        /// <summary>
        /// Tests that GetProvider("SentenceTransformers") returns the correct type.
        /// </summary>
        [Fact]
        public void GetProvider_SentenceTransformers_ReturnsType()
        {
            // Skip if model path is not set
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CHONKIE_SENTENCE_TRANSFORMER_MODEL_PATH")))
            {
                Assert.Skip("Sentence Transformers model path not configured");
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
