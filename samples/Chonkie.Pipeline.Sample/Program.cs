using Chonkie.Chefs;
using Chonkie.Chunkers;
using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;
using Chonkie.Embeddings.Azure;
using Chonkie.Embeddings.Interfaces;
using Chonkie.Embeddings.OpenAI;
using Chonkie.Embeddings.SentenceTransformers;
using Chonkie.Fetchers;
using Chonkie.Genies;
using Chonkie.Handshakes;
using Chonkie.Porters;
using Chonkie.Refineries;
using Chonkie.Tokenizers;
using Qdrant.Client.Grpc;
using System.Security.Cryptography;
using System.Text;

namespace Chonkie.Pipeline.Sample;

/// <summary>
/// Demonstrates fluent pipeline API for building text processing workflows.
/// Similar to Python's Pipeline class with method chaining.
/// </summary>
public static class Program
{
    /// <summary>
    /// Runs the pipeline tutorial demos.
    /// </summary>
    /// <param name="args">Optional arguments (use --rag to enable the RAG walkthrough).</param>
    static async Task Main(string[] args)
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

        // Demo 5: CHOMP Pipeline (string-based)
        await RunChompPipelineAsync();

        // Demo 6: Semantic RAG Pipeline with Embeddings
        await RunSemanticRagPipelineAsync();

