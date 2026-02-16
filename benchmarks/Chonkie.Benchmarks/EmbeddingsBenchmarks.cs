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

using BenchmarkDotNet.Attributes;

namespace Chonkie.Benchmarks;

/// <summary>
/// Benchmarks for embeddings operations comparing TensorPrimitives vs traditional implementations.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class EmbeddingsBenchmarks
{
    private float[] _embedding384 = null!;
    private float[] _embedding768 = null!;
    private float[] _embedding1024 = null!;
    private float[] _query = null!;
    private List<float[]> _batchEmbeddings = null!;

    /// <summary>
    /// Sets up test data for benchmarks.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        
        // Create embeddings of different sizes (common dimensions)
        _embedding384 = Enumerable.Range(0, 384).Select(_ => (float)random.NextDouble()).ToArray();
        _embedding768 = Enumerable.Range(0, 768).Select(_ => (float)random.NextDouble()).ToArray();
        _embedding1024 = Enumerable.Range(0, 1024).Select(_ => (float)random.NextDouble()).ToArray();
        
        // Query embedding for similarity search
        _query = Enumerable.Range(0, 384).Select(_ => (float)random.NextDouble()).ToArray();
        
        // Batch of embeddings for batch operations
        _batchEmbeddings = Enumerable.Range(0, 100)
            .Select(_ => Enumerable.Range(0, 384).Select(_ => (float)random.NextDouble()).ToArray())
            .ToList();
    }

    #region Magnitude Benchmarks

    /// <summary>
    /// Benchmark for computing magnitude using TensorPrimitives (384 dimensions).
    /// </summary>
    [Benchmark(Description = "TensorPrimitives Magnitude (384-dim)")]
    [BenchmarkCategory("Magnitude")]
    public float Magnitude384_TensorPrimitives()
    {
        return TensorOps.Magnitude(_embedding384);
    }

    /// <summary>
    /// Benchmark for computing magnitude using traditional loop (384 dimensions).
    /// </summary>
    [Benchmark(Baseline = true, Description = "Traditional Magnitude (384-dim)")]
    [BenchmarkCategory("Magnitude")]
    public float Magnitude384_Traditional()
    {
        return MagnitudeTraditional(_embedding384);
    }

    /// <summary>
    /// Benchmark for computing magnitude using TensorPrimitives (768 dimensions).
    /// </summary>
    [Benchmark(Description = "TensorPrimitives Magnitude (768-dim)")]
    [BenchmarkCategory("Magnitude")]
    public float Magnitude768_TensorPrimitives()
    {
        return TensorOps.Magnitude(_embedding768);
    }

    /// <summary>
    /// Benchmark for computing magnitude using traditional loop (768 dimensions).
    /// </summary>
    [Benchmark(Description = "Traditional Magnitude (768-dim)")]
    [BenchmarkCategory("Magnitude")]
    public float Magnitude768_Traditional()
    {
        return MagnitudeTraditional(_embedding768);
    }

    /// <summary>
    /// Benchmark for computing magnitude using TensorPrimitives (1024 dimensions).
    /// </summary>
    [Benchmark(Description = "TensorPrimitives Magnitude (1024-dim)")]
    [BenchmarkCategory("Magnitude")]
    public float Magnitude1024_TensorPrimitives()
    {
        return TensorOps.Magnitude(_embedding1024);
    }

    /// <summary>
    /// Benchmark for computing magnitude using traditional loop (1024 dimensions).
    /// </summary>
    [Benchmark(Description = "Traditional Magnitude (1024-dim)")]
    [BenchmarkCategory("Magnitude")]
    public float Magnitude1024_Traditional()
    {
        return MagnitudeTraditional(_embedding1024);
    }

    #endregion

    #region Cosine Similarity Benchmarks

    /// <summary>
    /// Benchmark for computing cosine similarity using TensorPrimitives (384 dimensions).
    /// </summary>
    [Benchmark(Description = "TensorPrimitives CosineSimilarity (384-dim)")]
    [BenchmarkCategory("CosineSimilarity")]
    public float CosineSimilarity_TensorPrimitives()
    {
        return TensorOps.CosineSimilarity(_query, _embedding384);
    }

    /// <summary>
    /// Benchmark for computing cosine similarity using traditional implementation (384 dimensions).
    /// </summary>
    [Benchmark(Baseline = true, Description = "Traditional CosineSimilarity (384-dim)")]
    [BenchmarkCategory("CosineSimilarity")]
    public float CosineSimilarity_Traditional()
    {
        return CosineSimilarityTraditional(_query, _embedding384);
    }

    #endregion

    #region Normalization Benchmarks

    /// <summary>
    /// Benchmark for normalizing embeddings in-place using TensorPrimitives (384 dimensions).
    /// </summary>
    [Benchmark(Description = "TensorPrimitives NormalizeInPlace (384-dim)")]
    [BenchmarkCategory("Normalization")]
    public void NormalizeInPlace_TensorPrimitives()
    {
        var copy = (float[])_embedding384.Clone();
        TensorOps.NormalizeInPlace(copy);
    }

    /// <summary>
    /// Benchmark for normalizing embeddings using traditional loop (384 dimensions).
    /// </summary>
    [Benchmark(Baseline = true, Description = "Traditional Normalize (384-dim)")]
    [BenchmarkCategory("Normalization")]
    public float[] Normalize_Traditional()
    {
        return NormalizeTraditional(_embedding384);
    }

    #endregion

    #region Batch Operations Benchmarks

    /// <summary>
    /// Benchmark for batch cosine similarity using TensorPrimitives (100 embeddings, 384-dim each).
    /// </summary>
    [Benchmark(Description = "TensorPrimitives BatchCosineSimilarity (100x384)")]
    [BenchmarkCategory("BatchOperations")]
    public float[] BatchCosineSimilarity_TensorPrimitives()
    {
        return TensorOps.BatchCosineSimilarity(_query, _batchEmbeddings);
    }

    /// <summary>
    /// Benchmark for batch cosine similarity using traditional implementation (100 embeddings, 384-dim each).
    /// </summary>
    [Benchmark(Baseline = true, Description = "Traditional BatchCosineSimilarity (100x384)")]
    [BenchmarkCategory("BatchOperations")]
    public float[] BatchCosineSimilarity_Traditional()
    {
        return _batchEmbeddings
            .Select(e => CosineSimilarityTraditional(_query, e))
            .ToArray();
    }

    /// <summary>
    /// Benchmark for finding most similar embedding using TensorPrimitives.
    /// </summary>
    [Benchmark(Description = "TensorPrimitives FindMostSimilar (100x384)")]
    [BenchmarkCategory("BatchOperations")]
    public int FindMostSimilar_TensorPrimitives()
    {
        return TensorOps.FindMostSimilar(_query, _batchEmbeddings);
    }

    /// <summary>
    /// Benchmark for finding most similar embedding using traditional implementation.
    /// </summary>
    [Benchmark(Description = "Traditional FindMostSimilar (100x384)")]
    [BenchmarkCategory("BatchOperations")]
    public int FindMostSimilar_Traditional()
    {
        var maxSimilarity = float.MinValue;
        var maxIndex = -1;
        
        for (var i = 0; i < _batchEmbeddings.Count; i++)
        {
            var similarity = CosineSimilarityTraditional(_query, _batchEmbeddings[i]);
            if (similarity > maxSimilarity)
            {
                maxSimilarity = similarity;
                maxIndex = i;
            }
        }
        
        return maxIndex;
    }

    #endregion

    #region Traditional Implementations (for comparison)

    private static float MagnitudeTraditional(float[] embedding)
    {
        var sumSquares = 0f;
        foreach (var value in embedding)
        {
            sumSquares += value * value;
        }
        return MathF.Sqrt(sumSquares);
    }

    private static float CosineSimilarityTraditional(float[] a, float[] b)
    {
        var dotProduct = 0f;
        var magnitudeA = 0f;
        var magnitudeB = 0f;

        for (var i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            magnitudeA += a[i] * a[i];
            magnitudeB += b[i] * b[i];
        }

        magnitudeA = MathF.Sqrt(magnitudeA);
        magnitudeB = MathF.Sqrt(magnitudeB);

        return dotProduct / (magnitudeA * magnitudeB);
    }

    private static float[] NormalizeTraditional(float[] embedding)
    {
        var magnitude = MagnitudeTraditional(embedding);
        var result = new float[embedding.Length];
        
        for (var i = 0; i < embedding.Length; i++)
        {
            result[i] = embedding[i] / magnitude;
        }
        
        return result;
    }

    #endregion
}
