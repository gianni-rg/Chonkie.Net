namespace Chonkie.Chunkers;

using Chonkie.Core.Chunker;
using Chonkie.Core.Interfaces;
using Chonkie.Core.Pipeline;
using Chonkie.Core.Types;
using Microsoft.Extensions.Logging;

/// <summary>
/// FastChunker - A lightweight, high-performance chunker.
/// 
/// FastChunker prioritizes speed over semantic accuracy, using simple
/// character-based splitting with optional word-boundary preservation.
/// Ideal for rapid prototyping and performance-critical applications.
/// 
/// Properly handles UTF-8 multi-byte characters (emojis, CJK, etc.) by
/// working with character positions rather than byte offsets.
/// 
/// Note: FastChunker does not require a tokenizer as it uses character-based splitting.
/// </summary>
[PipelineComponent("fast", ComponentType.Chunker)]
public class FastChunker : IChunker
{
    /// <summary>
    /// Gets the target size of each chunk in characters.
    /// </summary>
    public int ChunkSize { get; }

    /// <summary>
    /// Gets the number of overlapping characters between chunks.
    /// </summary>
    public int ChunkOverlap { get; }

    /// <summary>
    /// Initializes a new instance of the FastChunker class.
    /// </summary>
    /// <param name="chunkSize">Target size of each chunk in characters. Default is 512.</param>
    /// <param name="chunkOverlap">Number of overlapping characters between chunks. Default is 0.</param>
    /// <exception cref="ArgumentException">Thrown when chunk_size is not positive or overlap is invalid.</exception>
    public FastChunker(int chunkSize = 512, int chunkOverlap = 0)
    {
        if (chunkSize <= 0)
        {
            throw new ArgumentException("Chunk size must be positive.", nameof(chunkSize));
        }

        if (chunkOverlap < 0)
        {
            throw new ArgumentException("Chunk overlap cannot be negative.", nameof(chunkOverlap));
        }

        if (chunkOverlap >= chunkSize)
        {
            throw new ArgumentException("Chunk overlap must be less than chunk size.", nameof(chunkOverlap));
        }

        ChunkSize = chunkSize;
        ChunkOverlap = chunkOverlap;
    }

    /// <summary>
    /// Chunks a single text string into fixed-size chunks.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    /// <returns>A read-only list of chunks.</returns>
    public IReadOnlyList<Chunk> Chunk(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Array.Empty<Chunk>();
        }

        var chunks = new List<Chunk>();
        var position = 0;
        var chunkIndex = 0;

        while (position < text.Length)
        {
            // Calculate chunk end (don't exceed text length)
            var chunkEnd = Math.Min(position + ChunkSize, text.Length);

            // If not at the end of text, try to find a word boundary
            if (chunkEnd < text.Length)
            {
                // Find last space before chunkEnd (search backwards within a reasonable window)
                var newEnd = FindWordBoundaryBackwards(text, chunkEnd);
                if (newEnd > position) // Only use if it's after current position
                {
                    chunkEnd = newEnd;
                }
            }

            // Extract chunk text
            var chunkText = text[position..chunkEnd];

            chunks.Add(new Chunk
            {
                Id = $"fast_{chunkIndex}",
                Text = chunkText,
                StartIndex = position,
                EndIndex = chunkEnd,
            });

            // Move position forward by chunk size minus overlap
            var nextPosition = chunkEnd - ChunkOverlap;
            
            // Prevent infinite loop: if overlap calculation doesn't advance us, exit loop
            if (nextPosition <= position)
            {
                break; // Can't maintain proper overlap at text end, exit chunking
            }
            
            position = nextPosition;
            chunkIndex++;
        }

        return chunks.AsReadOnly();
    }

    /// <summary>
    /// Chunks multiple texts in batch.
    /// </summary>
    /// <param name="texts">The texts to chunk.</param>
    /// <param name="progress">Optional progress reporter for tracking completion.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A read-only list of chunk lists, one for each input text.</returns>
    public IReadOnlyList<IReadOnlyList<Chunk>> ChunkBatch(
        IEnumerable<string> texts,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var textList = texts.ToList();
        var result = new List<IReadOnlyList<Chunk>>();

        for (int i = 0; i < textList.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result.Add(Chunk(textList[i]));
            progress?.Report((double)(i + 1) / textList.Count);
        }

        return result.AsReadOnly();
    }

    /// <summary>
    /// Chunks a document and populates its Chunks collection.
    /// </summary>
    /// <param name="document">The document to chunk.</param>
    /// <returns>The document with populated chunks.</returns>
    public Document ChunkDocument(Document document)
    {
        var chunks = Chunk(document.Content);
        document.Chunks = new List<Chunk>(chunks);
        return document;
    }

    /// <summary>
    /// Finds the nearest word boundary (space) just before the given position.
    /// Searches backwards from the position looking for whitespace.
    /// If no space is found, returns the position itself.
    /// </summary>
    /// <param name="text">The text to search.</param>
    /// <param name="position">The position to search backwards from.</param>
    /// <returns>The position of the last whitespace before the given position, or position if not found.</returns>
    private static int FindWordBoundaryBackwards(string text, int position)
    {
        // Search backwards from position for a space
        for (int i = position - 1; i >= 0; i--)
        {
            if (char.IsWhiteSpace(text[i]))
            {
                // Return the position after the whitespace
                return i + 1;
            }
        }

        // If no space found, return the original position
        return position;
    }

    /// <summary>
    /// Returns a string representation of the FastChunker.
    /// </summary>
    /// <returns>A string representation showing the chunker parameters.</returns>
    public override string ToString()
    {
        return $"FastChunker(ChunkSize={ChunkSize}, ChunkOverlap={ChunkOverlap})";
    }
}
