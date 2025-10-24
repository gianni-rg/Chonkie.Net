using Chonkie.Chunkers;
using Chonkie.Embeddings.Azure;
using Chonkie.Tokenizers;

namespace Chonkie.Sample;

/// <summary>
/// Sample application demonstrating the usage of Chonkie.Net library for text chunking.
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

        // Example 1: Token Chunker
        await DemoTokenChunker(sampleText);

        // Example 2: Sentence Chunker
        await DemoSentenceChunker(sampleText);

        // Example 3: Recursive Chunker
        await DemoRecursiveChunker(sampleText);

        // Example 4: Semantic Chunker (requires embeddings)
        await DemoSemanticChunker(sampleText);

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
}
