namespace Chonkie.Fetchers.Tests.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Fetchers;
using Chonkie.Fetchers.Extensions;
using Xunit;

/// <summary>
/// Tests for IFetcher extension members (C# 14).
/// </summary>
public class FetcherExtensionsTests
{
    private class TestFetcher : IFetcher
    {
        private readonly Dictionary<string, List<(string Path, string Content)>> _data;

        public TestFetcher()
        {
            _data = new Dictionary<string, List<(string Path, string Content)>>
            {
                ["dir1"] = new List<(string, string)>
                {
                    ("dir1/file1.txt", "Content 1"),
                    ("dir1/file2.txt", "Content 2")
                },
                ["dir2"] = new List<(string, string)>
                {
                    ("dir2/file3.txt", "Content 3")
                },
                ["file.txt"] = new List<(string, string)>
                {
                    ("file.txt", "Single file content")
                }
            };
        }

        public Task<IReadOnlyList<(string Path, string Content)>> FetchAsync(
            string path,
            string? filter = null,
            CancellationToken cancellationToken = default)
        {
            if (_data.TryGetValue(path, out var files))
            {
                return Task.FromResult<IReadOnlyList<(string Path, string Content)>>(files);
            }
            return Task.FromResult<IReadOnlyList<(string Path, string Content)>>(
                Array.Empty<(string Path, string Content)>());
        }
    }

    [Fact]
    public void FetcherType_ReturnsFetcherTypeName()
    {
        // Arrange
        var fetcher = new FileFetcher();

        // Act
        var type = fetcher.FetcherType;

        // Assert
        Assert.Equal("File", type);
    }

    [Fact]
    public async Task FetchSingleAsync_WithExistingFile_ReturnsContent()
    {
        // Arrange
        var fetcher = new TestFetcher();

        // Act
        var content = await fetcher.FetchSingleAsync("file.txt");

        // Assert
        Assert.NotNull(content);
        Assert.Equal("Single file content", content);
    }

    [Fact]
    public async Task FetchSingleAsync_WithNonExistingFile_ReturnsNull()
    {
        // Arrange
        var fetcher = new TestFetcher();

        // Act
        var content = await fetcher.FetchSingleAsync("nonexistent.txt");

        // Assert
        Assert.Null(content);
    }

    [Fact]
    public async Task FetchMultipleAsync_WithMultiplePaths_ReturnsAllDocuments()
    {
        // Arrange
        var fetcher = new TestFetcher();
        var paths = new[] { "dir1", "dir2" };

        // Act
        var results = await fetcher.FetchMultipleAsync(paths);

        // Assert
        Assert.Equal(3, results.Count); // 2 from dir1 + 1 from dir2
        Assert.Contains(results, r => r.Path == "dir1/file1.txt");
        Assert.Contains(results, r => r.Path == "dir1/file2.txt");
        Assert.Contains(results, r => r.Path == "dir2/file3.txt");
    }

    [Fact]
    public async Task FetchMultipleAsync_WithEmptyPaths_ReturnsEmpty()
    {
        // Arrange
        var fetcher = new TestFetcher();
        var paths = Array.Empty<string>();

        // Act
        var results = await fetcher.FetchMultipleAsync(paths);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task FetchMultipleAsync_WithCancellation_CanBeCancelled()
    {
        // Arrange
        var fetcher = new TestFetcher();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var paths = Enumerable.Range(0, 100).Select(i => $"path{i}");

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => fetcher.FetchMultipleAsync(paths, null, cts.Token));
    }

    [Fact]
    public async Task CountDocumentsAsync_ReturnsCorrectCount()
    {
        // Arrange
        var fetcher = new TestFetcher();

        // Act
        var count = await fetcher.CountDocumentsAsync("dir1");

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CountDocumentsAsync_WithNonExistingPath_ReturnsZero()
    {
        // Arrange
        var fetcher = new TestFetcher();

        // Act
        var count = await fetcher.CountDocumentsAsync("nonexistent");

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void CommonTextExtensions_ReturnsExpectedExtensions()
    {
        // Act
        var extensions = IFetcher.CommonTextExtensions;

        // Assert
        Assert.NotEmpty(extensions);
        Assert.Contains(".txt", extensions);
        Assert.Contains(".md", extensions);
        Assert.Contains(".json", extensions);
    }

    [Fact]
    public void CommonCodeExtensions_ReturnsExpectedExtensions()
    {
        // Act
        var extensions = IFetcher.CommonCodeExtensions;

        // Assert
        Assert.NotEmpty(extensions);
        Assert.Contains(".cs", extensions);
        Assert.Contains(".py", extensions);
        Assert.Contains(".js", extensions);
        Assert.Contains(".ts", extensions);
    }
}
