# .NET 10 & C# 14 Enhancement Plan for Chonkie.Net (Solution-Wide)

**Date:** December 16, 2025  
**Status:** 🔄 Planning - Leveraging Latest .NET 10 RTM & C# 14 Features  
**Scope:** All 9 Projects in Chonkie.Net Solution

## Executive Summary

With .NET 10 now RTM (Released To Manufacturing) and C# 14 available, this document outlines comprehensive opportunities to enhance the entire Chonkie.Net solution by leveraging new runtime optimizations, language features, and library improvements across all components.

## Current Implementation Status

✅ **Complete Implementation** (as of December 2025):
- **Phase 1-6 Complete**: Core functionality, chunkers, embeddings, pipeline, infrastructure
- All 9 projects fully functional
- Production-ready ONNX Sentence Transformers
- Comprehensive test coverage (unit, integration, performance)
- Pipeline system with CHOMP flow (Fetcher → Chef → Chunker → Refinery → Porter)
- **Target**: .NET 10 and C# 14 exclusive (no backward compatibility)

## Solution Architecture Overview

The Chonkie.Net solution consists of 9 projects organized into functional domains:

### **Core Infrastructure**
1. **Chonkie.Core** - Base interfaces, types, and abstractions
2. **Chonkie.Tokenizers** - Text tokenization (Character, Word, HuggingFace)

### **Processing Components**
3. **Chonkie.Chunkers** - 9 chunker implementations (Token, Sentence, Recursive, Code, Semantic, Late, Neural, Slumber, Table)
4. **Chonkie.Embeddings** - 10+ embedding providers (OpenAI, Azure, Cohere, Gemini, Jina, Voyage, ONNX, Model2Vec, Catsu, LiteLLM)
5. **Chonkie.Chefs** - Text preprocessing (TextChef, MarkdownChef, TableChef)

### **Pipeline & Utilities**
6. **Chonkie.Pipeline** - Fluent API for building processing workflows
7. **Chonkie.Fetchers** - Data ingestion (FileFetcher)
8. **Chonkie.Porters** - Export formats (JSON, Datasets)
9. **Chonkie.Refineries** - Post-processing (Overlap, Embeddings)

## .NET 10 & C# 14 Enhancement Opportunities by Project

### 1. 🎯 **Chonkie.Core** - Base Infrastructure

**Purpose:** Foundational types, interfaces, and abstractions used across all projects.

**Key Components:**
- `IChunker`, `IEmbeddings`, `ITokenizer` interfaces
- `Chunk`, `Document`, `RecursiveRules` types
- `BaseChunker` abstract class with parallel processing

#### C# 14 Enhancements

**1.1 Extension Members for Common Patterns (HIGH)**
```csharp
// Current: Static extension methods
public static class ChunkExtensions
{
    public static IEnumerable<Chunk> WithMetadata(this IEnumerable<Chunk> chunks, string key, string value)
    {
        // ...
    }
}

// C# 14: Extension members with properties
public static extension ChunkCollectionExtensions for IEnumerable<Chunk>
{
    public int TotalTokens => this.Sum(c => c.TokenCount);
    public int AverageTokens => this.TotalTokens / this.Count();
    
    public static IEnumerable<Chunk> Empty => [];
    
    public IEnumerable<Chunk> WithSource(string source)
        => this.Select(c => c with { Metadata = c.Metadata.Add("source", source) });
}

// Usage
var chunks = chunker.Chunk(text);
var totalTokens = chunks.TotalTokens; // Property access
var tagged = chunks.WithSource("doc1.txt"); // Fluent method
```

**1.2 Field Keyword for Validated Properties (MEDIUM)**
```csharp
// All BaseChunker properties with validation
public int ChunkSize
{
    get;
    set => field = value > 0 ? value : throw new ArgumentException("Chunk size must be positive");
}

public int ChunkOverlap
{
    get;
    set => field = value >= 0 && value < ChunkSize 
        ? value 
        : throw new ArgumentException("Overlap must be non-negative and less than chunk size");
}
```

**1.3 Null-Conditional Assignment for Optional Configuration (HIGH)**
```csharp
// Current: Null checks before assignments
if (document.Metadata != null)
{
    document.Metadata["processed"] = DateTime.UtcNow.ToString();
}

// C# 14: Direct assignment
document.Metadata?["processed"] = DateTime.UtcNow.ToString();
progress?.Report(0.5);
cancellationToken?.ThrowIfCancellationRequested();
```

#### .NET 10 Runtime Optimizations

**1.4 Array Devirtualization in Batch Processing (HIGH)**
```csharp
// BaseChunker parallel processing benefits from 10-20% faster IEnumerable iteration
protected virtual IReadOnlyList<IReadOnlyList<Chunk>> ParallelBatchProcessing(
    IReadOnlyList<string> texts,
    IProgress<double>? progress,
    CancellationToken cancellationToken)
{
    // .NET 10 automatically optimizes this enumeration
    var results = new ConcurrentBag<(int Index, IReadOnlyList<Chunk> Chunks)>();
    
    Parallel.ForEach(texts.Select((t, i) => (Text: t, Index: i)),
        new ParallelOptions { CancellationToken = cancellationToken },
        item =>
        {
            var chunks = Chunk(item.Text);
            results.Add((item.Index, chunks));
            progress?.Report((results.Count) / (double)texts.Count);
        });
    
    return results.OrderBy(r => r.Index).Select(r => r.Chunks).ToList();
}
```

**Affected Files:**
- `src/Chonkie.Core/Chunker/BaseChunker.cs`
- `src/Chonkie.Core/Types/*.cs`
- `src/Chonkie.Core/Interfaces/*.cs`
- `src/Chonkie.Core/Extensions/*.cs` (new)

---

### 2. 🪙 **Chonkie.Tokenizers** - Text Tokenization

**Purpose:** Tokenization abstractions and implementations (Character, Word, HuggingFace).

**Key Components:**
- `ITokenizer` interface
- `CharacterTokenizer`, `WordTokenizer`
- `AutoTokenizer` factory

#### C# 14 Enhancements

