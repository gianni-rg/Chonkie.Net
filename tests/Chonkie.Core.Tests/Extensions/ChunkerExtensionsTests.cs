using Chonkie.Core.Extensions;
using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;
using Xunit;

namespace Chonkie.Core.Tests.Extensions;

/// <summary>
/// Tests for IChunker extension members (C# 14 feature).
/// </summary>
public class ChunkerExtensionsTests
{
    [Fact]
    public void StrategyName_ReturnsChunkerTypeName()
    {
        // Arrange
        var chunker = new TestChunker();

        // Act
        var strategyName = chunker.StrategyName;

        // Assert
        Assert.Equal("Test", strategyName);
    }

    [Fact]
    public void Empty_ReturnsEmptyChunkList()
    {
        // Act
        var emptyChunks = IChunker.Empty;

        // Assert
        Assert.NotNull(emptyChunks);
        Assert.Empty(emptyChunks);
    }

    [Fact]
    public async Task ChunkBatchAsync_ProcessesTextsAsynchronously()
    {
        // Arrange
        var chunker = new TestChunker();
        var texts = new[] { "text1", "text2", "text3" };

        // Act
        var result = await chunker.ChunkBatchAsync(texts);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, chunks => Assert.Single(chunks));
    }

    [Fact]
    public async Task ChunkBatchAsync_WithCancellation_CanBeCancelled()
    {
        // Arrange
        var chunker = new TestChunker();
        var texts = Enumerable.Repeat("text", 1000);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await chunker.ChunkBatchAsync(texts, cts.Token));
    }

    // Test chunker implementation
    private class TestChunker : IChunker
    {
        public IReadOnlyList<Chunk> Chunk(string text)
        {
            return new List<Chunk>
            {
                new Chunk
                {
                    Text = text,
                    StartIndex = 0,
                    EndIndex = text.Length,
                    TokenCount = text.Length
                }
            };
        }

        public IReadOnlyList<IReadOnlyList<Chunk>> ChunkBatch(
            IEnumerable<string> texts,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var result = new List<IReadOnlyList<Chunk>>();
            foreach (var text in texts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                result.Add(Chunk(text));
            }
            return result;
        }

        public Document ChunkDocument(Document document)
        {
            document.Chunks = Chunk(document.Content).ToList();
            return document;
        }
    }
}
