# API Reference - Refineries
**Scope:** Post-processing components for chunks.

## Python Reference
- [chonkie/docs/oss/refinery/overview.mdx](chonkie/docs/oss/refinery/overview.mdx)

## Chonkie.Refineries

### IRefinery
Refinery contract for chunk post-processing.

Members:
- Methods: `Task<IReadOnlyList<Chunk>> RefineAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken = default)`

### OverlapRefinery
Merges overlapping chunks.

Members:
- Constructors: `OverlapRefinery(int minOverlap = 16)`
- Methods: `Task<IReadOnlyList<Chunk>> RefineAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken = default)`

### EmbeddingsRefinery
Adds embeddings to chunks.

Members:
- Constructors: `EmbeddingsRefinery(IEmbeddings embeddings)`
- Methods: `Task<IReadOnlyList<Chunk>> RefineAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken = default)`

## Chonkie.Refineries.Extensions

### RefineryExtensions
C# 14 extension members for `IRefinery`.

Members:
- Extension properties: `string RefineryType { get; }`
- Extension methods: `Task<IReadOnlyList<Chunk>> RefineInBatchesAsync(IReadOnlyList<Chunk> chunks, int batchSize = 100, CancellationToken cancellationToken = default)`, `Task<bool> WouldModifyAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken = default)`
- Static members: `IReadOnlyList<Chunk> Empty { get; }`
