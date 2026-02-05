using Chonkie.Core.Types;
using Chonkie.Embeddings.SentenceTransformers;
using Chonkie.Handshakes;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests.Integration;

/// <summary>
/// Integration tests for MongoDBHandshake.
/// These tests require a running MongoDB instance and will be skipped if not available.
/// </summary>
public class MongoDBHandshakeIntegrationTests
{
    [SkippableFact]
    public async Task WriteAsync_WithRealMongoDBAndSentenceTransformers_WritesSuccessfully()
    {
        // Skip if MongoDB is not available
        var isMongoDBAvailable = await IsMongoDBAvailableAsync();
        Skip.If(!isMongoDBAvailable, "MongoDB server not available at localhost:27017");

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake = new MongoDBHandshake(
            embeddingModel: embeddings,
            hostname: "localhost",
            port: 27017
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
            // Cleanup is handled by unique database names
        }
    }

    [SkippableFact]
    public async Task SearchAsync_WithRealMongoDB_FindsSimilarChunks()
    {
        // Skip if MongoDB is not available
        var isMongoDBAvailable = await IsMongoDBAvailableAsync();
        Skip.If(!isMongoDBAvailable, "MongoDB server not available at localhost:27017");

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake = new MongoDBHandshake(
            embeddingModel: embeddings,
            hostname: "localhost",
            port: 27017
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
            // Cleanup is handled by unique database names
        }
    }

    [SkippableFact]
    public async Task WriteAsync_WithRandomDatabaseName_CreatesUniqueDatabases()
    {
        // Skip if MongoDB is not available
        var isMongoDBAvailable = await IsMongoDBAvailableAsync();
        Skip.If(!isMongoDBAvailable, "MongoDB server not available at localhost:27017");

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake1 = new MongoDBHandshake(
            embeddingModel: embeddings,
            hostname: "localhost",
            port: 27017,
            databaseName: "random"
        );

        var handshake2 = new MongoDBHandshake(
            embeddingModel: embeddings,
            hostname: "localhost",
            port: 27017,
            databaseName: "random"
        );

        var chunks = new[] { new Chunk { Text = "Test", StartIndex = 0, EndIndex = 4, TokenCount = 1 } };

        try
        {
            // Act & Assert
            var result1 = await handshake1.WriteAsync(chunks);
            var result2 = await handshake2.WriteAsync(chunks);

            result1.ShouldNotBeNull();
            result2.ShouldNotBeNull();
            
            // Database names should be different
            handshake1.ToString().ShouldNotBe(handshake2.ToString());
        }
        finally
        {
            // Cleanup is handled by unique database names
        }
    }

    /// <summary>
    /// Checks if MongoDB server is available.
    /// </summary>
    private static async Task<bool> IsMongoDBAvailableAsync()
    {
        try
        {
            var client = new MongoDB.Driver.MongoClient("mongodb://localhost:27017");
            var admin = client.GetDatabase("admin");
            var result = await admin.RunCommandAsync<MongoDB.Bson.BsonDocument>(
                new MongoDB.Bson.BsonDocument("ping", 1));
            return result != null;
        }
        catch
        {
            return false;
        }
    }
}
