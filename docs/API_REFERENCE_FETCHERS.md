# API Reference - Fetchers
**Scope:** Data ingestion components.

## Python Reference
- [chonkie/docs/oss/fetchers/overview.mdx](chonkie/docs/oss/fetchers/overview.mdx)

## Chonkie.Fetchers

### IFetcher
Fetcher contract for data ingestion.

Members:
- Methods: `Task<IReadOnlyList<(string Path, string Content)>> FetchAsync(string path, string? filter = null, CancellationToken cancellationToken = default)`

### FileFetcher
Filesystem fetcher.

Members:
- Methods: `Task<IReadOnlyList<(string Path, string Content)>> FetchAsync(string path, string? filter = null, CancellationToken cancellationToken = default)`

## Chonkie.Fetchers.Extensions

### FetcherExtensions
C# 14 extension members for `IFetcher`.

Members:
- Extension properties: `string FetcherType { get; }`
- Extension methods: `Task<string?> FetchSingleAsync(string filePath, CancellationToken cancellationToken = default)`, `Task<IReadOnlyList<(string Path, string Content)>> FetchMultipleAsync(IEnumerable<string> paths, string? filter = null, CancellationToken cancellationToken = default)`, `Task<int> CountDocumentsAsync(string path, string? filter = null, CancellationToken cancellationToken = default)`
- Static members: `IReadOnlyList<string> CommonTextExtensions { get; }`, `IReadOnlyList<string> CommonCodeExtensions { get; }`

### DocumentCollectionExtensions
Helpers for collections of fetched documents.

Members:
- Extension methods: `IEnumerable<(string Path, string Content)> FilterByExtension(this IEnumerable<(string Path, string Content)> documents, params string[] extensions)`, `IEnumerable<(string Path, string Content)> FilterByMinLength(this IEnumerable<(string Path, string Content)> documents, int minLength)`, `int TotalCharacters(this IEnumerable<(string Path, string Content)> documents)`
