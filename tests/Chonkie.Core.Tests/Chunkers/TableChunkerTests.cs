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

using Chonkie.Chunkers;
using Chonkie.Tokenizers;
using Xunit;

public class TableChunkerTests
{
    [Fact]
    public void Chunk_KeepsMarkdownTableIntact_WhenWithinBudget()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 512);

        var md = @"Intro paragraph.

| Col A | Col B |
|-------|-------|
| A1    | B1    |
| A2    | B2    |

Conclusion paragraph.";

        var chunks = chunker.Chunk(md);

        Assert.NotEmpty(chunks);
        // There should exist a chunk that contains the full table header marker
        Assert.Contains(chunks, c => c.Text.Contains("|-------|-------|"));
        // Ensure full coverage
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(md.Length, chunks[^1].EndIndex);
    }

    [Fact]
    public void Chunk_SplitsLargeTableByRows_WhenExceedingBudget()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 80);

        // Build a table large enough to exceed budget
        var header = "| A | B |\n|---|---|\n";
        var rows = string.Concat(Enumerable.Repeat("| AAAA | BBBB |\n", 20));
        var md = header + rows;

        var chunks = chunker.Chunk(md);

        Assert.True(chunks.Count > 1);
        Assert.All(chunks, c => Assert.True(c.TokenCount <= 80));
        // The first chunk should contain header + separator
        Assert.Contains("|---|---|", chunks[0].Text);
    }

    [Fact]
    public void Chunk_PreservesExactStringIndices()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 512);

        var md = @"Text before table.

| Header 1 | Header 2 |
|----------|----------|
| Cell 1   | Cell 2   |

Text after table.";

        var chunks = chunker.Chunk(md);

        // Verify complete coverage
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(md.Length, chunks[^1].EndIndex);

        // Verify each chunk text matches original at indices
        foreach (var chunk in chunks)
        {
            var extracted = md.Substring(chunk.StartIndex, chunk.EndIndex - chunk.StartIndex);
            Assert.Equal(chunk.Text, extracted);
        }
    }

    [Fact]
    public void Chunk_MultipleTables_HandlesCorrectly()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 256);

        var md = @"First section.

| Table 1 Col A | Table 1 Col B |
|---------------|---------------|
| Data A1       | Data B1       |

Middle text.

| Table 2 Col X | Table 2 Col Y |
|---------------|---------------|
| Data X1       | Data Y1       |

Final text.";

        var chunks = chunker.Chunk(md);

        Assert.NotEmpty(chunks);
        // Both tables should be present in chunks
        Assert.Contains(chunks, c => c.Text.Contains("Table 1 Col A"));
        Assert.Contains(chunks, c => c.Text.Contains("Table 2 Col X"));
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(md.Length, chunks[^1].EndIndex);
    }

    [Fact]
    public void Chunk_TableWithoutSeparator_TreatsAsNormalText()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 128);

        var text = @"This looks like a table row:
| Col A | Col B |
But no separator follows.";

        var chunks = chunker.Chunk(text);

        Assert.NotEmpty(chunks);
        // Should be treated as normal text, not a table
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(text.Length, chunks[^1].EndIndex);
    }

    [Fact]
    public void Chunk_EmptyTableRows_HandlesCorrectly()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 256);

        var md = @"| Header A | Header B |
|----------|----------|
| Value 1  | Value 2  |
|          |          |
| Value 3  | Value 4  |";

        var chunks = chunker.Chunk(md);

        Assert.NotEmpty(chunks);
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(md.Length, chunks[^1].EndIndex);
        Assert.Contains(chunks, c => c.Text.Contains("|----------|----------|"));
    }

    [Fact]
    public void Chunk_InterleavedTablesAndText_MaintainsCoverage()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 128);

        var md = @"Paragraph 1.

| A | B |
|---|---|
| 1 | 2 |

Paragraph 2.

| X | Y |
|---|---|
| 3 | 4 |

Paragraph 3.";

        var chunks = chunker.Chunk(md);

        Assert.NotEmpty(chunks);
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(md.Length, chunks[^1].EndIndex);

        // Verify no gaps
        for (int i = 1; i < chunks.Count; i++)
        {
            Assert.Equal(chunks[i - 1].EndIndex, chunks[i].StartIndex);
        }
    }

    [Fact]
    public void Chunk_TableWithManyColumns_HandlesCorrectly()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 256);

        var md = @"| C1 | C2 | C3 | C4 | C5 | C6 |
|----|----|----|----|----|----|
| V1 | V2 | V3 | V4 | V5 | V6 |
| W1 | W2 | W3 | W4 | W5 | W6 |";

        var chunks = chunker.Chunk(md);

        Assert.NotEmpty(chunks);
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(md.Length, chunks[^1].EndIndex);
        Assert.Contains(chunks, c => c.Text.Contains("C1"));
        Assert.Contains(chunks, c => c.Text.Contains("C6"));
    }

    [Fact]
    public void Chunk_VeryLargeTable_SplitsWhileKeepingHeader()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 100);

        var header = "| Header A | Header B | Header C |\n|----------|----------|----------|\n";
        var rows = string.Concat(Enumerable.Repeat("| DataAAAA | DataBBBB | DataCCCC |\n", 50));
        var md = header + rows;

        var chunks = chunker.Chunk(md);

        Assert.True(chunks.Count > 1);
        Assert.All(chunks, c => Assert.True(c.TokenCount <= 100));
        // First chunk must have header and separator
        Assert.Contains("Header A", chunks[0].Text);
        Assert.Contains("|----------|----------|----------|", chunks[0].Text);
    }
}
