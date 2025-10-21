# Chonkie.Net Chunk Visualizer Sample

This sample demonstrates visualization utilities for displaying chunks in the console with color-coding and formatting - similar to Python's `Visualizer` class.

## Features

The `ChunkVisualizer` utility provides several visualization methods:

### 1. **Color-Coded Output**
Displays chunks with alternating colors for easy distinction:

```csharp
ChunkVisualizer.Print(chunks, showOverlap: true);
```

### 2. **Detailed Metadata**
Shows token counts and character ranges:

```csharp
ChunkVisualizer.Print(chunks, showMetadata: true);
```

### 3. **Boundary Markers**
Visual markers showing chunk boundaries:

```csharp
ChunkVisualizer.Print(chunks, showBoundaries: true);
```

### 4. **Chunker Comparison**
Side-by-side comparison of different chunking strategies:

```csharp
ChunkVisualizer.Compare(new Dictionary<string, IReadOnlyList<Chunk>>
{
    ["Token"] = tokenChunks,
    ["Sentence"] = sentenceChunks,
    ["Recursive"] = recursiveChunks
});
```

### 5. **Bar Chart**
Visual representation of chunk sizes:

```csharp
ChunkVisualizer.PrintBarChart(chunks);
```

### 6. **Highlighted Text**
Shows full text with chunk boundaries highlighted:

```csharp
ChunkVisualizer.PrintHighlighted(chunks, originalText);
```

### 7. **HTML Export**
Saves chunks to an HTML file for browser viewing:

```csharp
ChunkVisualizer.SaveHtml("chunks.html", chunks, "My Chunks");
```

## Running the Sample

From the solution root:

```bash
dotnet run --project samples/Chonkie.Visualizer.Sample/Chonkie.Visualizer.Sample.csproj
```

Or from the sample directory:

```bash
cd samples/Chonkie.Visualizer.Sample
dotnet run
```

## Sample Output

### Demo 1: Token Chunker with Overlap Highlighting
```
[Chunk 1]
Artificial Intelligence has revolutionized technology in recent years.
Machine learning models can now understand and generate human language...
  ↓ Overlap: 39 characters

[Chunk 2]
generate human language with remarkable accuracy.
Deep learning architectures, particularly transformers...
```

### Demo 2: With Metadata
```
[Chunk 1 | Tokens: 22 | Range: 0-170]
Artificial Intelligence has revolutionized technology in recent years.
Machine learning models can now understand and generate human language...
```

### Demo 3: With Boundaries
```
[Chunk 1]
┌─ Artificial Intelligence has revolutionized technology...
└─

[Chunk 2]
┌─ Deep learning architectures, particularly transformers...
└─
```

### Demo 4: Comparison Table
```
┌─ Chunker Comparison ─────────────────────────────────┐
│ Strategy          │ Chunks │ Total Tokens │ Avg/Chunk │
├───────────────────┼────────┼──────────────┼───────────┤
│ Token (20)        │      3 │           56 │      18.7 │
│ Sentence (25)     │      3 │           48 │      16.0 │
│ Recursive (30)    │      2 │           49 │      24.5 │
└───────────────────┴────────┴──────────────┴───────────┘
```

## Use Cases

### Testing Chunker Settings
Quickly visualize the effects of different chunk sizes and overlap settings:

```csharp
var small = new TokenChunker(tokenizer, chunkSize: 50);
var large = new TokenChunker(tokenizer, chunkSize: 200);

ChunkVisualizer.Compare(new Dictionary<string, IReadOnlyList<Chunk>>
{
    ["Small (50)"] = small.Chunk(text),
    ["Large (200)"] = large.Chunk(text)
});
```

### Debugging Chunking Issues
See exactly how text is being split:

```csharp
var chunks = chunker.Chunk(problematicText);
ChunkVisualizer.Print(chunks, showOverlap: true, showMetadata: true);
```

### Documentation & Reports
Generate HTML visualizations for sharing:

```csharp
var chunks = chunker.Chunk(document);
ChunkVisualizer.SaveHtml("report.html", chunks, "Document Analysis");
```

### Comparing Strategies
Evaluate different chunking approaches:

```csharp
var strategies = new Dictionary<string, IReadOnlyList<Chunk>>
{
    ["Token-based"] = new TokenChunker(tokenizer, 100).Chunk(text),
    ["Sentence-based"] = new SentenceChunker(tokenizer, 100).Chunk(text),
    ["Recursive"] = new RecursiveChunker(tokenizer, 100).Chunk(text)
};

ChunkVisualizer.Compare(strategies);
```

## Visualization Options

### Print() Options

| Parameter | Description | Default |
|-----------|-------------|---------|
| `showOverlap` | Highlight overlapping regions between chunks | `false` |
| `showMetadata` | Display token counts and character ranges | `false` |
| `showBoundaries` | Show visual boundaries around chunks | `false` |

### Color Scheme

The visualizer uses a rotating palette of colors:
- Cyan
- Green
- Yellow
- Magenta
- Blue  
- Red

Colors automatically cycle for multi-chunk displays.

## HTML Export Format

The `SaveHtml()` method generates a styled HTML page with:
- Responsive design
- Color-coded chunk borders
- Metadata badges
- Summary statistics
- Professional formatting

Perfect for:
- Sharing results with stakeholders
- Documentation
- Blog posts or tutorials
- Archiving analysis results

## Comparison with Python

This .NET implementation provides similar functionality to Python's Visualizer:

**Python:**
```python
from chonkie import Visualizer

viz = Visualizer()
viz.print(chunks)
viz.save("output.html", chunks)
```

**.NET:**
```csharp
using Chonkie.Visualizer.Sample;

ChunkVisualizer.Print(chunks);
ChunkVisualizer.SaveHtml("output.html", chunks);
```

## Customization

The `ChunkVisualizer` class is a static utility that can be easily extended:

```csharp
// Add your own visualization methods
public static class MyVisualizer
{
    public static void PrintCustom(IReadOnlyList<Chunk> chunks)
    {
        // Your custom visualization logic
    }
}
```

## Best Practices

### Use Color for Distinction
```csharp
// Color helps distinguish adjacent chunks
ChunkVisualizer.Print(chunks, showBoundaries: true);
```

### Show Overlap for Token Chunking
```csharp
// Overlap visualization is crucial for understanding token chunking
var tokenChunks = new TokenChunker(tokenizer, 100, overlap: 20).Chunk(text);
ChunkVisualizer.Print(tokenChunks, showOverlap: true);
```

### Use Metadata for Analysis
```csharp
// Metadata helps verify chunking parameters are working correctly
ChunkVisualizer.Print(chunks, showMetadata: true);
```

### Export HTML for Documentation
```csharp
// HTML format is great for sharing and archiving
ChunkVisualizer.SaveHtml("analysis.html", chunks, "Text Analysis Report");
Console.WriteLine("Open analysis.html in your browser to view results");
```

## Learn More

- **Core Chunking**: See [Chonkie.Sample](../Chonkie.Sample/) for basic chunking examples
- **Pipeline API**: See [Chonkie.Pipeline.Sample](../Chonkie.Pipeline.Sample/) for fluent pipelines
- **Infrastructure**: See [Chonkie.Infrastructure.Sample](../Chonkie.Infrastructure.Sample/) for complete workflows
- **Python Visualizer**: See [Python Visualizer Docs](https://docs.chonkie.ai/oss/utils/visualizer) for original implementation
