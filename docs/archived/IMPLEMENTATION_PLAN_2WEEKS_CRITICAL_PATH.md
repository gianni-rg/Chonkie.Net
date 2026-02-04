# Implementation Plan: Chonkie.Net Genies & Handshakes (2-3 Weeks Critical Path)

**Last Updated**: January 5, 2026  
**Total Duration**: 14 working days (2-3 weeks depending on team size)  
**Total Effort**: 100-150 hours (1-2 developers, 8 hours/day)  
**Strategy**: Leverage Microsoft.Extensions.AI + Semantic Kernel  
**Status**: Ready for execution

---

## Executive Summary

This plan accelerates delivery of Genies (4) and Handshakes (11) from 4-6 weeks to 2-3 weeks by leveraging:
- **Microsoft.Extensions.AI**: Pre-built `IChatClient` abstractions for OpenAI, Azure, Ollama
- **Semantic Kernel**: 43 packages with 10+ vector DB connectors (Qdrant, Chroma, Pinecone, Weaviate, etc.)

**Time Savings**:
- Genies: 2-3 weeks → 4-5 days (85% faster)
- Handshakes: 2-3 weeks → 1 week (70% faster)

**Deliverables**: 4 Genies + 11 Handshakes + Pipeline integration (all tested, documented, production-ready)

---

## Critical Path Timeline

### Week 1: Foundation & Genies (Days 1-5)

#### Day 1: Project Setup & FastChunker Scaffold
**Duration**: 8 hours  
**Team**: 1 developer (full day)

**Tasks**:
1. Create new projects:
   - `Chonkie.Genies` (class library)
   - `Chonkie.Handshakes` (class library)
   - `Chonkie.Genies.Tests` (xUnit)
   - `Chonkie.Handshakes.Tests` (xUnit)

2. Add NuGet dependencies:
   - `Microsoft.Extensions.AI` (1.0+)
   - `Microsoft.SemanticKernel` (1.68+)
   - `Microsoft.SemanticKernel.Connectors.Qdrant` (1.68+)
   - `SemanticKernel.Connectors.Chroma` (1.0+)
   - `Pinecone` (1.0+)
   - `Weaviate.Client` (latest)
   - NSubstitute for mocking

3. Create base interfaces:
   ```csharp
   // src/Chonkie.Genies/IGenie.cs
   public interface IGenie
   {
       Task<string> Generate(string prompt);
       Task<List<string>> GenerateBatch(List<string> prompts);
       Task<string> GenerateJson(string prompt, string schema);
       Task<List<string>> GenerateJsonBatch(List<string> prompts, string schema);
   }
   
   // src/Chonkie.Handshakes/IHandshake.cs
   public interface IHandshake
   {
       Task Write(List<ChunkEmbedding> chunks);
       Task<List<SearchResult>> Search(string query, float[] embedding, int limit = 10);
       Task DeleteCollection(string collectionName);
   }
   ```

4. Create FastChunker scaffold (minimal implementation):
   - Base class structure
   - Unit test template
   - Configuration class

**Success Criteria**:
- ✅ All 4 projects created and compile
- ✅ Base interfaces defined and documented
- ✅ Dependencies resolve correctly
- ✅ 2 unit tests written (setup templates)

**Deliverables**:
- 4 new projects in solution
- Base interfaces in place
- Test infrastructure ready

---

#### Days 2-3: Implement FastChunker & First Genie
**Duration**: 16 hours (2 × 8-hour days)  
**Team**: 1 developer

**Day 2 Tasks** (FastChunker Core):
1. Implement FastChunker algorithm:
   - Character-based sliding window (default: 512 chars)
   - Word boundary preservation
   - Configurable overlap (default: 10%)
   - Batch processing support
   - Async/await throughout

   ```csharp
   public class FastChunker : IChunker
   {
       private readonly int _chunkSize;
       private readonly int _overlapSize;
   
       public async Task<List<Chunk>> ChunkAsync(string text)
       {
           var chunks = new List<Chunk>();
           int position = 0;
   
           while (position < text.Length)
           {
               int endPosition = Math.Min(position + _chunkSize, text.Length);
               
               // Preserve word boundaries
               if (endPosition < text.Length)
               {
                   int lastSpace = text.LastIndexOf(' ', endPosition);
                   if (lastSpace > position + _chunkSize / 2)
                       endPosition = lastSpace;
               }
   
               chunks.Add(new Chunk { Text = text[position..endPosition] });
               position = endPosition - _overlapSize;
           }
   
           return chunks;
       }
   }
   ```

2. Write 8+ unit tests:
   - Empty string handling
   - Single word
   - Word boundary preservation
   - Overlap calculation
   - Batch processing
   - Large documents
   - Special characters

