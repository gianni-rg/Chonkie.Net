namespace Chonkie.Chefs.Tests.Extensions;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Chefs.Extensions;
using Xunit;

/// <summary>
/// Tests for IChef extension members (C# 14).
/// </summary>
public class ChefExtensionsTests
{
    private class TestChef : IChef
    {
        private readonly string _suffix;

        public TestChef(string suffix = "")
        {
            _suffix = suffix;
        }

        public Task<string> ProcessAsync(string text, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(text + _suffix);
        }
    }

    [Fact]
    public void ChefType_ReturnsChefTypeName()
    {
        // Arrange
        var chef = new TextChef();

        // Act
        var type = chef.ChefType;

        // Assert
        Assert.Equal("Text", type);
    }

    [Fact]
    public async Task ProcessBatchAsync_ProcessesMultipleTexts()
    {
        // Arrange
        var chef = new TestChef(" processed");
        var texts = new[] { "text1", "text2", "text3" };

        // Act
        var results = await chef.ProcessBatchAsync(texts);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal("text1 processed", results[0]);
        Assert.Equal("text2 processed", results[1]);
        Assert.Equal("text3 processed", results[2]);
    }

    [Fact]
    public async Task ProcessBatchAsync_WithEmptyList_ReturnsEmpty()
    {
        // Arrange
        var chef = new TextChef();
        var texts = Array.Empty<string>();

        // Act
        var results = await chef.ProcessBatchAsync(texts);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task ProcessBatchAsync_WithCancellation_CanBeCancelled()
    {
        // Arrange
        var chef = new TestChef();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var texts = Enumerable.Range(0, 100).Select(i => $"text{i}");

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => chef.ProcessBatchAsync(texts, cts.Token));
    }

    [Fact]
    public async Task WouldModifyAsync_WithModifyingChef_ReturnsTrue()
    {
        // Arrange
        var chef = new TestChef(" modified");
        var text = "original";

        // Act
        var wouldModify = await chef.WouldModifyAsync(text);

        // Assert
        Assert.True(wouldModify);
    }

    [Fact]
    public async Task WouldModifyAsync_WithNonModifyingChef_ReturnsFalse()
    {
        // Arrange
        var chef = new TestChef(""); // Returns same text
        var text = "original";

        // Act
        var wouldModify = await chef.WouldModifyAsync(text);

        // Assert
        Assert.False(wouldModify);
    }

    [Fact]
    public void Empty_ReturnsEmptyString()
    {
        // Act
        var empty = IChef.Empty;

        // Assert
        Assert.Equal(string.Empty, empty);
    }
}
