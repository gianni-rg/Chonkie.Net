// Copyright 2025-2026 Gianni Rosa Gallina and Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Chonkie.Chunkers;

using Chonkie.Core.Chunker;
using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;
using Microsoft.Extensions.Logging;

/// <summary>
/// Chunker that splits text based on sentence boundaries while respecting token limits.
/// </summary>
public class SentenceChunker : BaseChunker
{
    private const string SeparatorToken = "✄";

    /// <summary>
    /// Gets the maximum number of tokens per chunk.
    /// </summary>
    public int ChunkSize { get; }

    /// <summary>
    /// Gets the number of tokens to overlap between chunks.
    /// </summary>
    public int ChunkOverlap { get; }

    /// <summary>
    /// Gets the minimum number of sentences per chunk.
    /// </summary>
    public int MinSentencesPerChunk { get; }

    /// <summary>
    /// Gets the minimum number of characters per sentence.
    /// </summary>
    public int MinCharactersPerSentence { get; }

    /// <summary>
    /// Gets the delimiters used to split sentences.
    /// </summary>
    public IReadOnlyList<string> Delimiters { get; }

    /// <summary>
    /// Gets how delimiters are included in chunks: "prev" (previous chunk), "next" (next chunk), or null (not included).
    /// </summary>
    public string? IncludeDelimiter { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SentenceChunker"/> class.
    /// </summary>
    /// <param name="tokenizer">The tokenizer to use for encoding and decoding.</param>
    /// <param name="chunkSize">Maximum number of tokens per chunk.</param>
    /// <param name="chunkOverlap">Number of tokens to overlap between chunks.</param>
    /// <param name="minSentencesPerChunk">Minimum number of sentences per chunk.</param>
    /// <param name="minCharactersPerSentence">Minimum number of characters per sentence.</param>
    /// <param name="delimiters">Delimiters to split sentences on.</param>
    /// <param name="includeDelimiter">Whether to include delimiters in previous chunk, next chunk or not at all.</param>
    /// <param name="logger">Optional logger instance.</param>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
    public SentenceChunker(
        ITokenizer tokenizer,
        int chunkSize = 2048,
        int chunkOverlap = 0,
        int minSentencesPerChunk = 1,
        int minCharactersPerSentence = 12,
        IReadOnlyList<string>? delimiters = null,
        string? includeDelimiter = "prev",
        ILogger? logger = null)
        : base(tokenizer, logger)
    {
        if (chunkSize <= 0)
            throw new ArgumentException("chunk_size must be positive", nameof(chunkSize));

        if (chunkOverlap >= chunkSize)
            throw new ArgumentException("chunk_overlap must be less than chunk_size", nameof(chunkOverlap));

        if (minSentencesPerChunk < 1)
            throw new ArgumentException("min_sentences_per_chunk must be at least 1", nameof(minSentencesPerChunk));

        if (minCharactersPerSentence < 1)
            throw new ArgumentException("min_characters_per_sentence must be at least 1", nameof(minCharactersPerSentence));

        if (includeDelimiter != null && includeDelimiter != "prev" && includeDelimiter != "next")
            throw new ArgumentException("include_delimiter must be 'prev', 'next' or null", nameof(includeDelimiter));

        ChunkSize = chunkSize;
        ChunkOverlap = chunkOverlap;
        MinSentencesPerChunk = minSentencesPerChunk;
        MinCharactersPerSentence = minCharactersPerSentence;
        Delimiters = delimiters ?? new[] { ". ", "! ", "? ", "\n" };
        IncludeDelimiter = includeDelimiter;

        Logger.LogDebug("SentenceChunker initialized with chunk_size={ChunkSize}, chunk_overlap={ChunkOverlap}, min_sentences={MinSentences}",
            chunkSize, chunkOverlap, minSentencesPerChunk);
    }

    /// <summary>
    /// Split text into sentences using configured delimiters.
    /// </summary>
    private List<string> SplitText(string text)
    {
        var t = text;

        // Replace delimiters with separator tokens
        foreach (var delimiter in Delimiters)
        {
            if (IncludeDelimiter == "prev")
            {
                t = t.Replace(delimiter, delimiter + SeparatorToken);
            }
            else if (IncludeDelimiter == "next")
            {
                t = t.Replace(delimiter, SeparatorToken + delimiter);
            }
            else
            {
                t = t.Replace(delimiter, SeparatorToken);
            }
        }

        // Initial split
        var splits = t.Split(SeparatorToken, StringSplitOptions.RemoveEmptyEntries);

        // Combine short splits with previous sentence
        var current = "";
        var sentences = new List<string>();

        foreach (var split in splits)
        {
            // If the split is short, add to current; if long, add to sentences
            if (split.Length < MinCharactersPerSentence)
            {
                current += split;
            }
            else if (!string.IsNullOrEmpty(current))
            {
                current += split;
                sentences.Add(current);
                current = "";
            }
            else
            {
                sentences.Add(split);
            }

            // If current sentence is longer than min_characters_per_sentence, add it
            if (current.Length >= MinCharactersPerSentence)
            {
                sentences.Add(current);
                current = "";
            }
        }

        // Add any remaining current split
        if (!string.IsNullOrEmpty(current))
        {
            sentences.Add(current);
        }

        return sentences;
    }

    /// <summary>
    /// Prepare sentences with token counts.
    /// </summary>
    private List<Sentence> PrepareSentences(string text)
    {
        // Split text into sentences
        var sentenceTexts = SplitText(text);
        if (sentenceTexts.Count == 0)
        {
            return new List<Sentence>();
        }

        // Calculate positions
        var positions = new List<int>();
        var currentPos = 0;
        foreach (var sent in sentenceTexts)
        {
            positions.Add(currentPos);
            currentPos += sent.Length;
        }

        // Get token counts in batch for efficiency
        var tokenCounts = Tokenizer.CountTokensBatch(sentenceTexts);

        // Create sentence objects
        var sentences = new List<Sentence>();
        for (int i = 0; i < sentenceTexts.Count; i++)
        {
            sentences.Add(new Sentence
            {
                Text = sentenceTexts[i],
                StartIndex = positions[i],
                EndIndex = positions[i] + sentenceTexts[i].Length,
                TokenCount = tokenCounts[i]
            });
        }

        return sentences;
    }

    /// <summary>
    /// Create a chunk from a list of sentences.
    /// </summary>
    private Chunk CreateChunk(List<Sentence> sentences)
    {
        var chunkText = string.Concat(sentences.Select(s => s.Text));

        // Calculate actual token count for the combined text
        var tokenCount = Tokenizer.CountTokens(chunkText);

        return new Chunk
        {
            Text = chunkText,
            StartIndex = sentences[0].StartIndex,
            EndIndex = sentences[^1].EndIndex,
            TokenCount = tokenCount
        };
    }

    /// <summary>
    /// Split text into overlapping chunks based on sentences while respecting token limits.
    /// </summary>
    /// <param name="text">Input text to be chunked.</param>
    /// <returns>List of Chunk objects containing the chunked text and metadata.</returns>
    public override IReadOnlyList<Chunk> Chunk(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Logger.LogDebug("Empty or whitespace text provided");
            return Array.Empty<Chunk>();
        }

        Logger.LogDebug("Chunking text of length {Length}", text.Length);

        // Get prepared sentences with token counts
        var sentences = PrepareSentences(text);
        if (sentences.Count == 0)
        {
            Logger.LogDebug("No sentences extracted from text");
            return Array.Empty<Chunk>();
        }

        Logger.LogDebug("Prepared {Count} sentences for chunking", sentences.Count);

        var chunks = new List<Chunk>();
        var pos = 0;

        while (pos < sentences.Count)
        {
            // Find split point based on token limits
            var splitIdx = FindSplitIndex(sentences, pos);

            // Ensure minimum sentences requirement
            splitIdx = Math.Max(splitIdx, pos + MinSentencesPerChunk);

            // Don't exceed total sentences
            if (splitIdx > sentences.Count)
            {
                if (pos + MinSentencesPerChunk <= sentences.Count)
                {
                    splitIdx = pos + MinSentencesPerChunk;
                }
                else
                {
                    Logger.LogWarning(
                        "Minimum sentences per chunk as {MinSentences} could not be met for all chunks. " +
                        "Last chunk will have only {ActualSentences} sentences",
                        MinSentencesPerChunk, sentences.Count - pos);
                    splitIdx = sentences.Count;
                }
            }

            // Create chunk from selected sentences
            var chunkSentences = sentences.GetRange(pos, splitIdx - pos);
            chunks.Add(CreateChunk(chunkSentences));

            // Calculate next position with overlap
            if (ChunkOverlap > 0 && splitIdx < sentences.Count)
            {
                var overlapTokens = 0;
                var overlapIdx = splitIdx - 1;

                while (overlapIdx > pos && overlapTokens < ChunkOverlap)
                {
                    var sent = sentences[overlapIdx];
                    var nextTokens = overlapTokens + sent.TokenCount;
                    if (nextTokens > ChunkOverlap)
                        break;

                    overlapTokens = nextTokens;
                    overlapIdx--;
                }

                pos = overlapIdx + 1;
            }
            else
            {
                pos = splitIdx;
            }
        }

        Logger.LogInformation("Created {ChunkCount} chunks from {SentenceCount} sentences",
            chunks.Count, sentences.Count);

        return chunks;
    }

    /// <summary>
    /// Find the split index for creating a chunk starting at the given position.
    /// </summary>
    private int FindSplitIndex(List<Sentence> sentences, int pos)
    {
        var currentTokens = 0;
        var splitIdx = pos;

        for (int i = pos; i < sentences.Count; i++)
        {
            var nextTokens = currentTokens + sentences[i].TokenCount;
            if (nextTokens > ChunkSize && i > pos)
            {
                break;
            }

            currentTokens = nextTokens;
            splitIdx = i + 1;

            if (currentTokens >= ChunkSize)
            {
                break;
            }
        }

        return splitIdx;
    }

    /// <summary>
    /// Returns a string representation of this SentenceChunker.
    /// </summary>
    public override string ToString()
    {
        return $"SentenceChunker(chunk_size={ChunkSize}, chunk_overlap={ChunkOverlap}, " +
               $"min_sentences={MinSentencesPerChunk}, min_chars={MinCharactersPerSentence})";
    }
}
