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

namespace Chonkie.Core.Tests.Chunkers;

using System;
using System.IO;
using System.Linq;
using Chonkie.Chunkers;
using Chonkie.Core.Types;
using Chonkie.Tokenizers;
using Shouldly;
using Xunit;

/// <summary>
/// Integration tests for NeuralChunker using ONNX models.
///
/// These tests verify that the NeuralChunker correctly loads and uses ONNX models
/// for neural-based token classification chunk detection.
///
/// Prerequisites:
/// - ONNX models must be present in the models/ directory
/// - Supported models: distilbert, modernbert-base, modernbert-large
/// </summary>
public class NeuralChunkerIntegrationTests : IDisposable
{
    /// <summary>
    /// Gets the path to a model directory.
    /// </summary>
    private static string GetModelPath(string modelName)
    {
        // Load models base path from environment variable
        var modelsPath = Environment.GetEnvironmentVariable("CHONKIE_NEURAL_MODEL_PATH");
        if (!string.IsNullOrWhiteSpace(modelsPath))
        {
            return Path.Combine(modelsPath, modelName);
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Checks if a model exists and is properly configured.
    /// </summary>
    private static bool ModelExists(string modelName)
    {
        var modelPath = GetModelPath(modelName);
        if (!Directory.Exists(modelPath))
        {
            return false;
        }

        // Check for required files
        var requiredFiles = new[] { "model.onnx", "config.json", "tokenizer.json" };
        return requiredFiles.All(f => File.Exists(Path.Combine(modelPath, f)));
    }

    // #endregion

    #region ONNX Model Initialization Tests

    [Theory]
    [InlineData("distilbert")]
    [InlineData("modernbert-base")]
    [InlineData("modernbert-large")]
    public void Constructor_WithOnnxModelPath_InitializesSuccessfully(string modelName)
    {
        if (!ModelExists(modelName))
        {
            Assert.Skip($"{modelName} model not available. Check model path and/or run model conversion script first.");
        }

        var tokenizer = new CharacterTokenizer();
        var modelPath = GetModelPath(modelName);

        var chunker = new NeuralChunker(tokenizer, modelPath);

        chunker.UseOnnx.ShouldBeTrue("ONNX model should be loaded");
        chunker.ChunkSize.ShouldBe(2048);
    }

    [Fact]
    public void Constructor_WithInvalidModelPath_FallsBackToRecursiveChunker()
    {
        var tokenizer = new CharacterTokenizer();
        var invalidPath = Path.Combine("nonexistent-model");

        var chunker = new NeuralChunker(tokenizer, invalidPath);

        chunker.UseOnnx.ShouldBeFalse("Should fall back to RecursiveChunker");
    }

    [Theory]
    [InlineData("distilbert")]
    [InlineData("modernbert-base")]
    [InlineData("modernbert-large")]
    public void InitializeOnnxModel_WithValidPath_EnablesOnnx(string modelName)
    {
        if (!ModelExists(modelName))
        {
            Assert.Skip($"{modelName} model not available. Check model path and/or run model conversion script first.");
        }

        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer); // Start in fallback mode
        chunker.UseOnnx.ShouldBeFalse();

        var modelPath = GetModelPath(modelName);
        var result = chunker.InitializeOnnxModel(modelPath);

        result.ShouldBeTrue("Initialization should succeed");
        chunker.UseOnnx.ShouldBeTrue("ONNX should now be enabled");
    }

    [Fact]
    public void InitializeOnnxModel_WithInvalidPath_ReturnsFalse()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer);

        var result = chunker.InitializeOnnxModel("/invalid/path/to/model");

