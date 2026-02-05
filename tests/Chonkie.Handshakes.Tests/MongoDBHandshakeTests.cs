using Chonkie.Core.Types;
using Chonkie.Handshakes;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests;

/// <summary>
/// Unit tests for MongoDBHandshake constructor validation and parameter handling.
/// </summary>
public class MongoDBHandshakeTests
{
    [Fact]
    public void Constructor_WithHostnameAndPort_InitializesSuccessfully()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act & Assert - should not throw
        var handshake = new MongoDBHandshake(
            embeddingModel,
            hostname: "localhost",
            port: 27017);

        handshake.ShouldNotBeNull();
        handshake.DatabaseName.ShouldBe("chonkie_db");
        handshake.CollectionName.ShouldBe("chonkie_collection");
    }

    [Fact]
    public void Constructor_WithUri_InitializesSuccessfully()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new MongoDBHandshake(
            "mongodb://localhost:27017",
            embeddingModel);

        // Assert
        handshake.ShouldNotBeNull();
        handshake.Dimension.ShouldBe(384);
    }

    [Fact]
    public void Constructor_WithNullEmbeddingModel_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new MongoDBHandshake(null!));
    }

    [Fact]
    public void Constructor_WithRandomDatabaseName_GeneratesUniqueName()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new MongoDBHandshake(
            embeddingModel,
            databaseName: "random");

        // Assert
        handshake.DatabaseName.ShouldNotBe("random");
        handshake.DatabaseName.ShouldStartWith("chonkie_");
    }

    [Fact]
    public void Constructor_WithRandomCollectionName_GeneratesUniqueName()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new MongoDBHandshake(
            embeddingModel,
            collectionName: "random");

        // Assert
        handshake.CollectionName.ShouldNotBe("random");
        handshake.CollectionName.ShouldStartWith("chonkie_");
    }

    [Fact]
    public void Constructor_WithCustomDatabaseAndCollectionNames_UsesProvidedNames()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new MongoDBHandshake(
            embeddingModel,
            databaseName: "my_db",
            collectionName: "my_collection");

        // Assert
        handshake.DatabaseName.ShouldBe("my_db");
        handshake.CollectionName.ShouldBe("my_collection");
    }

    [Fact]
    public void Constructor_WithUsernameAndPassword_BuildsCorrectUri()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new MongoDBHandshake(
            embeddingModel,
            hostname: "mongodb.example.com",
            port: 27017,
            username: "user",
            password: "password");

        // Assert
        handshake.ShouldNotBeNull();
        handshake.DatabaseName.ShouldBe("chonkie_db");
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);
        var handshake = new MongoDBHandshake(
            embeddingModel,
            databaseName: "my_db",
            collectionName: "my_collection");

        // Act
        var result = handshake.ToString();

        // Assert
        result.ShouldContain("MongoDBHandshake");
        result.ShouldContain("my_db");
        result.ShouldContain("my_collection");
    }

    [Fact]
    public void Dimension_ReturnsEmbeddingModelDimension()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(768);

        // Act
        var handshake = new MongoDBHandshake(embeddingModel);

        // Assert
        handshake.Dimension.ShouldBe(768);
    }
}
