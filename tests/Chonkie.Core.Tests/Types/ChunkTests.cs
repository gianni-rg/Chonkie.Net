using Chonkie.Core.Types;

namespace Chonkie.Core.Tests.Types;

/// <summary>
/// Unit tests for the <see cref="Chunk"/> type.
/// </summary>
public class ChunkTests
{
    [Fact]
    /// <summary>
    /// Tests that the constructor creates a chunk with required text.
    /// </summary>
    public void Constructor_WithRequiredText_CreatesChunk()
    {
        // Arrange & Act
        var chunk = new Chunk
        {
            Text = "Test chunk"
        };

        // Assert
        Assert.Equal("Test chunk", chunk.Text);
        Assert.StartsWith("chnk_", chunk.Id);
        Assert.Equal(0, chunk.StartIndex);
        Assert.Equal(0, chunk.EndIndex);
        Assert.Equal(0, chunk.TokenCount);
        Assert.Null(chunk.Context);
        Assert.Null(chunk.Embedding);
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
        Assert.Equal("custom_id", chunk.Id);
        Assert.Equal("Test chunk", chunk.Text);
        Assert.Equal(10, chunk.StartIndex);
        Assert.Equal(20, chunk.EndIndex);
        Assert.Equal(5, chunk.TokenCount);
        Assert.Equal("test context", chunk.Context);
        Assert.Same(embedding, chunk.Embedding);
    }

    [Fact]
    public void Length_ReturnsTextLength()
    {
        // Arrange
        var chunk = new Chunk { Text = "Hello World" };

        // Act & Assert
        Assert.Equal(11, chunk.Length);
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
        Assert.Equal("test_id", dict["Id"]);
        Assert.Equal("Test", dict["Text"]);
        Assert.Equal(5, dict["StartIndex"]);
        Assert.Equal(9, dict["EndIndex"]);
        Assert.Equal(1, dict["TokenCount"]);
        Assert.Equal("context", dict["Context"]);
        Assert.Equal(new float[] { 1.0f, 2.0f }, dict["Embedding"]);
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
        Assert.Equal("test_id", chunk.Id);
        Assert.Equal("Test", chunk.Text);
        Assert.Equal(5, chunk.StartIndex);
        Assert.Equal(9, chunk.EndIndex);
        Assert.Equal(1, chunk.TokenCount);
        Assert.Equal("context", chunk.Context);
        Assert.Equal(new float[] { 1.0f, 2.0f }, chunk.Embedding);
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
        Assert.Equal("Test", chunk.Text);
        Assert.StartsWith("chnk_", chunk.Id);
        Assert.Equal(0, chunk.StartIndex);
        Assert.Equal(0, chunk.EndIndex);
        Assert.Equal(0, chunk.TokenCount);
        Assert.Null(chunk.Context);
        Assert.Null(chunk.Embedding);
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
        Assert.Equal(original, copy);
        Assert.NotSame(original, copy);
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
        Assert.Equal(chunk1, chunk2);
        Assert.NotEqual(chunk1, chunk3);
    }
}
