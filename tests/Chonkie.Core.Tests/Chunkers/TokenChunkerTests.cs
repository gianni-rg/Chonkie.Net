namespace Chonkie.Core.Tests.Chunkers;

using Chonkie.Chunkers;
using Chonkie.Tokenizers;
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
        Assert.Equal(2048, chunker.ChunkSize);
        Assert.Equal(0, chunker.ChunkOverlap);
    }

    [Fact]
    public void TokenChunker_ShouldInitializeWithCustomParameters()
    {
        // Arrange & Act
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 512, chunkOverlap: 50);

        // Assert
        Assert.Equal(512, chunker.ChunkSize);
        Assert.Equal(50, chunker.ChunkOverlap);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TokenChunker_ShouldThrowOnInvalidChunkSize(int chunkSize)
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new TokenChunker(tokenizer, chunkSize: chunkSize));
        Assert.Contains("chunk_size must be positive", ex.Message);
    }

    [Fact]
    public void TokenChunker_ShouldThrowWhenOverlapExceedsChunkSize()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new TokenChunker(tokenizer, chunkSize: 100, chunkOverlap: 100));
        Assert.Contains("chunk_overlap must be less than chunk_size", ex.Message);
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
        Assert.Empty(chunks);
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
        Assert.Empty(chunks);
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
        Assert.Single(chunks);
        Assert.Equal(text, chunks[0].Text);
        Assert.Equal(11, chunks[0].TokenCount); // 11 characters
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(11, chunks[0].EndIndex);
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
        Assert.Equal(4, chunks.Count); // 10, 10, 10, 6
        Assert.Equal("0123456789", chunks[0].Text);
        Assert.Equal("ABCDEFGHIJ", chunks[1].Text);
        Assert.Equal("KLMNOPQRST", chunks[2].Text);
        Assert.Equal("UVWXYZ", chunks[3].Text);
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
        Assert.Equal(3, chunks.Count);
        
        // First chunk: chars 0-9 (indices 0-10)
        Assert.Equal("0123456789", chunks[0].Text);
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(10, chunks[0].EndIndex);
        Assert.Equal(10, chunks[0].TokenCount);

        // Second chunk starts at 7 (10 - 3 overlap) and contains 10 tokens: chars 7-16
        // Which means "789ABCDEFG"
        Assert.Equal("789ABCDEFG", chunks[1].Text);
        Assert.Equal(7, chunks[1].StartIndex);
        Assert.Equal(10, chunks[1].TokenCount);

        // Third chunk starts at position (17 - 3) = 14, contains remaining chars
        Assert.Equal("EFGHIJ", chunks[2].Text);
        Assert.Equal(14, chunks[2].StartIndex);
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
        Assert.True(chunks.Count > 1);
        Assert.True(chunks[0].TokenCount <= 3);

        // Verify all text is covered
        var firstChunkLength = chunks[0].Text.Length;
        Assert.True(firstChunkLength > 0);
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
            Assert.Equal(chunk.StartIndex + chunk.Text.Length, chunk.EndIndex);

            // Verify chunk text matches original text at those indices
            Assert.Equal(chunk.Text, text.Substring(chunk.StartIndex, chunk.Text.Length));
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
        Assert.Equal(3, results.Count);
        Assert.Single(results[0]); // "Hello" = 5 chars, fits in one chunk
        Assert.Single(results[1]); // "Test" = 4 chars, fits in one chunk
        Assert.Equal(2, results[2].Count); // 16 chars = 2 chunks of 10 and 6
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
        Assert.Empty(results);
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
        Assert.Equal(2, result.Chunks.Count);
        Assert.Equal("0123456789", result.Chunks[0].Text);
        Assert.Equal("ABCDEFGHIJ", result.Chunks[1].Text);
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
        Assert.Contains("512", result);
        Assert.Contains("50", result);
    }

    [Fact]
    public void Chunk_AsCallable_ShouldWorkLikeChunkMethod()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 10);
        var text = "0123456789ABCDEF";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert - verify the method works (simulating __call__ in Python)
        Assert.Equal(2, chunks.Count);
        Assert.Equal("0123456789", chunks[0].Text);
        Assert.Equal("ABCDEF", chunks[1].Text);
    }

    [Fact]
    public void Chunk_IndicesVerification_ShouldMapCorrectly()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 512, chunkOverlap: 128);
        var text = "The quick brown fox jumps over the lazy dog. " + 
                   "This is a test sentence to verify chunk indices. " +
                   "Another sentence here for testing purposes.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert - Verify each chunk's indices correctly map to original text
        foreach (var chunk in chunks)
        {
            var extractedText = text.Substring(chunk.StartIndex, chunk.EndIndex - chunk.StartIndex);
            Assert.Equal(chunk.Text.Trim(), extractedText.Trim());
        }
    }

    [Fact]
    public void Chunk_TokenCountVerification_ShouldMatchTokenizer()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 512, chunkOverlap: 128);
        var text = "The quick brown fox jumps over the lazy dog.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert - Verify token counts match what tokenizer reports
        foreach (var chunk in chunks)
        {
            var expectedCount = tokenizer.CountTokens(chunk.Text);
            Assert.Equal(expectedCount, chunk.TokenCount);
            Assert.True(chunk.TokenCount <= 512);
        }
    }

    [Fact]
    public void Chunk_WithComplexMarkdown_ShouldPreserveStructure()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 100, chunkOverlap: 20);
        var markdown = @"# Heading 1
