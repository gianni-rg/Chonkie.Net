using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Core.Types;

namespace Chonkie.Refineries
{
    /// <summary>
    /// Defines the contract for chunk refineries (post-processing).
    /// </summary>
    public interface IRefinery
    {
        /// <summary>
        /// Refines a list of chunks (e.g., merging, adding embeddings).
        /// </summary>
        /// <param name="chunks">Input chunks.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Refined chunks.</returns>
        Task<IReadOnlyList<Chunk>> RefineAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken = default);
    }
}
