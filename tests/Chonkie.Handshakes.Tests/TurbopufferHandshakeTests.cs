using Chonkie.Core.Types;
using Chonkie.Handshakes;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests;

/// <summary>
/// Unit tests for TurbopufferHandshake constructor validation and parameter handling.
/// </summary>
public class TurbopufferHandshakeTests
{
    [Fact]
    public void Constructor_WithApiKey_InitializesSuccessfully()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act & Assert - should not throw
        var handshake = new TurbopufferHandshake(
            embeddingModel,
            apiKey: "test-api-key",
            apiUrl: "https://api.turbopuffer.com");

        handshake.ShouldNotBeNull();
        handshake.Namespace.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Constructor_WithNullEmbeddingModel_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new TurbopufferHandshake(null!, apiKey: "test-key"));
    }

    [Fact]
    public void Constructor_WithoutApiKeyAndNoEnvironmentVariable_ThrowsInvalidOperationException()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Remove environment variable if set
        var originalValue = Environment.GetEnvironmentVariable("TURBOPUFFER_API_KEY");
        Environment.SetEnvironmentVariable("TURBOPUFFER_API_KEY", null);

        try
        {
            // Act
            var exception = Should.Throw<InvalidOperationException>(() =>
                new TurbopufferHandshake(embeddingModel));

            // Assert
            exception.Message.ShouldContain("API key");
        }
        finally
        {
            // Restore original value
            if (originalValue is not null)
                Environment.SetEnvironmentVariable("TURBOPUFFER_API_KEY", originalValue);
        }
    }

    [Fact]
    public void Constructor_WithRandomNamespaceName_GeneratesUniqueName()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new TurbopufferHandshake(
            embeddingModel,
            apiKey: "test-key",
            namespaceName: "random",
            apiUrl: "https://api.turbopuffer.com");

        // Assert
        handshake.Namespace.ShouldNotBe("random");
        handshake.Namespace.ShouldStartWith("ns_");
    }

    [Fact]
    public void Constructor_WithCustomNamespaceName_UsesProvidedName()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new TurbopufferHandshake(
            embeddingModel,
            apiKey: "test-key",
            namespaceName: "my_namespace",
            apiUrl: "https://api.turbopuffer.com");

        // Assert
        handshake.Namespace.ShouldBe("my_namespace");
    }

    [Fact]
    public void Constructor_WithCustomHttpClient_AcceptsHttpClientParameter()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);
        var httpClient = new HttpClient();

        // Act
        var handshake = new TurbopufferHandshake(
            embeddingModel,
            apiKey: "test-key",
            apiUrl: "https://api.turbopuffer.com",
            httpClient: httpClient);

        // Assert
        handshake.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_UsesEnvironmentVariableWhenApiKeyNotProvided()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Set environment variables
        Environment.SetEnvironmentVariable("TURBOPUFFER_API_KEY", "env-api-key");
        Environment.SetEnvironmentVariable("TURBOPUFFER_API_URL", "https://api.turbopuffer.com");

        try
        {
            // Act
            var handshake = new TurbopufferHandshake(embeddingModel);

            // Assert
            handshake.ShouldNotBeNull();
        }
        finally
        {
            // Clean up
            Environment.SetEnvironmentVariable("TURBOPUFFER_API_KEY", null);
            Environment.SetEnvironmentVariable("TURBOPUFFER_API_URL", null);
        }
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);
        var handshake = new TurbopufferHandshake(
            embeddingModel,
            apiKey: "test-key",
            namespaceName: "my_namespace",
            apiUrl: "https://api.turbopuffer.com");

        // Act
        var result = handshake.ToString();

        // Assert
        result.ShouldContain("TurbopufferHandshake");
        result.ShouldContain("my_namespace");
    }

    [Fact]
    public async Task SearchAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);
        var handshake = new TurbopufferHandshake(
            embeddingModel,
            apiKey: "test-key",
            apiUrl: "https://api.turbopuffer.com");

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(
            () => handshake.SearchAsync(null!, limit: 5));
    }
}
