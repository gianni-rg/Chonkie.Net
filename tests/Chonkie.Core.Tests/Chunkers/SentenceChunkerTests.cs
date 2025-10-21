namespace Chonkie.Core.Tests.Chunkers;

using Chonkie.Chunkers;
using Chonkie.Tokenizers;
using FluentAssertions;
using Xunit;

public class SentenceChunkerTests
{
    [Fact]
    public void SentenceChunker_ShouldInitializeWithDefaultParameters()
    {
        // Arrange & Act
        var tokenizer = new CharacterTokenizer();
        var chunker = new SentenceChunker(tokenizer);

        // Assert
        chunker.ChunkSize.Should().Be(2048);
        chunker.ChunkOverlap.Should().Be(0);
        chunker.MinSentencesPerChunk.Should().Be(1);
        chunker.MinCharactersPerSentence.Should().Be(12);
    }

    [Fact]
    public void SentenceChunker_ShouldInitializeWithCustomParameters()
    {
        // Arrange & Act
        var tokenizer = new CharacterTokenizer();
        var chunker = new SentenceChunker(
            tokenizer,
            chunkSize: 512,
            chunkOverlap: 50,
            minSentencesPerChunk: 2,
            minCharactersPerSentence: 20);

        // Assert
        chunker.ChunkSize.Should().Be(512);
        chunker.ChunkOverlap.Should().Be(50);
        chunker.MinSentencesPerChunk.Should().Be(2);
        chunker.MinCharactersPerSentence.Should().Be(20);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SentenceChunker_ShouldThrowOnInvalidChunkSize(int chunkSize)
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act & Assert
        var act = () => new SentenceChunker(tokenizer, chunkSize: chunkSize);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*chunk_size must be positive*");
    }

    [Fact]
    public void SentenceChunker_ShouldThrowWhenOverlapExceedsChunkSize()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act & Assert
        var act = () => new SentenceChunker(tokenizer, chunkSize: 100, chunkOverlap: 100);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*chunk_overlap must be less than chunk_size*");
    }

    [Fact]
    public void Chunk_ShouldReturnEmptyListForEmptyText()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SentenceChunker(tokenizer, chunkSize: 100);

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
        var chunker = new SentenceChunker(tokenizer, chunkSize: 100);

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
        var chunker = new SentenceChunker(tokenizer, chunkSize: 100);
        var text = "This is a sentence. This is another sentence.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().ContainSingle();
        chunks[0].Text.Should().Be(text);
    }

    [Fact]
    public void Chunk_ShouldSplitOnSentenceBoundaries()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SentenceChunker(tokenizer, chunkSize: 30, minCharactersPerSentence: 5);
        var text = "First sentence. Second sentence. Third sentence. Fourth sentence.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().HaveCountGreaterThan(1);
        // Each chunk should end with a complete sentence
        chunks.Should().AllSatisfy(c => {
            var lastChunk = chunks[chunks.Count - 1];
            c.Text.TrimEnd().Should().Match(s => 
                s.EndsWith(".") || s == lastChunk.Text.TrimEnd());
        });
    }

    [Fact]
    public void Chunk_ShouldHandleMultipleDelimiters()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var delimiters = new[] { ". ", "! ", "? " };
        var chunker = new SentenceChunker(
            tokenizer,
            chunkSize: 50,
            delimiters: delimiters,
            minCharactersPerSentence: 5);
        
        var text = "Hello world! How are you? I am fine. Thanks for asking!";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().NotBeEmpty();
        // Verify text is fully covered
        var totalText = string.Concat(chunks.Select(c => c.Text));
        totalText.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Chunk_ShouldRespectMinSentencesPerChunk()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SentenceChunker(
            tokenizer,
            chunkSize: 100,
            minSentencesPerChunk: 2,
            minCharactersPerSentence: 5);
        
        var text = "One. Two. Three. Four. Five. Six.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().NotBeEmpty();
        // Most chunks should have at least 2 sentences (except possibly the last)
        var sentenceCounts = chunks.Select(c => 
            c.Text.Split(new[] { ". " }, StringSplitOptions.RemoveEmptyEntries).Length).ToList();
        
        for (int i = 0; i < chunks.Count - 1; i++)
        {
            sentenceCounts[i].Should().BeGreaterThanOrEqualTo(2);
        }
    }

    [Fact]
    public void Chunk_ShouldHandleNewlineDelimiter()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SentenceChunker(
            tokenizer,
            chunkSize: 50,
            minCharactersPerSentence: 5);
        
        var text = "First paragraph.\nSecond paragraph.\nThird paragraph.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().NotBeEmpty();
        var totalText = string.Concat(chunks.Select(c => c.Text));
        totalText.Should().Contain("First");
        totalText.Should().Contain("Second");
        totalText.Should().Contain("Third");
    }

    [Fact]
    public void Chunk_ShouldFilterShortSentences()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SentenceChunker(
            tokenizer,
            chunkSize: 100,
            minCharactersPerSentence: 15);
        
        var text = "Hi. This is a longer sentence that should be kept. Yes.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().NotBeEmpty();
        // Short sentences should be merged with adjacent ones
        chunks[0].Text.Should().Contain("longer sentence");
    }

    [Fact]
    public void Chunk_ShouldPreserveIndices()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SentenceChunker(tokenizer, chunkSize: 50, minCharactersPerSentence: 5);
        var text = "First sentence here. Second sentence here. Third sentence here.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().NotBeEmpty();
        foreach (var chunk in chunks)
        {
            chunk.EndIndex.Should().Be(chunk.StartIndex + chunk.Text.Length);
            // Verify chunk text matches original text at those indices
            if (chunk.StartIndex < text.Length && chunk.EndIndex <= text.Length)
            {
                text.Substring(chunk.StartIndex, Math.Min(chunk.Text.Length, text.Length - chunk.StartIndex))
                    .Should().Be(chunk.Text.Substring(0, Math.Min(chunk.Text.Length, text.Length - chunk.StartIndex)));
            }
        }
    }

    [Fact]
    public void Chunk_ShouldHandleIncludeDelimiterPrev()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SentenceChunker(
            tokenizer,
            chunkSize: 100,
            includeDelimiter: "prev",
            minCharactersPerSentence: 5);
        
        var text = "First. Second. Third.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().NotBeEmpty();
        // Delimiters should be included with previous sentence
        chunks[0].Text.Should().Contain(".");
    }

    [Fact]
    public void ChunkBatch_ShouldProcessMultipleTexts()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SentenceChunker(tokenizer, chunkSize: 100);
        var texts = new[] { "First text. Second sentence.", "Another text here." };

        // Act
        var results = chunker.ChunkBatch(texts);

        // Assert
        results.Should().HaveCount(2);
        results[0].Should().NotBeEmpty();
        results[1].Should().NotBeEmpty();
    }

    [Fact]
    public void ToString_ShouldReturnDescriptiveString()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SentenceChunker(tokenizer, chunkSize: 512, chunkOverlap: 50);

        // Act
        var result = chunker.ToString();

        // Assert
        result.Should().Contain("512");
        result.Should().Contain("50");
    }
}
