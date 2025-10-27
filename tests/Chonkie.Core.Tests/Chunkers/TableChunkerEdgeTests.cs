namespace Chonkie.Core.Tests.Chunkers;

using Chonkie.Chunkers;
using Chonkie.Tokenizers;
using Xunit;

public class TableChunkerEdgeTests
{
    [Fact]
    public void Chunk_DoesNotTreatSinglePipeTextAsTable()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 128);
        var text = "This | is not a table. Just a single pipe in text.";

        var chunks = chunker.Chunk(text);

        Assert.Single(chunks);
        Assert.Equal(text, chunks[0].Text);
    }

    [Fact]
    public void Constructor_ThrowsOnZeroChunkSize()
    {
        var tokenizer = new CharacterTokenizer();

        var ex = Assert.Throws<ArgumentException>(() =>
            new TableChunker(tokenizer, chunkSize: 0));
        Assert.Contains("chunk_size must be greater than 0", ex.Message);
    }

    [Fact]
    public void Constructor_ThrowsOnNegativeChunkSize()
    {
        var tokenizer = new CharacterTokenizer();

        var ex = Assert.Throws<ArgumentException>(() =>
            new TableChunker(tokenizer, chunkSize: -1));
        Assert.Contains("chunk_size must be greater than 0", ex.Message);
    }

    [Fact]
    public void Chunk_EmptyInput_ReturnsEmpty()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 500);

        var chunks = chunker.Chunk("");

        Assert.Empty(chunks);
    }

    [Fact]
    public void Chunk_SingleRowTable_ReturnsOneChunk()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 100);

        var table = @"| Name | Value |
|------|-------|
| A | 1 |";

        var chunks = chunker.Chunk(table);

        Assert.Single(chunks);
        Assert.Equal(table, chunks[0].Text);
    }

    [Fact]
    public void Chunk_VeryWideTable_HandlesCorrectly()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 100);

        var table = @"| C1 | C2 | C3 | C4 | C5 | C6 | C7 | C8 | C9 | C10 |
|----|----|----|----|----|----|----|----|----|-----|
| 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 |
| 11 | 12 | 13 | 14 | 15 | 16 | 17 | 18 | 19 | 20 |";

        var chunks = chunker.Chunk(table);

        // Should chunk due to size constraint
        Assert.NotEmpty(chunks);
        // Verify table content is preserved
        var combined = string.Concat(chunks.Select(c => c.Text));
        Assert.Contains("C1", combined);
        Assert.Contains("C10", combined);
    }

    [Fact]
    public void Chunk_VeryLongRowExceedingChunkSize_StillCreatesChunk()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 50);

        var table = @"| Name | Description |
|------|-------------|
| Item | This is an extremely long description that goes on and on and contains lots of information that will definitely exceed the chunk size limit we set for this test |";

        var chunks = chunker.Chunk(table);

        // Should create chunk even though row exceeds size
        Assert.NotEmpty(chunks);
        // Verify the long description is preserved somewhere in the chunks
        var combined = string.Concat(chunks.Select(c => c.Text));
        Assert.Contains("extremely long description", combined);
    }

    [Fact]
    public void Chunk_IrregularSpacing_PreservesRows()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 500);

        var table = @"| Name|Age|City |
|---|---|---|
|John  |25|NYC|
| Alice|30| London   |
|  Bob  | 35 |  Paris  |";

        var chunks = chunker.Chunk(table);

        Assert.NotEmpty(chunks);
        // Check that all rows are preserved
        var combined = string.Concat(chunks.Select(c => c.Text));
        Assert.Contains("John", combined);
        Assert.Contains("Alice", combined);
        Assert.Contains("Bob", combined);
    }

    [Fact]
    public void Chunk_SpecialCharacters_HandlesCorrectly()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 80);

        var table = @"| Symbol | Meaning |
|--------|---------|
| @ | At sign |
| # | Hash |
| $ | Dollar |
| % | Percent |
| & | Ampersand |
| * | Asterisk |";

        var chunks = chunker.Chunk(table);

        // Verify all symbols are present
        var combined = string.Concat(chunks.Select(c => c.Text));
        Assert.Contains("@", combined);
        Assert.Contains("#", combined);
        Assert.Contains("$", combined);
        Assert.Contains("%", combined);
        Assert.Contains("&", combined);
        Assert.Contains("*", combined);
    }

    [Fact]
    public void Chunk_UnicodeContent_PreservesUnicode()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 100);

        var table = @"| Name | Country | Flag |
