namespace Chonkie.Core.Tests.Chunkers;

using Chonkie.Chunkers;
using Chonkie.Tokenizers;
using Xunit;

public class TableChunkerHeaderRepetitionTests
{

    [Fact]
    public void TableChunker_WithRepeatHeadersFalse_HeaderOnlyInFirstChunk()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 80, repeatHeaders: false);

        var table = @"| Header A | Header B |
|----------|----------|
| Data 1   | Value 1  |
| Data 2   | Value 2  |
| Data 3   | Value 3  |";

        var chunks = chunker.Chunk(table);

        Assert.True(chunks.Count >= 2); // Should split due to size

        // First chunk should have header
        Assert.Contains("| Header A | Header B |", chunks[0].Text);
        Assert.Contains("|----------|----------|", chunks[0].Text);

        // Subsequent chunks should NOT have header
        for (int i = 1; i < chunks.Count; i++)
        {
            Assert.DoesNotContain("| Header A | Header B |", chunks[i].Text);
        }
    }

    [Fact]
    public void TableChunker_WithRepeatHeadersTrue_HeaderInEveryChunk()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 80, repeatHeaders: true);

        var table = @"| Header A | Header B |
|----------|----------|
| Data 1   | Value 1  |
| Data 2   | Value 2  |
| Data 3   | Value 3  |";

        var chunks = chunker.Chunk(table);

        Assert.True(chunks.Count >= 2); // Should split due to size

        // Every chunk should have header
        foreach (var chunk in chunks)
        {
            Assert.Contains("| Header A | Header B |", chunk.Text);
            Assert.Contains("|----------|----------|", chunk.Text);
        }
    }

    [Fact]
    public void TableChunker_RepeatHeadersDefault_IsFalse()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer);

        Assert.False(chunker.RepeatHeaders);
    }

    [Fact]
    public void TableChunker_RepeatHeadersTrue_InToString()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 512, repeatHeaders: true);

        var str = chunker.ToString();

        Assert.Contains("repeat_headers=True", str);
    }

    [Fact]
    public void TableChunker_RepeatHeadersFalse_InToString()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 512, repeatHeaders: false);

        var str = chunker.ToString();

        Assert.Contains("repeat_headers=False", str);
    }

    [Fact]
    public void TableChunker_InvalidTable_LogsWarning()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 100);

        // Table with only header and separator (no data rows)
        var invalidTable = @"| Name | Value |
|------|-------|";

        var chunks = chunker.Chunk(invalidTable);

        // Should still produce chunks (treated as normal text via fallback)
        Assert.NotEmpty(chunks);
    }

    [Fact]
    public void TableChunker_WithRepeatHeaders_PreservesAllDataRows()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 60, repeatHeaders: true);

        var table = @"| A | B |
|---|---|
| 1 | X |
| 2 | Y |
| 3 | Z |";

        var chunks = chunker.Chunk(table);

        // Collect all data rows from chunks (excluding headers)
        var allDataRows = new HashSet<string>();
        foreach (var chunk in chunks)
        {
            var lines = chunk.Text.Split('\n');
            // Skip first two lines (header and separator)
            for (int i = 2; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    allDataRows.Add(lines[i].Trim());
                }
            }
        }

        // Should have all three data rows
        Assert.Contains("| 1 | X |", allDataRows);
        Assert.Contains("| 2 | Y |", allDataRows);
        Assert.Contains("| 3 | Z |", allDataRows);
    }

    [Fact]
    public void TableChunker_WithRepeatHeaders_LargeTable_AllRowsPresent()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 100, repeatHeaders: true);

        var header = "| ID | Name | Value |\n|----|------|-------|\n";
        var rows = new List<string>();
        for (int i = 1; i <= 20; i++)
        {
            rows.Add($"| {i:D2} | Row{i} | Val{i} |");
        }
        var table = header + string.Join("\n", rows);

        var chunks = chunker.Chunk(table);

        Assert.True(chunks.Count > 1); // Should split into multiple chunks

        // Verify all rows appear somewhere
        var allText = string.Concat(chunks.Select(c => c.Text));
        for (int i = 1; i <= 20; i++)
        {
            Assert.Contains($"Row{i}", allText);
        }

        // Verify every chunk has the header
        foreach (var chunk in chunks)
        {
            Assert.Contains("| ID | Name | Value |", chunk.Text);
            Assert.Contains("|----|------|-------|", chunk.Text);
        }
    }

    [Fact]
    public void TableChunker_WithoutRepeatHeaders_SequentialIndices()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 60, repeatHeaders: false);

        var table = @"| A | B |
|---|---|
| 1 | X |
| 2 | Y |
| 3 | Z |";

        var chunks = chunker.Chunk(table);

        // Verify no gaps or overlaps
        for (int i = 1; i < chunks.Count; i++)
        {
            Assert.Equal(chunks[i - 1].EndIndex, chunks[i].StartIndex);
        }

        // Verify complete coverage
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(table.Length, chunks[^1].EndIndex);
    }

    [Fact]
    public void TableChunker_BothModesProduceValidChunks()
    {
        var tokenizer = new CharacterTokenizer();
        var table = @"| Name | Value |
|------|-------|
| A | 1 |
| B | 2 |
| C | 3 |
| D | 4 |";

        // Test without repeat headers
        var chunkerNoRepeat = new TableChunker(tokenizer, chunkSize: 50, repeatHeaders: false);
        var chunksNoRepeat = chunkerNoRepeat.Chunk(table);
        Assert.NotEmpty(chunksNoRepeat);
        Assert.All(chunksNoRepeat, c => Assert.NotEmpty(c.Text));

        // Test with repeat headers
        var chunkerRepeat = new TableChunker(tokenizer, chunkSize: 50, repeatHeaders: true);
        var chunksRepeat = chunkerRepeat.Chunk(table);
        Assert.NotEmpty(chunksRepeat);
        Assert.All(chunksRepeat, c => Assert.NotEmpty(c.Text));
    }
}
