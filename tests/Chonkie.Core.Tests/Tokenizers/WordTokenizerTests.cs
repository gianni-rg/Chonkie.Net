using Chonkie.Tokenizers;

namespace Chonkie.Core.Tests.Tokenizers;

/// <summary>
/// Unit tests for the <see cref="WordTokenizer"/> class.
/// </summary>
public class WordTokenizerTests
{
    [Fact]
    /// <summary>
    /// Tests encoding of simple text returns correct token IDs.
    /// </summary>
    public void Encode_SimpleText_ReturnsTokenIds()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var text = "hello world test";

        // Act
        var tokens = tokenizer.Encode(text);

        // Assert
        Assert.Equal(3, tokens.Count); // This is correct for multiple items, but if you want to check for a single item, use Assert.Single(tokens)
        Assert.All(tokens, token => Assert.IsType<int>(token));
    }

    [Fact]
    public void Encode_EmptyString_ReturnsEmptyList()
    {
        // Arrange
        var tokenizer = new WordTokenizer();

        // Act
        var tokens = tokenizer.Encode("");

        // Assert
        Assert.Empty(tokens);
    }

    [Fact]
    public void Encode_SingleWord_ReturnsOneToken()
    {
        // Arrange
        var tokenizer = new WordTokenizer();

        // Act
        var tokens = tokenizer.Encode("hello");

        // Assert
        Assert.Single(tokens);
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
        Assert.Equal(text, decoded);
    }

    [Fact]
    public void Decode_EmptyList_ReturnsEmptyString()
    {
        // Arrange
        var tokenizer = new WordTokenizer();

        // Act
        var decoded = tokenizer.Decode(Array.Empty<int>());

        // Assert
        Assert.Empty(decoded);
    }

    [Fact]
    public void Decode_InvalidTokenId_ThrowsException()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var invalidTokens = new[] { 99999 };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => tokenizer.Decode(invalidTokens));
        Assert.Contains("not found in vocabulary", ex.Message);
    }

    [Fact]
    public void CountTokens_ReturnsWordCount()
    {
        // Arrange
        var tokenizer = new WordTokenizer();

        // Act & Assert
        Assert.Equal(0, tokenizer.CountTokens(""));
        Assert.Equal(1, tokenizer.CountTokens("word"));
        Assert.Equal(2, tokenizer.CountTokens("two words"));
        Assert.Equal(4, tokenizer.CountTokens("the quick brown fox"));
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
        Assert.Equal(5, count);
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
            Assert.Equal(text, decoded);
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
        Assert.Equal(3, encoded.Count);
        Assert.Equal(2, encoded[0].Count);
        Assert.Equal(2, encoded[1].Count);
        Assert.Single(encoded[2]);
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
        Assert.Equal(3, decoded.Count);
        Assert.Equal("one two", decoded[0]);
        Assert.Equal("three four", decoded[1]);
        Assert.Equal("five", decoded[2]);
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
        Assert.Equal(3, counts.Count);
        Assert.Equal(1, counts[0]);
        Assert.Equal(2, counts[1]);
        Assert.Equal(3, counts[2]);
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
        Assert.Contains("WordTokenizer", result);
        Assert.Contains("vocab_size", result);
    }

    [Fact]
    public void Encode_SpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var text = "Hello! 😀 你好 🌍 Café naïve résumé";

        // Act
        var tokens = tokenizer.Encode(text);
        var decoded = tokenizer.Decode(tokens);

        // Assert
        Assert.Equal(text, decoded);
    }

    [Fact]
    public void Encode_WhitespaceVariations_PreservesWhitespace()
    {
        // Arrange
        var tokenizer = new WordTokenizer();

        // Test tabs and newlines
        var textWithWhitespace = "hello\tworld\ntest";
        var tokens = tokenizer.Encode(textWithWhitespace);
        Assert.Equal(textWithWhitespace, tokenizer.Decode(tokens));

        // Test leading/trailing spaces
        var textPadded = "  hello world  ";
        var tokensPadded = tokenizer.Encode(textPadded);
        Assert.Equal(textPadded, tokenizer.Decode(tokensPadded));
    }

    [Fact]
    public void Encode_SingleCharacterWords_HandlesSeparately()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var text = "I a m t e s t i n g";

        // Act
        var tokens = tokenizer.Encode(text);
        var decoded = tokenizer.Decode(tokens);

        // Assert
        Assert.Equal(text, decoded);
        Assert.Equal(text.Split(' ').Length, tokenizer.CountTokens(text));
    }

    [Fact]
    public void Encode_LargeText_HandlesEfficiently()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var baseText = "The quick brown fox jumps over the lazy dog. ";
        var largeText = string.Concat(Enumerable.Repeat(baseText, 100));

        // Act
        var tokens = tokenizer.Encode(largeText);
        var decoded = tokenizer.Decode(tokens);

        // Assert
        Assert.Equal(largeText, decoded);
    }

    [Fact]
    public void Encode_NumericContent_HandlesCorrectly()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var numericText = "123 456.789 -10 +20 1.23e-4";

        // Act
        var tokens = tokenizer.Encode(numericText);
        var decoded = tokenizer.Decode(tokens);

        // Assert
        Assert.Equal(numericText, decoded);
    }

    [Fact]
    public void Vocabulary_PersistsAndGrowsCorrectly()
    {
        // Arrange
        var tokenizer = new WordTokenizer();

        var text1 = "Wall-E is truly a masterpiece that should be required viewing.";
        var text2 = "Ratatouille is truly a delightful film that every kid should watch.";

        // Act
        tokenizer.Encode(text1);
        var vocabSize1 = tokenizer.GetVocabulary().Count;
        tokenizer.Encode(text2);
        var vocabSize2 = tokenizer.GetVocabulary().Count;

        // Assert
        Assert.True(vocabSize2 > vocabSize1);
        Assert.Contains("Wall-E", tokenizer.GetVocabulary());
        Assert.Contains("Ratatouille", tokenizer.GetVocabulary());
        Assert.True(tokenizer.GetTokenMapping().ContainsKey("truly"));
    }

    [Fact]
    public void CountTokens_ConsistentWithEncode()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var text = "The quick brown fox jumps over the lazy dog.";

        // Act
        var countDirect = tokenizer.CountTokens(text);
        var countFromEncode = tokenizer.Encode(text).Count;

        // Assert
        Assert.Equal(countFromEncode, countDirect);
    }

    [Fact]
    public void Encode_WithMultipleSpaces_HandlesCorrectly()
    {
        // Arrange
        var tokenizer = new WordTokenizer();
        var text = "hello  world   test";

        // Act
        var tokens = tokenizer.Encode(text);
        var decoded = tokenizer.Decode(tokens);

        // Assert
        Assert.Equal(text, decoded);
        // Splitting on space will create empty strings for consecutive spaces
        Assert.Equal(text.Split(' ').Length, tokens.Count);
    }
}
