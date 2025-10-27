namespace Chonkie.Chunkers;

using Chonkie.Core.Chunker;
using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;
using Microsoft.Extensions.Logging;

/// <summary>
/// Placeholder for a neural model-driven chunker. Until ONNX models are provided,
/// this class delegates to RecursiveChunker and logs a notice.
/// </summary>
public class NeuralChunker : BaseChunker
{
    private readonly RecursiveChunker _fallback;

    /// <summary>
    /// Maximum number of tokens per chunk.
    /// </summary>
    public int ChunkSize { get; }

    public NeuralChunker(
        ITokenizer tokenizer,
        int chunkSize = 2048,
        ILogger<NeuralChunker>? logger = null)
        : base(tokenizer, logger)
    {
        if (chunkSize <= 0) throw new ArgumentException("chunk_size must be greater than 0", nameof(chunkSize));
        ChunkSize = chunkSize;
        _fallback = new RecursiveChunker(tokenizer, chunkSize);
    }

    public override IReadOnlyList<Chunk> Chunk(string text)
    {
        Logger.LogInformation("NeuralChunker fallback engaged — using RecursiveChunker until ONNX model is configured.");
        return _fallback.Chunk(text);
    }

    public override string ToString() => $"NeuralChunker(chunk_size={ChunkSize}, mode=fallback)";
}
