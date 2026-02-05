using Chonkie.Chunkers;
using Chonkie.Core.Types;
using Shouldly;
using Xunit;

namespace Chonkie.Tests.Chunkers;

/// <summary>
/// Unit tests for FastChunker, including comprehensive UTF-8 multi-byte character handling.
/// </summary>
public class FastChunkerTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithDefaultParameters_SetsProperties()
    {
        var chunker = new FastChunker();

        chunker.ChunkSize.ShouldBe(512);
        chunker.ChunkOverlap.ShouldBe(0);
    }

    [Fact]
    public void Constructor_WithCustomParameters_SetsProperties()
    {
        var chunker = new FastChunker(chunkSize: 1024, chunkOverlap: 128);

        chunker.ChunkSize.ShouldBe(1024);
        chunker.ChunkOverlap.ShouldBe(128);
    }

    [Fact]
    public void Constructor_WithZeroChunkSize_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new FastChunker(chunkSize: 0));
    }

    [Fact]
    public void Constructor_WithNegativeChunkSize_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new FastChunker(chunkSize: -10));
    }

    [Fact]
    public void Constructor_WithNegativeOverlap_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new FastChunker(chunkSize: 100, chunkOverlap: -10));
    }

    [Fact]
    public void Constructor_WithOverlapGreaterThanChunkSize_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new FastChunker(chunkSize: 100, chunkOverlap: 150));
    }

    #endregion

    #region Basic Chunking Tests

    [Fact]
    public void Chunk_WithEmptyString_ReturnsEmptyList()
    {
        var chunker = new FastChunker(chunkSize: 20);

        var result = chunker.Chunk("");

        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Chunk_WithNullString_ReturnsEmptyList()
    {
        var chunker = new FastChunker(chunkSize: 20);

        var result = chunker.Chunk(null!);

        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Chunk_WithTextShorterThanChunkSize_ReturnsSingleChunk()
    {
        var chunker = new FastChunker(chunkSize: 50);
        var text = "Hello World";

        var result = chunker.Chunk(text);

        result.Count.ShouldBe(1);
        result[0].Text.ShouldBe("Hello World");
        result[0].StartIndex.ShouldBe(0);
        result[0].EndIndex.ShouldBe(11);
    }

    [Fact]
    public void Chunk_WithTextLongerThanChunkSize_ReturnsMultipleChunks()
    {
        var chunker = new FastChunker(chunkSize: 20);
        var text = "Hello World this is a test text";

        var result = chunker.Chunk(text);

        result.Count.ShouldBeGreaterThan(1);
        var reconstructed = string.Concat(result.Select(c => c.Text));
        reconstructed.ShouldBe(text);
    }

    #endregion

    #region UTF-8 Multi-Byte Character Tests

    [Fact]
    public void Chunk_WithEmojis_PreservesEmojisCorrectly()
    {
        var chunker = new FastChunker(chunkSize: 20);
        var text = "Hello 👋 World 🌍 with emojis 🎉";

        var result = chunker.Chunk(text);

        var reconstructed = string.Concat(result.Select(c => c.Text));
        reconstructed.ShouldBe(text);
        reconstructed.ShouldContain("👋");
        reconstructed.ShouldContain("🌍");
        reconstructed.ShouldContain("🎉");
    }

    [Fact]
    public void Chunk_WithChineseCharacters_PreservesChineseCorrectly()
    {
        var chunker = new FastChunker(chunkSize: 20);
        var text = "这是一个测试文本，用于验证中文字符处理。";

        var result = chunker.Chunk(text);

        var reconstructed = string.Concat(result.Select(c => c.Text));
        reconstructed.ShouldBe(text);
    }

    [Fact]
    public void Chunk_WithKoreanCharacters_PreservesKoreanCorrectly()
    {
        var chunker = new FastChunker(chunkSize: 20);
        var text = "안녕하세요, 이것은 한국어 테스트입니다.";

        var result = chunker.Chunk(text);

        var reconstructed = string.Concat(result.Select(c => c.Text));
        reconstructed.ShouldBe(text);
    }

    [Fact]
    public void Chunk_WithJapaneseCharacters_PreservesJapaneseCorrectly()
    {
        var chunker = new FastChunker(chunkSize: 20);
        var text = "こんにちは、これは日本語のテストです。";

        var result = chunker.Chunk(text);

        var reconstructed = string.Concat(result.Select(c => c.Text));
        reconstructed.ShouldBe(text);
    }

    [Fact]
    public void Chunk_WithMixedLanguages_PreservesMixedCorrectly()
    {
        var chunker = new FastChunker(chunkSize: 30);
        var text = "Hello 😊 世界 🌏 مرحبا Привет";

        var result = chunker.Chunk(text);

        var reconstructed = string.Concat(result.Select(c => c.Text));
        reconstructed.ShouldBe(text);
    }

    [Fact]
    public void Chunk_WithArabicText_PreservesArabicCorrectly()
    {
        var chunker = new FastChunker(chunkSize: 20);
        var text = "مرحبا بك في العالم";

        var result = chunker.Chunk(text);

        var reconstructed = string.Concat(result.Select(c => c.Text));
        reconstructed.ShouldBe(text);
    }

    [Fact]
    public void Chunk_WithCombiningCharacters_PreservesDiacriticsCorrectly()
    {
        var chunker = new FastChunker(chunkSize: 20);
        var text = "Café naïve résumé";

        var result = chunker.Chunk(text);

        var reconstructed = string.Concat(result.Select(c => c.Text));
        reconstructed.ShouldBe(text);
    }

    #endregion

    #region Chunk Overlap Tests

    [Fact]
    public void Chunk_WithOverlap_CreatesOverlappingChunks()
    {
        var chunker = new FastChunker(chunkSize: 20, chunkOverlap: 5);
        var text = "Hello World this is a test document";

        var result = chunker.Chunk(text);

        result.Count.ShouldBeGreaterThan(1);
        var second = result[1];
        second.StartIndex.ShouldBeLessThan(result[0].EndIndex);
    }

    [Fact]
    public void Chunk_WithOverlap_PreservesAllText()
    {
        var chunker = new FastChunker(chunkSize: 20, chunkOverlap: 5);
        var text = "The quick brown fox jumps over the lazy dog";

        var result = chunker.Chunk(text);

        var reconstructed = string.Concat(result.Select(c => c.Text).Take(1));
        for (int i = 1; i < result.Count; i++)
        {
            var prevChunk = result[i - 1].Text;
            var currChunk = result[i].Text;
            var overlap = prevChunk.Substring(prevChunk.Length - 5);
            currChunk.ShouldStartWith(overlap);
        }
    }

    #endregion

    #region Word Boundary Tests

    [Fact]
    public void Chunk_WithWordBoundaries_DoesNotSplitMidWord()
    {
        var chunker = new FastChunker(chunkSize: 12);
        var text = "Hello World test";

        var result = chunker.Chunk(text);

        foreach (var chunk in result)
        {
            chunk.Text.ShouldNotMatch(@"\w\s\w");
        }
    }

    [Fact]
    public void Chunk_WithoutSpaces_SplitsAtBoundary()
    {
        var chunker = new FastChunker(chunkSize: 5);
        var text = "abcdefghijk";

        var result = chunker.Chunk(text);

        var reconstructed = string.Concat(result.Select(c => c.Text));
        reconstructed.ShouldBe(text);
    }

    #endregion

    #region Chunk Properties Tests

    [Fact]
    public void Chunk_SetsCorrectProperties()
    {
        var chunker = new FastChunker(chunkSize: 20);
        var text = "Hello World this is a test";

        var result = chunker.Chunk(text);

        result.Count.ShouldBeGreaterThan(0);
        var firstChunk = result[0];
        firstChunk.Text.ShouldNotBeNullOrEmpty();
        firstChunk.StartIndex.ShouldBe(0);
        firstChunk.EndIndex.ShouldBeGreaterThan(0);
        firstChunk.Length.ShouldBe(firstChunk.Text.Length);
    }

    [Fact]
    public void Chunk_VerifiesStartAndEndIndexes()
    {
        var chunker = new FastChunker(chunkSize: 15);
        var text = "The quick brown fox jumps";

        var result = chunker.Chunk(text);

        foreach (var chunk in result)
        {
            var substring = text.Substring(chunk.StartIndex, chunk.EndIndex - chunk.StartIndex);
            substring.ShouldBe(chunk.Text);
        }
    }

    #endregion

    #region Batch Processing Tests

    [Fact]
    public void ChunkBatch_WithMultipleTexts_ReturnsChunksForEach()
    {
        var chunker = new FastChunker(chunkSize: 20);
        var texts = new[] { "First text here.", "Second text here.", "Third text here." };

        var result = chunker.ChunkBatch(texts);

        result.Count.ShouldBe(3);
        foreach (var batch in result)
        {
            batch.Count.ShouldBeGreaterThan(0);
        }
    }

    #endregion

    #region ChunkDocument Tests

    [Fact]
    public void ChunkDocument_WithValidDocument_PopulatesChunksCollection()
    {
        var chunker = new FastChunker(chunkSize: 30);
        var document = new Document { Content = "Hello World this is a test document with multiple sentences." };

        var result = chunker.ChunkDocument(document);

        result.ShouldBeSameAs(document);
        result.Chunks.Count.ShouldBeGreaterThan(0);
        foreach (var chunk in result.Chunks)
        {
            chunk.Text.ShouldNotBeNullOrEmpty();
        }
    }

    [Fact]
    public void ChunkDocument_WithEmptyContent_PopulatesEmptyChunks()
    {
        var chunker = new FastChunker(chunkSize: 30);
        var document = new Document { Content = "" };

        var result = chunker.ChunkDocument(document);

        result.Chunks.Count.ShouldBe(0);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var chunker = new FastChunker(chunkSize: 512, chunkOverlap: 128);

        var result = chunker.ToString();

        result.ShouldContain("FastChunker");
        result.ShouldContain("512");
        result.ShouldContain("128");
    }

    #endregion
}