**2.1 Extension Members for Tokenizer Utilities (HIGH)**
```csharp
public static extension TokenizerExtensions for ITokenizer
{
    // Default token limits
    public static int DefaultMaxLength => 512;
    public static int DefaultChunkSize => 1024;
    
    // Common operations as properties
    public string Type => this.GetType().Name.Replace("Tokenizer", "");
    
    // Utility methods
    public int CountTokensInBatch(ReadOnlySpan<string> texts)
        => texts.ToArray().Sum(t => this.CountTokens(t));
    
    public static ITokenizer CreateDefault(string type = "character")
        => type.ToLowerInvariant() switch
        {
            "character" => new CharacterTokenizer(),
            "word" => new WordTokenizer(),
            _ => throw new ArgumentException($"Unknown tokenizer type: {type}")
        };
}
```

**2.2 Implicit Span Conversions for Token Processing (HIGH)**
```csharp
// Current: Explicit AsSpan() calls
public int CountTokens(string text)
{
    var span = text.AsSpan();
    return CountTokensInternal(span);
}

// C# 14: Direct span parameter acceptance
public int CountTokens(ReadOnlySpan<char> text)
{
    // Automatically accepts strings, char[], Memory<char>, etc.
    int count = 0;
    foreach (var c in text)
    {
        if (!char.IsWhiteSpace(c)) count++;
    }
    return count;
}

// Callers don't need AsSpan()
var tokens = tokenizer.CountTokens("Hello world");
var tokens2 = tokenizer.CountTokens(charArray); // Implicit conversion
```

**Affected Files:**
- `src/Chonkie.Tokenizers/CharacterTokenizer.cs`
- `src/Chonkie.Tokenizers/WordTokenizer.cs`
- `src/Chonkie.Tokenizers/AutoTokenizer.cs`

---

### 3. ✂️ **Chonkie.Chunkers** - Chunking Strategies

**Purpose:** 9 chunker implementations for different text splitting strategies.

**Key Components:**
- `TokenChunker` - Fixed-size token chunks
- `SentenceChunker` - Sentence-boundary aware
- `RecursiveChunker` - Hierarchical splitting
- `CodeChunker`, `SemanticChunker`, `LateChunker`, `NeuralChunker`, `SlumberChunker`, `TableChunker`

#### C# 14 Enhancements

**3.1 Extension Members for Chunker Patterns (HIGH)**
```csharp
public static extension ChunkerExtensions for IChunker
{
    // Common properties
    public string StrategyName => this.GetType().Name.Replace("Chunker", "");
    public bool SupportsParallelProcessing => this is BaseChunker { UseParallelProcessing: true };
    
    // Fluent configuration
    public IChunker WithParallelism(bool enabled)
    {
        if (this is BaseChunker bc)
        {
            bc.UseParallelProcessing = enabled;
        }
        return this;
    }
    
    // Batch processing shortcuts
    public async Task<IReadOnlyList<IReadOnlyList<Chunk>>> ChunkBatchAsync(
        IEnumerable<string> texts,
        CancellationToken ct = default)
        => await Task.Run(() => this.ChunkBatch(texts, null, ct), ct);
}
```

**3.2 Null-Conditional Assignment in RecursiveChunker (HIGH)**
```csharp
// Current: Nested null checks
if (chunk.Metadata != null)
{
    if (levelInfo != null)
    {
        chunk.Metadata["level"] = levelInfo.Level.ToString();
    }
}

// C# 14: Chained null-conditional
chunk.Metadata?["level"] = levelInfo?.Level.ToString();
chunk.Metadata?["depth"] = currentDepth.ToString();
progress?.Report(processedChunks / (double)totalChunks);
```

**3.3 Field Keyword for Chunker Configuration (MEDIUM)**
```csharp
public class RecursiveChunker : BaseChunker
{
    public int ChunkSize
    {
        get;
        set => field = value > 0 ? value : throw new ArgumentException("Chunk size must be positive");
    }
    
    public RecursiveRules Rules
    {
        get;
        set => field = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    public int MinCharactersPerChunk
    {
        get;
        set => field = value >= 0 ? value : throw new ArgumentException("Min characters must be non-negative");
    }
}
```

#### .NET 10 Runtime Optimizations

**3.4 Array Devirtualization for Batch Processing (AUTOMATIC)**
- All `ChunkBatch` methods automatically benefit from 10-20% faster iteration
- No code changes required - JIT optimization

**3.5 Loop Inversion for Text Processing (AUTOMATIC)**
- `RecursiveChunker` recursive algorithms optimized
- Better branch prediction in nested loops

**Affected Files:**
- `src/Chonkie.Chunkers/TokenChunker.cs`
- `src/Chonkie.Chunkers/SentenceChunker.cs`
- `src/Chonkie.Chunkers/RecursiveChunker.cs`
- `src/Chonkie.Chunkers/CodeChunker.cs`
- `src/Chonkie.Chunkers/SemanticChunker.cs`
- All other chunker implementations

---

### 4. 🧮 **Chonkie.Embeddings** - Vector Embeddings

**Purpose:** 10+ embedding provider implementations for generating text embeddings.

**Key Components:**
- `BaseEmbeddings` abstract class
- Provider implementations: OpenAI, Azure OpenAI, Cohere, Gemini, Jina, Voyage AI
- Local models: ONNX Sentence Transformers, Model2Vec
- Unified clients: Catsu, LiteLLM

#### C# 14 Enhancements

**4.1 Extension Members for Embeddings Utilities (HIGH)**
```csharp
public static extension EmbeddingsExtensions for IEmbeddings
{
    // Common properties
    public string ProviderName => this.GetType().Name.Replace("Embeddings", "");
    public bool IsLocal => this is SentenceTransformerEmbeddings or Model2VecEmbeddings;
    public bool SupportsStreaming => this is OpenAIEmbeddings or AzureOpenAIEmbeddings;
    
    // Batch size optimization
    public static int DefaultBatchSize => 32;
    public static int MaxBatchSize => 256;
    
    // Utility methods
    public async Task<float[][]> EmbedWithRetryAsync(
        IEnumerable<string> texts,
        int maxRetries = 3,
        CancellationToken ct = default)
    {
        // Retry logic for transient failures
    }
}
```

