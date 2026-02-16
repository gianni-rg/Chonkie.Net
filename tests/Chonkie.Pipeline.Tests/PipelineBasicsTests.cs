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

namespace Chonkie.Pipeline.Tests;

/// <summary>
/// Tests for basic Pipeline functionality.
/// </summary>
public class PipelineBasicsTests
{
    /// Basics: constructing a pipeline with defaults succeeds.
    [Fact]
    public void Pipeline_CanBeInstantiated()
    {
        // Act
        var pipeline = new Pipeline();

        // Assert
        Assert.NotNull(pipeline);
    }

    /// Basics: ChunkWith registers a chunker and is chainable.
    [Fact]
    public void Pipeline_WithDirectTextInput_ReturnsDocument()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: "This is a test document for chunking.");

        // Assert
        Assert.NotNull(result);
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
        Assert.Equal("This is a test document for chunking.", doc.Content);
    }

    /// Basics: ProcessWith registers a chef and is chainable.
    [Fact]
    public void Pipeline_WithMultipleTexts_ReturnsListOfDocuments()
    {
        // Arrange
        var texts = new[]
        {
            "First document text.",
            "Second document text.",
            "Third document text."
        };

        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var result = pipeline.Run(texts: texts);

        // Assert
        Assert.NotNull(result);
        var docs = Assert.IsAssignableFrom<List<Document>>(result);
        Assert.Equal(3, docs.Count);

        foreach (var doc in docs)
        {
            Assert.NotNull(doc);
            Assert.NotEmpty(doc.Chunks);
        }
    }

    /// Basics: RefineWith registers a refinery and is chainable.
    [Fact]
    public void Pipeline_RequiresChunker_ThrowsException()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ProcessWith("text");

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            pipeline.Run(texts: "test"));
        Assert.Contains("must include a chunker", ex.Message);
    }

    /// Basics: Describe returns a human-readable pipeline summary.
    [Fact]
    public void Pipeline_RequiresInput_ThrowsException()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive");

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            pipeline.Run());
        Assert.Contains("must include a fetcher", ex.Message);
    }

    /// Basics: ToConfig returns serializable configuration.
    [Fact]
    public void Pipeline_WithNoSteps_ThrowsException()
    {
        // Arrange
        var pipeline = new Pipeline();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            pipeline.Run(texts: "test"));
        Assert.Contains("no steps", ex.Message);
    }

    /// Basics: FromConfig reconstructs the pipeline from config.
    [Fact]
    public void Pipeline_Describe_ReturnsReadableString()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ProcessWith("text")
            .ChunkWith("recursive", new { chunk_size = 512 });

        // Act
        var description = pipeline.Describe();

        // Assert
        Assert.NotNull(description);
        Assert.Contains("process", description.ToLower());
        Assert.Contains("chunk", description.ToLower());
    }

    /// Basics: Unknown component registration throws.
    [Fact]
    public void Pipeline_Reset_ClearsAllSteps()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive")
            .ProcessWith("text");

        // Act
        pipeline.Reset();

        // Assert
        var description = pipeline.Describe();
        Assert.Equal("Empty pipeline", description);
    }
}
