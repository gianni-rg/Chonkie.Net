# .NET 10 & C# 14 Implementation Guide

## Quick Reference: What Changed in .NET 10 & C# 14

### C# 14 Language Features Summary

| Feature | Use Case | Example | Priority |
|---------|----------|---------|----------|
| **Extension Members** | Properties & static members | `extension for IEnumerable<T>` | HIGH |
| **`field` Keyword** | Simplified properties | `set => field = value ?? throw ...` | MEDIUM |
| **Null-Conditional Assignment** | Cleaner null checks | `customer?.Order = order;` | HIGH |
| **Implicit Span Conversions** | Less allocation | `Method(array)` → `ReadOnlySpan<T>` | HIGH |
| **Partial Constructors** | Source generator support | `partial ctor` | LOW |
| **`nameof` Generic** | Unbound types | `nameof(List<>)` | LOW |
| **Lambda Modifiers** | Simpler syntax | `(text, out result) =>` | LOW |

### .NET 10 Runtime Improvements

| Optimization | Benefit | Automatic? | Action |
|--------------|---------|------------|--------|
| **Stack Allocation** | Small arrays → stack | ✅ Yes | Review array sizes |
| **Array Devirtualization** | IEnumerable faster | ✅ Yes | Benchmark |
| **Loop Inversion** | Better branching | ✅ Yes | None |
| **Improved Inlining** | Smaller code | Partial | Add `AggressiveInlining` |
| **TensorPrimitives** | SIMD operations | ❌ No | Rewrite pooling |

### .NET 10 Library Updates

| Library | Status | Benefit | Action |
|---------|--------|---------|--------|
| **System.Numerics.Tensors** | ✅ Stable | Better API | Consider migration |
| **Async ZIP APIs** | ✅ Available | Model packaging | Add to ModelManager |
| **JSON PipeReader** | ✅ Available | Streaming | Optional |
| **Numeric StringComparer** | ✅ Available | Model versioning | Add utility |

## Step-by-Step Migration Checklist

### 1️⃣ Project Setup (Day 1)

- [ ] Update `Directory.Build.props`:
  ```xml
  <PropertyGroup>
    <TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
    <LangVersion>14.0</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  ```

- [ ] Update package references:
  ```xml
  <PackageReference Include="Microsoft.ML.OnnxRuntime" Version="1.23.0" />
  <PackageReference Include="System.Numerics.Tensors" Version="10.0.0" />
  ```

- [ ] Install .NET 10 SDK: `winget install Microsoft.DotNet.SDK.10`

- [ ] Verify build: `dotnet build -c Release`

### 2️⃣ Apply C# 14 Features (Week 1)

#### Null-Conditional Assignment

**Files to update:**
- `SentenceTransformerEmbeddings.cs`
- `ModelConfig.cs`
- All validation code

**Pattern:**
```csharp
// Before
if (config is not null)
{
    config.MaxLength = maxLength;
}

// After
config?.MaxLength = maxLength;
```

**Script:**
```bash
# Find patterns to replace
git grep -n "if.*is not null" -- "*.cs"
```

#### Field Keyword

**Files to update:**
- Properties with backing fields
- Validated properties

**Pattern:**
```csharp
// Before
private int _dimension;
public int Dimension 
{ 
    get => _dimension;
    set => _dimension = value > 0 ? value : throw new ArgumentException();
}

// After
public int Dimension
{
    get;
    set => field = value > 0 ? value : throw new ArgumentException();
}
```

#### Implicit Span Conversions

**Files to update:**
- `PoolingUtilities.cs` (critical)
- `SentenceTransformerEmbeddings.cs`

**Pattern:**
```csharp
// Before
public void ProcessTokens(float[] embeddings)
{
    var span = embeddings.AsSpan();
    Process(span);
}

// After (cleaner)
public void ProcessTokens(ReadOnlySpan<float> embeddings)
{
    Process(embeddings); // Direct use
}

// Calling code - no change needed!
float[] data = GetData();
ProcessTokens(data); // Implicit conversion
```

### 3️⃣ Extension Members (Week 2)

**Create new file:** `src/Chonkie.Core/Extensions/TokenizerExtensions.cs`

