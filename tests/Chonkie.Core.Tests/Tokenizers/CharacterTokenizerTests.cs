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

    [Fact]
    public void Encode_SpecialCharactersAndUnicode_HandlesCorrectly()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var text = "Hello! 😀 你好 🌍 Café naïve résumé";

        // Act
        var tokens = tokenizer.Encode(text);
        var decoded = tokenizer.Decode(tokens);

        // Assert
        decoded.Should().Be(text);
        tokens.Should().HaveCount(text.Length);
    }

    [Fact]
    public void Encode_WhitespaceVariations_PreservesWhitespace()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        
        // Test multiple spaces
        var textWithSpaces = "hello    world";
        var tokensSpaces = tokenizer.Encode(textWithSpaces);
        tokensSpaces.Should().HaveCount(textWithSpaces.Length);
        tokenizer.Decode(tokensSpaces).Should().Be(textWithSpaces);

        // Test tabs and newlines
        var textWithWhitespace = "hello\tworld\ntest";
        var tokensWhitespace = tokenizer.Encode(textWithWhitespace);
        tokenizer.Decode(tokensWhitespace).Should().Be(textWithWhitespace);

        // Test leading/trailing spaces
        var textPadded = "  hello world  ";
        var tokensPadded = tokenizer.Encode(textPadded);
        tokenizer.Decode(tokensPadded).Should().Be(textPadded);
    }

    [Fact]
    public void Encode_LargeText_HandlesEfficiently()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var baseText = "The quick brown fox jumps over the lazy dog. ";
        var largeText = string.Concat(Enumerable.Repeat(baseText, 100)); // 4500+ characters

        // Act
        var tokens = tokenizer.Encode(largeText);
        var decoded = tokenizer.Decode(tokens);

        // Assert
        tokens.Should().HaveCount(largeText.Length);
        decoded.Should().Be(largeText);
    }

    [Fact]
    public void Encode_NumericContent_HandlesCorrectly()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var numericText = "123 456.789 -10 +20 1.23e-4";

        // Act
        var tokens = tokenizer.Encode(numericText);
        var decoded = tokenizer.Decode(tokens);

        // Assert
        decoded.Should().Be(numericText);
        tokens.Should().HaveCount(numericText.Length);
    }

    [Fact]
    public void Vocabulary_PersistsAcrossOperations()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        
        // Encode first text
        var text1 = "hello";
        tokenizer.Encode(text1);
        var vocabAfterFirst = tokenizer.GetVocabulary().Count;
        
        // Encode same text again - vocab should not grow
        tokenizer.Encode(text1);
        var vocabAfterRepeat = tokenizer.GetVocabulary().Count;
        vocabAfterFirst.Should().Be(vocabAfterRepeat);
        
        // Encode new text - vocab should grow
        var text2 = "xyz";
        tokenizer.Encode(text2);
        var vocabAfterNew = tokenizer.GetVocabulary().Count;
        vocabAfterNew.Should().BeGreaterThan(vocabAfterRepeat);
    }

    [Fact]
    public void CountTokens_ConsistentWithEncode()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var text = "The quick brown fox jumps over the lazy dog.";

        // Act
        var countDirect = tokenizer.CountTokens(text);
        var countFromEncode = tokenizer.Encode(text).Count;

        // Assert
        countDirect.Should().Be(countFromEncode);
        countDirect.Should().Be(text.Length);
    }

    [Fact]
    public void GetVocabulary_ReturnsAllEncodedCharacters()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var text = "Hello World!";

        // Act
        tokenizer.Encode(text);
        var vocab = tokenizer.GetVocabulary();
        var tokenMapping = tokenizer.GetTokenMapping();

        // Assert
        vocab.Should().Contain("H");
        vocab.Should().Contain("e");
        vocab.Should().Contain("l");
        vocab.Should().Contain("o");
        vocab.Should().Contain(" ");
        vocab.Should().Contain("W");
        vocab.Should().Contain("r");
        vocab.Should().Contain("d");
        vocab.Should().Contain("!");

        // Verify mapping consistency
        foreach (var character in text)
        {
            var charStr = character.ToString();
            vocab.Should().Contain(charStr);
            tokenMapping.Should().ContainKey(charStr);
        }
    }
}
