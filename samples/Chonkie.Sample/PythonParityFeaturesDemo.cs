using Chonkie.Chunkers;
using Chonkie.Core.Types;
using Chonkie.Pipeline;
using Chonkie.Tokenizers;

namespace Chonkie.Sample;

/// <summary>
/// Demonstrates the new Python parity features:
/// 1. MarkdownDocument type
/// 2. Invalid table logging
/// 3. Header repetition mode
/// </summary>
public class PythonParityFeaturesDemo
{
    public static void Main()
    {
        Console.WriteLine("=== Python Parity Features Demo ===\n");

        // Feature 1: MarkdownDocument
        DemoMarkdownDocument();

        // Feature 2: Invalid Table Logging
        DemoInvalidTableLogging();

        // Feature 3: Header Repetition Mode
        DemoHeaderRepetition();
    }

    static void DemoMarkdownDocument()
    {
        Console.WriteLine("1. MarkdownDocument Type");
        Console.WriteLine("------------------------");

        var markdown = @"# Employee Directory

| ID | Name | Department |
|----|------|------------|
| 1 | Alice | Engineering |
| 2 | Bob | Sales |

```csharp
public class Employee {
    public int Id { get; set; }
}
```

![Company Logo](logo.png)
";

        var doc = new MarkdownDocument
        {
            Content = markdown,
            Source = "employees.md",
            Tables = new List<MarkdownTable>
            {
                new MarkdownTable
                {
                    Content = "| ID | Name | Department |\n|----|------|------------|\n| 1 | Alice | Engineering |\n| 2 | Bob | Sales |",
                    StartIndex = 21,
                    EndIndex = 110
                }
            },
            Code = new List<MarkdownCode>
            {
                new MarkdownCode
                {
                    Content = "public class Employee {\n    public int Id { get; set; }\n}",
                    Language = "csharp",
                    StartIndex = 112,
                    EndIndex = 180
                }
            },
            Images = new List<MarkdownImage>
            {
                new MarkdownImage
                {
                    Alias = "Company Logo",
                    Link = "logo.png",
                    StartIndex = 182,
                    EndIndex = 210
                }
            }
        };

        Console.WriteLine($"Document ID: {doc.Id}");
        Console.WriteLine($"Source: {doc.Source}");
        Console.WriteLine($"Tables: {doc.Tables.Count}");
        Console.WriteLine($"Code blocks: {doc.Code.Count}");
        Console.WriteLine($"Images: {doc.Images.Count}");
        Console.WriteLine();
    }

    static void DemoInvalidTableLogging()
    {
        Console.WriteLine("2. Invalid Table Logging");
        Console.WriteLine("------------------------");

        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 100);

        // Invalid table: only header and separator, no data rows
        var invalidTable = @"| Name | Value |
|------|-------|";

        Console.WriteLine("Processing invalid table (header only, no data rows)...");
        var chunks = chunker.Chunk(invalidTable);
        Console.WriteLine($"Result: Treated as normal text, {chunks.Count} chunk(s) created");
        Console.WriteLine("(Check logs for warning message)");
        Console.WriteLine();
    }

    static void DemoHeaderRepetition()
    {
        Console.WriteLine("3. Header Repetition Mode");
        Console.WriteLine("-------------------------");

        var tokenizer = new CharacterTokenizer();
        var table = @"| Product | Price | Stock |
|---------|-------|-------|
| Apple   | $1.50 | 100   |
| Banana  | $0.75 | 150   |
| Orange  | $1.25 | 80    |
| Grape   | $2.00 | 60    |
| Mango   | $1.80 | 40    |";

        // Without header repetition (.NET default)
        Console.WriteLine("A. Without Header Repetition (default):");
        var chunkerNoRepeat = new TableChunker(tokenizer, chunkSize: 80, repeatHeaders: false);
        var chunksNoRepeat = chunkerNoRepeat.Chunk(table);

        for (int i = 0; i < chunksNoRepeat.Count; i++)
        {
            Console.WriteLine($"Chunk {i + 1}:");
            Console.WriteLine(chunksNoRepeat[i].Text);
            Console.WriteLine($"(Tokens: {chunksNoRepeat[i].TokenCount}, Indices: {chunksNoRepeat[i].StartIndex}-{chunksNoRepeat[i].EndIndex})");
            Console.WriteLine();
        }

        // With header repetition (Python-style)
        Console.WriteLine("\nB. With Header Repetition (Python-style):");
        var chunkerRepeat = new TableChunker(tokenizer, chunkSize: 80, repeatHeaders: true);
        var chunksRepeat = chunkerRepeat.Chunk(table);

        for (int i = 0; i < chunksRepeat.Count; i++)
        {
            Console.WriteLine($"Chunk {i + 1}:");
            Console.WriteLine(chunksRepeat[i].Text);
            Console.WriteLine($"(Tokens: {chunksRepeat[i].TokenCount})");
            Console.WriteLine();
        }

        // Via Pipeline
        Console.WriteLine("\nC. Via Pipeline with repeat_headers:");
        var pipeline = new Pipeline()
            .ChunkWith("table", new { chunk_size = 80, repeat_headers = true });

        var doc = (Document)pipeline.Run(texts: table);
        Console.WriteLine($"Pipeline created {doc.Chunks.Count} chunk(s) with headers repeated");
        Console.WriteLine();
    }
}