**4.2 Implicit Span Conversions for ONNX Processing (HIGH)**
```csharp
// Current: Explicit span conversions
public float[] ApplyPooling(float[] tokenEmbeddings, int[] attentionMask)
{
    var embeddingsSpan = tokenEmbeddings.AsSpan();
    var maskSpan = attentionMask.AsSpan();
    // Process...
}

// C# 14: Direct span parameters
public float[] ApplyPooling(ReadOnlySpan<float> tokenEmbeddings, ReadOnlySpan<int> attentionMask)
{
    // Automatically accepts arrays, Memory<T>, etc.
    // Better performance, no explicit conversions needed
}
```

**4.3 Null-Conditional Assignment for Optional Configs (MEDIUM)**
```csharp
// Simplified configuration updates
config?.MaxLength = maxLength;
config?.Normalize = normalize;
poolingConfig?.WordEmbeddingDimension = dimension;
cache?.RefCount += 1;
metrics?.EmbeddingCount += embeddings.Length;
```

#### .NET 10 Runtime Optimizations

**4.4 System.Numerics.Tensors Stabilization (CRITICAL - HIGH IMPACT)**

```csharp
// Current: Custom SIMD operations or slow scalar code
public static float[] Normalize(float[] embeddings)
{
    float norm = 0;
    for (int i = 0; i < embeddings.Length; i++)
    {
        norm += embeddings[i] * embeddings[i];
    }
    norm = MathF.Sqrt(norm);
    
    var result = new float[embeddings.Length];
    for (int i = 0; i < embeddings.Length; i++)
    {
        result[i] = embeddings[i] / norm;
    }
    return result;
}

// .NET 10: Use stable System.Numerics.Tensors with TensorPrimitives
using System.Numerics.Tensors;

public static float[] Normalize(ReadOnlySpan<float> embeddings)
{
    // Now stable API (no longer experimental!)
    var norm = TensorPrimitives.Norm(embeddings);
    
    var result = new float[embeddings.Length];
    TensorPrimitives.Divide(embeddings, norm, result);
    return result;
}

// TensorPrimitives expanded from 40 to ~200 operations
public static float CosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
{
    // SIMD-optimized operations
    var dotProduct = TensorPrimitives.Dot(a, b);
    var normA = TensorPrimitives.Norm(a);
    var normB = TensorPrimitives.Norm(b);
    return dotProduct / (normA * normB);
}
```

**4.5 Stack Allocation for Small Buffers (AUTOMATIC)**
- Token ID arrays < 100 elements automatically stack-allocated
- Attention masks stack-allocated for short sequences
- Pooling intermediates optimized

**4.6 Array Devirtualization for Batch Embeddings (AUTOMATIC)**
- `EmbedBatchAsync` iterations 10-20% faster
- No code changes required

**Affected Files:**
- `src/Chonkie.Embeddings/Base/BaseEmbeddings.cs`
- `src/Chonkie.Embeddings/SentenceTransformers/*` (all ONNX files)
- `src/Chonkie.Embeddings/OpenAI/OpenAIEmbeddings.cs`
- `src/Chonkie.Embeddings/Azure/AzureOpenAIEmbeddings.cs`
- `src/Chonkie.Embeddings/Cohere/CohereEmbeddings.cs`
- All embedding provider implementations

---

### 5. 👨‍🍳 **Chonkie.Chefs** - Text Preprocessing

**Purpose:** Text preprocessing and normalization before chunking.

**Key Components:**
- `TextChef` - Plain text cleaning
- `MarkdownChef` - Markdown parsing
- `TableChef` - Table extraction

#### C# 14 Enhancements

**5.1 Extension Members for Chef Utilities (MEDIUM)**
```csharp
public static extension ChefExtensions for IChef
{
    public string ChefType => this.GetType().Name.Replace("Chef", "");
    
    public async Task<string[]> ProcessBatchAsync(
        IEnumerable<string> texts,
        CancellationToken ct = default)
    {
        var tasks = texts.Select(t => this.ProcessAsync(t, ct));
        return await Task.WhenAll(tasks);
    }
}
```

**5.2 Implicit Span Conversions for Text Processing (HIGH)**
```csharp
// Process text as spans to avoid allocations
public string CleanText(ReadOnlySpan<char> text)
{
    // Operates on span, returns cleaned string
    // Accepts string, char[], Memory<char> implicitly
}
```

**Affected Files:**
- `src/Chonkie.Chefs/TextChef.cs`
- `src/Chonkie.Chefs/MarkdownChef.cs`
- `src/Chonkie.Chefs/TableChef.cs`

---

### 6. 🔄 **Chonkie.Pipeline** - Workflow Orchestration

**Purpose:** Fluent API for building complete text processing workflows.

**Key Components:**
- `Pipeline` class with CHOMP flow
- `ComponentRegistry` for component discovery
- Reflection-based component instantiation

#### C# 14 Enhancements

**6.1 Extension Members for Pipeline Builders (HIGH)**
```csharp
public static extension PipelineExtensions for Pipeline
{
    // Common pipeline templates
    public static Pipeline TextProcessingPipeline(int chunkSize = 512)
        => new Pipeline()
            .ProcessWith("text")
            .ChunkWith("recursive", new { chunk_size = chunkSize });
    
    public static Pipeline CodePipeline(string language, int chunkSize = 1024)
        => new Pipeline()
            .ProcessWith("text")
            .ChunkWith("code", new { chunk_size = chunkSize, language });
    
    public static Pipeline EmbeddingPipeline(string model, int chunkSize = 512)
        => new Pipeline()
            .ProcessWith("text")
            .ChunkWith("recursive", new { chunk_size = chunkSize })
            .RefineWith("embeddings", new { model });
}

// Usage
var pipeline = Pipeline.TextProcessingPipeline(1024);
```

**6.2 Null-Conditional Assignment for Optional Steps (HIGH)**
```csharp
// Simplified step configuration
step.Parameters?["chunk_size"] = chunkSize;
step.Component?["model"] = modelName;
progress?.Report(completedSteps / (double)totalSteps);
```

#### .NET 10 Runtime Optimizations

**6.3 Array Devirtualization for Step Processing (AUTOMATIC)**
- Pipeline step iteration optimized automatically
- Faster execution of multi-step workflows

