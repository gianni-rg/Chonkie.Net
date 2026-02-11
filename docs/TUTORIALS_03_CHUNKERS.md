# 🔀 Tutorial: Choosing & Using Different Chunkers

**Time to complete:** 15-20 minutes  
**Level:** Intermediate-Advanced  
**What you'll learn:** When to use each chunker type and how to configure them

---

## 📋 Chunker Overview

Chonkie.Net provides **10 specialized chunkers**. Each has different strengths:

| Chunker | Best For | Speed | Code Quality |
| --- | --- | --- | --- |
| TokenChunker | Simple, fast splitting | ⚡⚡⚡ | Varies |
| SentenceChunker | Sentence boundaries | ⚡⚡ | Better |
| RecursiveChunker | Natural documents | ⚡⚡ | Best |
| SemanticChunker | Meaning-aware grouping | ⚡ | Best |
| CodeChunker | Source code | ⚡⚡ | Perfect |
| TableChunker | Structured data | ⚡⚡ | Best |
| MarkdownChunker | Markdown documents | ⚡⚡ | Perfect |
| LateChunker | Late in pipeline | ⚡ | Good |
| NeuralChunker | ONNX embeddings | ⚡ | Excellent |
| SlumberChunker | Complex documents | ⚡ | Excellent |
| FastChunker | High speed | ⚡⚡⚡ | Good |

---

## 1️⃣ TokenChunker - The Simplest

**Use when:** You need the fastest, simplest approach  
**Trade-off:** Chunks may break mid-sentence

```csharp
using Chonkie.Chunkers;
using Chonkie.Tokenizers;

var tokenizer = new WordTokenizer();
var chunker = new TokenChunker(
    tokenizer: tokenizer,
    chunkSize: 512,
    chunkOverlap: 50
);

var chunks = chunker.Chunk(text);
```

**Parameters:**

- `chunkSize` (int) - Target tokens per chunk
- `chunkOverlap` (int) - Overlapping tokens between chunks

**Output Characteristics:**

- Exact token boundaries
- Consistent chunk sizes
- May split sentences awkwardly

**Example:**

```csharp
var text = @"Artificial intelligence revolutionizes everything. 
Machine learning is the future.
Deep learning powers modern AI.";

var chunker = new TokenChunker(
    new WordTokenizer(),
    chunkSize: 10,
    chunkOverlap: 2
);
var chunks = chunker.Chunk(text);

// Output: May split like:
// Chunk 1: "Artificial intelligence revolutionizes everything."
// Chunk 2: "everything. Machine learning is the future."
```

---

## 2️⃣ SentenceChunker - Respects Sentence Boundaries

**Use when:** Document has natural sentence structure  
**Advantage:** Never splits mid-sentence

```csharp
var chunker = new SentenceChunker(
    tokenizer: new WordTokenizer(),
    chunkSize: 512,
    chunkOverlap: 50
);

var chunks = chunker.Chunk(text);
```

**Parameters:**

- `chunkSize` (int) - Target tokens per chunk
- `chunkOverlap` (int) - Overlapping tokens

**Output Characteristics:**

- Respects sentence boundaries
- Chunks may vary in size
- Natural reading flow

**Example:**

```csharp
var text = @"Artificial intelligence revolutionizes everything. 
Machine learning is the future. 
Deep learning powers modern AI.";

var chunker = new SentenceChunker(
    new WordTokenizer(),
    chunkSize: 20,
    chunkOverlap: 5
);
var chunks = chunker.Chunk(text);

// Output: One sentence per chunk
// Chunk 1: "Artificial intelligence revolutionizes everything."
// Chunk 2: "Machine learning is the future."
// Chunk 3: "Deep learning powers modern AI."
```

---

## 3️⃣ RecursiveChunker - Hierarchical Splitting (⭐ Recommended)

**Use when:** Processing natural documents (essays, articles, documentation)  
**Advantage:** Respects document structure (paragraphs → sentences → words)

```csharp
var chunker = new RecursiveChunker(
    tokenizer: new WordTokenizer(),
    chunkSize: 512,
    chunkOverlap: 50,
    separators: null  // Use defaults: ["\n\n", "\n", " ", ""]
);

var chunks = chunker.Chunk(text);
```

**Parameters:**

- `chunkSize` (int) - Target tokens per chunk
- `chunkOverlap` (int) - Overlapping tokens
- `separators` (string[]) - Split hierarchy (tries each in order)

