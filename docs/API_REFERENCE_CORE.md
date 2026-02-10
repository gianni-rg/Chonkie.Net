# API Reference - Core
**Scope:** Core types, interfaces, and extension members used across Chonkie.Net.

## Python Reference
- [chonkie/docs/oss/chunkers/overview.mdx](chonkie/docs/oss/chunkers/overview.mdx)
- [chonkie/docs/oss/quick-start.mdx](chonkie/docs/oss/quick-start.mdx)
- [docs/PYTHON_NET_BEHAVIOR_DIFFERENCES.md](docs/PYTHON_NET_BEHAVIOR_DIFFERENCES.md)

## Chonkie.Core.Types

### Chunk
Represents a chunk of text with metadata and optional embeddings.

Members:
- Properties: `string Id { get; init; }`, `string Text { get; init; }`, `int StartIndex { get; init; }`, `int EndIndex { get; init; }`, `int TokenCount { get; init; }`, `string? Context { get; init; }`, `float[]? Embedding { get; init; }`, `int Length { get; }`
- Methods: `Dictionary<string, object?> ToDictionary()`, `static Chunk FromDictionary(Dictionary<string, object?> data)`, `Chunk Copy()`

### Document
Represents a document that can be chunked and processed.

Members:
- Properties: `string Id { get; set; }`, `string Content { get; set; }`, `List<Chunk> Chunks { get; set; }`, `Dictionary<string, object> Metadata { get; set; }`, `string? Source { get; set; }`

### Sentence
Represents a sentence with position and token information.

Members:
- Properties: `string Text { get; init; }`, `int StartIndex { get; init; }`, `int EndIndex { get; init; }`, `int TokenCount { get; init; }`

### MarkdownDocument
Represents a markdown document with extracted structural elements.

Members:
- Properties: `List<MarkdownTable> Tables { get; set; }`, `List<MarkdownCode> Code { get; set; }`, `List<MarkdownImage> Images { get; set; }`

### MarkdownTable
Represents a table found in a markdown document.

Members:
- Properties: `string Content { get; set; }`, `int StartIndex { get; set; }`, `int EndIndex { get; set; }`

### MarkdownCode
Represents a code block found in a markdown document.

Members:
- Properties: `string Content { get; set; }`, `string? Language { get; set; }`, `int StartIndex { get; set; }`, `int EndIndex { get; set; }`

### MarkdownImage
Represents an image found in a markdown document.

Members:
- Properties: `string Alias { get; set; }`, `string Content { get; set; }`, `int StartIndex { get; set; }`, `int EndIndex { get; set; }`, `string? Link { get; set; }`

### RecursiveLevel
Defines chunking rules at a specific recursive level.

Members:
- Properties: `IReadOnlyList<string>? Delimiters { get; init; }`, `bool Whitespace { get; init; }`, `string? IncludeDelimiter { get; init; }`
- Constructors: `RecursiveLevel()`
- Methods: `override string ToString()`

### RecursiveRules
Defines a hierarchy of recursive chunking rules.

Members:
- Properties: `IReadOnlyList<RecursiveLevel> Levels { get; }`, `int Count { get; }`, `RecursiveLevel this[int index] { get; }`
- Constructors: `RecursiveRules(IReadOnlyList<RecursiveLevel>? levels = null)`
- Methods: `override string ToString()`

## Chonkie.Core.Interfaces

### IChunker
Interface for chunker implementations.

Members:
- Methods: `IReadOnlyList<Chunk> Chunk(string text)`, `IReadOnlyList<IReadOnlyList<Chunk>> ChunkBatch(IEnumerable<string> texts, IProgress<double>? progress = null, CancellationToken cancellationToken = default)`, `Document ChunkDocument(Document document)`

### ITokenizer
Interface for tokenizers.

Members:
- Methods: `IReadOnlyList<int> Encode(string text)`, `string Decode(IReadOnlyList<int> tokens)`, `int CountTokens(string text)`, `IReadOnlyList<IReadOnlyList<int>> EncodeBatch(IEnumerable<string> texts)`, `IReadOnlyList<string> DecodeBatch(IEnumerable<IReadOnlyList<int>> tokenSequences)`, `IReadOnlyList<int> CountTokensBatch(IEnumerable<string> texts)`

## Chonkie.Core.Chunker

### BaseChunker
Abstract base class providing common chunker functionality.

Members:
- Constructors: `protected BaseChunker(ITokenizer tokenizer, ILogger? logger = null)`
- Properties (protected): `ITokenizer Tokenizer { get; }`, `ILogger Logger { get; }`, `bool UseParallelProcessing { get; set; }`
- Methods: `abstract IReadOnlyList<Chunk> Chunk(string text)`, `virtual IReadOnlyList<IReadOnlyList<Chunk>> ChunkBatch(IEnumerable<string> texts, IProgress<double>? progress = null, CancellationToken cancellationToken = default)`, `virtual Document ChunkDocument(Document document)`
- Methods (protected): `virtual IReadOnlyList<IReadOnlyList<Chunk>> SequentialBatchProcessing(IReadOnlyList<string> texts, IProgress<double>? progress, CancellationToken cancellationToken)`, `virtual IReadOnlyList<IReadOnlyList<Chunk>> ParallelBatchProcessing(IReadOnlyList<string> texts, IProgress<double>? progress, CancellationToken cancellationToken)`

## Chonkie.Core.Extensions

### ChunkerExtensions
C# 14 extension members for `IChunker`.

Members:
- Extension properties: `string StrategyName { get; }`
- Extension methods: `Task<IReadOnlyList<IReadOnlyList<Chunk>>> ChunkBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)`
- Static members: `IReadOnlyList<Chunk> Empty { get; }`

### TokenizerExtensions
C# 14 extension members for `ITokenizer`.

Members:
- Extension properties: `string TokenizerName { get; }`
- Extension methods: `bool IsEmpty(string text)`, `Task<IReadOnlyList<int>> EncodeAsync(string text, CancellationToken cancellationToken = default)`, `Task<string> DecodeAsync(IReadOnlyList<int> tokens, CancellationToken cancellationToken = default)`
- Static members: `int MaxTokenLength { get; }`

## Chonkie.Core.Pipeline

### ComponentType
Defines pipeline component types.

Members:
- Enum values: `Fetcher`, `Chef`, `Chunker`, `Refinery`, `Porter`, `Handshake`

### PipelineComponentAttribute
Attribute used to register pipeline components.

Members:
- Properties: `string Alias { get; }`, `ComponentType ComponentType { get; }`
- Constructors: `PipelineComponentAttribute(string alias, ComponentType componentType)`
