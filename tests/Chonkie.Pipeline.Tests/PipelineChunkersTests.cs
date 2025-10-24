using Chonkie.Core.Types;
using Chonkie.Pipeline;

namespace Chonkie.Pipeline.Tests;

/// <summary>
/// Tests for Pipeline with different chunkers.
/// </summary>
public class PipelineChunkersTests
{
    /// <inheritdoc/>
    [Fact]
    public void Pipeline_WithTokenChunker_CreatesCorrectChunks()
    {
        // Arrange
        var text = "This is a test document with enough text to create multiple chunks when using token chunking.";
        var pipeline = new Pipeline()
            .ChunkWith("token", new { chunk_size = 100, chunk_overlap = 10 });

        // Act
        var result = pipeline.Run(texts: text);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);

        foreach (var chunk in doc.Chunks)
        {
            Assert.True(chunk.TokenCount <= 100);
        }
    }

    /// <inheritdoc/>
    [Fact]
    public void Pipeline_WithRecursiveChunker_CreatesChunks()
    {
        // Arrange
        var text = "Paragraph one.\n\nParagraph two.\n\nParagraph three.";
        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 256 });

        // Act
        var result = pipeline.Run(texts: text);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
    }

    /// <inheritdoc/>
    [Fact]
    public void Pipeline_WithSentenceChunker_CreatesChunks()
    {
        // Arrange
        var text = "First sentence. Second sentence. Third sentence. Fourth sentence.";
        var pipeline = new Pipeline()
            .ChunkWith("sentence", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: text);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
    }

    /// <inheritdoc/>
    [Fact]
    public void Pipeline_WithSemanticChunker_CreatesChunks()
    {
        // Arrange
        var text = "This is about artificial intelligence. " +
                   "Machine learning is a subset of AI. " +
                   "Now let's talk about cooking. " +
                   "Baking requires precision and patience.";

        var pipeline = new Pipeline()
            .ChunkWith("semantic", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: text);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
    }

    /// <inheritdoc/>
    [Fact]
    public void Pipeline_WithLateChunker_CreatesChunks()
    {
        // Arrange
        var text = "This is a test document for late chunking. " +
                   "Late chunking embeds then chunks the text. " +
                   "This creates better chunk boundaries.";

        var pipeline = new Pipeline()
            .ChunkWith("late", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: text);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);

        // Late chunker should add embeddings to chunks
        foreach (var chunk in doc.Chunks)
        {
            Assert.NotNull(chunk.Embedding);
            Assert.NotEmpty(chunk.Embedding);
        }
    }

    /// <inheritdoc/>
    [Fact]
    public void Pipeline_WithInvalidChunker_ThrowsException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new Pipeline().ChunkWith("nonexistent_chunker"));

        Assert.Contains("Unknown component", ex.Message);
    }

    /// <inheritdoc/>
    [Fact]
    public void Pipeline_WithDifferentChunkSizes_ProducesDifferentResults()
    {
        // Arrange
        // Create text with ~1000 tokens to ensure different chunk sizes produce different results
        var longText = string.Concat(Enumerable.Repeat("Test sentence with more content to ensure proper chunking behavior. ", 200));

        var pipeline256 = new Pipeline().ChunkWith("recursive", new { chunk_size = 256 });
        var pipeline512 = new Pipeline().ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result256 = pipeline256.Run(texts: longText);
        var result512 = pipeline512.Run(texts: longText);

        // Assert
        var doc256 = Assert.IsType<Document>(result256);
        var doc512 = Assert.IsType<Document>(result512);

        Assert.NotEqual(doc256.Chunks.Count, doc512.Chunks.Count);
        Assert.True(doc256.Chunks.Count > doc512.Chunks.Count);
    }

    /// <inheritdoc/>
    [Fact]
    public void Pipeline_WithSemanticChunker_CustomModelName_Works()
    {
        // Arrange
        var text = "AI systems can learn. ML is part of AI. Cooking is different.";
        var pipeline = new Pipeline()
            .ChunkWith("semantic", new { chunk_size = 256, embedding_model = "sentence-transformers/all-MiniLM-L6-v2" });

        // Act
        var result = pipeline.Run(texts: text);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
    }

    /// <inheritdoc/>
    [Fact]
    public void Pipeline_WithLateChunker_CustomModelName_Works()
    {
        // Arrange
        var text = "This text will be embedded before late chunking.";
        var pipeline = new Pipeline()
            .ChunkWith("late", new { chunk_size = 256, embedding_model = "sentence-transformers/all-MiniLM-L6-v2" });

        // Act
        var result = pipeline.Run(texts: text);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
        Assert.All(doc.Chunks, c => Assert.NotNull(c.Embedding));
    }
}
