namespace Chonkie.Porters.Tests.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Core.Types;
using Chonkie.Porters;
using Chonkie.Porters.Extensions;
using Xunit;

/// <summary>
/// Tests for IPorter extension members (C# 14).
/// </summary>
public class PorterExtensionsTests
{
    private class TestPorter : IPorter
    {
        public Dictionary<string, int> ExportedChunkCounts { get; } = new();

        public Task<bool> ExportAsync(
            IReadOnlyList<Chunk> chunks,
            string destination,
            CancellationToken cancellationToken = default)
        {
            ExportedChunkCounts[destination] = chunks.Count;
            return Task.FromResult(true);
        }
    }

    private class FailingPorter : IPorter
    {
        public Task<bool> ExportAsync(
            IReadOnlyList<Chunk> chunks,
            string destination,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }
    }

    [Fact]
    public void PorterType_ReturnsPorterTypeName()
    {
        // Arrange
        var porter = new JsonPorter();

        // Act
        var type = porter.PorterType;

        // Assert
        Assert.Equal("Json", type);
    }

    [Fact]
    public async Task ExportInBatchesAsync_WithSmallList_ExportsInOneGo()
    {
        // Arrange
        var porter = new TestPorter();
        var chunks = CreateChunks(5);

        // Act
        var success = await porter.ExportInBatchesAsync(chunks, "output.json", batchSize: 10);

        // Assert
        Assert.True(success);
        Assert.Single(porter.ExportedChunkCounts);
        Assert.Equal(5, porter.ExportedChunkCounts["output.json"]);
    }

    [Fact]
    public async Task ExportInBatchesAsync_WithLargeList_ExportsInMultipleBatches()
    {
        // Arrange
        var porter = new TestPorter();
        var chunks = CreateChunks(25);

        // Act
        var success = await porter.ExportInBatchesAsync(chunks, "output.json", batchSize: 10);

        // Assert
        Assert.True(success);
        Assert.Equal(3, porter.ExportedChunkCounts.Count); // 3 batches: 10, 10, 5
        Assert.Equal(10, porter.ExportedChunkCounts["output.json.part0"]);
        Assert.Equal(10, porter.ExportedChunkCounts["output.json.part1"]);
        Assert.Equal(5, porter.ExportedChunkCounts["output.json.part2"]);
    }

    [Fact]
    public async Task ExportInBatchesAsync_WithInvalidBatchSize_ThrowsException()
    {
        // Arrange
        var porter = new TestPorter();
        var chunks = CreateChunks(5);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => porter.ExportInBatchesAsync(chunks, "output.json", batchSize: 0));
    }

    [Fact]
    public async Task ExportInBatchesAsync_WithFailure_ReturnsFalse()
    {
        // Arrange
        var porter = new FailingPorter();
        var chunks = CreateChunks(25);

        // Act
        var success = await porter.ExportInBatchesAsync(chunks, "output.json", batchSize: 10);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public async Task ExportInBatchesAsync_WithCancellation_CanBeCancelled()
    {
        // Arrange
        var porter = new TestPorter();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var chunks = CreateChunks(100);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => porter.ExportInBatchesAsync(chunks, "output.json", batchSize: 10, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task ExportMultipleAsync_WithMultipleLists_ExportsAll()
    {
        // Arrange
        var porter = new TestPorter();
        var chunkLists = new[]
        {
            CreateChunks(3),
            CreateChunks(5),
            CreateChunks(2)
        };

        // Act
        var success = await porter.ExportMultipleAsync(chunkLists, "output_{0}.json");

        // Assert
        Assert.True(success);
        Assert.Equal(3, porter.ExportedChunkCounts.Count);
        Assert.Equal(3, porter.ExportedChunkCounts["output_0.json"]);
        Assert.Equal(5, porter.ExportedChunkCounts["output_1.json"]);
        Assert.Equal(2, porter.ExportedChunkCounts["output_2.json"]);
    }

    [Fact]
    public async Task ExportMultipleAsync_WithEmptyLists_ReturnsTrue()
    {
        // Arrange
        var porter = new TestPorter();
        var chunkLists = Array.Empty<IReadOnlyList<Chunk>>();

        // Act
        var success = await porter.ExportMultipleAsync(chunkLists, "output_{0}.json");

        // Assert
        Assert.True(success);
        Assert.Empty(porter.ExportedChunkCounts);
    }

    [Fact]
    public async Task ExportMultipleAsync_WithFailure_ReturnsFalse()
    {
        // Arrange
        var porter = new FailingPorter();
        var chunkLists = new[] { CreateChunks(3) };

        // Act
        var success = await porter.ExportMultipleAsync(chunkLists, "output_{0}.json");

        // Assert
        Assert.False(success);
    }

    [Fact]
    public async Task ExportMultipleAsync_WithCancellation_CanBeCancelled()
    {
        // Arrange
        var porter = new TestPorter();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var chunkLists = Enumerable.Range(0, 100).Select(_ => CreateChunks(5));

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => porter.ExportMultipleAsync(chunkLists, "output_{0}.json", cts.Token));
    }

    [Fact]
    public void CommonFormats_ReturnsExpectedFormats()
    {
        // Act
        var formats = IPorter.CommonFormats;

        // Assert
        Assert.NotEmpty(formats);
        Assert.Contains("json", formats);
        Assert.Contains("csv", formats);
        Assert.Contains("xml", formats);
    }

    [Fact]
    public void DefaultBatchSize_ReturnsPositiveValue()
    {
        // Act
        var batchSize = IPorter.DefaultBatchSize;

        // Assert
        Assert.True(batchSize > 0);
        Assert.Equal(1000, batchSize);
    }

    private static IReadOnlyList<Chunk> CreateChunks(int count)
    {
        var chunks = new List<Chunk>();
        for (int i = 0; i < count; i++)
        {
            chunks.Add(new Chunk
            {
                Text = $"Chunk {i}",
                StartIndex = i * 10,
                EndIndex = (i + 1) * 10,
                TokenCount = 5
            });
        }
        return chunks;
    }
}
