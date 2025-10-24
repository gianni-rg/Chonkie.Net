using Chonkie.Core.Types;

namespace Chonkie.Visualizer.Sample;

/// <summary>
/// Utility class for visualizing chunks in the console with colors and formatting.
/// Similar to Python's Visualizer class.
/// </summary>
public static class ChunkVisualizer
{
    private static readonly ConsoleColor[] ChunkColors =
    [
        ConsoleColor.Cyan,
        ConsoleColor.Green,
        ConsoleColor.Yellow,
        ConsoleColor.Magenta,
        ConsoleColor.Blue,
        ConsoleColor.Red
    ];

    /// <summary>
    /// Prints chunks to console with color coding.
    /// </summary>
    /// <param name="chunks">The chunks to visualize.</param>
    /// <param name="showOverlap">Whether to highlight overlapping regions.</param>
    /// <param name="showMetadata">Whether to show detailed metadata.</param>
    /// <param name="showBoundaries">Whether to show chunk boundaries.</param>
    public static void Print(
        IReadOnlyList<Chunk> chunks,
        bool showOverlap = false,
        bool showMetadata = false,
        bool showBoundaries = false)
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var color = ChunkColors[i % ChunkColors.Length];

            // Print chunk header
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[Chunk {i + 1}");

            if (showMetadata)
            {
                Console.Write($" | Tokens: {chunk.TokenCount} | Range: {chunk.StartIndex}-{chunk.EndIndex}");
            }

            Console.WriteLine("]");

            // Print chunk text with color
            Console.ForegroundColor = color;

            if (showBoundaries)
            {
                Console.Write("┌─ ");
            }

            Console.WriteLine(chunk.Text.Trim());

            if (showBoundaries)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("└─");
            }

