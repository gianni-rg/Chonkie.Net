namespace Chonkie.Chunkers;

using System.Text.RegularExpressions;
using Chonkie.Core.Chunker;
using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;
using Microsoft.Extensions.Logging;

/// <summary>
/// Table-aware chunker that keeps Markdown table blocks intact when possible and
/// falls back to recursive chunking for non-table text.
/// </summary>
public class TableChunker : BaseChunker
{
    private static readonly Regex TableSeparatorRegex = new(
        pattern: @"^\s*\|?(\s*:?-+:?\s*\|)+\s*:?-+:?\s*\|?\s*$",
        options: RegexOptions.Compiled);

    /// <summary>
    /// Maximum number of tokens per chunk.
    /// </summary>
    public int ChunkSize { get; }

    /// <summary>
    /// Whether to repeat table headers in every chunk (Python-style behavior).
    /// When true, each chunk containing table rows will include the header and separator.
    /// When false (default), only the first chunk of a table includes the header.
    /// </summary>
    public bool RepeatHeaders { get; }

    private readonly RecursiveChunker _fallback;

    /// <summary>
    /// Initializes a new instance of the <see cref="TableChunker"/> class.
    /// </summary>
    /// <param name="tokenizer">Tokenizer for counting tokens.</param>
    /// <param name="chunkSize">Token budget per chunk (default 2048).</param>
    /// <param name="repeatHeaders">Whether to repeat headers in every table chunk (default false).</param>
    /// <param name="logger">Optional logger.</param>
    public TableChunker(
        ITokenizer tokenizer,
        int chunkSize = 2048,
        bool repeatHeaders = false,
        ILogger<TableChunker>? logger = null)
        : base(tokenizer, logger)
    {
        if (chunkSize <= 0) throw new ArgumentException("chunk_size must be greater than 0", nameof(chunkSize));
        ChunkSize = chunkSize;
        RepeatHeaders = repeatHeaders;
        _fallback = new RecursiveChunker(tokenizer, chunkSize);
    }

    /// <summary>
    /// Chunks the input text, preserving markdown table structures.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    /// <returns>A list of chunks.</returns>
    public override IReadOnlyList<Chunk> Chunk(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Logger.LogDebug("Empty or whitespace text provided");
            return Array.Empty<Chunk>();
        }

        // Fast path
        var totalTokens = Tokenizer.CountTokens(text);
        if (totalTokens <= ChunkSize)
        {
            return new[]
            {
                new Chunk { Text = text, StartIndex = 0, EndIndex = text.Length, TokenCount = totalTokens }
            };
        }

        var segments = ExtractSegments(text);

        var chunks = new List<Chunk>();
        var currentIndex = 0;

        foreach (var seg in segments)
        {
            if (seg.IsTable)
            {
                var rows = seg.Text.Replace("\r\n", "\n").Split('\n');

                // Validate table structure
                if (rows.Length < 3)
                {
                    Logger.LogWarning("Table must have at least a header, separator, and one data row. Treating as normal text.");
                    // Delegate to fallback for invalid table
                    var subChunks = _fallback.Chunk(seg.Text);
                    foreach (var c in subChunks)
                    {
                        chunks.Add(new Chunk
                        {
                            Text = c.Text,
                            StartIndex = currentIndex,
                            EndIndex = currentIndex + c.Text.Length,
                            TokenCount = c.TokenCount
                        });
                        currentIndex += c.Text.Length;
                    }
                    continue;
                }

                var headerCount = Math.Min(2, rows.Length); // header + separator
                var header = string.Join("\n", rows.Take(headerCount));
                var headerTokens = Tokenizer.CountTokens(header);

                if (RepeatHeaders)
                {
                    // Python-style: repeat header in every chunk
                    ChunkTableWithHeaderRepetition(rows, header, headerTokens, chunks, ref currentIndex);
                }
                else
                {
                    // .NET-style: header only in first chunk
                    ChunkTableWithoutHeaderRepetition(rows, headerCount, chunks, ref currentIndex);
                }
            }
            else
            {
                // Delegate to fallback for non-table
                var subChunks = _fallback.Chunk(seg.Text);
                foreach (var c in subChunks)
                {
                    chunks.Add(new Chunk
                    {
                        Text = c.Text,
                        StartIndex = currentIndex,
                        EndIndex = currentIndex + c.Text.Length,
                        TokenCount = c.TokenCount
                    });
                    currentIndex += c.Text.Length;
                }
            }
        }

