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
}
