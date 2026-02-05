using Chonkie.Core.Types;
using Chonkie.Embeddings.Interfaces;
using Chonkie.Handshakes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Pinecone;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests;

/// <summary>
/// Unit tests for PineconeHandshake.
/// </summary>
public class PineconeHandshakeTests
{
    private readonly IEmbeddings _mockEmbeddings;

    public PineconeHandshakeTests()
    {
        // Setup mock embeddings
        _mockEmbeddings = Substitute.For<IEmbeddings>();
        _mockEmbeddings.Dimension.Returns(384);
        _mockEmbeddings.Name.Returns("test-embeddings");
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange & Act
        var handshake = new PineconeHandshake(
            "test-api-key",
            "test-index",
            _mockEmbeddings,
            "default",
            NullLogger.Instance
        );

        // Assert
        handshake.ShouldNotBeNull();
        handshake.IndexName.ShouldBe("test-index");
        handshake.Namespace.ShouldBe("default");
        handshake.Dimension.ShouldBe(384);
    }

    [Fact]
    public void Constructor_WithNullApiKey_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new PineconeHandshake(
                (string)null!,
                "test-index",
                _mockEmbeddings
            )
        );
    }

    [Fact]
    public void Constructor_WithNullIndexName_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new PineconeHandshake(
                "test-api-key",
                null!,
                _mockEmbeddings
            )
        );
    }

    [Fact]
    public void Constructor_WithNullEmbeddingModel_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new PineconeHandshake(
                "test-api-key",
                "test-index",
                null!
            )
        );
    }

    [Fact]
    public void Constructor_WithNullNamespace_UsesEmptyString()
    {
        // Arrange & Act
        var handshake = new PineconeHandshake(
            "test-api-key",
            "test-index",
            _mockEmbeddings,
            null,
            NullLogger.Instance
        );

        // Assert
        handshake.Namespace.ShouldBe(string.Empty);
    }

    [Fact]
    public void Constructor_WithClientConstructor_CreatesInstance()
    {
        // Arrange
        var client = new PineconeClient("test-api-key");

        // Act
        var handshake = new PineconeHandshake(
            client,
            "test-index",
            _mockEmbeddings,
            "default",
            NullLogger.Instance
        );

        // Assert
        handshake.ShouldNotBeNull();
        handshake.IndexName.ShouldBe("test-index");
        handshake.Namespace.ShouldBe("default");
    }

    [Fact]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new PineconeHandshake(
                (PineconeClient)null!,
                "test-index",
                _mockEmbeddings
            )
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithInvalidApiKey_ThrowsArgumentException(string apiKey)
    {
        // Arrange, Act & Assert
        Should.Throw<ArgumentException>(() =>
            new PineconeHandshake(
                apiKey,
                "test-index",
                _mockEmbeddings
            )
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithInvalidIndexName_ThrowsArgumentException(string indexName)
    {
        // Arrange, Act & Assert
        Should.Throw<ArgumentException>(() =>
            new PineconeHandshake(
                "test-api-key",
                indexName,
                _mockEmbeddings
            )
        );
    }

    [Fact]
    public void Dimension_ReturnsEmbeddingDimension()
    {
        // Arrange
        var handshake = new PineconeHandshake(
            "test-api-key",
            "test-index",
            _mockEmbeddings
        );

        // Act
        var dimension = handshake.Dimension;

        // Assert
        dimension.ShouldBe(384);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var handshake = new PineconeHandshake(
            "test-api-key",
            "test-index",
            _mockEmbeddings,
            "default"
        );

        // Act
        var result = handshake.ToString();

        // Assert
        result.ShouldContain("test-index");
        result.ShouldContain("default");
        result.ShouldContain("384");
    }

    [Fact]
    public void ToString_WithEmptyNamespace_ReturnsFormattedString()
    {
        // Arrange
        var handshake = new PineconeHandshake(
            "test-api-key",
            "test-index",
            _mockEmbeddings
        );

        // Act
        var result = handshake.ToString();

        // Assert
        result.ShouldContain("test-index");
        result.ShouldContain("384");
    }

    [Fact]
    public async Task SearchAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        var handshake = new PineconeHandshake(
            "test-api-key",
            "test-index",
            _mockEmbeddings
        );

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await handshake.SearchAsync(null!)
        );
    }

    [Fact]
    public async Task WriteAsync_WithNullChunks_ThrowsArgumentNullException()
    {
        // Arrange
        var handshake = new PineconeHandshake(
            "test-api-key",
            "test-index",
            _mockEmbeddings
        );

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await handshake.WriteAsync(null!)
        );
    }

    [Fact]
    public async Task WriteAsync_WithEmptyChunks_ReturnsSuccessWithZeroCount()
    {
        // Arrange
        var handshake = new PineconeHandshake(
            "test-api-key",
            "test-index",
            _mockEmbeddings
        );

        var chunks = Array.Empty<Chunk>();

        // Act
        var result = await handshake.WriteAsync(chunks);

        // Assert
        result.ShouldNotBeNull();
        var resultType = result.GetType();
        var countProperty = resultType.GetProperty("Count");
        var successProperty = resultType.GetProperty("Success");
        
        countProperty.ShouldNotBeNull();
        successProperty.ShouldNotBeNull();
        
        var countValue = (int?)countProperty.GetValue(result);
        var successValue = (bool?)successProperty.GetValue(result);
        
        countValue.ShouldBe(0);
        successValue.ShouldBe(true);
    }
}
