using Xunit;
using Chonkie.Chunkers;
using Chonkie.Tokenizers;
using Chonkie.Core.Types;
using Chonkie.Embeddings.Base;

namespace Chonkie.Core.Tests.Chunkers;

public class LateChunkerTests
{
    private class TestEmbeddings : BaseEmbeddings
    {
        public override string Name => "test-model";
        public override int Dimension => 384;

        public override async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
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
    public void LateChunker_ShouldInitializeWithDefaultParameters()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();

        // Act
        var chunker = new LateChunker(tokenizer, embeddings);

        // Assert
        Assert.NotNull(chunker);
        Assert.Equal(2048, chunker.ChunkSize);
        Assert.Equal(24, chunker.MinCharactersPerChunk);
    }

    [Fact]
    public void LateChunker_ShouldInitializeWithCustomParameters()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var rules = new RecursiveRules();

        // Act
        var chunker = new LateChunker(
            tokenizer,
            embeddings,
            chunkSize: 1024,
            rules: rules,
            minCharactersPerChunk: 50);

        // Assert
        Assert.NotNull(chunker);
        Assert.Equal(1024, chunker.ChunkSize);
        Assert.Equal(50, chunker.MinCharactersPerChunk);
    }

    [Fact]
    public void LateChunker_ShouldChunkEmptyText()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new LateChunker(tokenizer, embeddings);

        // Act
        var result = chunker.Chunk("");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void LateChunker_ShouldChunkTextAndAddEmbeddings()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new LateChunker(tokenizer, embeddings);
        var text = "This is a test. This is only a test.";

        // Act
        var result = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, chunk => Assert.NotNull(chunk.Embedding));
        Assert.All(result, chunk => Assert.NotEmpty(chunk.Embedding!));
    }

    [Fact]
    public void LateChunker_ShouldRespectChunkSize()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new LateChunker(tokenizer, embeddings, chunkSize: 10);
        var text = string.Join(" ", Enumerable.Repeat("word", 100));

        // Act
        var result = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, chunk => Assert.True(chunk.TokenCount <= 10));
    }

    [Fact]
    public void LateChunker_ToString_ReturnsFormattedString()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new LateChunker(tokenizer, embeddings, chunkSize: 1024);

        // Act
        var result = chunker.ToString();

        // Assert
        Assert.Contains("LateChunker", result);
        Assert.Contains("1024", result);
        Assert.Contains("test-model", result);
    }

    [Fact]
    public void LateChunker_ShouldVerifyEmbeddingDimensions()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new LateChunker(tokenizer, embeddings);
        var text = "This is a test sentence for embedding verification.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        Assert.All(chunks, chunk =>
        {
            Assert.NotNull(chunk.Embedding);
            Assert.Equal(384, chunk.Embedding!.Length);
        });
    }

    [Fact]
    public void LateChunker_ShouldVerifyChunkIndices()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new LateChunker(tokenizer, embeddings, chunkSize: 50);
        var text = "The quick brown fox jumps over the lazy dog. " +
                   "The dog was sleeping peacefully in the sun.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        foreach (var chunk in chunks)
        {
            var extractedText = text.Substring(chunk.StartIndex, chunk.EndIndex - chunk.StartIndex);
            Assert.Equal(chunk.Text, extractedText);
        }
    }

    [Fact]
    public void LateChunker_ShouldReconstructOriginalText()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new LateChunker(tokenizer, embeddings);
        var text = "First paragraph with multiple sentences. " +
                   "Second paragraph continues here. " +
                   "Third paragraph concludes the text.";

        // Act
        var chunks = chunker.Chunk(text);
        var reconstructed = string.Join("", chunks.Select(c => c.Text));

        // Assert
        Assert.Equal(text, reconstructed);
    }

    [Fact]
    public void LateChunker_ShouldHandleWhitespaceOnlyText()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new LateChunker(tokenizer, embeddings);

        // Act
        var chunks = chunker.Chunk("   \n  \t  ");

        // Assert
        Assert.Empty(chunks);
    }

    [Fact]
    public void LateChunker_ShouldWorkWithCustomRules()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var customRules = new RecursiveRules(new[]
        {
            new RecursiveLevel
            {
                Delimiters = new[] { ".", "!", "?", "\n" },
                IncludeDelimiter = "prev"
            }
        });
        var chunker = new LateChunker(
            tokenizer,
            embeddings,
            chunkSize: 50,
            rules: customRules);
        var text = "First sentence. Second sentence! Third sentence?";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        Assert.All(chunks, chunk =>
        {
            Assert.NotNull(chunk.Embedding);
            Assert.True(chunk.TokenCount <= 50);
        });
    }

    [Fact]
    public void LateChunker_ShouldRespectMinCharactersPerChunk()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var minChars = 20;
        var chunker = new LateChunker(
            tokenizer,
            embeddings,
            minCharactersPerChunk: minChars);
        var text = "This is a test. With multiple sentences. Each one different.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        Assert.All(chunks, chunk =>
        {
            Assert.True(chunk.Text.Length >= minChars || chunk == chunks[chunks.Count - 1]);
        });
    }

    [Fact]
    public void LateChunker_ShouldGenerateEmbeddingsForAllChunks()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new LateChunker(tokenizer, embeddings, chunkSize: 30);
        var text = string.Join(" ", Enumerable.Repeat("word", 100));

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        Assert.True(chunks.Count > 1, "Should generate multiple chunks");
        Assert.All(chunks, chunk =>
        {
            Assert.NotNull(chunk.Embedding);
            Assert.Equal(384, chunk.Embedding!.Length);
            Assert.True(chunk.Embedding.Any(e => Math.Abs(e) > 1e-6f), "Embedding should not be all zeros");
        });
    }

    [Fact]
    public void LateChunker_ShouldInheritRecursiveChunkerBehavior()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var lateChunker = new LateChunker(tokenizer, embeddings, chunkSize: 50);
        var recursiveChunker = new RecursiveChunker(tokenizer, chunkSize: 50);
        var text = "This is a test paragraph. It contains multiple sentences. " +
                   "Each sentence should be properly chunked.";

        // Act
        var lateChunks = lateChunker.Chunk(text);
        var recursiveChunks = recursiveChunker.Chunk(text);

        // Assert - same number of chunks (LateChunker just adds embeddings)
        Assert.Equal(recursiveChunks.Count, lateChunks.Count);

        // Assert - text content matches
        for (int i = 0; i < lateChunks.Count; i++)
        {
            Assert.Equal(recursiveChunks[i].Text, lateChunks[i].Text);
            Assert.Equal(recursiveChunks[i].TokenCount, lateChunks[i].TokenCount);
        }

        // Assert - LateChunker has embeddings, RecursiveChunker doesn't
        Assert.All(lateChunks, chunk => Assert.NotNull(chunk.Embedding));
        Assert.All(recursiveChunks, chunk => Assert.Null(chunk.Embedding));
    }

    [Theory]
    [InlineData(256)]
    [InlineData(512)]
    [InlineData(1024)]
    public void LateChunker_ShouldWorkWithDifferentChunkSizes(int chunkSize)
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new LateChunker(tokenizer, embeddings, chunkSize: chunkSize);
        var text = string.Join(" ", Enumerable.Repeat("word", 500));

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        Assert.All(chunks, chunk =>
        {
            Assert.True(chunk.TokenCount <= chunkSize);
            Assert.NotNull(chunk.Embedding);
        });
    }

    [Fact]
    public void LateChunker_ShouldHandleLongText()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new LateChunker(tokenizer, embeddings, chunkSize: 100);
        var text = string.Join(" ", Enumerable.Repeat(
            "This is a long text document with many words and sentences.", 50));

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.NotEmpty(chunks);
        Assert.True(chunks.Count > 5, "Long text should produce multiple chunks");
        Assert.All(chunks, chunk =>
        {
            Assert.NotNull(chunk.Embedding);
            Assert.True(chunk.TokenCount > 0);
            Assert.True(chunk.TokenCount <= 100);
        });
    }

    [Fact]
    public void LateChunker_ShouldHandleSingleWord()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new LateChunker(tokenizer, embeddings);

        // Act
        var chunks = chunker.Chunk("Word");

        // Assert
        Assert.Single(chunks);
        Assert.Equal("Word", chunks[0].Text);
        Assert.NotNull(chunks[0].Embedding);
    }

    [Fact]
    public void LateChunker_EmbeddingsShouldVaryByContent()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var embeddings = new TestEmbeddings();
        var chunker = new LateChunker(tokenizer, embeddings, chunkSize: 5);
        var text = "First distinct text here. Second completely different text there. Third unique content now.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        Assert.True(chunks.Count >= 2, "Should generate at least 2 chunks");
        // Verify all chunks have embeddings
        Assert.All(chunks, chunk => Assert.NotNull(chunk.Embedding));

        // If we have multiple chunks, verify embeddings are different
        if (chunks.Count >= 2)
        {
            // Embeddings should be different for different content
            Assert.False(chunks[0].Embedding!.SequenceEqual(chunks[1].Embedding!));
        }
    }
}

