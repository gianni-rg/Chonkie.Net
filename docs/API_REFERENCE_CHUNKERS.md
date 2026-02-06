# API Reference - Chunkers
**Scope:** Chunking strategies and neural helpers.

## Python Reference
- [chonkie/docs/oss/chunkers/overview.mdx](chonkie/docs/oss/chunkers/overview.mdx)
- [chonkie/docs/oss/chunkers/token-chunker.mdx](chonkie/docs/oss/chunkers/token-chunker.mdx)
- [chonkie/docs/oss/chunkers/sentence-chunker.mdx](chonkie/docs/oss/chunkers/sentence-chunker.mdx)
- [chonkie/docs/oss/chunkers/recursive-chunker.mdx](chonkie/docs/oss/chunkers/recursive-chunker.mdx)

## Chonkie.Chunkers

### TokenChunker
Splits text into fixed-size token chunks with optional overlap.

Members:
- Constructors: `TokenChunker(ITokenizer tokenizer, int chunkSize = 2048, int chunkOverlap = 0, ILogger? logger = null)`, `TokenChunker(ITokenizer tokenizer, int chunkSize, double chunkOverlapFraction, ILogger? logger = null)`
- Properties: `int ChunkSize { get; }`, `int ChunkOverlap { get; }`
- Methods: `override IReadOnlyList<Chunk> Chunk(string text)`, `override string ToString()`

### SentenceChunker
Splits text at sentence boundaries while respecting token limits.

Members:
- Constructors: `SentenceChunker(ITokenizer tokenizer, int chunkSize = 2048, int chunkOverlap = 0, int minSentencesPerChunk = 1, int minCharactersPerSentence = 12, IReadOnlyList<string>? delimiters = null, string? includeDelimiter = "prev", ILogger? logger = null)`
- Properties: `int ChunkSize { get; }`, `int ChunkOverlap { get; }`, `int MinSentencesPerChunk { get; }`, `int MinCharactersPerSentence { get; }`, `IReadOnlyList<string> Delimiters { get; }`, `string? IncludeDelimiter { get; }`
- Methods: `override IReadOnlyList<Chunk> Chunk(string text)`, `override string ToString()`

### RecursiveChunker
Recursively splits text according to `RecursiveRules`.

Members:
- Constructors: `RecursiveChunker(ITokenizer tokenizer, int chunkSize = 2048, RecursiveRules? rules = null, int minCharactersPerChunk = 24, ILogger? logger = null)`
- Properties: `int ChunkSize { get; }`, `int MinCharactersPerChunk { get; }`, `RecursiveRules Rules { get; }`
- Methods: `override IReadOnlyList<Chunk> Chunk(string text)`, `override string ToString()`

### SemanticChunker
Uses semantic similarity to find split points.

Members:
- Constructors: `SemanticChunker(ITokenizer tokenizer, IEmbeddings embeddingModel, ILogger<SemanticChunker>? logger = null, float threshold = 0.8f, int chunkSize = 2048, int similarityWindow = 3, int minSentencesPerChunk = 1, int minCharactersPerSentence = 24, string[]? delimiters = null, string includeDelim = "prev", int skipWindow = 0)`
- Properties: `int ChunkSize { get; }`
- Methods: `override IReadOnlyList<Chunk> Chunk(string text)`, `override string ToString()`

### LateChunker
Embeds chunks after recursive splitting (late interaction).

Members:
- Constructors: `LateChunker(ITokenizer tokenizer, IEmbeddings embeddingModel, int chunkSize = 2048, RecursiveRules? rules = null, int minCharactersPerChunk = 24, ILogger<LateChunker>? logger = null)`
- Methods: `override IReadOnlyList<Chunk> Chunk(string text)`, `override string ToString()`

### CodeChunker
Heuristically splits code at structure boundaries.

Members:
- Constructors: `CodeChunker(ITokenizer tokenizer, int chunkSize = 2048, int minCharactersPerChunk = 24, ILogger<CodeChunker>? logger = null)`
- Properties: `int ChunkSize { get; }`, `int MinCharactersPerChunk { get; }`
- Methods: `override IReadOnlyList<Chunk> Chunk(string text)`, `override string ToString()`

### TableChunker
Preserves markdown tables and optionally repeats headers.