**Affected Files:**
- `src/Chonkie.Pipeline/Pipeline.cs`
- `src/Chonkie.Pipeline/ComponentRegistry.cs`
- `src/Chonkie.Pipeline/PipelineStep.cs`

---

### 7. 📁 **Chonkie.Fetchers** - Data Ingestion

**Purpose:** Load data from various sources.

**Key Components:**
- `FileFetcher` - Load from files/directories

#### C# 14 Enhancements

**7.1 Extension Members for Fetcher Utilities (MEDIUM)**
```csharp
public static extension FetcherExtensions for IFetcher
{
    public async Task<IReadOnlyList<(string Path, string Content)>> FetchAllAsync(
        params string[] paths)
    {
        var tasks = paths.Select(p => this.FetchAsync(p));
        var results = await Task.WhenAll(tasks);
        return results.SelectMany(r => r).ToList();
    }
}
```

**Affected Files:**
- `src/Chonkie.Fetchers/FileFetcher.cs`

---

### 8. 📦 **Chonkie.Porters** - Export Formats

**Purpose:** Export chunks to various formats.

**Key Components:**
- `JsonPorter` - JSON export
- `DatasetsPorter` - HuggingFace Datasets format (future)

#### C# 14 Enhancements

**8.1 Extension Members for Porter Utilities (MEDIUM)**
```csharp
public static extension PorterExtensions for IPorter
{
    public string OutputFormat => this.GetType().Name.Replace("Porter", "").ToLowerInvariant();
    
    public async Task<bool> ExportWithCompressionAsync(
        IReadOnlyList<Chunk> chunks,
        string destination,
        CompressionLevel level = CompressionLevel.Optimal,
        CancellationToken ct = default)
    {
        // Compressed export
    }
}
```

**Affected Files:**
- `src/Chonkie.Porters/JsonPorter.cs`

---

### 9. 🔧 **Chonkie.Refineries** - Post-Processing

**Purpose:** Refine chunks after initial chunking.

**Key Components:**
- `OverlapRefinery` - Add context overlap
- `EmbeddingsRefinery` - Add embeddings to chunks

#### C# 14 Enhancements

**9.1 Extension Members for Refinery Patterns (HIGH)**
```csharp
public static extension RefineryExtensions for IRefinery
{
    public string RefineryType => this.GetType().Name.Replace("Refinery", "");
    
    public async Task<IReadOnlyList<Chunk>> RefineInBatchesAsync(
        IReadOnlyList<Chunk> chunks,
        int batchSize = 100,
        CancellationToken ct = default)
    {
        // Process in batches for large chunk lists
        var results = new List<Chunk>();
        for (int i = 0; i < chunks.Count; i += batchSize)
        {
            var batch = chunks.Skip(i).Take(batchSize).ToList();
            var refined = await this.RefineAsync(batch, ct);
            results.AddRange(refined);
        }
        return results;
    }
}
```

**9.2 Null-Conditional Assignment for Context Addition (HIGH)**
```csharp
// OverlapRefinery: Simplified context assignment
chunk.Metadata?["prefix_context"] = prefixContext;
chunk.Metadata?["suffix_context"] = suffixContext;
chunk.Metadata?["overlap_method"] = method.ToString();
```

**Affected Files:**
- `src/Chonkie.Refineries/OverlapRefinery.cs`
- `src/Chonkie.Refineries/EmbeddingsRefinery.cs`

---

## Cross-Cutting Enhancements

### Common Patterns Across All Projects

#### Pattern 1: Extension Member Naming Convention
```csharp
// Standard extension member pattern for all interfaces
public static extension [InterfaceName]Extensions for I[InterfaceName]
{
    // Properties for metadata
    public string TypeName => this.GetType().Name;
    
    // Static factory methods
    public static I[InterfaceName] CreateDefault(...) => ...;
    
    // Async convenience methods
    public async Task<T> [Method]Async(...) => ...;
}
```

#### Pattern 2: Field Keyword for All Validated Properties
```csharp
// Consistent validation pattern
public int PropertyName
{
    get;
    set => field = ValidateValue(value) 
        ? value 
        : throw new ArgumentException("Validation message");
}
```

#### Pattern 3: Implicit Span Conversions for Text Processing
```csharp
// All text-processing methods accept spans
public Result ProcessText(ReadOnlySpan<char> text) { ... }
public int CountTokens(ReadOnlySpan<char> text) { ... }
public float[] Embed(ReadOnlySpan<char> text) { ... }
```

---

## .NET 10 Automatic Optimizations

### System-Wide Performance Improvements (No Code Changes Required)

**1. Stack Allocation**
- Small arrays (< 100 elements) automatically stack-allocated
- Applies to: token arrays, embedding buffers, chunk metadata arrays
- **Expected Impact:** 5-10% reduction in GC pressure

**2. Array Devirtualization**
- All `IEnumerable<T>`, `IReadOnlyList<T>` iterations optimized
- Applies to: batch processing in all projects
- **Expected Impact:** 10-20% faster iteration

**3. Loop Inversion**
- Better branch prediction in nested loops
- Applies to: RecursiveChunker, text splitting algorithms
- **Expected Impact:** 5-15% faster recursive operations

**4. Arm64 Write-Barrier Improvements**
- 8-20% faster GC pauses on ARM processors
- Applies to: all async operations, especially embeddings
- **Expected Impact:** Better responsiveness on ARM-based systems

---

## System.Numerics.Tensors Migration (High Priority)

### Critical Opportunity: Stable Tensor APIs

**.NET 10 Milestone:** `System.Numerics.Tensors` is now **STABLE** (no longer experimental)

**Current State:**
- Custom SIMD operations scattered across codebase
- Scalar fallbacks for non-SIMD operations
- Manual vector optimizations

**Target State:**
- Unified tensor API with ~200 SIMD-optimized operations
- `TensorPrimitives` for high-performance math
- `IReadOnlyTensor` interface for better abstractions

### Migration Opportunities

