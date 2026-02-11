using System;
using System.Linq;
using System.Threading.Tasks;
using Chonkie.Embeddings.SentenceTransformers;

namespace Chonkie.Examples
{
    /// <summary>
    /// Example demonstrating the use of ONNX Sentence Transformer embeddings.
    /// </summary>
    static class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Chonkie.Net - Sentence Transformer Embeddings Example ===\n");

            // Check if model path is provided
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run <path-to-model-directory>");
                Console.WriteLine("\nExample:");
                Console.WriteLine("  dotnet run ./models/all-MiniLM-L6-v2");
                Console.WriteLine("\nTo convert a model:");
                Console.WriteLine("  use the provided script (run in the uv env)");
                Console.WriteLine("  python scripts/convert_model.py sentence-transformers/all-MiniLM-L6-v2");
                return;
            }

            var modelPath = args[0];

            // Validate model
            Console.WriteLine("Validating model...");
            if (!ModelManager.ValidateModel(modelPath))
            {
                Console.WriteLine($"❌ Invalid model directory: {modelPath}");
                Console.WriteLine("\nRequired files:");
                Console.WriteLine("  - model.onnx");
                Console.WriteLine("  - config.json");
                Console.WriteLine("  - vocab.txt");
                Console.WriteLine("  - tokenizer_config.json (optional but recommended)");
                return;
            }
            Console.WriteLine("✓ Model validated successfully\n");

            // Get model metadata
            var metadata = ModelManager.GetModelMetadata(modelPath);
            Console.WriteLine($"Model Metadata:");
            Console.WriteLine($"  Type: {metadata.ModelType}");
            Console.WriteLine($"  Hidden Size: {metadata.HiddenSize}");
            Console.WriteLine($"  Embedding Dimension: {metadata.EmbeddingDimension}");
            Console.WriteLine($"  Max Position Embeddings: {metadata.MaxPositionEmbeddings}");
            Console.WriteLine($"  Vocabulary Size: {metadata.VocabSize}");
            Console.WriteLine($"  Pooling Mode: {metadata.PoolingMode}");
            Console.WriteLine();

            // Initialize embeddings
            Console.WriteLine("Loading model...");
            using var embeddings = new SentenceTransformerEmbeddings(modelPath);
            Console.WriteLine($"✓ Model loaded successfully");
            Console.WriteLine($"  Name: {embeddings.Name}");
            Console.WriteLine($"  Dimension: {embeddings.Dimension}");
            Console.WriteLine($"  Max Sequence Length: {embeddings.MaxSequenceLength}");
            Console.WriteLine();

            // Example 1: Single text embedding
            Console.WriteLine("=== Example 1: Single Text Embedding ===");
            var text = "The quick brown fox jumps over the lazy dog.";
            Console.WriteLine($"Text: \"{text}\"");

            var tokenCount = embeddings.CountTokens(text);
            Console.WriteLine($"Token count: {tokenCount}");

            var embedding = await embeddings.EmbedAsync(text);
            Console.WriteLine($"Embedding dimension: {embedding.Length}");
            Console.WriteLine($"First 5 values: [{string.Join(", ", embedding.Take(5).Select(x => x.ToString("F4")))}]");
            Console.WriteLine($"Norm: {Math.Sqrt(embedding.Sum(x => x * x)):F4}");
            Console.WriteLine();

            // Example 2: Batch embedding
            Console.WriteLine("=== Example 2: Batch Embedding ===");
            var texts = new[]
            {
                "Machine learning is a subset of artificial intelligence.",
                "Deep learning uses neural networks with multiple layers.",
                "Natural language processing enables computers to understand text.",
                "Computer vision allows machines to interpret visual information."
            };

            Console.WriteLine($"Embedding {texts.Length} texts...");
            var batchEmbeddings = await embeddings.EmbedBatchAsync(texts);

            for (int i = 0; i < texts.Length; i++)
            {
                Console.WriteLine($"\nText {i + 1}: \"{texts[i]}\"");
                Console.WriteLine($"  Embedding dimension: {batchEmbeddings[i].Length}");
                Console.WriteLine($"  First 3 values: [{string.Join(", ", batchEmbeddings[i].Take(3).Select(x => x.ToString("F4")))}]");
            }
            Console.WriteLine();

            // Example 3: Semantic similarity
            Console.WriteLine("=== Example 3: Semantic Similarity ===");
            var query = "What is artificial intelligence?";
            var documents = new[]
            {
                "Artificial intelligence is the simulation of human intelligence by machines.",
                "Machine learning is a method of data analysis that automates analytical model building.",
                "Python is a high-level programming language.",
                "The weather today is sunny and warm."
            };

            Console.WriteLine($"Query: \"{query}\"\n");
            Console.WriteLine("Documents:");
            for (int i = 0; i < documents.Length; i++)
            {
                Console.WriteLine($"  {i + 1}. \"{documents[i]}\"");
            }
            Console.WriteLine();

            var queryEmbedding = await embeddings.EmbedAsync(query);
            var docEmbeddings = await embeddings.EmbedBatchAsync(documents);

            Console.WriteLine("Similarity scores:");
            for (int i = 0; i < documents.Length; i++)
            {
                var similarity = CosineSimilarity(queryEmbedding, docEmbeddings[i]);
                Console.WriteLine($"  {i + 1}. {similarity:F4} - {documents[i]}");
            }
            Console.WriteLine();

            // Example 4: Finding most similar documents
            Console.WriteLine("=== Example 4: Finding Most Similar Documents ===");
            var similarities = docEmbeddings
                .Select((emb, idx) => new { Index = idx, Similarity = CosineSimilarity(queryEmbedding, emb) })
                .OrderByDescending(x => x.Similarity)
                .ToList();

            Console.WriteLine("Documents ranked by relevance:");
            for (int i = 0; i < similarities.Count; i++)
            {
                var sim = similarities[i];
                Console.WriteLine($"  {i + 1}. [{sim.Similarity:F4}] {documents[sim.Index]}");
            }

            Console.WriteLine("\n=== Example completed successfully! ===");
        }

        /// <summary>
        /// Computes cosine similarity between two vectors.
        /// </summary>
        static float CosineSimilarity(float[] a, float[] b)
        {
            return Chonkie.Embeddings.VectorMath.CosineSimilarity(a, b);
        }
    }
}
