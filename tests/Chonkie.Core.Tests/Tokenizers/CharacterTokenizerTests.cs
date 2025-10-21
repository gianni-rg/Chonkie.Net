using Chonkie.Tokenizers;

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
        Assert.Equal(3, tokens.Count);
        Assert.All(tokens, token => Assert.IsType<int>(token));
    }

    [Fact]
    public void Encode_EmptyString_ReturnsEmptyList()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act
        var tokens = tokenizer.Encode("");

        // Assert
        Assert.Empty(tokens);
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
        Assert.Equal(text, decoded);
    }

    [Fact]
    public void Decode_EmptyList_ReturnsEmptyString()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act
        var decoded = tokenizer.Decode(Array.Empty<int>());

        // Assert
        Assert.Empty(decoded);
    }

    [Fact]
    public void Decode_InvalidTokenId_ThrowsException()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        var invalidTokens = new[] { 99999 };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => tokenizer.Decode(invalidTokens));
        Assert.Contains("not found in vocabulary", ex.Message);
    }

    [Fact]
    public void CountTokens_ReturnsCharacterCount()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();

        // Act & Assert
        Assert.Equal(0, tokenizer.CountTokens(""));
        Assert.Equal(1, tokenizer.CountTokens("a"));
        Assert.Equal(5, tokenizer.CountTokens("Hello"));
        Assert.Equal(12, tokenizer.CountTokens("Hello World!"));
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
            Assert.Equal(text, decoded);
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
        Assert.Equal(3, encoded.Count);
        Assert.Equal(3, encoded[0].Count);
        Assert.Equal(3, encoded[1].Count);
        Assert.Equal(3, encoded[2].Count);
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
        Assert.Equal(3, decoded.Count);
        Assert.Equal("abc", decoded[0]);
        Assert.Equal("def", decoded[1]);
        Assert.Equal("ghi", decoded[2]);
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
        Assert.Equal(3, counts.Count);
        Assert.Equal(1, counts[0]);
        Assert.Equal(2, counts[1]);
        Assert.Equal(3, counts[2]);
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
        Assert.Contains("CharacterTokenizer", result);
        Assert.Contains("vocab_size", result);
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
        Assert.Equal(text, decoded);
        Assert.Equal(text.Length, tokens.Count);
    }

    [Fact]
    public void Encode_WhitespaceVariations_PreservesWhitespace()
    {
        // Arrange
        var tokenizer = new CharacterTokenizer();
        
        // Test multiple spaces
        var textWithSpaces = "hello    world";
        var tokensSpaces = tokenizer.Encode(textWithSpaces);
        Assert.Equal(textWithSpaces.Length, tokensSpaces.Count);
        Assert.Equal(textWithSpaces, tokenizer.Decode(tokensSpaces));

        // Test tabs and newlines
        var textWithWhitespace = "hello\tworld\ntest";
        var tokensWhitespace = tokenizer.Encode(textWithWhitespace);
        Assert.Equal(textWithWhitespace, tokenizer.Decode(tokensWhitespace));

        // Test leading/trailing spaces
        var textPadded = "  hello world  ";
        var tokensPadded = tokenizer.Encode(textPadded);
        Assert.Equal(textPadded, tokenizer.Decode(tokensPadded));
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
        Assert.Equal(largeText.Length, tokens.Count);
        Assert.Equal(largeText, decoded);
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
        Assert.Equal(numericText, decoded);
        Assert.Equal(numericText.Length, tokens.Count);
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
        Assert.Equal(vocabAfterFirst, vocabAfterRepeat);
        
        // Encode new text - vocab should grow
        var text2 = "xyz";
        tokenizer.Encode(text2);
        var vocabAfterNew = tokenizer.GetVocabulary().Count;
        Assert.True(vocabAfterNew > vocabAfterRepeat);
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
        Assert.Equal(countFromEncode, countDirect);
        Assert.Equal(text.Length, countDirect);
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
        Assert.Contains("H", vocab);
        Assert.Contains("e", vocab);
        Assert.Contains("l", vocab);
        Assert.Contains("o", vocab);
        Assert.Contains(" ", vocab);
        Assert.Contains("W", vocab);
        Assert.Contains("r", vocab);
        Assert.Contains("d", vocab);
        Assert.Contains("!", vocab);

        // Verify mapping consistency
        foreach (var character in text)
        {
            var charStr = character.ToString();
            Assert.Contains(charStr, vocab);
            Assert.True(tokenMapping.ContainsKey(charStr));
        }
    }
}