|------|---------|------|
| Tokyo | Japan | 🇯🇵 |
| Paris | France | 🇫🇷 |
| Berlin | Germany | 🇩🇪 |
| Москва | Россия | 🇷🇺 |
| 北京 | 中国 | 🇨🇳 |";

        var chunks = chunker.Chunk(table);

        // Verify unicode is preserved
        var combined = string.Concat(chunks.Select(c => c.Text));
        Assert.Contains("Tokyo", combined);
        Assert.Contains("Москва", combined);
        Assert.Contains("北京", combined);
        Assert.Contains("🇯🇵", combined);
        Assert.Contains("🇨🇳", combined);
    }

    [Fact]
    public void Chunk_WhitespaceOnlyCells_MaintainsStructure()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 100);

        var table = @"| Name | Value |
|------|-------|
| A |   |
|   | B |
|  |  |";

        var chunks = chunker.Chunk(table);

        Assert.NotEmpty(chunks);
        // Structure should be maintained
        foreach (var chunk in chunks)
        {
            Assert.Contains("|", chunk.Text);
        }
    }

    [Fact]
    public void Chunk_TrailingNewlines_HandlesGracefully()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 100);

        var table = @"| Name | Value |
|------|-------|
| A | 1 |
| B | 2 |

";

        var chunks = chunker.Chunk(table);

        Assert.NotEmpty(chunks);
        // Should handle gracefully
        var combined = string.Concat(chunks.Select(c => c.Text));
        Assert.Contains("| A | 1 |", combined);
    }

    [Fact]
    public void Chunk_NumericEdgeCases_PreservesNumbers()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 70);

        var table = @"| Number | Value |
|--------|-------|
| 0 | Zero |
| -1 | Negative |
| 3.14159 | Pi |
| 1e10 | Scientific |
| 0xFF | Hex |";

        var chunks = chunker.Chunk(table);

        var combined = string.Concat(chunks.Select(c => c.Text));
        Assert.Contains("0", combined);
        Assert.Contains("-1", combined);
        Assert.Contains("3.14159", combined);
        Assert.Contains("1e10", combined);
        Assert.Contains("0xFF", combined);
    }

    [Fact]
    public void Chunk_ExactBoundaryConditions_SplitsCorrectly()
    {
        var tokenizer = new CharacterTokenizer();

        var table = @"| A | B |
|---|---|
| 1 | 2 |
| 3 | 4 |
| 5 | 6 |";

        var headerAndSep = "| A | B |\n|---|---|";
        var rowSize = "\n| 1 | 2 |".Length;

        // Set chunk size to fit header + exactly 2 rows
        var chunkSize = headerAndSep.Length + (rowSize * 2);

        var chunker = new TableChunker(tokenizer, chunkSize: chunkSize);
        var chunks = chunker.Chunk(table);

        // Should split into chunks respecting boundary
        Assert.NotEmpty(chunks);
        // Verify all content is preserved
        var combined = string.Concat(chunks.Select(c => c.Text));
        Assert.Contains("| A | B |", combined);
        Assert.Equal(0, chunks[0].StartIndex);
        Assert.Equal(table.Length, chunks[^1].EndIndex);
    }

    [Fact]
    public void Chunk_VerySmallChunkSize_StillIncludesContent()
    {
        var tokenizer = new CharacterTokenizer();
        var table = @"| Name | Value |
|------|-------|
| A | 1 |
| B | 2 |";

        // Set chunk size smaller than header
        var chunker = new TableChunker(tokenizer, chunkSize: 20);
        var chunks = chunker.Chunk(table);

        // Should still create chunks even with very small chunk size
        Assert.NotEmpty(chunks);
        // Verify all data is preserved
        var combined = string.Concat(chunks.Select(c => c.Text));
        Assert.Contains("| A | 1 |", combined);
        Assert.Contains("| B | 2 |", combined);
    }
}
