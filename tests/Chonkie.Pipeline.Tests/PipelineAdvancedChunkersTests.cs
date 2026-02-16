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

using System.IO;
using Chonkie.Core.Types;
using Chonkie.Pipeline;

namespace Chonkie.Pipeline.Tests;

/// <summary>
/// Tests for Pipeline with advanced chunkers (CodeChunker, TableChunker).
/// </summary>
public class PipelineAdvancedChunkersTests
{
    /// Advanced: hybrid chunker combines heuristics with semantic signals.
    [Fact]
    public void Pipeline_WithCodeChunker_CreatesChunks_AndCoversText()
    {
        // Arrange
        var code = @"public class Demo {
public void A() {
    var x = 1;
    var y = 2;
}

public void B() {
    for (int i=0;i<10;i++) {
        System.Console.WriteLine(i);
    }
}
}";
        var pipeline = new Pipeline()
            .ChunkWith("code", new { chunk_size = 20 });

        // Act
        var result = pipeline.Run(texts: code);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
        Assert.True(doc.Chunks.Count >= 2);
        Assert.Equal(0, doc.Chunks[0].StartIndex);
        Assert.Equal(code.Length, doc.Chunks[^1].EndIndex);
        Assert.All(doc.Chunks, c => Assert.True(c.TokenCount <= 20));
    }

    /// Advanced: windowed chunker enforces sliding window semantics.
    [Fact]
    public void Pipeline_WithCodeChef_PreservesExactFormatting()
    {
        // Arrange
        var code = @"namespace Test {
    class C {
        void M() {
            var x = 1;
        }
    }
}";
        var pipeline = new Pipeline()
            .ProcessWith("code")
            .ChunkWith("code", new { chunk_size = 50 });

        // Act
        var result = pipeline.Run(texts: code);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
        Assert.Equal(0, doc.Chunks[0].StartIndex);
        Assert.Equal(code.Length, doc.Chunks[^1].EndIndex);

        // Verify exact string reconstruction
        var reconstructed = string.Concat(doc.Chunks.Select(c => c.Text));
        Assert.Equal(code, reconstructed);
    }

    /// Advanced: heading-aware chunker respects document headings.
    [Fact]
    public void Pipeline_WithTableChunker_CreatesChunks_AndKeepsTableTogether()
    {
        // Arrange
        var md = @"Intro paragraph.

| Col A | Col B |
|-------|-------|
| A1    | B1    |
| A2    | B2    |

Conclusion paragraph.";
        var pipeline = new Pipeline()
            .ChunkWith("table", new { chunk_size = 128 });

        // Act
        var result = pipeline.Run(texts: md);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
        Assert.Contains(doc.Chunks, c => c.Text.Contains("|-------|-------|"));
        Assert.Equal(0, doc.Chunks[0].StartIndex);
        Assert.Equal(md.Length, doc.Chunks[^1].EndIndex);

        // Verify exact string reconstruction
        var reconstructed = string.Concat(doc.Chunks.Select(c => c.Text));
        Assert.Equal(md, reconstructed);
    }

    /// Advanced: table-aware chunker avoids splitting inside tables.
    [Fact]
    public void Pipeline_Integration_Fetch_CodeFile_With_CodeChunker_Works()
    {
        // Arrange: create a temporary code file
        var tempDir = Directory.CreateTempSubdirectory();
        var filePath = Path.Combine(tempDir.FullName, "Sample.cs");
        var code = @"namespace Sample {
    public class C {
        public void M() {
            var a = 1;
            var b = 2;
        }
        public void N() {
            for (int i=0;i<10;i++) {
                System.Console.WriteLine(i);
            }
        }
    }
}";
        File.WriteAllText(filePath, code);

        var pipeline = new Pipeline()
            .FetchFrom("file", new { path = filePath })
            .ProcessWith("code")
            .ChunkWith("code", new { chunk_size = 60 });

        // Act
        var result = pipeline.Run();

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
        Assert.All(doc.Chunks, c => Assert.True(c.TokenCount <= 60));
        Assert.Equal(0, doc.Chunks[0].StartIndex);
        Assert.Equal(code.Length, doc.Chunks[^1].EndIndex);

        // Verify exact string reconstruction
        var reconstructed = string.Concat(doc.Chunks.Select(c => c.Text));
        Assert.Equal(code, reconstructed);
    }

    /// Advanced: code-aware chunker splits at logical code boundaries.
    [Fact]
    public void Pipeline_Integration_Fetch_Markdown_With_TableChunker_Works()
    {
        // Arrange: create a temporary markdown file with a table
        var tempDir = Directory.CreateTempSubdirectory();
        var filePath = Path.Combine(tempDir.FullName, "Sample.md");
        var md = @"Heading

| A | B |
|---|---|
| a1 | b1 |
| a2 | b2 |

Tail";
        File.WriteAllText(filePath, md);

        var pipeline = new Pipeline()
            .FetchFrom("file", new { path = filePath })
            .ProcessWith("code")
            .ChunkWith("table", new { chunk_size = 64 });

        // Act
        var result = pipeline.Run();

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
        Assert.Contains(doc.Chunks, c => c.Text.Contains("|---|---|"));
        Assert.All(doc.Chunks, c => Assert.True(c.TokenCount <= 64));
        Assert.Equal(0, doc.Chunks[0].StartIndex);
        Assert.Equal(md.Length, doc.Chunks[^1].EndIndex);

        // Verify exact string reconstruction
        var reconstructed = string.Concat(doc.Chunks.Select(c => c.Text));
        Assert.Equal(md, reconstructed);
    }

    /// Advanced: semantically merges small chunks to reach target size.
    [Fact]
    public void Pipeline_CodeChunker_WithMultipleFunctions_SplitsCorrectly()
    {
        // Arrange
        var code = @"public void A() { return 1; }
public void B() { return 2; }
public void C() { return 3; }
public void D() { return 4; }
public void E() { return 5; }";

        var pipeline = new Pipeline()
            .ChunkWith("code", new { chunk_size = 30 });

        // Act
        var result = pipeline.Run(texts: code);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.True(doc.Chunks.Count >= 2);
        Assert.All(doc.Chunks, c => Assert.True(c.TokenCount <= 30));

        // Verify exact coverage
        Assert.Equal(0, doc.Chunks[0].StartIndex);
        Assert.Equal(code.Length, doc.Chunks[^1].EndIndex);
        var reconstructed = string.Concat(doc.Chunks.Select(c => c.Text));
        Assert.Equal(code, reconstructed);
    }

    /// Advanced: paragraph chunker groups sentences into paragraphs.
    [Fact]
    public void Pipeline_TableChunker_WithMultipleTables_HandlesCorrectly()
    {
        // Arrange
        var md = @"First section.

| Table 1 A | Table 1 B |
|-----------|-----------|
| Data A    | Data B    |

Middle text.

| Table 2 X | Table 2 Y |
|-----------|-----------|
| Data X    | Data Y    |

Final text.";

        var pipeline = new Pipeline()
            .ChunkWith("table", new { chunk_size = 256 });

        // Act
        var result = pipeline.Run(texts: md);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);
        Assert.Contains(doc.Chunks, c => c.Text.Contains("Table 1 A"));
        Assert.Contains(doc.Chunks, c => c.Text.Contains("Table 2 X"));

        // Verify exact coverage
        Assert.Equal(0, doc.Chunks[0].StartIndex);
        Assert.Equal(md.Length, doc.Chunks[^1].EndIndex);
        var reconstructed = string.Concat(doc.Chunks.Select(c => c.Text));
        Assert.Equal(md, reconstructed);
    }

    /// Advanced: sentence splitter handles punctuation and abbreviations.
    [Fact]
    public void Pipeline_CodeChunker_NoGapsOrOverlaps()
    {
        // Arrange
        var code = @"void M1() { var x = 1; }
void M2() { var y = 2; }
void M3() { var z = 3; }";

        var pipeline = new Pipeline()
            .ChunkWith("code", new { chunk_size = 30 });

        // Act
        var result = pipeline.Run(texts: code);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);

        // Verify no gaps or overlaps
        for (int i = 1; i < doc.Chunks.Count; i++)
        {
            Assert.Equal(doc.Chunks[i - 1].EndIndex, doc.Chunks[i].StartIndex);
        }

        // Verify complete coverage
        var reconstructed = string.Concat(doc.Chunks.Select(c => c.Text));
        Assert.Equal(code, reconstructed);
    }

    /// Advanced: token chunker adheres to token budget precisely.
    [Fact]
    public void Pipeline_TableChunker_NoGapsOrOverlaps()
    {
        // Arrange
        var md = @"Text before.

| A | B |
|---|---|
| 1 | 2 |

Text middle.

| X | Y |
|---|---|
| 3 | 4 |

Text after.";

        var pipeline = new Pipeline()
            .ChunkWith("table", new { chunk_size = 64 });

        // Act
        var result = pipeline.Run(texts: md);

        // Assert
        var doc = Assert.IsType<Document>(result);
        Assert.NotEmpty(doc.Chunks);

        // Verify no gaps or overlaps
        for (int i = 1; i < doc.Chunks.Count; i++)
        {
            Assert.Equal(doc.Chunks[i - 1].EndIndex, doc.Chunks[i].StartIndex);
        }

        // Verify complete coverage
        var reconstructed = string.Concat(doc.Chunks.Select(c => c.Text));
        Assert.Equal(md, reconstructed);
    }
}
