using Chonkie.Core.Types;
using Chonkie.Embeddings.SentenceTransformers;
using Chonkie.Handshakes;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests.Integration;

/// <summary>
/// Integration tests for PgvectorHandshake.
/// These tests require a running PostgreSQL instance with pgvector extension enabled.
/// </summary>
public class PgvectorHandshakeIntegrationTests
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=chonkie_test;Username=postgres;Password=postgres";
    private const string TableName = "chunks_integration_test";

    [SkippableFact]
    public async Task WriteAsync_WithRealPostgresAndSentenceTransformers_WritesSuccessfully()
    {
        // Skip if PostgreSQL is not available or pgvector extension is not installed
        var isPostgresAvailable = await IsPostgresAvailableAsync();
        Skip.If(!isPostgresAvailable, "PostgreSQL server not available or pgvector extension not installed");

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake = new PgvectorHandshake(
            connectionString: DefaultConnectionString,
            tableName: TableName,
            embeddingModel: embeddings
        );

        var chunks = new[]
        {
            new Chunk { Text = "The quick brown fox jumps over the lazy dog", StartIndex = 0, EndIndex = 44, TokenCount = 9 },
            new Chunk { Text = "Lorem ipsum dolor sit amet", StartIndex = 45, EndIndex = 71, TokenCount = 5 }
        };

        try
        {
            // Act
            var result = await handshake.WriteAsync(chunks);

            // Assert
            result.ShouldNotBeNull();
            dynamic resultObj = result;
            ((int)resultObj.Count).ShouldBe(2);
        }
        finally
        {
            // Cleanup: Drop the test table
            await CleanupTableAsync();
        }
    }

    [SkippableFact]
    public async Task SearchAsync_WithRealPostgres_FindsSimilarChunks()
    {
        // Skip if PostgreSQL is not available
        var isPostgresAvailable = await IsPostgresAvailableAsync();
        Skip.If(!isPostgresAvailable, "PostgreSQL server not available or pgvector extension not installed");

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake = new PgvectorHandshake(
            connectionString: DefaultConnectionString,
            tableName: TableName,
            embeddingModel: embeddings
        );

        var chunks = new[]
        {
            new Chunk { Text = "Hello world", StartIndex = 0, EndIndex = 11, TokenCount = 2 },
            new Chunk { Text = "Test chunk", StartIndex = 12, EndIndex = 22, TokenCount = 2 }
        };

        try
        {
            // Insert chunks
            await handshake.WriteAsync(chunks);

            // Act
            var results = await handshake.SearchAsync("hello", limit: 5);

            // Assert
            results.ShouldNotBeNull();
            results.Count.ShouldBeGreaterThan(0);
            results.Count.ShouldBeLessThanOrEqualTo(5);

            // Check result structure
            foreach (var result in results)
            {
                result.ShouldContainKey("id");
                result.ShouldContainKey("text");
                result.ShouldContainKey("similarity");
                ((double)result["similarity"]).ShouldBeGreaterThanOrEqualTo(0);
                ((double)result["similarity"]).ShouldBeLessThanOrEqualTo(1);
            }
        }
        finally
        {
            // Cleanup
            await CleanupTableAsync();
        }
    }

    [SkippableFact]
    public async Task WriteAsync_WithRandomTableName_CreatesUniqueTables()
    {
        // Skip if PostgreSQL is not available
        var isPostgresAvailable = await IsPostgresAvailableAsync();
        Skip.If(!isPostgresAvailable, "PostgreSQL server not available or pgvector extension not installed");

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake1 = new PgvectorHandshake(
            connectionString: DefaultConnectionString,
            tableName: "random",
            embeddingModel: embeddings
        );

        var handshake2 = new PgvectorHandshake(
            connectionString: DefaultConnectionString,
            tableName: "random",
            embeddingModel: embeddings
        );

        var chunks = new[] { new Chunk { Text = "Test", StartIndex = 0, EndIndex = 4, TokenCount = 1 } };

        try
        {
            // Act & Assert
            var result1 = await handshake1.WriteAsync(chunks);
            var result2 = await handshake2.WriteAsync(chunks);

            result1.ShouldNotBeNull();
            result2.ShouldNotBeNull();

            // Table names should be different
            handshake1.ToString().ShouldNotBe(handshake2.ToString());
        }
        finally
        {
            // Cleanup would happen here
        }
    }

    /// <summary>
    /// Checks if PostgreSQL server with pgvector is available.
    /// </summary>
    private static async Task<bool> IsPostgresAvailableAsync()
    {
        try
        {
            using var connection = new Npgsql.NpgsqlConnection(DefaultConnectionString);
            await connection.OpenAsync();

            // Check if pgvector extension is available
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM pg_extension WHERE extname = 'vector'";
            var result = await cmd.ExecuteScalarAsync();

            return result != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Cleans up test table from PostgreSQL.
    /// </summary>
    private static async Task CleanupTableAsync()
    {
        try
        {
            using var connection = new Npgsql.NpgsqlConnection(DefaultConnectionString);
            await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"DROP TABLE IF EXISTS {TableName}";
            await cmd.ExecuteNonQueryAsync();
        }
        catch
        {
            // Cleanup errors are non-fatal
        }
    }
}
