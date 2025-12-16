namespace Chonkie.Embeddings.Extensions;

using Chonkie.Embeddings.Interfaces;

/// <summary>
/// C# 14 extension members for IEmbeddings interface.
/// Provides additional utility methods and properties for embeddings implementations.
/// </summary>
public static class EmbeddingsExtensions
{
    /// <summary>
    /// Extension members for IEmbeddings instances.
    /// </summary>
    extension(IEmbeddings embeddings)
    {
        /// <summary>
        /// Gets the provider type (name without "Embeddings" suffix).
        /// </summary>
        public string ProviderType => embeddings.Name.Replace("Embeddings", string.Empty);

        /// <summary>
        /// Calculates the magnitude (L2 norm) of an embedding vector.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <returns>The magnitude of the vector.</returns>
        public float Magnitude(float[] vector)
        {
            var sumOfSquares = 0.0f;
            foreach (var value in vector)
            {
                sumOfSquares += value * value;
            }
            return (float)Math.Sqrt(sumOfSquares);
        }

        /// <summary>
        /// Checks if an embedding vector is normalized (unit length).
        /// </summary>
        /// <param name="vector">The vector to check.</param>
        /// <param name="tolerance">Tolerance for floating-point comparison.</param>
        /// <returns>True if the vector has unit magnitude; otherwise, false.</returns>
        public bool IsNormalized(float[] vector, float tolerance = 1e-5f)
        {
            var magnitude = embeddings.Magnitude(vector);
            return Math.Abs(magnitude - 1.0f) < tolerance;
        }

        /// <summary>
        /// Calculates the Euclidean distance between two embedding vectors.
        /// </summary>
        /// <param name="u">First vector.</param>
        /// <param name="v">Second vector.</param>
        /// <returns>The Euclidean distance.</returns>
        public float Distance(float[] u, float[] v)
        {
            if (u.Length != v.Length)
            {
                throw new ArgumentException("Vectors must have the same dimension.");
            }

            var sumOfSquares = 0.0f;
            for (int i = 0; i < u.Length; i++)
            {
                var diff = u[i] - v[i];
                sumOfSquares += diff * diff;
            }
            return (float)Math.Sqrt(sumOfSquares);
        }
    }

    /// <summary>
    /// Static extension members for IEmbeddings type.
    /// </summary>
    extension(IEmbeddings)
    {
        /// <summary>
        /// Gets the default embedding dimension commonly used (384 for all-MiniLM-L6-v2).
        /// </summary>
        public static int DefaultDimension => 384;

        /// <summary>
        /// Creates a zero vector of the specified dimension.
        /// </summary>
        /// <param name="dimension">The dimension of the zero vector.</param>
        /// <returns>A zero vector.</returns>
        public static float[] Zero(int dimension)
        {
            return new float[dimension];
        }
    }
}