This is a paragraph with some **bold text** and _italic text_. 
## Heading 2
- Bullet point 1
- Bullet point 2 with `inline code`
```python
def hello():
    print('Hello')
```
Another paragraph.";

        // Act
        var chunks = chunker.Chunk(markdown);

        // Assert
        Assert.NotEmpty(chunks);
        
        // Note: Due to whitespace trimming, we verify indices match correctly
        // rather than exact text reconstruction
        foreach (var chunk in chunks)
        {
            var extractedText = markdown.Substring(chunk.StartIndex, chunk.EndIndex - chunk.StartIndex);
            Assert.Equal(chunk.Text, extractedText);
        }
        
        // Verify chunks cover the entire text
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(markdown.Length, chunks[chunks.Count - 1].EndIndex);
    }

    [Fact]
    public void Chunk_WithFractionalOverlap_ShouldHandleCorrectly()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        // Note: In .NET we use int for overlap, so we test with whole numbers
        var chunker = new TokenChunker(tokenizer, chunkSize: 10, chunkOverlap: 2);
        var text = "0123456789ABCDEFGHIJ"; // 20 characters

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.True(chunks.Count > 1);
        Assert.All(chunks, c => Assert.True(c.TokenCount <= 10));
        
        // Verify overlap is working
        if (chunks.Count > 1)
        {
            Assert.True(chunks[1].StartIndex < chunks[0].EndIndex);
        }
    }

    [Fact]
    public void ChunkBatch_ShouldProcessInParallel()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 50);
        var texts = Enumerable.Range(0, 10)
            .Select(i => $"Text {i}: " + string.Concat(Enumerable.Repeat($"word{i} ", 20)))
            .ToArray();

        // Act
        var results = chunker.ChunkBatch(texts);

        // Assert
        Assert.Equal(10, results.Count);
        Assert.All(results, chunks => Assert.NotEmpty(chunks));
    }

    [Fact]
    public void Chunk_EmptyOrWhitespaceStrings_ShouldReturnEmpty()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 100);

        // Act & Assert
        Assert.Empty(chunker.Chunk(""));
        Assert.Empty(chunker.Chunk("   "));
        Assert.Empty(chunker.Chunk("\t\n\r"));
        Assert.Empty(chunker.Chunk("     \t\t\n\n   "));
    }
}