**Embeddings Similarity Calculations:**
```csharp
// Before
public float CosineSimilarity(float[] u, float[] v)
{
    float dot = 0, normU = 0, normV = 0;
    for (int i = 0; i < u.Length; i++)
    {
        dot += u[i] * v[i];
        normU += u[i] * u[i];
        normV += v[i] * v[i];
    }
    return dot / (MathF.Sqrt(normU) * MathF.Sqrt(normV));
}

// After (.NET 10)
using System.Numerics.Tensors;

public float CosineSimilarity(ReadOnlySpan<float> u, ReadOnlySpan<float> v)
{
    return TensorPrimitives.CosineSimilarity(u, v); // Single SIMD call!
}
```

**Pooling Operations:**
```csharp
// Before: Custom mean pooling
public float[] MeanPooling(float[] embeddings, int seqLength, int hiddenDim)
{
    var result = new float[hiddenDim];
    for (int i = 0; i < hiddenDim; i++)
    {
        float sum = 0;
        for (int j = 0; j < seqLength; j++)
        {
            sum += embeddings[j * hiddenDim + i];
        }
        result[i] = sum / seqLength;
    }
    return result;
}

// After: TensorPrimitives operations
public float[] MeanPooling(ReadOnlySpan<float> embeddings, int seqLength, int hiddenDim)
{
    var result = new float[hiddenDim];
    var resultSpan = result.AsSpan();
    
    for (int i = 0; i < hiddenDim; i++)
    {
        var column = embeddings.Slice(i, seqLength * hiddenDim);
        resultSpan[i] = TensorPrimitives.Sum(column) / seqLength;
    }
    return result;
}
```

**Expected Performance Improvements:**
- Similarity calculations: **2-4x faster**
- Normalization operations: **3-5x faster**
- Pooling operations: **2-3x faster**
- Overall embeddings throughput: **15-30% improvement**

---

## Implementation Roadmap

### Phase 1: Foundation (Weeks 1-2)
**Goal:** Establish C# 14 patterns and update core infrastructure

**Tasks:**
1. **Chonkie.Core Updates**
   - Add extension members for `IChunker`, `IEmbeddings`, `ITokenizer`
   - Convert validated properties to use `field` keyword
   - Replace null-check-then-assign patterns with null-conditional assignment
   - Add comprehensive XML documentation for new extension members

2. **Project File Updates**
   - Update all `.csproj` to `<LangVersion>14</LangVersion>` (required)
   - Update all `.csproj` to `<TargetFramework>net10.0</TargetFramework>` (single target only)
   - Update solution-level `Directory.Build.props`
   - Remove any multi-targeting configurations
   - Set minimum SDK version to .NET 10

3. **Testing Infrastructure**
   - Verify all tests pass on .NET 10
   - Add performance benchmarks for automatic optimizations
   - Create baseline measurements

**Deliverables:**
- Updated core interfaces with extension members
- All projects targeting .NET 10 & C# 14
- Baseline performance metrics

---

### Phase 2: Chunkers & Tokenizers (Weeks 3-4)
**Goal:** Modernize text processing components

**Tasks:**
1. **Chonkie.Tokenizers**
   - Add extension members for tokenizer utilities
   - Convert text processing methods to accept `ReadOnlySpan<char>`
   - Update documentation and samples

2. **Chonkie.Chunkers**
   - Add extension members for all chunker types
   - Apply null-conditional assignment throughout
   - Convert `RecursiveChunker` properties to use `field` keyword
   - Update `CodeChunker` text splitting to use spans

3. **Performance Testing**
   - Benchmark array devirtualization improvements
   - Measure stack allocation benefits
   - Document observed performance gains

**Deliverables:**
- Modernized tokenizers and chunkers
- Performance comparison report
- Updated samples and documentation

---

### Phase 3: Embeddings & TensorPrimitives (Weeks 5-6)
**Goal:** Leverage System.Numerics.Tensors for high-performance embeddings

**Tasks:**
1. **System.Numerics.Tensors Integration**
   - Migrate similarity calculations to `TensorPrimitives`
   - Update pooling operations to use tensor APIs
   - Replace custom SIMD with `TensorPrimitives` operations
   - Add `IReadOnlyTensor` support where applicable

2. **Chonkie.Embeddings Updates**
   - Add extension members for embedding providers
   - Convert ONNX operations to use `ReadOnlySpan<float>`
   - Apply implicit span conversions
   - Update all provider implementations

3. **Comprehensive Benchmarking**
   - ONNX embeddings performance
   - Similarity calculations
   - Batch processing throughput
   - Memory usage and GC pressure

**Deliverables:**
- Fully migrated embeddings to TensorPrimitives
- Performance improvement documentation (target: 15-30%)
- Updated ONNX sample with new APIs

---

### Phase 4: Pipeline & Infrastructure (Weeks 7-8)
**Goal:** Modernize pipeline system and supporting components

**Tasks:**
1. **Chonkie.Pipeline**
   - Add extension members for pipeline templates
   - Simplify step configuration with null-conditional assignment
   - Update documentation with new patterns

2. **Chonkie.Chefs**
   - Add extension members
   - Convert to span-based text processing
   - Update preprocessing algorithms

3. **Chonkie.Fetchers & Porters**
   - Add extension members for utilities
   - Modernize async patterns
   - Update export formats

4. **Chonkie.Refineries**
   - Add extension members
   - Update `OverlapRefinery` with null-conditional assignment
   - Optimize batch processing

**Deliverables:**
- Modernized pipeline system
- Updated infrastructure components
- End-to-end pipeline samples

---

### Phase 5: Testing & Documentation (Weeks 9-10)
**Goal:** Comprehensive validation and documentation

**Tasks:**
1. **Testing**
   - Full regression testing on .NET 10
   - Performance benchmark suite
   - Integration tests for all patterns
   - Cross-platform validation (Windows, Linux, macOS)

2. **Documentation**
   - Update all API documentation
   - Create migration guide for users
   - Document performance improvements
   - Update samples and tutorials

3. **Code Quality**
   - Code review for C# 14 patterns
   - Static analysis with latest analyzers
   - Security review
   - Performance profiling

**Deliverables:**
- Complete test coverage on .NET 10
- Comprehensive documentation
- Performance comparison report (baseline vs .NET 10)
- API documentation with C# 14 patterns

---

## Success Criteria

### Performance Targets
- **Overall Solution Performance:** 15-25% improvement over .NET 9
- **Embeddings Throughput:** 20-35% improvement (TensorPrimitives)
- **Batch Processing:** 10-20% improvement (devirtualization)
- **Memory Usage:** 5-10% reduction (stack allocation)
- **GC Pressure:** 10-15% reduction (automatic optimizations)