**Day 3 Tasks** (OpenAIGenie Implementation):
1. Implement OpenAIGenie using Microsoft.Extensions.AI:
   ```csharp
   public class OpenAIGenie : IGenie
   {
       private readonly IChatClient _chatClient;
       private readonly string _model;
   
       public OpenAIGenie(string apiKey, string model = "gpt-4-turbo")
       {
           var client = new ChatClientBuilder()
               .UseFunctionInvocation(DefaultFunctionInvokingChatClient.Instance)
               .UseLogging(loggerFactory)
               .Build(new OpenAIChatClient(apiKey, model));
           
           _chatClient = client;
           _model = model;
       }
   
       public async Task<string> Generate(string prompt)
       {
           var messages = new List<ChatMessage>
           {
               new UserChatMessage(prompt)
           };
           
           var response = await _chatClient.CompleteAsync(messages);
           return response.Content[0].Text;
       }
   
       public async Task<List<string>> GenerateBatch(List<string> prompts)
       {
           var tasks = prompts.Select(p => Generate(p));
           return new List<string>(await Task.WhenAll(tasks));
       }
   
       public async Task<string> GenerateJson(string prompt, string schema)
       {
           var systemPrompt = $"Respond with valid JSON matching this schema:\n{schema}";
           var messages = new List<ChatMessage>
           {
               new SystemChatMessage(systemPrompt),
               new UserChatMessage(prompt)
           };
           
           var response = await _chatClient.CompleteAsync(messages);
           return response.Content[0].Text;
       }
   
       public async Task<List<string>> GenerateJsonBatch(List<string> prompts, string schema)
       {
           var tasks = prompts.Select(p => GenerateJson(p, schema));
           return new List<string>(await Task.WhenAll(tasks));
       }
   }
   ```

2. Write 6+ unit tests:
   - Basic prompt completion
   - Batch processing
   - JSON schema validation
   - Error handling (invalid API key)
   - Model fallback
   - Token counting

**Success Criteria**:
- ✅ FastChunker passes all 8+ tests
- ✅ OpenAIGenie passes all 6+ tests
- ✅ 90%+ code coverage
- ✅ Async/await patterns used throughout
- ✅ Error handling in place

**Deliverables**:
- FastChunker implementation (150-200 LOC)
- OpenAIGenie implementation (200-250 LOC)
- 14+ unit tests
- Documentation for both classes

---

#### Days 4-5: Complete Remaining 3 Genies
**Duration**: 16 hours (2 × 8-hour days)  
**Team**: 1 developer

**Day 4 Tasks** (AzureOpenAIGenie + GeminiGenie):
1. Implement AzureOpenAIGenie:
   ```csharp
   public class AzureOpenAIGenie : IGenie
   {
       private readonly IChatClient _chatClient;
       private readonly string _model;
   
       public AzureOpenAIGenie(
           string endpoint, 
           string apiKey, 
           string deployment)
       {
           var client = new ChatClientBuilder()
               .UseLogging(loggerFactory)
               .Build(new AzureOpenAIChatClient(
                   deployment: deployment,
                   endpoint: new Uri(endpoint),
                   credential: new AzureKeyCredential(apiKey)));
           
           _chatClient = client;
           _model = deployment;
       }
       
       // Same interface implementation as OpenAIGenie
   }
   ```

2. Implement GeminiGenie:
   ```csharp
   public class GeminiGenie : IGenie
   {
       private readonly IChatClient _chatClient;
       private readonly string _model;
   
       public GeminiGenie(string apiKey, string model = "gemini-2.0-flash")
       {
           var client = new ChatClientBuilder()
               .UseLogging(loggerFactory)
               .Build(new OllamaChatClient(
                   endpoint: new Uri("https://generativelanguage.googleapis.com/"),
                   modelId: model));
           
           _chatClient = client;
           _model = model;
       }
       
       // Same interface implementation as OpenAIGenie
   }
   ```

3. Write 6+ tests for each (reuse from OpenAI tests)

**Day 5 Tasks** (LiteLLMGenie + Handshake Foundation):
1. Implement LiteLLMGenie (provider-agnostic via Semantic Kernel):
   ```csharp
   public class LiteLLMGenie : IGenie
   {
       private readonly IChatClient _chatClient;
       private readonly string _model;
   
       public LiteLLMGenie(string modelId, Dictionary<string, string> config)
       {
           // Use Semantic Kernel's flexible provider model
           var client = new ChatClientBuilder()
               .UseLogging(loggerFactory)
               .Build(new OllamaChatClient(
                   endpoint: new Uri(config["endpoint"]),
                   modelId: modelId));
           
           _chatClient = client;
           _model = modelId;
       }
   }
   ```

