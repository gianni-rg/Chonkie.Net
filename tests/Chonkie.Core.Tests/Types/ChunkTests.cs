using Chonkie.Core.Types;
using FluentAssertions;

namespace Chonkie.Core.Tests.Types;

public class ChunkTests
{
    [Fact]
    public void Constructor_WithRequiredText_CreatesChunk()
    {
        // Arrange & Act
        var chunk = new Chunk
        {
            Text = "Test chunk"
        };

        // Assert
        chunk.Text.Should().Be("Test chunk");
        chunk.Id.Should().StartWith("chnk_");
        chunk.StartIndex.Should().Be(0);
        chunk.EndIndex.Should().Be(0);
        chunk.TokenCount.Should().Be(0);
        chunk.Context.Should().BeNull();
        chunk.Embedding.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithAllProperties_CreatesChunk()
    {
        // Arrange & Act
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };
        var chunk = new Chunk
        {
            Id = "custom_id",
            Text = "Test chunk",
            StartIndex = 10,
            EndIndex = 20,
            TokenCount = 5,
            Context = "test context",
            Embedding = embedding
        };

        // Assert
        chunk.Id.Should().Be("custom_id");
        chunk.Text.Should().Be("Test chunk");
        chunk.StartIndex.Should().Be(10);
        chunk.EndIndex.Should().Be(20);
        chunk.TokenCount.Should().Be(5);
        chunk.Context.Should().Be("test context");
        chunk.Embedding.Should().BeSameAs(embedding);
    }

    [Fact]
    public void Length_ReturnsTextLength()
    {
        // Arrange
        var chunk = new Chunk { Text = "Hello World" };

        // Act & Assert
        chunk.Length.Should().Be(11);
    }

    [Fact]
    public void ToDictionary_ConvertsAllProperties()
    {
        // Arrange
        var chunk = new Chunk
        {
            Id = "test_id",
            Text = "Test",
            StartIndex = 5,
            EndIndex = 9,
            TokenCount = 1,
            Context = "context",
            Embedding = new float[] { 1.0f, 2.0f }
        };

        // Act
        var dict = chunk.ToDictionary();

        // Assert
        dict["Id"].Should().Be("test_id");
        dict["Text"].Should().Be("Test");
        dict["StartIndex"].Should().Be(5);
        dict["EndIndex"].Should().Be(9);
        dict["TokenCount"].Should().Be(1);
        dict["Context"].Should().Be("context");
        dict["Embedding"].Should().BeEquivalentTo(new float[] { 1.0f, 2.0f });
    }

    [Fact]
    public void FromDictionary_CreatesChunkFromDictionary()
    {
        // Arrange
        var dict = new Dictionary<string, object?>
        {
            ["Id"] = "test_id",
            ["Text"] = "Test",
            ["StartIndex"] = 5,
            ["EndIndex"] = 9,
            ["TokenCount"] = 1,
            ["Context"] = "context",
            ["Embedding"] = new float[] { 1.0f, 2.0f }
        };

        // Act
        var chunk = Chunk.FromDictionary(dict);

        // Assert
        chunk.Id.Should().Be("test_id");
        chunk.Text.Should().Be("Test");
        chunk.StartIndex.Should().Be(5);
        chunk.EndIndex.Should().Be(9);
        chunk.TokenCount.Should().Be(1);
        chunk.Context.Should().Be("context");
        chunk.Embedding.Should().BeEquivalentTo(new float[] { 1.0f, 2.0f });
    }

    [Fact]
    public void FromDictionary_WithMissingOptionalFields_UsesDefaults()
    {
        // Arrange
        var dict = new Dictionary<string, object?>
        {
            ["Text"] = "Test"
        };

        // Act
        var chunk = Chunk.FromDictionary(dict);

        // Assert
        chunk.Text.Should().Be("Test");
        chunk.Id.Should().StartWith("chnk_");
        chunk.StartIndex.Should().Be(0);
        chunk.EndIndex.Should().Be(0);
        chunk.TokenCount.Should().Be(0);
        chunk.Context.Should().BeNull();
        chunk.Embedding.Should().BeNull();
    }

    [Fact]
    public void Copy_CreatesShallowCopy()
    {
        // Arrange
        var original = new Chunk
        {
            Text = "Test",
            StartIndex = 1,
            EndIndex = 5,
            TokenCount = 2
        };

        // Act
        var copy = original.Copy();

        // Assert
        copy.Should().Be(original);
        copy.Should().NotBeSameAs(original);
    }

    [Fact]
    public void RecordEquality_ComparesValues()
    {
        // Arrange
        var chunk1 = new Chunk
        {
            Id = "same_id",
            Text = "Test",
            StartIndex = 0,
            EndIndex = 4,
            TokenCount = 1
        };

        var chunk2 = new Chunk
        {
            Id = "same_id",
            Text = "Test",
            StartIndex = 0,
            EndIndex = 4,
            TokenCount = 1
        };

        var chunk3 = new Chunk
        {
            Id = "different_id",
            Text = "Test",
            StartIndex = 0,
            EndIndex = 4,
            TokenCount = 1
        };

        // Act & Assert
        chunk1.Should().Be(chunk2);
        chunk1.Should().NotBe(chunk3);
    }
}
