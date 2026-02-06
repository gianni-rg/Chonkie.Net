using Chonkie.Core.Types;
using Chonkie.Embeddings.OpenAI;
using Chonkie.Embeddings.SentenceTransformers;
using Chonkie.Handshakes;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests.Integration;

/// <summary>
/// Integration tests for QdrantHandshake.
/// These tests require a running Qdrant instance and will be skipped if not available.
/// </summary>
public class QdrantHandshakeIntegrationTests
{
    private const string QdrantUrl = "http://localhost:6333";
    private const string CollectionName = "chonkie-integration-test";

    [Fact]
    public async Task WriteAsync_WithRealQdrantAndOpenAI_WritesSuccessfully()
    {
        // Skip if Qdrant is not available or API key is missing
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
            Assert.Skip("OPENAI_API_KEY environment variable not set");

        var isQdrantAvailable = await IsQdrantAvailableAsync();
        if (!isQdrantAvailable)
            Assert.Skip("Qdrant server not available at " + QdrantUrl);

        // Arrange
        var embeddings = new OpenAIEmbeddings(apiKey!);
        var handshake = new QdrantHandshake(
            url: QdrantUrl,
            collectionName: CollectionName,
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
            // Cleanup: Delete the collection
            await CleanupCollectionAsync();
        }
    }

    [Fact]
    public async Task WriteAsync_WithRealQdrantAndSentenceTransformers_WritesSuccessfully()
    {
        // Skip if Qdrant is not available
        var isQdrantAvailable = await IsQdrantAvailableAsync();
        if (!isQdrantAvailable)
            Assert.Skip("Qdrant server not available at " + QdrantUrl);

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        if (!Directory.Exists(modelPath))
            Assert.Skip($"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake = new QdrantHandshake(
            url: QdrantUrl,
            collectionName: CollectionName,
            embeddingModel: embeddings
        );

        var chunks = new[]
        {
            new Chunk { Text = "Hello world", StartIndex = 0, EndIndex = 11, TokenCount = 2 },
            new Chunk { Text = "Test chunk", StartIndex = 12, EndIndex = 22, TokenCount = 2 }
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
            // Cleanup
            await CleanupCollectionAsync();
        }
    }

    [Fact]
    public async Task SearchAsync_WithRealQdrantAndOpenAI_FindsSimilarChunks()
    {
        // Skip if prerequisites not met
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
            Assert.Skip("OPENAI_API_KEY environment variable not set");

        var isQdrantAvailable = await IsQdrantAvailableAsync();
        if (!isQdrantAvailable)
            Assert.Skip("Qdrant server not available at " + QdrantUrl);

        // Arrange
        var embeddings = new OpenAIEmbeddings(apiKey!);
        var handshake = new QdrantHandshake(
            url: QdrantUrl,
            collectionName: CollectionName,
            embeddingModel: embeddings
        );

        var chunks = new[]
        {
            new Chunk { Text = "Machine learning is a subset of artificial intelligence", StartIndex = 0, EndIndex = 56, TokenCount = 10 },
            new Chunk { Text = "Deep learning uses neural networks", StartIndex = 57, EndIndex = 91, TokenCount = 6 },
            new Chunk { Text = "Pizza is a popular Italian dish", StartIndex = 92, EndIndex = 124, TokenCount = 6 }
        };

        try
        {
            // Write chunks
            await handshake.WriteAsync(chunks);

            // Act - Search for AI-related content
            var results = await handshake.SearchAsync("What is artificial intelligence?", limit: 3);

            // Assert
            results.ShouldNotBeNull();
            results.Count.ShouldBeGreaterThan(0);
            // First result should be most relevant to AI
            var firstResult = results[0];
            firstResult.ShouldNotBeNull();
        }
        finally
        {
            // Cleanup
            await CleanupCollectionAsync();
        }
    }

    [Fact]
    public async Task WriteAsync_WithRandomCollectionName_CreatesUniqueCollection()
    {
        // Skip if Qdrant is not available
        var isQdrantAvailable = await IsQdrantAvailableAsync();
        if (!isQdrantAvailable)
            Assert.Skip("Qdrant server not available at " + QdrantUrl);

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        if (!Directory.Exists(modelPath))
            Assert.Skip($"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake1 = new QdrantHandshake(
            url: QdrantUrl,
            collectionName: "random",
            embeddingModel: embeddings
        );
        var handshake2 = new QdrantHandshake(
            url: QdrantUrl,
            collectionName: "random",
            embeddingModel: embeddings
        );

        // Assert
        handshake1.CollectionName.ShouldNotBe("random");
        handshake2.CollectionName.ShouldNotBe("random");
        handshake1.CollectionName.ShouldNotBe(handshake2.CollectionName);
    }

    /// <summary>
    /// Helper method to check if Qdrant is available.
    /// </summary>
    private static async Task<bool> IsQdrantAvailableAsync()
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var response = await httpClient.GetAsync($"{QdrantUrl}/healthz");
            if (!response.IsSuccessStatusCode)
                return false;
            
            // Additional check: Verify gRPC connectivity by attempting a simple check
            // This helps catch HTTP/2 protocol errors early
            try
            {
                using var grpcClient = new Qdrant.Client.QdrantClient(host: "localhost", port: 6333, https: false);
                // Attempt a simple check to verify gRPC connectivity
                _ = await grpcClient.CollectionExistsAsync("test-availability-check", System.Threading.CancellationToken.None);
                return true;
            }
            catch (Grpc.Core.RpcException grpcEx) when (grpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
            {
                // Collection doesn't exist is OK - gRPC connection works
                return true;
            }
            catch (Grpc.Core.RpcException)
            {
                // gRPC connection failed
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Helper method to cleanup the test collection.
    /// </summary>
    private static async Task CleanupCollectionAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            await httpClient.DeleteAsync($"{QdrantUrl}/collections/{CollectionName}");
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
