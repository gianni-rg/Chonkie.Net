// Copyright 2025-2026 Gianni Rosa Gallina and Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
