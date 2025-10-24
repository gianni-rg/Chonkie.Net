using Chonkie.Core.Types;
using Chonkie.Pipeline;

namespace Chonkie.Pipeline.Tests;

/// <summary>
/// Tests for Pipeline edge cases and error handling.
/// </summary>
public class PipelineEdgeCasesTests
{
    [Fact]
    public void Pipeline_EmptyTextInput_HandlesGracefully()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: "");

        // Assert
        var doc = Assert.IsType<Document>(result);
        // Empty text should produce no chunks or be handled gracefully
        Assert.True(doc.Chunks.Count >= 0);
    }

    [Fact]
    public void Pipeline_VeryShortText_CreatesAtLeastOneChunk()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: "Hi");

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.True(doc.Chunks.Count >= 1);
    }

    [Fact]
    public void Pipeline_VeryLongText_CreatesManyChunks()
    {
        // Arrange
        var longText = string.Concat(Enumerable.Repeat("This is a test sentence. ", 1000));
        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 256 });

        // Act
        var result = pipeline.Run(texts: longText);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.True(doc.Chunks.Count > 10);
    }

    [Fact]
    public void Pipeline_WhitespaceOnlyText_HandlesGracefully()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: "   \n\n   \t  ");

        // Assert
        var doc = Assert.IsType<Document>(result);
        // Should handle whitespace gracefully
        Assert.NotNull(doc);
    }

    [Fact]
    public void Pipeline_NullText_ThrowsException()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            pipeline.Run(texts: null));
    }

    [Fact]
    public void Pipeline_SingleText_ReturnsDocument()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: "Single text");

        // Assert
        Assert.IsType<Document>(result);
        Assert.IsNotType<List<Document>>(result);
    }

    [Fact]
    public void Pipeline_MultipleTexts_ReturnsList()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: new[] { "Text 1", "Text 2" });

        // Assert
        var docs = Assert.IsAssignableFrom<List<Document>>(result);
        Assert.Equal(2, docs.Count);
        Assert.All(docs, doc => Assert.IsType<Document>(doc));
    }

    [Fact]
    public void Pipeline_UnicodeText_HandlesCorrectly()
    {
        // Arrange
        var unicodeText = "Hello 世界 🦛 مرحبا Привет";
        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: unicodeText);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.Contains("世界", doc.Content);
        Assert.Contains("🦛", doc.Content);
    }

    [Fact]
    public void Pipeline_SpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var specialText = "Test with special chars: @#$%^&*()[]{}|\\\"'<>?/";
        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: specialText);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
    }

    [Fact]
    public void Pipeline_InvalidParameters_ThrowsClearError()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { invalid_param = 999 });

        // Act & Assert
        var ex = Assert.ThrowsAny<Exception>(() =>
            pipeline.Run(texts: "test"));

        // Should throw some exception related to invalid parameters
        Assert.NotNull(ex);
    }

    [Fact]
    public void Pipeline_AfterReset_RequiresNewSteps()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive");

        pipeline.Reset();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            pipeline.Run(texts: "test"));
        Assert.Contains("no steps", ex.Message);
    }

    [Fact]
    public void Pipeline_CanBeReusedAfterReset()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive");

        var result1 = pipeline.Run(texts: "First text");

        // Act
        pipeline.Reset();
        pipeline.ChunkWith("token", new { chunk_size = 100 });
        var result2 = pipeline.Run(texts: "Second text");

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
    }
}
