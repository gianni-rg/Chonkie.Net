# FastChunker Implementation Guide for Chonkie.Net

**Target Implementation**: January 2026  
**Priority**: 🔴 CRITICAL  
**Estimated Effort**: 15-20 hours  
**Complexity**: Low-Medium

---

## Overview

FastChunker is the newest addition to the Python Chonkie library (v1.5.1). It's a lightweight, high-performance chunker that prioritizes speed over semantic accuracy, making it ideal for rapid prototyping, simple use cases, and performance-critical applications.

**Key Philosophy**: Simple, fast, minimal dependencies.

---

## What FastChunker Does

### Basic Chunking Strategy
- Splits text into fixed-size chunks
- No semantic analysis (unlike SemanticChunker)
- No complex rules (unlike RecursiveChunker)
- Just straightforward size-based splitting

### When to Use
✅ When you need speed above all else  
✅ For simple texts without complex structure  
✅ When you don't need semantic awareness  
✅ For rapid prototyping and testing  
✅ For very large documents where latency is critical  

### When NOT to Use
❌ For documents requiring semantic chunking  
❌ When chunk quality is more important than speed  
❌ For complex structured text  
❌ When you need overlap refinement  

---

## Python Implementation Reference

### Python FastChunker Key Methods

```python
class FastChunker(BaseChunker):
    def __init__(self, chunk_size: int = 512, chunk_overlap: int = 0):
        """
        Initialize FastChunker.
        
        Args:
            chunk_size: Target size of each chunk in characters
            chunk_overlap: Number of overlapping characters between chunks
        """
        self.chunk_size = chunk_size
        self.chunk_overlap = chunk_overlap
    
    def chunk(self, text: str) -> list[Chunk]:
        """Chunk a single text string."""
        # Simple sliding window approach
        # No semantic analysis, just character-based splitting
    
    def chunk_batch(self, texts: list[str]) -> list[Chunk]:
        """Chunk multiple texts in batch."""
        # Process multiple texts efficiently
    
    def __repr__(self) -> str:
        """String representation for debugging."""
        # Returns something like:
        # FastChunker(chunk_size=512, chunk_overlap=0)
```

### Algorithm

```
1. Start at position 0
2. Take next `chunk_size` characters
3. Find nearest word boundary (space) to avoid mid-word splits
4. Create chunk from 0 to boundary
5. Move forward by (chunk_size - overlap)
6. If overlap > 0:
   - Include last `overlap` characters of previous chunk
7. Repeat until text exhausted
```

---

## C# Implementation Plan

### File Structure

```
src/Chonkie.Chunkers/
├── FastChunker.cs                    (New)
└── Tests/
    └── FastChunkerTests.cs           (New)
```

### FastChunker.cs Structure

```csharp
namespace Chonkie.Chunkers;

/// <summary>
/// FastChunker - A lightweight, high-performance chunker.
/// 
/// FastChunker prioritizes speed over semantic accuracy, using simple
/// character-based splitting with optional word-boundary preservation.
/// Ideal for rapid prototyping and performance-critical applications.
/// </summary>
public class FastChunker : IChunker
{
    private readonly int _chunkSize;
    private readonly int _chunkOverlap;
    
    // Constructor
    public FastChunker(int chunkSize = 512, int chunkOverlap = 0)
    {
        // Validation
        // Storage
    }
    
    // IChunker implementation
    public string StrategyName => "fast";
    
    public IEnumerable<Chunk> Chunk(string text)
    {
        // Main algorithm
    }
    
    public IEnumerable<IEnumerable<Chunk>> ChunkBatch(IEnumerable<string> texts)
    {
        // Batch processing
    }
    
    public async Task<IEnumerable<Chunk>> ChunkAsync(string text)
    {
        // Async version
    }
    
    public async IAsyncEnumerable<IEnumerable<Chunk>> ChunkBatchAsync(
        IEnumerable<string> texts)
    {
        // Async batch processing
    }
    
    // Helper methods
    private int FindWordBoundary(string text, int position, bool backwards = false)
    {
        // Find nearest space
    }
    
    private int FindNearestSpace(ReadOnlySpan<char> text, int maxPosition)
    {
        // Optimized space finding
    }
    
    public override string ToString()
    {
        // Nice string representation
        // FastChunker(ChunkSize=512, ChunkOverlap=0)
    }
}
```

### Key Implementation Details

#### 1. Constructor Validation

