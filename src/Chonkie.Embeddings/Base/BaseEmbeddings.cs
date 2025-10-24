using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Chonkie.Embeddings.Base
{
    /// <summary>
    /// Base class for embedding providers.
    /// </summary>
    public abstract class BaseEmbeddings : Interfaces.IEmbeddings
    {
        /// <summary>
        /// Gets the name of the embedding provider.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the dimension of the embedding vectors.
        /// </summary>
        public abstract int Dimension { get; }

        /// <summary>
        /// Generates an embedding vector for the given text.
        /// </summary>
        /// <param name="text">The text to embed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The embedding vector.</returns>
        public abstract Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates embedding vectors for multiple texts.
        /// </summary>
        /// <param name="texts">The texts to embed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A collection of embedding vectors.</returns>
        public virtual async Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
        {
            var results = new List<float[]>();
            foreach (var text in texts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                results.Add(await EmbedAsync(text, cancellationToken));
            }
            return results;
        }

        /// <summary>
        /// Computes the cosine similarity between two embedding vectors.
        /// </summary>
        /// <param name="u">First embedding vector.</param>
        /// <param name="v">Second embedding vector.</param>
        /// <returns>Cosine similarity score between -1 and 1.</returns>
        public virtual float Similarity(float[] u, float[] v)
        {
            return VectorMath.CosineSimilarity(u, v);
        }

        /// <summary>
        /// Returns a string representation of the embeddings provider.
        /// </summary>
        public override string ToString()
        {
            return $"{GetType().Name}(name={Name}, dimension={Dimension})";
        }
    }
}
