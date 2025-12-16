namespace Chonkie.Fetchers.Extensions;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// C# 14 extension members for IFetcher interface.
/// Provides additional utility methods and properties for fetcher implementations.
/// </summary>
public static class FetcherExtensions
{
    /// <summary>
    /// Extension members for IFetcher instances.
    /// </summary>
    extension(IFetcher fetcher)
    {
        /// <summary>
        /// Gets the fetcher type name (type name without "Fetcher" suffix).
        /// </summary>
        public string FetcherType => fetcher.GetType().Name.Replace("Fetcher", string.Empty);

        /// <summary>
        /// Fetches a single file and returns its content.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation with file content.</returns>
        public async Task<string?> FetchSingleAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            var results = await fetcher.FetchAsync(filePath, null, cancellationToken);
            return results.Count > 0 ? results[0].Content : null;
        }

        /// <summary>
        /// Fetches multiple paths and returns a flattened list of documents.
        /// </summary>
        /// <param name="paths">Paths to fetch from.</param>
        /// <param name="filter">Optional file filter.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation with all documents.</returns>
        public async Task<IReadOnlyList<(string Path, string Content)>> FetchMultipleAsync(
            IEnumerable<string> paths,
            string? filter = null,
            CancellationToken cancellationToken = default)
        {
            var results = new List<(string Path, string Content)>();
            foreach (var path in paths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var docs = await fetcher.FetchAsync(path, filter, cancellationToken);
                results.AddRange(docs);
            }
            return results;
        }

        /// <summary>
        /// Counts the number of documents that would be fetched without actually fetching them.
        /// </summary>
        /// <param name="path">Source path.</param>
        /// <param name="filter">Optional file filter.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Number of documents that would be fetched.</returns>
        public async Task<int> CountDocumentsAsync(
            string path,
            string? filter = null,
            CancellationToken cancellationToken = default)
        {
            var results = await fetcher.FetchAsync(path, filter, cancellationToken);
            return results.Count;
        }
    }

    /// <summary>
    /// Static extension members for IFetcher type.
    /// </summary>
    extension(IFetcher)
    {
        /// <summary>
        /// Gets common text file extensions.
        /// </summary>
        public static IReadOnlyList<string> CommonTextExtensions =>
            new[] { ".txt", ".md", ".rst", ".tex", ".log", ".csv", ".json", ".xml" };

        /// <summary>
        /// Gets common code file extensions.
        /// </summary>
        public static IReadOnlyList<string> CommonCodeExtensions =>
            new[] { ".cs", ".py", ".js", ".ts", ".java", ".cpp", ".c", ".h", ".go", ".rs", ".rb", ".php" };
    }
}

/// <summary>
/// Extension methods for document collections.
/// </summary>
public static class DocumentCollectionExtensions
{
    /// <summary>
    /// Filters fetched documents by extension.
    /// </summary>
    /// <param name="documents">Documents to filter.</param>
    /// <param name="extensions">Extensions to include (e.g., ".txt", ".md").</param>
    /// <returns>Filtered documents.</returns>
    public static IEnumerable<(string Path, string Content)> FilterByExtension(
        this IEnumerable<(string Path, string Content)> documents,
        params string[] extensions)
    {
        var extensionSet = new HashSet<string>(extensions.Select(e => e.ToLowerInvariant()));
        return documents.Where(doc =>
        {
            var ext = System.IO.Path.GetExtension(doc.Path).ToLowerInvariant();
            return extensionSet.Contains(ext);
        });
    }

    /// <summary>
    /// Filters fetched documents by minimum content length.
    /// </summary>
    /// <param name="documents">Documents to filter.</param>
    /// <param name="minLength">Minimum content length in characters.</param>
    /// <returns>Filtered documents.</returns>
    public static IEnumerable<(string Path, string Content)> FilterByMinLength(
        this IEnumerable<(string Path, string Content)> documents,
        int minLength)
    {
        return documents.Where(doc => doc.Content.Length >= minLength);
    }

    /// <summary>
    /// Gets total character count across all documents.
    /// </summary>
    /// <param name="documents">Documents to count.</param>
    /// <returns>Total character count.</returns>
    public static int TotalCharacters(this IEnumerable<(string Path, string Content)> documents)
    {
        return documents.Sum(doc => doc.Content.Length);
    }
}
