namespace Chonkie.Core.Tests.Chunkers;

using Chonkie.Chunkers;
using Chonkie.Core.Types;
using Chonkie.Tokenizers;
using Shouldly;
using Xunit;

/// <summary>
/// Unit tests for NeuralChunker.
/// 
/// NeuralChunker is a placeholder implementation that delegates to RecursiveChunker
/// until ONNX models are provided for neural-based split point detection.
/// </summary>
public class NeuralChunkerTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullTokenizer_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new NeuralChunker(null!, chunkSize: 1024));
    }

    [Fact]
    public void Constructor_WithValidTokenizer_SetsProperties()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 1024);

        chunker.ChunkSize.ShouldBe(1024);
    }

    [Fact]
    public void Constructor_WithCustomChunkSize_SetsProperties()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 2048);

        chunker.ChunkSize.ShouldBe(2048);
    }

    [Fact]
    public void Constructor_WithZeroChunkSize_ThrowsArgumentException()
    {
        var tokenizer = new CharacterTokenizer();

        Should.Throw<ArgumentException>(() => new NeuralChunker(tokenizer, chunkSize: 0));
    }

    [Fact]
    public void Constructor_WithNegativeChunkSize_ThrowsArgumentException()
    {
        var tokenizer = new CharacterTokenizer();

        Should.Throw<ArgumentException>(() => new NeuralChunker(tokenizer, chunkSize: -1));
    }

    #endregion

    #region Chunking Tests

    [Fact]
    public void Chunk_WithEmptyString_ReturnsEmptyList()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 1024);

        var result = chunker.Chunk("");

        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Chunk_WithNullString_ReturnsEmptyList()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 1024);

        var result = chunker.Chunk(null!);

        result.Count.ShouldBe(0);
    }

    [Fact]
    public void Chunk_WithTextShorterThanChunkSize_ReturnsSingleChunk()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 1024);
        var text = "Hello world this is a test";

        var result = chunker.Chunk(text);

        result.Count.ShouldBe(1);
        result[0].Text.ShouldBe(text);
        result[0].StartIndex.ShouldBe(0);
    }

    [Fact]
    public void Chunk_WithTextLongerThanChunkSize_ReturnsMultipleChunks()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 50);
        var text = "Hello world this is a test document with many words to ensure multiple chunks are created";

        var result = chunker.Chunk(text);

        result.Count.ShouldBeGreaterThan(1);
        var reconstructed = string.Concat(result.Select(c => c.Text));
        reconstructed.ShouldBe(text);
    }

    [Fact]
    public void Chunk_ReturnsChunksWithValidProperties()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 512);
        var text = "This is a test document for neural chunking";

        var result = chunker.Chunk(text);

        result.Count.ShouldBeGreaterThan(0);
        foreach (var chunk in result)
        {
            chunk.Text.ShouldNotBeNullOrEmpty();
            chunk.StartIndex.ShouldBeGreaterThanOrEqualTo(0);
            chunk.EndIndex.ShouldBeLessThanOrEqualTo(text.Length);
            chunk.StartIndex.ShouldBeLessThan(chunk.EndIndex);

            // Verify text matches the document slice
            var sliced = text.Substring(chunk.StartIndex, chunk.EndIndex - chunk.StartIndex);
            sliced.ShouldBe(chunk.Text);
        }
    }

    [Fact]
    public void Chunk_WithMultipleSentences_CreatesProperChunks()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 100);
        var text = "First sentence here. Second sentence there. Third sentence elsewhere. Fourth sentence here. Fifth sentence final.";

        var result = chunker.Chunk(text);

        result.Count.ShouldBeGreaterThan(0);
        var reconstructed = string.Concat(result.Select(c => c.Text));
        reconstructed.ShouldBe(text);
    }

    #endregion

    #region ChunkBatch Tests

    [Fact]
    public void ChunkBatch_WithMultipleTexts_ReturnsChunksForEach()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 512);
        var texts = new[]
        {
            "First text here with some content",
            "Second text here with more content",
            "Third text here with additional content"
        };

        var result = chunker.ChunkBatch(texts);

        result.Count.ShouldBe(3);
        foreach (var batch in result)
        {
            batch.Count.ShouldBeGreaterThan(0);
        }
    }

    [Fact]
    public void ChunkBatch_WithEmptyTexts_ReturnsEmptyChunks()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 512);
        var texts = new[] { "", "", "" };

        var result = chunker.ChunkBatch(texts);

        result.Count.ShouldBe(3);
        foreach (var batch in result)
        {
            batch.Count.ShouldBe(0);
        }
    }

    #endregion

    #region ChunkDocument Tests

    [Fact]
    public void ChunkDocument_WithValidDocument_PopulatesChunksCollection()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 512);
        var document = new Document
        {
            Content = "This is a document with substantial content that will be chunked into multiple pieces for testing purposes"
        };

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
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 512);
        var document = new Document { Content = "" };

        var result = chunker.ChunkDocument(document);

        result.Chunks.Count.ShouldBe(0);
    }

    [Fact]
    public void ChunkDocument_ModifiesOriginalDocument()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 512);
        var document = new Document
        {
            Content = "Document with content for testing chunking functionality"
        };
        var originalDocument = document;

        chunker.ChunkDocument(document);

        ReferenceEquals(document, originalDocument).ShouldBeTrue();
        document.Chunks.Count.ShouldBeGreaterThan(0);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 1024);

        var result = chunker.ToString();

        result.ShouldContain("NeuralChunker");
        result.ShouldContain("1024");
        result.ShouldContain("fallback");
    }

    [Fact]
    public void ToString_WithDifferentChunkSizes_IncludesChunkSize()
    {
        var tokenizer = new CharacterTokenizer();

        var chunker2048 = new NeuralChunker(tokenizer, chunkSize: 2048);
        chunker2048.ToString().ShouldContain("2048");

        var chunker512 = new NeuralChunker(tokenizer, chunkSize: 512);
        chunker512.ToString().ShouldContain("512");
    }

    #endregion

    #region UTF-8 and Unicode Tests

    [Fact]
    public void Chunk_WithEmojis_PreservesEmojisCorrectly()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 100);
        var text = "Hello 👋 World 🌍 with emojis 🎉";

        var result = chunker.Chunk(text);

        var reconstructed = string.Concat(result.Select(c => c.Text));
        reconstructed.ShouldBe(text);
    }

    [Fact]
    public void Chunk_WithChineseCharacters_PreservesChineseCorrectly()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 200);
        var text = "这是一个测试文本，用于验证中文字符处理。";

        var result = chunker.Chunk(text);

        var reconstructed = string.Concat(result.Select(c => c.Text));
        reconstructed.ShouldBe(text);
    }

    [Fact]
    public void Chunk_WithMixedLanguages_PreservesMixedCorrectly()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 100);
        var text = "Hello 😊 世界 🌏 مرحبا Привет";

        var result = chunker.Chunk(text);

        var reconstructed = string.Concat(result.Select(c => c.Text));
        reconstructed.ShouldBe(text);
    }

    #endregion

    #region Fallback Behavior Tests

    [Fact]
    public void Chunk_UsesRecursiveChunkerFallback_ProducesValidChunks()
    {
        var tokenizer = new CharacterTokenizer();
        var chunker = new NeuralChunker(tokenizer, chunkSize: 50);
        var text = "The quick brown fox jumps over the lazy dog. The quick brown fox jumps over the lazy dog again.";

        var result = chunker.Chunk(text);

        // Verify that chunks are created and valid
        result.Count.ShouldBeGreaterThan(0);

        // Verify reconstruction
        var reconstructed = string.Concat(result.Select(c => c.Text));
        reconstructed.ShouldBe(text);

        // Verify each chunk has valid boundaries
        foreach (var chunk in result)
        {
            chunk.Text.Length.ShouldBeGreaterThan(0);
            chunk.EndIndex.ShouldBeLessThanOrEqualTo(text.Length);
            chunk.StartIndex.ShouldBeLessThan(chunk.EndIndex);
        }
    }

    #endregion
}
