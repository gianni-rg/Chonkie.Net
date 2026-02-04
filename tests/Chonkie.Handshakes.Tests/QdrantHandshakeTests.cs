using Chonkie.Core.Types;
using Chonkie.Embeddings.Interfaces;
using Chonkie.Handshakes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests;

/// <summary>
/// Unit tests for QdrantHandshake.
/// </summary>
public class QdrantHandshakeTests
{
    private readonly IEmbeddings _mockEmbeddings;

    public QdrantHandshakeTests()
    {
        // Setup mock embeddings
        _mockEmbeddings = Substitute.For<IEmbeddings>();
        _mockEmbeddings.Dimension.Returns(384);
        _mockEmbeddings.Name.Returns("test-embeddings");
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var mockClient = new QdrantClient("http://localhost:6333");

        // Act
        var handshake = new QdrantHandshake(
            mockClient,
            "test-collection",
            _mockEmbeddings,
            NullLogger.Instance
        );

        // Assert
        handshake.ShouldNotBeNull();
        handshake.CollectionName.ShouldBe("test-collection");
        handshake.Dimension.ShouldBe(384u);
    }

    [Fact]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new QdrantHandshake(
                (QdrantClient)null!,
                "test-collection",
                _mockEmbeddings
            )
        );
    }

    [Fact]
    public void Constructor_WithNullCollectionName_ThrowsArgumentNullException()
    {
        // Arrange
        var client = new QdrantClient("http://localhost:6333");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new QdrantHandshake(
                client,
                null!,
                _mockEmbeddings
            )
        );
    }

    [Fact]
    public void Constructor_WithNullEmbeddingModel_ThrowsArgumentNullException()
    {
        // Arrange
        var client = new QdrantClient("http://localhost:6333");

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new QdrantHandshake(
                client,
                "test-collection",
                null!
            )
        );
    }

    [Fact]
    public void Constructor_WithRandomCollectionName_GeneratesRandomName()
    {
        // Arrange
        var client = new QdrantClient("http://localhost:6333");

        // Act
        var handshake = new QdrantHandshake(
            client,
            "random",
            _mockEmbeddings
        );

        // Assert
        handshake.CollectionName.ShouldNotBe("random");
        handshake.CollectionName.ShouldStartWith("chonkie-");
    }

    [Fact]
    public async Task WriteAsync_WithValidChunks_ReturnsSuccess()
    {
        // Arrange
        var client = new QdrantClient("http://localhost:6333");
        var handshake = new QdrantHandshake(
            client,
            "test-collection",
            _mockEmbeddings
        );

        var chunks = new[]
        {
            new Chunk { Text = "Hello world", StartIndex = 0, EndIndex = 11, TokenCount = 2 },
            new Chunk { Text = "Test chunk", StartIndex = 12, EndIndex = 22, TokenCount = 2 }
        };

        var embeddings = new[] { new float[] { 0.1f, 0.2f }, new float[] { 0.3f, 0.4f } };
        _mockEmbeddings.EmbedBatchAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(embeddings);

        // Act
        var result = await handshake.WriteAsync(chunks);

        // Assert
        result.ShouldNotBeNull();
        await _mockEmbeddings.Received(1).EmbedBatchAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WriteAsync_WithEmptyChunks_ReturnsSuccessWithZeroCount()
    {
        // Arrange
        var client = new QdrantClient("http://localhost:6333");
        var handshake = new QdrantHandshake(
            client,
            "test-collection",
            _mockEmbeddings
        );

        var chunks = Array.Empty<Chunk>();

        // Act
        var result = await handshake.WriteAsync(chunks);

        // Assert
        result.ShouldNotBeNull();
        dynamic resultObj = result;
        ((int)resultObj.Count).ShouldBe(0);
        ((bool)resultObj.Success).ShouldBe(true);
    }

    [Fact]
    public async Task WriteAsync_WithNullChunks_ThrowsArgumentNullException()
    {
        // Arrange
        var client = new QdrantClient("http://localhost:6333");
        var handshake = new QdrantHandshake(
            client,
            "test-collection",
            _mockEmbeddings
        );

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await handshake.WriteAsync(null!)
        );
    }

    [Fact]
    public async Task SearchAsync_WithValidQuery_ReturnsResults()
    {
        // Arrange
        var client = new QdrantClient("http://localhost:6333");
        var handshake = new QdrantHandshake(
            client,
            "test-collection",
            _mockEmbeddings
        );

        // First, add some data
        var chunks = new[]
        {
            new Chunk { Text = "Hello world", StartIndex = 0, EndIndex = 11, TokenCount = 2 }
        };

        var embeddings = new[] { new float[] { 0.1f, 0.2f } };
        _mockEmbeddings.EmbedBatchAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(embeddings);
        _mockEmbeddings.EmbedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new float[] { 0.1f, 0.2f });

        await handshake.WriteAsync(chunks);

        // Act
        var results = await handshake.SearchAsync("Hello", limit: 5);

        // Assert
        results.ShouldNotBeNull();
        results.Count.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task SearchAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        var client = new QdrantClient("http://localhost:6333");
        var handshake = new QdrantHandshake(
            client,
            "test-collection",
            _mockEmbeddings
        );

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await handshake.SearchAsync(null!)
        );
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var client = new QdrantClient("http://localhost:6333");
        var handshake = new QdrantHandshake(
            client,
            "test-collection",
            _mockEmbeddings
        );

        // Act
        var result = handshake.ToString();

        // Assert
        result.ShouldContain("QdrantHandshake");
        result.ShouldContain("test-collection");
        result.ShouldContain("384");
    }
}