### Code Quality Targets
- **C# 14 Adoption:** 90%+ of eligible code uses new features
- **Extension Members:** All major interfaces have extension member support
- **Span Usage:** 80%+ of text processing uses `ReadOnlySpan<char>`
- **Null Safety:** 100% null-conditional assignment where applicable
- **Test Coverage:** Maintain 85%+ line coverage

### API Modernization
- All public APIs leverage C# 14 patterns
- Modern, idiomatic C# 14 code throughout
- Clean break from older patterns
- Comprehensive XML documentation

---

## Risk Mitigation

### Technical Risks

**Risk 1: Performance Regressions**
- **Mitigation:** Continuous benchmarking, performance gates in CI/CD
- **Fallback:** Revert specific optimizations if needed
- **Impact:** Low - .NET 10 optimizations are well-tested

**Risk 2: C# 14 Feature Adoption**
- **Mitigation:** Use stable C# 14 features only, comprehensive IDE testing
- **Fallback:** None needed - all features are RTM
- **Impact:** Very Low - language features are stable

**Risk 3: TensorPrimitives Stability**
- **Mitigation:** System.Numerics.Tensors is now stable (no longer experimental)
- **Fallback:** None needed - stable API
- **Impact:** Very Low - production-ready API

### Process Risks

**Risk 1: Timeline Overruns**
- **Mitigation:** Phased approach, prioritize high-impact changes
- **Fallback:** Defer low-priority enhancements

**Risk 2: Testing Coverage**
- **Mitigation:** Automated testing, continuous integration
- **Fallback:** Extended testing phase

---

## Long-Term Maintenance

### Version Support Strategy
- **NET 10 LTS:** Exclusive support through November 2027
- **C# 14 Features:** Full adoption from day one
- **Future Upgrades:** Direct migration to .NET 11 when available (no multi-targeting)
- **Minimum Requirements:** .NET 10 SDK, C# 14 compiler

### Monitoring & Feedback
- Track performance metrics in production
- Monitor GitHub issues for user feedback
- Continuous benchmarking in CI/CD
- Regular performance audits (quarterly)
- Stay current with .NET 10 servicing updates

---

## Appendix: Quick Reference

### C# 14 Feature Adoption Priority

| Feature | Priority | Complexity | Impact | Projects Affected |
|---------|----------|------------|--------|-------------------|
| Extension Members | HIGH | Medium | High | All 9 projects |
| Implicit Span Conversions | HIGH | Low | High | Core, Tokenizers, Chunkers, Embeddings, Chefs |
| Null-Conditional Assignment | HIGH | Low | Medium | All 9 projects |
| Field Keyword | MEDIUM | Low | Low | All projects with validated properties |
| Partial Constructors | LOW | Low | Low | Future use with source generators |

### .NET 10 Performance Improvements

| Optimization | Type | Impact | Projects Affected |
|--------------|------|--------|-------------------|
| Stack Allocation | Automatic | 5-10% GC reduction | All projects |
| Array Devirtualization | Automatic | 10-20% faster iteration | Core, Chunkers, Embeddings, Pipeline |
| Loop Inversion | Automatic | 5-15% faster recursion | Chunkers (Recursive) |
| TensorPrimitives | Manual | 15-30% embeddings | Embeddings |
| Arm64 Write-Barrier | Automatic | 8-20% GC pauses | All projects |

### File Change Estimates

| Project | Files to Modify | Estimated LOC Changes | Complexity |
|---------|----------------|----------------------|------------|
| Chonkie.Core | 15 | 500 | Medium |
| Chonkie.Tokenizers | 5 | 200 | Low |
| Chonkie.Chunkers | 12 | 600 | Medium |
| Chonkie.Embeddings | 18 | 1200 | High |
| Chonkie.Chefs | 4 | 150 | Low |
| Chonkie.Pipeline | 8 | 400 | Medium |
| Chonkie.Fetchers | 2 | 80 | Low |
| Chonkie.Porters | 2 | 100 | Low |
| Chonkie.Refineries | 3 | 200 | Low |
| **Total** | **69** | **3430** | **Medium** |

---

## Conclusion

Migrating Chonkie.Net to .NET 10 and C# 14 presents significant opportunities for performance improvements, code modernization, and enhanced developer experience across all 9 projects. The phased approach ensures systematic implementation with continuous validation.

**Key Highlights:**
- **Performance:** 15-25% overall improvement expected
- **Code Quality:** Modern C# 14 patterns throughout
- **API Design:** Extension members for better discoverability
- **High-Performance Computing:** TensorPrimitives for embeddings (20-35% improvement)
- **Maintainability:** Cleaner code with less boilerplate
- **Future-Proof:** Built on stable .NET 10 LTS foundation

**Timeline:** 10 weeks for full solution migration  
**Effort Estimate:** ~3500 LOC changes across 69 files  
**Risk Level:** Low (stable platform, RTM features, comprehensive testing)  
**Breaking Changes:** YES - requires .NET 10, no backward compatibility

**Next Steps:**
1. Review and approve enhancement plan
2. Set up .NET 10 exclusive development environment
3. Update all project files to .NET 10 single-target
4. Begin Phase 1: Foundation updates
5. Establish performance baselines on .NET 10
6. Execute phased implementation

---

**Document Version:** 2.0 (Solution-Wide)  
**Last Updated:** December 16, 2025  
**Status:** Ready for Review & Approval
}
```

**Impact:** Automatic 10-20% performance improvement for array enumeration.

**Action:** No code changes needed - verify performance improvements with benchmarks.

#### 2.3 Improved Loop Optimizations

**Current Implementation:**
```csharp
// Traditional loop patterns
for (int i = 0; i < tokens.Length; i++)
{
    result[i] = ProcessToken(tokens[i]);
}

// While loops
int idx = 0;
while (idx < sequence.Length && sequence[idx] != padToken)
{
    Process(sequence[idx]);
    idx++;
}
```

**.NET 10 Benefit:**
- Automatic loop inversion optimization
- Better inlining of loop body methods
- Improved branch prediction

**Impact:** 5-15% performance improvement in tight loops.

**Action:** Verify with benchmarks, no code changes needed.

#### 2.4 Enhanced Inlining with Profile-Guided Optimization

**Current Implementation:**
```csharp
private float[] ApplyNormalization(float[] embeddings)
{
    return L2Normalize(embeddings);
}

