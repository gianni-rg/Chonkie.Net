using System;

namespace Chonkie.Embeddings
{
    /// <summary>
    /// Utility class for vector mathematics operations.
    /// </summary>
    public static class VectorMath
    {
        /// <summary>
        /// Computes the cosine similarity between two vectors.
        /// Cosine similarity measures the cosine of the angle between two vectors,
        /// producing a value between -1 (opposite) and 1 (identical).
        /// </summary>
        /// <param name="u">First vector.</param>
        /// <param name="v">Second vector.</param>
        /// <returns>Cosine similarity score between -1 and 1.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either vector is null.</exception>
        /// <exception cref="ArgumentException">Thrown when vectors have different lengths.</exception>
        public static float CosineSimilarity(float[] u, float[] v)
        {
            if (u == null)
                throw new ArgumentNullException(nameof(u));
            if (v == null)
                throw new ArgumentNullException(nameof(v));
            if (u.Length != v.Length)
                throw new ArgumentException("Vectors must have the same length.");

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
    }
}
