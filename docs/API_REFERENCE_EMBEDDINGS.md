# API Reference - Embeddings
**Scope:** Embedding providers, utilities, and SentenceTransformer support.

## Python Reference
- [chonkie/docs/oss/embeddings/overview.mdx](chonkie/docs/oss/embeddings/overview.mdx)

## Chonkie.Embeddings.Interfaces

### IEmbeddings
Contract for embedding providers.

Members:
- Properties: `string Name { get; }`, `int Dimension { get; }`
- Methods: `Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)`, `Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)`, `float Similarity(float[] u, float[] v)`

## Chonkie.Embeddings.Base

### BaseEmbeddings
Abstract base class for embedding providers.

Members:
- Properties: `abstract string Name { get; }`, `abstract int Dimension { get; }`
- Methods: `abstract Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)`, `virtual Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)`, `virtual float Similarity(float[] u, float[] v)`, `override string ToString()`

## Chonkie.Embeddings.Extensions

### EmbeddingsExtensions
C# 14 extension members for `IEmbeddings`.

Members:
- Extension properties: `string ProviderType { get; }`
- Extension methods: `float Magnitude(float[] vector)`, `bool IsNormalized(float[] vector, float tolerance = 1e-5f)`, `float Distance(float[] u, float[] v)`, `float CosineSimilarity(float[] u, float[] v)`, `void NormalizeInPlace(float[] vector)`, `float[] BatchCosineSimilarity(float[] query, float[][] candidates)`, `(int Index, float Similarity) FindMostSimilar(float[] query, float[][] candidates)`, `(int Index, float Similarity)[] FindTopKSimilar(float[] query, float[][] candidates, int k)`
- Static members: `int DefaultDimension { get; }`, `float[] Zero(int dimension)`

## Chonkie.Embeddings.Exceptions

### EmbeddingException
Base exception for embedding errors.

Members:
- Constructors: `EmbeddingException()`, `EmbeddingException(string message)`, `EmbeddingException(string message, Exception innerException)`

### EmbeddingRateLimitException
Rate limit error.

Members:
- Properties: `int? RetryAfterSeconds { get; }`
- Constructors: `EmbeddingRateLimitException()`, `EmbeddingRateLimitException(string message, int? retryAfterSeconds = null)`, `EmbeddingRateLimitException(string message, Exception innerException, int? retryAfterSeconds = null)`

### EmbeddingAuthenticationException
Authentication error.

Members:
- Constructors: `EmbeddingAuthenticationException()`, `EmbeddingAuthenticationException(string message)`, `EmbeddingAuthenticationException(string message, Exception innerException)`

### EmbeddingNetworkException
Network error.

Members:
- Constructors: `EmbeddingNetworkException()`, `EmbeddingNetworkException(string message)`, `EmbeddingNetworkException(string message, Exception innerException)`

### EmbeddingInvalidResponseException
Invalid response error.

Members:
- Properties: `int? StatusCode { get; }`
- Constructors: `EmbeddingInvalidResponseException()`, `EmbeddingInvalidResponseException(string message, int? statusCode = null)`, `EmbeddingInvalidResponseException(string message, Exception innerException, int? statusCode = null)`

## Chonkie.Embeddings

### VectorMath
Utility for vector math operations.

Members:
- Methods: `static float CosineSimilarity(float[] u, float[] v)`

### AutoEmbeddings
Factory for selecting providers by name.

Members:
- Methods: `static void RegisterProvider(string name, Func<IEmbeddings> factory)`, `static IEmbeddings GetProvider(string name)`, `static IReadOnlyList<string> ListProviders()`

### UnifiedEmbeddings
Wraps `Microsoft.Extensions.AI` embedding generators.

Members:
- Constructors: `UnifiedEmbeddings(IEmbeddingGenerator<string, Embedding<float>> generator, string providerName, int dimension, ILogger? logger = null)`
- Properties: `override string Name { get; }`, `override int Dimension { get; }`
- Methods: `override Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)`, `override Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)`, `override string ToString()`

### UnifiedEmbeddingsFactory
Factory methods for unified providers.

Members:
- Methods: `static IEmbeddings CreateOpenAI(string apiKey, string model = "text-embedding-ada-002", int dimension = 1536, ILogger? logger = null)`, `static IEmbeddings CreateAzureOpenAI(string endpoint, string apiKey, string deploymentName, int dimension = 1536, ILogger? logger = null)`, `static IEmbeddings CreateOllama(string model, int dimension, string endpoint = "http://localhost:11434", ILogger? logger = null)`, `static IEmbeddings CreateFromEnvironment(string providerName, string model, int dimension, ILogger? logger = null)`

