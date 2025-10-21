namespace Chonkie.Core.Tests.Chunkers;

using Chonkie.Chunkers;
using Chonkie.Tokenizers;
using FluentAssertions;
using Xunit;

public class TokenChunkerTests
{
    [Fact]
    public void TokenChunker_ShouldInitializeWithDefaultParameters()
    {
        // Arrange & Act
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer);

        // Assert
        chunker.ChunkSize.Should().Be(2048);
        chunker.ChunkOverlap.Should().Be(0);
    }

    [Fact]
    public void TokenChunker_ShouldInitializeWithCustomParameters()
    {
        // Arrange & Act
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 512, chunkOverlap: 50);

        // Assert
        chunker.ChunkSize.Should().Be(512);
        chunker.ChunkOverlap.Should().Be(50);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TokenChunker_ShouldThrowOnInvalidChunkSize(int chunkSize)
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act & Assert
        var act = () => new TokenChunker(tokenizer, chunkSize: chunkSize);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*chunk_size must be positive*");
    }

    [Fact]
    public void TokenChunker_ShouldThrowWhenOverlapExceedsChunkSize()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act & Assert
        var act = () => new TokenChunker(tokenizer, chunkSize: 100, chunkOverlap: 100);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*chunk_overlap must be less than chunk_size*");
    }

    [Fact]
    public void Chunk_ShouldReturnEmptyListForEmptyText()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 10);

        // Act
        var chunks = chunker.Chunk("");

        // Assert
        chunks.Should().BeEmpty();
    }

    [Fact]
    public void Chunk_ShouldReturnEmptyListForWhitespaceText()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 10);

        // Act
        var chunks = chunker.Chunk("   \t\n  ");

        // Assert
        chunks.Should().BeEmpty();
    }

    [Fact]
    public void Chunk_ShouldCreateSingleChunkForShortText()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 100);
        var text = "Hello World";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().ContainSingle();
        chunks[0].Text.Should().Be(text);
        chunks[0].TokenCount.Should().Be(11); // 11 characters
        chunks[0].StartIndex.Should().Be(0);
        chunks[0].EndIndex.Should().Be(11);
    }

    [Fact]
    public void Chunk_ShouldSplitTextIntoMultipleChunks()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 10, chunkOverlap: 0);
        var text = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // 36 characters

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().HaveCount(4); // 10, 10, 10, 6
        chunks[0].Text.Should().Be("0123456789");
        chunks[1].Text.Should().Be("ABCDEFGHIJ");
        chunks[2].Text.Should().Be("KLMNOPQRST");
        chunks[3].Text.Should().Be("UVWXYZ");
    }

    [Fact]
    public void Chunk_ShouldHandleOverlap()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 10, chunkOverlap: 3);
        var text = "0123456789ABCDEFGHIJ"; // 20 characters

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().HaveCount(3);
        
        // First chunk: chars 0-9 (indices 0-10)
        chunks[0].Text.Should().Be("0123456789");
        chunks[0].StartIndex.Should().Be(0);
        chunks[0].EndIndex.Should().Be(10);
        chunks[0].TokenCount.Should().Be(10);

        // Second chunk starts at 7 (10 - 3 overlap) and contains 10 tokens: chars 7-16
        // Which means "789ABCDEFG"
        chunks[1].Text.Should().Be("789ABCDEFG");
        chunks[1].StartIndex.Should().Be(7);
        chunks[1].TokenCount.Should().Be(10);

        // Third chunk starts at position (17 - 3) = 14, contains remaining chars
        chunks[2].Text.Should().Be("EFGHIJ");
        chunks[2].StartIndex.Should().Be(14);
    }

    [Fact]
    public void Chunk_ShouldHandleWordTokenizer()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 3, chunkOverlap: 1);
        var text = "the quick brown fox jumps over lazy dog";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().HaveCountGreaterThan(1);
        chunks[0].TokenCount.Should().BeLessThanOrEqualTo(3);

        // Verify all text is covered
        var firstChunkLength = chunks[0].Text.Length;
        firstChunkLength.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Chunk_ShouldPreserveIndices()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 5, chunkOverlap: 0);
        var text = "ABCDEFGHIJKLMNO";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            chunk.EndIndex.Should().Be(chunk.StartIndex + chunk.Text.Length);

            // Verify chunk text matches original text at those indices
            text.Substring(chunk.StartIndex, chunk.Text.Length).Should().Be(chunk.Text);
        }
    }

    [Fact]
    public void ChunkBatch_ShouldProcessMultipleTexts()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 10);
        var texts = new[] { "Hello", "Test", "0123456789ABCDEF" };

        // Act
        var results = chunker.ChunkBatch(texts);

        // Assert
        results.Should().HaveCount(3);
        results[0].Should().ContainSingle(); // "Hello" = 5 chars, fits in one chunk
        results[1].Should().ContainSingle(); // "Test" = 4 chars, fits in one chunk
        results[2].Should().HaveCount(2); // 16 chars = 2 chunks of 10 and 6
    }

    [Fact]
    public void ChunkBatch_ShouldHandleEmptyList()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 10);
        var texts = Array.Empty<string>();

        // Act
        var results = chunker.ChunkBatch(texts);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void ChunkDocument_ShouldPopulateDocumentChunks()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 10);
        var document = new Chonkie.Core.Types.Document
        {
            Content = "0123456789ABCDEFGHIJ"
        };

        // Act
        var result = chunker.ChunkDocument(document);

        // Assert
        result.Chunks.Should().HaveCount(2);
        result.Chunks[0].Text.Should().Be("0123456789");
        result.Chunks[1].Text.Should().Be("ABCDEFGHIJ");
    }

    [Fact]
    public void ToString_ShouldReturnDescriptiveString()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 512, chunkOverlap: 50);

        // Act
        var result = chunker.ToString();

        // Assert
        result.Should().Contain("512");
        result.Should().Contain("50");
    }
}