            // Show overlap with next chunk
            if (showOverlap && i < chunks.Count - 1)
            {
                var nextChunk = chunks[i + 1];
                if (chunk.EndIndex > nextChunk.StartIndex)
                {
                    var overlapSize = chunk.EndIndex - nextChunk.StartIndex;
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"  ↓ Overlap: {overlapSize} characters");
                }
            }

            Console.WriteLine();
        }

        Console.ResetColor();
    }

    /// <summary>
    /// Prints a compact summary of chunks.
    /// </summary>
    public static void PrintSummary(IReadOnlyList<Chunk> chunks)
    {
        Console.WriteLine("┌─ Chunk Summary ─────────────────────────┐");
        Console.WriteLine($"│ Total Chunks: {chunks.Count,-27} │");
        Console.WriteLine($"│ Total Tokens: {chunks.Sum(c => c.TokenCount),-27} │");
        Console.WriteLine($"│ Avg Tokens/Chunk: {chunks.Average(c => c.TokenCount):F1,-23} │");
        Console.WriteLine($"│ Min Tokens: {chunks.Min(c => c.TokenCount),-29} │");
        Console.WriteLine($"│ Max Tokens: {chunks.Max(c => c.TokenCount),-29} │");
        Console.WriteLine("└─────────────────────────────────────────┘\n");
    }

    /// <summary>
    /// Compares multiple chunking strategies side-by-side.
    /// </summary>
    public static void Compare(Dictionary<string, IReadOnlyList<Chunk>> strategies)
    {
        Console.WriteLine("┌─ Chunker Comparison ─────────────────────────────────┐");
        Console.WriteLine("│ Strategy          │ Chunks │ Total Tokens │ Avg/Chunk │");
        Console.WriteLine("├───────────────────┼────────┼──────────────┼───────────┤");

        foreach (var (name, chunks) in strategies)
        {
            var totalTokens = chunks.Sum(c => c.TokenCount);
            var avgTokens = chunks.Average(c => c.TokenCount);

            Console.WriteLine($"│ {name,-17} │ {chunks.Count,6} │ {totalTokens,12} │ {avgTokens,9:F1} │");
        }

        Console.WriteLine("└───────────────────┴────────┴──────────────┴───────────┘");
    }

    /// <summary>
    /// Creates a simple text-based bar chart showing chunk sizes.
    /// </summary>
    public static void PrintBarChart(IReadOnlyList<Chunk> chunks, int maxWidth = 50)
    {
        var maxTokens = chunks.Max(c => c.TokenCount);

        Console.WriteLine("Chunk Size Distribution:");
        Console.WriteLine();

        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var barWidth = (int)((double)chunk.TokenCount / maxTokens * maxWidth);
            var color = ChunkColors[i % ChunkColors.Length];

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"  Chunk {i + 1,2}: ");

            Console.ForegroundColor = color;
            Console.Write(new string('█', barWidth));

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($" {chunk.TokenCount} tokens");
        }

        Console.ResetColor();
        Console.WriteLine();
    }

    /// <summary>
    /// Visualizes chunk text with highlighted boundaries.
    /// </summary>
    public static void PrintHighlighted(IReadOnlyList<Chunk> chunks, string fullText)
    {
        Console.WriteLine("Full Text with Chunk Boundaries:\n");

        int lastPos = 0;
        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var color = ChunkColors[i % ChunkColors.Length];

            // Print any gap before this chunk
            if (chunk.StartIndex > lastPos)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(fullText.Substring(lastPos, chunk.StartIndex - lastPos));
            }

            // Print chunk number marker
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{i + 1}]");

            // Print chunk text
            Console.ForegroundColor = color;
            Console.Write(fullText.Substring(chunk.StartIndex,
                Math.Min(chunk.EndIndex - chunk.StartIndex, fullText.Length - chunk.StartIndex)));

            lastPos = chunk.EndIndex;
        }

        // Print any remaining text
        if (lastPos < fullText.Length)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(fullText.Substring(lastPos));
        }

        Console.ResetColor();
        Console.WriteLine("\n");
    }

    /// <summary>
    /// Saves chunks to an HTML file for browser viewing.
    /// </summary>
    public static void SaveHtml(string filePath, IReadOnlyList<Chunk> chunks, string? title = null)
    {
        var html = GenerateHtml(chunks, title ?? "Chunk Visualization");
        File.WriteAllText(filePath, html);
        Console.WriteLine($"✓ Visualization saved to: {filePath}");
    }

    private static string GenerateHtml(IReadOnlyList<Chunk> chunks, string title)
    {
        var htmlColors = new[]
        {
            "#00CED1", "#32CD32", "#FFD700", "#FF00FF", "#1E90FF", "#FF6347"
        };

        var chunksHtml = string.Join("\n", chunks.Select((chunk, i) =>
        {
            var color = htmlColors[i % htmlColors.Length];
            return $@"
                <div class='chunk' style='border-left: 4px solid {color}'>
                    <div class='chunk-header' style='background-color: {color}20'>
                        <strong>Chunk {i + 1}</strong>
                        <span class='meta'>Tokens: {chunk.TokenCount} | Range: {chunk.StartIndex}-{chunk.EndIndex}</span>
                    </div>
                    <div class='chunk-text'>{System.Net.WebUtility.HtmlEncode(chunk.Text.Trim())}</div>
                </div>";
        }));

        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>{title}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            max-width: 1000px;
            margin: 40px auto;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        h1 {{
            color: #333;
            border-bottom: 3px solid #00CED1;
            padding-bottom: 10px;
        }}
        .chunk {{
            background: white;
            margin: 20px 0;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .chunk-header {{
            padding: 12px 16px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }}
        .chunk-text {{
            padding: 16px;
            line-height: 1.6;
            color: #333;
        }}
        .meta {{
            color: #666;
            font-size: 0.9em;
        }}
        .summary {{
            background: white;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
    </style>
</head>
<body>
    <h1>{title}</h1>
    <div class='summary'>
        <strong>Summary:</strong> {chunks.Count} chunks | 
        Total Tokens: {chunks.Sum(c => c.TokenCount)} | 
        Average: {chunks.Average(c => c.TokenCount):F1} tokens/chunk
    </div>
    {chunksHtml}
</body>
</html>";
    }
}
