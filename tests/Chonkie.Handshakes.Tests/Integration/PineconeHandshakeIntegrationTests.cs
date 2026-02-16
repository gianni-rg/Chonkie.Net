// Copyright 2025-2026 Gianni Rosa Gallina and Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Chonkie.Core.Types;
using Chonkie.Embeddings.SentenceTransformers;
using Chonkie.Handshakes;
using Pinecone;
using Shouldly;
using System.Linq;
using Xunit;

namespace Chonkie.Handshakes.Tests.Integration;

/// <summary>
/// Integration tests for PineconeHandshake.
/// These tests require Pinecone API credentials and will be skipped if not available.
/// </summary>
public class PineconeHandshakeIntegrationTests
{
    private const string IndexName = "chonkie-integration-test";

    [Fact]
    public async Task WriteAsync_WithRealPineconeAndSentenceTransformers_WritesSuccessfully()
    {
        // Skip if API key is missing
        var apiKey = Environment.GetEnvironmentVariable("PINECONE_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
            Assert.Skip("PINECONE_API_KEY environment variable not set");

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        if (!Directory.Exists(modelPath))
            Assert.Skip($"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake = new PineconeHandshake(
            apiKey: apiKey!,
            indexName: IndexName,
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
        catch (Exception ex) when (ex.Message.Contains("index") || ex.Message.Contains("404"))
        {
            // Skip if index doesn't exist
            Assert.Skip("Pinecone index not available or not properly configured");
        }
    }

    [Fact]
    public async Task SearchAsync_WithRealPinecone_FindsSimilarChunks()
    {
        // Skip if API key is missing
        var apiKey = Environment.GetEnvironmentVariable("PINECONE_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
            Assert.Skip("PINECONE_API_KEY environment variable not set");

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        if (!Directory.Exists(modelPath))
            Assert.Skip($"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake = new PineconeHandshake(
            apiKey: apiKey!,
            indexName: IndexName,
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
            results.Matches.ShouldNotBeNull();
            var matches = results.Matches?.ToList() ?? new List<ScoredVector>();
            var matchCount = matches.Count;
            matchCount.ShouldBeGreaterThan(0);
            matchCount.ShouldBeLessThanOrEqualTo(5);

            // Check result structure
            if (matches.Count > 0)
            {
                foreach (var match in matches)
                {
                    match.Metadata.ShouldNotBeNull();
                    match.Metadata.ShouldContainKey("text");
                    match.Metadata.ShouldContainKey("start_index");
                    match.Metadata.ShouldContainKey("end_index");
                    match.Metadata.ShouldContainKey("token_count");
                    var score = match.Score is null ? 0f : Convert.ToSingle(match.Score);
                    score.ShouldBeGreaterThanOrEqualTo(0f);
                }
            }
        }
        catch (Exception ex) when (ex.Message.Contains("index") || ex.Message.Contains("404"))
        {
            // Skip if index doesn't exist
            Assert.Skip("Pinecone index not available or not properly configured");
        }
    }

    [Fact]
    public async Task WriteAsync_WithRandomNamespace_CreatesUniqueNamespaces()
    {
        // Skip if API key is missing
        var apiKey = Environment.GetEnvironmentVariable("PINECONE_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
            Assert.Skip("PINECONE_API_KEY environment variable not set");

        // Check if model directory exists
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "models", "all-MiniLM-L12-v2");
        if (!Directory.Exists(modelPath))
            Assert.Skip($"Model directory not found at {modelPath}");

        // Arrange
        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        var handshake1 = new PineconeHandshake(
            apiKey: apiKey!,
            indexName: IndexName,
            embeddingModel: embeddings,
            @namespace: "random"
        );

        var handshake2 = new PineconeHandshake(
            apiKey: apiKey!,
            indexName: IndexName,
            embeddingModel: embeddings,
            @namespace: "random"
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
        catch (Exception ex) when (ex.Message.Contains("index") || ex.Message.Contains("404"))
        {
            // Skip if index doesn't exist
            Assert.Skip("Pinecone index not available or not properly configured");
        }
    }
}
