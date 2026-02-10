namespace Chonkie.Handshakes;

/// <summary>
/// Configuration options for <see cref="PgvectorHandshake"/>.
/// </summary>
public sealed class PgvectorHandshakeOptions
{
    /// <summary>
    /// PostgreSQL host name. Defaults to "localhost".
    /// </summary>
    public string Host { get; init; } = "localhost";

    /// <summary>
    /// PostgreSQL port. Defaults to 5432.
    /// </summary>
    public int Port { get; init; } = 5432;

    /// <summary>
    /// Database name. Defaults to "postgres".
    /// </summary>
    public string Database { get; init; } = "postgres";

    /// <summary>
    /// Database username. Defaults to "postgres".
    /// </summary>
    public string Username { get; init; } = "postgres";

    /// <summary>
    /// Database password. Defaults to "postgres".
    /// </summary>
    public string Password { get; init; } = "postgres";

    /// <summary>
    /// Optional connection string that overrides individual parameters.
    /// </summary>
    public string? ConnectionString { get; init; }

    /// <summary>
    /// The collection (table) name. Defaults to "chonkie_chunks".
    /// </summary>
    public string CollectionName { get; init; } = "chonkie_chunks";

    /// <summary>
    /// Optional vector dimension override.
    /// </summary>
    public int? VectorDimensions { get; init; }
}
