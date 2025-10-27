namespace Chonkie.Chunkers;

using Chonkie.Core.Chunker;
using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;
using Microsoft.Extensions.Logging;

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
    /// Initializes a new instance of the <see cref="SlumberChunker"/> class.
    /// </summary>
    /// <param name="tokenizer">The tokenizer to use for tokenization.</param>
    /// <param name="chunkSize">Maximum number of tokens per chunk.</param>
    /// <param name="logger">Optional logger for diagnostic messages.</param>
    public SlumberChunker(
        ITokenizer tokenizer,
        int chunkSize = 2048,
        ILogger<SlumberChunker>? logger = null)
        : base(tokenizer, logger)
    {
        if (chunkSize <= 0) throw new ArgumentException("chunk_size must be greater than 0", nameof(chunkSize));
        ChunkSize = chunkSize;
        _fallback = new RecursiveChunker(tokenizer, chunkSize);
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
    public override string ToString() => $"SlumberChunker(chunk_size={ChunkSize}, mode=fallback)";
}
