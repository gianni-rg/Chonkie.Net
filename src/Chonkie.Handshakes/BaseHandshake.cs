using Chonkie.Core.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chonkie.Handshakes;

/// <summary>
/// Abstract base class for handshake implementations providing common functionality.
/// Handshakes facilitate writing chunked text to vector databases with unified error handling and logging.
/// </summary>
public abstract class BaseHandshake : IHandshake
{
    /// <summary>
    /// Logger instance for recording handshake operations and errors.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseHandshake"/> class.
    /// </summary>
    /// <param name="logger">Optional logger for recording operations. If null, a null logger is used.</param>
    protected BaseHandshake(ILogger? logger = null)
    {
        Logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Writes chunks to the vector database. This method provides logging and error handling around the concrete implementation.
    /// </summary>
    /// <param name="chunks">The chunks to write.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Database-specific metadata about the write operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chunks"/> is null.</exception>
    public async Task<object> WriteAsync(IEnumerable<Chunk> chunks, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chunks);

        var chunkList = chunks.ToList();
        var chunkCount = chunkList.Count;

        if (chunkCount == 0)
        {
            Logger.LogWarning("WriteAsync called with empty chunk collection");
            return new { Success = true, Count = 0 };
        }

        Logger.LogInformation("Writing {ChunkCount} chunk(s) to {HandshakeName}", chunkCount, GetType().Name);

        try
        {
            var result = await WriteInternalAsync(chunkList, cancellationToken);
            Logger.LogDebug("Successfully wrote {ChunkCount} chunk(s)", chunkCount);
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to write {ChunkCount} chunk(s) to {HandshakeName}", chunkCount, GetType().Name);
            throw new InvalidOperationException(
                $"Failed to write {chunkCount} chunk(s) with {GetType().Name}.",
                ex);
        }
    }

    /// <summary>
    /// Concrete implementation of writing chunks to the vector database.
    /// Override this method to provide database-specific write logic.
    /// </summary>
    /// <param name="chunks">Non-empty list of chunks to write.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Database-specific metadata about the write operation.</returns>
    protected abstract Task<object> WriteInternalAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken);
}
