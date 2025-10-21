using Chonkie.Chunkers;
using Chonkie.Chefs;
using Chonkie.Core.Chunker;
using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;
using Chonkie.Fetchers;
using Chonkie.Porters;
using Chonkie.Refineries;
using Chonkie.Tokenizers;

namespace Chonkie.Pipeline.Sample;

/// <summary>
/// Demonstrates fluent pipeline API for building text processing workflows.
/// Similar to Python's Pipeline class with method chaining.
/// </summary>
class Program
{
    static async Task Main()
    {
        Console.WriteLine("=== Chonkie.Net Fluent Pipeline API Demo ===\n");
        Console.WriteLine("This sample demonstrates fluent, chainable API for building");
        Console.WriteLine("complete text processing pipelines in a readable, declarative style.\n");

        // Demo 1: Simple Pipeline - Direct Text Input
        await RunSimplePipeline();

        // Demo 2: File-Based Pipeline - Single File
        await RunSingleFilePipeline();

        // Demo 3: Complete RAG Pipeline - Multiple Files
        await RunCompleteRagPipeline();

        // Demo 4: Semantic Pipeline with Embeddings
        await RunSemanticPipeline();

        Console.WriteLine("\n=== Pipeline Demo Complete! ===");
    }

    /// <summary>
    /// Demo 1: Simple pipeline with direct text input
    /// Equivalent to Python: Pipeline().chunk_with("token").run(texts="...")
    /// </summary>
    static async Task RunSimplePipeline()
    {
        Console.WriteLine("--- Demo 1: Simple Pipeline (Direct Text) ---\n");

        var text = "Chonkie is the goodest boi! My favorite chunking hippo. " +
                   "It helps you process text efficiently for RAG applications. " +
                   "Chonkie makes text chunking simple and fun!";

        // Fluent pipeline: just chunk the text
        var result = await FluentPipeline.Create()
            .WithText(text)
            .WithTokenizer(new CharacterTokenizer())
            .ChunkWith(new TokenChunker(new CharacterTokenizer(), chunkSize: 50, chunkOverlap: 10))
            .RunAsync();

        Console.WriteLine($"✓ Created {result.FinalChunks.Count} chunks from direct text input\n");
        foreach (var chunk in result.FinalChunks.Take(2))
        {
            Console.WriteLine($"Chunk: \"{chunk.Text}\"");
            Console.WriteLine($"Tokens: {chunk.TokenCount}\n");
        }
    }

    /// <summary>
    /// Demo 2: Pipeline with file fetching
    /// Equivalent to Python: Pipeline().fetch_from("file").process_with("text").chunk_with("token").run()
    /// </summary>
    static async Task RunSingleFilePipeline()
    {
        Console.WriteLine("\n--- Demo 2: File-Based Pipeline ---\n");

        // Create sample file
        var sampleFile = "pipeline_sample.txt";
        await File.WriteAllTextAsync(sampleFile, 
            "Natural Language Processing (NLP) is a field of artificial intelligence. " +
            "It focuses on the interaction between computers and human language. " +
            "Modern NLP uses deep learning and transformer models. " +
            "These models have revolutionized how machines understand text.");

        try
        {
            // Fluent pipeline: fetch -> process -> chunk
            var result = await FluentPipeline.Create()
                .FetchFrom(new FileFetcher(), sampleFile)
                .ProcessWith(new TextChef())
                .ChunkWith(new SentenceChunker(new WordTokenizer(), chunkSize: 15))
                .RunAsync();

            Console.WriteLine($"✓ Processed file: {sampleFile}");
            Console.WriteLine($"✓ Created {result.FinalChunks.Count} sentence-based chunks\n");
            
            foreach (var chunk in result.FinalChunks)
            {
                Console.WriteLine($"[{chunk.StartIndex}-{chunk.EndIndex}] {chunk.Text}");
            }
        }
        finally
        {
            if (File.Exists(sampleFile))
                File.Delete(sampleFile);
        }
    }

