using Chonkie.Core.Types;
using Chonkie.Embeddings;
using Chonkie.Handshakes;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests;

/// <summary>
/// Unit tests for ChromaHandshake constructor validation, parameter handling, and search functionality.
/// </summary>
public class ChromaHandshakeTests
{
    [Fact]
    public void Constructor_WithValidParameters_InitializesSuccessfully()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act & Assert - should not throw
        var handshake = new ChromaHandshake("test_collection", embeddingModel);
        handshake.ShouldNotBeNull();
        handshake.CollectionName.ShouldBe("test_collection");
    }

    [Fact]
    public void Constructor_WithNullCollectionName_ThrowsArgumentNullException()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new ChromaHandshake(null!, embeddingModel));
    }

    [Fact]
    public void Constructor_WithNullEmbeddingModel_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new ChromaHandshake("collection", null!));
    }

    [Fact]
    public void Constructor_WithRandomCollectionName_GeneratesUniqueName()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new ChromaHandshake("random", embeddingModel);

        // Assert
        handshake.CollectionName.ShouldNotBe("random");
        handshake.CollectionName.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Constructor_WithEmptyCollectionName_SucceedsWithEmptyName()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act - empty string is allowed, but not null
        var handshake = new ChromaHandshake(string.Empty, embeddingModel);

        // Assert
        handshake.ShouldNotBeNull();
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);
        var handshake = new ChromaHandshake("my_collection", embeddingModel);

        // Act
        var result = handshake.ToString();

        // Assert
        result.ShouldContain("ChromaHandshake");
        result.ShouldContain("my_collection");
    }

    [Fact]
    public async Task SearchAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);
        var handshake = new ChromaHandshake("test_collection", embeddingModel);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            handshake.SearchAsync(null!));
    }
}
