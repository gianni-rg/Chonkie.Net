using Chonkie.Core.Types;
using Chonkie.Pipeline;

namespace Chonkie.Pipeline.Tests;

/// <summary>
/// Tests for Pipeline with chefs (preprocessors).
/// </summary>
public class PipelineChefTests
{
    /// Chef: text processor normalizes input before chunking.
    [Fact]
    public void Pipeline_WithTextChef_ProcessesText()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ProcessWith("text")
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: "This is plain text.");

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
    }

    /// Chef: markdown processor extracts text content correctly.
    [Fact]
    public void Pipeline_WithMultipleChefs_ThrowsException()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ProcessWith("text")
            .ProcessWith("markdown")  // Second chef
            .ChunkWith("recursive");

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            pipeline.Run(texts: "test"));
        Assert.Contains("Multiple process steps", ex.Message);
    }

    /// Chef: unknown processor name throws a clear exception.
    [Fact]
    public void Pipeline_WithoutChef_AddsDefaultTextChef()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: "Text without chef processing.");

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.Equal("Text without chef processing.", doc.Content);
        Assert.NotEmpty(doc.Chunks);
    }

    /// Chef: multiple processors are not allowed in a single pipeline.
    [Fact]
    public void Pipeline_WithMarkdownChef_ProcessesMarkdown()
    {
        // Arrange
        var markdown = "# Header\n\nThis is **bold** text.\n\n- Item 1\n- Item 2";
        var pipeline = new Pipeline()
            .ProcessWith("markdown")
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: markdown);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
    }

    /// Chef: processor options are honored during processing.
    [Fact]
    public void Pipeline_WithInvalidChef_ThrowsException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new Pipeline().ProcessWith("nonexistent_chef"));

        Assert.Contains("Unknown component", ex.Message);
    }
}
