# C# 14 Extension Members Implementation

## Date: December 16, 2025

## Overview

This document details the implementation of C# 14 extension members in the Chonkie.Net solution, including the correct syntax discovered through Microsoft documentation research.

## Correct C# 14 Extension Member Syntax

### Key Discovery

The correct syntax for C# 14 extension members uses `extension(Type receiver)` **inside** a static class, not `extension for` as initially attempted.

### Syntax Structure

```csharp
public static class ExtensionClassName
{
    // Instance extension members
    extension(TargetType receiver)
    {
        public ReturnType PropertyName => receiver.SomeOperation();
        
        public ReturnType MethodName(parameters)
        {
            // Implementation using 'receiver'
        }
    }
    
    // Static extension members (extends the type itself)
    extension(TargetType)
    {
        public static ReturnType StaticProperty => SomeValue;
        
        public static ReturnType StaticMethod(parameters)
        {
            // Static implementation
        }
    }
}
```

## Implementation: IChunker Extension Members

### File: `src/Chonkie.Core/Extensions/ChunkerExtensions.cs`

```csharp
namespace Chonkie.Core.Extensions;

using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;

/// <summary>
/// C# 14 extension members for IChunker interface.
/// Provides additional utility methods and properties for chunker implementations.
/// </summary>
public static class ChunkerExtensions
{
    /// <summary>
    /// Extension members for IChunker instances.
    /// </summary>
    extension(IChunker chunker)
    {
        /// <summary>
        /// Gets the strategy name of the chunker (type name without "Chunker" suffix).
        /// </summary>
        public string StrategyName => chunker.GetType().Name.Replace("Chunker", string.Empty);

        /// <summary>
        /// Chunks multiple texts asynchronously using Task.Run for background processing.
        /// </summary>
        /// <param name="texts">The texts to chunk.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<IReadOnlyList<IReadOnlyList<Chunk>>> ChunkBatchAsync(
            IEnumerable<string> texts,
            CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => chunker.ChunkBatch(texts, null, cancellationToken), cancellationToken);
        }
    }

    /// <summary>
    /// Static extension members for IChunker type.
    /// </summary>
    extension(IChunker)
    {
        /// <summary>
        /// Gets an empty chunk list for initialization or fallback scenarios.
        /// </summary>
        public static IReadOnlyList<Chunk> Empty => Array.Empty<Chunk>();
    }
}
```

## Usage Examples

### Instance Extension Members

```csharp
using Chonkie.Core.Extensions;

IChunker chunker = new TokenChunker();

// Access instance extension property
string strategy = chunker.StrategyName; // Returns "Token"

// Call instance extension method
var texts = new[] { "text1", "text2", "text3" };
var results = await chunker.ChunkBatchAsync(texts);
```

### Static Extension Members

```csharp
using Chonkie.Core.Extensions;

// Access static extension property
var emptyChunks = IChunker.Empty; // Returns Array.Empty<Chunk>()
```

## Test Implementation

### File: `tests/Chonkie.Core.Tests/Extensions/ChunkerExtensionsTests.cs`

Four test methods verify the extension members:

1. **StrategyName_ReturnsChunkerTypeName**
   - Validates instance property extracts type name correctly
   
2. **Empty_ReturnsEmptyChunkList**
   - Validates static property returns empty collection
   - **Important**: Must use `IChunker.Empty` not `ChunkerExtensions.Empty`
   
3. **ChunkBatchAsync_ProcessesTextsAsynchronously**
   - Validates async batch processing
   
4. **ChunkBatchAsync_WithCancellation_CanBeCancelled**
   - Validates cancellation token support

## Build Configuration

### File: `Directory.Build.props`

```xml
<LangVersion>14.0</LangVersion>
```

This setting enables C# 14 features across all projects in the solution.

## Verification Results

- ✅ Solution builds successfully with C# 14 syntax
- ✅ All 476 tests pass (410 successful, 66 skipped due to missing API keys)
- ✅ Extension members compile and execute correctly
- ✅ No breaking changes to existing code

## Key Differences from Traditional Extension Methods

### Traditional (C# 13 and earlier)

```csharp
public static class ChunkerExtensions
{
    public static string StrategyName(this IChunker chunker)
        => chunker.GetType().Name.Replace("Chunker", string.Empty);
}
```

### Modern (C# 14)

```csharp
public static class ChunkerExtensions
{
    extension(IChunker chunker)
    {
        public string StrategyName 
            => chunker.GetType().Name.Replace("Chunker", string.Empty);
    }
}
```

## Benefits of C# 14 Extension Members

1. **Properties**: Can extend with properties, not just methods
2. **Static Members**: Can extend the type itself with static members
3. **Operators**: Can define custom operators for extended types
4. **Cleaner Syntax**: More intuitive declaration within extension blocks
5. **Better Organization**: Group related extensions together

## Documentation References

- [Extension members (C# Programming Guide)](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
- [Extension declaration (C# Reference)](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/extension)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14#extension-members)
- [Tutorial: Explore extension members in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/extension-members)

## Next Steps

This successful implementation validates the approach for extending additional interfaces:

1. **ITokenizer** - Add extension members for tokenization utilities
2. **IEmbeddings** - Add extension members for embedding operations
3. **Other interfaces** - Apply pattern across all major interfaces

## Notes for Developers

- The receiver parameter name (e.g., `chunker`) is accessible within the extension block
- Static extension members omit the parameter name: `extension(IChunker)` not `extension(IChunker chunker)`
- Static extension members are called on the type: `IChunker.Empty`
- Instance extension members are called on instances: `chunker.StrategyName`
- XML documentation comments on the extension block describe the extended type and are copied to generated members