2. Create GenieFactory:
   ```csharp
   public class GenieFactory
   {
       public static IGenie CreateOpenAI(string apiKey, string model = "gpt-4-turbo")
           => new OpenAIGenie(apiKey, model);
   
       public static IGenie CreateAzure(string endpoint, string apiKey, string deployment)
           => new AzureOpenAIGenie(endpoint, apiKey, deployment);
   
       public static IGenie CreateGemini(string apiKey, string model = "gemini-2.0-flash")
           => new GeminiGenie(apiKey, model);
   
       public static IGenie CreateLiteLLM(string modelId, Dictionary<string, string> config)
           => new LiteLLMGenie(modelId, config);
   }
   ```

3. Create Handshake base infrastructure:
   - ChunkEmbedding DTO
   - SearchResult DTO
   - IHandshake interface (already done Day 1)
   - HandshakeFactory skeleton

4. Write integration tests for all 4 Genies:
   - Test with real API calls (mocked in CI)
   - Verify response formats
   - Check error handling

**Success Criteria**:
- ✅ All 4 Genies implemented (250 LOC each)
- ✅ GenieFactory created with all 4 methods
- ✅ 24+ unit/integration tests
- ✅ 90%+ code coverage
- ✅ All tests pass in CI

**Deliverables**:
- 4 complete Genie implementations
- GenieFactory with all methods
- 24+ tests
- Handshake foundation ready for Week 2

---

### Week 2: Handshakes & Pipeline Integration (Days 6-9)

#### Days 6-7: Implement 4 Primary Handshakes
**Duration**: 16 hours (2 × 8-hour days)  
**Team**: 1 developer

**Day 6 Tasks** (QdrantHandshake + ChromaHandshake):
1. Implement QdrantHandshake using Semantic Kernel:
   ```csharp
   public class QdrantHandshake : IHandshake
   {
       private readonly QdrantClient _client;
       private readonly string _collectionName;
   
       public QdrantHandshake(
           string endpoint, 
           int port, 
           string collectionName)
       {
           _client = new QdrantClient(endpoint, port);
           _collectionName = collectionName;
       }
   
       public async Task Write(List<ChunkEmbedding> chunks)
       {
           var points = chunks.Select((chunk, idx) => new PointStruct
           {
               Id = (ulong)idx,
               Vectors = chunk.Embedding,
               Payload = new Dictionary<string, ValueVariant>
               {
                   { "text", new ValueVariant { StringValue = chunk.Text } },
                   { "metadata", new ValueVariant { StringValue = chunk.Metadata } }
               }
           }).ToList();
   
           await _client.UpsertAsync(_collectionName, points);
       }
   
       public async Task<List<SearchResult>> Search(
           string query, 
           float[] embedding, 
           int limit = 10)
       {
           var results = await _client.SearchAsync(
               _collectionName,
               embedding,
               limit);
   
           return results.Select(r => new SearchResult
           {
               Text = r.Payload["text"].StringValue,
               Score = r.Score,
               Metadata = r.Payload["metadata"].StringValue
           }).ToList();
       }
   
       public async Task DeleteCollection(string collectionName)
       {
           await _client.DeleteCollectionAsync(collectionName);
       }
   }
   ```

2. Implement ChromaHandshake:
   ```csharp
   public class ChromaHandshake : IHandshake
   {
       private readonly HttpClient _httpClient;
       private readonly string _baseUrl;
       private readonly string _collectionName;
   
       public ChromaHandshake(string endpoint, string collectionName)
       {
           _baseUrl = endpoint;
           _collectionName = collectionName;
           _httpClient = new HttpClient();
       }
   
       public async Task Write(List<ChunkEmbedding> chunks)
       {
           var payload = new
           {
               documents = chunks.Select(c => c.Text).ToList(),
               embeddings = chunks.Select(c => c.Embedding).ToList(),
               metadatas = chunks.Select(c => new { metadata = c.Metadata }).ToList(),
               ids = chunks.Select((_, idx) => idx.ToString()).ToList()
           };
   
           var content = new StringContent(
               JsonSerializer.Serialize(payload),
               Encoding.UTF8,
               "application/json");
   
           await _httpClient.PostAsync(
               $"{_baseUrl}/api/v1/collections/{_collectionName}/add",
               content);
       }
   
       public async Task<List<SearchResult>> Search(
           string query, 
           float[] embedding, 
           int limit = 10)
       {
           var response = await _httpClient.PostAsync(
               $"{_baseUrl}/api/v1/collections/{_collectionName}/query",
               new StringContent(
                   JsonSerializer.Serialize(new { query_embeddings = new[] { embedding }, n_results = limit }),
                   Encoding.UTF8,
                   "application/json"));
   
           var json = await response.Content.ReadAsAsync<JsonElement>();
           var results = new List<SearchResult>();
           
           // Parse Chroma response format
           // ...
           
           return results;
       }
   
       public async Task DeleteCollection(string collectionName)
       {
           await _httpClient.DeleteAsync(
               $"{_baseUrl}/api/v1/collections/{collectionName}");
       }
   }
   ```

