using System.Numerics.Tensors;

namespace Chonkie.Benchmarks;

/// <summary>
/// Standalone tensor operations for benchmarking comparisons.
/// </summary>
public static class TensorOps
{
    /// <summary>
    /// Calculates magnitude using TensorPrimitives.
    /// </summary>
    public static float Magnitude(float[] vector) => TensorPrimitives.Norm(vector);

    /// <summary>
    /// Calculates cosine similarity using TensorPrimitives.
    /// </summary>
    public static float CosineSimilarity(float[] a, float[] b) => TensorPrimitives.CosineSimilarity(a, b);

    /// <summary>
    /// Normalizes vector in-place using TensorPrimitives.
    /// </summary>
    public static void NormalizeInPlace(float[] vector)
    {
        var magnitude = TensorPrimitives.Norm(vector);
        if (magnitude > 0)
        {
            TensorPrimitives.Divide(vector, magnitude, vector);
        }
    }

    /// <summary>
    /// Batch cosine similarity using TensorPrimitives.
    /// </summary>
    public static float[] BatchCosineSimilarity(float[] query, List<float[]> candidates)
    {
        var results = new float[candidates.Count];
        for (int i = 0; i < candidates.Count; i++)
        {
            results[i] = TensorPrimitives.CosineSimilarity(query, candidates[i]);
        }
        return results;
    }

    /// <summary>
    /// Finds most similar using TensorPrimitives.
    /// </summary>
    public static int FindMostSimilar(float[] query, List<float[]> candidates)
    {
        var maxSimilarity = float.MinValue;
        var maxIndex = -1;

        for (int i = 0; i < candidates.Count; i++)
        {
            var similarity = TensorPrimitives.CosineSimilarity(query, candidates[i]);
            if (similarity > maxSimilarity)
            {
                maxSimilarity = similarity;
                maxIndex = i;
            }
        }

        return maxIndex;
    }
}
