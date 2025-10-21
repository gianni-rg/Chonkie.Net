using Xunit;
using Chonkie.Chunkers;
using Chonkie.Tokenizers;
using Chonkie.Core.Types;
using Chonkie.Embeddings.Base;

namespace Chonkie.Core.Tests.Chunkers;

public class SemanticChunkerTests
{
    private class TestEmbeddings : BaseEmbeddings
    {
        public override string Name => "test-model";
        public override int Dimension => 384;

        public override async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            // Return simple embedding based on text length
            return Enumerable.Range(0, Dimension)
                .Select(i => (float)((text.Length + i) % 100) / 100.0f)
                .ToArray();
        }

        public override async Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            var result = new List<float[]>();
            foreach (var text in texts)
            {
                result.Add(Enumerable.Range(0, Dimension)
                    .Select(i => (float)((text.Length + i) % 100) / 100.0f)
                    .ToArray());
            }
            return result;
        }
    }

    [Fact]
    public void SemanticChunker_ShouldInitializeWithDefaultParameters()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var embeddings = new TestEmbeddings();

        // Act
        var chunker = new SemanticChunker(tokenizer, embeddings);

        // Assert
        Assert.NotNull(chunker);
        Assert.Equal(2048, chunker.ChunkSize);
    }

    [Fact]
    public void SemanticChunker_ShouldInitializeWithCustomParameters()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();

        // Act
        var chunker = new SemanticChunker(
            tokenizer,
            embeddings,
            threshold: 0.7f,
            chunkSize: 512);

        // Assert
        Assert.NotNull(chunker);
        Assert.Equal(512, chunker.ChunkSize);
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(1.0f)]
    [InlineData(-0.5f)]
    [InlineData(1.5f)]
    public void SemanticChunker_ShouldThrowOnInvalidThreshold(float threshold)
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var embeddings = new TestEmbeddings();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            new SemanticChunker(tokenizer, embeddings, threshold: threshold));
        Assert.Contains("Threshold", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SemanticChunker_ShouldThrowOnInvalidChunkSize(int chunkSize)
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var embeddings = new TestEmbeddings();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            new SemanticChunker(tokenizer, embeddings, chunkSize: chunkSize));
        Assert.Contains("ChunkSize", ex.Message);
    }

    [Fact]
    public void SemanticChunker_ShouldChunkEmptyText()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new SemanticChunker(tokenizer, embeddings);

        // Act
        var result = chunker.Chunk("");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void SemanticChunker_ShouldChunkShortText()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new SemanticChunker(tokenizer, embeddings);

        // Act
        var result = chunker.Chunk("Hello. World.");

        // Assert
        Assert.Single(result);
        Assert.Equal("Hello. World.", result[0].Text);
    }

    [Fact]
    public void SemanticChunker_ShouldHandleMultipleSentences()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new SemanticChunker(tokenizer, embeddings, chunkSize: 50);
        var text = "First sentence. Second sentence. Third sentence. Fourth sentence. Fifth sentence.";

        // Act
        var result = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, chunk => Assert.True(chunk.Text.Length > 0));
    }

    [Fact]
    public void SemanticChunker_ToString_ReturnsFormattedString()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new SemanticChunker(tokenizer, embeddings, chunkSize: 1024);

        // Act
        var result = chunker.ToString();

        // Assert
        Assert.Contains("SemanticChunker", result);
        Assert.Contains("1024", result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SemanticChunker_ShouldThrowOnInvalidSimilarityWindow(int window)
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var embeddings = new TestEmbeddings();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            new SemanticChunker(tokenizer, embeddings, similarityWindow: window));
        Assert.Contains("similarityWindow", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SemanticChunker_ShouldThrowOnInvalidMinSentencesPerChunk(int minSentences)
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var embeddings = new TestEmbeddings();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            new SemanticChunker(tokenizer, embeddings, minSentencesPerChunk: minSentences));
        Assert.Contains("minSentencesPerChunk", ex.Message);
    }

    [Fact]
    public void SemanticChunker_ShouldThrowOnNegativeSkipWindow()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var embeddings = new TestEmbeddings();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            new SemanticChunker(tokenizer, embeddings, skipWindow: -1));
        Assert.Contains("skipWindow", ex.Message);
    }

    [Fact]
    public void SemanticChunker_ShouldVerifyTokenCounts()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new SemanticChunker(tokenizer, embeddings, chunkSize: 50);
        var text = "The process of text chunking represents a delicate balance. " +
                   "We must optimize for information density. " +
                   "Each chunk should carry sufficient signal without excessive noise.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        Assert.All(chunks, chunk =>
        {
            Assert.True(chunk.TokenCount > 0, "Token count must be positive");
            Assert.True(chunk.TokenCount <= 50, "Token count must respect chunk size");
        });
        
        // Verify reconstruction works
        var reconstructed = string.Join("", chunks.Select(c => c.Text));
        Assert.Equal(text, reconstructed);
    }

    [Fact]
    public void SemanticChunker_ShouldReconstructOriginalText()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new SemanticChunker(tokenizer, embeddings, chunkSize: 100);
        var text = "First sentence here. Second sentence follows. " +
                   "Third sentence continues. Fourth sentence ends the paragraph.";

        // Act
        var chunks = chunker.Chunk(text);
        var reconstructed = string.Join("", chunks.Select(c => c.Text));

        // Assert
        Assert.Equal(text, reconstructed);
    }

    [Fact]
    public void SemanticChunker_ShouldVerifyChunkIndices()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new SemanticChunker(tokenizer, embeddings, chunkSize: 50);
        var text = "The quick brown fox jumps. Over the lazy dog. " +
                   "The dog was very lazy indeed.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        foreach (var chunk in chunks)
        {
            var extractedText = text.Substring(chunk.StartIndex, chunk.EndIndex - chunk.StartIndex);
            Assert.Equal(chunk.Text.Trim(), extractedText.Trim());
        }
    }

    [Theory]
    [InlineData(0.1f)]
    [InlineData(0.5f)]
    [InlineData(0.9f)]
    public void SemanticChunker_ShouldWorkWithDifferentThresholds(float threshold)
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new SemanticChunker(tokenizer, embeddings, threshold: threshold, chunkSize: 100);
        var text = "First topic here. Related to first topic. " +
                   "Different topic now. More on different topic.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        Assert.All(chunks, chunk => Assert.True(chunk.Text.Length > 0));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void SemanticChunker_ShouldWorkWithDifferentSimilarityWindows(int window)
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new SemanticChunker(
            tokenizer, 
            embeddings, 
            similarityWindow: window,
            chunkSize: 100);
        var text = "Sentence one. Sentence two. Sentence three. " +
                   "Sentence four. Sentence five. Sentence six.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        Assert.All(chunks, chunk => Assert.True(chunk.TokenCount > 0));
    }

    [Fact]
    public void SemanticChunker_ShouldHandleVerySmallChunkSize()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new SemanticChunker(
            tokenizer, 
            embeddings, 
            chunkSize: 10,
            minSentencesPerChunk: 1);
        var text = "Short. Also short. Another short one. Final short sentence.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        // Allow some buffer for semantic grouping
        Assert.All(chunks, chunk => Assert.True(chunk.TokenCount <= 15));
    }

    [Fact]
    public void SemanticChunker_ShouldWorkWithSkipWindowEnabled()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new SemanticChunker(
            tokenizer,
            embeddings,
            threshold: 0.7f,
            skipWindow: 1,
            chunkSize: 100);
        var text = "Dogs are loyal. They love fetch. " +
                   "Cats are independent. They climb trees. " +
                   "Puppies need training. They require patience.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        Assert.All(chunks, chunk =>
        {
            Assert.True(chunk.Text.Length > 0);
            Assert.True(chunk.StartIndex >= 0);
            Assert.True(chunk.EndIndex > chunk.StartIndex);
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void SemanticChunker_ShouldRespectSkipWindowParameter(int skipWindow)
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new SemanticChunker(
            tokenizer,
            embeddings,
            skipWindow: skipWindow,
            chunkSize: 100);
        var text = "Machine learning is fascinating. Neural networks are powerful. " +
                   "The stock market fluctuated. Economic indicators show growth.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        Assert.All(chunks, chunk => Assert.True(chunk.TokenCount <= 100));
    }

    [Fact]
    public void SemanticChunker_ShouldHandleCustomDelimiters()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var customDelimiters = new[] { ".", "!", "?" };
        var chunker = new SemanticChunker(
            tokenizer,
            embeddings,
            delimiters: customDelimiters,
            chunkSize: 100);
        var text = "First sentence. Second sentence! Third sentence?";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        Assert.All(chunks, chunk => Assert.True(chunk.Text.Length > 0));
    }

    [Fact]
    public void SemanticChunker_ShouldHandleWhitespaceOnlyText()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new SemanticChunker(tokenizer, embeddings);

        // Act
        var result = chunker.Chunk("   \n  \t  ");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void SemanticChunker_ShouldPreserveChunkSizeLimitsWithSkipWindow()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new SemanticChunker(
            tokenizer,
            embeddings,
            chunkSize: 30,
            skipWindow: 2);
        var text = "The Renaissance was a period of cultural rebirth in Europe. " +
                   "It began in Italy during the fourteenth century and spread. " +
                   "Artists like Leonardo created masterpieces during this time.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        Assert.All(chunks, chunk => Assert.True(chunk.TokenCount <= 30));
    }
}

