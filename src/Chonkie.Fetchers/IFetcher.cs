using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Chonkie.Fetchers
{
    /// <summary>
    /// Defines the contract for data fetchers (file, directory, etc).
    /// </summary>
    public interface IFetcher
    {
        /// <summary>
        /// Fetches text data from a source (file, directory, etc).
        /// </summary>
        /// <param name="path">Source path (file or directory).</param>
        /// <param name="filter">Optional file filter (e.g., *.txt).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of documents (path, content).</returns>
        Task<IReadOnlyList<(string Path, string Content)>> FetchAsync(string path, string? filter = null, CancellationToken cancellationToken = default);
    }
}
