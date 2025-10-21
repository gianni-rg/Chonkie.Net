using Chonkie.Core.Types;
using FluentAssertions;

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
        document.Content.Should().Be("Test content");
        document.Id.Should().StartWith("doc_");
        document.Chunks.Should().BeEmpty();
        document.Metadata.Should().BeEmpty();
        document.Source.Should().BeNull();
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
        document.Id.Should().Be("custom_doc_id");
        document.Content.Should().Be("Test content");
        document.Chunks.Should().HaveCount(2);
        document.Metadata.Should().HaveCount(2);
        document.Source.Should().Be("test.txt");
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
        document.Chunks.Should().ContainSingle();
        document.Chunks[0].Should().BeSameAs(chunk);
    }

    [Fact]
    public void Metadata_CanBeModified()
    {
        // Arrange
        var document = new Document { Content = "Test" };

        // Act
        document.Metadata["author"] = "Test Author";
        document.Metadata["date"] = DateTime.Now;

        // Assert
        document.Metadata.Should().HaveCount(2);
        document.Metadata["author"].Should().Be("Test Author");
    }
}
