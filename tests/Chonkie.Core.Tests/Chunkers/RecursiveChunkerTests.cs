namespace Chonkie.Core.Tests.Chunkers;

using Chonkie.Chunkers;
using Chonkie.Core.Types;
using Chonkie.Tokenizers;
using FluentAssertions;
using Xunit;

public class RecursiveChunkerTests
{
    [Fact]
    public void RecursiveChunker_ShouldInitializeWithDefaultParameters()
    {
        // Arrange & Act
        var tokenizer = new CharacterTokenizer();
        var chunker = new RecursiveChunker(tokenizer);

        // Assert
        chunker.ChunkSize.Should().Be(2048);
        chunker.MinCharactersPerChunk.Should().Be(24);
        chunker.Rules.Count.Should().Be(5); // Default 5 levels
    }

    [Fact]
    public void RecursiveChunker_ShouldInitializeWithCustomRules()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var customRules = new RecursiveRules(new[]
        {
            new RecursiveLevel { Delimiters = new[] { "\n\n" } },
            new RecursiveLevel { Whitespace = true }
        });

        // Act
        var chunker = new RecursiveChunker(tokenizer, rules: customRules);

        // Assert
        chunker.Rules.Count.Should().Be(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RecursiveChunker_ShouldThrowOnInvalidChunkSize(int chunkSize)
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act & Assert
        var act = () => new RecursiveChunker(tokenizer, chunkSize: chunkSize);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*chunk_size must be greater than 0*");
    }

    [Fact]
    public void Chunk_ShouldReturnEmptyListForEmptyText()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new RecursiveChunker(tokenizer, chunkSize: 100);

        // Act
        var chunks = chunker.Chunk("");

