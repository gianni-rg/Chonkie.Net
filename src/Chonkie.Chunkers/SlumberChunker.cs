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

    public override IReadOnlyList<Chunk> Chunk(string text)
    {
        Logger.LogInformation("SlumberChunker fallback engaged — using RecursiveChunker until Genie integration is available.");
        return _fallback.Chunk(text);
    }

    public override string ToString() => $"SlumberChunker(chunk_size={ChunkSize}, mode=fallback)";
}
