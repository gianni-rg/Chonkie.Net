using Chonkie.Core.Types;
using Chonkie.Embeddings.SentenceTransformers;
using Chonkie.Handshakes;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests.Integration;

/// <summary>
/// Integration tests for TurbopufferHandshake.
/// These tests require Turbopuffer API credentials and will be skipped if not available.
/// </summary>
public class TurbopufferHandshakeIntegrationTests
{
    [SkippableFact]
    public async Task WriteAsync_WithRealTurbopufferAndSentenceTransformers_WritesSuccessfully()
    {
        // Skip if API key is missing
        var apiKey = Environment.GetEnvironmentVariable("TURBOPUFFER_API_KEY");
        Skip.If(string.IsNullOrEmpty(apiKey), "TURBOPUFFER_API_KEY environment variable not set");

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake = new TurbopufferHandshake(
            apiKey: apiKey,
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
        catch (Exception ex) when (ex.Message.Contains("401") || ex.Message.Contains("authentication"))
        {
            // Skip if authentication fails
            Skip.If(true, "Turbopuffer API key authentication failed");
        }
    }

    [SkippableFact]
    public async Task SearchAsync_WithRealTurbopuffer_FindsSimilarChunks()
    {
        // Skip if API key is missing
        var apiKey = Environment.GetEnvironmentVariable("TURBOPUFFER_API_KEY");
        Skip.If(string.IsNullOrEmpty(apiKey), "TURBOPUFFER_API_KEY environment variable not set");

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake = new TurbopufferHandshake(
            apiKey: apiKey,
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
            }
        }
        catch (Exception ex) when (ex.Message.Contains("401") || ex.Message.Contains("authentication"))
        {
            // Skip if authentication fails
            Skip.If(true, "Turbopuffer API key authentication failed");
        }
    }

    [SkippableFact]
    public async Task WriteAsync_WithRandomNamespace_CreatesUniqueNamespaces()
    {
        // Skip if API key is missing
        var apiKey = Environment.GetEnvironmentVariable("TURBOPUFFER_API_KEY");
        Skip.If(string.IsNullOrEmpty(apiKey), "TURBOPUFFER_API_KEY environment variable not set");

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        Skip.If(!Directory.Exists(modelPath), $"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake1 = new TurbopufferHandshake(
            apiKey: apiKey,
            namespaceName: "random",
            embeddingModel: embeddings
        );

        var handshake2 = new TurbopufferHandshake(
            apiKey: apiKey,
            namespaceName: "random",
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

            // Namespace names should be different
            handshake1.ToString().ShouldNotBe(handshake2.ToString());
        }
        catch (Exception ex) when (ex.Message.Contains("401") || ex.Message.Contains("authentication"))
        {
            // Skip if authentication fails
            Skip.If(true, "Turbopuffer API key authentication failed");
        }
    }
}