```csharp
using Microsoft.ML.Tokenizers;

namespace Chonkie.Core.Extensions;

/// <summary>
/// Extension members for Tokenizer.
/// </summary>
public static extension TokenizerExtensions for Tokenizer
{
    /// <summary>
    /// Gets the maximum token length for this tokenizer.
    /// </summary>
    public int MaxTokenLength => this.Model.GetMaxLength();
    
    /// <summary>
    /// Default maximum sequence length for transformers.
    /// </summary>
    public static int DefaultMaxLength => 512;
    
    /// <summary>
    /// Encodes text with default special tokens.
    /// </summary>
    public EncodingResult EncodeWithDefaults(string text)
        => this.Encode(text, addSpecialTokens: true);
    
    /// <summary>
    /// Batch encodes texts with padding.
    /// </summary>
    public IEnumerable<EncodingResult> EncodeBatchWithPadding(
        IEnumerable<string> texts,
        int? maxLength = null)
    {
        var results = texts.Select(t => this.Encode(t, addSpecialTokens: true)).ToList();
        var max = maxLength ?? results.Max(r => r.Ids.Count);
        
        foreach (var result in results)
        {
            // Apply padding...
            yield return result;
        }
    }
}
```

**Usage:**
```csharp
var tokenizer = Tokenizer.FromFile(path);
var maxLen = Tokenizer.DefaultMaxLength; // Static extension property
var encoding = tokenizer.EncodeWithDefaults(text); // Instance extension method
```

### 4️⃣ Performance - TensorPrimitives (Week 3-4)

**File:** `src/Chonkie.Embeddings/SentenceTransformers/PoolingUtilities.cs`

**Before:**
```csharp
private static float[] MeanPooling(
    float[] tokenEmbeddings,
    int[] attentionMask,
    int batchSize,
    int seqLength,
    int hiddenDim)
{
    var result = new float[hiddenDim];
    var count = 0;
    
    for (int s = 0; s < seqLength; s++)
    {
        if (attentionMask[s] == 1)
        {
            for (int d = 0; d < hiddenDim; d++)
            {
                result[d] += tokenEmbeddings[s * hiddenDim + d];
            }
            count++;
        }
    }
    
    for (int d = 0; d < hiddenDim; d++)
    {
        result[d] /= Math.Max(count, 1);
    }
    
    return result;
}
```

**After (with TensorPrimitives):**
```csharp
using System.Numerics.Tensors;

private static float[] MeanPooling(
    ReadOnlySpan<float> tokenEmbeddings,
    ReadOnlySpan<int> attentionMask,
    int batchSize,
    int seqLength,
    int hiddenDim)
{
    var result = new float[hiddenDim];
    var resultSpan = result.AsSpan();
    var count = 0;
    
    for (int s = 0; s < seqLength; s++)
    {
        if (attentionMask[s] == 1)
        {
            var tokenSlice = tokenEmbeddings.Slice(s * hiddenDim, hiddenDim);
            
            // SIMD-optimized addition
            TensorPrimitives.Add(resultSpan, tokenSlice, resultSpan);
            count++;
        }
    }
    
    // SIMD-optimized division
    TensorPrimitives.Divide(resultSpan, (float)Math.Max(count, 1), resultSpan);
    
    return result;
}
```

**Benchmark:**
```csharp
[Benchmark]
public float[] MeanPooling_Original() => ...

[Benchmark]
public float[] MeanPooling_TensorPrimitives() => ...
```

### 5️⃣ Aggressive Inlining (Week 3)

**Pattern:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static float[] L2Normalize(ReadOnlySpan<float> vector)
{
    var result = new float[vector.Length];
    var resultSpan = result.AsSpan();
    
    // Calculate norm
    var norm = TensorPrimitives.Norm(vector);
    
    // Normalize
    TensorPrimitives.Divide(vector, norm, resultSpan);
    
    return result;
}
```

**Apply to:**
- Small, frequently called methods
- Methods called in tight loops
- Property getters with simple calculations

**Profile first:**
```bash
dotnet trace collect --process-id <PID>
PerfView analyze trace.nettrace
```

### 6️⃣ System.Numerics.Tensors Migration (Week 4-5)

**Create abstraction layer:**

```csharp
// src/Chonkie.Core/Tensors/ITensorFactory.cs
public interface ITensorFactory
{
    Tensor<T> Create<T>(T[] data, ReadOnlySpan<int> dimensions);
    Tensor<T> Create<T>(ReadOnlySpan<T> data, ReadOnlySpan<int> dimensions);
}

// Implementation for System.Numerics.Tensors
public class SystemTensorFactory : ITensorFactory
{
    public Tensor<T> Create<T>(T[] data, ReadOnlySpan<int> dimensions)
        => Tensor.Create(data, dimensions);
}

// Legacy implementation for Microsoft.ML.OnnxRuntime.Tensors
public class OnnxRuntimeTensorFactory : ITensorFactory
{
    public Tensor<T> Create<T>(T[] data, ReadOnlySpan<int> dimensions)
        => new DenseTensor<T>(data, dimensions.ToArray());
}
```

**Update code:**
```csharp
// Inject factory
public SentenceTransformerEmbeddings(
    string modelPath,
    ITensorFactory? tensorFactory = null)
{
    _tensorFactory = tensorFactory ?? new SystemTensorFactory();
}

