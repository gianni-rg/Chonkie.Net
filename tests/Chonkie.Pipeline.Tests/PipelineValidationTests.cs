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
/// Tests for Pipeline step ordering and validation.
/// </summary>
public class PipelineValidationTests
{
    /// Validates the pipeline reorders steps to CHOMP order automatically.
    [Fact]
    public void Pipeline_ReordersSteps_AccordingToCHOMP()
    {
        // Arrange - Add steps in wrong order
        var pipeline = new Pipeline()
            .ChunkWith("recursive", new { chunk_size = 512 })  // Should be after process
            .ProcessWith("text");  // Should be before chunk

        // Act
        var result = pipeline.Run(texts: "Test text");

        // Assert - Should succeed with proper ordering
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
    }

    /// Ensures refinery runs after chunker even if defined first.
    [Fact]
    public void Pipeline_RefineryRunsAfterChunker_EvenIfDefinedFirst()
    {
        // Arrange
        var text = string.Concat(Enumerable.Repeat("Test text for ordering. ", 20));
        var pipeline = new Pipeline()
            .RefineWith("overlap", new { context_size = 50 })  // Defined first
            .ChunkWith("recursive", new { chunk_size = 512 }); // Should run first

        // Act
        var result = pipeline.Run(texts: text);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
    }

    /// Validates that a chunker step is required to run the pipeline.
    [Fact]
    public void Pipeline_Validation_RequiresChunker()
    {
        // Arrange
        var pipeline = new Pipeline().ProcessWith("text");

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            pipeline.Run(texts: "test"));
        Assert.Contains("must include a chunker", ex.Message);
    }

    /// Validates that adding multiple process (chef) steps is rejected.
    [Fact]
    public void Pipeline_Validation_RejectsMultipleChefs()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ProcessWith("text")
            .ProcessWith("markdown")
            .ChunkWith("recursive");

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            pipeline.Run(texts: "test"));
        Assert.Contains("Multiple process steps", ex.Message);
    }

    /// Validates that either a fetcher or direct text input is required.
    [Fact]
    public void Pipeline_Validation_RequiresFetcherOrTextInput()
    {
        // Arrange
        var pipeline = new Pipeline().ChunkWith("recursive");

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            pipeline.Run());
        Assert.Contains("must include a fetcher", ex.Message);
    }

    /// Verifies that when both fetcher and text are provided, text input is used.
    [Fact]
    public void Pipeline_WithFetcherAndTextInput_UsesTextInput()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "File content that should be ignored");

        try
        {
            var pipeline = new Pipeline()
                .FetchFrom("file", new { path = tempFile })
                .ChunkWith("recursive");

            // Act
            var result = pipeline.Run(texts: "Direct text input");

            // Assert
            var doc = Assert.IsType<Document>(result);
            Assert.Equal("Direct text input", doc.Content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    /// Verifies Describe() shows CHOMP order: process -> chunk -> refine.
    [Fact]
    public void Pipeline_Describe_ShowsCHOMPOrder()
    {
        // Arrange
        var pipeline = new Pipeline()
            .ChunkWith("recursive")  // Wrong order
            .RefineWith("overlap")   // Wrong order
            .ProcessWith("text");    // Wrong order

        // Act
        var description = pipeline.Describe();

        // Assert
        Assert.NotNull(description);
        // Should show CHOMP order: process -> chunk -> refine
        var processIndex = description.IndexOf("process", StringComparison.OrdinalIgnoreCase);
        var chunkIndex = description.IndexOf("chunk", StringComparison.OrdinalIgnoreCase);
        var refineIndex = description.IndexOf("refine", StringComparison.OrdinalIgnoreCase);

        Assert.True(processIndex < chunkIndex);
        Assert.True(chunkIndex < refineIndex);
    }
}
