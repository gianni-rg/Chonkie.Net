# Chonkie.Net Sample Application

This sample application walks through the Quick Start and Chunkers tutorials using the .NET APIs.

## What it demonstrates

The sample includes examples of:

1. **Quick Start CHONK** - First chunk + metadata
2. **Tokenizers** - Word, character, and auto tokenizers
3. **Token Chunker** - Fixed token windows with overlap
4. **Sentence Chunker** - Sentence-aware chunking
5. **Recursive Chunker** - Hierarchical splitting
6. **Fast Chunker** - High-speed character chunking
7. **Code Chunker** - Code-aware chunking
8. **Table Chunker** - Markdown table chunking
9. **Markdown Processing** - MarkdownChef + recursive chunking
10. **Semantic Chunker** - Embeddings-driven semantic boundaries
11. **Late Chunker** - Late interaction embeddings
12. **Neural Chunker** - ONNX-backed chunking (fallback-aware)
13. **Slumber Chunker** - LLM-guided chunking (fallback-aware)
14. **MarkdownDocument** - Structured markdown metadata

## Running the sample

From the solution root directory:

```bash
dotnet run --project samples/Chonkie.Sample/Chonkie.Sample.csproj
```

Or from the sample directory:

```bash
cd samples/Chonkie.Sample
dotnet run
```

## Sample Output

The application will demonstrate each chunker type using a sample text about Artificial Intelligence, showing:
- The number of chunks created
- A preview of each chunk
- Token counts for each chunk

## Embeddings Configuration

Semantic, late, and vector-aware demos require embeddings. Set one of these options:

### Option 1: Azure OpenAI

```text
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_API_KEY=your-key
AZURE_OPENAI_EMBEDDINGS_DEPLOYMENT=text-embedding-3-small
```

### Option 2: OpenAI

```text
OPENAI_API_KEY=your-key
OPENAI_EMBEDDINGS_MODEL=text-embedding-3-small
OPENAI_EMBEDDINGS_DIMENSION=1536
```

### Option 3: Local Sentence Transformers (ONNX)

```text
CHONKIE_SENTENCE_TRANSFORMER_MODEL_PATH=./models/all-MiniLM-L6-v2
```

### Force Offline Demo Embeddings

```text
CHONKIE_SAMPLE_OFFLINE=1
```

### Optional: Neural Chunker ONNX Model

```text
CHONKIE_NEURAL_MODEL_PATH=./models/distilbert
```

## Learn More

For more information about Chonkie.Net, see the main [README](../../README.md).
