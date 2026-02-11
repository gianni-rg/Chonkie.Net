# 🚀 Quick Start Guide - Chonkie.Net

**Time to complete:** 5-10 minutes  
**What you'll learn:** Basic chunking with Chonkie.Net in 3 steps

---

## 📦 Installation

### Option 1: From NuGet (Recommended)

```powershell
dotnet add package Chonkie.Core
dotnet add package Chonkie.Tokenizers
```

### Option 2: Build from Source

```powershell
git clone https://github.com/gianni-rg/Chonkie.Net.git
cd Chonkie.Net
dotnet build
```

---

## ⚡ CHONK! - Your First Chunk (30 seconds)

The simplest possible example - create a chunker and split text:

```csharp
using Chonkie.Chunkers;
using Chonkie.Tokenizers;

// 1. Choose a tokenizer
var tokenizer = new WordTokenizer();

// 2. Choose a chunker
var chunker = new TokenChunker(
    tokenizer: tokenizer,
    chunkSize: 100,
    chunkOverlap: 10
);

// 3. Chunk your text
var text = "Chonkie is the no-nonsense chunking library for RAG." +
           " It's lightweight, fast, and incredibly easy to use!";

var chunks = chunker.Chunk(text);

// 4. Process the chunks
foreach (var chunk in chunks)
{
    Console.WriteLine($"Chunk: {chunk.Text}");
    Console.WriteLine($"Tokens: {chunk.TokenCount}\n");
}
```

**Output:**

```text
Chunk: Chonkie is the no-nonsense chunking library for RAG.
It's lightweight, fast, and incredibly easy to use!
Tokens: 24
```

---

## 📚 Understanding the Basics

### What is a Chunk?

Every chunk is a `Chunk` object with metadata:

```csharp
var chunk = chunks[0];

// Available properties
Console.WriteLine(chunk.Text);          // The actual content
Console.WriteLine(chunk.TokenCount);    // Number of tokens
Console.WriteLine(chunk.StartIndex);    // Position in original text
Console.WriteLine(chunk.EndIndex);      // Position in original text
```

### Tokenizers Explained

A **tokenizer** breaks text into tokens. Different tokenizers exist
for different needs:

| Tokenizer             | Use Case             | Example     |
|-----------------------|----------------------|-------------|
| `WordTokenizer`       | Simple, lightweight  | Prototyping |
| `CharacterTokenizer`  | Character control    | Analysis    |
| `SharpTokenTokenizer` | GPT-2/OpenAI         | Production  |
| `HuggingFaceTokenizer`| Custom models        | Advanced    |

```csharp
// Example: Different tokenizers
var wordTokenizer = new WordTokenizer();
var charTokenizer = new CharacterTokenizer();
var gpt2Tokenizer = new SharpTokenTokenizer();

var text = "Hello, world!";

Console.WriteLine($"Word tokens: {wordTokenizer.Tokenize(text).Count}");   // 3
Console.WriteLine($"Char tokens: {charTokenizer.Tokenize(text).Count}");   // 13
Console.WriteLine($"GPT2 tokens: {gpt2Tokenizer.Tokenize(text).Count}");   // 4
```

---

## 🔀 Different Chunker Types

Chonkie.Net provides **10 built-in chunkers** for different scenarios.
Here are the main ones:

### 1. TokenChunker - Fixed Token Size (Simplest)

Splits text into chunks of fixed token count.

```csharp
var chunker = new TokenChunker(
    tokenizer: new WordTokenizer(),
    chunkSize: 512,
    chunkOverlap: 50
);

var chunks = chunker.Chunk(text);
```

**Best for:** Simple splitting, fast processing

---

### 2. SentenceChunker - Sentence Boundaries

Respects sentence boundaries while staying within token limits.

```csharp
var chunker = new SentenceChunker(
    tokenizer: new WordTokenizer(),
    chunkSize: 512,
    chunkOverlap: 50
);

var chunks = chunker.Chunk(text);
```

**Best for:** Natural breaking points, documents with varied sentence
lengths

---

### 3. RecursiveChunker - Hierarchical Splits

Recursively splits by multiple delimiters: paragraphs → sentences → words.

```csharp
var chunker = new RecursiveChunker(
    tokenizer: new WordTokenizer(),
    chunkSize: 512,
    chunkOverlap: 50
);

var chunks = chunker.Chunk(text);
```

**Best for:** Natural documents (markdown, HTML, structured text)

---

### 4. SemanticChunker - Meaning-Aware Grouping

Groups semantically similar sentences together using embeddings.

```csharp
using Chonkie.Embeddings.Azure;

var embeddings = new AzureOpenAIEmbeddings(
    endpoint: "https://your-instance.openai.azure.com/",
    apiKey: "your-key",
    deploymentName: "text-embedding-3-large",
    dimension: 3072
);

var chunker = new SemanticChunker(
    tokenizer: new WordTokenizer(),
    embeddingModel: embeddings,
    chunkSize: 512,
    threshold: 0.8f
);

var chunks = chunker.Chunk(text);
```

**Best for:** Complex documents where meaning matters more than structure

---

## 🔗 Pipelines - Combine Multiple Steps

For more complex workflows, use **Pipelines** to chain operations:

