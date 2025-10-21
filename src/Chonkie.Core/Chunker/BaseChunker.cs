namespace Chonkie.Core.Chunker;

using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
/// Abstract base class for all chunker implementations providing common functionality.
/// </summary>
public abstract class BaseChunker : IChunker
{
    /// <summary>
    /// Gets the tokenizer used by this chunker.
    /// </summary>
    protected ITokenizer Tokenizer { get; }

    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Gets or sets whether to use parallel processing for batch operations.
    /// </summary>
    protected bool UseParallelProcessing { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseChunker"/> class.
    /// </summary>
    /// <param name="tokenizer">The tokenizer to use for encoding and decoding.</param>
    /// <param name="logger">Optional logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when tokenizer is null.</exception>
    protected BaseChunker(ITokenizer tokenizer, ILogger? logger = null)
    {
        Tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
        Logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Chunks a single text into a list of chunks.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    /// <returns>A read-only list of chunks.</returns>
    public abstract IReadOnlyList<Chunk> Chunk(string text);

    /// <summary>
    /// Chunks multiple texts in batch, optionally reporting progress.
    /// </summary>
    /// <param name="texts">The texts to chunk.</param>
    /// <param name="progress">Optional progress reporter for tracking completion.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A read-only list of chunk lists, one for each input text.</returns>
    public virtual IReadOnlyList<IReadOnlyList<Chunk>> ChunkBatch(
        IEnumerable<string> texts,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var textList = texts.ToList();

        if (textList.Count == 0)
        {
            Logger.LogDebug("Empty text list provided for batch chunking");
            return Array.Empty<IReadOnlyList<Chunk>>();
        }

        if (textList.Count == 1)
        {
            Logger.LogDebug("Single text in batch, processing directly");
            return new[] { Chunk(textList[0]) };
        }

        Logger.LogInformation("Starting batch chunking of {Count} texts", textList.Count);

        if (UseParallelProcessing)
        {
            return ParallelBatchProcessing(textList, progress, cancellationToken);
        }
        else
        {
            return SequentialBatchProcessing(textList, progress, cancellationToken);
        }
    }

    /// <summary>
    /// Chunks a document and populates its Chunks collection.
    /// </summary>
    /// <param name="document">The document to chunk.</param>
    /// <returns>The document with populated chunks.</returns>
    public virtual Document ChunkDocument(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document.Chunks.Any())
        {
            Logger.LogDebug("Re-chunking document with existing {Count} chunks", document.Chunks.Count);

            // Re-chunk existing chunks
            var newChunks = new List<Chunk>();
            foreach (var oldChunk in document.Chunks)
            {
                var subChunks = Chunk(oldChunk.Text);
                foreach (var subChunk in subChunks)
                {
                    newChunks.Add(subChunk with
                    {
                        StartIndex = subChunk.StartIndex + oldChunk.StartIndex,
                        EndIndex = subChunk.EndIndex + oldChunk.StartIndex
                    });
                }
            }

            document.Chunks = newChunks;
        }
        else
        {
            Logger.LogDebug("Chunking document content of length {Length}", document.Content.Length);
            document.Chunks = Chunk(document.Content).ToList();
        }

        Logger.LogInformation("Document chunked into {Count} chunks", document.Chunks.Count);
        return document;
    }

    /// <summary>
    /// Processes texts sequentially with optional progress reporting.
    /// </summary>
    /// <param name="texts">The texts to process.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of chunk lists.</returns>
    protected virtual IReadOnlyList<IReadOnlyList<Chunk>> SequentialBatchProcessing(
        IReadOnlyList<string> texts,
        IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        Logger.LogDebug("Using sequential batch processing for {Count} texts", texts.Count);

        var results = new List<IReadOnlyList<Chunk>>(texts.Count);
        for (int i = 0; i < texts.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            results.Add(Chunk(texts[i]));
            progress?.Report((i + 1) / (double)texts.Count);
        }

        var totalChunks = results.Sum(r => r.Count);
        Logger.LogInformation("Sequential processing complete: {TotalChunks} chunks from {TextCount} texts",
            totalChunks, texts.Count);

        return results;
    }

    /// <summary>
    /// Processes texts in parallel with optional progress reporting.
    /// </summary>
    /// <param name="texts">The texts to process.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of chunk lists.</returns>
    protected virtual IReadOnlyList<IReadOnlyList<Chunk>> ParallelBatchProcessing(
        IReadOnlyList<string> texts,
        IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        Logger.LogDebug("Using parallel batch processing for {Count} texts", texts.Count);

        var results = new IReadOnlyList<Chunk>[texts.Count];
        var processedCount = 0;
        var lockObj = new object();

        Parallel.For(0, texts.Count, new ParallelOptions
        {
            CancellationToken = cancellationToken
        }, i =>
        {
            results[i] = Chunk(texts[i]);

            if (progress != null)
            {
                int currentCount;
                lock (lockObj)
                {
                    currentCount = ++processedCount;
                }
                progress.Report(currentCount / (double)texts.Count);
            }
        });

        var totalChunks = results.Sum(r => r.Count);
        Logger.LogInformation("Parallel processing complete: {TotalChunks} chunks from {TextCount} texts",
            totalChunks, texts.Count);

        return results;
    }
}
