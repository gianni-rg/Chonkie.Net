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

    #region Additional Extension Member Tests

    [Fact]
    public void TokenizerName_WithDifferentTokenizers_ReturnsCorrectNames()
    {
        // Arrange
        var testTokenizer = new TestTokenizer();
        var customTokenizer = new CustomTestTokenizer();

        // Act
        var testName = testTokenizer.TokenizerName;
        var customName = customTokenizer.TokenizerName;

        // Assert
        Assert.Equal("Test", testName);
        Assert.Equal("CustomTest", customName);
    }

    [Theory]
    [InlineData("\t")]
    [InlineData("\n")]
    public void IsEmpty_WithWhitespaceText_ReturnsFalse(string text)
    {
        // Arrange
        var tokenizer = new TestTokenizer();

        // Act
        var isEmpty = tokenizer.IsEmpty(text);

        // Assert - tabs and newlines are not split by space delimiter, so they produce tokens
        Assert.False(isEmpty);
    }

    [Fact]
    public async Task EncodeAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var tokenizer = new TestTokenizer();
        var text = "hello world test";
        using var cts = new CancellationTokenSource();

        // Act
        var tokens = await tokenizer.EncodeAsync(text, cts.Token);

        // Assert
        Assert.NotNull(tokens);
        Assert.Equal(3, tokens.Count);
    }

    [Fact]
    public async Task DecodeAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var tokenizer = new TestTokenizer();
        var tokens = new[] { 1, 2, 3 };
        using var cts = new CancellationTokenSource();

        // Act
        var text = await tokenizer.DecodeAsync(tokens, cts.Token);

        // Assert
        Assert.Equal("token1 token2 token3", text);
    }

    [Fact]
    public async Task EncodeAsync_WithEmptyString_ReturnsEmptyList()
    {
        // Arrange
        var tokenizer = new TestTokenizer();

        // Act
        var tokens = await tokenizer.EncodeAsync(string.Empty);

        // Assert
        Assert.NotNull(tokens);
        Assert.Empty(tokens);
    }

    [Fact]
    public async Task DecodeAsync_WithEmptyList_ReturnsEmptyString()
    {
        // Arrange
        var tokenizer = new TestTokenizer();

        // Act
        var text = await tokenizer.DecodeAsync(Array.Empty<int>());

        // Assert
        Assert.Equal(string.Empty, text);
    }

    [Fact]
    public async Task EncodeAsync_MultipleCallsConcurrently_AllSucceed()
    {
        // Arrange
        var tokenizer = new TestTokenizer();
        var texts = new[] { "text one", "text two", "text three" };

        // Act
        var tasks = texts.Select(t => tokenizer.EncodeAsync(t)).ToList();
        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(3, results.Length);
        Assert.All(results, r => Assert.NotNull(r));
        Assert.All(results, r => Assert.Equal(2, r.Count));
    }

    [Fact]
    public void MaxTokenLength_IsConstant()
    {
        // Act
        var length1 = ITokenizer.MaxTokenLength;
        var length2 = ITokenizer.MaxTokenLength;

        // Assert
        Assert.Equal(length1, length2);
        Assert.Equal(1024 * 1024, length1);
    }

    [Fact]
    public void IsEmpty_WithNullText_ThrowsException()
    {
        // Arrange
        var tokenizer = new TestTokenizer();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => tokenizer.IsEmpty(null!));
    }

    #endregion

    // Test tokenizer implementation
    private class TestTokenizer : ITokenizer
    {
        public IReadOnlyList<int> Encode(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
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
            if (text == null) throw new ArgumentNullException(nameof(text));
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

    private class CustomTestTokenizer : ITokenizer
    {
        public IReadOnlyList<int> Encode(string text) => Array.Empty<int>();
        public string Decode(IReadOnlyList<int> tokens) => string.Empty;
        public int CountTokens(string text) => 0;
        public IReadOnlyList<IReadOnlyList<int>> EncodeBatch(IEnumerable<string> texts) => Array.Empty<IReadOnlyList<int>>();
        public IReadOnlyList<string> DecodeBatch(IEnumerable<IReadOnlyList<int>> tokenSequences) => Array.Empty<string>();
        public IReadOnlyList<int> CountTokensBatch(IEnumerable<string> texts) => Array.Empty<int>();
    }
}
