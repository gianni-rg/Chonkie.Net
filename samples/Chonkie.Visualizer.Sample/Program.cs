using Chonkie.Chunkers;
using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;
using Chonkie.Tokenizers;
using Chonkie.Visualizer.Sample;

Console.WriteLine("=== Chonkie.Net Chunk Visualizer Demo ===\n");
Console.WriteLine("Visualize chunks in your terminal with color-coded output.\n");

// Sample text for chunking
var sampleText = @"Artificial Intelligence has revolutionized technology in recent years. 
Machine learning models can now understand and generate human language with remarkable accuracy. 
Deep learning architectures, particularly transformers, have enabled breakthroughs in natural language processing. 
These advances power modern applications like chatbots, search engines, and content recommendation systems.";

// Demo 1: Visualize Token Chunks
Console.WriteLine("=== Demo 1: Token Chunker Visualization ===\n");
var tokenizer = new WordTokenizer();
var tokenChunker = new TokenChunker(tokenizer, chunkSize: 20, chunkOverlap: 5);
var chunks = tokenChunker.Chunk(sampleText);

Console.WriteLine($"Total: {chunks.Count} chunks\n");
ChunkVisualizer.Print(chunks, showOverlap: true);

// Demo 2: Visualize Sentence Chunks
Console.WriteLine("\n\n=== Demo 2: Sentence Chunker Visualization ===\n");
var sentenceChunker = new SentenceChunker(tokenizer, chunkSize: 25);
var sentenceChunks = sentenceChunker.Chunk(sampleText);

Console.WriteLine($"Total: {sentenceChunks.Count} chunks\n");
ChunkVisualizer.Print(sentenceChunks, showMetadata: true);

// Demo 3: Visualize Recursive Chunks
Console.WriteLine("\n\n=== Demo 3: Recursive Chunker Visualization ===\n");
var recursiveChunker = new RecursiveChunker(tokenizer, chunkSize: 30);
var recursiveChunks = recursiveChunker.Chunk(sampleText);

Console.WriteLine($"Total: {recursiveChunks.Count} chunks\n");
ChunkVisualizer.Print(recursiveChunks, showBoundaries: true);

// Demo 4: Compare Chunkers Side-by-Side
Console.WriteLine("\n\n=== Demo 4: Chunker Comparison ===\n");
ChunkVisualizer.Compare(new Dictionary<string, IReadOnlyList<Chunk>>
{
    ["Token (20)"] = chunks,
    ["Sentence (25)"] = sentenceChunks,
    ["Recursive (30)"] = recursiveChunks
});

// Demo 5: Chunk Summary Statistics
Console.WriteLine("\n\n=== Demo 5: Chunk Summary Statistics ===\n");
ChunkVisualizer.PrintSummary(chunks);

// Demo 6: Bar Chart Visualization
Console.WriteLine("=== Demo 6: Chunk Size Distribution ===\n");
ChunkVisualizer.PrintBarChart(chunks);

// Demo 7: Highlighted Text with Boundaries
Console.WriteLine("=== Demo 7: Text with Highlighted Boundaries ===\n");
ChunkVisualizer.PrintHighlighted(chunks, sampleText);

// Demo 8: HTML Export
Console.WriteLine("=== Demo 8: HTML Export ===\n");
var htmlFilePath = "chunks_visualization.html";
ChunkVisualizer.SaveHtml(htmlFilePath, chunks, "Token Chunker Analysis");
Console.WriteLine($"Open {htmlFilePath} in your browser to view the visualization.\n");

Console.WriteLine("=== Visualization Demo Complete! ===");
