namespace Chonkie.Chunkers;

using Chonkie.Core.Chunker;
using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;
using Microsoft.Extensions.Logging;

/// <summary>
/// Mode for extracting split index from LLM response.
/// </summary>
public enum ExtractionMode
{
    /// <summary>
    /// Use structured JSON output via GenerateJsonAsync (requires genie support).
    /// </summary>
    Json,

    /// <summary>
    /// Use plain text generation via GenerateAsync and parse integer response.
    /// </summary>
    Text,

    /// <summary>
    /// Auto-detect based on genie capabilities (default).
    /// </summary>
    Auto
}

/// <summary>
/// Placeholder for an LLM-guided agentic chunker. Until Genie integration is available,
/// this class delegates to RecursiveChunker and logs a notice.
/// </summary>
public class SlumberChunker : BaseChunker
{
    private readonly RecursiveChunker _fallback;

    /// <summary>
    /// Maximum number of tokens per chunk.
    /// </summary>
    public int ChunkSize { get; }

    /// <summary>
    /// Mode for extracting split index from LLM response.
    /// </summary>
    public ExtractionMode ExtractionMode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SlumberChunker"/> class.
    /// </summary>
    /// <param name="tokenizer">The tokenizer to use for tokenization.</param>
    /// <param name="chunkSize">Maximum number of tokens per chunk.</param>
    /// <param name="extractionMode">Mode for extracting split index from LLM (JSON, Text, or Auto).</param>
    /// <param name="logger">Optional logger for diagnostic messages.</param>
    public SlumberChunker(
        ITokenizer tokenizer,
        int chunkSize = 2048,
        ExtractionMode extractionMode = ExtractionMode.Auto,
        ILogger<SlumberChunker>? logger = null)
        : base(tokenizer, logger)
    {
        if (chunkSize <= 0) throw new ArgumentException("chunk_size must be greater than 0", nameof(chunkSize));
        ChunkSize = chunkSize;
        ExtractionMode = extractionMode;
        _fallback = new RecursiveChunker(tokenizer, chunkSize);

        Logger.LogInformation(
            "SlumberChunker initialized with ExtractionMode={Mode}, ChunkSize={Size}. " +
            "Currently using RecursiveChunker fallback until Genie integration is available.",
            extractionMode, chunkSize);
    }

    /// <summary>
    /// Chunks the input text using the fallback RecursiveChunker.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    /// <returns>A list of chunks.</returns>
    public override IReadOnlyList<Chunk> Chunk(string text)
    {
        Logger.LogInformation("SlumberChunker fallback engaged — using RecursiveChunker until Genie integration is available.");
        return _fallback.Chunk(text);
    }

    /// <summary>
    /// Returns a string representation of the SlumberChunker.
    /// </summary>
    /// <returns>A string describing the chunker configuration.</returns>
    public override string ToString() => $"SlumberChunker(chunk_size={ChunkSize}, extraction_mode={ExtractionMode}, mode=fallback)";
}