// Use factory
private Tensor<long> CreateInputIdsTensor(int[][] inputIds)
{
    int batchSize = inputIds.Length;
    int seqLength = inputIds[0].Length;
    
    var flatData = FlattenArray(inputIds);
    return _tensorFactory.Create(flatData, [batchSize, seqLength]);
}
```

### 7️⃣ Testing & Validation (Ongoing)

**Unit Tests:**
```csharp
[Theory]
[InlineData("sentence-transformers/all-MiniLM-L6-v2")]
public async Task EmbedAsync_Net10_ProducesSameResults(string modelPath)
{
    // Arrange
    var net9Embeddings = new SentenceTransformerEmbeddings_Net9(modelPath);
    var net10Embeddings = new SentenceTransformerEmbeddings(modelPath);
    var text = "Test sentence for validation";
    
    // Act
    var net9Result = await net9Embeddings.EmbedAsync(text);
    var net10Result = await net10Embeddings.EmbedAsync(text);
    
    // Assert - cosine similarity should be > 0.999
    var similarity = CosineSimilarity(net9Result, net10Result);
    similarity.Should().BeGreaterThan(0.999);
}
```

**Performance Tests:**
```csharp
public class PerformanceRegressionTests
{
    [Fact]
    public async Task EmbedBatch_Net10_IsFaster()
    {
        var texts = Enumerable.Repeat("Test", 100).ToArray();
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        
        var sw = Stopwatch.StartNew();
        await embeddings.EmbedBatchAsync(texts);
        sw.Stop();
        
        // Net 10 should be at least 15% faster
        sw.ElapsedMilliseconds.Should().BeLessThan(baselineMs * 0.85);
    }
}
```

**Memory Tests:**
```csharp
[Fact]
public void StackAllocation_ReducesHeapPressure()
{
    var initialMemory = GC.GetTotalMemory(forceFullCollection: true);
    
    // Perform operations
    var embeddings = new SentenceTransformerEmbeddings(modelPath);
    var result = embeddings.EmbedAsync("Test").Result;
    
    var finalMemory = GC.GetTotalMemory(forceFullCollection: false);
    var allocated = finalMemory - initialMemory;
    
    // Should allocate less with stack allocation
    allocated.Should().BeLessThan(baselineAllocation);
}
```

## Performance Targets

| Metric | .NET 9 Baseline | .NET 10 Target | Status |
|--------|----------------|----------------|---------|
| Single embed | 50ms | 42ms (-15%) | 🔄 |
| Batch 100 | 800ms | 640ms (-20%) | 🔄 |
| Memory/embed | 2.5 KB | 2.0 KB (-20%) | 🔄 |
| Throughput | 100/s | 125/s (+25%) | 🔄 |

## Common Pitfalls

### ❌ Over-using AggressiveInlining
```csharp
// BAD - large method
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public async Task<float[]> ComplexProcessing() { /* 100+ lines */ }
```
✅ **Solution:** Only inline small, hot methods (< 30 lines).

### ❌ Breaking Span Lifetimes
```csharp
// BAD - returning span from local
public ReadOnlySpan<float> GetData()
{
    var data = new float[100];
    return data.AsSpan(); // Don't do this!
}
```
✅ **Solution:** Return arrays, let caller create spans.

### ❌ Premature Optimization
```csharp
// BAD - optimizing cold code
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private void LoadConfigOnce() { /* called once */ }
```
✅ **Solution:** Profile first, optimize hot paths only.

## Rollback Plan

If issues arise:

1. **Revert C# version:**
   ```xml
   <LangVersion>13.0</LangVersion>
   ```

2. **Disable .NET 10 target:**
   ```xml
   <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
   ```

3. **Keep abstraction layers** for easy feature toggles.

## Resources

- [C# 14 What's New](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [.NET 10 Runtime](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/runtime)
- [.NET 10 Libraries](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/libraries)
- [TensorPrimitives](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.tensors.tensorprimitives)
- [System.Numerics.Tensors](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.tensors)

## Next Steps

1. ✅ Review and approve enhancement plan
2. 🔄 Set up .NET 10 development environment
3. 🔄 Run baseline benchmarks (.NET 9)
4. 🔄 Begin Phase 1: Quick Wins (C# 14 features)
5. 🔄 Measure and document improvements
6. 🔄 Proceed to tensor migration (Phase 3)

---

**Questions?** See [DOTNET10_CSHARP14_ENHANCEMENT_PLAN.md](DOTNET10_CSHARP14_ENHANCEMENT_PLAN.md) for complete details.
