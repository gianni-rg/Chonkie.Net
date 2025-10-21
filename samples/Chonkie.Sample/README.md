# Chonkie.Net Sample Application

This sample application demonstrates the key features of the Chonkie.Net library for text chunking.

## What it demonstrates

The sample includes examples of:

1. **Token Chunker** - Splits text into chunks based on token count
2. **Sentence Chunker** - Splits text based on sentence boundaries
3. **Recursive Chunker** - Uses hierarchical splitting with multiple separators
4. **Semantic Chunker** - Groups semantically similar sentences (requires embeddings configuration)

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

## Customizing the Sample

You can modify the `Program.cs` file to:
- Use your own text
- Adjust chunker parameters (chunk size, overlap, etc.)
- Configure the Semantic Chunker with your embeddings provider API key

### Enabling Semantic Chunker

To test the Semantic Chunker, uncomment the code in `DemoSemanticChunker` and provide your API key:

```csharp
var embeddings = new OpenAIEmbeddings(apiKey: "your-api-key-here");
```

Supported embedding providers:
- OpenAI
- Azure OpenAI
- Cohere
- Google Gemini
- Jina AI
- Voyage AI
- Sentence Transformers (local ONNX models)

## Learn More

For more information about Chonkie.Net, see the main [README](../../README.md).
