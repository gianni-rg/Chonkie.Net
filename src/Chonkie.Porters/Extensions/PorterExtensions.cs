namespace Chonkie.Porters.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Core.Types;

/// <summary>
/// C# 14 extension members for IPorter interface.
/// Provides additional utility methods and properties for porter implementations.
/// </summary>
public static class PorterExtensions
{
    /// <summary>
    /// Extension members for IPorter instances.
    /// </summary>
    extension(IPorter porter)
    {
        /// <summary>
        /// Gets the porter type name (type name without "Porter" suffix).
        /// </summary>
        public string PorterType => porter.GetType().Name.Replace("Porter", string.Empty);

        /// <summary>
        /// Exports chunks in batches for better memory management with large chunk lists.
        /// </summary>
        /// <param name="chunks">The chunks to export.</param>
        /// <param name="destination">Destination path or identifier.</param>
        /// <param name="batchSize">Number of chunks to export per batch.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation with success status.</returns>
        public async Task<bool> ExportInBatchesAsync(
            IReadOnlyList<Chunk> chunks,
            string destination,
            int batchSize = 1000,
            CancellationToken cancellationToken = default)
        {
            if (batchSize <= 0)
            {
                throw new ArgumentException("Batch size must be positive", nameof(batchSize));
            }

            if (chunks.Count <= batchSize)
            {
                // Small enough to export in one go
                return await porter.ExportAsync(chunks, destination, cancellationToken);
            }

            // Export in batches
            for (int i = 0; i < chunks.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var batch = chunks.Skip(i).Take(batchSize).ToList();
                var batchDestination = $"{destination}.part{i / batchSize}";
                var success = await porter.ExportAsync(batch, batchDestination, cancellationToken);
                
                if (!success)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Exports multiple chunk lists to separate destinations.
        /// </summary>
        /// <param name="chunkLists">Lists of chunks to export.</param>
        /// <param name="destinationPattern">Destination pattern with {0} for index.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation with success status.</returns>
        public async Task<bool> ExportMultipleAsync(
            IEnumerable<IReadOnlyList<Chunk>> chunkLists,
            string destinationPattern,
            CancellationToken cancellationToken = default)
        {
            int index = 0;
            foreach (var chunks in chunkLists)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var destination = string.Format(destinationPattern, index++);
                var success = await porter.ExportAsync(chunks, destination, cancellationToken);
                
                if (!success)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Static extension members for IPorter type.
    /// </summary>
    extension(IPorter)
    {
        /// <summary>
        /// Gets common export formats.
        /// </summary>
        public static IReadOnlyList<string> CommonFormats => 
            new[] { "json", "csv", "xml", "parquet", "arrow" };

        /// <summary>
        /// Gets the default batch size for exports.
        /// </summary>
        public static int DefaultBatchSize => 1000;
    }
}