        result.ShouldBeFalse("Initialization should fail with invalid path");
        chunker.UseOnnx.ShouldBeFalse("ONNX should remain disabled");
    }

    #endregion

    #region Chunking with ONNX Tests

    [Theory]
    [InlineData("distilbert")]
    [InlineData("modernbert-base")]
    [InlineData("modernbert-large")]
    public void Chunk_WithOnnxModel_ProducesValidChunks(string modelName)
    {
        if (!ModelExists(modelName))
        {
            Assert.Skip($"{modelName} model not available. Check model path and/or run model conversion script first.");
        }

        var tokenizer = new CharacterTokenizer();
        var modelPath = GetModelPath(modelName);
        var chunker = new NeuralChunker(tokenizer, modelPath);

        var text = "This is a test document. It contains multiple sentences. " +
                   "Each sentence should be analyzed for chunk boundaries. " +
                   "The neural model predicts split points based on token classification. " +
                   "This helps create semantically meaningful chunks.";

        var chunks = chunker.Chunk(text);

        chunks.Count.ShouldBeGreaterThan(0, "Should produce at least one chunk");

        // Verify reconstruction
        var reconstructed = string.Concat(chunks.Select(c => c.Text));
        reconstructed.ShouldBe(text, "Chunks should perfectly reconstruct original text");

        // Verify chunk properties
        foreach (var chunk in chunks)
        {
            chunk.Text.ShouldNotBeNullOrEmpty("Each chunk must have text");
            chunk.StartIndex.ShouldBeGreaterThanOrEqualTo(0);
            chunk.EndIndex.ShouldBeLessThanOrEqualTo(text.Length);
            chunk.StartIndex.ShouldBeLessThan(chunk.EndIndex);

            var sliced = text.Substring(chunk.StartIndex, chunk.EndIndex - chunk.StartIndex);
            sliced.ShouldBe(chunk.Text, "Chunk text should match original indices");
        }
    }

    [Theory]
    [InlineData("distilbert")]
    [InlineData("modernbert-base")]
    [InlineData("modernbert-large")]
    public void Chunk_WithShortText_ProducesSingleChunk(string modelName)
    {
        if (!ModelExists(modelName))
        {
            Assert.Skip($"{modelName} model not available. Check model path and/or run model conversion script first.");
        }

        var tokenizer = new CharacterTokenizer();
        var modelPath = GetModelPath(modelName);
        var chunker = new NeuralChunker(tokenizer, modelPath, chunkSize: 2048);

        var text = "This is a short text.";
        var chunks = chunker.Chunk(text);

        chunks.Count.ShouldBe(1, "Short text should produce single chunk");
        chunks[0].Text.ShouldBe(text);
    }

    [Theory]
    [InlineData("distilbert")]
    [InlineData("modernbert-base")]
    [InlineData("modernbert-large")]
    public void Chunk_WithLongDocument_ProducesMultipleChunks(string modelName)
    {
        if (!ModelExists(modelName))
        {
            Assert.Skip($"{modelName} model not available. Check model path and/or run model conversion script first.");
        }

        var tokenizer = new CharacterTokenizer();
        var modelPath = GetModelPath(modelName);
        var chunker = new NeuralChunker(tokenizer, modelPath, chunkSize: 512);

        var text = string.Join(" ", Enumerable.Range(1, 100)
            .Select(i => $"This is sentence number {i}. It contains some content for testing. " +
                        $"The neural chunker should identify appropriate split points."));

        var chunks = chunker.Chunk(text);

        chunks.Count.ShouldBeGreaterThan(1, "Long text should produce multiple chunks");
        var reconstructed = string.Concat(chunks.Select(c => c.Text));
        reconstructed.ShouldBe(text, "Reconstruction should be complete and accurate");
    }

    [Theory]
    [InlineData("distilbert")]
    [InlineData("modernbert-base")]
    [InlineData("modernbert-large")]
    public void Chunk_WithUnicodeText_PreservesCharactersCorrectly(string modelName)
    {
        if (!ModelExists(modelName))
        {
            Assert.Skip($"{modelName} model not available. Check model path and/or run model conversion script first.");
        }

        var tokenizer = new CharacterTokenizer();
        var modelPath = GetModelPath(modelName);
        var chunker = new NeuralChunker(tokenizer, modelPath);

        var text = "Hello world! 你好世界！ مرحبا العالم Привет мир 🌍";

        var chunks = chunker.Chunk(text);

        var reconstructed = string.Concat(chunks.Select(c => c.Text));
        reconstructed.ShouldBe(text, "Unicode characters should be preserved");
    }

    [Theory]
    [InlineData("distilbert")]
    [InlineData("modernbert-base")]
    [InlineData("modernbert-large")]
    public void Chunk_WithEmojis_PreservesEmojisCorrectly(string modelName)
    {
        if (!ModelExists(modelName))
        {
            Assert.Skip($"{modelName} model not available. Check model path and/or run model conversion script first.");
        }

        var tokenizer = new CharacterTokenizer();
        var modelPath = GetModelPath(modelName);
        var chunker = new NeuralChunker(tokenizer, modelPath);

        var text = "Data science 📊 is awesome! Machine learning 🤖 rocks! " +
                   "Neural networks 🧠 are powerful! 🚀 Let's go!";

        var chunks = chunker.Chunk(text);

        var reconstructed = string.Concat(chunks.Select(c => c.Text));
        reconstructed.ShouldBe(text, "Emojis should be preserved");
    }

    #endregion

    #region Batch Chunking with ONNX Tests

    [Theory]
    [InlineData("distilbert")]
    [InlineData("modernbert-base")]
    [InlineData("modernbert-large")]
    public void ChunkBatch_WithOnnxModel_ProcessesMultipleTexts(string modelName)
    {
        if (!ModelExists(modelName))
        {
            Assert.Skip($"{modelName} model not available. Check model path and/or run model conversion script first.");
        }

        var tokenizer = new CharacterTokenizer();
        var modelPath = GetModelPath(modelName);
        var chunker = new NeuralChunker(tokenizer, modelPath, chunkSize: 512);

        var texts = new[]
        {
            "First document with some content. It contains multiple sentences.",
            "Second document with different content. This is also a test.",
            "Third document here. Contains valuable information for testing."
        };

        var results = chunker.ChunkBatch(texts);

        results.Count.ShouldBe(3, "Should process all input texts");
        foreach (var batch in results)
        {
            batch.Count.ShouldBeGreaterThan(0, "Each text should produce at least one chunk");
        }
    }

    #endregion

    #region Document Chunking with ONNX Tests

    [Theory]
    [InlineData("distilbert")]
    [InlineData("modernbert-base")]
    [InlineData("modernbert-large")]
    public void ChunkDocument_WithOnnxModel_PopulatesChunks(string modelName)
    {
        if (!ModelExists(modelName))
        {
            Assert.Skip($"{modelName} model not available. Check model path and/or run model conversion script first.");
        }

        var tokenizer = new CharacterTokenizer();
        var modelPath = GetModelPath(modelName);
        var chunker = new NeuralChunker(tokenizer, modelPath);

        var document = new Document
        {
            Content = "Document about machine learning. " +
                     "Neural networks are powerful. " +
                     "Deep learning has many applications. " +
                     "Computer vision is one example. " +
                     "Natural language processing is another."
        };

        var result = chunker.ChunkDocument(document);

        result.ShouldBeSameAs(document, "Should return the same document instance");
        result.Chunks.Count.ShouldBeGreaterThan(0, "Document should have chunks");
        foreach (var chunk in result.Chunks)
        {
            chunk.Text.ShouldNotBeNullOrEmpty("Each chunk must have content");
        }
    }

    #endregion

    #region Model Comparison Tests

    [Fact]
    public void MultipleModels_ProduceSimilarChunking()
    {
        if (!ModelExists("distilbert") || !ModelExists("modernbert-base"))
        {
            Assert.Skip("Required models not available for comparison. Check model paths and/or run model conversion scripts first.");
        }

        var tokenizer = new CharacterTokenizer();
        var text = "First sentence here. Second sentence there. " +
                   "Third sentence elsewhere. Fourth sentence finally.";

        var distilbertPath = GetModelPath("distilbert");
        var modernbertPath = GetModelPath("modernbert-base");

        var distilbertChunker = new NeuralChunker(tokenizer, distilbertPath, chunkSize: 512);
        var modernbertChunker = new NeuralChunker(tokenizer, modernbertPath, chunkSize: 512);

        var distilbertChunks = distilbertChunker.Chunk(text);
        var modernbertChunks = modernbertChunker.Chunk(text);

        // Both should successfully chunk the text
        distilbertChunks.Count.ShouldBeGreaterThan(0);
        modernbertChunks.Count.ShouldBeGreaterThan(0);

        // Both should reconstruct perfectly
        var distilbertReconstructed = string.Concat(distilbertChunks.Select(c => c.Text));
        var modernbertReconstructed = string.Concat(modernbertChunks.Select(c => c.Text));

        distilbertReconstructed.ShouldBe(text);
        modernbertReconstructed.ShouldBe(text);

        distilbertChunker.Dispose();
        modernbertChunker.Dispose();
    }

    #endregion

    #region Edge Cases Tests

    [Theory]
    [InlineData("distilbert")]
    [InlineData("modernbert-base")]
    [InlineData("modernbert-large")]
    public void Chunk_WithEmptyString_ReturnsEmptyList(string modelName)
    {
        if (!ModelExists(modelName))
        {
            Assert.Skip($"{modelName} model not available. Check model path and/or run model conversion script first.");
        }

        var tokenizer = new CharacterTokenizer();
        var modelPath = GetModelPath(modelName);
        var chunker = new NeuralChunker(tokenizer, modelPath);

        var chunks = chunker.Chunk("");

        chunks.Count.ShouldBe(0, "Empty text should produce no chunks");
        chunker.Dispose();
    }

    [Theory]
    [InlineData("distilbert")]
    [InlineData("modernbert-base")]
    [InlineData("modernbert-large")]
    public void Chunk_WithWhitespaceOnly_ReturnsEmptyList(string modelName)
    {
        if (!ModelExists(modelName))
        {
            Assert.Skip($"{modelName} model not available. Check model path and/or run model conversion script first.");
        }

        var tokenizer = new CharacterTokenizer();
        var modelPath = GetModelPath(modelName);
        var chunker = new NeuralChunker(tokenizer, modelPath);

        var chunks = chunker.Chunk("   \n\t   ");

        chunks.Count.ShouldBe(0, "Whitespace-only text should produce no chunks");
        chunker.Dispose();
    }

    [Theory]
    [InlineData("distilbert")]
    [InlineData("modernbert-base")]
    [InlineData("modernbert-large")]
    public void Chunk_WithVeryLongText_CompletesSuccessfully(string modelName)
    {
        if (!ModelExists(modelName))
        {
            Assert.Skip($"{modelName} model not available. Check model path and/or run model conversion script first.");
        }

        var tokenizer = new CharacterTokenizer();
        var modelPath = GetModelPath(modelName);
        var chunker = new NeuralChunker(tokenizer, modelPath, chunkSize: 256);

        // Create a very long document
        var sentences = Enumerable.Range(1, 500)
            .Select(i => $"This is test sentence number {i}. ")
            .ToArray();
        var longText = string.Concat(sentences);

        var chunks = chunker.Chunk(longText);

        chunks.Count.ShouldBeGreaterThan(0, "Should handle long documents");
        var reconstructed = string.Concat(chunks.Select(c => c.Text));
        reconstructed.ShouldBe(longText, "Should reconstruct perfectly despite length");

        chunker.Dispose();
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        // Cleanup if needed
        GC.SuppressFinalize(this);
    }

    #endregion
}
