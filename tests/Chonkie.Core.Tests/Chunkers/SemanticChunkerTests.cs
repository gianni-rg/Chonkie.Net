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
}
