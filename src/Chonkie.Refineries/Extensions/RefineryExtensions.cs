namespace Chonkie.Refineries.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Core.Types;

/// <summary>
/// C# 14 extension members for IRefinery interface.
/// Provides additional utility methods and properties for refinery implementations.
/// </summary>
public static class RefineryExtensions
{
    /// <summary>
    /// Extension members for IRefinery instances.
    /// </summary>
    extension(IRefinery refinery)
{
    /// <summary>
    /// Gets the refinery type name (type name without "Refinery" suffix).
    /// </summary>
        public string RefineryType => refinery.GetType().Name.Replace("Refinery", string.Empty);
        /// <param name="chunks">The chunks to refine.</param>
        /// <param name="batchSize">Number of chunks to process per batch.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation with refined chunks.</returns>
        public async Task<IReadOnlyList<Chunk>> RefineInBatchesAsync(
            IReadOnlyList<Chunk> chunks,
            int batchSize = 100,
            CancellationToken cancellationToken = default)
        {
            if (batchSize <= 0)
            {
                throw new ArgumentException("Batch size must be positive", nameof(batchSize));
            }

            if (chunks.Count <= batchSize)
            {
                // Small enough to process in one go
                return await refinery.RefineAsync(chunks, cancellationToken);
            }

            // Process in batches
            var results = new List<Chunk>();
            for (int i = 0; i < chunks.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var batch = chunks.Skip(i).Take(batchSize).ToList();
                var refined = await refinery.RefineAsync(batch, cancellationToken);
                results.AddRange(refined);
            }
            return results;
        }

        /// <summary>
        /// Checks if the refinery would modify the given chunks.
        /// </summary>
        /// <param name="chunks">The chunks to check.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the chunks would be modified; otherwise, false.</returns>
        public async Task<bool> WouldModifyAsync(
            IReadOnlyList<Chunk> chunks,
            CancellationToken cancellationToken = default)
        {
            var refined = await refinery.RefineAsync(chunks, cancellationToken);
            return refined.Count != chunks.Count || 
                   !refined.SequenceEqual(chunks, ChunkEqualityComparer.Instance);
        }
    }

    /// <summary>
    /// Static extension members for IRefinery type.
    /// </summary>
    extension(IRefinery)
    {
        /// <summary>
        /// Gets an empty chunk list for initialization or fallback scenarios.
        /// </summary>
        public static IReadOnlyList<Chunk> Empty => Array.Empty<Chunk>();
    }
}

/// <summary>
/// Equality comparer for chunks that checks text content and metadata.
/// </summary>
internal class ChunkEqualityComparer : IEqualityComparer<Chunk>
    {
        public static readonly ChunkEqualityComparer Instance = new();

        public bool Equals(Chunk? x, Chunk? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            
            return string.Equals(x.Text, y.Text, StringComparison.Ordinal) &&
                   x.TokenCount == y.TokenCount &&
                   x.StartIndex == y.StartIndex &&
                   x.EndIndex == y.EndIndex;
        }

    public int GetHashCode(Chunk obj)
    {
        return HashCode.Combine(obj.Text, obj.TokenCount, obj.StartIndex, obj.EndIndex);
    }
}
