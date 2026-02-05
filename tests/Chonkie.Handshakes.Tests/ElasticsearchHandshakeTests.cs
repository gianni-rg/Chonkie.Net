using Chonkie.Core.Types;
using Chonkie.Handshakes;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests;

/// <summary>
/// Unit tests for ElasticsearchHandshake constructor validation and parameter handling.
/// </summary>
public class ElasticsearchHandshakeTests
{
    [Fact]
    public void Constructor_WithDefaultHostAndIndexName_InitializesSuccessfully()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act & Assert - should not throw
        var handshake = new ElasticsearchHandshake(embeddingModel);

        handshake.ShouldNotBeNull();
        handshake.Dimension.ShouldBe(384);
    }

    [Fact]
    public void Constructor_WithNullEmbeddingModel_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new ElasticsearchHandshake(null!));
    }

    [Fact]
    public void Constructor_WithCustomIndexName_UsesProvidedName()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new ElasticsearchHandshake(
            embeddingModel,
            indexName: "my_index");

        // Assert
        handshake.IndexName.ShouldBe("my_index");
    }

    [Fact]
    public void Constructor_WithRandomIndexName_GeneratesUniqueName()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new ElasticsearchHandshake(
            embeddingModel,
            indexName: "random");

        // Assert
        handshake.IndexName.ShouldNotBe("random");
        handshake.IndexName.ShouldStartWith("chonkie-");
    }

    [Fact]
    public void Constructor_WithCustomServerUrl_AcceptsServerUrl()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new ElasticsearchHandshake(
            embeddingModel,
            serverUrl: "http://es1:9200");

        // Assert
        handshake.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithApiKey_AcceptsApiKeyAuthentication()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new ElasticsearchHandshake(
            embeddingModel,
            serverUrl: "http://localhost:9200",
            apiKey: "VnItWjRCSTQyMEVWeFM6");

        // Assert
        handshake.ShouldNotBeNull();
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);
        var handshake = new ElasticsearchHandshake(
            embeddingModel,
            indexName: "my_index");

        // Act
        var result = handshake.ToString();

        // Assert
        result.ShouldContain("ElasticsearchHandshake");
        result.ShouldContain("my_index");
    }

    [Fact]
    public void Dimension_ReturnsEmbeddingModelDimension()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(768);

        // Act
        var handshake = new ElasticsearchHandshake(embeddingModel);

        // Assert
        handshake.Dimension.ShouldBe(768);
    }
}