```csharp
using Chonkie.Pipeline;
using Chonkie.Refineries;
using Chonkie.Tokenizers;

var tokenizer = new WordTokenizer();

var result = await FluentPipeline.Create()
    .WithText("Your document text here...")
    .ChunkWith(new RecursiveChunker(tokenizer, chunkSize: 512))
    .RefineWith(new OverlapRefinery(minOverlap: 8))  // Optional: post-process
    .RunAsync();

// result.Chunks contains the final chunks
foreach (var chunk in result.Chunks)
{
    Console.WriteLine(chunk.Text);
}
```

**Pipeline steps (in order):**

1. **Chunker** - Split text into chunks
2. **Refinery** (optional) - Post-process and enhance chunks
3. **Porter/Handshake** (optional) - Export or store chunks

---

## 🎯 Real-World Example: RAG Document Processing

Here's a complete example for building a Retrieval-Augmented Generation (RAG) pipeline:

```csharp
using Chonkie.Chunkers;
using Chonkie.Embeddings.OpenAI;
using Chonkie.Pipeline;
using Chonkie.Tokenizers;

// 1. Load your document
string documentPath = "my-document.txt";
string documentText = File.ReadAllText(documentPath);

// 2. Set up components
var tokenizer = new WordTokenizer();
var embeddings = new OpenAIEmbeddings(
    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!
);

// 3. Create and run pipeline
var result = await FluentPipeline.Create()
    .WithText(documentText)
    .ChunkWith(new RecursiveChunker(tokenizer, chunkSize: 512, chunkOverlap: 50))
    .RunAsync();

// 4. Generate embeddings for each chunk
var chunks = result.Chunks;
var embeddings_list = await embeddings.EmbedAsync(
    chunks.Select(c => c.Text).ToList()
);

// 5. Store in vector database (example structure)
for (int i = 0; i < chunks.Count; i++)
{
    var chunk = chunks[i];
    var embedding = embeddings_list[i];
    
    // TODO: Store in your vector database
    Console.WriteLine($"Chunk {i}: {chunk.Text[..50]}...");
    Console.WriteLine($"Embedding dimensions: {embedding.Length}");
}
```

---

## 🔍 Common Parameters Explained

When creating chunkers, you'll see these parameters frequently:

### `chunkSize` (int)

Maximum **tokens** per chunk. Larger = fewer chunks, potentially losing detail.

```csharp
// Small chunks (detailed, more API calls)
var chunker1 = new TokenChunker(tokenizer, chunkSize: 128);

// Medium chunks (balanced)
var chunker2 = new TokenChunker(tokenizer, chunkSize: 512);

// Large chunks (high-level, fewer chunks)
var chunker3 = new TokenChunker(tokenizer, chunkSize: 2048);
```

### `chunkOverlap` (int)

Number of overlapping tokens between consecutive chunks for context preservation.

```csharp
// No overlap (smallest output size)
var chunker1 = new TokenChunker(tokenizer, chunkSize: 512, chunkOverlap: 0);

// Small overlap (some continuity)
var chunker2 = new TokenChunker(tokenizer, chunkSize: 512, chunkOverlap: 50);

// Large overlap (maximum continuity, larger output size)
var chunker3 = new TokenChunker(tokenizer, chunkSize: 512, chunkOverlap: 150);
```

### For Semantic Chunker: `threshold` (float, 0-1)

Higher threshold = larger chunks (more diverse content in one chunk)  
Lower threshold = smaller chunks (more similar content grouped)

```csharp
var chunker1 = new SemanticChunker(
    tokenizer, embeddings, threshold: 0.9f);  // Large chunks
var chunker2 = new SemanticChunker(
    tokenizer, embeddings, threshold: 0.5f);  // Small chunks
```

---

## ✅ Next Steps

You now know:

- ✅ How to install Chonkie.Net
- ✅ How to chunk text with your first chunker
- ✅ The differences between chunker types
- ✅ How to use pipelines for complex workflows
- ✅ A complete RAG example

### Where to Go Next

1. **[RAG Tutorial](TUTORIALS_02_RAG.md)** - Build a complete RAG system
step-by-step
2. **[Chunkers Tutorial](TUTORIALS_03_CHUNKERS.md)** - Deep dive into each
chunker type with examples
3. **[Vector Database Integration](TUTORIALS_04_VECTORDB.md)** - Store and
search embeddings
4. **[Pipeline Configuration](TUTORIALS_05_PIPELINES.md)** - Advanced pipeline patterns

---

## 🐛 Troubleshooting

### "TokenChunker requires a tokenizer"

```csharp
// ❌ Wrong - TokenChunker needs a tokenizer
var chunker = new TokenChunker(chunkSize: 512);

// ✅ Correct - Pass a tokenizer
var chunker = new TokenChunker(new WordTokenizer(), chunkSize: 512);
```

### "No chunks returned"

Check your `chunkSize` - if it's larger than your text tokenized length,
you'll get one chunk:

```csharp
var tokenizer = new WordTokenizer();
var text = "Short text";  // 2 tokens

// ❌ This returns 1 chunk (text is smaller than chunkSize)
var chunker = new TokenChunker(tokenizer, chunkSize: 1000);

// ✅ This returns 1 chunk correctly
var chunker = new TokenChunker(tokenizer, chunkSize: 10);
```

### Chunks are too large/small

Adjust `chunkSize` and `chunkOverlap`:

```csharp
// Chunks too large? Reduce chunkSize
var chunker = new TokenChunker(tokenizer, chunkSize: 128);

// Chunks feel disconnected? Increase overlap
var chunker = new TokenChunker(tokenizer, chunkSize: 512, chunkOverlap: 100);
```