3. Write 8+ integration tests for each
4. Verify Docker containers (Qdrant, Chroma) work locally

**Day 7 Tasks** (PineconeHandshake + WeaviateHandshake):
1. Implement PineconeHandshake:
   ```csharp
   public class PineconeHandshake : IHandshake
   {
       private readonly PineconeClient _client;
       private readonly string _indexName;
   
       public PineconeHandshake(string apiKey, string indexName)
       {
           _client = new PineconeClient(new Pinecone.PineconeConfig(apiKey));
           _indexName = indexName;
       }
   
       public async Task Write(List<ChunkEmbedding> chunks)
       {
           var vectors = chunks.Select((chunk, idx) => new Vector
           {
               Id = idx.ToString(),
               Values = chunk.Embedding,
               Metadata = new Dictionary<string, object>
               {
                   { "text", chunk.Text },
                   { "metadata", chunk.Metadata }
               }
           }).ToList();
   
           var index = _client.Index(_indexName);
           await index.Upsert(vectors);
       }
   
       public async Task<List<SearchResult>> Search(
           string query, 
           float[] embedding, 
           int limit = 10)
       {
           var index = _client.Index(_indexName);
           var results = await index.Query(
               vector: embedding,
               topK: limit,
               includeMetadata: true);
   
           return results.Matches.Select(m => new SearchResult
           {
               Text = m.Metadata?["text"]?.ToString() ?? "",
               Score = m.Score,
               Metadata = m.Metadata?["metadata"]?.ToString() ?? ""
           }).ToList();
       }
   
       public async Task DeleteCollection(string collectionName)
       {
           // Pinecone uses namespace deletion
           var index = _client.Index(_indexName);
           await index.DeleteNamespace(collectionName);
       }
   }
   ```

2. Implement WeaviateHandshake:
   ```csharp
   public class WeaviateHandshake : IHandshake
   {
       private readonly WeaviateClient _client;
       private readonly string _className;
   
       public WeaviateHandshake(string endpoint, string className)
       {
           _client = new WeaviateClient(new Uri(endpoint));
           _className = className;
       }
   
       public async Task Write(List<ChunkEmbedding> chunks)
       {
           foreach (var chunk in chunks)
           {
               var obj = new WeaviateObject
               {
                   Class = _className,
                   Vector = chunk.Embedding,
                   Properties = new Dictionary<string, object>
                   {
                       { "text", chunk.Text },
                       { "metadata", chunk.Metadata }
                   }
               };
   
               await _client.Data.Create(obj);
           }
       }
   
       public async Task<List<SearchResult>> Search(
           string query, 
           float[] embedding, 
           int limit = 10)
       {
           var results = await _client.GraphQL
               .Raw($@"
                   {{
                       Get {{
                           {_className}(nearVector: {{vector: {JsonSerializer.Serialize(embedding)}}}, limit: {limit}}) {{
                               text
                               metadata
                               _additional {{
                                   distance
                               }}
                           }}
                       }}
                   }}
               ")
               .Do();
   
           // Parse results and convert to SearchResult
           return new List<SearchResult>();
       }
   
       public async Task DeleteCollection(string collectionName)
       {
           await _client.Schema.DeleteClass(_className);
       }
   }
   ```

3. Write 8+ integration tests for each
4. Verify Docker containers (Pinecone mock, Weaviate) work locally

**Success Criteria**:
- ✅ 4 Handshake implementations complete (250 LOC each)
- ✅ 32+ integration tests (all pass with Docker containers)
- ✅ 90%+ code coverage
- ✅ Docker Compose file updated with all services
- ✅ Integration test guide documented

**Deliverables**:
- 4 Handshake implementations
- 32+ integration tests
- Docker Compose with all 4 services
- Handshake documentation

---

#### Days 8-9: Pipeline Integration & Factory
**Duration**: 16 hours (2 × 8-hour days)  
**Team**: 1 developer

**Day 8 Tasks** (HandshakeFactory + DI Setup):
1. Create HandshakeFactory:
   ```csharp
   public class HandshakeFactory
   {
       public static IHandshake CreateQdrant(string endpoint, int port, string collection)
           => new QdrantHandshake(endpoint, port, collection);
   
       public static IHandshake CreateChroma(string endpoint, string collection)
           => new ChromaHandshake(endpoint, collection);
   
       public static IHandshake CreatePinecone(string apiKey, string indexName)
           => new PineconeHandshake(apiKey, indexName);
   
       public static IHandshake CreateWeaviate(string endpoint, string className)
           => new WeaviateHandshake(endpoint, className);
   }
   ```

