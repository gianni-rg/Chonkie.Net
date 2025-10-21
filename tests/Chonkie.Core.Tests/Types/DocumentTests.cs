using Chonkie.Core.Types;

namespace Chonkie.Core.Tests.Types;

public class DocumentTests
{
    [Fact]
    public void Constructor_WithRequiredContent_CreatesDocument()
    {
        // Arrange & Act
        var document = new Document
        {
            Content = "Test content"
        };

        // Assert
        Assert.Equal("Test content", document.Content);
        Assert.StartsWith("doc_", document.Id);
        Assert.Empty(document.Chunks);
        Assert.Empty(document.Metadata);
        Assert.Null(document.Source);
    }

    [Fact]
    public void Constructor_WithAllProperties_CreatesDocument()
    {
        // Arrange
        var chunks = new List<Chunk>
        {
            new Chunk { Text = "Chunk 1" },
            new Chunk { Text = "Chunk 2" }
        };
        var metadata = new Dictionary<string, object>
        {
            ["key1"] = "value1",
            ["key2"] = 42
        };

        // Act
        var document = new Document
        {
            Id = "custom_doc_id",
            Content = "Test content",
            Chunks = chunks,
            Metadata = metadata,
            Source = "test.txt"
        };

        // Assert
        Assert.Equal("custom_doc_id", document.Id);
        Assert.Equal("Test content", document.Content);
        Assert.Equal(2, document.Chunks.Count);
        Assert.Equal(2, document.Metadata.Count);
        Assert.Equal("test.txt", document.Source);
    }

    [Fact]
    public void Chunks_CanBeModified()
    {
        // Arrange
        var document = new Document { Content = "Test" };
        var chunk = new Chunk { Text = "Test chunk" };

        // Act
        document.Chunks.Add(chunk);

        // Assert
        Assert.Single(document.Chunks);
        Assert.Same(chunk, document.Chunks[0]);
    }

    [Fact]
    public void Metadata_CanBeModified()
    {
        // Arrange
        var document = new Document { Content = "Test" };
        var now = DateTime.Now;

        // Act
        document.Metadata["author"] = "Test Author";
        document.Metadata["date"] = now;

        // Assert
        Assert.Equal(2, document.Metadata.Count);
        Assert.Equal("Test Author", document.Metadata["author"]);
    }
}