        // Assert
        chunks.Should().BeEmpty();
    }

    [Fact]
    public void Chunk_ShouldReturnEmptyListForWhitespaceText()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new RecursiveChunker(tokenizer, chunkSize: 100);

        // Act
        var chunks = chunker.Chunk("   \t\n  ");

        // Assert
        chunks.Should().BeEmpty();
    }

    [Fact]
    public void Chunk_ShouldCreateSingleChunkForShortText()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new RecursiveChunker(tokenizer, chunkSize: 100);
        var text = "Short text";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().ContainSingle();
        chunks[0].Text.Should().Be(text);
        chunks[0].TokenCount.Should().Be(10);
    }

    [Fact]
    public void Chunk_ShouldSplitOnParagraphs()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new RecursiveChunker(tokenizer, chunkSize: 30);
        var text = "First paragraph here.\n\nSecond paragraph here.\n\nThird paragraph here.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().HaveCountGreaterThan(1);
        // Verify all text is covered
        var totalLength = chunks.Sum(c => c.Text.Length);
        totalLength.Should().Be(text.Length);
    }

    [Fact]
    public void Chunk_ShouldSplitOnSentences()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new RecursiveChunker(tokenizer, chunkSize: 25, minCharactersPerChunk: 5);
        var text = "First sentence. Second sentence. Third sentence.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().HaveCountGreaterThan(1);
        // Verify indices are continuous
        for (int i = 1; i < chunks.Count; i++)
        {
            chunks[i].StartIndex.Should().Be(chunks[i - 1].EndIndex);
        }
    }

    [Fact]
    public void Chunk_ShouldHandleWhitespaceSplitting()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var rules = new RecursiveRules(new[]
        {
            new RecursiveLevel { Whitespace = true }
        });
        var chunker = new RecursiveChunker(tokenizer, chunkSize: 3, rules: rules);
        var text = "one two three four five six";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().HaveCountGreaterThan(1);
        chunks.Should().AllSatisfy(c => c.TokenCount.Should().BeLessThanOrEqualTo(3));
    }

    [Fact]
    public void Chunk_ShouldRespectMinCharactersPerChunk()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new RecursiveChunker(tokenizer, chunkSize: 50, minCharactersPerChunk: 20);
        var text = "a b c d e f g h i j k l m n o p q r s t u v w x y z";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().NotBeEmpty();
        // Most chunks should meet minimum (except possibly last)
        chunks.Take(chunks.Count - 1).Should().AllSatisfy(c => 
            c.Text.Length.Should().BeGreaterThanOrEqualTo(20));
    }

    [Fact]
    public void Chunk_ShouldPreserveIndices()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new RecursiveChunker(tokenizer, chunkSize: 30);
        var text = "First paragraph.\n\nSecond paragraph.\n\nThird paragraph.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().NotBeEmpty();
        
        // First chunk should start at 0
        chunks[0].StartIndex.Should().Be(0);
        
        // Verify each chunk's indices
        foreach (var chunk in chunks)
        {
            chunk.EndIndex.Should().Be(chunk.StartIndex + chunk.Text.Length);
            
            // Verify chunk text matches original text at those indices
            if (chunk.EndIndex <= text.Length)
            {
                text.Substring(chunk.StartIndex, chunk.Text.Length).Should().Be(chunk.Text);
            }
        }
        
        // Last chunk should end at text length
        chunks[chunks.Count - 1].EndIndex.Should().Be(text.Length);
    }

    [Fact]
    public void Chunk_ShouldHandleCustomDelimiters()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var rules = new RecursiveRules(new[]
        {
            new RecursiveLevel { Delimiters = new[] { "|" }, IncludeDelimiter = "prev" }
        });
        var chunker = new RecursiveChunker(tokenizer, chunkSize: 15, rules: rules, minCharactersPerChunk: 3);
        var text = "part1|part2|part3|part4";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().NotBeEmpty();
        // Verify all parts are present in the combined output
        var combined = string.Concat(chunks.Select(c => c.Text));
        combined.Should().Contain("part1");
        combined.Should().Contain("part2");
        combined.Should().Contain("part3");
        combined.Should().Contain("part4");
    }

    [Fact]
    public void Chunk_ShouldHandleMultipleLevels()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var rules = new RecursiveRules(new[]
        {
            new RecursiveLevel { Delimiters = new[] { "\n\n" } },  // Paragraphs
            new RecursiveLevel { Delimiters = new[] { ". " } },    // Sentences
            new RecursiveLevel { Whitespace = true }               // Words
        });
        var chunker = new RecursiveChunker(tokenizer, chunkSize: 30, rules: rules, minCharactersPerChunk: 5);
        var text = "First paragraph. Second sentence.\n\nAnother paragraph. More text.";

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().NotBeEmpty();
        chunks.Should().AllSatisfy(c => c.TokenCount.Should().BeLessThanOrEqualTo(30));
    }

    [Fact]
    public void Chunk_ShouldMergeSmallSplits()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new RecursiveChunker(tokenizer, chunkSize: 50, minCharactersPerChunk: 10);
        var text = "a. b. c. d. e. f. g. h. i. j."; // Many tiny sentences

        // Act
        var chunks = chunker.Chunk(text);

        // Assert
        chunks.Should().NotBeEmpty();
        // Small splits should be merged
        chunks.Should().AllSatisfy(c => c.Text.Length.Should().BeGreaterThan(5));
    }

    [Fact]
    public void ChunkBatch_ShouldProcessMultipleTexts()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new RecursiveChunker(tokenizer, chunkSize: 30);
        var texts = new[] { "Text one.\n\nText two.", "Another text here." };

        // Act
        var results = chunker.ChunkBatch(texts);

        // Assert
        results.Should().HaveCount(2);
        results[0].Should().NotBeEmpty();
        results[1].Should().NotBeEmpty();
    }

    [Fact]
    public void ToString_ShouldReturnDescriptiveString()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var chunker = new RecursiveChunker(tokenizer, chunkSize: 512);

        // Act
        var result = chunker.ToString();

        // Assert
        result.Should().Contain("512");
        result.Should().Contain("5"); // 5 levels
    }
}