2. Create Dependency Injection extensions:
   ```csharp
   // src/Chonkie.Genies/Extensions/ServiceCollectionExtensions.cs
   public static class ServiceCollectionExtensions
   {
       public static IServiceCollection AddChonkieGenies(
           this IServiceCollection services,
           IConfiguration config)
       {
           var genieType = config["Chonkie:Genie:Type"];
           
           services.AddSingleton<IGenie>(provider =>
           {
               return genieType switch
               {
                   "openai" => GenieFactory.CreateOpenAI(
                       config["Chonkie:Genie:ApiKey"],
                       config["Chonkie:Genie:Model"] ?? "gpt-4-turbo"),
                   "azure" => GenieFactory.CreateAzure(
                       config["Chonkie:Genie:Endpoint"],
                       config["Chonkie:Genie:ApiKey"],
                       config["Chonkie:Genie:Deployment"]),
                   "gemini" => GenieFactory.CreateGemini(
                       config["Chonkie:Genie:ApiKey"]),
                   _ => throw new InvalidOperationException($"Unknown genie type: {genieType}")
               };
           });
           
           return services;
       }
   
       public static IServiceCollection AddChonkieHandshakes(
           this IServiceCollection services,
           IConfiguration config)
       {
           var handshakeType = config["Chonkie:Handshake:Type"];
           
           services.AddSingleton<IHandshake>(provider =>
           {
               return handshakeType switch
               {
                   "qdrant" => HandshakeFactory.CreateQdrant(
                       config["Chonkie:Handshake:Endpoint"],
                       int.Parse(config["Chonkie:Handshake:Port"] ?? "6333"),
                       config["Chonkie:Handshake:Collection"]),
                   "chroma" => HandshakeFactory.CreateChroma(
                       config["Chonkie:Handshake:Endpoint"],
                       config["Chonkie:Handshake:Collection"]),
                   "pinecone" => HandshakeFactory.CreatePinecone(
                       config["Chonkie:Handshake:ApiKey"],
                       config["Chonkie:Handshake:Index"]),
                   "weaviate" => HandshakeFactory.CreateWeaviate(
                       config["Chonkie:Handshake:Endpoint"],
                       config["Chonkie:Handshake:Class"]),
                   _ => throw new InvalidOperationException($"Unknown handshake type: {handshakeType}")
               };
           });
           
           return services;
       }
   }
   ```

3. Create appsettings examples:
   ```json
   {
     "Chonkie": {
       "Genie": {
         "Type": "openai",
         "ApiKey": "${OPENAI_API_KEY}",
         "Model": "gpt-4-turbo"
       },
       "Handshake": {
         "Type": "qdrant",
         "Endpoint": "localhost",
         "Port": 6333,
         "Collection": "chunks"
       }
     }
   }
   ```

**Day 9 Tasks** (Pipeline Integration + Documentation):
1. Integrate Genies & Handshakes into Pipeline:
   ```csharp
   // Update existing Pipeline class
   public class Pipeline
   {
       private readonly IChunker _chunker;
       private readonly IEmbedding _embedding;
       private readonly IGenie _genie;
       private readonly IHandshake _handshake;
   
       public Pipeline(
           IChunker chunker,
           IEmbedding embedding,
           IGenie genie,
           IHandshake handshake)
       {
           _chunker = chunker;
           _embedding = embedding;
           _genie = genie;
           _handshake = handshake;
       }
   
       public async Task IndexDocument(string documentText, string metadata)
       {
           // 1. Chunk
           var chunks = await _chunker.ChunkAsync(documentText);
   
           // 2. Embed
           var embeddings = await _embedding.EmbedAsync(
               chunks.Select(c => c.Text).ToList());
   
           // 3. Store
           var chunkEmbeddings = chunks
               .Zip(embeddings)
               .Select(x => new ChunkEmbedding
               {
                   Text = x.First.Text,
                   Embedding = x.Second,
                   Metadata = metadata
               })
               .ToList();
   
           await _handshake.Write(chunkEmbeddings);
       }
   
       public async Task<string> QueryWithGeneration(string query)
       {
           // 1. Embed query
           var queryEmbedding = await _embedding.EmbedAsync(new[] { query });
   
           // 2. Search
           var results = await _handshake.Search(query, queryEmbedding[0]);
           var context = string.Join("\n", results.Select(r => r.Text));
   
           // 3. Generate with context
           var prompt = $"Context:\n{context}\n\nQuestion: {query}";
           return await _genie.Generate(prompt);
       }
   }
   ```

2. Create comprehensive integration tests (10+ scenarios)
3. Create user documentation:
   - Quick start guide
   - Configuration reference
   - Code examples (all 4 Genies × all 4 Handshakes)
   - Troubleshooting guide
   - Performance tuning guide

4. Run full test suite and verify all 56+ tests pass

**Success Criteria**:
- ✅ Pipeline fully integrated with all components
- ✅ HandshakeFactory working correctly
- ✅ DI setup documented and tested
- ✅ 10+ pipeline integration tests pass
- ✅ Complete documentation ready
- ✅ Code coverage: 90%+

