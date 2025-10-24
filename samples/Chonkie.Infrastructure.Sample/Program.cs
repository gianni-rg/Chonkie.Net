using Chonkie.Fetchers;
using Chonkie.Chefs;
using Chonkie.Refineries;
using Chonkie.Porters;
using Chonkie.Chunkers;
using Chonkie.Tokenizers;
using Chonkie.Core.Types;

namespace Chonkie.Infrastructure.Sample;

/// <summary>
/// Sample application demonstrating the Chonkie.Net infrastructure pipeline components.
/// This showcases the complete text processing workflow: Fetch → Preprocess → Chunk → Refine → Export
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Chonkie.Net Infrastructure Pipeline Demo ===\n");

        // Create sample data directory and files for demonstration
        await CreateSampleDataAsync();

        Console.WriteLine("Pipeline: Fetch → Preprocess → Chunk → Refine → Export\n");
        Console.WriteLine(new string('=', 80) + "\n");

        // Step 1: FETCH - Load text files from directory
        Console.WriteLine("Step 1: FETCH - Loading text files from directory...");
        var fetcher = new FileFetcher();
        var files = await fetcher.FetchAsync("./sample_data", "*.txt");
        Console.WriteLine($"   ✓ Fetched {files.Count} file(s)\n");

        // Step 2: PREPROCESS - Clean and normalize text
        Console.WriteLine("Step 2: PREPROCESS - Cleaning and normalizing text...");
        var chef = new TextChef();
        var processedTexts = new List<string>();
        foreach (var file in files)
        {
            var processed = await chef.ProcessAsync(file.Content);
            processedTexts.Add(processed);
            Console.WriteLine($"   ✓ Processed: {Path.GetFileName(file.Path)}");
        }
        Console.WriteLine();

        // Step 3: CHUNK - Split text into manageable chunks
        Console.WriteLine("Step 3: CHUNK - Splitting text into chunks...");
        var tokenizer = new WordTokenizer();
        var chunker = new TokenChunker(tokenizer, chunkSize: 50, chunkOverlap: 10);

        var allChunks = new List<Chunk>();
        foreach (var text in processedTexts)
        {
            var chunks = chunker.Chunk(text);
            allChunks.AddRange(chunks);
        }
        Console.WriteLine($"   ✓ Created {allChunks.Count} chunks\n");

        // Step 4: REFINE - Post-process chunks (merge overlaps)
        Console.WriteLine("Step 4: REFINE - Optimizing chunks...");
        var refinery = new OverlapRefinery(minOverlap: 8);
        var refinedChunks = await refinery.RefineAsync(allChunks);
        Console.WriteLine($"   ✓ Refined to {refinedChunks.Count} optimized chunks\n");

        // Step 5: EXPORT - Save results to JSON
        Console.WriteLine("Step 5: EXPORT - Saving results...");
        var porter = new JsonPorter();
        var outputPath = "chunked_output.json";
        await porter.ExportAsync(refinedChunks, outputPath);
        Console.WriteLine($"   ✓ Exported to: {Path.GetFullPath(outputPath)}\n");

        // Display summary
        Console.WriteLine(new string('=', 80));
        Console.WriteLine("Pipeline Summary:");
        Console.WriteLine($"  Files processed:  {files.Count}");
        Console.WriteLine($"  Initial chunks:   {allChunks.Count}");
        Console.WriteLine($"  Refined chunks:   {refinedChunks.Count}");
        Console.WriteLine($"  Output file:      {outputPath}");
        Console.WriteLine(new string('=', 80));

        // Display sample chunk
        if (refinedChunks.Count > 0)
        {
            Console.WriteLine("\nSample Chunk Preview:");
            var sample = refinedChunks[0];
            var preview = sample.Text.Length > 150 ? sample.Text.Substring(0, 150) + "..." : sample.Text;
            Console.WriteLine($"  Text: {preview}");
            Console.WriteLine($"  Tokens: {sample.TokenCount}");
            Console.WriteLine($"  Range: [{sample.StartIndex}, {sample.EndIndex}]");
        }

        Console.WriteLine("\n=== Infrastructure Pipeline Complete ===");
    }

    static async Task CreateSampleDataAsync()
    {
        var dataDir = "sample_data";
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }

        // Create sample text files for demonstration
        var sampleTexts = new Dictionary<string, string>
        {
            ["ai_fundamentals.txt"] = @"Artificial Intelligence represents the simulation of human intelligence by machines. 
Machine learning is a subset of AI that enables systems to learn and improve from experience without being explicitly programmed. 
Deep learning uses neural networks with multiple layers to progressively extract higher-level features from raw input. 
Natural Language Processing allows computers to understand, interpret, and generate human language in a valuable way.",

            ["ml_overview.txt"] = @"Supervised learning algorithms learn from labeled training data to make predictions on unseen data. 
Unsupervised learning identifies patterns in data without pre-existing labels or categories. 
Reinforcement learning trains agents to make sequential decisions by rewarding desired behaviors. 
Feature engineering transforms raw data into features that better represent the underlying problem.",

            ["future_tech.txt"] = @"Quantum computing promises exponential speedups for certain computational problems. 
Edge AI brings artificial intelligence processing closer to data sources for faster response times. 
Explainable AI aims to make machine learning models more transparent and interpretable. 
Federated learning enables collaborative model training while keeping data decentralized and private."
        };

        foreach (var (filename, content) in sampleTexts)
        {
            var path = Path.Combine(dataDir, filename);
            if (!File.Exists(path))
            {
                await File.WriteAllTextAsync(path, content);
            }
        }
    }
}
