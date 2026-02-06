# API Reference - Chefs
**Scope:** Preprocessing components.

## Python Reference
- [chonkie/docs/oss/chefs/overview.mdx](chonkie/docs/oss/chefs/overview.mdx)

## Chonkie.Chefs

### IChef
Chef contract for preprocessing text.

Members:
- Methods: `Task<string> ProcessAsync(string text, CancellationToken cancellationToken = default)`

### TextChef
Basic text normalization.

Members:
- Methods: `Task<string> ProcessAsync(string text, CancellationToken cancellationToken = default)`, `string Process(ReadOnlySpan<char> text)`

### MarkdownChef
Markdown preprocessing with Markdig.

Members:
- Constructors: `MarkdownChef(MarkdownPipeline? pipeline = null)`
- Methods: `Task<string> ProcessAsync(string text, CancellationToken cancellationToken = default)`, `string Process(ReadOnlySpan<char> text)`

### TableChef
Extracts markdown tables.

Members:
- Methods: `Task<string> ProcessAsync(string text, CancellationToken cancellationToken = default)`

### CodeChef
Pass-through code preprocessing.

Members:
- Methods: `Task<string> ProcessAsync(string text, CancellationToken cancellationToken = default)`, `string Process(ReadOnlySpan<char> text)`

## Chonkie.Chefs.Extensions

### ChefExtensions
C# 14 extension members for `IChef`.

Members:
- Extension properties: `string ChefType { get; }`
- Extension methods: `Task<IReadOnlyList<string>> ProcessBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)`, `Task<bool> WouldModifyAsync(string text, CancellationToken cancellationToken = default)`
- Static members: `string Empty { get; }`