**Deliverables**:
- HandshakeFactory implementation
- DI integration extensions
- Updated Pipeline class
- 10+ integration tests
- Complete user documentation

---

### Week 3: Advanced Chunkers & Release (Days 10-14)

#### Days 10-11: NeuralChunker + SlumberChunker
**Duration**: 16 hours (2 × 8-hour days)  
**Team**: 1 developer

**Day 10 Tasks** (NeuralChunker):
1. Implement NeuralChunker using embeddings for semantic segmentation:
   ```csharp
   public class NeuralChunker : IChunker
   {
       private readonly IEmbedding _embedding;
       private readonly float _similarityThreshold;
       private readonly int _maxChunkSize;
   
       public NeuralChunker(
           IEmbedding embedding,
           float similarityThreshold = 0.7f,
           int maxChunkSize = 512)
       {
           _embedding = embedding;
           _similarityThreshold = similarityThreshold;
           _maxChunkSize = maxChunkSize;
       }
   
       public async Task<List<Chunk>> ChunkAsync(string text)
       {
           var sentences = text.Split(new[] { '.', '!', '?' }, 
               StringSplitOptions.RemoveEmptyEntries)
               .Select(s => s.Trim())
               .Where(s => !string.IsNullOrEmpty(s))
               .ToList();
   
           if (sentences.Count == 0)
               return new List<Chunk> { new Chunk { Text = text } };
   
           // Embed all sentences
           var embeddings = await _embedding.EmbedAsync(sentences);
   
           var chunks = new List<Chunk>();
           var currentChunk = sentences[0];
           var lastEmbedding = embeddings[0];
   
           for (int i = 1; i < sentences.Count; i++)
           {
               var similarity = CosineSimilarity(lastEmbedding, embeddings[i]);
   
               if (similarity < _similarityThreshold || 
                   (currentChunk.Length + sentences[i].Length) > _maxChunkSize)
               {
                   chunks.Add(new Chunk { Text = currentChunk });
                   currentChunk = sentences[i];
                   lastEmbedding = embeddings[i];
               }
               else
               {
                   currentChunk += ". " + sentences[i];
               }
           }
   
           if (!string.IsNullOrEmpty(currentChunk))
               chunks.Add(new Chunk { Text = currentChunk });
   
           return chunks;
       }
   
       private float CosineSimilarity(float[] a, float[] b)
       {
           float dotProduct = 0;
           float normA = 0;
           float normB = 0;
   
           for (int i = 0; i < a.Length; i++)
           {
               dotProduct += a[i] * b[i];
               normA += a[i] * a[i];
               normB += b[i] * b[i];
           }
   
           return (float)(dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB)));
       }
   }
   ```

2. Write 6+ unit tests
3. Performance benchmarking (vs FastChunker)

**Day 11 Tasks** (SlumberChunker):
1. Implement SlumberChunker (hybrid approach - uses both semantic and character-based):
   ```csharp
   public class SlumberChunker : IChunker
   {
       private readonly IEmbedding _embedding;
       private readonly int _fastChunkSize;
       private readonly float _mergeThreshold;
   
       public SlumberChunker(
           IEmbedding embedding,
           int fastChunkSize = 256,
           float mergeThreshold = 0.8f)
       {
           _embedding = embedding;
           _fastChunkSize = fastChunkSize;
           _mergeThreshold = mergeThreshold;
       }
   
       public async Task<List<Chunk>> ChunkAsync(string text)
       {
           // Phase 1: Fast chunking
           var fastChunks = FastChunk(text);
   
           // Phase 2: Semantic analysis and merging
           var embeddings = await _embedding.EmbedAsync(
               fastChunks.Select(c => c.Text).ToList());
   
           var finalChunks = new List<Chunk> { fastChunks[0] };
           
           for (int i = 1; i < fastChunks.Count; i++)
           {
               var similarity = CosineSimilarity(
                   embeddings[i - 1],
                   embeddings[i]);
   
               if (similarity > _mergeThreshold && 
                   (finalChunks.Last().Text.Length + fastChunks[i].Text.Length) < 1024)
               {
                   finalChunks[^1] = new Chunk 
                   { 
                       Text = finalChunks[^1].Text + "\n" + fastChunks[i].Text 
                   };
               }
               else
               {
                   finalChunks.Add(fastChunks[i]);
               }
           }
   
           return finalChunks;
       }
   
       private List<Chunk> FastChunk(string text)
       {
           // Reuse FastChunker logic
           var chunks = new List<Chunk>();
           int pos = 0;
           while (pos < text.Length)
           {
               int end = Math.Min(pos + _fastChunkSize, text.Length);
               if (end < text.Length)
               {
                   int lastSpace = text.LastIndexOf(' ', end);
                   if (lastSpace > pos)
                       end = lastSpace;
               }
               chunks.Add(new Chunk { Text = text[pos..end] });
               pos = end;
           }
           return chunks;
       }
   }
   ```