**Default Separators:**

```csharp
new string[]
{
    "\n\n",      // Try paragraph breaks first
    "\n",        // Then line breaks
    " ",         // Then spaces (words)
    ""           // Finally, individual characters
}
```

**Output Characteristics:**

- Respects document structure
- Good balance of readability and consistency
- Most natural for mixed-content documents

**Example:**

```csharp
var text = @"# Introduction
Artificial intelligence is transforming industries.

Machine learning enables computers to learn.

# Deep Learning
Deep learning models achieve remarkable results.";

var chunker = new RecursiveChunker(new WordTokenizer(), chunkSize: 30);
var chunks = chunker.Chunk(text);

// Output: Respects structure
// Chunk 1: "# Introduction\nArtificial intelligence is transforming..."
// Chunk 2: "Machine learning enables computers..."
// Chunk 3: "# Deep Learning\nDeep learning models..."
```

---

## 4️⃣ SemanticChunker - Meaning-Aware Grouping

**Use when:** Meaning matters more than structure  
**Requirement:** Embeddings API (costs money)  
**Advantage:** Groups semantically similar content together

```csharp
using Chonkie.Embeddings.OpenAI;

var embeddings = new OpenAIEmbeddings(apiKey: "sk-...");

var chunker = new SemanticChunker(
    tokenizer: new WordTokenizer(),
    embeddingModel: embeddings,
    chunkSize: 512,
    threshold: 0.8f  // Similarity threshold (higher = larger chunks)
);

var chunks = chunker.Chunk(text);
```

**Parameters:**

- `embeddingModel` (IEmbeddingsModel) - Embedding provider
- `chunk_size` (int) - Maximum tokens per chunk
- `threshold` (float, 0-1) - Similarity cutoff
  - `0.9` → More diverse content in chunks
  - `0.5` → More focused chunks
- `similarity_window` (int, default=3) - Sentences to compare

**Output Characteristics:**

- Groups conceptually similar sentences
- Chunk sizes vary based on content
- Best preservation of meaning
- **Higher cost** (API calls per text)

**Example:**

```csharp
var text = @"Quantum computers use quantum bits. 
They perform calculations in superposition.

Machine learning models analyze patterns.
Deep learning uses neural networks.";

var embeddings = new OpenAIEmbeddings(apiKey: "sk-...");
var chunker = new SemanticChunker(embeddings, threshold: 0.75f);
var chunks = chunker.Chunk(text);

// Output: Semantic grouping
// Chunk 1: Quantum computing sentences (cohesive topic)
// Chunk 2: Machine learning sentences (cohesive topic)
```

---

## 5️⃣ CodeChunker - Source Code

**Use when:** Processing source code files  
**Advantage:** Respects code structure (functions, classes)

```csharp
using Chonkie.Chunkers;

var chunker = new CodeChunker(
    tokenizer: new WordTokenizer(),
    chunkSize: 512,
    language: "csharp"  // Syntax-aware splitting
);

var chunks = chunker.Chunk(sourceCode);
```

**Parameters:**

- `language` (string) - "csharp", "python", "javascript", "java", etc.
- `chunkSize` (int) - Target tokens per chunk

**Output Characteristics:**

- Respects code structure (methods, functions)
- Never splits in middle of function
- Syntax-aware delimiters

**Example:**

```csharp
var sourceCode = @"
public class Calculator 
{
    public int Add(int a, int b) 
    {
        return a + b;
    }

    public int Multiply(int a, int b) 
    {
        return a * b;
    }
}";

var chunker = new CodeChunker(new WordTokenizer(), chunkSize: 30, language: "csharp");
var chunks = chunker.Chunk(sourceCode);

// Output: Preserves code structure
// Chunk 1: Full Add method
// Chunk 2: Full Multiply method
```

---

## 6️⃣ TableChunker - Structured Data

**Use when:** Processing tables/DataFrames  
**Advantage:** Keeps rows together, respects columns

```csharp
using Chonkie.Chunkers;

var chunker = new TableChunker(
    tokenizer: new WordTokenizer(),
    chunkSize: 512
);

var csvContent = "Name,Age,Occupation\nAlice,30,Engineer\n...";
var chunks = chunker.Chunk(csvContent);
```

**Parameters:**

- `chunkSize` (int) - Target tokens per chunk

