using Chonkie.Tokenizers;
using FluentAssertions;

namespace Chonkie.Core.Tests.Tokenizers;

public class CharacterTokenizerTests
{
    [Fact]
    public void Encode_SimpleText_ReturnsTokenIds()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var text = "abc";

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
        var tokenizer = new CharacterTokenizer();

        // Act
        var tokens = tokenizer.Encode("");

        // Assert
        tokens.Should().BeEmpty();
    }

    [Fact]
    public void Decode_TokenIds_ReturnsOriginalText()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var text = "Hello";
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
        var tokenizer = new CharacterTokenizer();

        // Act
        var decoded = tokenizer.Decode(Array.Empty<int>());

        // Assert
        decoded.Should().BeEmpty();
    }

    [Fact]
    public void Decode_InvalidTokenId_ThrowsException()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var invalidTokens = new[] { 99999 };

        // Act
        var act = () => tokenizer.Decode(invalidTokens);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*not found in vocabulary*");
    }

    [Fact]
    public void CountTokens_ReturnsCharacterCount()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act & Assert
        tokenizer.CountTokens("").Should().Be(0);
        tokenizer.CountTokens("a").Should().Be(1);
        tokenizer.CountTokens("Hello").Should().Be(5);
        tokenizer.CountTokens("Hello World!").Should().Be(12);
    }

    [Fact]
    public void EncodeDecode_RoundTrip_PreservesText()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var texts = new[]
        {
            "Simple text",
            "Text with numbers 123",
            "Special chars: !@#$%",
            "Unicode: 你好世界 🦛",
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
        var tokenizer = new CharacterTokenizer();
        var texts = new[] { "abc", "def", "ghi" };

        // Act
        var encoded = tokenizer.EncodeBatch(texts);

        // Assert
        encoded.Should().HaveCount(3);
        encoded[0].Should().HaveCount(3);
        encoded[1].Should().HaveCount(3);
        encoded[2].Should().HaveCount(3);
    }

    [Fact]
    public void DecodeBatch_MultipleTokenSequences_ReturnsAllDecoded()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var texts = new[] { "abc", "def", "ghi" };
        var encoded = tokenizer.EncodeBatch(texts);

        // Act
        var decoded = tokenizer.DecodeBatch(encoded);

        // Assert
        decoded.Should().HaveCount(3);
        decoded[0].Should().Be("abc");
        decoded[1].Should().Be("def");
        decoded[2].Should().Be("ghi");
    }

    [Fact]
    public void CountTokensBatch_MultipleTexts_ReturnsAllCounts()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var texts = new[] { "a", "ab", "abc" };

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
        var tokenizer = new CharacterTokenizer();
        tokenizer.Encode("abc"); // Build some vocabulary

        // Act
        var result = tokenizer.ToString();

        // Assert
        result.Should().Contain("CharacterTokenizer");
        result.Should().Contain("vocab_size");
    }
}