    /// <summary>
    /// Demo 3: Complete RAG pipeline with refining and exporting
    /// Equivalent to Python: Pipeline().fetch_from().process_with().chunk_with().refine_with().export_with().run()
    /// </summary>
    static async Task RunCompleteRagPipeline()
    {
        Console.WriteLine("\n--- Demo 3: Complete RAG Pipeline ---\n");
        Console.WriteLine("Pipeline: Fetch → Process → Chunk → Refine → Export\n");

        // Create sample directory with multiple files
        var sampleDir = "pipeline_docs";
        Directory.CreateDirectory(sampleDir);

        var files = new Dictionary<string, string>
        {
            ["doc1.txt"] = "Machine learning is a subset of artificial intelligence. It enables systems to learn from data. Deep learning is a type of machine learning using neural networks.",
            ["doc2.txt"] = "Transformers are neural network architectures. They use attention mechanisms. BERT and GPT are popular transformer models.",
            ["doc3.txt"] = "RAG stands for Retrieval Augmented Generation. It combines retrieval and generation. This improves LLM accuracy with external knowledge."
        };

        try
        {
            foreach (var (filename, content) in files)
            {
                await File.WriteAllTextAsync(Path.Combine(sampleDir, filename), content);
            }

            // Complete pipeline with all stages
            var result = await FluentPipeline.Create()
                .FetchFrom(new FileFetcher(), sampleDir, "*.txt")
                .ProcessWith(new TextChef())
                .ChunkWith(new RecursiveChunker(
                    tokenizer: new WordTokenizer(),
                    chunkSize: 20))
                .RefineWith(new OverlapRefinery(minOverlap: 5))
                .ExportTo(new JsonPorter(), "pipeline_output.json")
                .RunAsync();

            Console.WriteLine($"✓ Fetched {files.Count} files from directory");
            Console.WriteLine($"✓ Processed all files with TextChef");
            Console.WriteLine($"✓ Created {result.InitialChunkCount} initial chunks");
            Console.WriteLine($"✓ Refined to {result.FinalChunks.Count} optimized chunks");
            Console.WriteLine($"✓ Exported to pipeline_output.json\n");

            Console.WriteLine("Sample refined chunks:");
            foreach (var chunk in result.FinalChunks.Take(3))
            {
                Console.WriteLine($"  • \"{chunk.Text.Substring(0, Math.Min(60, chunk.Text.Length))}...\"");
                Console.WriteLine($"    Tokens: {chunk.TokenCount}, Range: [{chunk.StartIndex}, {chunk.EndIndex}]");
            }
        }
        finally
        {
            if (Directory.Exists(sampleDir))
                Directory.Delete(sampleDir, true);
            if (File.Exists("pipeline_output.json"))
                File.Delete("pipeline_output.json");
        }
    }

    /// <summary>
    /// Demo 4: Semantic pipeline (template - embeddings would be added when available)
    /// Shows how semantic chunking would work in the pipeline
    /// </summary>
    static async Task RunSemanticPipeline()
    {
        Console.WriteLine("\n--- Demo 4: Semantic Pipeline Pattern ---\n");

        var text = "Artificial intelligence is transforming technology. " +
                   "Machine learning models can now understand context. " +
                   "Deep learning has enabled breakthroughs in NLP. " +
                   "Natural language processing powers many applications today.";

        // Semantic pipeline pattern (using RecursiveChunker as placeholder)
        var result = await FluentPipeline.Create()
            .WithText(text)
            .ProcessWith(new TextChef())
            .ChunkWith(new RecursiveChunker(
                tokenizer: new WordTokenizer(),
                chunkSize: 15))
            .RunAsync();

        Console.WriteLine("✓ Semantic-style chunking complete");
        Console.WriteLine($"✓ Created {result.FinalChunks.Count} semantically-grouped chunks\n");
        Console.WriteLine("Note: Full semantic chunking with embeddings");
        Console.WriteLine("will be available when embedding providers are configured.\n");

        foreach (var chunk in result.FinalChunks)
        {
            Console.WriteLine($"Chunk: {chunk.Text}");
        }
    }
}

/// <summary>
/// Fluent pipeline builder for composing text processing workflows.
/// Provides a chainable, declarative API similar to Python's Pipeline class.
/// </summary>
public class FluentPipeline
{
    private string? _inputText;
    private IFetcher? _fetcher;
    private string? _fetchPath;
    private string? _fetchPattern;
    private IChef? _chef;
    private IChunker? _chunker;
    private IRefinery? _refinery;
    private IPorter? _porter;
    private string? _exportPath;
    private ITokenizer? _tokenizer;

    private FluentPipeline() { }

    /// <summary>
    /// Creates a new fluent pipeline builder.
    /// </summary>
    public static FluentPipeline Create() => new();

    /// <summary>
    /// Sets the input text directly (skip fetcher stage).
    /// </summary>
    public FluentPipeline WithText(string text)
    {
        _inputText = text;
        return this;
    }

    /// <summary>
    /// Sets the tokenizer to use.
    /// </summary>
    public FluentPipeline WithTokenizer(ITokenizer tokenizer)
    {
        _tokenizer = tokenizer;
        return this;
    }

    /// <summary>
    /// Configures data fetching stage.
    /// </summary>
    public FluentPipeline FetchFrom(IFetcher fetcher, string path, string pattern = "*.*")
    {
        _fetcher = fetcher;
        _fetchPath = path;
        _fetchPattern = pattern;
        return this;
    }

