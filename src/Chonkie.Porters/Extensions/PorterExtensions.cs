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
        /// Creates separate batch files with sequential numbering when batch boundaries are exceeded.
        /// </summary>
        /// <param name="chunks">The chunks to export. Must not be null or empty.</param>
        /// <param name="destination">Base destination path or identifier. Batch files will be named as {destination}.part0, {destination}.part1, etc.</param>
        /// <param name="batchSize">Number of chunks to export per batch. Default is 1000. Must be positive.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true if all batches exported successfully; false if any batch fails.</returns>
        /// <remarks>
        /// This method is useful for exporting large numbers of chunks when file size limits or memory constraints are a concern.
        /// For example, exporting 1 million chunks in a single file might exceed memory or disk space limits, so batching into
        /// 1000-chunk batches creates more manageable files.
        /// If the total chunk count is less than or equal to batchSize, all chunks are exported in a single file.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when batchSize is not positive.</exception>
        /// <example>
        /// <code>
        /// var porter = new JsonPorter();
        /// var chunks = new List&lt;Chunk&gt; { /* 50,000 chunks */ };
        /// 
        /// // Export in batches of 5000 to create 10 separate files
        /// var success = await porter.ExportInBatchesAsync(chunks, "output/chunks", batchSize: 5000);
        /// 
        /// // Results in: output/chunks.part0, output/chunks.part1, ..., output/chunks.part9
        /// </code>
        /// </example>
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
        /// Exports multiple chunk lists to separate destinations using a naming pattern.
        /// Useful for exporting results from multiple pipelines or different data sources.
        /// </summary>
        /// <param name="chunkLists">Enumerable of chunk lists to export. Content is indexed by position in the enumerable.</param>
        /// <param name="destinationPattern">Destination pattern with {0} placeholder for zero-based index. Example: "output/doc_{0}.json"</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. Returns true if all chunk lists exported successfully; false if any export fails.</returns>
        /// <remarks>
        /// This method is ideal for scenarios where you have multiple sets of chunks and need to export them all
        /// with consistent naming. The index parameter in the pattern is zero-based.
        /// If any export fails, the method returns false immediately without continuing with remaining exports.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when chunkLists or destinationPattern is null.</exception>
        /// <example>
        /// <code>
        /// var porter = new JsonPorter();
        /// var chunkCollections = new[]
        /// {
        ///     chunks1, // document 1 chunks
        ///     chunks2, // document 2 chunks
        ///     chunks3  // document 3 chunks
        /// };
        /// 
        /// // Export all with sequential naming
        /// var success = await porter.ExportMultipleAsync(chunkCollections, "output/doc_{0}.json");
        /// 
        /// // Results in: output/doc_0.json, output/doc_1.json, output/doc_2.json
        /// </code>
        /// </example>
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