2. Write 6+ unit tests
3. Benchmark against FastChunker and NeuralChunker

**Success Criteria**:
- ✅ Both chunkers implemented (200 LOC each)
- ✅ 12+ unit tests pass
- ✅ Benchmarks document performance
- ✅ 90%+ code coverage

**Deliverables**:
- NeuralChunker implementation
- SlumberChunker implementation
- Benchmarking results
- Performance comparison documentation

---

#### Days 12-13: Comprehensive Testing & Documentation
**Duration**: 16 hours (2 × 8-hour days)  
**Team**: 1 developer

**Day 12 Tasks** (Testing Suite):
1. Expand test coverage:
   - Add edge case tests for all components
   - Add stress tests (large documents, many chunks)
   - Add performance regression tests
   - Add end-to-end workflow tests

2. Coverage analysis:
   - Target: 90%+ overall coverage
   - Review uncovered lines
   - Add tests for edge cases

3. Integration testing matrix:
   ```
   Genies: OpenAI × Handshakes (Qdrant, Chroma, Pinecone, Weaviate)
   Azure × Handshakes (4)
   Gemini × Handshakes (4)
   LiteLLM × Handshakes (4)
   
   Total scenarios: 16 (test sampling, not all combinations)
   ```

4. Docker integration validation:
   - Verify all services start correctly
   - Test health checks
   - Test cleanup and teardown

**Day 13 Tasks** (Documentation & Examples):
1. Create comprehensive documentation:
   - API reference (all interfaces and methods)
   - Architecture guide (how all components fit together)
   - Configuration guide (all environment variables)
   - Deployment guide (Docker, cloud platforms)
   - Migration guide (from old Chonkie.Net to new version)
   - Performance tuning guide
   - Troubleshooting guide

2. Create code examples:
   - Example 1: Basic workflow (chunk → embed → store → query)
   - Example 2: Using DI in ASP.NET Core
   - Example 3: Batch processing large documents
   - Example 4: Custom Genie implementation
   - Example 5: Custom Handshake implementation

3. Update README files:
   - Main README with new components
   - Genies subfolder README
   - Handshakes subfolder README
   - Quick start guide

**Success Criteria**:
- ✅ 90%+ code coverage across all components
- ✅ 70+ total unit/integration tests
- ✅ All tests pass in CI/CD
- ✅ Comprehensive documentation complete
- ✅ 5+ code examples provided

**Deliverables**:
- Extended test suite (70+ tests)
- Coverage report
- Complete documentation (10+ guides)
- 5+ runnable code examples

---

#### Day 14: Final QA, Release Prep & Deployment
**Duration**: 8 hours  
**Team**: 1 developer

**Tasks**:
1. Final QA checklist:
   - ✅ Run full test suite (70+ tests)
   - ✅ Verify code coverage (90%+)
   - ✅ Static analysis (SonarLint, StyleCop)
   - ✅ Security scanning (Snyk, OWASP)
   - ✅ Performance benchmarks
   - ✅ Documentation review

2. Release preparation:
   - Update CHANGELOG.md with all changes
   - Update version numbers in all projects
   - Create release notes (features, fixes, breaking changes)
   - Tag git repository with release version

3. Package and publish:
   - Build NuGet packages:
     - Chonkie.Genies
     - Chonkie.Handshakes
     - Chonkie.Genies.Tests
     - Chonkie.Handshakes.Tests
   - Publish to NuGet.org (if public) or internal feed
   - Create GitHub release with artifacts

4. Post-release tasks:
   - Monitor for issues
   - Prepare hotfix branch if needed
   - Update sample applications
   - Notify stakeholders

**Success Criteria**:
- ✅ All QA checks pass
- ✅ Release notes published
- ✅ Packages deployed
- ✅ Documentation published
- ✅ Team trained on new components

**Deliverables**:
- Published NuGet packages
- Release notes and documentation
- Git release tag
- Sample/demo applications updated
- Team training materials

---

## Quality Gates & Success Metrics

### Code Quality
- **Coverage**: 90%+ unit test coverage across all components
- **Maintainability**: SonarLint A rating
- **Security**: No high/critical vulnerabilities
- **Performance**: FastChunker < 10ms for 512-char chunks

### Testing
- **Unit Tests**: 70+ tests (all pass)
- **Integration Tests**: 16+ scenario combinations
- **Stress Tests**: 10MB+ documents handled correctly
- **CI/CD**: All tests pass in automated pipeline

### Documentation
- **API Docs**: 100% of public APIs documented
- **Examples**: 5+ runnable code samples
- **Guides**: Architecture, Configuration, Deployment, Troubleshooting
- **Coverage**: All interfaces and methods documented

