using Chonkie.Embeddings;
using Chonkie.Embeddings.Azure;
using Shouldly;
using Xunit;

namespace Chonkie.Embeddings.Tests;

/// <summary>
/// Tests for UnifiedEmbeddingsFactory class.
/// Azure tests are skipped if Azure.AI.OpenAI SDK is incompatible.
/// </summary>
public class UnifiedEmbeddingsFactoryTests
{
    private const string TestAzureEndpoint = "https://test.openai.azure.com";
    private const string TestAzureApiKey = "test-api-key";
    private const string TestAzureDeployment = "test-deployment";

    private static bool CanCreateAzureEmbeddings()
    {
        try
        {
            _ = new AzureOpenAIEmbeddings(
                TestAzureEndpoint,
                TestAzureApiKey,
                TestAzureDeployment);
            return true;
        }
        catch (TypeLoadException)
        {
            // Azure SDK has compatibility issues on this system
            return false;
        }
        catch
        {
            // Other exceptions mean SDK is working
            return true;
        }
    }

    private void SkipIfAzureNotAvailable()
    {
        if (!CanCreateAzureEmbeddings())
        {
            Assert.Skip("Azure.AI.OpenAI SDK is not compatible on this system. Skipping test.");
        }
    }

    #region OpenAI Tests

    [Fact]
    public void CreateOpenAI_WithNullApiKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            UnifiedEmbeddingsFactory.CreateOpenAI(null!));
    }

    [Fact]
    public void CreateOpenAI_WithEmptyApiKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            UnifiedEmbeddingsFactory.CreateOpenAI(""));
    }

    [Fact]
    public void CreateOpenAI_WithWhitespaceApiKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            UnifiedEmbeddingsFactory.CreateOpenAI("   "));
    }

    [Fact]
    public void CreateOpenAI_WithValidApiKey_ShouldReturnInstance()
    {
        // Act
        var embeddings = UnifiedEmbeddingsFactory.CreateOpenAI("test-api-key");

        // Assert
        embeddings.ShouldNotBeNull();
        embeddings.Name.ShouldBe("unified-openai");
        embeddings.Dimension.ShouldBe(1536);
    }

    [Fact]
    public void CreateOpenAI_WithCustomModel_ShouldUseProvidedModel()
    {
        // Act
        var embeddings = UnifiedEmbeddingsFactory.CreateOpenAI(
            "test-api-key",
            "text-embedding-3-small",
            1536);

        // Assert
        embeddings.ShouldNotBeNull();
        embeddings.Name.ShouldBe("unified-openai");
    }

    #endregion

    #region Azure OpenAI Tests

    [Fact]
    public void CreateAzureOpenAI_WithNullEndpoint_ShouldThrowArgumentException()
    {
        // Act & Assert (validation happens before SDK instantiation)
        Should.Throw<ArgumentException>(() =>
            UnifiedEmbeddingsFactory.CreateAzureOpenAI(null!, "api-key", "deployment"));
    }

    [Fact]
    public void CreateAzureOpenAI_WithNullApiKey_ShouldThrowArgumentException()
    {
        // Act & Assert (validation happens before SDK instantiation)
        Should.Throw<ArgumentException>(() =>
            UnifiedEmbeddingsFactory.CreateAzureOpenAI("https://test.openai.azure.com", null!, "deployment"));
    }

    [Fact]
    public void CreateAzureOpenAI_WithNullDeployment_ShouldThrowArgumentException()
    {
        // Act & Assert (validation happens before SDK instantiation)
        Should.Throw<ArgumentException>(() =>
            UnifiedEmbeddingsFactory.CreateAzureOpenAI("https://test.openai.azure.com", "api-key", null!));
    }

    [Fact]
    public void CreateAzureOpenAI_WithValidParameters_ShouldReturnInstance()
    {
        try
        {
            SkipIfAzureNotAvailable();

            // Act
            var embeddings = UnifiedEmbeddingsFactory.CreateAzureOpenAI(
                TestAzureEndpoint,
                TestAzureApiKey,
                TestAzureDeployment,
                logger: null);

            // Assert
            embeddings.ShouldNotBeNull();
            embeddings.Name.ShouldBe("unified-azure-openai");
            embeddings.Dimension.ShouldBe(1536);
        }
        catch (TypeLoadException)
        {
            Assert.Skip("Azure.AI.OpenAI SDK is not compatible on this system. Skipping test.");
        }
    }

    [Fact]
    public void CreateAzureOpenAI_WithCustomDimension_ShouldUseProvidedDimension()
    {
        try
        {
            SkipIfAzureNotAvailable();

            // Act
            var embeddings = UnifiedEmbeddingsFactory.CreateAzureOpenAI(
                TestAzureEndpoint,
                TestAzureApiKey,
                TestAzureDeployment,
                3072,
                logger: null);

            // Assert
            embeddings.Dimension.ShouldBe(3072);
        }
        catch (TypeLoadException)
        {
            Assert.Skip("Azure.AI.OpenAI SDK is not compatible on this system. Skipping test.");
        }
    }

    #endregion

    #region Ollama Tests

    [Fact]
    public void CreateOllama_WithNullModel_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            UnifiedEmbeddingsFactory.CreateOllama(null!, 384));
    }

    [Fact]
    public void CreateOllama_WithEmptyModel_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            UnifiedEmbeddingsFactory.CreateOllama("", 384));
    }

    [Fact]
    public void CreateOllama_WithValidModel_ShouldReturnInstance()
    {
        // Act
        var embeddings = UnifiedEmbeddingsFactory.CreateOllama("all-minilm", 384);

        // Assert
        embeddings.ShouldNotBeNull();
        embeddings.Name.ShouldBe("unified-ollama");
        embeddings.Dimension.ShouldBe(384);
    }

    [Fact]
    public void CreateOllama_WithCustomEndpoint_ShouldUseProvidedEndpoint()
    {
        // Act
        var embeddings = UnifiedEmbeddingsFactory.CreateOllama(
            "all-minilm",
            384,
            "http://custom-host:11434");

        // Assert
        embeddings.ShouldNotBeNull();
        embeddings.Name.ShouldBe("unified-ollama");
    }

    #endregion

    #region CreateFromEnvironment Tests

    [Fact]
    public void CreateFromEnvironment_WithNullProviderName_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            UnifiedEmbeddingsFactory.CreateFromEnvironment(null!, "model", 768));
    }

    [Fact]
    public void CreateFromEnvironment_WithUnsupportedProvider_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            UnifiedEmbeddingsFactory.CreateFromEnvironment("unsupported", "model", 768));

        exception.Message.ShouldContain("Unsupported provider");
    }

    [Fact]
    public void CreateFromEnvironment_WithOpenAIButMissingKey_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", null);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            UnifiedEmbeddingsFactory.CreateFromEnvironment("openai", "text-embedding-ada-002", 1536));
    }

    [Fact]
    public void CreateFromEnvironment_WithAzureButMissingEndpoint_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_OPENAI_ENDPOINT", null);
        Environment.SetEnvironmentVariable("AZURE_OPENAI_API_KEY", "test-key");
        Environment.SetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT", "test-deployment");

        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            UnifiedEmbeddingsFactory.CreateFromEnvironment("azure", "model", 1536));
    }

    #endregion
}
