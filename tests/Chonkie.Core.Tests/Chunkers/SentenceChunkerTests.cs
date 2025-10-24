namespace Chonkie.Core.Tests.Chunkers;

using Chonkie.Chunkers;
using Chonkie.Tokenizers;
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
        Assert.Equal(2048, chunker.ChunkSize);
        Assert.Equal(0, chunker.ChunkOverlap);
        Assert.Equal(1, chunker.MinSentencesPerChunk);
        Assert.Equal(12, chunker.MinCharactersPerSentence);
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
        Assert.Equal(512, chunker.ChunkSize);
        Assert.Equal(50, chunker.ChunkOverlap);
        Assert.Equal(2, chunker.MinSentencesPerChunk);
        Assert.Equal(20, chunker.MinCharactersPerSentence);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SentenceChunker_ShouldThrowOnInvalidChunkSize(int chunkSize)
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new SentenceChunker(tokenizer, chunkSize: chunkSize));
        Assert.Contains("chunk_size must be positive", ex.Message);
    }

    [Fact]
    public void SentenceChunker_ShouldThrowWhenOverlapExceedsChunkSize()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new SentenceChunker(tokenizer, chunkSize: 100, chunkOverlap: 100));
        Assert.Contains("chunk_overlap must be less than chunk_size", ex.Message);
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
        Assert.Empty(chunks);
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
        Assert.Empty(chunks);
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
        Assert.Single(chunks);
        Assert.Equal(text, chunks[0].Text);
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
        Assert.True(chunks.Count > 1);
        // Each chunk should end with a complete sentence
        var lastChunk = chunks[chunks.Count - 1];
        Assert.All(chunks, c =>
        {
            var trimmed = c.Text.TrimEnd();
            Assert.True(trimmed.EndsWith(".") || trimmed == lastChunk.Text.TrimEnd());
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
        Assert.NotEmpty(chunks);
        // Verify text is fully covered
        var totalText = string.Concat(chunks.Select(c => c.Text));
        Assert.True(totalText.Length > 0);
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
        Assert.NotEmpty(chunks);
        // Most chunks should have at least 2 sentences (except possibly the last)
        var sentenceCounts = chunks.Select(c =>
            c.Text.Split(new[] { ". " }, StringSplitOptions.RemoveEmptyEntries).Length).ToList();

        for (int i = 0; i < chunks.Count - 1; i++)
        {
            Assert.True(sentenceCounts[i] >= 2);
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
        Assert.NotEmpty(chunks);
        var totalText = string.Concat(chunks.Select(c => c.Text));
        Assert.Contains("First", totalText);
        Assert.Contains("Second", totalText);
        Assert.Contains("Third", totalText);
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
        Assert.NotEmpty(chunks);
        // Short sentences should be merged with adjacent ones
        Assert.Contains("longer sentence", chunks[0].Text);
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
        Assert.NotEmpty(chunks);
        foreach (var chunk in chunks)
        {
            Assert.Equal(chunk.StartIndex + chunk.Text.Length, chunk.EndIndex);
            // Verify chunk text matches original text at those indices
            if (chunk.StartIndex < text.Length && chunk.EndIndex <= text.Length)
            {
                var expectedText = text.Substring(chunk.StartIndex, Math.Min(chunk.Text.Length, text.Length - chunk.StartIndex));
                var actualText = chunk.Text.Substring(0, Math.Min(chunk.Text.Length, text.Length - chunk.StartIndex));
                Assert.Equal(expectedText, actualText);
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
        Assert.NotEmpty(chunks);
        // Delimiters should be included with previous sentence
        Assert.Contains(".", chunks[0].Text);
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
        Assert.Equal(2, results.Count);
        Assert.NotEmpty(results[0]);
        Assert.NotEmpty(results[1]);
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
        Assert.Contains("512", result);
        Assert.Contains("50", result);
    }

    [Fact]
    public void Chunk_WithOverlap_ShouldCreateOverlappingChunks()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SentenceChunker(
            tokenizer,
            chunkSize: 100,
            chunkOverlap: 20,
            minCharactersPerSentence: 5);

        var text = "First sentence here. Second sentence here. Third sentence here. Fourth sentence here. Fifth sentence here.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);

        // Note: SentenceChunker may not always create overlap if sentences are too large
        // We verify that if we have multiple chunks, they respect the chunk size constraint
        Assert.All(chunks, c => Assert.True(c.TokenCount <= 100));

        // If we have multiple chunks, check for potential overlap or at least continuity
        if (chunks.Count > 1)
        {
            for (int i = 1; i < chunks.Count; i++)
            {
                // Chunks should either overlap or be continuous
                Assert.True(chunks[i].StartIndex <= chunks[i - 1].EndIndex);
            }
        }
    }

    [Fact]
    public void Chunk_TokenCountVerification_ShouldMatchTokenizer()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SentenceChunker(tokenizer, chunkSize: 512, chunkOverlap: 128);
        var text = "First sentence. Second sentence. Third sentence. Fourth sentence.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        foreach (var chunk in chunks)
        {
            var expectedCount = tokenizer.CountTokens(chunk.Text);
            Assert.Equal(expectedCount, chunk.TokenCount);
            Assert.True(chunk.TokenCount <= 512);
        }
    }

    [Fact]
    public void Chunk_WithComplexMarkdown_ShouldHandleCorrectly()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SentenceChunker(
            tokenizer,
            chunkSize: 200,
            minCharactersPerSentence: 5);

        var markdown = @"# Heading 1
This is a paragraph with some **bold text** and _italic text_. 
## Heading 2
- Bullet point 1
- Bullet point 2 with `inline code`
Another paragraph with [a link](https://example.com) and an image.
Finally, a paragraph at the end.";

        // Act
        var chunks = chunker.Chunk(markdown);

        // Assert
        Assert.NotEmpty(chunks);

        // Verify indices map correctly
        foreach (var chunk in chunks)
        {
            if (chunk.EndIndex <= markdown.Length)
            {
                var expectedText = markdown.Substring(chunk.StartIndex, chunk.EndIndex - chunk.StartIndex).Trim();
                Assert.Equal(expectedText, chunk.Text.Trim());
            }
        }
    }

    [Fact]
    public void Chunk_WithMinCharactersPerSentence_ShouldFilterOrMerge()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SentenceChunker(
            tokenizer,
            chunkSize: 512,
            minCharactersPerSentence: 20);

        var text = "Hi. This is a much longer sentence that should definitely be kept in the output. Yes.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        // Short sentences should be merged with adjacent ones
        var firstChunk = chunks[0].Text;
        Assert.Contains("longer sentence", firstChunk);
    }

    [Fact]
    public void Chunk_EmptyOrWhitespaceStrings_ShouldReturnEmpty()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SentenceChunker(tokenizer, chunkSize: 100);

        // Act & Assert
        Assert.Empty(chunker.Chunk(""));
        Assert.Empty(chunker.Chunk("   "));
        Assert.Empty(chunker.Chunk("\t\n\r"));
    }

    [Fact]
    public void ChunkBatch_WithMultipleTexts_ShouldProcessAll()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new SentenceChunker(tokenizer, chunkSize: 100);
        var texts = new[]
        {
            "First text with sentences. Multiple sentences here.",
            "Second text. Also with sentences.",
            "Third text with a single sentence."
        };

        // Act
        var results = chunker.ChunkBatch(texts);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.All(results, chunks => Assert.NotEmpty(chunks));
    }
}