        return chunks;
    }

    private void ChunkTableWithHeaderRepetition(
        string[] rows,
        string header,
        int headerTokens,
        List<Chunk> chunks,
        ref int currentIndex)
    {
        var headerCount = Math.Min(2, rows.Length);
        var dataRows = new List<string>();
        var dataRowTokens = new List<int>();

        // Collect all data rows
        for (int i = headerCount; i < rows.Length; i++)
        {
            dataRows.Add(rows[i]);
            var lineWithNl = i < rows.Length - 1 ? rows[i] + "\n" : rows[i];
            dataRowTokens.Add(Tokenizer.CountTokens(lineWithNl));
        }

        if (dataRows.Count == 0)
        {
            Logger.LogWarning("Table has no data rows. Skipping.");
            return;
        }

        var currentRows = new List<string>();
        var currentTokens = headerTokens;

        for (int i = 0; i < dataRows.Count; i++)
        {
            var rowTokens = dataRowTokens[i];

            if (currentTokens + rowTokens > ChunkSize && currentRows.Count > 0)
            {
                // Flush current chunk
                var chunkText = header + "\n" + string.Join("\n", currentRows);
                var actualTokens = Tokenizer.CountTokens(chunkText);

                chunks.Add(new Chunk
                {
                    Text = chunkText,
                    StartIndex = currentIndex,
                    EndIndex = currentIndex + chunkText.Length,
                    TokenCount = actualTokens
                });

                // For header repetition, advance index only by data rows (header is repeated, not counted in original)
                var dataText = string.Join("\n", currentRows);
                currentIndex += dataText.Length;
                if (currentRows.Count > 0) currentIndex += 1; // newline

                currentRows.Clear();
                currentTokens = headerTokens;
            }

            currentRows.Add(dataRows[i]);
            currentTokens += rowTokens;
        }

        // Flush remaining rows
        if (currentRows.Count > 0)
        {
            var chunkText = header + "\n" + string.Join("\n", currentRows);
            var actualTokens = Tokenizer.CountTokens(chunkText);

            chunks.Add(new Chunk
            {
                Text = chunkText,
                StartIndex = currentIndex,
                EndIndex = currentIndex + chunkText.Length,
                TokenCount = actualTokens
            });

            var dataText = string.Join("\n", currentRows);
            currentIndex += dataText.Length;
            if (currentRows.Count > 0 && currentIndex < currentIndex + chunkText.Length)
            {
                currentIndex += 1; // final newline if exists
            }
        }
    }

    private void ChunkTableWithoutHeaderRepetition(
        string[] rows,
        int headerCount,
        List<Chunk> chunks,
        ref int currentIndex)
    {
        var current = string.Empty;
        var currentTokens = 0;
        var rowIndex = 0;

        // Always keep header and separator together
        var header = string.Join("\n", rows.Take(headerCount)) + (rows.Length > headerCount ? "\n" : "");
        current = header;
        currentTokens = Tokenizer.CountTokens(header);
        rowIndex = headerCount;

        for (; rowIndex < rows.Length; rowIndex++)
        {
            var line = rows[rowIndex];
            var lineWithNl = rowIndex < rows.Length - 1 ? line + "\n" : line;
            var lineTokens = Tokenizer.CountTokens(lineWithNl);

            if (currentTokens + lineTokens > ChunkSize && currentTokens > 0)
            {
                // Flush current chunk
                var tokenCount = currentTokens;
                chunks.Add(new Chunk
                {
                    Text = current,
                    StartIndex = currentIndex,
                    EndIndex = currentIndex + current.Length,
                    TokenCount = tokenCount
                });
                currentIndex += current.Length;
                current = string.Empty;
                currentTokens = 0;
            }

            current += lineWithNl;
            currentTokens += lineTokens;
        }

        // Flush remaining content
        if (!string.IsNullOrEmpty(current))
        {
            var tokenCount = currentTokens;
            chunks.Add(new Chunk
            {
                Text = current,
                StartIndex = currentIndex,
                EndIndex = currentIndex + current.Length,
                TokenCount = tokenCount
            });
            currentIndex += current.Length;
        }
    }

    private List<(bool IsTable, string Text)> ExtractSegments(string text)
    {
        var segments = new List<(bool, string)>();
        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var i = 0;

        while (i < lines.Length)
        {
            // Detect start of a markdown table: a row with '|' and next line a separator
            if (IsTableRow(lines[i]) && i + 1 < lines.Length && TableSeparatorRegex.IsMatch(lines[i + 1]))
            {
                var tableLines = new List<string> { lines[i], lines[i + 1] };
                i += 2;
                while (i < lines.Length && IsTableRow(lines[i]))
                {
                    tableLines.Add(lines[i]);
                    i++;
                }
                var tableText = string.Join("\n", tableLines);
                segments.Add((true, tableText));
            }
            else
            {
                // Accumulate non-table text until next table or end
                var textLines = new List<string>();
                while (i < lines.Length && !(IsTableRow(lines[i]) && i + 1 < lines.Length && TableSeparatorRegex.IsMatch(lines[i + 1])))
                {
                    textLines.Add(lines[i]);
                    i++;
                }
                var normalText = string.Join("\n", textLines);
                if (!string.IsNullOrEmpty(normalText))
                    segments.Add((false, normalText));
            }
        }

        return segments;
    }

    private static bool IsTableRow(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return false;
        // A simple heuristic: a table row has at least two pipe separators
        var count = 0;
        foreach (var ch in line)
        {
            if (ch == '|') count++;
            if (count >= 2) return true;
        }
        return false;
    }

    /// <summary>
    /// Returns a string representation of the TableChunker.
    /// </summary>
    /// <returns>A string describing the chunker configuration.</returns>
    public override string ToString() => $"TableChunker(chunk_size={ChunkSize}, repeat_headers={RepeatHeaders})";
}
