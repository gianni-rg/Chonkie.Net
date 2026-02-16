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

namespace Chonkie.Pipeline.Tests;

using Chonkie.Core.Types;
using Chonkie.Pipeline;
using Xunit;

/// <summary>
/// Tests for Pipeline table chunker configuration options.
/// </summary>
public class PipelineTableChunkerConfigTests
{
    /// <summary>
    /// Tests that Pipeline with repeat_headers=false does not repeat headers in subsequent chunks.
    /// </summary>
    [Fact]
    public void Pipeline_TableChunker_WithRepeatHeadersFalse()
    {
        var md = @"| A | B |
|---|---|
| 1 | 2 |
| 3 | 4 |
| 5 | 6 |";

        var pipeline = new Pipeline()
            .ChunkWith("table", new { chunk_size = 30, repeat_headers = false });

        var result = pipeline.Run(texts: md);

        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);

        if (doc.Chunks.Count > 1)
        {
            // First chunk should have header
            Assert.Contains("| A | B |", doc.Chunks[0].Text);

            // Subsequent chunks should not have header
            for (int i = 1; i < doc.Chunks.Count; i++)
            {
                Assert.DoesNotContain("| A | B |", doc.Chunks[i].Text);
            }
        }
    }

    /// <summary>
    /// Tests that Pipeline with repeat_headers=true repeats headers in all chunks.
    /// </summary>
    [Fact]
    public void Pipeline_TableChunker_WithRepeatHeadersTrue()
    {
        var md = @"| A | B |
|---|---|
| 1 | 2 |
| 3 | 4 |
| 5 | 6 |";

        var pipeline = new Pipeline()
            .ChunkWith("table", new { chunk_size = 30, repeat_headers = true });

        var result = pipeline.Run(texts: md);

        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);

        if (doc.Chunks.Count > 1)
        {
            // Every chunk should have header
            foreach (var chunk in doc.Chunks)
            {
                Assert.Contains("| A | B |", chunk.Text);
                Assert.Contains("|---|---|", chunk.Text);
            }
        }
    }

    /// <summary>
    /// Tests that Pipeline table chunker defaults to repeat_headers=false when not specified.
    /// </summary>
    [Fact]
    public void Pipeline_TableChunker_DefaultRepeatHeaders()
    {
        var md = @"| Name | Value |
|------|-------|
| A | 1 |
| B | 2 |";

        // Default should be false
        var pipeline = new Pipeline()
            .ChunkWith("table", new { chunk_size = 30 });

        var result = pipeline.Run(texts: md);

        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);

        // Behavior should match repeat_headers = false
        if (doc.Chunks.Count > 1)
        {
            // Subsequent chunks should not have header
            for (int i = 1; i < doc.Chunks.Count; i++)
            {
                Assert.DoesNotContain("| Name | Value |", doc.Chunks[i].Text);
            }
        }
    }

    /// <summary>
    /// Tests that Pipeline table chunker with repeat_headers=true works correctly with large tables that require multiple chunks.
    /// </summary>
    [Fact]
    public void Pipeline_TableChunker_RepeatHeadersWithLargeTable()
    {
        var header = "| ID | Name |\n|----|------|\n";
        var rows = new List<string>();
        for (int i = 1; i <= 10; i++)
        {
            rows.Add($"| {i:D2} | Row{i} |");
        }
        var md = header + string.Join("\n", rows);

        var pipeline = new Pipeline()
            .ChunkWith("table", new { chunk_size = 60, repeat_headers = true });

        var result = pipeline.Run(texts: md);

        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);

        if (doc.Chunks.Count > 1)
        {
            // Every chunk should have header and separator
            foreach (var chunk in doc.Chunks)
            {
                Assert.Contains("| ID | Name |", chunk.Text);
                Assert.Contains("|----|------|", chunk.Text);
            }
        }
    }

    /// <summary>
    /// Tests that Pipeline table chunker with repeat_headers=true preserves all table data across chunks.
    /// </summary>
    [Fact]
    public void Pipeline_TableChunker_RepeatHeadersPreservesAllData()
    {
        var md = @"| Col1 | Col2 |
|------|------|
| A | 1 |
| B | 2 |
| C | 3 |
| D | 4 |
| E | 5 |";

        var pipeline = new Pipeline()
            .ChunkWith("table", new { chunk_size = 40, repeat_headers = true });

        var result = pipeline.Run(texts: md);

        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);

        // All data should be present in the combined chunks
        var allText = string.Concat(doc.Chunks.Select(c => c.Text));
        Assert.Contains("| A | 1 |", allText);
        Assert.Contains("| B | 2 |", allText);
        Assert.Contains("| C | 3 |", allText);
        Assert.Contains("| D | 4 |", allText);
        Assert.Contains("| E | 5 |", allText);
    }
}
