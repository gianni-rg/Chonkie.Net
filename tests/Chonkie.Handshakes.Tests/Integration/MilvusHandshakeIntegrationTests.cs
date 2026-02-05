using Chonkie.Core.Types;
using Chonkie.Embeddings.SentenceTransformers;
using Chonkie.Handshakes;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests.Integration;

/// <summary>
/// Integration tests for MilvusHandshake.
/// These tests require a running Milvus server and will be skipped if not available.
/// </summary>
public class MilvusHandshakeIntegrationTests
{
    private const string MilvusUrl = "http://localhost:19530";

    
    public async Task WriteAsync_WithRealMilvusAndSentenceTransformers_WritesSuccessfully()
    {
        // Skip if Milvus is not available
        var isMilvusAvailable = await IsMilvusAvailableAsync();
        if (!isMilvusAvailable)
            Assert.Skip("Milvus server not available at " + MilvusUrl);

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        if (!Directory.Exists(modelPath))
            Assert.Skip($"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake = new MilvusHandshake(
            embeddingModel: embeddings,
            serverUrl: MilvusUrl
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
            var count = Convert.ToInt32(resultObj.Count);
            count.ShouldBe(2);
        }
        finally
        {
            // Cleanup: Delete the collection
            await CleanupCollectionAsync(handshake.CollectionName);
        }
    }

    
    public async Task SearchAsync_WithRealMilvus_FindsSimilarChunks()
    {
        // Skip if Milvus is not available
        var isMilvusAvailable = await IsMilvusAvailableAsync();
        if (!isMilvusAvailable)
            Assert.Skip("Milvus server not available at " + MilvusUrl);

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        if (!Directory.Exists(modelPath))
            Assert.Skip($"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake = new MilvusHandshake(
            embeddingModel: embeddings,
            serverUrl: MilvusUrl
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
                var similarity = Convert.ToDouble(result["similarity"]);
                similarity.ShouldBeGreaterThanOrEqualTo(0);
            }
        }
        finally
        {
            // Cleanup
            await CleanupCollectionAsync(handshake.CollectionName);
        }
    }

    
    public async Task WriteAsync_WithRandomCollectionName_CreatesUniqueCollections()
    {
        // Skip if Milvus is not available
        var isMilvusAvailable = await IsMilvusAvailableAsync();
        if (!isMilvusAvailable)
            Assert.Skip("Milvus server not available at " + MilvusUrl);

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        if (!Directory.Exists(modelPath))
            Assert.Skip($"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake1 = new MilvusHandshake(
            embeddingModel: embeddings,
            serverUrl: MilvusUrl,
            collectionName: "random"
        );

        var handshake2 = new MilvusHandshake(
            embeddingModel: embeddings,
            serverUrl: MilvusUrl,
            collectionName: "random"
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
            await CleanupCollectionAsync(handshake1.CollectionName);
            await CleanupCollectionAsync(handshake2.CollectionName);
        }
    }

    /// <summary>
    /// Checks if Milvus server is available.
    /// </summary>
    private static async Task<bool> IsMilvusAvailableAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var response = await client.GetAsync($"{MilvusUrl}/v1/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Cleans up test collection from Milvus.
    /// </summary>
    private static async Task CleanupCollectionAsync(string collectionName)
    {
        try
        {
            using var client = new HttpClient();
            var dropRequest = new { collection_name = collectionName };
            var json = System.Text.Json.JsonSerializer.Serialize(dropRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await client.PostAsync($"{MilvusUrl}/v1/collections/drop", content);
        }
        catch
        {
            // Cleanup errors are non-fatal
        }
    }
}