private float[] L2Normalize(float[] vector)
{
    // Normalization logic
}
```

**.NET 10 Enhancement:**
```csharp
// Same code benefits from improved inlining heuristics
// Consider marking hot paths with AggressiveInlining
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private float[] L2Normalize(float[] vector)
{
    // Normalization logic
}
```

**Impact:** Better inlining of small, frequently called methods.

**Action:** Profile hot paths and apply AggressiveInlining attribute strategically.

### 3. 📚 .NET 10 Library Enhancements

#### 3.1 System.Numerics.Tensors Improvements (CRITICAL)

**Current Implementation:**
```csharp
// Using Microsoft.ML.OnnxRuntime.Tensors
var tensor = new DenseTensor<long>(new[] { batchSize, seqLength });
for (int b = 0; b < batchSize; b++)
{
    for (int s = 0; s < seqLength; s++)
    {
        tensor[b, s] = inputIds[b][s];
    }
}
```

**.NET 10 Enhancement:**
```csharp
// System.Numerics.Tensors is now STABLE (no longer experimental)
using System.Numerics.Tensors;

// Better API with ReadOnlyTensor interface
var tensor = Tensor.Create<long>(inputIds, [batchSize, seqLength]);

// Slice operations without copying
var batchSlice = tensor[0..10, ..]; // First 10 sequences

// Direct arithmetic operations with C# 14 extension operators
var normalized = tensor / tensor.Max(); // If T supports division
```

**Impact:** Better tensor API, improved performance, more intuitive code.

**Affected Files:**
- `src/Chonkie.Embeddings/SentenceTransformers/SentenceTransformerEmbeddings.cs` (tensor creation)
- `src/Chonkie.Embeddings/SentenceTransformers/PoolingUtilities.cs` (tensor operations)

**Action Items:**
1. Evaluate migration from `Microsoft.ML.OnnxRuntime.Tensors` to `System.Numerics.Tensors`
2. Benchmark performance differences
3. Update tensor creation and manipulation code
4. Leverage new `IReadOnlyTensor` interface for API design

#### 3.2 Async ZIP APIs (MEDIUM PRIORITY)

**Current Implementation:**
```csharp
// No current ZIP usage, but potential for model packaging
```

**Future Enhancement:**
```csharp
// Async model download and extraction
public async Task<string> DownloadModelAsync(string modelName, CancellationToken ct)
{
    var zipPath = await DownloadZipAsync(modelName, ct);
    var extractPath = GetCachePath(modelName);
    
    // Use new async ZIP APIs (.NET 10)
    await ZipFile.ExtractToDirectoryAsync(zipPath, extractPath, ct);
    
    return extractPath;
}
```

**Impact:** Better async I/O for model management.

**Affected Files:**
- `src/Chonkie.Embeddings/SentenceTransformers/ModelManager.cs`

#### 3.3 PipeReader Support for JSON (LOW PRIORITY)

**Current Implementation:**
```csharp
// Loading config files
var configJson = File.ReadAllText(configPath);
var config = JsonSerializer.Deserialize<ModelConfig>(configJson);
```

**.NET 10 Option:**
```csharp
// For streaming large configs (rarely needed for our models)
var pipe = PipeReader.Create(File.OpenRead(configPath));
var config = await JsonSerializer.DeserializeAsync<ModelConfig>(pipe);
```

**Impact:** Minimal - our config files are small. Consider for future large-model scenarios.

#### 3.4 Improved String APIs (MEDIUM PRIORITY)

**New in .NET 10:**
- UTF-8 hex conversion: `Convert.FromHexString(ReadOnlySpan<byte>)`
- String normalization for spans
- Numeric ordering: `StringComparer.Create(..., CompareOptions.NumericOrdering)`

**Potential Application:**
```csharp
// Better token ID handling from hex
public static int[] TokenIdsFromHex(ReadOnlySpan<byte> hexBytes)
{
    var bytes = Convert.FromHexString(hexBytes);
    return ParseTokenIds(bytes);
}

// Numeric sorting for model versions
var models = new[] { "model-v2", "model-v10", "model-v1" };
var sorted = models.Order(StringComparer.Create(
    CultureInfo.InvariantCulture, 
    CompareOptions.NumericOrdering
));
// Result: model-v1, model-v2, model-v10 (correct numeric order)
```

### 4. 🎯 ONNX Runtime & ML Improvements

#### 4.1 Latest ONNX Runtime (v1.23+)

**.NET 10 includes ONNX Runtime 1.23** with improvements:
- Better execution providers
- Improved graph optimizations
- Enhanced type support

**Action Items:**
1. Update `Microsoft.ML.OnnxRuntime` to latest compatible version
2. Test execution provider configurations
3. Benchmark performance improvements

#### 4.2 TensorPrimitives Expansion

**.NET 10 TensorPrimitives:**
- Expanded from 40 to ~200 operations
- SIMD-optimized operations
- Generic overloads for any `T`

**Current Implementation:**
```csharp
// Manual pooling calculations
private float[] MeanPooling(float[] embeddings, int[] mask, int seqLen, int dim)
{
    var result = new float[dim];
    for (int d = 0; d < dim; d++)
    {
        float sum = 0;
        int count = 0;
        for (int s = 0; s < seqLen; s++)
        {
            if (mask[s] == 1)
            {
                sum += embeddings[s * dim + d];
                count++;
            }
        }
        result[d] = count > 0 ? sum / count : 0;
    }
    return result;
}
```

**Enhanced with TensorPrimitives:**
```csharp
using System.Numerics.Tensors;