```csharp
public FastChunker(int chunkSize = 512, int chunkOverlap = 0)
{
    if (chunkSize <= 0)
        throw new ArgumentException("ChunkSize must be positive", nameof(chunkSize));
    
    if (chunkOverlap < 0)
        throw new ArgumentException("ChunkOverlap cannot be negative", nameof(chunkOverlap));
    
    if (chunkOverlap >= chunkSize)
        throw new ArgumentException("ChunkOverlap must be less than ChunkSize");
    
    _chunkSize = chunkSize;
    _chunkOverlap = chunkOverlap;
}
```

#### 2. Core Chunking Algorithm

```csharp
public IEnumerable<Chunk> Chunk(string text)
{
    if (string.IsNullOrEmpty(text))
        yield break;
    
    var position = 0;
    var chunkIndex = 0;
    
    while (position < text.Length)
    {
        // Calculate chunk end
        var chunkEnd = Math.Min(position + _chunkSize, text.Length);
        
        // If not at end, find word boundary
        if (chunkEnd < text.Length)
        {
            chunkEnd = FindWordBoundary(text, chunkEnd);
        }
        
        // Extract chunk
        var chunkText = text[position..chunkEnd];
        
        yield return new Chunk
        {
            Id = $"{StrategyName}_{chunkIndex}",
            Text = chunkText,
            StartIndex = position,
            EndIndex = chunkEnd,
        };
        
        // Move position forward with overlap
        position = chunkEnd - _chunkOverlap;
        chunkIndex++;
    }
}
```

#### 3. Word Boundary Finding

```csharp
private int FindWordBoundary(string text, int position, bool backwards = false)
{
    if (backwards)
    {
        // Find last space before position
        for (int i = position - 1; i >= 0; i--)
        {
            if (char.IsWhiteSpace(text[i]))
                return i;
        }
        // Fallback to position if no space found
        return position > 0 ? position : 1;
    }
    else
    {
        // Find first space after position
        for (int i = position; i < text.Length; i++)
        {
            if (char.IsWhiteSpace(text[i]))
                return i;
        }
        // If no space found, use position
        return position;
    }
}
```

#### 4. Batch Processing

```csharp
public IEnumerable<IEnumerable<Chunk>> ChunkBatch(IEnumerable<string> texts)
{
    foreach (var text in texts)
    {
        yield return Chunk(text);
    }
}

public async IAsyncEnumerable<IEnumerable<Chunk>> ChunkBatchAsync(
    IEnumerable<string> texts,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    foreach (var text in texts)
    {
        cancellationToken.ThrowIfCancellationRequested();
        yield return Chunk(text);
    }
    
    await Task.CompletedTask;
}
```

#### 5. ToString() Implementation

```csharp
public override string ToString()
{
    return $"{nameof(FastChunker)}(" +
           $"{nameof(ChunkSize)}={_chunkSize}, " +
           $"{nameof(ChunkOverlap)}={_chunkOverlap})";
}

// Alternative with property names:
public override string ToString() =>
    $"FastChunker(ChunkSize={_chunkSize}, ChunkOverlap={_chunkOverlap})";
```

---

## Unit Tests

### FastChunkerTests.cs Structure

```csharp
namespace Chonkie.Chunkers.Tests;

public class FastChunkerTests
{
    // Test Cases
    [Fact]
    public void Chunk_WithSimpleText_ReturnsSingleChunk()
    {
        // Arrange
        var chunker = new FastChunker(chunkSize: 100);
        var text = "This is a simple text.";
        
        // Act
        var result = chunker.Chunk(text).ToList();
        
        // Assert
        result.Should().HaveCount(1);
        result[0].Text.Should().Be(text);
    }
    
    [Fact]
    public void Chunk_WithLongText_ReturnsMultipleChunks()
    {
        // Arrange
        var chunker = new FastChunker(chunkSize: 20);
        var text = "This is a longer text that should be split into multiple chunks";
        
        // Act
        var result = chunker.Chunk(text).ToList();
        
        // Assert
        result.Should().HaveCount(greaterThan: 1);
        result.All(c => c.Text.Length <= 20).Should().BeTrue();
    }
    
    [Fact]
    public void Chunk_WithOverlap_CreatesOverlappingChunks()
    {
        // Test chunk overlap functionality
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidChunkSize_ThrowsException(int chunkSize)
    {
        // Arrange & Act & Assert
        var action = () => new FastChunker(chunkSize);
        action.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void Chunk_WithEmptyText_ReturnsEmpty()
    {
        // Test empty string handling
    }
    
    [Fact]
    public void ChunkBatch_WithMultipleTexts_ChunksAll()
    {
        // Test batch processing
    }
    
    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Test string representation
        var chunker = new FastChunker(512, 64);
        chunker.ToString().Should().Contain("FastChunker")
            .And.Contain("512")
            .And.Contain("64");
    }
}
```

