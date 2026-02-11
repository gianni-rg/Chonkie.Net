namespace Chonkie.Chunkers;

using Chonkie.Chunkers.Neural;
using Chonkie.Core.Chunker;
using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;
using Microsoft.Extensions.Logging;

/// <summary>
/// Neural chunker that uses ONNX models for token classification-based splitting.
/// Predicts split points using a pre-trained token classification model.
/// When ONNX model is not available, falls back to RecursiveChunker.
/// </summary>
public sealed class NeuralChunker : BaseChunker, IDisposable
{
    private readonly RecursiveChunker _fallback;
    private OnnxTokenClassifier? _classifier;
    private readonly int _minCharactersPerChunk;
    private bool _useOnnx;

    /// <summary>
    /// Supported pre-trained models that can be converted to ONNX.
    /// These are from Chonky: https://github.com/mirth/chonky
    /// </summary>
    public static readonly string[] SupportedModels = new[]
    {
        "mirth/chonky_distilbert_base_uncased_1",
        "mirth/chonky_modernbert_base_1",
        "mirth/chonky_modernbert_large_1"
    };

    /// <summary>
    /// Gets the maximum number of tokens per chunk.
    /// </summary>
    public int ChunkSize { get; }

    /// <summary>
    /// Gets whether ONNX models are being used.
    /// </summary>
    public bool UseOnnx => _useOnnx;

    /// <summary>
    /// Initializes a new instance of the <see cref="NeuralChunker"/> class with fallback behavior.
    /// </summary>
    /// <param name="tokenizer">The tokenizer to use for tokenization.</param>
    /// <param name="chunkSize">Maximum number of tokens per chunk.</param>
    /// <param name="minCharactersPerChunk">Minimum characters before considering a split point.</param>
    /// <param name="logger">Optional logger for diagnostic messages.</param>
    public NeuralChunker(
        ITokenizer tokenizer,
        int chunkSize = 2048,
        int minCharactersPerChunk = 10,
        ILogger<NeuralChunker>? logger = null)
        : base(tokenizer, logger)
    {
        if (chunkSize <= 0) throw new ArgumentException("chunk_size must be greater than 0", nameof(chunkSize));

        ChunkSize = chunkSize;
        _minCharactersPerChunk = minCharactersPerChunk;
        _fallback = new RecursiveChunker(tokenizer, chunkSize);
        _useOnnx = false;
        _classifier = null;

        Logger.LogInformation(
            "NeuralChunker initialized in fallback mode. Use InitializeOnnxModel() to enable ONNX-based splitting.");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NeuralChunker"/> class with ONNX model support.
    /// </summary>
    /// <param name="tokenizer">The tokenizer to use for tokenization.</param>
    /// <param name="modelPath">Path to the ONNX model directory.</param>
    /// <param name="chunkSize">Maximum number of tokens per chunk.</param>
    /// <param name="minCharactersPerChunk">Minimum characters before considering a split point.</param>
    /// <param name="logger">Optional logger for diagnostic messages.</param>
    public NeuralChunker(
        ITokenizer tokenizer,
        string modelPath,
        int chunkSize = 2048,
        int minCharactersPerChunk = 10,
        ILogger<NeuralChunker>? logger = null)
        : base(tokenizer, logger)
    {
        if (chunkSize <= 0) throw new ArgumentException("chunk_size must be greater than 0", nameof(chunkSize));
        if (string.IsNullOrEmpty(modelPath)) throw new ArgumentNullException(nameof(modelPath));

        ChunkSize = chunkSize;
        _minCharactersPerChunk = minCharactersPerChunk;
        _fallback = new RecursiveChunker(tokenizer, chunkSize);

        try
        {
            var onnxLogger = logger is not null
                ? new OnnxLoggerAdapter(logger)
                : null;

            _classifier = new OnnxTokenClassifier(modelPath, sessionOptions: null, logger: onnxLogger);
            _useOnnx = true;

            Logger.LogInformation(
                "NeuralChunker initialized with ONNX model from {ModelPath}. Model supports {NumLabels} labels.",
                modelPath,
                _classifier.NumLabels);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(
                ex,
                "Failed to load ONNX model from {ModelPath}. Falling back to RecursiveChunker.",
                modelPath);
            _useOnnx = false;
            _classifier = null;
        }
    }

    /// <summary>
    /// Initializes ONNX model support after construction.
    /// </summary>
    /// <param name="modelPath">Path to the ONNX model directory.</param>
    /// <returns>True if initialization was successful, false otherwise.</returns>
    public bool InitializeOnnxModel(string modelPath)
    {
        if (string.IsNullOrEmpty(modelPath))
        {
            Logger.LogWarning("Model path is empty. Cannot initialize ONNX model.");
            _useOnnx = false;
            return false;
        }

        try
        {
            var onnxLogger = new OnnxLoggerAdapter(Logger);
            _classifier = new OnnxTokenClassifier(modelPath, sessionOptions: null, logger: onnxLogger);
            _useOnnx = true;

            Logger.LogInformation(
                "Successfully initialized ONNX model from {ModelPath}",
                modelPath);

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(
                ex,
                "Failed to initialize ONNX model from {ModelPath}",
                modelPath);
            _useOnnx = false;
            return false;
        }
    }

    /// <summary>
    /// Chunks the input text using neural methods (ONNX) or falls back to recursive chunking.
    /// Automatically selects strategy based on ONNX model availability.
    /// </summary>
    /// <param name="text">The text to chunk. If null or empty, returns an empty list.</param>
    /// <returns>A list of chunks with token counts and position information.</returns>
    /// <remarks>
    /// This chunker implements a hybrid approach:
    /// 
    /// **ONNX Mode (if available):**
    /// - Uses pre-trained token classification model to detect optimal chunk boundaries
    /// - Models are trained to recognize natural breaking points in text
    /// - Results in more semantically coherent chunks than rule-based approaches
    /// - Requires pre-converted ONNX model files (models/*.onnx)
    /// 
    /// **Fallback Mode (no ONNX model):**
    /// - Automatically falls back to RecursiveChunker if ONNX unavailable
    /// - No performance degradation, just less semantic intelligence
    /// - Useful during development or when models aren't pre-downloaded
    /// 
    /// The neural approach works by:
    /// 1. Tokenizing the input text
    /// 2. Running ONNX token classifier to get split probabilities
    /// 3. Identifying split points (tokens with score &gt; 0.5)
    /// 4. Creating chunks honoring the chunk_size limit
    /// 5. Falling back to recursive chunking if neural method fails
    /// </remarks>
    /// <example>
    /// <code>
    /// // With ONNX model
    /// var chunker = new NeuralChunker(tokenizer, "models/modernbert", chunkSize: 2048);
    /// if (chunker.UseOnnx)
    ///     Console.WriteLine("Using neural chunking");
    /// 
    /// var chunks = chunker.Chunk(longDocument);
    /// 
    /// // Without ONNX model (uses fallback)
    /// var chunker2 = new NeuralChunker(tokenizer, chunkSize: 2048);
    /// // Automatically falls back to RecursiveChunker internally
    /// </code>
    /// </example>
    public override IReadOnlyList<Chunk> Chunk(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Array.Empty<Chunk>();
        }

        if (_useOnnx && _classifier is not null)
        {
            return ChunkWithOnnx(text);
        }

        Logger.LogDebug("Using RecursiveChunker fallback for text of length {Length}", text.Length);
        return _fallback.Chunk(text);
    }

