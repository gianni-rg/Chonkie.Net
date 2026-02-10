using Chonkie.Core.Types;

namespace Chonkie.Handshakes;

/// <summary>
/// Defines the contract for writing chunks to vector databases.
/// Handshakes provide a unified interface for ingesting chunked text into various vector stores.
/// </summary>
public interface IHandshake
{
    /// <summary>
    /// Writes a single chunk or a collection of chunks to the vector database.
    /// </summary>
    /// <param name="chunks">The chunk or chunks to write. Can be a single <see cref="Chunk"/> or a collection of chunks.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous write operation. The result contains database-specific metadata about the write operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chunks"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database connection is not properly initialized.</exception>
    Task<object> WriteAsync(IEnumerable<Chunk> chunks, CancellationToken cancellationToken = default);
}