        // Demo 7: Full RAG Walkthrough (optional)
        if (args.Any(arg => arg.Equals("--rag", StringComparison.OrdinalIgnoreCase) ||
                            arg.Equals("rag", StringComparison.OrdinalIgnoreCase)))
        {
            await RunRagTutorialAsync();
        }
        else
        {
            Console.WriteLine("\n--- Demo 7: RAG Tutorial (Skipped) ---\n");
            Console.WriteLine("Run with --rag to enable the full RAG walkthrough (requires embeddings + vector DB).\n");
        }


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
                Console.WriteLine($"  • \"{chunk.Text[..Math.Min(60, chunk.Text.Length)]}...\"");
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
    /// Demo 4: Full semantic chunking pipeline with embeddings
    /// Demonstrates advanced semantic chunking using embeddings for similarity-based boundary detection
    /// </summary>
    static async Task RunSemanticPipeline()
    {
        Console.WriteLine("\n--- Demo 4: Semantic Pipeline with Embeddings ---\n");

        var text = "Artificial intelligence is transforming technology. " +
                   "Machine learning models can now understand context. " +
                   "Deep learning has enabled breakthroughs in natural language processing. " +
                   "Transformers use attention mechanisms to process sequences. " +
                   "These models have revolutionized how machines understand text. " +
                   "Modern NLP systems achieve near-human performance. " +
                   "Transfer learning allows reusing pre-trained models. " +
                   "Fine-tuning adapts models to specific tasks efficiently.";

        // Get embeddings provider
        using var embeddingsHandle = CreateEmbeddingsHandle();
        Console.WriteLine($"Using embeddings: {embeddingsHandle.Source}\n");

        try
        {
            // Create semantic chunker with embeddings
            var semanticChunker = new Chonkie.Chunkers.SemanticChunker(
                tokenizer: new WordTokenizer(),
                embeddingModel: embeddingsHandle.Embeddings,
                threshold: 0.75f,  // Sensitivity to semantic boundaries (0-1)
                chunkSize: 50,     // Max tokens per chunk
                similarityWindow: 2);  // Window for similarity calculation

            // Semantic pipeline: process -> semantic chunk -> refine
            var result = await FluentPipeline.Create()
                .WithText(text)
                .ProcessWith(new TextChef())
                .ChunkWith(semanticChunker)
                .RefineWith(new OverlapRefinery(minOverlap: 3))
                .RunAsync();

            Console.WriteLine($"✓ Semantic chunking complete");
            Console.WriteLine($"✓ Created {result.FinalChunks.Count} semantically-coherent chunks\n");

            Console.WriteLine("Chunks (grouped by semantic similarity):");
            foreach (var chunk in result.FinalChunks)
            {
                Console.WriteLine($"  [{chunk.TokenCount} tokens] \"{chunk.Text}\"");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Semantic chunking demo failed: {ex.Message}");
            Console.WriteLine("Ensure embeddings are configured properly.\n");

            // Fallback to recursive chunking
            Console.WriteLine("Falling back to recursive chunking...\n");
            var result = await FluentPipeline.Create()
                .WithText(text)
                .ProcessWith(new TextChef())
                .ChunkWith(new RecursiveChunker(
                    tokenizer: new WordTokenizer(),
                    chunkSize: 50))
                .RunAsync();

            Console.WriteLine($"✓ Created {result.FinalChunks.Count} recursive chunks");
            foreach (var chunk in result.FinalChunks.Take(3))
            {
                Console.WriteLine($"  • \"{chunk.Text}\"");
            }
        }
    }

    /// <summary>
    /// Demonstrates the CHOMP pipeline using the string-based Pipeline API.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    static async Task RunChompPipelineAsync()
    {
        Console.WriteLine("\n--- Demo 5: CHOMP Pipeline (String-Based API) ---\n");

        var sampleFile = "chomp_sample.txt";
        await File.WriteAllTextAsync(sampleFile,
            "CHOMP pipelines orchestrate fetchers, chefs, chunkers, refineries, and porters. " +
            "This sample uses the built-in Pipeline API with string aliases.");

        try
        {
            // String-based pipeline configuration
            var pipeline = new Chonkie.Pipeline.Pipeline()
                .FetchFrom("file", new { path = sampleFile })
                .ProcessWith("text")
                .ChunkWith("recursive", new { chunk_size = 40 })
                .RefineWith("overlap", new { context_size = 8 });

            var result = await pipeline.RunAsync();

            if (result is Chonkie.Core.Types.Document doc)
            {
                Console.WriteLine($"✓ Pipeline created {doc.Chunks.Count} chunk(s)");
            }
            else if (result is IReadOnlyList<Chonkie.Core.Types.Document> docs)
            {
                var totalChunks = docs.Sum(d => d.Chunks.Count);
                Console.WriteLine($"✓ Pipeline created {totalChunks} chunk(s) across {docs.Count} document(s)");
            }
            else
            {
                Console.WriteLine("✓ Pipeline completed (result type not recognized)");
            }

            Console.WriteLine("✓ CHOMP Pipeline executed successfully");
        }
        finally
        {
            if (File.Exists(sampleFile))
                File.Delete(sampleFile);
        }
    }

    /// <summary>
    /// Demo 6: Semantic RAG pipeline combining semantic chunking with vector storage and retrieval.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    static async Task RunSemanticRagPipelineAsync()
    {
        Console.WriteLine("\n--- Demo 6: Semantic RAG Pipeline ---\n");
        Console.WriteLine("Pipeline: Semantic Chunking → Vector Storage → Semantic Retrieval → Generation\n");

        // Sample documents with rich semantic content
        var documents = new Dictionary<string, string>
        {
            ["ai_basics.txt"] =
                "Artificial intelligence (AI) refers to the simulation of human intelligence processes by machines. " +
                "AI systems can learn from data and make predictions. " +
                "Machine learning is a subset of AI that enables systems to learn without explicit programming. " +
                "Deep learning uses neural networks to process complex patterns in data.",

            ["nlp_guide.txt"] =
                "Natural language processing (NLP) is a branch of artificial intelligence. " +
                "NLP focuses on the interaction between computers and human language. " +
                "Transformers are neural network architectures that revolutionized NLP. " +
                "Attention mechanisms allow models to focus on relevant parts of text. " +
                "BERT and GPT are popular transformer-based models.",

            ["rag_explained.txt"] =
                "Retrieval augmented generation (RAG) is a technique combining retrieval with generation. " +
                "RAG systems retrieve relevant information from a knowledge base. " +
                "The retrieved context is then used by an LLM to generate grounded answers. " +
                "RAG improves factuality and reduces hallucinations in AI responses. " +
                "Semantic chunking and vector databases are crucial for RAG performance."
        };

        var sampleDir = "semantic_rag_docs";
        Directory.CreateDirectory(sampleDir);

        try
        {
            // Write sample documents
            foreach (var (filename, content) in documents)
            {
                await File.WriteAllTextAsync(Path.Combine(sampleDir, filename), content);
            }

            // Get embeddings provider
            using var embeddingsHandle = CreateEmbeddingsHandle();
            Console.WriteLine($"Embeddings: {embeddingsHandle.Source}");
            Console.WriteLine($"Embedding dimension: {embeddingsHandle.Embeddings.Dimension}\n");

            // Create semantic chunker
            var semanticChunker = new Chonkie.Chunkers.SemanticChunker(
                tokenizer: new WordTokenizer(),
                embeddingModel: embeddingsHandle.Embeddings,
                threshold: 0.7f,
                chunkSize: 50,
                similarityWindow: 2);

            // Step 1: Semantic chunking with refinement
            var pipelineResult = await FluentPipeline.Create()
                .FetchFrom(new FileFetcher(), sampleDir, "*.txt")
                .ProcessWith(new TextChef())
                .ChunkWith(semanticChunker)
                .RefineWith(new OverlapRefinery(minOverlap: 2))
                .ExportTo(new JsonPorter(), "semantic_chunks.json")
                .RunAsync();

            var allChunks = pipelineResult.FinalChunks.ToList();
            Console.WriteLine($"✓ Step 1: Semantic chunking");
            Console.WriteLine($"  - Processed {documents.Count} documents");
            Console.WriteLine($"  - Created {allChunks.Count} semantic chunks");
            Console.WriteLine($"  - Exported to semantic_chunks.json\n");

            // Step 2: Build semantic index with embeddings
            Console.WriteLine($"✓ Step 2: Building semantic index");
            var embeddings = new Dictionary<Chunk, float[]>();
            var embeddingsList = new List<float[]>();

            // Embed all chunks
            foreach (var chunk in allChunks)
            {
                var embedding = await embeddingsHandle.Embeddings.EmbedAsync(chunk.Text);
                embeddings[chunk] = embedding;
                embeddingsList.Add(embedding);
            }
            Console.WriteLine($"  - Generated embeddings for {allChunks.Count} chunks\n");

            // Step 3: Semantic search
            Console.WriteLine($"✓ Step 3: Semantic retrieval");
            var query = "How do transformers and attention work in NLP?";
            var queryEmbedding = await embeddingsHandle.Embeddings.EmbedAsync(query);
            Console.WriteLine($"  - Query: \"{query}\"");
            Console.WriteLine($"  - Searching {allChunks.Count} semantically-chunked documents\n");

            // Find most similar chunks
            var similarities = allChunks.Select(chunk =>
            {
                var sim = embeddingsHandle.Embeddings is DemoEmbeddings demo
                    ? demo.Similarity(queryEmbedding, embeddings[chunk])
                    : ComputeCosineSimilarity(queryEmbedding, embeddings[chunk]);
                return new { Chunk = chunk, Similarity = sim };
            })
            .OrderByDescending(x => x.Similarity)
            .Take(3)
            .ToList();

            var retrievedContexts = similarities.Select(x => x.Chunk.Text).ToList();
            Console.WriteLine($"  - Retrieved {retrievedContexts.Count} most relevant chunks:\n");

            foreach (var (result, index) in similarities.Select((x, i) => (x, i)))
            {
                Console.WriteLine($"  [{index + 1}] Relevance: {result.Similarity:P1}");
                Console.WriteLine($"      \"{result.Chunk.Text[..Math.Min(80, result.Chunk.Text.Length)]}...\"");
                Console.WriteLine();
            }

            // Step 4: Generation with retrieved context
            if (TryCreateGenie(out var genie, out var genieSource))
            {
                Console.WriteLine($"✓ Step 4: Generating answer with LLM");
                Console.WriteLine($"  - Using: {genieSource}\n");

                var prompt = BuildRagPrompt(query, retrievedContexts);
                try
                {
                    var answer = await genie.GenerateAsync(prompt);
                    Console.WriteLine($"Question: {query}");
                    Console.WriteLine($"\nAnswer:\n{answer}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  - Generation failed: {ex.Message}");
                    Console.WriteLine($"  - Retrieved context would be used for answer generation\n");
                }
            }
            else
            {
                Console.WriteLine($"✓ Step 4: (Generation skipped - no LLM configured)\n");
                Console.WriteLine("To enable generation, configure:");
                Console.WriteLine("  - OPENAI_API_KEY or");
                Console.WriteLine("  - AZURE_OPENAI_ENDPOINT + AZURE_OPENAI_API_KEY + AZURE_OPENAI_DEPLOYMENT_LLM\n");
            }

            Console.WriteLine("✓ Semantic RAG pipeline complete");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Semantic RAG pipeline failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            if (Directory.Exists(sampleDir))
                Directory.Delete(sampleDir, true);
            if (File.Exists("semantic_chunks.json"))
                File.Delete("semantic_chunks.json");
        }
    }

    /// <summary>
    /// Computes cosine similarity between two vectors.
    /// </summary>
    /// <param name="u">The first vector.</param>
    /// <param name="v">The second vector.</param>
    /// <returns>The cosine similarity (0-1).</returns>
    static float ComputeCosineSimilarity(float[] u, float[] v)
    {
        if (u.Length != v.Length)
            throw new ArgumentException("Vectors must have the same dimension");

        double dot = 0;
        double normU = 0;
        double normV = 0;

        for (var i = 0; i < u.Length; i++)
        {
            dot += u[i] * v[i];
            normU += u[i] * u[i];
            normV += v[i] * v[i];
        }

        var denom = Math.Sqrt(normU) * Math.Sqrt(normV);
        return denom <= 0 ? 0 : (float)(dot / denom);
    }

    /// <summary>
    /// Runs a full RAG walkthrough with chunking, embeddings, vector storage, retrieval, and generation.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    static async Task RunRagTutorialAsync()
    {
        Console.WriteLine("\n--- Demo 7: RAG Tutorial Walkthrough ---\n");

        var documents = new[]
        {
            "RAG combines retrieval with generation for grounded answers.",
            "Chunking breaks long documents into manageable pieces for embeddings.",
            "Vector databases store embeddings for fast semantic retrieval.",
            "LLMs generate answers using retrieved context to stay accurate."
        };

        var tokenizer = new WordTokenizer();
        var chunker = new RecursiveChunker(tokenizer, chunkSize: 40);
        var chunks = documents.SelectMany(chunker.Chunk).ToList();

        Console.WriteLine($"✓ Chunked {documents.Length} documents into {chunks.Count} chunks");

        using var embeddingsHandle = CreateEmbeddingsHandle();
        Console.WriteLine($"✓ Embeddings source: {embeddingsHandle.Source}");

        var qdrantUrl = Environment.GetEnvironmentVariable("CHONKIE_QDRANT_URL") ?? "http://localhost:6333";
        var collectionName = Environment.GetEnvironmentVariable("CHONKIE_QDRANT_COLLECTION") ?? "rag_tutorial";

        try
        {
            var handshake = new QdrantHandshake(qdrantUrl, collectionName, embeddingsHandle.Embeddings);
            await handshake.WriteAsync(chunks);
            Console.WriteLine($"✓ Stored chunks in Qdrant collection '{collectionName}'");

            var question = "How does RAG improve LLM answers?";
            var results = await handshake.SearchAsync(question, limit: 3);
            var contexts = results
                .Select(TryGetQdrantText)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Cast<string>()
                .ToList();

            Console.WriteLine($"✓ Retrieved {contexts.Count} relevant chunk(s)");

            if (TryCreateGenie(out var genie, out var genieSource))
            {
                Console.WriteLine($"✓ Using genie: {genieSource}");
                var prompt = BuildRagPrompt(question, contexts);
                var answer = await genie.GenerateAsync(prompt);
                Console.WriteLine($"\nAnswer: {answer}\n");
            }
            else
            {
                Console.WriteLine("LLM generation skipped (no API keys configured). Retrieved context:");
                foreach (var context in contexts)
                {
                    Console.WriteLine($" - {context}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RAG demo failed: {ex.Message}");
            Console.WriteLine("Ensure Qdrant is running and embeddings credentials are configured.");
        }
    }

    /// <summary>
    /// Builds a RAG prompt from a question and retrieved context.
    /// </summary>
    /// <param name="question">The user question.</param>
    /// <param name="contexts">Retrieved context chunks.</param>
    /// <returns>A prompt ready for LLM generation.</returns>
    static string BuildRagPrompt(string question, IReadOnlyList<string> contexts)
    {
        var contextText = string.Join("\n\n", contexts);
        return $"Use the context to answer the question. If the answer is not in the context, say 'I don't know'.\n\n" +
               $"Context:\n{contextText}\n\nQuestion: {question}\nAnswer:";
    }

    /// <summary>
    /// Attempts to extract the text payload from a Qdrant search result.
    /// </summary>
    /// <param name="point">The Qdrant scored point.</param>
    /// <returns>The text payload when available.</returns>
    static string? TryGetQdrantText(ScoredPoint point)
    {
        if (point.Payload.TryGetValue("text", out var value))
        {
            return value.StringValue ?? value.ToString();
        }

        return null;
    }

    /// <summary>
    /// Creates an embeddings provider based on environment variables or demo fallback.
    /// </summary>
    /// <returns>A handle that manages embeddings lifetime.</returns>
    static EmbeddingsHandle CreateEmbeddingsHandle()
    {
        if (TryCreateSentenceTransformerEmbeddings(out var local, out _))
        {
            return local;
        }

        if (TryCreateAzureEmbeddings(out var azure, out var azureSource))
        {
            return new EmbeddingsHandle(azure, azureSource);
        }

        if (TryCreateOpenAiEmbeddings(out var openAi, out var openAiSource))
        {
            return new EmbeddingsHandle(openAi, openAiSource);
        }

        return new EmbeddingsHandle(new DemoEmbeddings(), "demo (no provider configured)");
    }

    /// <summary>
    /// Attempts to create Azure OpenAI embeddings from environment variables.
    /// </summary>
    /// <param name="embeddings">The embeddings provider when created.</param>
    /// <param name="source">The source label for diagnostics.</param>
    /// <returns>True if a provider was created.</returns>
    static bool TryCreateAzureEmbeddings(out IEmbeddings embeddings, out string source)
    {
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_EMBEDDINGS_DEPLOYMENT");

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(deployment))
        {
            embeddings = null!;
            source = string.Empty;
            return false;
        }

        embeddings = new AzureOpenAIEmbeddings(endpoint, apiKey, deployment);
        source = $"azure-openai:{deployment}";
        return true;
    }

    /// <summary>
    /// Attempts to create OpenAI embeddings from environment variables.
    /// </summary>
    /// <param name="embeddings">The embeddings provider when created.</param>
    /// <param name="source">The source label for diagnostics.</param>
    /// <returns>True if a provider was created.</returns>
    static bool TryCreateOpenAiEmbeddings(out IEmbeddings embeddings, out string source)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            embeddings = null!;
            source = string.Empty;
            return false;
        }

        var model = Environment.GetEnvironmentVariable("OPENAI_EMBEDDINGS_MODEL") ?? "text-embedding-3-small";
        embeddings = new OpenAIEmbeddings(apiKey, model);
        source = $"openai:{model}";
        return true;
    }

