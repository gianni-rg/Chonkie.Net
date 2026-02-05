using Chonkie.Core.Types;
using Chonkie.Embeddings.SentenceTransformers;
using Chonkie.Handshakes;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests.Integration;

/// <summary>
/// Integration tests for ElasticsearchHandshake.
/// These tests require a running Elasticsearch instance with dense_vector support.
/// </summary>
public class ElasticsearchHandshakeIntegrationTests
{
    private const string ElasticsearchUrl = "http://localhost:9200";

    [SkippableFact]
    public async Task WriteAsync_WithRealElasticsearchAndSentenceTransformers_WritesSuccessfully()
    {
        // Skip if Elasticsearch is not available
        var isElasticsearchAvailable = await IsElasticsearchAvailableAsync();
        Skip.If(!isElasticsearchAvailable, "Elasticsearch server not available at " + ElasticsearchUrl);

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake = new ElasticsearchHandshake(
            embeddingModel: embeddings,
            serverUrl: ElasticsearchUrl
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
            ((bool)resultObj.Success).ShouldBeTrue();
            ((int)resultObj.Count).ShouldBe(2);
        }
        finally
        {
            // Cleanup: Delete the index
            await CleanupIndexAsync(handshake.IndexName);
        }
    }

    [SkippableFact]
    public async Task SearchAsync_WithRealElasticsearch_FindsSimilarChunks()
    {
        // Skip if Elasticsearch is not available
        var isElasticsearchAvailable = await IsElasticsearchAvailableAsync();
        Skip.If(!isElasticsearchAvailable, "Elasticsearch server not available at " + ElasticsearchUrl);

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake = new ElasticsearchHandshake(
            embeddingModel: embeddings,
            serverUrl: ElasticsearchUrl
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
            }
        }
        finally
        {
            // Cleanup
            await CleanupIndexAsync(handshake.IndexName);
        }
    }

    [SkippableFact]
    public async Task WriteAsync_WithRandomIndexName_CreatesUniqueIndices()
    {
        // Skip if Elasticsearch is not available
        var isElasticsearchAvailable = await IsElasticsearchAvailableAsync();
        Skip.If(!isElasticsearchAvailable, "Elasticsearch server not available at " + ElasticsearchUrl);

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake1 = new ElasticsearchHandshake(
            embeddingModel: embeddings,
            serverUrl: ElasticsearchUrl,
            indexName: "random"
        );

        var handshake2 = new ElasticsearchHandshake(
            embeddingModel: embeddings,
            serverUrl: ElasticsearchUrl,
            indexName: "random"
        );

        var chunks = new[] { new Chunk { Text = "Test", StartIndex = 0, EndIndex = 4, TokenCount = 1 } };

        try
        {
            // Act & Assert
            var result1 = await handshake1.WriteAsync(chunks);
            var result2 = await handshake2.WriteAsync(chunks);

            result1.ShouldNotBeNull();
            result2.ShouldNotBeNull();
            
            // Index names should be different
            handshake1.ToString().ShouldNotBe(handshake2.ToString());
        }
        finally
        {
            // Cleanup
            await CleanupIndexAsync(handshake1.IndexName);
            await CleanupIndexAsync(handshake2.IndexName);
        }
    }

    /// <summary>
    /// Checks if Elasticsearch server is available.
    /// </summary>
    private static async Task<bool> IsElasticsearchAvailableAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var response = await client.GetAsync($"{ElasticsearchUrl}/");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Cleans up test index from Elasticsearch.
    /// </summary>
    private static async Task CleanupIndexAsync(string indexName)
    {
        try
        {
            using var client = new HttpClient();
            await client.DeleteAsync($"{ElasticsearchUrl}/{indexName}");
        }
        catch
        {
            // Cleanup errors are non-fatal
        }
    }
}
