namespace Chonkie.Core.Extensions;

using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;

/// <summary>
/// C# 14 extension members for IChunker interface.
/// Provides additional utility methods and properties for chunker implementations.
/// </summary>
public static class ChunkerExtensions
{
    /// <summary>
    /// Extension members for IChunker instances.
    /// </summary>
    extension(IChunker chunker)
    {
        /// <summary>
        /// Gets the strategy name of the chunker (type name without "Chunker" suffix).
        /// </summary>
        public string StrategyName => chunker.GetType().Name.Replace("Chunker", string.Empty);

        /// <summary>
        /// Chunks multiple texts asynchronously using Task.Run for background processing.
        /// </summary>
        /// <param name="texts">The texts to chunk.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<IReadOnlyList<IReadOnlyList<Chunk>>> ChunkBatchAsync(
            IEnumerable<string> texts,
            CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => chunker.ChunkBatch(texts, null, cancellationToken), cancellationToken);
        }
    }

    /// <summary>
    /// Static extension members for IChunker type.
    /// </summary>
    extension(IChunker)
    {
        /// <summary>
        /// Gets an empty chunk list for initialization or fallback scenarios.
        /// </summary>
        public static IReadOnlyList<Chunk> Empty => Array.Empty<Chunk>();
    }
}
