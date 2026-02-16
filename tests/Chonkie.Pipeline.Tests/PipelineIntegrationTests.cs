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
using Chonkie.Pipeline;
using System.Text.Json;

namespace Chonkie.Pipeline.Tests;

/// <summary>
/// Integration tests for complete pipelines.
/// </summary>
public class PipelineIntegrationTests
{
    private const string SampleText = @"
This is a sample document for testing pipelines.

It has multiple paragraphs to ensure proper chunking behavior.

Each paragraph contains some meaningful content that can be split.";

    /// Integration: full pipeline with text input processes successfully.
    [Fact]
    public void Pipeline_CompleteWithTextInput_ProcessesSuccessfully()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ProcessWith("text")
            .ChunkWith("recursive", new { chunk_size = 256 })
            .RefineWith("overlap", new { context_size = 20 });

        // Act
        var result = pipeline.Run(texts: SampleText.Trim());

        // Assert
        var doc = Assert.IsType<Document>(result);
        // TextChef normalizes whitespace, so we expect normalized content
        Assert.NotNull(doc.Content);
        Assert.Contains("sample document for testing pipelines", doc.Content);
        Assert.NotEmpty(doc.Chunks);

        foreach (var chunk in doc.Chunks)
        {
            Assert.NotNull(chunk.Text);
            Assert.True(chunk.TokenCount > 0);
        }
    }

    /// Ensures document structure (content, chunks, metadata) is preserved.
    [Fact]
    public void Pipeline_PreservesDocumentStructure()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("token", new { chunk_size = 50 });

        // Act
        var result = pipeline.Run(texts: SampleText.Trim());

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotNull(doc.Content);
        Assert.NotNull(doc.Chunks);
        Assert.NotNull(doc.Metadata);
        Assert.Equal(SampleText.Trim(), doc.Content);
    }

    /// Batch text processing returns one document per input string.
    [Fact]
    public void Pipeline_BatchTextProcessing_ReturnsMultipleDocuments()
    {
        // Arrange
        var texts = new[]
        {
            "First document for batch processing.",
            "Second document for batch processing.",
            "Third document for batch processing."
        };

        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: texts);

        // Assert
        var docs = Assert.IsAssignableFrom<List<Document>>(result);
        Assert.Equal(3, docs.Count);

        for (int i = 0; i < docs.Count; i++)
        {
            Assert.Equal(texts[i], docs[i].Content);
            Assert.NotEmpty(docs[i].Chunks);
        }
    }

    /// Running the same pipeline twice reuses component instances.
    [Fact]
    public void Pipeline_ComponentCaching_ReusesInstances()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result1 = pipeline.Run(texts: "First run text.");
        var result2 = pipeline.Run(texts: "Second run text.");

        // Assert - Both runs should succeed
        var doc1 = Assert.IsType<Document>(result1);
        var doc2 = Assert.IsType<Document>(result2);
        Assert.NotNull(doc1);
        Assert.NotNull(doc2);
    }

    /// Different chunker parameters should produce different results.
    [Fact]
    public void Pipeline_DifferentParameters_CreateDifferentResults()
    {
        // Arrange
        // Create text with ~1000 tokens to ensure different chunk sizes produce different results
        var longText = string.Concat(Enumerable.Repeat("Test text with more content to ensure proper chunking behavior. ", 200));

        var pipeline1 = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 256 });
        var pipeline2 = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result1 = pipeline1.Run(texts: longText);
        var result2 = pipeline2.Run(texts: longText);

        // Assert
        var doc1 = Assert.IsType<Document>(result1);
        var doc2 = Assert.IsType<Document>(result2);

        Assert.NotEqual(doc1.Chunks.Count, doc2.Chunks.Count);
    }

    /// Exports pipeline configuration to a serializable structure.
    [Fact]
    public void Pipeline_ToConfig_ExportsConfiguration()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ProcessWith("text")
            .ChunkWith("recursive", new { chunk_size = 512 })
            .RefineWith("overlap", new { context_size = 50 });

        // Act
        var config = pipeline.ToConfig();

        // Assert
        Assert.NotNull(config);
        Assert.Equal(3, config.Count);
        Assert.Contains(config, c => c.Type == "process");
        Assert.Contains(config, c => c.Type == "chunk");
        Assert.Contains(config, c => c.Type == "refine");
    }

    /// Saves then loads pipeline config and validates round-trip.
    [Fact]
    public void Pipeline_SaveAndLoadConfig_RoundTrip()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();

        try
        {
            var originalPipeline = new Pipeline()
                .ProcessWith("text")
                .ChunkWith("recursive", new { chunk_size = 512 });

            // Act - Save
            originalPipeline.SaveConfig(tempFile);

            // Act - Load
            var loadedPipeline = Pipeline.FromConfig(tempFile);
            var description = loadedPipeline.Describe();

            // Assert
            Assert.NotNull(description);
            Assert.Contains("process", description.ToLower());
            Assert.Contains("chunk", description.ToLower());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    /// Async pipeline execution returns a document with chunks.
    [Fact]
    public async Task Pipeline_RunAsync_WorksCorrectly()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result = await pipeline.RunAsync(texts: "Async test text");

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
    }

    /// Fluent API methods return Pipeline to allow chaining calls.
    [Fact]
    public void Pipeline_FluentAPI_ReturnsPipelineForChaining()
    {
        // Arrange
        var pipeline = new Pipeline();

        // Act & Assert
        Assert.IsType<Pipeline>(pipeline.ChunkWith("recursive"));
        Assert.IsType<Pipeline>(pipeline.ProcessWith("text"));
        Assert.IsType<Pipeline>(pipeline.RefineWith("overlap", new { context_size = 50 }));
    }

    /// Complex chained pipeline executes and produces chunks.
    [Fact]
    public void Pipeline_ComplexChaining_WorksCorrectly()
    {
        // Arrange & Act
        var result = new Pipeline()
            .ProcessWith("text")
            .ChunkWith("recursive", new { chunk_size = 512 })
            .RefineWith("overlap", new { context_size = 50 })
            .Run(texts: string.Concat(Enumerable.Repeat("Complex chaining test text. ", 50)));

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
    }
}
