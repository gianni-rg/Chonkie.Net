using Chonkie.Core.Types;
using Chonkie.Handshakes;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests;

/// <summary>
/// Unit tests for MilvusHandshake constructor validation and parameter handling.
/// </summary>
public class MilvusHandshakeTests
{
    [Fact]
    public void Constructor_WithDefaultParameters_InitializesSuccessfully()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act & Assert - should not throw
        var handshake = new MilvusHandshake(embeddingModel, "http://localhost:19530");

        handshake.ShouldNotBeNull();
        handshake.Dimension.ShouldBe(384);
    }

    [Fact]
    public void Constructor_WithNullEmbeddingModel_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new MilvusHandshake(null!));
    }

    [Fact]
    public void Constructor_WithCustomHostAndPort_InitializesSuccessfully()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new MilvusHandshake(
            embeddingModel,
            serverUrl: "http://milvus.example.com:19530");

        // Assert
        handshake.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithRandomCollectionName_GeneratesUniqueName()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new MilvusHandshake(
            embeddingModel,
            "http://localhost:19530",
            collectionName: "random");

        // Assert
        handshake.CollectionName.ShouldNotBe("random");
        handshake.CollectionName.ShouldStartWith("collection_");
    }

    [Fact]
    public void Constructor_WithCustomCollectionName_UsesProvidedName()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new MilvusHandshake(
            embeddingModel,
            "http://localhost:19530",
            collectionName: "my_collection");

        // Assert
        handshake.CollectionName.ShouldBe("my_collection");
    }

    [Fact]
    public void Constructor_WithServerUrl_InitializesSuccessfully()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(768);

        // Act
        var handshake = new MilvusHandshake(
            embeddingModel,
            serverUrl: "http://milvus-server:19530");

        // Assert
        handshake.Dimension.ShouldBe(768);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);
        var handshake = new MilvusHandshake(
            embeddingModel,
            "http://localhost:19530",
            collectionName: "test_collection");

        // Act
        var result = handshake.ToString();

        // Assert
        result.ShouldContain("MilvusHandshake");
        result.ShouldContain("test_collection");
    }

    [Fact]
    public void Dimension_ReturnsEmbeddingModelDimension()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(1024);

        // Act
        var handshake = new MilvusHandshake(embeddingModel, "http://localhost:19530");

        // Assert
        handshake.Dimension.ShouldBe(1024);
    }

    [Fact]
    public async Task SearchAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);
        var handshake = new MilvusHandshake(embeddingModel, "http://localhost:19530");

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            handshake.SearchAsync(null!));
    }

}
