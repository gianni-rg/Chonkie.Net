using Chonkie.Core.Types;
using Chonkie.Pipeline;

namespace Chonkie.Pipeline.Tests;

/// <summary>
/// Tests for basic Pipeline functionality.
/// </summary>
public class PipelineBasicsTests
{
    /// <inheritdoc/>
    [Fact]
    public void Pipeline_CanBeInstantiated()
    {
        // Act
        var pipeline = new Pipeline();

        // Assert
        Assert.NotNull(pipeline);
    }

    /// <inheritdoc/>
    [Fact]
    public void Pipeline_WithDirectTextInput_ReturnsDocument()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: "This is a test document for chunking.");

        // Assert
        Assert.NotNull(result);
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
        Assert.Equal("This is a test document for chunking.", doc.Content);
    }

    /// <inheritdoc/>
    [Fact]
    public void Pipeline_WithMultipleTexts_ReturnsListOfDocuments()
    {
        // Arrange
        var texts = new[]
        {
            "First document text.",
            "Second document text.",
            "Third document text."
        };

        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: texts);

        // Assert
        Assert.NotNull(result);
        var docs = Assert.IsAssignableFrom<List<Document>>(result);
        Assert.Equal(3, docs.Count);

        foreach (var doc in docs)
        {
            Assert.NotNull(doc);
            Assert.NotEmpty(doc.Chunks);
        }
    }

    /// <inheritdoc/>
    [Fact]
    public void Pipeline_RequiresChunker_ThrowsException()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ProcessWith("text");

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            pipeline.Run(texts: "test"));
        Assert.Contains("must include a chunker", ex.Message);
    }

    /// <inheritdoc/>
    [Fact]
    public void Pipeline_RequiresInput_ThrowsException()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive");

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            pipeline.Run());
        Assert.Contains("must include a fetcher", ex.Message);
    }

    /// <inheritdoc/>
    [Fact]
    public void Pipeline_WithNoSteps_ThrowsException()
    {
        // Arrange
        var pipeline = new Pipeline();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            pipeline.Run(texts: "test"));
        Assert.Contains("no steps", ex.Message);
    }

    /// <inheritdoc/>
    [Fact]
    public void Pipeline_Describe_ReturnsReadableString()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ProcessWith("text")
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var description = pipeline.Describe();

        // Assert
        Assert.NotNull(description);
        Assert.Contains("process", description.ToLower());
        Assert.Contains("chunk", description.ToLower());
    }

    /// <inheritdoc/>
    [Fact]
    public void Pipeline_Reset_ClearsAllSteps()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive")
            .ProcessWith("text");

        // Act
        pipeline.Reset();

        // Assert
        var description = pipeline.Describe();
        Assert.Equal("Empty pipeline", description);
    }
}