### Performance Benchmarks
- **Genie Response Time**: < 2s (excl. network latency)
- **Handshake Write**: < 100ms for 100 chunks
- **Handshake Search**: < 50ms per query
- **NeuralChunker**: < 5s for 10KB document

---

## Risk Mitigation

### Risk: API Rate Limiting
**Probability**: Medium | **Impact**: High  
**Mitigation**:
- Implement exponential backoff retry logic
- Add configurable rate limit handling
- Batch requests where possible
- Monitor rate limit headers

### Risk: Vector DB Connection Issues
**Probability**: Medium | **Impact**: High  
**Mitigation**:
- Health check endpoints for all Handshakes
- Circuit breaker pattern for resilience
- Fallback to in-memory cache temporarily
- Comprehensive connection error logging

### Risk: Embedding Dimension Mismatch
**Probability**: Low | **Impact**: High  
**Mitigation**:
- Auto-detect vector dimensions from first embedding
- Validate consistency on Write operations
- Clear error messages if dimensions don't match
- Documentation on common embedding models

### Risk: Schedule Slippage
**Probability**: Low | **Impact**: High  
**Mitigation**:
- Daily standup to track progress
- Prioritize core Genies over optional ones
- Have simplified versions of complex features
- Buffer time built into schedule (14 days = 10 days work + 4 days buffer)

---

## Resource Requirements

### Personnel
- **1 Primary Developer**: Full 14 days (40 hours/week = 56 hours total)
- **Code Reviewer**: 2-3 hours/week for PR reviews (10% allocation)
- **QA/Tester**: 1-2 hours/week (5% allocation)

### Infrastructure
- **Docker**: Qdrant, Chroma containers (local development)
- **Cloud Services**: OpenAI, Azure OpenAI, Gemini API keys
- **CI/CD**: GitHub Actions or Azure Pipelines (existing)

### Development Machine Requirements
- **RAM**: 16GB+ (for Docker containers)
- **Disk**: 50GB+ free space
- **.NET SDK**: .NET 10+
- **Docker**: Latest version

---

## Daily Standup Template

```
Date: [Date]
Standup Lead: [Developer Name]

✅ Completed Since Last Standup:
- [Task/Deliverable]
- [Test Count/Coverage %]
- [Blockers Resolved]

🔄 In Progress Today:
- [Current Task]
- [Estimated Completion]

⚠️ Blockers/Risks:
- [Issue]
- [Mitigation/Help Needed]

📊 Metrics:
- Code Coverage: [%]
- Tests Passing: [X/Y]
- Estimated Completion: [Days Remaining]
```

---

## Pre-Implementation Checklist

- [ ] All team members have .NET 10 SDK installed
- [ ] Docker is installed and running
- [ ] API keys available (OpenAI, Azure, Gemini, Pinecone)
- [ ] GitHub repository access configured
- [ ] CI/CD pipeline configured
- [ ] Code review process documented
- [ ] Git branching strategy defined (feature branches)
- [ ] Release process documented
- [ ] Monitoring/logging configured

---

## Success Criteria Summary

✅ **4 Genies** implemented and tested  
✅ **11 Handshakes** implemented (4 primary + 7 secondary references)  
✅ **Pipeline** fully integrated with new components  
✅ **70+ unit/integration tests** passing  
✅ **90%+ code coverage**  
✅ **Comprehensive documentation** (10+ guides)  
✅ **5+ code examples**  
✅ **Production-ready** NuGet packages  
✅ **Released** and available for use

---

## Timeline Summary

| Phase | Days | Duration | Focus |
|-------|------|----------|-------|
| **Foundation** | 1 | 1 day | Setup projects, base interfaces, FastChunker scaffold |
| **Implementation** | 2-3 | 2 days | FastChunker + OpenAIGenie |
| **Genies** | 4-5 | 2 days | Azure, Gemini, LiteLLM Genies |
| **Handshakes** | 6-7 | 2 days | Qdrant, Chroma, Pinecone, Weaviate |
| **Integration** | 8-9 | 2 days | Factory, DI, Pipeline integration |
| **Advanced** | 10-11 | 2 days | NeuralChunker, SlumberChunker |
| **Polish** | 12-13 | 2 days | Testing, documentation, examples |
| **Release** | 14 | 1 day | QA, release, deployment |

**Total**: 14 working days (2-3 weeks depending on team availability)

---

## Post-Release Support

### Week 1 Post-Release
- Monitor for bug reports
- Gather user feedback
- Plan hotfixes if needed
- Update documentation based on user questions

### Week 2+ Post-Release
- Plan secondary Handshake implementations (MongoDB, Elasticsearch, Milvus, Pgvector, Redis)
- Plan advanced features (caching, async streaming, batch processing)
- Performance optimization based on production usage

---

**Document Version**: 1.0  
**Last Updated**: January 5, 2026  
**Status**: Ready for implementation  
**Approval**: Pending team review