private float[] MeanPooling(ReadOnlySpan<float> embeddings, ReadOnlySpan<int> mask, int seqLen, int dim)
{
    var result = new float[dim];
    var resultSpan = result.AsSpan();
    
    // Use SIMD-optimized operations
    for (int s = 0; s < seqLen; s++)
    {
        if (mask[s] == 1)
        {
            var tokenEmbedding = embeddings.Slice(s * dim, dim);
            TensorPrimitives.Add(resultSpan, tokenEmbedding, resultSpan);
        }
    }
    
    // Normalize by count
    var maskSum = TensorPrimitives.Sum(mask);
    TensorPrimitives.Divide(resultSpan, maskSum, resultSpan);
    
    return result;
}
```

**Impact:** 2-5x performance improvement for tensor operations.

**Affected Files:**
- `src/Chonkie.Embeddings/SentenceTransformers/PoolingUtilities.cs` (critical)

## Implementation Phases

### Phase 1: Quick Wins (1-2 weeks)
- ✅ Update to .NET 10 SDK and runtime
- ✅ Enable C# 14 language features
- 🔄 Apply null-conditional assignment throughout codebase
- 🔄 Replace explicit `.AsSpan()` calls with implicit conversions
- 🔄 Add `field` keyword to validated properties
- 🔄 Update package references to .NET 10 compatible versions

### Phase 2: Performance Optimizations (2-3 weeks)
- 🔄 Benchmark current performance baseline
- 🔄 Profile hot paths with .NET 10 runtime
- 🔄 Apply `AggressiveInlining` to critical methods
- 🔄 Identify small arrays for stack allocation
- 🔄 Verify devirtualization benefits
- 🔄 Re-benchmark and document improvements

### Phase 3: Tensor API Migration (3-4 weeks)
- 🔄 Evaluate `System.Numerics.Tensors` vs `Microsoft.ML.OnnxRuntime.Tensors`
- 🔄 Create compatibility layer if needed
- 🔄 Migrate tensor creation code
- 🔄 Leverage `TensorPrimitives` for operations
- 🔄 Update pooling utilities with SIMD operations
- 🔄 Comprehensive testing and benchmarking

### Phase 4: API Enhancements (2-3 weeks)
- 🔄 Design extension member APIs
- 🔄 Implement extension properties for common patterns
- 🔄 Update documentation and samples
- 🔄 Add C# 14 feature examples

### Phase 5: Advanced Features (Future)
- 🔄 Async model management with ZIP APIs
- 🔄 Streaming config loading with PipeReader
- 🔄 Enhanced string utilities
- 🔄 Model versioning with numeric ordering

## Compatibility & Breaking Changes

### Backwards Compatibility
- ✅ Maintain compatibility with .NET 8/9 for current consumers
- ✅ Use multi-targeting if needed: `<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>`
- ✅ Document .NET 10-specific optimizations

### Breaking Changes
- ⚠️ None expected - enhancements are mostly internal
- ⚠️ API changes via extension members are additive
- ⚠️ Tensor API migration may require major version bump

## Testing Strategy

### Performance Testing
```csharp
[Benchmark]
[Arguments("sentence-transformers/all-MiniLM-L6-v2")]
public async Task<float[]> EmbedSingle_Net10(string modelPath)
{
    var embeddings = new SentenceTransformerEmbeddings(modelPath);
    return await embeddings.EmbedAsync("Test sentence for benchmarking");
}

[Benchmark]
[Arguments("sentence-transformers/all-MiniLM-L6-v2", 100)]
public async Task<List<float[]>> EmbedBatch_Net10(string modelPath, int batchSize)
{
    var embeddings = new SentenceTransformerEmbeddings(modelPath);
    var texts = Enumerable.Repeat("Test sentence", batchSize).ToArray();
    return await embeddings.EmbedBatchAsync(texts);
}
```

### Regression Testing
- ✅ Verify all existing unit tests pass
- ✅ Validate output equivalence with .NET 9 version
- ✅ Cross-check embedding results (cosine similarity > 0.999)
- ✅ Memory profiling (ensure no memory leaks)

## Documentation Updates

### Required Updates
1. **README.md** - Add .NET 10 & C# 14 features section
2. **AGENTS.md** - Update C# 14 guidelines with examples
3. **ONNX_EMBEDDINGS_DEVELOPMENT_PLAN.md** - Mark .NET 10 enhancements
4. **New: DOTNET10_FEATURES.md** - Detailed C# 14 feature usage guide
5. **Performance benchmarks** - Document .NET 10 performance improvements

## Expected Performance Improvements

Based on Microsoft's .NET 10 benchmarks:

| Component | Expected Improvement | Notes |
|-----------|---------------------|-------|
| Array enumeration | 10-20% | Devirtualization |
| Tight loops | 5-15% | Loop inversion, inlining |
| Tensor operations | 50-200% | TensorPrimitives SIMD |
| Small array allocation | 20-40% | Stack allocation |
| Overall throughput | 15-30% | Combined optimizations |

## Success Criteria

- ✅ All existing tests pass on .NET 10
- ✅ No breaking changes for existing consumers
- ✅ Documented 15%+ performance improvement
- ✅ Code uses C# 14 features where beneficial
- ✅ Clean build with no warnings
- ✅ Updated documentation with examples

## Risk Assessment

| Risk | Severity | Mitigation |
|------|----------|------------|
| Tensor API breaking changes | High | Careful evaluation, compatibility layer |
| Performance regression | Medium | Comprehensive benchmarking |
| .NET 10 adoption barrier | Low | Multi-target support |
| Learning curve for C# 14 | Low | Good documentation, examples |

## Timeline

- **Phase 1 (Quick Wins):** Weeks 1-2 of 2026
- **Phase 2 (Performance):** Weeks 3-5 of 2026
- **Phase 3 (Tensor API):** Weeks 6-9 of 2026  
- **Phase 4 (API Enhancement):** Weeks 10-12 of 2026
- **Phase 5 (Advanced):** Q2 2026

**Target Completion:** End of Q1 2026

## Conclusion

.NET 10 and C# 14 provide significant opportunities for performance improvements and code quality enhancements in Chonkie.Net. The implementation is already solid; these enhancements will make it even better.

**Key Focus Areas:**
1. ⚡ Performance (TensorPrimitives, runtime optimizations)
2. 🎨 Code Quality (C# 14 features)
3. 📊 Tensor API modernization
4. 📚 Documentation

**Next Steps:**
1. Review and approve this plan
2. Create detailed task breakdown
3. Set up .NET 10 development environment
4. Begin Phase 1 implementation
