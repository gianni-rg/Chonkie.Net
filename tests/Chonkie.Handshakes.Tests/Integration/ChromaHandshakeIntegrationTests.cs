using Chonkie.Core.Types;
using Chonkie.Embeddings.SentenceTransformers;
using Chonkie.Handshakes;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests.Integration;

/// <summary>
/// Integration tests for ChromaHandshake.
/// These tests require a running Chroma server and will be skipped if not available.
/// </summary>
public class ChromaHandshakeIntegrationTests
{
    private const string ChromaUrl = "http://localhost:8000";
    private const string CollectionName = "chonkie-integration-test";

    [SkippableFact]
    public async Task WriteAsync_WithRealChromaAndSentenceTransformers_WritesSuccessfully()
    {
        // Skip if Chroma is not available
        var isChromaAvailable = await IsChromaAvailableAsync();
        Skip.If(!isChromaAvailable, "Chroma server not available at " + ChromaUrl);

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake = new ChromaHandshake(
            collectionName: CollectionName,
            embeddingModel: embeddings,
            serverUrl: ChromaUrl
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
            // Cleanup: Delete the collection
            await CleanupCollectionAsync();
        }
    }

    [SkippableFact]
    public async Task SearchAsync_WithRealChroma_FindsSimilarChunks()
    {
        // Skip if Chroma is not available
        var isChromaAvailable = await IsChromaAvailableAsync();
        Skip.If(!isChromaAvailable, "Chroma server not available at " + ChromaUrl);

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake = new ChromaHandshake(
            collectionName: CollectionName,
            embeddingModel: embeddings,
            serverUrl: ChromaUrl
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
            await CleanupCollectionAsync();
        }
    }

    [SkippableFact]
    public async Task WriteAsync_WithRandomCollectionName_CreatesUniqueCollections()
    {
        // Skip if Chroma is not available
        var isChromaAvailable = await IsChromaAvailableAsync();
        Skip.If(!isChromaAvailable, "Chroma server not available at " + ChromaUrl);

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake1 = new ChromaHandshake(
            collectionName: "random",
            embeddingModel: embeddings,
            serverUrl: ChromaUrl
        );

        var handshake2 = new ChromaHandshake(
            collectionName: "random",
            embeddingModel: embeddings,
            serverUrl: ChromaUrl
        );

        var chunks = new[] { new Chunk { Text = "Test", StartIndex = 0, EndIndex = 4, TokenCount = 1 } };

        try
        {
            // Act & Assert
            var result1 = await handshake1.WriteAsync(chunks);
            var result2 = await handshake2.WriteAsync(chunks);

            result1.ShouldNotBeNull();
            result2.ShouldNotBeNull();
            
            // Collection names should be different
            handshake1.ToString().ShouldNotBe(handshake2.ToString());
        }
        finally
        {
            // Cleanup
        }
    }

    /// <summary>
    /// Checks if Chroma server is available.
    /// </summary>
    private static async Task<bool> IsChromaAvailableAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var response = await client.GetAsync($"{ChromaUrl}/api/v1");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Cleans up test collection from Chroma.
    /// </summary>
    private static async Task CleanupCollectionAsync()
    {
        try
        {
            using var client = new HttpClient();
            await client.DeleteAsync($"{ChromaUrl}/api/v1/collections/{CollectionName}");
        }
        catch
        {
            // Cleanup errors are non-fatal
        }
    }
}