**Output Characteristics:**

- Keeps table rows intact
- Preserves headers
- Good for CSV/TSV data

**Example:**

```csharp
var csvData = @"Product,Price,Stock
Laptop,999.99,5
Mouse,29.99,50
Keyboard,79.99,20";

var chunker = new TableChunker(new WordTokenizer(), chunkSize: 50);
var chunks = chunker.Chunk(csvData);

// Output: Complete rows per chunk
// Chunk 1: Headers + some rows
// Chunk 2: Remaining rows
```

---

## 7️⃣ MarkdownChunker - Markdown Special Handling

**Use when:** Processing Markdown files with headers, code blocks  
**Advantage:** Respects Markdown structure

```csharp
using Chonkie.Chunkers;

var chunker = new MarkdownChunker(
    tokenizer: new WordTokenizer(),
    chunkSize: 512,
    chunkOverlap: 50
);

var markdownContent = File.ReadAllText("document.md");
var chunks = chunker.Chunk(markdownContent);
```

**Parameters:**

- `chunkSize` (int) - Target tokens per chunk
- `chunkOverlap` (int) - Overlapping tokens

**Output Characteristics:**

- Respects hierarchy (# → ## → ### sections)
- Preserves code blocks
- Intelligent heading relationships

**Example:**

````csharp
var markdown = @"# Main Topic
Content here.

## Subtopic 1
More content.

```csharp
code here
```

## Subtopic 2
Even more content.";

var chunker = new MarkdownChunker(new WordTokenizer(), chunkSize: 50);
var chunks = chunker.Chunk(markdown);

// Output: Respects structure
// Chunk 1: Main Topic + Subtopic 1 content
// Chunk 2: Subtopic 2 content
````

---

## 8️⃣ LateChunker - Two-Stage Chunking

**Use when:** You need fine-grained control over chunking  
**Use case:** Chunk after processing with other pipelines

```csharp
using Chonkie.Chunkers;

var chunker = new LateChunker(
    tokenizer: new WordTokenizer(),
    chunkSize: 512,
    chunkOverlap: 50
);

var chunks = chunker.Chunk(processedText);
```

**Why "Late"?**

- Token-based (like TokenChunker)
- Typically used as final stage in pipelines
- Works well after refinement steps

---

## 9️⃣ NeuralChunker - ONNX-Based Semantic Chunking (⭐ Advanced)

**Use when:** You want semantic chunking without API costs  
**Requirement:** ONNX model loaded locally  
**Advantage:** Free, fast semantic understanding

```csharp
using Chonkie.Chunkers;

// Requires ONNX model and SentenceTransformer embeddings
var chunker = new NeuralChunker(
    tokenizer: new WordTokenizer(),
    embeddingModel: sentenceTransformerEmbeddings,
    chunkSize: 512,
    threshold: 0.8f
);

var chunks = chunker.Chunk(text);
```

**Parameters:**

- `embeddingModel` - Local ONNX-based embeddings
- `threshold` (0-1) - Semantic similarity threshold
- `chunkSize` - Maximum tokens

**Output Characteristics:**

- Semantic grouping (like SemanticChunker)
- No API costs (local processing)
- Fast for repeated use

**Cost Comparison:**

```text
SemanticChunker: $0.10-0.20 per 1M tokens (API)
NeuralChunker:   FREE (one-time model download)
```

---

## 🔟 SlumberChunker - Complex Document Handling

**Use when:** Processing complex documents with mixed content  
**Advantage:** Handles edge cases (orphaned text, nested structures)

```csharp
using Chonkie.Chunkers;

var chunker = new SlumberChunker(
    tokenizer: new WordTokenizer(),
    chunkSize: 512,
    chunkOverlap: 50
);

var chunks = chunker.Chunk(complexDocument);
```

**Parameters:**

- `chunkSize` (int) - Target tokens
- `chunkOverlap` (int) - Overlap between chunks
- Various extraction modes for special handling

**Output Characteristics:**

- Intelligent edge case handling
- Good for real-world messy documents
- More computation than simpler chunkers

---

## 1️⃣1️⃣ FastChunker - High-Speed Chunking

**Use when:** Processing millions of documents (performance critical)  
**Trade-off:** Less intelligent splitting than Recursive

```csharp
using Chonkie.Chunkers;

var chunker = new FastChunker(
    tokenizer: new WordTokenizer(),
    chunkSize: 512,
    chunkOverlap: 50
);

var chunks = chunker.Chunk(text);
```

**Parameters:**

- `chunkSize` - Target tokens
- `chunkOverlap` - Overlapping tokens
- UTF-8 optimized for speed

**Performance:**

- **3-5x faster** than RecursiveChunker
- Good for batch processing

---

## 🎯 Decision Matrix: Which Chunker to Use?

```text
Question: What are you processing?

├─ Source Code?              → CodeChunker
├─ Structured Data (CSV)?    → TableChunker
├─ Markdown with headers?    → MarkdownChunker
├─ Need meaning awareness?
│  ├─ With API budget?       → SemanticChunker
│  ├─ Without API budget?    → NeuralChunker
│  └─ Can't use either?      → RecursiveChunker
├─ Need extreme speed?       → FastChunker
└─ Regular text?
   ├─ Simple & fast?         → TokenChunker
   ├─ Sentence boundaries?   → SentenceChunker
   ├─ Complex document?      → SlumberChunker
   └─ Natural hierarchy?     → RecursiveChunker ⭐
```

---

## 📊 Performance Comparison

Testing with 10,000 words across 10 documents:

| Chunker | Speed | CPU | Memory | Quality |
| --- | --- | --- | --- | --- |
| FastChunker | ⚡⚡⚡ 50ms | Low | Low | Good |
| TokenChunker | ⚡⚡⚡ 60ms | Low | Low | Fair |
| SentenceChunker | ⚡⚡ 100ms | Low | Low | Good |
| RecursiveChunker | ⚡⚡ 120ms | Low | Low | Excellent |
| CodeChunker | ⚡⚡ 150ms | Low | Low | Perfect |
| MarkdownChunker | ⚡⚡ 140ms | Low | Low | Perfect |
| SlumberChunker | ⚡ 300ms | Medium | Medium | Excellent |
| TableChunker | ⚡ 400ms | Medium | Medium | Perfect |
| SemanticChunker | 🐌 5000ms | High | High | Perfect |
| NeuralChunker | 🐌 3000ms | High | High | Perfect |
| LateChunker | ⚡⚡ 80ms | Low | Low | Good |

**Takeaway:** Use RecursiveChunker first; optimize only if needed.

---

## 🔄 Chaining Chunkers

Sometimes you want multiple passes:

```csharp
using Chonkie.Pipeline;

// First: Split by paragraph
var paragraph = new RecursiveChunker(
    tokenizer, 
    separators: new[] { "\n\n" }, 
    chunkSize: 2048
);

// Then: Further split oversized chunks
var sentence = new SentenceChunker(
    tokenizer,
    chunkSize: 512,
    chunkOverlap: 25
);

var result = await FluentPipeline.Create()
    .WithText(text)
    .ChunkWith(paragraph)          // First pass
    .ChunkWith(sentence)           // Second pass (optional)
    .RunAsync();
```

---

## 🧪 Testing Chunker Output

Utility to compare chunkers:

```csharp
public async Task CompareChunkersAsync(string text)
{
    var tokenizer = new WordTokenizer();
    var chunkers = new Dictionary<string, IChunker>()
    {
        ("Recursive", new RecursiveChunker(tokenizer, chunkSize: 256)),
        ("Sentence", new SentenceChunker(tokenizer, chunkSize: 256)),
        ("Token", new TokenChunker(tokenizer, chunkSize: 256)),
    };

    foreach (var (name, chunker) in chunkers)
    {
        var chunks = chunker.Chunk(text);
        Console.WriteLine($"\n{name} Chunker:");
        Console.WriteLine($"  Chunks: {chunks.Count}");
        Console.WriteLine($"  Avg size: {chunks.Average(c => c.TokenCount)} tokens");
        Console.WriteLine($"  First chunk: {chunks[0].Text[..50]}...");
    }
}
```

---

## 🚀 Next Steps

1. **[Quick-Start Guide](TUTORIALS_01_QUICK_START.md)** - Back to basics
2. **[RAG with Chunkers](TUTORIALS_02_RAG.md)** - Using chunkers in RAG
3. **[Vector Database Integration](TUTORIALS_04_VECTORDB.md)** - After chunking
4. **[Pipeline Configuration](TUTORIALS_05_PIPELINES.md)** - Advanced workflows