    private IReadOnlyList<Chunk> ChunkWithOnnx(string text)
    {
        Logger.LogDebug("Starting ONNX-based neural chunking for text of length {Length}", text.Length);

        try
        {
            var tokenResults = _classifier!.Classify(text);
            if (tokenResults.Count == 0)
            {
                Logger.LogWarning("Token classification returned no results. Using fallback.");
                return _fallback.Chunk(text);
            }

            var spans = _classifier.AggregateTokens(tokenResults);

            var splitPoints = new List<int>();
            foreach (var span in spans)
            {
                if (span.IsSplitPoint && span.Score > 0.5f)
                {
                    splitPoints.Add(span.End);
                }
            }

            if (splitPoints.Count == 0)
            {
                Logger.LogDebug("No high-confidence split points found. Using fallback.");
                return _fallback.Chunk(text);
            }

            return GenerateChunksFromSplitPoints(text, splitPoints);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "ONNX-based chunking failed. Using fallback.");
            return _fallback.Chunk(text);
        }
    }

    private IReadOnlyList<Chunk> GenerateChunksFromSplitPoints(string text, List<int> splitPoints)
    {
        var chunks = new List<Chunk>();
        splitPoints = splitPoints.Where(p => p > 0 && p < text.Length).OrderBy(p => p).Distinct().ToList();

        if (splitPoints.Count == 0)
        {
            chunks.Add(CreateChunk(text, 0, text.Length));
            return chunks;
        }

        var currentPos = 0;
        foreach (var splitPoint in splitPoints)
        {
            var chunkLength = splitPoint - currentPos;

            if (chunkLength >= _minCharactersPerChunk)
            {
                chunks.Add(CreateChunk(text, currentPos, splitPoint));
                currentPos = splitPoint;
            }
        }

        if (currentPos < text.Length)
        {
            chunks.Add(CreateChunk(text, currentPos, text.Length));
        }

        Logger.LogInformation(
            "Created {ChunkCount} chunks using ONNX neural classification",
            chunks.Count);

        return chunks;
    }

    private Chunk CreateChunk(string text, int start, int end)
    {
        var chunkText = text.Substring(start, end - start);
        var tokenCount = Tokenizer.CountTokens(chunkText);

        return new Chunk
        {
            Text = chunkText,
            StartIndex = start,
            EndIndex = end,
            TokenCount = tokenCount
        };
    }

    /// <summary>
    /// Returns a string representation of the NeuralChunker.
    /// </summary>
    /// <returns>A string describing the chunker configuration.</returns>
    public override string ToString()
    {
        var mode = _useOnnx ? "onnx" : "fallback";
        return $"NeuralChunker(chunk_size={ChunkSize}, mode={mode}, min_chars={_minCharactersPerChunk})";
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
    public void Dispose()
    {
        _classifier?.Dispose();
    }
}

/// <summary>
/// Adapter to make ILogger work with OnnxTokenClassifier which uses different logger interface.
/// </summary>
internal class OnnxLoggerAdapter : ILogger<OnnxTokenClassifier>
{
    private readonly ILogger _logger;

    public OnnxLoggerAdapter(ILogger innerLogger)
    {
        _logger = innerLogger ?? throw new ArgumentNullException(nameof(innerLogger));
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _logger.BeginScope(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _logger.IsEnabled(logLevel);
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        _logger.Log(logLevel, eventId, state, exception, formatter);
    }
}