    /// <summary>
    /// Attempts to create local Sentence Transformer embeddings from environment variables.
    /// </summary>
    /// <param name="handle">The embeddings handle when created.</param>
    /// <param name="source">The source label for diagnostics.</param>
    /// <returns>True if a provider was created.</returns>
    static bool TryCreateSentenceTransformerEmbeddings(out EmbeddingsHandle handle, out string source)
    {
        var modelPath = Environment.GetEnvironmentVariable("CHONKIE_SENTENCE_TRANSFORMER_MODEL_PATH");
        if (string.IsNullOrWhiteSpace(modelPath))
        {
            handle = null!;
            source = string.Empty;
            return false;
        }

        var embeddings = new SentenceTransformerEmbeddings(modelPath);
        source = $"sentence-transformers:{Path.GetFileName(modelPath)}";
        handle = new EmbeddingsHandle(embeddings, source, embeddings);
        return true;
    }

    /// <summary>
    /// Attempts to create a genie for LLM generation from environment variables.
    /// </summary>
    /// <param name="genie">The configured genie instance.</param>
    /// <param name="source">The genie source label.</param>
    /// <returns>True if a genie was created.</returns>
    static bool TryCreateGenie(out IGeneration genie, out string source)
    {
        try
        {
            var azureEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
            var azureKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
            var azureDeployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_LLM");

            if (!string.IsNullOrWhiteSpace(azureEndpoint) &&
                !string.IsNullOrWhiteSpace(azureKey) &&
                !string.IsNullOrWhiteSpace(azureDeployment))
            {
                genie = new AzureOpenAIGenie(azureEndpoint, azureKey, azureDeployment);
                source = $"azure-openai:{azureDeployment}";
                return true;
            }

            var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (!string.IsNullOrWhiteSpace(openAiKey))
            {
                genie = new OpenAIGenie(openAiKey);
                source = "openai:gpt-4o";
                return true;
            }
        }
        catch (Exception)
        {
            // Ignore and fall through to return false.
        }

        genie = null!;
        source = string.Empty;
        return false;
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
        return [.. fetchedData.Select(d => d.Content)];
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
        return [.. refined];
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

/// <summary>
/// Wraps an embeddings provider and handles cleanup when needed.
/// </summary>
public sealed class EmbeddingsHandle : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingsHandle"/> class.
    /// </summary>
    /// <param name="embeddings">The embeddings provider.</param>
    /// <param name="source">The provider source label.</param>
    /// <param name="disposable">Optional disposable resource for cleanup.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="embeddings"/> is null.</exception>
    public EmbeddingsHandle(IEmbeddings embeddings, string source, IDisposable? disposable = null)
    {
        Embeddings = embeddings ?? throw new ArgumentNullException(nameof(embeddings));
        Source = source;
        _disposable = disposable;
    }

    /// <summary>
    /// Gets the embeddings provider.
    /// </summary>
    public IEmbeddings Embeddings { get; }

    /// <summary>
    /// Gets the provider source label.
    /// </summary>
    public string Source { get; }

    private readonly IDisposable? _disposable;

    /// <summary>
    /// Disposes any underlying resources if required.
    /// </summary>
    public void Dispose()
    {
        _disposable?.Dispose();
    }
}

/// <summary>
/// A deterministic, offline embeddings provider for samples without external dependencies.
/// </summary>
public sealed class DemoEmbeddings : IEmbeddings
{
    private readonly int _dimension;

