namespace Chonkie.Chunkers;

using Chonkie.Core.Chunker;
using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;
using Microsoft.Extensions.Logging;

/// <summary>
/// Chunker that splits text into chunks of a specified token size with optional overlap.
/// </summary>
public class TokenChunker : BaseChunker
{
    /// <summary>
    /// Gets the maximum number of tokens per chunk.
    /// </summary>
    public int ChunkSize { get; }

    /// <summary>
    /// Gets the number of tokens to overlap between chunks.
    /// </summary>
    public int ChunkOverlap { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenChunker"/> class.
    /// </summary>
    /// <param name="tokenizer">The tokenizer to use for encoding and decoding.</param>
    /// <param name="chunkSize">Maximum number of tokens per chunk.</param>
    /// <param name="chunkOverlap">Number of tokens to overlap between chunks (can be int or fraction).</param>
    /// <param name="logger">Optional logger instance.</param>
    /// <exception cref="ArgumentException">Thrown when chunkSize is less than or equal to 0 or chunkOverlap is greater than or equal to chunkSize.</exception>
    public TokenChunker(
        ITokenizer tokenizer,
        int chunkSize = 2048,
        int chunkOverlap = 0,
        ILogger? logger = null)
        : base(tokenizer, logger)
    {
        if (chunkSize <= 0)
            throw new ArgumentException("chunk_size must be positive", nameof(chunkSize));

        if (chunkOverlap >= chunkSize)
            throw new ArgumentException("chunk_overlap must be less than chunk_size", nameof(chunkOverlap));

        if (chunkOverlap < 0)
            throw new ArgumentException("chunk_overlap must be non-negative", nameof(chunkOverlap));

        ChunkSize = chunkSize;
        ChunkOverlap = chunkOverlap;

        // TokenChunker uses sequential batch processing for better efficiency
        UseParallelProcessing = false;

        Logger.LogDebug("TokenChunker initialized with chunk_size={ChunkSize}, chunk_overlap={ChunkOverlap}",
            chunkSize, chunkOverlap);
    }

    /// <summary>
    /// Overload constructor that accepts chunk overlap as a fraction of chunk size.
    /// </summary>
    /// <param name="tokenizer">The tokenizer to use.</param>
    /// <param name="chunkSize">Maximum number of tokens per chunk.</param>
    /// <param name="chunkOverlapFraction">Fraction of chunk_size to use as overlap (0.0 to 1.0).</param>
    /// <param name="logger">Optional logger instance.</param>
    public TokenChunker(
        ITokenizer tokenizer,
        int chunkSize,
        double chunkOverlapFraction,
        ILogger? logger = null)
        : this(tokenizer, chunkSize, (int)(chunkOverlapFraction * chunkSize), logger)
    {
        if (chunkOverlapFraction < 0.0 || chunkOverlapFraction >= 1.0)
            throw new ArgumentException("chunk_overlap_fraction must be between 0.0 and 1.0", nameof(chunkOverlapFraction));
    }

    /// <summary>
    /// Split text into overlapping chunks of specified token size.
    /// </summary>
    /// <param name="text">Input text to be chunked.</param>
    /// <returns>List of Chunk objects containing the chunked text and metadata.</returns>
    public override IReadOnlyList<Chunk> Chunk(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Logger.LogDebug("Empty or whitespace text provided");
            return Array.Empty<Chunk>();
        }

        Logger.LogDebug("Chunking text of length {Length} with chunk_size={ChunkSize}",
            text.Length, ChunkSize);

        // Encode full text
        var textTokens = Tokenizer.Encode(text);

        // Generate token groups
        var tokenGroups = GenerateTokenGroups(textTokens);
        var tokenCounts = tokenGroups.Select(g => g.Count).ToList();

        // Decode token groups into chunk texts
        var chunkTexts = Tokenizer.DecodeBatch(tokenGroups);

        // Create chunks
        var chunks = CreateChunks(chunkTexts, tokenGroups, tokenCounts);

        Logger.LogInformation("Created {ChunkCount} chunks from {TokenCount} tokens",
            chunks.Count, textTokens.Count);

        return chunks;
    }

    /// <summary>
    /// Generates token groups from a token sequence with overlap.
    /// </summary>
    private List<IReadOnlyList<int>> GenerateTokenGroups(IReadOnlyList<int> tokens)
    {
        var tokenGroups = new List<IReadOnlyList<int>>();
        var step = ChunkSize - ChunkOverlap;

        for (int start = 0; start < tokens.Count; start += step)
        {
            var end = Math.Min(start + ChunkSize, tokens.Count);
            var group = tokens.Skip(start).Take(end - start).ToList();
            tokenGroups.Add(group);

            if (end == tokens.Count)
                break;
        }

        return tokenGroups;
    }

    /// <summary>
    /// Creates chunks from decoded texts and token groups.
    /// </summary>
    private List<Chunk> CreateChunks(
        IReadOnlyList<string> chunkTexts,
        IReadOnlyList<IReadOnlyList<int>> tokenGroups,
        IReadOnlyList<int> tokenCounts)
    {
        // Calculate overlap lengths for index calculation
        var overlapLengths = new int[tokenGroups.Count];

        if (ChunkOverlap > 0)
        {
            // Get overlap texts for all chunks except the last one
            var overlapTexts = new List<IReadOnlyList<int>>();
            for (int i = 0; i < tokenGroups.Count; i++)
            {
                var group = tokenGroups[i];
                var overlapSize = Math.Min(ChunkOverlap, group.Count);
                var overlapTokens = group.Skip(group.Count - overlapSize).ToList();
                overlapTexts.Add(overlapTokens);
            }

            var decodedOverlaps = Tokenizer.DecodeBatch(overlapTexts);
            for (int i = 0; i < decodedOverlaps.Count; i++)
            {
                overlapLengths[i] = decodedOverlaps[i].Length;
            }
        }

        // Create chunks with proper indices
        var chunks = new List<Chunk>();
        var currentIndex = 0;

        for (int i = 0; i < chunkTexts.Count; i++)
        {
            var chunkText = chunkTexts[i];
            var startIndex = currentIndex;
            var endIndex = startIndex + chunkText.Length;

            chunks.Add(new Chunk
            {
                Text = chunkText,
                StartIndex = startIndex,
                EndIndex = endIndex,
                TokenCount = tokenCounts[i]
            });

            // Move current index forward, accounting for overlap (except for last chunk)
            if (i < chunkTexts.Count - 1)
            {
                currentIndex = endIndex - overlapLengths[i];
            }
        }

        return chunks;
    }

    /// <summary>
    /// Returns a string representation of this TokenChunker.
    /// </summary>
    public override string ToString()
    {
        return $"TokenChunker(chunk_size={ChunkSize}, chunk_overlap={ChunkOverlap})";
    }
}
