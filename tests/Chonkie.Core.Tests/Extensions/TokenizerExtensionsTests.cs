using Chonkie.Core.Extensions;
using Chonkie.Core.Interfaces;
using Xunit;

namespace Chonkie.Core.Tests.Extensions;

/// <summary>
/// Tests for ITokenizer extension members (C# 14 feature).
/// </summary>
public class TokenizerExtensionsTests
{
    [Fact]
    public void TokenizerName_ReturnsTokenizerTypeName()
    {
        // Arrange
        var tokenizer = new TestTokenizer();

        // Act
        var name = tokenizer.TokenizerName;

        // Assert
        Assert.Equal("Test", name);
    }

    [Fact]
    public void IsEmpty_WithEmptyText_ReturnsTrue()
    {
        // Arrange
        var tokenizer = new TestTokenizer();

        // Act
        var isEmpty = tokenizer.IsEmpty(string.Empty);

        // Assert
        Assert.True(isEmpty);
    }

    [Fact]
    public void IsEmpty_WithNonEmptyText_ReturnsFalse()
    {
        // Arrange
        var tokenizer = new TestTokenizer();

        // Act
        var isEmpty = tokenizer.IsEmpty("some text");

        // Assert
        Assert.False(isEmpty);
    }

    [Fact]
    public async Task EncodeAsync_EncodesTextAsynchronously()
    {
        // Arrange
        var tokenizer = new TestTokenizer();
        var text = "hello world";

        // Act
        var tokens = await tokenizer.EncodeAsync(text);

        // Assert
        Assert.NotNull(tokens);
        Assert.Equal(2, tokens.Count);
    }

    [Fact]
    public async Task DecodeAsync_DecodesTokensAsynchronously()
    {
        // Arrange
        var tokenizer = new TestTokenizer();
        var tokens = new[] { 1, 2 };

        // Act
        var text = await tokenizer.DecodeAsync(tokens);

        // Assert
        Assert.Equal("token1 token2", text);
    }

    [Fact]
    public void MaxTokenLength_ReturnsPositiveValue()
    {
        // Act
        var maxLength = ITokenizer.MaxTokenLength;

        // Assert
        Assert.True(maxLength > 0);
    }

    // Test tokenizer implementation
    private class TestTokenizer : ITokenizer
    {
        public IReadOnlyList<int> Encode(string text)
        {
            return text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select((_, i) => i + 1)
                .ToList();
        }

        public string Decode(IReadOnlyList<int> tokens)
        {
            return string.Join(" ", tokens.Select(t => $"token{t}"));
        }

        public int CountTokens(string text)
        {
            return Encode(text).Count;
        }

        public IReadOnlyList<IReadOnlyList<int>> EncodeBatch(IEnumerable<string> texts)
        {
            return texts.Select(Encode).ToList();
        }

        public IReadOnlyList<string> DecodeBatch(IEnumerable<IReadOnlyList<int>> tokenSequences)
        {
            return tokenSequences.Select(Decode).ToList();
        }

        public IReadOnlyList<int> CountTokensBatch(IEnumerable<string> texts)
        {
            return texts.Select(CountTokens).ToList();
        }
    }
}
