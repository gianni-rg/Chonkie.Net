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

using System.Text.RegularExpressions;
using Chonkie.Core.Chunker;
using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;
using Microsoft.Extensions.Logging;

/// <summary>
/// Code-aware chunker that heuristically respects common code structure boundaries
/// (functions, classes, braces) while enforcing a token-size budget.
/// This first-pass implementation avoids external parsers and works across many languages.
/// </summary>
public class CodeChunker : BaseChunker
{
    private static readonly Regex BoundaryRegex = new(
        pattern: @"^\s*(class\b|def\b|function\b|module\b|namespace\b|struct\b|interface\b|public\b|private\b|protected\b|async\b|export\b|import\b)",
        options: RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Maximum number of tokens per chunk.
    /// </summary>
    public int ChunkSize { get; }

    /// <summary>
    /// Minimum number of characters per chunk to avoid over-fragmentation.
    /// </summary>
    public int MinCharactersPerChunk { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeChunker"/> class.
    /// </summary>
    /// <param name="tokenizer">Tokenizer for counting tokens.</param>
    /// <param name="chunkSize">Token budget per chunk (default 2048).</param>
    /// <param name="minCharactersPerChunk">Minimum characters per chunk (default 24).</param>
    /// <param name="logger">Optional logger.</param>
    public CodeChunker(
        ITokenizer tokenizer,
        int chunkSize = 2048,
        int minCharactersPerChunk = 24,
        ILogger<CodeChunker>? logger = null)
        : base(tokenizer, logger)
    {
        if (chunkSize <= 0) throw new ArgumentException("chunk_size must be greater than 0", nameof(chunkSize));
        if (minCharactersPerChunk <= 0) throw new ArgumentException("min_characters_per_chunk must be greater than 0", nameof(minCharactersPerChunk));

        ChunkSize = chunkSize;
        MinCharactersPerChunk = minCharactersPerChunk;
    }

    /// <summary>
    /// Heuristically split code into chunks at structure boundaries under a token budget.
    /// </summary>
    public override IReadOnlyList<Chunk> Chunk(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Logger.LogDebug("Empty or whitespace text provided");
            return Array.Empty<Chunk>();
        }

        // Fast path: if it fits, return single chunk
        var totalTokens = Tokenizer.CountTokens(text);
        if (totalTokens <= ChunkSize)
        {
            return new[]
            {
                new Chunk
                {
                    Text = text,
                    StartIndex = 0,
                    EndIndex = text.Length,
                    TokenCount = totalTokens
                }
            };
        }

        // Split into lines and identify soft boundaries
        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var blocks = GroupIntoBlocks(lines);

        // Merge blocks under token budget
        var (mergedTexts, mergedTokenCounts) = MergeBlocks(blocks);

        // Materialize chunks with running indices
        var chunks = new List<Chunk>(mergedTexts.Count);
        var currentIndex = 0;
        foreach (var (blockText, tokenCount) in mergedTexts.Zip(mergedTokenCounts))
        {
            chunks.Add(new Chunk
            {
                Text = blockText,
                StartIndex = currentIndex,
                EndIndex = currentIndex + blockText.Length,
                TokenCount = tokenCount
            });
            currentIndex += blockText.Length;
        }

        return chunks;
    }

    private List<string> GroupIntoBlocks(string[] lines)
    {
        var blocks = new List<string>();
        var current = new List<string>();

        bool IsBoundary(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            if (BoundaryRegex.IsMatch(line)) return true;
            var trimmed = line.TrimEnd();
            if (trimmed.EndsWith("{") || trimmed == "}" || trimmed.EndsWith(":")) return true;
            return false;
        }

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Start a new block at boundaries if current has content
            if (current.Count > 0 && IsBoundary(line))
            {
                var block = string.Join("\n", current) + "\n"; // preserve newline after block
                blocks.Add(block);
                current.Clear();
            }

            current.Add(line);
        }

        if (current.Count > 0)
        {
            blocks.Add(string.Join("\n", current));
        }

        // Ensure minimum block size by merging tiny blocks
        var normalized = new List<string>();
        string carry = string.Empty;
        foreach (var b in blocks)
        {
            var candidate = string.IsNullOrEmpty(carry) ? b : carry + b;
            if (candidate.Length < MinCharactersPerChunk)
            {
                carry = candidate;
            }
            else
            {
                if (!string.IsNullOrEmpty(carry))
                {
                    normalized.Add(carry);
                    carry = string.Empty;
                }
                normalized.Add(b);
            }
        }
        if (!string.IsNullOrEmpty(carry)) normalized.Add(carry);

        return normalized;
    }

    private (List<string> MergedTexts, List<int> TokenCounts) MergeBlocks(List<string> blocks)
    {
        if (blocks.Count == 0) return (new List<string>(), new List<int>());

        var merged = new List<string>();
        var tokenCounts = new List<int>();

        var current = string.Empty;
        var currentTokens = 0;

        foreach (var block in blocks)
        {
            var blockTokens = Tokenizer.CountTokens(block);

            if (current.Length == 0)
            {
                current = block;
                currentTokens = blockTokens;
                continue;
            }

            // If adding this block exceeds budget, finalize current
            if (currentTokens + blockTokens > ChunkSize)
            {
                merged.Add(current);
                tokenCounts.Add(currentTokens);
                current = block;
                currentTokens = blockTokens;
            }
            else
            {
                current += block;
                currentTokens += blockTokens;
            }
        }

        if (current.Length > 0)
        {
            merged.Add(current);
            tokenCounts.Add(currentTokens);
        }

        // Split any oversized merged block by tokens if required (rare)
        var finalTexts = new List<string>();
        var finalCounts = new List<int>();
        for (int i = 0; i < merged.Count; i++)
        {
            var text = merged[i];
            var tokens = tokenCounts[i];
            if (tokens <= ChunkSize)
            {
                finalTexts.Add(text);
                finalCounts.Add(tokens);
                continue;
            }

            // Hard split by tokens
            var ids = Tokenizer.Encode(text);
            for (int start = 0; start < ids.Count; start += ChunkSize)
            {
                var end = Math.Min(start + ChunkSize, ids.Count);
                var sub = ids.Skip(start).Take(end - start).ToList();
                var subText = Tokenizer.DecodeBatch(new[] { sub })[0];
                finalTexts.Add(subText);
                finalCounts.Add(sub.Count);
            }
        }

        return (finalTexts, finalCounts);
    }

    /// <summary>
    /// Returns a string representation of the CodeChunker.
    /// </summary>
    /// <returns>A string describing the chunker configuration.</returns>
    public override string ToString() => $"CodeChunker(chunk_size={ChunkSize}, min_chars={MinCharactersPerChunk})";
}