### OpenAIEmbeddings
OpenAI embeddings provider.

Members:
- Constructors: `OpenAIEmbeddings(string apiKey, string model = "text-embedding-ada-002", int dimension = 1536)`
- Properties: `override string Name { get; }`, `override int Dimension { get; }`
- Methods: `override Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)`, `override Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)`

### AzureOpenAIEmbeddings
Azure OpenAI embeddings provider.

Members:
- Constructors: `AzureOpenAIEmbeddings(string endpoint, string apiKey, string deploymentName, int dimension = 1536)`
- Properties: `override string Name { get; }`, `override int Dimension { get; }`
- Methods: `override Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)`, `override Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)`

### CohereEmbeddings
Cohere embeddings provider.

Members:
- Constructors: `CohereEmbeddings(string apiKey, string model = "embed-english-v3.0", int dimension = 1024)`
- Properties: `override string Name { get; }`, `override int Dimension { get; }`
- Methods: `override Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)`, `override Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)`

### GeminiEmbeddings
Google Gemini embeddings provider.

Members:
- Constructors: `GeminiEmbeddings(string apiKey, string model = "embedding-001", int dimension = 768)`
- Properties: `override string Name { get; }`, `override int Dimension { get; }`
- Methods: `override Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)`, `override Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)`

### JinaEmbeddings
Jina AI embeddings provider.

Members:
- Constructors: `JinaEmbeddings(string apiKey, string model = "jina-embeddings-v2-base-en", int dimension = 768)`
- Properties: `override string Name { get; }`, `override int Dimension { get; }`
- Methods: `override Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)`, `override Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)`

### VoyageAIEmbeddings
Voyage AI embeddings provider.

Members:
- Constructors: `VoyageAIEmbeddings(string apiKey, string model = "voyage-2", int dimension = 1024)`
- Properties: `override string Name { get; }`, `override int Dimension { get; }`
- Methods: `override Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)`, `override Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)`

## Chonkie.Embeddings.SentenceTransformers

### SentenceTransformerEmbeddings
ONNX-based SentenceTransformer provider.

Members:
- Constructors: `SentenceTransformerEmbeddings(string modelPath, PoolingMode? poolingMode = null, bool normalize = true, int? maxLength = null, SessionOptions? sessionOptions = null)`
- Properties: `override string Name { get; }`, `override int Dimension { get; }`, `int MaxSequenceLength { get; }`
- Methods: `override Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)`, `int CountTokens(string text)`, `IReadOnlyList<int> CountTokensBatch(IEnumerable<string> texts)`, `void Dispose()`

### SentenceTransformerTokenizer
Tokenizer wrapper for SentenceTransformer models.

Members:
- Constructors: `SentenceTransformerTokenizer(string modelPath, int? maxLength = null)`
- Properties: `int MaxLength { get; }`
- Methods: `EncodingResult Encode(string text, bool addSpecialTokens = true, int? maxLength = null)`, `BatchEncodingResult EncodeBatch(IEnumerable<string> texts, bool addSpecialTokens = true, int? maxLength = null)`, `string Decode(IEnumerable<int> tokenIds, bool skipSpecialTokens = true)`, `int CountTokens(string text)`, `void Dispose()`

### EncodingResult
Encoding result for a single text.

Members:
- Properties: `int[] InputIds { get; set; }`, `int[] AttentionMask { get; set; }`, `int[] TokenTypeIds { get; set; }`

### BatchEncodingResult
Batch encoding result with padding.

Members:
- Properties: `int[][] InputIds { get; set; }`, `int[][] AttentionMask { get; set; }`, `int[][] TokenTypeIds { get; set; }`

### ModelConfig
Model configuration loaded from `config.json`.

Members:
- Properties: `string? ModelType { get; set; }`, `int HiddenSize { get; set; }`, `int Dim { get; set; }`, `int EffectiveHiddenSize { get; }`, `int MaxPositionEmbeddings { get; set; }`, `int VocabSize { get; set; }`, `int NumHiddenLayers { get; set; }`, `int NumAttentionHeads { get; set; }`, `int IntermediateSize { get; set; }`, `string? HiddenAct { get; set; }`, `double HiddenDropoutProb { get; set; }`, `double AttentionProbsDropoutProb { get; set; }`, `int TypeVocabSize { get; set; }`, `double InitializerRange { get; set; }`, `double LayerNormEps { get; set; }`, `int PadTokenId { get; set; }`, `string? PositionEmbeddingType { get; set; }`, `bool UseCache { get; set; }`, `double? ClassifierDropout { get; set; }`
- Methods: `static ModelConfig Load(string configPath)`

