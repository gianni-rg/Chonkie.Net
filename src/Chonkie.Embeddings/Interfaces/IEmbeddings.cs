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

        /// <summary>
        /// Computes the cosine similarity between two embedding vectors.
        /// </summary>
        /// <param name="u">First embedding vector.</param>
        /// <param name="v">Second embedding vector.</param>
        /// <returns>Cosine similarity score between 0 and 1.</returns>
        float Similarity(float[] u, float[] v);
    }
}