    /// <summary>
    /// Configures text preprocessing stage.
    /// </summary>
    public FluentPipeline ProcessWith(IChef chef)
    {
        _chef = chef;
        return this;
    }

    /// <summary>
    /// Configures text chunking stage (required).
    /// </summary>
    public FluentPipeline ChunkWith(IChunker chunker)
    {
        _chunker = chunker;
        return this;
    }

    /// <summary>
    /// Configures chunk refinement stage (optional).
    /// </summary>
    public FluentPipeline RefineWith(IRefinery refinery)
    {
        _refinery = refinery;
        return this;
    }

    /// <summary>
    /// Configures export stage (optional).
    /// </summary>
    public FluentPipeline ExportTo(IPorter porter, string path)
    {
        _porter = porter;
        _exportPath = path;
        return this;
    }

    /// <summary>
    /// Executes the pipeline and returns the chunks.
    /// </summary>
    private async Task<List<Chunk>> ExecuteAsync()
    {
        if (_chunker == null)
            throw new InvalidOperationException("Pipeline must have a chunker. Use ChunkWith() to configure.");

        // Stage 1: Fetch or use direct text
        var texts = await FetchTextsAsync();

        // Stage 2: Process (optional)
        var processedTexts = _chef != null ? await ProcessTexts(texts) : texts;

        // Stage 3: Chunk (required)
        var chunks = ChunkTexts(processedTexts);

        // Stage 4: Refine (optional)
        if (_refinery != null)
        {
            chunks = await RefineChunks(chunks);
        }

        // Stage 5: Export (optional)
        if (_porter != null && _exportPath != null)
        {
            await _porter.ExportAsync(chunks, _exportPath);
        }

        return chunks;
    }

    /// <summary>
    /// Executes the pipeline and returns detailed results.
    /// </summary>
    public async Task<PipelineResult> RunAsync()
    {
        if (_chunker == null)
            throw new InvalidOperationException("Pipeline must have a chunker. Use ChunkWith() to configure.");

        var result = new PipelineResult();

        // Stage 1: Fetch or use direct text
        var texts = await FetchTextsAsync();
        result.SourceCount = texts.Count;

        // Stage 2: Process (optional)
        var processedTexts = _chef != null ? await ProcessTexts(texts) : texts;

        // Stage 3: Chunk (required)
        var chunks = ChunkTexts(processedTexts);
        result.InitialChunkCount = chunks.Count;

        // Stage 4: Refine (optional)
        if (_refinery != null)
        {
            chunks = await RefineChunks(chunks);
        }

        result.FinalChunks = chunks;

        // Stage 5: Export (optional)
        if (_porter != null && _exportPath != null)
        {
            await _porter.ExportAsync(chunks, _exportPath);
            result.ExportPath = _exportPath;
        }

        return result;
    }

    private async Task<List<string>> FetchTextsAsync()
    {
        if (_inputText != null)
        {
            return [_inputText];
        }

        if (_fetcher == null || _fetchPath == null)
        {
            throw new InvalidOperationException("Pipeline must have input. Use WithText() or FetchFrom().");
        }

        var fetchedData = await _fetcher.FetchAsync(_fetchPath, _fetchPattern ?? "*.*");
        return fetchedData.Select(d => d.Content).ToList();
    }

    private async Task<List<string>> ProcessTexts(List<string> texts)
    {
        if (_chef == null) return texts;
        var processed = new List<string>();
        foreach (var text in texts)
        {
            var result = await _chef.ProcessAsync(text);
            processed.Add(result);
        }
        return processed;
    }

    private List<Chunk> ChunkTexts(List<string> texts)
    {
        var allChunks = new List<Chunk>();
        foreach (var text in texts)
        {
            var chunks = _chunker!.Chunk(text);
            allChunks.AddRange(chunks);
        }
        return allChunks;
    }

    private async Task<List<Chunk>> RefineChunks(List<Chunk> chunks)
    {
        if (_refinery == null) return chunks;
        var refined = await _refinery.RefineAsync(chunks);
        return refined.ToList();
    }
}

/// <summary>
/// Results from pipeline execution with detailed metrics.
/// </summary>
public class PipelineResult
{
    /// <summary>
    /// Number of source texts/files processed.
    /// </summary>
    public int SourceCount { get; set; }

    /// <summary>
    /// Number of chunks before refinement.
    /// </summary>
    public int InitialChunkCount { get; set; }

    /// <summary>
    /// Final chunks after all processing.
    /// </summary>
    public List<Chunk> FinalChunks { get; set; } = [];

    /// <summary>
    /// Path where chunks were exported (if applicable).
    /// </summary>
    public string? ExportPath { get; set; }
}