### PoolingConfig
Pooling configuration loaded from `1_Pooling/config.json` or `pooling_config.json`.

Members:
- Properties: `int WordEmbeddingDimension { get; set; }`, `bool PoolingModeClsToken { get; set; }`, `bool PoolingModeMeanTokens { get; set; }`, `bool PoolingModeMaxTokens { get; set; }`, `bool PoolingModeMeanSqrtLenTokens { get; set; }`, `bool PoolingModeWeightedMeanTokens { get; set; }`, `bool PoolingModeLastToken { get; set; }`
- Methods: `PoolingMode GetPrimaryPoolingMode()`, `static PoolingConfig Load(string configPath)`

### SpecialToken
Represents a special token entry in tokenizer configs.

Members:
- Properties: `string? Content { get; set; }`
- Methods: `string GetValue()`

### SpecialTokensMap
Special tokens map loaded from `special_tokens_map.json`.

Members:
- Properties: `JsonElement UnkTokenElement { get; set; }`, `JsonElement SepTokenElement { get; set; }`, `JsonElement PadTokenElement { get; set; }`, `JsonElement ClsTokenElement { get; set; }`, `JsonElement MaskTokenElement { get; set; }`, `string? UnkToken { get; }`, `string? SepToken { get; }`, `string? PadToken { get; }`, `string? ClsToken { get; }`, `string? MaskToken { get; }`
- Methods: `static SpecialTokensMap Load(string tokensPath)`

### TokenizerConfig
Tokenizer configuration loaded from `tokenizer_config.json`.

Members:
- Properties: `bool DoLowerCase { get; set; }`, `string? UnkToken { get; set; }`, `string? SepToken { get; set; }`, `string? PadToken { get; set; }`, `string? ClsToken { get; set; }`, `string? MaskToken { get; set; }`, `int ModelMaxLength { get; set; }`, `bool TokenizeChineseChars { get; set; }`, `bool? StripAccents { get; set; }`
- Methods: `static TokenizerConfig Load(string configPath)`

### PoolingMode
Pooling modes for sentence embeddings.

Members:
- Enum values: `Mean`, `Cls`, `Max`, `LastToken`

### PoolingUtilities
Pooling helpers for token embeddings.

Members:
- Methods: `static float[][] MeanPooling(float[] tokenEmbeddings, int[] attentionMask, int batchSize, int seqLength, int hiddenDim)`, `static float[][] ClsPooling(float[] tokenEmbeddings, int batchSize, int seqLength, int hiddenDim)`, `static float[][] MaxPooling(float[] tokenEmbeddings, int[] attentionMask, int batchSize, int seqLength, int hiddenDim)`, `static float[][] LastTokenPooling(float[] tokenEmbeddings, int[] attentionMask, int batchSize, int seqLength, int hiddenDim)`, `static float[][] L2Normalize(float[][] embeddings)`, `static float[] L2Normalize(float[] embedding)`, `static float[][] ApplyPooling(float[] tokenEmbeddings, int[] attentionMask, int batchSize, int seqLength, int hiddenDim, PoolingMode mode, bool normalize = true)`

### ModelManager
Model management utilities.

Members:
- Properties: `static string DefaultCacheDirectory { get; }`
- Methods: `static bool ValidateModel(string modelPath)`, `static ModelMetadata GetModelMetadata(string modelPath)`, `static string EnsureCacheDirectory(string? cacheDir = null)`, `static string GetModelPath(string modelName, string? cacheDir = null)`, `static bool IsModelCached(string modelName, string? cacheDir = null)`, `static Task<string> DownloadModelAsync(string modelName, string? cacheDir = null, CancellationToken cancellationToken = default)`, `static Task<string> LoadModelAsync(string modelName, string? cacheDir = null, CancellationToken cancellationToken = default)`

### ModelMetadata
Metadata for a SentenceTransformer model.

Members:
- Properties: `int HiddenSize { get; set; }`, `int MaxPositionEmbeddings { get; set; }`, `int VocabSize { get; set; }`, `string ModelType { get; set; }`, `int EmbeddingDimension { get; set; }`, `PoolingMode PoolingMode { get; set; }`
- Methods: `override string ToString()`
