namespace Chonkie.Core.Interfaces;

using Chonkie.Core.Types;

/// <summary>
/// Interface for all chunker implementations that split text into manageable chunks.
/// </summary>
public interface IChunker
{
    /// <summary>
    /// Chunks a single text into a list of chunks.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    /// <returns>A read-only list of chunks.</returns>
    IReadOnlyList<Chunk> Chunk(string text);

    /// <summary>
    /// Chunks multiple texts in batch, optionally reporting progress.
    /// </summary>
    /// <param name="texts">The texts to chunk.</param>
    /// <param name="progress">Optional progress reporter for tracking completion.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A read-only list of chunk lists, one for each input text.</returns>
    IReadOnlyList<IReadOnlyList<Chunk>> ChunkBatch(
        IEnumerable<string> texts,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Chunks a document and populates its Chunks collection.
    /// </summary>
    /// <param name="document">The document to chunk.</param>
    /// <returns>The document with populated chunks.</returns>
    Document ChunkDocument(Document document);
}
