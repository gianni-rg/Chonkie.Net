namespace Chonkie.Chunkers;

using Chonkie.Core.Chunker;
using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;
using Microsoft.Extensions.Logging;

/// <summary>
/// Chunker that recursively splits text into smaller chunks based on a hierarchy of rules.
/// </summary>
public class RecursiveChunker : BaseChunker
{
    private const string SeparatorToken = "✄";

    /// <summary>
    /// Gets the maximum size of each chunk in tokens.
    /// </summary>
    public int ChunkSize { get; }

    /// <summary>
    /// Gets the minimum number of characters per chunk.
    /// </summary>
    public int MinCharactersPerChunk { get; }

    /// <summary>
    /// Gets the recursive rules defining the chunking hierarchy.
    /// </summary>
    public RecursiveRules Rules { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecursiveChunker"/> class.
    /// </summary>
    /// <param name="tokenizer">The tokenizer to use.</param>
    /// <param name="chunkSize">Maximum size of each chunk in tokens.</param>
    /// <param name="rules">The recursive rules defining chunking hierarchy.</param>
    /// <param name="minCharactersPerChunk">Minimum number of characters per chunk.</param>
    /// <param name="logger">Optional logger instance.</param>
    public RecursiveChunker(
        ITokenizer tokenizer,
        int chunkSize = 2048,
        RecursiveRules? rules = null,
        int minCharactersPerChunk = 24,
        ILogger? logger = null)
        : base(tokenizer, logger)
    {
        if (chunkSize <= 0)
            throw new ArgumentException("chunk_size must be greater than 0", nameof(chunkSize));

        if (minCharactersPerChunk <= 0)
            throw new ArgumentException("min_characters_per_chunk must be greater than 0", nameof(minCharactersPerChunk));

        ChunkSize = chunkSize;
        MinCharactersPerChunk = minCharactersPerChunk;
        Rules = rules ?? new RecursiveRules();

        Logger.LogDebug("RecursiveChunker initialized with chunk_size={ChunkSize}, levels={LevelCount}",
            chunkSize, Rules.Count);
    }

    /// <summary>
    /// Estimates or calculates the token count for the given text.
    /// </summary>
    private int EstimateTokenCount(string text)
    {
        // For accuracy, always use actual token counting
        return Tokenizer.CountTokens(text);
    }

    /// <summary>
    /// Splits text using the delimiters and rules from the specified recursive level.
    /// </summary>
    private List<string> SplitText(string text, RecursiveLevel level)
    {
        if (level.Whitespace)
        {
            return text.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        else if (level.Delimiters != null && level.Delimiters.Count > 0)
        {
            var t = text;

            // Replace delimiters with separator tokens
            foreach (var delimiter in level.Delimiters)
            {
                if (level.IncludeDelimiter == "prev")
                {
                    t = t.Replace(delimiter, delimiter + SeparatorToken);
                }
                else if (level.IncludeDelimiter == "next")
                {
                    t = t.Replace(delimiter, SeparatorToken + delimiter);
                }
                else
                {
                    t = t.Replace(delimiter, SeparatorToken);
                }
            }

            var splits = t.Split(SeparatorToken, StringSplitOptions.RemoveEmptyEntries).ToList();

            // Merge short splits
            var current = "";
            var merged = new List<string>();

            foreach (var split in splits)
            {
                if (split.Length < MinCharactersPerChunk)
                {
                    current += split;
                }
                else if (!string.IsNullOrEmpty(current))
                {
                    current += split;
                    merged.Add(current);
                    current = "";
                }
                else
                {
                    merged.Add(split);
                }

                if (current.Length >= MinCharactersPerChunk)
                {
                    merged.Add(current);
                    current = "";
                }
            }

            if (!string.IsNullOrEmpty(current))
            {
                merged.Add(current);
            }

            return merged;
        }
        else
        {
            // Fallback: Encode, split, and decode by tokens
            var encoded = Tokenizer.Encode(text);
            var tokenSplits = new List<List<int>>();

            for (int i = 0; i < encoded.Count; i += ChunkSize)
            {
                var end = Math.Min(i + ChunkSize, encoded.Count);
                tokenSplits.Add(encoded.Skip(i).Take(end - i).ToList());
            }

            return Tokenizer.DecodeBatch(tokenSplits).ToList();
        }
    }

    /// <summary>
    /// Creates a chunk object with indices based on the current offset.
    /// </summary>
    private Chunk MakeChunk(string text, int tokenCount, int startOffset)
    {
        return new Chunk
        {
            Text = text,
            StartIndex = startOffset,
            EndIndex = startOffset + text.Length,
            TokenCount = tokenCount
        };
    }

    /// <summary>
    /// Merges short splits into larger chunks respecting token limits.
    /// </summary>
    private (List<string>, List<int>) MergeSplits(
        List<string> splits,
        List<int> tokenCounts,
        bool combineWhitespace)
    {
        if (splits.Count == 0 || tokenCounts.Count == 0)
        {
            return (new List<string>(), new List<int>());
        }

        if (splits.Count != tokenCounts.Count)
        {
            throw new ArgumentException(
                $"Number of splits {splits.Count} does not match number of token counts {tokenCounts.Count}");
        }

        // If all splits are larger than chunk size, return them as-is
        if (tokenCounts.All(c => c > ChunkSize))
        {
            return (splits, tokenCounts);
        }

        // Calculate cumulative token counts
        var cumulativeCounts = new List<int> { 0 };
        foreach (var count in tokenCounts)
        {
            var last = cumulativeCounts[^1];
            cumulativeCounts.Add(last + count + (combineWhitespace ? 1 : 0));
        }

        var merged = new List<string>();
        var combinedTokenCounts = new List<int>();
        var currentIndex = 0;

        while (currentIndex < splits.Count)
        {
            var currentTokenCount = cumulativeCounts[currentIndex];
            var requiredTokenCount = currentTokenCount + ChunkSize;

            // Find the index to merge at using binary search
            var index = BinarySearchCeiling(cumulativeCounts, requiredTokenCount, currentIndex);
            index = Math.Min(index - 1, splits.Count);

            // Ensure we include at least one split
            if (index == currentIndex)
            {
                index++;
            }

            // Merge splits
            var mergedText = combineWhitespace
                ? string.Join(" ", splits.Skip(currentIndex).Take(index - currentIndex))
                : string.Concat(splits.Skip(currentIndex).Take(index - currentIndex));

            merged.Add(mergedText);

            // Adjust token count
            var endIndex = Math.Min(index, splits.Count);
            combinedTokenCounts.Add(cumulativeCounts[endIndex] - currentTokenCount);

            currentIndex = index;
        }

        return (merged, combinedTokenCounts);
    }

    /// <summary>
    /// Binary search to find the ceiling index.
    /// </summary>
    private int BinarySearchCeiling(List<int> list, int target, int startIdx)
    {
        var left = startIdx;
        var right = list.Count;

        while (left < right)
        {
            var mid = left + (right - left) / 2;
            if (list[mid] < target)
            {
                left = mid + 1;
            }
            else
            {
                right = mid;
            }
        }

        return left;
    }

    /// <summary>
    /// Recursive helper for core chunking logic.
    /// </summary>
    private List<Chunk> RecursiveChunk(string text, int level, int startOffset)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new List<Chunk>();
        }

        if (level >= Rules.Count)
        {
            return new List<Chunk> { MakeChunk(text, EstimateTokenCount(text), startOffset) };
        }

        var currentRule = Rules[level];
        var splits = SplitText(text, currentRule);
        var tokenCounts = splits.Select(EstimateTokenCount).ToList();

        List<string> merged;
        List<int> combinedTokenCounts;

        if (currentRule.Delimiters == null && !currentRule.Whitespace)
        {
            merged = splits;
            combinedTokenCounts = tokenCounts;
        }
        else if (currentRule.Delimiters == null && currentRule.Whitespace)
        {
            (merged, combinedTokenCounts) = MergeSplits(splits, tokenCounts, combineWhitespace: true);

            // Fix reconstruction: add space prefix to all splits except first
            if (merged.Count > 1)
            {
                for (int i = 1; i < merged.Count; i++)
                {
                    merged[i] = " " + merged[i];
                }
            }
        }
        else
        {
            (merged, combinedTokenCounts) = MergeSplits(splits, tokenCounts, combineWhitespace: false);
        }

        // Chunk long merged splits recursively
        var chunks = new List<Chunk>();
        var currentOffset = startOffset;

        for (int i = 0; i < merged.Count; i++)
        {
            var split = merged[i];
            var tokenCount = combinedTokenCounts[i];

            if (tokenCount > ChunkSize)
            {
                var recursiveResult = RecursiveChunk(split, level + 1, currentOffset);
                chunks.AddRange(recursiveResult);
            }
            else
            {
                chunks.Add(MakeChunk(split, tokenCount, currentOffset));
            }

            currentOffset += split.Length;
        }

        return chunks;
    }

    /// <summary>
    /// Recursively chunks text based on the configured rules hierarchy.
    /// </summary>
    /// <param name="text">Text to chunk.</param>
    /// <returns>List of chunks.</returns>
    public override IReadOnlyList<Chunk> Chunk(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Logger.LogDebug("Empty or whitespace text provided");
            return Array.Empty<Chunk>();
        }

        Logger.LogDebug("Starting recursive chunking for text of length {Length}", text.Length);

        var chunks = RecursiveChunk(text, level: 0, startOffset: 0);

        Logger.LogInformation("Created {ChunkCount} chunks using recursive chunking", chunks.Count);

        return chunks;
    }

    /// <summary>
    /// Returns a string representation of this RecursiveChunker.
    /// </summary>
    public override string ToString()
    {
        return $"RecursiveChunker(chunk_size={ChunkSize}, levels={Rules.Count}, min_chars={MinCharactersPerChunk})";
    }
}
