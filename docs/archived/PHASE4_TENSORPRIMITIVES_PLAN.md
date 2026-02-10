# Phase 4: TensorPrimitives Migration Plan

## Overview

Migrate embedding similarity calculations and vector operations to `System.Numerics.Tensors.TensorPrimitives` API for hardware-accelerated performance.

**Status:** 🔜 NOT STARTED  
**Prerequisites:** ✅ Phase 1-3 complete  
**Expected Performance Gain:** 20-35% for embedding operations

---

## Objectives

1. Replace custom vector math with TensorPrimitives API
2. Leverage hardware-accelerated SIMD operations
3. Improve embedding similarity calculations
4. Reduce maintenance burden (use framework APIs)
5. Ensure cross-platform hardware acceleration

---

## Target Files

### Primary Targets

1. **EmbeddingsExtensions.cs** (`src\Chonkie.Embeddings\Extensions\`)
   - Current: Custom similarity calculations
   - Target: TensorPrimitives.CosineSimilarity, TensorPrimitives.Distance
   - Operations: Magnitude, IsNormalized, Distance

2. **SentenceTransformerEmbeddings.cs** (`src\Chonkie.Embeddings\`)
   - Current: ONNX Runtime with custom pooling
   - Target: TensorPrimitives for post-processing
   - Operations: Mean pooling, normalization

3. **Similarity Utilities** (if any custom implementations exist)
   - Current: Manual dot product, Euclidean distance
   - Target: TensorPrimitives vectorized operations

---

## TensorPrimitives API Reference

### Available Operations (.NET 10)

```csharp
using System.Numerics.Tensors;

// Dot product (for cosine similarity)
float dotProduct = TensorPrimitives.Dot(
    new ReadOnlySpan<float>(vector1), 
    new ReadOnlySpan<float>(vector2)
);

// Euclidean distance
float distance = TensorPrimitives.Distance(
    new ReadOnlySpan<float>(vector1), 
    new ReadOnlySpan<float>(vector2)
);

// Cosine similarity (new in .NET 10)
float similarity = TensorPrimitives.CosineSimilarity(
    new ReadOnlySpan<float>(vector1), 
    new ReadOnlySpan<float>(vector2)
);

// L2 norm (magnitude)
float magnitude = TensorPrimitives.Norm(new ReadOnlySpan<float>(vector));

// Sum (for mean pooling)
float sum = TensorPrimitives.Sum(new ReadOnlySpan<float>(vector));

// Multiply by scalar (for normalization)
TensorPrimitives.Multiply(
    new ReadOnlySpan<float>(input), 
    scalar, 
    new Span<float>(output)
);
```

### Key Benefits

- **Hardware Acceleration:** Automatic SIMD (AVX2, AVX512, NEON)
- **Cross-Platform:** Works on Windows, Linux, macOS, ARM
- **Optimized:** Framework-level optimizations
- **Span-Based:** Zero-copy operations with ReadOnlySpan<float>

---

## Implementation Tasks

### Task 1: Update EmbeddingsExtensions.cs

**Current Implementation:**
```csharp
public static extension(IEmbeddings) float Magnitude(float[] vector)
{
    float sum = 0f;
    for (int i = 0; i < vector.Length; i++)
        sum += vector[i] * vector[i];
    return MathF.Sqrt(sum);
}

public static extension(IEmbeddings) bool IsNormalized(float[] vector, float tolerance = 0.01f)
{
    float mag = Magnitude(vector);
    return MathF.Abs(mag - 1.0f) < tolerance;
}

public static extension(IEmbeddings) float Distance(float[] vector1, float[] vector2)
{
    if (vector1.Length != vector2.Length)
        throw new ArgumentException("Vectors must have same length");
    
    float sum = 0f;
    for (int i = 0; i < vector1.Length; i++)
    {
        float diff = vector1[i] - vector2[i];
        sum += diff * diff;
    }
    return MathF.Sqrt(sum);
}
```

**Target Implementation:**
```csharp
using System.Numerics.Tensors;

public static extension(IEmbeddings) float Magnitude(float[] vector)
{
    return TensorPrimitives.Norm(vector);
}

public static extension(IEmbeddings) bool IsNormalized(float[] vector, float tolerance = 0.01f)
{
    float mag = TensorPrimitives.Norm(vector);
    return MathF.Abs(mag - 1.0f) < tolerance;
}

public static extension(IEmbeddings) float Distance(float[] vector1, float[] vector2)
{
    if (vector1.Length != vector2.Length)
        throw new ArgumentException("Vectors must have same length");
    
    return TensorPrimitives.Distance(vector1, vector2);
}

// NEW: Add cosine similarity (not in Python version)
public static extension(IEmbeddings) float CosineSimilarity(float[] vector1, float[] vector2)
{
    if (vector1.Length != vector2.Length)
        throw new ArgumentException("Vectors must have same length");
    
    return TensorPrimitives.CosineSimilarity(vector1, vector2);
}
```

**Benefits:**
- 2-3x faster magnitude calculation
- Hardware-accelerated distance computation
- Built-in cosine similarity (no manual dot product)

### Task 2: Optimize SentenceTransformerEmbeddings Pooling

**Current Mean Pooling (if manual):**
```csharp
private float[] MeanPooling(float[] tokenEmbeddings, int sequenceLength, int dimension)
{
    var pooled = new float[dimension];
    for (int i = 0; i < dimension; i++)
    {
        float sum = 0f;
        for (int j = 0; j < sequenceLength; j++)
            sum += tokenEmbeddings[j * dimension + i];
        pooled[i] = sum / sequenceLength;
    }
    return pooled;
}
```

**Target Implementation:**
```csharp
private float[] MeanPooling(float[] tokenEmbeddings, int sequenceLength, int dimension)
{
    var pooled = new float[dimension];
    
    for (int i = 0; i < dimension; i++)
    {
        var columnSpan = new ReadOnlySpan<float>(tokenEmbeddings)
            .Slice(i, sequenceLength * dimension)
            .ToArray(); // Extract column
        
        float sum = TensorPrimitives.Sum(columnSpan);
        pooled[i] = sum / sequenceLength;
    }
    
    return pooled;
}

// Alternative: Use TensorPrimitives.Divide for normalization
private void NormalizeInPlace(float[] vector)
{
    float magnitude = TensorPrimitives.Norm(vector);
    if (magnitude > 0)
        TensorPrimitives.Divide(vector, magnitude, vector);
}
```

**Benefits:**
- Vectorized sum operations
- Hardware-accelerated division
- Reduced manual loop overhead

### Task 3: Add Batch Similarity Operations

```csharp
// NEW: Batch cosine similarity for semantic chunking
public static extension(IEmbeddings) float[] BatchCosineSimilarity(
    float[] queryVector, 
    float[][] candidateVectors)
{
    var similarities = new float[candidateVectors.Length];
    
    for (int i = 0; i < candidateVectors.Length; i++)
    {
        similarities[i] = TensorPrimitives.CosineSimilarity(queryVector, candidateVectors[i]);
    }
    
    return similarities;
}

// NEW: Find most similar vector
public static extension(IEmbeddings) (int Index, float Similarity) FindMostSimilar(
    float[] queryVector, 
    float[][] candidateVectors)
{
    var similarities = BatchCosineSimilarity(queryVector, candidateVectors);
    
    float maxSim = float.MinValue;
    int maxIdx = -1;
    
    for (int i = 0; i < similarities.Length; i++)
    {
        if (similarities[i] > maxSim)
        {
            maxSim = similarities[i];
            maxIdx = i;
        }
    }
    
    return (maxIdx, maxSim);
}
```

---

## Testing Strategy

### Unit Tests

1. **Accuracy Tests** - Verify TensorPrimitives produces same results
   ```csharp
   [Fact]
   public void Magnitude_WithTensorPrimitives_MatchesExpected()
   {
       var vector = new float[] { 3f, 4f, 0f };
       var magnitude = IEmbeddings.Magnitude(vector);
       magnitude.ShouldBe(5f, tolerance: 0.001f);
   }
   ```

2. **Performance Tests** - Benchmark vs. old implementation
   ```csharp
   [Fact]
   public void CosineSimilarity_WithTensorPrimitives_IsFaster()
   {
       var v1 = new float[384];
       var v2 = new float[384];
       
       // Warm up
       IEmbeddings.CosineSimilarity(v1, v2);
       
       var sw = Stopwatch.StartNew();
       for (int i = 0; i < 10000; i++)
           IEmbeddings.CosineSimilarity(v1, v2);
       sw.Stop();
       
       // Should be significantly faster than manual implementation
       sw.ElapsedMilliseconds.ShouldBeLessThan(100);
   }
   ```

3. **Edge Case Tests**
   ```csharp
   [Fact]
   public void Distance_WithZeroVectors_ReturnsZero()
   {
       var zero1 = IEmbeddings.Zero(384);
       var zero2 = IEmbeddings.Zero(384);
       
       var distance = IEmbeddings.Distance(zero1, zero2);
       distance.ShouldBe(0f, tolerance: 0.001f);
   }
   ```

### Integration Tests

1. **Semantic Chunking** - Verify chunking still works correctly
2. **Embedding Similarity** - Validate similarity scores are accurate
3. **Cross-Platform** - Test on Windows, Linux, macOS (if possible)

---

## Performance Benchmarks

### Expected Results

| Operation | Current | With TensorPrimitives | Improvement |
|-----------|---------|----------------------|-------------|
| Magnitude | ~100 µs | ~30 µs | 70% faster |
| Distance | ~150 µs | ~50 µs | 67% faster |
| Cosine Similarity | ~200 µs | ~40 µs | 80% faster |
| Mean Pooling (384-dim) | ~500 µs | ~150 µs | 70% faster |

### Benchmark Code Template

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
public class EmbeddingsBenchmarks
{
    private float[] _vector1;
    private float[] _vector2;

    [GlobalSetup]
    public void Setup()
    {
        _vector1 = new float[384];
        _vector2 = new float[384];
        
        Random.Shared.Fill(_vector1);
        Random.Shared.Fill(_vector2);
    }

    [Benchmark]
    public float CosineSimilarity_TensorPrimitives()
    {
        return TensorPrimitives.CosineSimilarity(_vector1, _vector2);
    }

    [Benchmark]
    public float Magnitude_TensorPrimitives()
    {
        return TensorPrimitives.Norm(_vector1);
    }

    [Benchmark]
    public float Distance_TensorPrimitives()
    {
        return TensorPrimitives.Distance(_vector1, _vector2);
    }
}
```

---

## Migration Checklist

- [ ] Update EmbeddingsExtensions.cs with TensorPrimitives
- [ ] Add CosineSimilarity method (new feature)
- [ ] Optimize SentenceTransformerEmbeddings pooling
- [ ] Add batch similarity operations
- [ ] Create unit tests for accuracy
- [ ] Create performance benchmarks
- [ ] Verify all 527 tests still pass
- [ ] Run benchmarks and document improvements
- [ ] Update XML documentation
- [ ] Create migration guide for users

---

## Dependencies

**NuGet Packages:** None required (included in .NET 10)

**Required Namespaces:**
```csharp
using System;
using System.Numerics.Tensors;
```

**Framework Requirements:**
- .NET 10.0 or later
- TensorPrimitives API (stable in .NET 10)

---

## Risks & Mitigation

| Risk | Mitigation |
|------|-----------|
| Accuracy differences | Comprehensive unit tests with tolerance |
| Breaking API changes | Add new methods, mark old as obsolete if needed |
| Performance regression | Benchmark before/after, revert if slower |
| Platform compatibility | Test on Windows, Linux, macOS |

---

## Timeline Estimate

- **Analysis & Design:** 1-2 hours
- **Implementation:** 3-4 hours
- **Testing & Validation:** 2-3 hours
- **Benchmarking:** 1-2 hours
- **Documentation:** 1 hour

**Total Estimated Time:** 8-12 hours

---

## Success Criteria

✅ All 527 tests passing  
✅ 20-35% performance improvement for embedding operations  
✅ No accuracy regressions (within 0.1% tolerance)  
✅ Clean build with no new warnings  
✅ Benchmarks documented  
✅ Updated documentation

---

## References

- [TensorPrimitives API Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.tensors.tensorprimitives)
- [.NET 10 Performance Improvements](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-10/)
- [SIMD in .NET](https://learn.microsoft.com/en-us/dotnet/standard/simd)
- [Vector Math Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/numerics)

---

**Document Version:** 1.0  
**Last Updated:** 2025-01-29  
**Author:** GitHub Copilot (Claude Sonnet 4.5)