    /// <summary>
    /// Initializes a new instance of the <see cref="DemoEmbeddings"/> class.
    /// </summary>
    /// <param name="dimension">The embedding vector dimension.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dimension"/> is not positive.</exception>
    public DemoEmbeddings(int dimension = 384)
    {
        if (dimension <= 0)
        {
            throw new ArgumentException("Dimension must be positive.", nameof(dimension));
        }

        _dimension = dimension;
    }

    /// <inheritdoc />
    public string Name => "demo-embeddings";

    /// <inheritdoc />
    public int Dimension => _dimension;

    /// <inheritdoc />
    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateEmbedding(text));
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        var results = texts.Select(CreateEmbedding).ToList();
        return Task.FromResult((IReadOnlyList<float[]>)results);
    }

    /// <summary>
    /// Computes cosine similarity for demo embeddings.
    /// </summary>
    /// <param name="u">The first vector.</param>
    /// <param name="v">The second vector.</param>
    /// <returns>The cosine similarity value.</returns>
    /// <exception cref="ArgumentException">Thrown when vector dimensions do not match.</exception>
    public float Similarity(float[] u, float[] v)
    {
        if (u.Length != v.Length)
        {
            throw new ArgumentException("Vectors must have the same dimension.", nameof(v));
        }

        double dot = 0;
        double normU = 0;
        double normV = 0;

        for (var i = 0; i < u.Length; i++)
        {
            dot += u[i] * v[i];
            normU += u[i] * u[i];
            normV += v[i] * v[i];
        }

        var denom = Math.Sqrt(normU) * Math.Sqrt(normV);
        return denom <= 0 ? 0 : (float)(dot / denom);
    }

    /// <summary>
    /// Creates a deterministic embedding vector from a text input.
    /// </summary>
    /// <param name="text">The text to embed.</param>
    /// <returns>A normalized embedding vector.</returns>
    private float[] CreateEmbedding(string text)
    {
        var input = Encoding.UTF8.GetBytes(text ?? string.Empty);
        var hash = SHA256.HashData(input);

        var vector = new float[_dimension];
        for (var i = 0; i < _dimension; i++)
        {
            var value = hash[i % hash.Length];
            vector[i] = (value - 128) / 128f;
        }

        Normalize(vector);
        return vector;
    }

    /// <summary>
    /// Normalizes a vector in-place to unit length.
    /// </summary>
    /// <param name="vector">The vector to normalize.</param>
    private static void Normalize(float[] vector)
    {
        double sum = 0;
        foreach (var value in vector)
        {
            sum += value * value;
        }

        var norm = Math.Sqrt(sum);
        if (norm <= 0)
        {
            return;
        }

        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] = (float)(vector[i] / norm);
        }
    }
}
