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
        /// <returns>Cosine similarity score between 0 and 1.</returns>
        public virtual float Similarity(float[] u, float[] v)
        {
            if (u == null || v == null)
                throw new ArgumentNullException(u == null ? nameof(u) : nameof(v));
            
            if (u.Length != v.Length)
                throw new ArgumentException("Embedding vectors must have the same length.");

            // Compute cosine similarity: dot(u, v) / (||u|| * ||v||)
            float dotProduct = 0f;
            float normU = 0f;
            float normV = 0f;

            for (int i = 0; i < u.Length; i++)
            {
                dotProduct += u[i] * v[i];
                normU += u[i] * u[i];
                normV += v[i] * v[i];
            }

            var denominator = MathF.Sqrt(normU) * MathF.Sqrt(normV);
            return denominator > 0 ? dotProduct / denominator : 0f;
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