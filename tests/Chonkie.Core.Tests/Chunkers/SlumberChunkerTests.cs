namespace Chonkie.Core.Tests.Chunkers;

using Chonkie.Chunkers;
using Chonkie.Tokenizers;
using Xunit;

public class SlumberChunkerTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act
        var chunker = new SlumberChunker(tokenizer, 1024, ExtractionMode.Json);

        // Assert
        Assert.Equal(1024, chunker.ChunkSize);
        Assert.Equal(ExtractionMode.Json, chunker.ExtractionMode);
    }

    [Fact]
    public void Constructor_WithDefaultParameters_ShouldUseDefaults()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act
        var chunker = new SlumberChunker(tokenizer);

        // Assert
        Assert.Equal(2048, chunker.ChunkSize);
        Assert.Equal(ExtractionMode.Auto, chunker.ExtractionMode);
    }

    [Fact]
    public void Constructor_WithZeroChunkSize_ShouldThrowArgumentException()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new SlumberChunker(tokenizer, 0));
        Assert.Equal("chunkSize", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNegativeChunkSize_ShouldThrowArgumentException()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new SlumberChunker(tokenizer, -1));
        Assert.Equal("chunkSize", ex.ParamName);
    }

    [Theory]
    [InlineData(ExtractionMode.Json)]
    [InlineData(ExtractionMode.Text)]
    [InlineData(ExtractionMode.Auto)]
    public void Constructor_WithDifferentExtractionModes_ShouldSucceed(ExtractionMode mode)
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act
        var chunker = new SlumberChunker(tokenizer, 512, mode);

        // Assert
        Assert.Equal(mode, chunker.ExtractionMode);
        Assert.Equal(512, chunker.ChunkSize);
    }

    [Fact]
    public void Chunk_WithSimpleText_ShouldReturnChunks()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SlumberChunker(tokenizer, 512, ExtractionMode.Text);
        var text = "This is a test text that will be chunked.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotNull(chunks);
        Assert.NotEmpty(chunks);
    }

    [Fact]
    public void Chunk_WithEmptyText_ShouldReturnEmptyList()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SlumberChunker(tokenizer, 512, ExtractionMode.Json);

        // Act
        var chunks = chunker.Chunk(string.Empty);

        // Assert
        Assert.NotNull(chunks);
        Assert.Empty(chunks);
    }

    [Fact]
    public void Chunk_WithLongText_ShouldCreateMultipleChunks()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SlumberChunker(tokenizer, 50, ExtractionMode.Auto);

        // Create text that will exceed chunk size
        var text = string.Join(" ", Enumerable.Range(1, 200).Select(i => $"word{i}"));

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotNull(chunks);
        Assert.True(chunks.Count > 1);
    }

    [Fact]
    public void ToString_ShouldIncludeChunkSizeAndExtractionMode()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SlumberChunker(tokenizer, 1024, ExtractionMode.Json);

        // Act
        var result = chunker.ToString();

        // Assert
        Assert.Contains("1024", result);
        Assert.Contains("Json", result);
        Assert.Contains("fallback", result);
    }

    [Theory]
    [InlineData(256, ExtractionMode.Text)]
    [InlineData(512, ExtractionMode.Json)]
    [InlineData(1024, ExtractionMode.Auto)]
    [InlineData(2048, ExtractionMode.Text)]
    [InlineData(4096, ExtractionMode.Json)]
    public void ToString_WithVariousConfigurations_ShouldReflectSettings(int chunkSize, ExtractionMode mode)
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SlumberChunker(tokenizer, chunkSize, mode);

        // Act
        var result = chunker.ToString();

        // Assert
        Assert.Contains(chunkSize.ToString(), result);
        Assert.Contains(mode.ToString(), result);
    }

    [Fact]
    public void Chunk_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SlumberChunker(tokenizer, 512, ExtractionMode.Text);
        var text = "Hello! This is a test. Does it work? Yes, it does!\n\nNew paragraph here.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotNull(chunks);
        Assert.NotEmpty(chunks);
        Assert.True(chunks.All(c => !string.IsNullOrEmpty(c.Text)));
    }

    [Fact]
    public void Chunk_PreservesTextContent()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SlumberChunker(tokenizer, 100, ExtractionMode.Json);
        var text = "First sentence. Second sentence. Third sentence.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        var reconstructed = string.Concat(chunks.Select(c => c.Text));
        Assert.Equal(text, reconstructed);
    }

    [Fact]
    public void Chunk_WithDifferentTokenizers_ShouldWork()
    {
        // Arrange
        var text = "This is test text for chunking.";

        // Test with CharacterTokenizer
        var charTokenizer = new CharacterTokenizer();
        var charChunker = new SlumberChunker(charTokenizer, 20, ExtractionMode.Auto);
        var charChunks = charChunker.Chunk(text);

        // Assert
        Assert.NotNull(charChunks);
        Assert.NotEmpty(charChunks);
    }

    [Fact]
    public void ExtractionMode_JsonMode_ShouldBeSupported()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act
        var chunker = new SlumberChunker(tokenizer, 512, ExtractionMode.Json);

        // Assert
        Assert.Equal(ExtractionMode.Json, chunker.ExtractionMode);
        Assert.Contains("Json", chunker.ToString());
    }

    [Fact]
    public void ExtractionMode_TextMode_ShouldBeSupported()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act
        var chunker = new SlumberChunker(tokenizer, 512, ExtractionMode.Text);

        // Assert
        Assert.Equal(ExtractionMode.Text, chunker.ExtractionMode);
        Assert.Contains("Text", chunker.ToString());
    }

    [Fact]
    public void ExtractionMode_AutoMode_ShouldBeDefault()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act
        var chunker = new SlumberChunker(tokenizer);

        // Assert
        Assert.Equal(ExtractionMode.Auto, chunker.ExtractionMode);
        Assert.Contains("Auto", chunker.ToString());
    }
}

