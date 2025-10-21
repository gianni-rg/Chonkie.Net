using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Core.Types;

namespace Chonkie.Porters
{
    /// <summary>
    /// Defines the contract for exporting chunked data.
    /// </summary>
    public interface IPorter
    {
        /// <summary>
        /// Exports a list of chunks to a destination (file, stream, etc).
        /// </summary>
        /// <param name="chunks">Chunks to export.</param>
        /// <param name="destination">Destination path or identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if export succeeded.</returns>
        Task<bool> ExportAsync(IReadOnlyList<Chunk> chunks, string destination, CancellationToken cancellationToken = default);
    }
}
