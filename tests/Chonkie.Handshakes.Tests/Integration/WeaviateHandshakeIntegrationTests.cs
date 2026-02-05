using Chonkie.Core.Types;
using Chonkie.Embeddings.SentenceTransformers;
using Chonkie.Handshakes;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests.Integration;

/// <summary>
/// Integration tests for WeaviateHandshake.
/// These tests require a running Weaviate instance and will be skipped if not available.
/// </summary>
public class WeaviateHandshakeIntegrationTests
{
    private const string WeaviateUrl = "http://localhost:8080";
    private const string ClassName = "ChonkieIntegrationTest";

    [SkippableFact]
    public async Task WriteAsync_WithRealWeaviateAndSentenceTransformers_WritesSuccessfully()
    {
        // Skip if Weaviate is not available
        var isWeaviateAvailable = await IsWeaviateAvailableAsync();
        Skip.If(!isWeaviateAvailable, "Weaviate server not available at " + WeaviateUrl);

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake = await WeaviateHandshake.CreateCloudAsync(
            url: WeaviateUrl,
            apiKey: "demo-key",
            className: ClassName,
            embeddings: embeddings
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
            ((int)resultObj.Count).ShouldBeGreaterThanOrEqualTo(2);
        }
        finally
        {
            // Cleanup: Delete the class
            await CleanupClassAsync();
        }
    }

    [SkippableFact]
    public async Task SearchAsync_WithRealWeaviate_FindsSimilarChunks()
    {
        // Skip if Weaviate is not available
        var isWeaviateAvailable = await IsWeaviateAvailableAsync();
        Skip.If(!isWeaviateAvailable, "Weaviate server not available at " + WeaviateUrl);

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake = await WeaviateHandshake.CreateCloudAsync(
            url: WeaviateUrl,
            apiKey: "demo-key",
            className: ClassName,
            embeddings: embeddings
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
            await CleanupClassAsync();
        }
    }

    [SkippableFact]
    public async Task WriteAsync_WithRandomClassName_CreatesUniqueClasses()
    {
        // Skip if Weaviate is not available
        var isWeaviateAvailable = await IsWeaviateAvailableAsync();
        Skip.If(!isWeaviateAvailable, "Weaviate server not available at " + WeaviateUrl);

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake1 = await WeaviateHandshake.CreateCloudAsync(
            url: WeaviateUrl,
            apiKey: "demo-key",
            className: "random",
            embeddings: embeddings
        );

        var handshake2 = await WeaviateHandshake.CreateCloudAsync(
            url: WeaviateUrl,
            apiKey: "demo-key",
            className: "random",
            embeddings: embeddings
        );

        var chunks = new[] { new Chunk { Text = "Test", StartIndex = 0, EndIndex = 4, TokenCount = 1 } };

        try
        {
            // Act & Assert
            var result1 = await handshake1.WriteAsync(chunks);
            var result2 = await handshake2.WriteAsync(chunks);

            result1.ShouldNotBeNull();
            result2.ShouldNotBeNull();

            // Class names should be different
            handshake1.ToString().ShouldNotBe(handshake2.ToString());
        }
        finally
        {
            // Cleanup would happen here
        }
    }

    /// <summary>
    /// Checks if Weaviate server is available.
    /// </summary>
    private static async Task<bool> IsWeaviateAvailableAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var response = await client.GetAsync($"{WeaviateUrl}/v1/.well-known/ready");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Cleans up test class from Weaviate.
    /// </summary>
    private static async Task CleanupClassAsync()
    {
        try
        {
            using var client = new HttpClient();
            await client.DeleteAsync($"{WeaviateUrl}/v1/schema/{ClassName}");
        }
        catch
        {
            // Cleanup errors are non-fatal
        }
    }
}