Members:
- Constructors: `TableChunker(ITokenizer tokenizer, int chunkSize = 2048, bool repeatHeaders = false, ILogger<TableChunker>? logger = null)`
- Properties: `int ChunkSize { get; }`, `bool RepeatHeaders { get; }`
- Methods: `override IReadOnlyList<Chunk> Chunk(string text)`, `override string ToString()`

### FastChunker
High-performance character-based chunker.

Members:
- Constructors: `FastChunker(int chunkSize = 512, int chunkOverlap = 0)`
- Properties: `int ChunkSize { get; }`, `int ChunkOverlap { get; }`
- Methods: `IReadOnlyList<Chunk> Chunk(string text)`, `IReadOnlyList<IReadOnlyList<Chunk>> ChunkBatch(IEnumerable<string> texts, IProgress<double>? progress = null, CancellationToken cancellationToken = default)`, `Document ChunkDocument(Document document)`, `override string ToString()`

### ExtractionMode
Mode for extracting split indices from LLM responses.

Members:
- Enum values: `Json`, `Text`, `Auto`

### SlumberChunker
LLM-guided chunker with recursive fallback.

Members:
- Constructors: `SlumberChunker(ITokenizer tokenizer, int chunkSize = 2048, ExtractionMode extractionMode = ExtractionMode.Auto, ILogger<SlumberChunker>? logger = null)`
- Properties: `int ChunkSize { get; }`, `ExtractionMode ExtractionMode { get; }`
- Methods: `override IReadOnlyList<Chunk> Chunk(string text)`, `override string ToString()`

### NeuralChunker
ONNX-backed neural splitter with recursive fallback.

Members:
- Constructors: `NeuralChunker(ITokenizer tokenizer, int chunkSize = 2048, int minCharactersPerChunk = 10, ILogger<NeuralChunker>? logger = null)`, `NeuralChunker(ITokenizer tokenizer, string modelPath, int chunkSize = 2048, int minCharactersPerChunk = 10, ILogger<NeuralChunker>? logger = null)`
- Properties: `int ChunkSize { get; }`, `bool UseOnnx { get; }`, `static string[] SupportedModels { get; }`
- Methods: `bool InitializeOnnxModel(string modelPath)`, `override IReadOnlyList<Chunk> Chunk(string text)`, `override string ToString()`, `void Dispose()`

## Chonkie.Chunkers.Neural

### OnnxTokenClassifier
Runs ONNX token classification to detect split points.

Members:
- Constructors: `OnnxTokenClassifier(string modelPath, SessionOptions? sessionOptions = null, ILogger<OnnxTokenClassifier>? logger = null)`
- Properties: `TokenClassificationConfig Config { get; }`, `int MaxSequenceLength { get; }`, `int NumLabels { get; }`
- Methods: `List<TokenClassificationResult> Classify(string text, int stride = 0)`, `List<TokenClassificationSpan> AggregateTokens(List<TokenClassificationResult> tokenResults)`, `void Dispose()`

### TokenClassificationConfig
Model config for token classification.

Members:
- Properties: `string? ModelType { get; set; }`, `int HiddenSize { get; set; }`, `int MaxPositionEmbeddings { get; set; }`, `int VocabSize { get; set; }`, `int NumAttentionHeads { get; set; }`, `int IntermediateSize { get; set; }`, `int NumLabels { get; set; }`, `Dictionary<string, int> Label2Id { get; set; }`, `Dictionary<string, string> Id2Label { get; set; }`
- Methods: `static TokenClassificationConfig LoadFromFile(string configPath)`, `int GetLabelIdByName(string labelName)`, `string GetLabelNameById(int labelId)`

### TokenClassificationResult
Represents a single token classification prediction.

Members:
- Properties: `string Token { get; set; }`, `float Score { get; set; }`, `string Label { get; set; }`, `int LabelId { get; set; }`, `int Start { get; set; }`, `int End { get; set; }`, `bool IsSplitPoint { get; }`
- Methods: `override string ToString()`

### TokenClassificationSpan
Aggregated token span for split detection.

Members:
- Properties: `string Label { get; set; }`, `int Start { get; set; }`, `int End { get; set; }`, `float Score { get; set; }`, `List<TokenClassificationResult> Tokens { get; set; }`, `bool IsSplitPoint { get; }`, `int Length { get; }`
- Methods: `override string ToString()`
