// Copyright 2025-2026 Gianni Rosa Gallina and Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Chonkie.Refineries.Tests.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Core.Types;
using Chonkie.Refineries;
using Chonkie.Refineries.Extensions;
using Xunit;

/// <summary>
/// Tests for IRefinery extension members (C# 14).
/// </summary>
public class RefineryExtensionsTests
{
    private class TestRefinery : IRefinery
    {
        private readonly Func<Chunk, Chunk> _transform;

        public TestRefinery(Func<Chunk, Chunk>? transform = null)
        {
            _transform = transform ?? (c => c);
        }

        public Task<IReadOnlyList<Chunk>> RefineAsync(
            IReadOnlyList<Chunk> chunks,
            CancellationToken cancellationToken = default)
        {
            var refined = chunks.Select(_transform).ToList();
            return Task.FromResult<IReadOnlyList<Chunk>>(refined);
        }
    }

    [Fact]
    public void RefineryType_ReturnsRefineryTypeName()
    {
        // Arrange
        var refinery = new OverlapRefinery();

        // Act
        var type = refinery.RefineryType;

        // Assert
        Assert.Equal("Overlap", type);
    }

    [Fact]
    public async Task RefineInBatchesAsync_WithSmallList_ProcessesInOneGo()
    {
        // Arrange
        var refinery = new TestRefinery(c => c with { Text = c.Text + " refined" });
        var chunks = CreateChunks(5);

        // Act
        var results = await refinery.RefineInBatchesAsync(chunks, batchSize: 10);

        // Assert
        Assert.Equal(5, results.Count);
        Assert.All(results, c => Assert.EndsWith(" refined", c.Text));
    }

    [Fact]
    public async Task RefineInBatchesAsync_WithLargeList_ProcessesInBatches()
    {
        // Arrange
        var refinery = new TestRefinery(c => c with { Text = c.Text + " refined" });
        var chunks = CreateChunks(25);

        // Act
        var results = await refinery.RefineInBatchesAsync(chunks, batchSize: 10);

        // Assert
        Assert.Equal(25, results.Count);
        Assert.All(results, c => Assert.EndsWith(" refined", c.Text));
    }

    [Fact]
    public async Task RefineInBatchesAsync_WithInvalidBatchSize_ThrowsException()
    {
        // Arrange
        var refinery = new TestRefinery();
        var chunks = CreateChunks(5);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => refinery.RefineInBatchesAsync(chunks, batchSize: 0));
    }

    [Fact]
    public async Task RefineInBatchesAsync_WithCancellation_CanBeCancelled()
    {
        // Arrange
        var refinery = new TestRefinery();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var chunks = CreateChunks(100);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => refinery.RefineInBatchesAsync(chunks, batchSize: 10, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task WouldModifyAsync_WithModifyingRefinery_ReturnsTrue()
    {
        // Arrange
        var refinery = new TestRefinery(c => c with { Text = c.Text + " modified" });
        var chunks = CreateChunks(3);

        // Act
        var wouldModify = await refinery.WouldModifyAsync(chunks);

        // Assert
        Assert.True(wouldModify);
    }

    [Fact]
    public async Task WouldModifyAsync_WithNonModifyingRefinery_ReturnsFalse()
    {
        // Arrange
        var refinery = new TestRefinery(); // Returns same chunks
        var chunks = CreateChunks(3);

        // Act
        var wouldModify = await refinery.WouldModifyAsync(chunks);

        // Assert
        Assert.False(wouldModify);
    }

    [Fact]
    public async Task WouldModifyAsync_WithDifferentCount_ReturnsTrue()
    {
        // Arrange - Create a refinery that filters chunks
        var filteringRefinery = new FilteringRefinery();
        var chunks = CreateChunks(3);

        // Act
        var wouldModify = await filteringRefinery.WouldModifyAsync(chunks);

        // Assert - different count means modification
        Assert.True(wouldModify);
    }

    [Fact]
    public void Empty_ReturnsEmptyChunkList()
    {
        // Act
        var empty = IRefinery.Empty;

        // Assert
        Assert.NotNull(empty);
        Assert.Empty(empty);
    }

    private class FilteringRefinery : IRefinery
    {
        public Task<IReadOnlyList<Chunk>> RefineAsync(
            IReadOnlyList<Chunk> chunks,
            CancellationToken cancellationToken = default)
        {
            // Filter out some chunks to change the count
            var filtered = chunks.Take(chunks.Count - 1).ToList();
            return Task.FromResult<IReadOnlyList<Chunk>>(filtered);
        }
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
