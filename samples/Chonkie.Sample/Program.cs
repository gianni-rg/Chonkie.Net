using Chonkie.Chunkers;
using Chonkie.Core.Types;
using Chonkie.Embeddings.Azure;
using Chonkie.Tokenizers;

namespace Chonkie.Sample;

/// <summary>
/// Sample application demonstrating the usage of Chonkie.Net library for text chunking.
/// Includes both basic chunking features and Python parity features.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Chonkie.Net Sample Application ===\n");

        // Sample text for demonstration
        var sampleText = @"Artificial Intelligence has revolutionized many aspects of modern technology.
Machine learning algorithms can now process vast amounts of data to identify patterns and make predictions.
Natural Language Processing enables computers to understand and generate human language.
Deep learning models, inspired by the human brain, have achieved remarkable results in image recognition,
speech synthesis, and game playing. The field continues to evolve rapidly with new breakthroughs emerging regularly.";

        Console.WriteLine("Sample Text:");
        Console.WriteLine(sampleText);
        Console.WriteLine("\n" + new string('=', 80) + "\n");

        // Basic Chunker Examples
        await DemoTokenChunker(sampleText);
        await DemoSentenceChunker(sampleText);
        await DemoRecursiveChunker(sampleText);
        await DemoSemanticChunker(sampleText);

        Console.WriteLine("\n" + new string('=', 80) + "\n");

        // Python Parity Features
        DemoMarkdownDocument();
        DemoInvalidTableLogging();
        DemoHeaderRepetition();

        Console.WriteLine("\n=== Sample Application Complete ===");
    }

    static Task DemoTokenChunker(string text)
    {
        Console.WriteLine("1. Token Chunker Example");
        Console.WriteLine("   Splits text into chunks based on token count.\n");

        var tokenizer = new WordTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 20, chunkOverlap: 5);

        var chunks = chunker.Chunk(text);

        Console.WriteLine($"   Created {chunks.Count} chunks:");
        for (int i = 0; i < chunks.Count; i++)
        {
            Console.WriteLine($"   Chunk {i + 1}: {chunks[i].Text[..Math.Min(60, chunks[i].Text.Length)]}...");
            Console.WriteLine($"   Token Count: {chunks[i].TokenCount}\n");
        }
        Console.WriteLine(new string('-', 80) + "\n");

        return Task.CompletedTask;
    }

    static Task DemoSentenceChunker(string text)
    {
        Console.WriteLine("2. Sentence Chunker Example");
        Console.WriteLine("   Splits text into chunks based on sentences.\n");

        var tokenizer = new WordTokenizer();
        var chunker = new SentenceChunker(tokenizer, chunkSize: 50, chunkOverlap: 10);

        var chunks = chunker.Chunk(text);

        Console.WriteLine($"   Created {chunks.Count} chunks:");
        for (int i = 0; i < chunks.Count; i++)
        {
            Console.WriteLine($"   Chunk {i + 1}: {chunks[i].Text[..Math.Min(60, chunks[i].Text.Length)]}...");
            Console.WriteLine($"   Token Count: {chunks[i].TokenCount}\n");
        }
        Console.WriteLine(new string('-', 80) + "\n");

        return Task.CompletedTask;
    }

    static Task DemoRecursiveChunker(string text)
    {
        Console.WriteLine("3. Recursive Chunker Example");
        Console.WriteLine("   Uses hierarchical splitting with multiple separators.\n");

        var tokenizer = new WordTokenizer();
        var chunker = new RecursiveChunker(tokenizer, chunkSize: 30);

        var chunks = chunker.Chunk(text);

        Console.WriteLine($"   Created {chunks.Count} chunks:");
        for (int i = 0; i < chunks.Count; i++)
        {
            Console.WriteLine($"   Chunk {i + 1}: {chunks[i].Text[..Math.Min(60, chunks[i].Text.Length)]}...");
            Console.WriteLine($"   Token Count: {chunks[i].TokenCount}\n");
        }
        Console.WriteLine(new string('-', 80) + "\n");

        return Task.CompletedTask;
    }

    static Task DemoSemanticChunker(string text)
    {
        Console.WriteLine("4. Semantic Chunker Example");
        Console.WriteLine("   Groups semantically similar sentences together.");

        var tokenizer = new WordTokenizer();
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("ERROR: Please set the AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_API_KEY environment variables.");
            return Task.CompletedTask;
        }
        var embeddings = new AzureOpenAIEmbeddings(
            endpoint: endpoint,
            apiKey: apiKey,
            deploymentName: "text-embedding-3-large",
            dimension: 3072);
        var chunker = new SemanticChunker(
            tokenizer: tokenizer,
            embeddingModel: embeddings,
            chunkSize: 50,
            threshold: 0.5f
        );

        var chunks = chunker.Chunk(text);

        Console.WriteLine($"   Created {chunks.Count} chunks:");
        for (int i = 0; i < chunks.Count; i++)
        {
            Console.WriteLine($"   Chunk {i + 1}: {chunks[i].Text[..Math.Min(60, chunks[i].Text.Length)]}...");
            Console.WriteLine($"   Token Count: {chunks[i].TokenCount}\n");
        }

        Console.WriteLine(new string('-', 80) + "\n");

        return Task.CompletedTask;
    }

    static void DemoMarkdownDocument()
    {
        Console.WriteLine("5. MarkdownDocument Type");
        Console.WriteLine("   Demonstrates the MarkdownDocument structure with tables, code, and images.\n");

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

        Console.WriteLine($"   Document ID: {doc.Id}");
        Console.WriteLine($"   Source: {doc.Source}");
        Console.WriteLine($"   Tables: {doc.Tables.Count}");
        Console.WriteLine($"   Code blocks: {doc.Code.Count}");
        Console.WriteLine($"   Images: {doc.Images.Count}");
        Console.WriteLine(new string('-', 80) + "\n");
    }

    static void DemoInvalidTableLogging()
    {
        Console.WriteLine("6. Invalid Table Logging");
        Console.WriteLine("   Shows how invalid tables (header-only) are handled.\n");

        var tokenizer = new CharacterTokenizer();
        var chunker = new TableChunker(tokenizer, chunkSize: 100);

        // Invalid table: only header and separator, no data rows
        var invalidTable = @"| Name | Value |
|------|-------|";

        Console.WriteLine("   Processing invalid table (header only, no data rows)...");
        var chunks = chunker.Chunk(invalidTable);
        Console.WriteLine($"   Result: Treated as normal text, {chunks.Count} chunk(s) created");
        Console.WriteLine("   (Check logs for warning message)");
        Console.WriteLine(new string('-', 80) + "\n");
    }

    static void DemoHeaderRepetition()
    {
        Console.WriteLine("7. Header Repetition Mode");
        Console.WriteLine("   Compares chunking with and without header repetition.\n");

        var tokenizer = new CharacterTokenizer();
        var table = @"| Product | Price | Stock |
|---------|-------|-------|
| Apple   | $1.50 | 100   |
| Banana  | $0.75 | 150   |
| Orange  | $1.25 | 80    |
| Grape   | $2.00 | 60    |
| Mango   | $1.80 | 40    |";

        // Without header repetition (.NET default)
        Console.WriteLine("   A. Without Header Repetition (default):");
        var chunkerNoRepeat = new TableChunker(tokenizer, chunkSize: 80, repeatHeaders: false);
        var chunksNoRepeat = chunkerNoRepeat.Chunk(table);

        for (int i = 0; i < chunksNoRepeat.Count; i++)
        {
            Console.WriteLine($"      Chunk {i + 1}:");
            Console.WriteLine($"      {chunksNoRepeat[i].Text.Replace("\n", "\n      ")}");
            Console.WriteLine($"      (Tokens: {chunksNoRepeat[i].TokenCount})");
            Console.WriteLine();
        }

        // With header repetition (Python-style)
        Console.WriteLine("   B. With Header Repetition (Python-style):");
        var chunkerRepeat = new TableChunker(tokenizer, chunkSize: 80, repeatHeaders: true);
        var chunksRepeat = chunkerRepeat.Chunk(table);

        for (int i = 0; i < chunksRepeat.Count; i++)
        {
            Console.WriteLine($"      Chunk {i + 1}:");
            Console.WriteLine($"      {chunksRepeat[i].Text.Replace("\n", "\n      ")}");
            Console.WriteLine($"      (Tokens: {chunksRepeat[i].TokenCount})");
            Console.WriteLine();
        }

        // Via Pipeline
        Console.WriteLine("   C. Via Pipeline with repeat_headers:");
        var pipeline = new Chonkie.Pipeline.Pipeline()
            .ChunkWith("table", new { chunk_size = 80, repeat_headers = true });

        var doc = (Document)pipeline.Run(texts: table);
        Console.WriteLine($"      Pipeline created {doc.Chunks.Count} chunk(s) with headers repeated");
        Console.WriteLine(new string('-', 80) + "\n");
    }
}
