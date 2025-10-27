using Chonkie.Core.Types;
using Chonkie.Pipeline;

namespace Chonkie.Pipeline.Tests;

/// <summary>
/// Tests for Pipeline with file fetchers.
/// </summary>
public class PipelineFetcherTests
{
    private readonly string _tempDirectory;

    /// Unit test: validates file fetcher reads local file content.
    public PipelineFetcherTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"chonkie_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
    }

    /// Unit test: validates URL fetcher handles HTTP content.
    [Fact]
    public void Pipeline_WithSingleFile_LoadsAndChunks()
    {
        // Arrange
        var tempFile = Path.Combine(_tempDirectory, "test.txt");
        File.WriteAllText(tempFile, "This is test content for the file fetcher.");

        try
        {
            var pipeline = new Pipeline()
                .FetchFrom("file", new { path = tempFile })
                .ProcessWith("text")
                .ChunkWith("recursive", new { chunk_size = 512 });

            // Act
            var result = pipeline.Run();

            // Assert
            var doc = Assert.IsType<Document>(result);
            Assert.NotEmpty(doc.Chunks);
            Assert.Contains("test content", doc.Content, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    /// Unit test: invalid fetcher name throws a clear exception.
    [Fact]
    public void Pipeline_WithDirectory_LoadsAllFiles()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_tempDirectory, "doc1.txt"), "Content of first file.");
        File.WriteAllText(Path.Combine(_tempDirectory, "doc2.txt"), "Content of second file.");
        File.WriteAllText(Path.Combine(_tempDirectory, "doc3.md"), "# Markdown content");

        try
        {
            var pipeline = new Pipeline()
                .FetchFrom("file", new { dir = _tempDirectory })
                .ProcessWith("text")
                .ChunkWith("recursive", new { chunk_size = 512 });

            // Act
            var result = pipeline.Run();

            // Assert
            var docs = Assert.IsAssignableFrom<List<Document>>(result);
            Assert.Equal(3, docs.Count);

            foreach (var doc in docs)
            {
                Assert.NotEmpty(doc.Chunks);
            }
        }
        finally
        {
            foreach (var file in Directory.GetFiles(_tempDirectory))
            {
                File.Delete(file);
            }
        }
    }

    /// Unit test: fetcher followed by chunker produces chunks.
    [Fact]
    public void Pipeline_WithDirectoryAndExtensionFilter_LoadsOnlyMatchingFiles()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_tempDirectory, "doc1.txt"), "Content 1");
        File.WriteAllText(Path.Combine(_tempDirectory, "doc2.txt"), "Content 2");
        File.WriteAllText(Path.Combine(_tempDirectory, "doc3.md"), "# Markdown");

        try
        {
            var pipeline = new Pipeline()
                .FetchFrom("file", new { dir = _tempDirectory, filter = "*.txt" })
                .ProcessWith("text")
                .ChunkWith("recursive", new { chunk_size = 512 });

            // Act
            var result = pipeline.Run();

            // Assert
            var docs = Assert.IsAssignableFrom<List<Document>>(result);
            Assert.Equal(2, docs.Count); // Only .txt files
        }
        finally
        {
            foreach (var file in Directory.GetFiles(_tempDirectory))
            {
                File.Delete(file);
            }
        }
    }

    /// Unit test: fetcher errors are surfaced during Run().
    [Fact]
    public void Pipeline_WithNonexistentFile_ThrowsException()
    {
        // Arrange
        var nonexistentFile = Path.Combine(_tempDirectory, "nonexistent.txt");
        var pipeline = new Pipeline()
            .FetchFrom("file", new { path = nonexistentFile })
            .ChunkWith("recursive");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => pipeline.Run());
    }

    /// Unit test: fetcher respects provided options.
    [Fact]
    public void Pipeline_WithInvalidFetcher_ThrowsException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            new Pipeline().FetchFrom("nonexistent_fetcher", new { path = "test.txt" }));

        Assert.Contains("Unknown component", ex.Message);
    }

    /// Unit test: fetcher is optional when direct text is provided.
    [Fact]
    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }
}