---

## Integration Points

### 1. Update IChunker Interface (if needed)
Ensure `IChunker` includes all required methods:
- `StrategyName` property
- `Chunk(string)` method
- `ChunkBatch(IEnumerable<string>)` method
- Async versions

### 2. Update Chunker Exports
In `src/Chonkie.Chunkers/__init__.cs` (or equivalent):

```csharp
public static class ChunkerExports
{
    public static FastChunker CreateFastChunker(
        int chunkSize = 512, 
        int chunkOverlap = 0) =>
        new FastChunker(chunkSize, chunkOverlap);
}
```

### 3. Pipeline Support
Ensure FastChunker works with the Pipeline class:

```csharp
pipeline
    .ChunkWith(new FastChunker(512, 0))
    .RefineWith(...)
```

### 4. Documentation
Create `docs/FASTCHUNKER.md`:
- Description
- Usage examples
- Performance characteristics
- Comparison with other chunkers
- When to use

---

## Performance Expectations

### Benchmark Targets
| Operation | Target | Notes |
|-----------|--------|-------|
| Small text (1KB) | < 1ms | Single chunk |
| Medium text (100KB) | < 10ms | Multiple chunks |
| Large text (1MB) | < 100ms | Many chunks |
| Batch (100x 10KB) | < 50ms | Parallel if possible |

### Optimizations
1. Use `Span<char>` for zero-copy operations
2. Avoid string allocations where possible
3. Use ArrayPool for temporary buffers (if needed)
4. Implement word boundary search efficiently

---

## Configuration Options

### Basic Configuration
```csharp
var chunker = new FastChunker(
    chunkSize: 512,      // Chars per chunk
    chunkOverlap: 0      // Overlap in chars
);
```

### Advanced Configuration (Future)
```csharp
var chunker = new FastChunker(
    chunkSize: 512,
    chunkOverlap: 50,
    preserveWordBoundaries: true,  // Don't split mid-word
    maxChunkSize: 1024              // Hard limit
);
```

---

## Error Handling

```csharp
public class FastChunkerException : Exception
{
    public FastChunkerException(string message) : base(message) { }
}
```

Handle:
1. Invalid chunk sizes (≤ 0)
2. Overlap >= chunk_size
3. Null/empty texts
4. Memory constraints for very large texts

---

## Testing Scenarios

### Coverage Checklist
- [ ] Normal text chunking
- [ ] Text shorter than chunk size
- [ ] Text exactly equal to chunk size
- [ ] Text multiple times chunk size
- [ ] Empty text
- [ ] Null text
- [ ] Unicode/special characters
- [ ] Very long texts (performance)
- [ ] Word boundary preservation
- [ ] Chunk overlap functionality
- [ ] Constructor validation
- [ ] ToString() output
- [ ] Batch processing
- [ ] Async operations
- [ ] Cancellation tokens

---

## Implementation Checklist

- [ ] Create `FastChunker.cs` file
- [ ] Implement `IChunker` interface
- [ ] Add constructor with validation
- [ ] Implement `Chunk()` method
- [ ] Implement `ChunkBatch()` method
- [ ] Add async versions
- [ ] Implement `ToString()`
- [ ] Create comprehensive tests
- [ ] Add XML documentation
- [ ] Update module exports
- [ ] Add to pipeline support
- [ ] Create documentation file
- [ ] Performance testing
- [ ] Code review
- [ ] Merge to main

---

## Next Steps After FastChunker

1. **NeuralChunker** (20-25 hours)
   - More complex, requires ML.NET
   - Token classification based

2. **SlumberChunker** (18-22 hours)
   - LLM-based semantic chunking
   - Requires Genie abstraction

3. **New Embeddings** (12-15 hours each)
   - CatsuEmbeddings
   - GeminiEmbeddings
   - JinaEmbeddings

---

## Resources

- Python Reference: https://github.com/chonkie-inc/chonkie/blob/main/src/chonkie/chunker/fast.py
- Tests: https://github.com/chonkie-inc/chonkie/blob/main/tests/chunkers/test_fast_chunker.py
- Documentation: https://docs.chonkie.ai/chunkers/fast

