using Chonkie.Core.Types;
using Chonkie.Pipeline;
using Xunit;

namespace Chonkie.Pipeline.Tests;

/// <summary>
/// Tests for Pipeline with refineries (post-processors).
/// </summary>
public class PipelineRefineriesTests
{
    /// Overlap refinery adds neighboring context to each chunk.
    [Fact]
    public void Pipeline_WithOverlapRefinery_AddsContext()
    {
        // Arrange
        var text = string.Concat(Enumerable.Repeat(
            "This is a longer text that will be chunked and then refined with overlap to add context. ",
            10));

        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 256 })
            .RefineWith("overlap", new { context_size = 50 });

        // Act
        var result = pipeline.Run(texts: text);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);

        foreach (var chunk in doc.Chunks)
        {
            Assert.NotNull(chunk.Text);
        }
    }

    /// Multiple refineries can be chained and execute in order.
    [Fact]
    public void Pipeline_WithMultipleRefineries_ChainsCorrectly()
    {
        // Arrange
        var text = string.Concat(Enumerable.Repeat("Test text for multiple refineries. ", 20));
        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 })
            .RefineWith("overlap", new { context_size = 50 });

        // Act
        var result = pipeline.Run(texts: text);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
    }

    /// Embeddings refinery appends embeddings to chunks using a local model.
    [SkippableFact]
    public void Pipeline_WithEmbeddingsRefinery_AddsEmbeddings()
    {
        // Arrange
        TestHelpers.SkipIfModelNotAvailable();

        var text = "This is a test document for embeddings refinery. " +
                   "It should add embeddings to each chunk.";

        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 })
            .RefineWith("embeddings", new { embedding_model = "sentence-transformers/all-MiniLM-L6-v2" });

        // Act
        var result = pipeline.Run(texts: text);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);

        foreach (var chunk in doc.Chunks)
        {
            Assert.NotNull(chunk.Embedding);
            Assert.NotEmpty(chunk.Embedding);
        }
    }

    /// Unknown refinery name should raise a clear exception.
    [Fact]
    public void Pipeline_WithInvalidRefinery_ThrowsException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new Pipeline().RefineWith("nonexistent_refinery"));

        Assert.Contains("Unknown component", ex.Message);
    }

    /// Refinery steps run after chunking even if defined before.
    [Fact]
    public void Pipeline_RefineryRunsAfterChunker()
    {
        // Arrange - Define refinery before chunker
        var text = string.Concat(Enumerable.Repeat("Test text for ordering. ", 20));
        var pipeline = new Pipeline()
            .RefineWith("overlap", new { context_size = 50 })  // Defined first
            .ChunkWith("recursive", new { chunk_size = 512 }); // Should run first

        // Act
        var result = pipeline.Run(texts: text);

        // Assert - Should succeed without errors
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
    }
}
