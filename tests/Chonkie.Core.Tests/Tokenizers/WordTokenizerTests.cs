using Chonkie.Tokenizers;
using FluentAssertions;

namespace Chonkie.Core.Tests.Tokenizers;

public class WordTokenizerTests
{
    [Fact]
    public void Encode_SimpleText_ReturnsTokenIds()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var text = "hello world test";

        // Act
        var tokens = tokenizer.Encode(text);

        // Assert
        tokens.Should().HaveCount(3);
        tokens.Should().AllBeOfType<int>();
    }

    [Fact]
    public void Encode_EmptyString_ReturnsEmptyList()
    {
        // Arrange
        var tokenizer = new WordTokenizer();

        // Act
        var tokens = tokenizer.Encode("");

        // Assert
        tokens.Should().BeEmpty();
    }

    [Fact]
    public void Encode_SingleWord_ReturnsOneToken()
    {
        // Arrange
        var tokenizer = new WordTokenizer();

        // Act
        var tokens = tokenizer.Encode("hello");

        // Assert
        tokens.Should().ContainSingle();
    }

    [Fact]
    public void Decode_TokenIds_ReturnsOriginalText()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var text = "hello world test";
        var encoded = tokenizer.Encode(text);

        // Act
        var decoded = tokenizer.Decode(encoded);

        // Assert
        decoded.Should().Be(text);
    }

    [Fact]
    public void Decode_EmptyList_ReturnsEmptyString()
    {
        // Arrange
        var tokenizer = new WordTokenizer();

        // Act
        var decoded = tokenizer.Decode(Array.Empty<int>());

        // Assert
        decoded.Should().BeEmpty();
    }

    [Fact]
    public void Decode_InvalidTokenId_ThrowsException()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var invalidTokens = new[] { 99999 };

        // Act
        var act = () => tokenizer.Decode(invalidTokens);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*not found in vocabulary*");
    }

    [Fact]
    public void CountTokens_ReturnsWordCount()
    {
        // Arrange
        var tokenizer = new WordTokenizer();

        // Act & Assert
        tokenizer.CountTokens("").Should().Be(0);
        tokenizer.CountTokens("word").Should().Be(1);
        tokenizer.CountTokens("two words").Should().Be(2);
        tokenizer.CountTokens("the quick brown fox").Should().Be(4);
    }

    [Fact]
    public void CountTokens_WithMultipleSpaces_CountsEachSpace()
    {
        // Arrange
        var tokenizer = new WordTokenizer();

        // Act
        var count = tokenizer.CountTokens("word  double  space");

        // Assert
        // Split behavior: "word", "", "double", "", "space"
        count.Should().Be(5);
    }

    [Fact]
    public void EncodeDecode_RoundTrip_PreservesText()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var texts = new[]
        {
            "simple text",
            "text with numbers 123",
            "special chars !@#",
            "one",
            ""
        };

        foreach (var text in texts)
        {
            // Act
            var encoded = tokenizer.Encode(text);
            var decoded = tokenizer.Decode(encoded);

            // Assert
            decoded.Should().Be(text);
        }
    }

    [Fact]
    public void EncodeBatch_MultipleTexts_ReturnsAllEncoded()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var texts = new[] { "one two", "three four", "five" };

        // Act
        var encoded = tokenizer.EncodeBatch(texts);

        // Assert
        encoded.Should().HaveCount(3);
        encoded[0].Should().HaveCount(2);
        encoded[1].Should().HaveCount(2);
        encoded[2].Should().HaveCount(1);
    }

    [Fact]
    public void DecodeBatch_MultipleTokenSequences_ReturnsAllDecoded()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var texts = new[] { "one two", "three four", "five" };
        var encoded = tokenizer.EncodeBatch(texts);

        // Act
        var decoded = tokenizer.DecodeBatch(encoded);

        // Assert
        decoded.Should().HaveCount(3);
        decoded[0].Should().Be("one two");
        decoded[1].Should().Be("three four");
        decoded[2].Should().Be("five");
    }

    [Fact]
    public void CountTokensBatch_MultipleTexts_ReturnsAllCounts()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var texts = new[] { "one", "one two", "one two three" };

        // Act
        var counts = tokenizer.CountTokensBatch(texts);

        // Assert
        counts.Should().HaveCount(3);
        counts[0].Should().Be(1);
        counts[1].Should().Be(2);
        counts[2].Should().Be(3);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        tokenizer.Encode("one two three"); // Build some vocabulary

        // Act
        var result = tokenizer.ToString();

        // Assert
        result.Should().Contain("WordTokenizer");
        result.Should().Contain("vocab_size");
    }
}
