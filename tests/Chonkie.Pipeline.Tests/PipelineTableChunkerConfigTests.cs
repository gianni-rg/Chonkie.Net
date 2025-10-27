namespace Chonkie.Pipeline.Tests;

using Chonkie.Core.Types;
using Chonkie.Pipeline;
using Xunit;

public class PipelineTableChunkerConfigTests
{
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
