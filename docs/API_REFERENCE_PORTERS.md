# API Reference - Porters
**Scope:** Export components for chunks.

## Python Reference
- [chonkie/docs/oss/porters/overview.mdx](chonkie/docs/oss/porters/overview.mdx)

## Chonkie.Porters

### IPorter
Porter contract for exporting chunks.

Members:
- Methods: `Task<bool> ExportAsync(IReadOnlyList<Chunk> chunks, string destination, CancellationToken cancellationToken = default)`

### JsonPorter
JSON exporter for chunks.

Members:
- Methods: `Task<bool> ExportAsync(IReadOnlyList<Chunk> chunks, string destination, CancellationToken cancellationToken = default)`

## Chonkie.Porters.Extensions

### PorterExtensions
C# 14 extension members for `IPorter`.

Members:
- Extension properties: `string PorterType { get; }`
- Extension methods: `Task<bool> ExportInBatchesAsync(IReadOnlyList<Chunk> chunks, string destination, int batchSize = 1000, CancellationToken cancellationToken = default)`, `Task<bool> ExportMultipleAsync(IEnumerable<IReadOnlyList<Chunk>> chunkLists, string destinationPattern, CancellationToken cancellationToken = default)`
- Static members: `IReadOnlyList<string> CommonFormats { get; }`, `int DefaultBatchSize { get; }`
