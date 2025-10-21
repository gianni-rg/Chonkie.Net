using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Core.Types;

namespace Chonkie.Embeddings.Interfaces
{
    /// <summary>
    /// Defines the contract for embedding providers.
    /// </summary>
    public interface IEmbeddings
    {
        /// <summary>
        /// Gets the name of the embedding provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the dimension of the embedding vectors.
        /// </summary>
        int Dimension { get; }

        /// <summary>
        /// Generates an embedding for a single text.
        /// </summary>
        Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates embeddings for a batch of texts.
        /// </summary>
        Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default);
    }
}