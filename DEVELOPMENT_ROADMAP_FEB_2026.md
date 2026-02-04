# Chonkie.Net - Development Roadmap (February 2026)
**Based on Python Chonkie v1.5.4 Analysis**  
**Date:** February 4, 2026

---

## 🎯 Executive Summary

Since the January 2026 analysis, Python Chonkie has advanced to v1.5.4 with **119 new commits**. The C# implementation needs **2 new Genies** and several enhancements to maintain parity.

**Estimated Total Effort:** 36-53 hours (2-3 weeks)

---

## 🔴 CRITICAL: Missing Features (HIGH PRIORITY)

### 1. GroqGenie Implementation
**Effort:** 8-10 hours  
**Location:** `src/Chonkie.Genies/GroqGenie.cs` (NEW)

#### Requirements
- Wrap Groq API for fast LLM inference
- Support Llama 3.3 models (default: `llama-3.3-70b-versatile`)
- Implement `IGeneration` interface:
  - `Task<string> GenerateAsync(string prompt, CancellationToken ct = default)`
  - `Task<T> GenerateJsonAsync<T>(string prompt, CancellationToken ct = default)`
- JSON schema validation using System.Text.Json
- Retry logic with exponential backoff (5 attempts, max 60s)
- Configuration from environment variable `GROQ_API_KEY`

#### Implementation Options
1. **Use Groq .NET SDK** (if available)
2. **Use HttpClient** with Groq REST API
3. **Use Microsoft.Extensions.AI.OpenAI** (Groq is OpenAI-compatible)

#### Files to Create
```
src/Chonkie.Genies/
├── IGeneration.cs (interface)
├── GroqGenie.cs
├── GroqGenieOptions.cs
└── Extensions/
    └── GroqGenieServiceExtensions.cs

tests/Chonkie.Genies.Tests/
├── GroqGenieTests.cs
└── GroqGenieIntegrationTests.cs
```

#### Sample API
```csharp
public class GroqGenie : IGeneration
{
    public GroqGenie(string apiKey, string model = "llama-3.3-70b-versatile")
    
    public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
    
    public async Task<T> GenerateJsonAsync<T>(string prompt, CancellationToken ct = default)
}

// Usage
var genie = new GroqGenie(Environment.GetEnvironmentVariable("GROQ_API_KEY"));
var response = await genie.GenerateAsync("Hello, world!");
```

---

### 2. CerebrasGenie Implementation
**Effort:** 8-10 hours  
**Location:** `src/Chonkie.Genies/CerebrasGenie.cs` (NEW)

#### Requirements
- Wrap Cerebras API for fastest LLM inference
- Support Llama 3.3 models (default: `llama-3.3-70b`)
- Implement `IGeneration` interface (same as GroqGenie)
- JSON schema validation (basic mode - schema in prompt)
- Retry logic with exponential backoff
- Configuration from environment variable `CEREBRAS_API_KEY`

#### Implementation Options
1. **Use Cerebras .NET SDK** (if available)
2. **Use HttpClient** with Cerebras REST API
3. **Use Microsoft.Extensions.AI.OpenAI** (Cerebras is OpenAI-compatible)

#### Files to Create
```
src/Chonkie.Genies/
├── CerebrasGenie.cs
├── CerebrasGenieOptions.cs
└── Extensions/
    └── CerebrasGenieServiceExtensions.cs

tests/Chonkie.Genies.Tests/
├── CerebrasGenieTests.cs
└── CerebrasGenieIntegrationTests.cs
```

#### Sample API
```csharp
public class CerebrasGenie : IGeneration
{
    public CerebrasGenie(string apiKey, string model = "llama-3.3-70b")
    
    public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
    
    public async Task<T> GenerateJsonAsync<T>(string prompt, CancellationToken ct = default)
}

// Usage
var genie = new CerebrasGenie(Environment.GetEnvironmentVariable("CEREBRAS_API_KEY"));
var response = await genie.GenerateAsync("Hello, world!");
```

---

## 🟡 MEDIUM PRIORITY: Enhancements

### 3. SlumberChunker Extraction Mode
**Effort:** 5-8 hours  
**Location:** `src/Chonkie.Chunkers/SlumberChunker.cs` (EXISTING)

#### Requirements
- Add `ExtractionMode` enum: `Json` (default) or `Text`
- JSON mode: Parse structured JSON responses from Genie
- Text mode: Extract split index from plain text responses
- Safe fallback when extraction fails (use `groupEndIndex`)

#### Changes Needed
```csharp
public enum ExtractionMode
{
    Json,  // Structured JSON response
    Text   // Plain text with split index
}

public class SlumberChunker : BaseChunker
{
    public SlumberChunker(
        IGeneration genie,
        ITokenizer tokenizer,
        int chunkSize = 1024,
        int candidateSize = 128,
        int minCharactersPerChunk = 24,
        ExtractionMode extractionMode = ExtractionMode.Json)
    
    private int ExtractSplitIndex(string response, int groupEndIndex)
    {
        // Try JSON extraction
        if (extractionMode == ExtractionMode.Json)
        {
            // Parse JSON response
        }
        else
        {
            // Extract from text response
        }
        
        // Fallback to groupEndIndex on failure
        return groupEndIndex;
    }
}
```

#### Tests to Add
- Test JSON extraction mode
- Test text extraction mode
- Test fallback behavior
- Test edge cases (empty responses, invalid JSON, etc.)

---

### 4. OpenAI Exception Handling Improvements
**Effort:** 3-5 hours  
**Location:** `src/Chonkie.Embeddings/OpenAIEmbeddings.cs` (EXISTING)

#### Requirements
- Better exception handling with proper inner exceptions
- Specific exception types for different error scenarios
- Comprehensive error messages
- Retry logic improvements

#### Changes Needed
```csharp
public class OpenAIEmbeddings : IEmbeddings
{
    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        try
        {
            return await _client.GetEmbeddingsAsync(text, ct);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new RateLimitException("OpenAI rate limit exceeded", ex);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new AuthenticationException("Invalid OpenAI API key", ex);
        }
        catch (Exception ex)
        {
            throw new EmbeddingException("Failed to generate embeddings", ex);
        }
    }
}
```

#### Tests to Add
- Test rate limit handling
- Test authentication errors
- Test network failures
- Test timeout scenarios

---

### 5. FastChunker UTF-8 Verification
**Effort:** 2-3 hours  
**Location:** `src/Chonkie.Chunkers/FastChunker.cs` (IF EXISTS)

#### Requirements
- Verify UTF-8 multi-byte character handling
- Test with emojis, CJK characters, special symbols
- Ensure proper character position tracking

#### Tests to Add
```csharp
[Fact]
public void FastChunker_ShouldHandleEmojis()
{
    var chunker = new FastChunker(chunkSize: 100);
    var text = "Hello 👋 World 🌍 with emojis 🎉";
    var chunks = chunker.Chunk(text);
    
    // Verify proper byte offset handling
    var reconstructed = string.Concat(chunks.Select(c => c.Text));
    reconstructed.ShouldBe(text);
}

[Fact]
public void FastChunker_ShouldHandleCJKCharacters()
{
    var chunker = new FastChunker(chunkSize: 100);
    var text = "这是中文 한글 日本語";
    var chunks = chunker.Chunk(text);
    
    // Verify proper character handling
}
```

---

### 6. Exception Chaining Review
**Effort:** 4-6 hours  
**Location:** All projects

#### Requirements
- Review all exception handling code
- Ensure inner exceptions are properly preserved
- Use `throw new Exception("message", innerException)` pattern

#### Files to Review
```
src/Chonkie.Core/
src/Chonkie.Chunkers/
src/Chonkie.Embeddings/
src/Chonkie.Genies/ (NEW)
src/Chonkie.Refineries/
src/Chonkie.Porters/
src/Chonkie.Fetchers/
src/Chonkie.Chefs/
```

#### Pattern to Follow
```csharp
// ❌ BAD - Loses stack trace
catch (Exception ex)
{
    throw new CustomException("Error occurred");
}

// ✅ GOOD - Preserves inner exception
catch (Exception ex)
{
    throw new CustomException("Error occurred", ex);
}

// ✅ GOOD - Re-throws same exception
catch (Exception ex)
{
    _logger.LogError(ex, "Error occurred");
    throw;
}
```

---

## 🟢 LOW PRIORITY: Nice to Have

### 7. Model Registry Enhancements
**Effort:** 1-2 hours  
**Location:** `src/Chonkie.Embeddings/ModelRegistry.cs` (IF EXISTS)

#### Requirements
- Add official SentenceTransformer model names
- Inline model list for better maintainability

```csharp
public static class SentenceTransformerModels
{
    public static readonly string[] OfficialModels = new[]
    {
        "all-MiniLM-L6-v2",
        "all-MiniLM-L12-v2",
        "all-mpnet-base-v2",
        "paraphrase-MiniLM-L6-v2",
        "sentence-t5-base",
        // ... add more
    };
    
    public static bool IsOfficialModel(string model)
        => OfficialModels.Contains(model, StringComparer.OrdinalIgnoreCase);
}
```

---

### 8. Dependency Updates
**Effort:** 2-3 hours  
**Location:** All `.csproj` files

#### Requirements
- Update NuGet packages to latest stable versions
- Review security vulnerabilities
- Test for breaking changes

#### Packages to Review
- Microsoft.Extensions.* packages
- System.Text.Json
- HTTP client packages
- ML/AI packages (if any)
- Testing packages (xUnit, NSubstitute, Shouldly)

```powershell
# Check for outdated packages
dotnet list package --outdated

# Update packages
dotnet add package <PackageName> --version <Version>
```

---

### 9. CI/CD Optimization
**Effort:** 2-3 hours  
**Location:** `.github/workflows/`

#### Requirements
- Enable parallel test execution
- Optimize build times
- Cache NuGet packages

```yaml
# .github/workflows/test.yml
- name: Run tests
  run: dotnet test --no-build --no-restore --configuration Release --logger "trx" --collect:"XPlat Code Coverage" -- RunConfiguration.ParallelizeAssemblies=true
```

---

## 📅 Implementation Timeline

### Week 1: Critical Genies (15-20 hours)
**Days 1-2:**
- [ ] Create `Chonkie.Genies` project
- [ ] Define `IGeneration` interface
- [ ] Implement GroqGenie base functionality

**Days 3-4:**
- [ ] Implement CerebrasGenie base functionality
- [ ] Add JSON schema support to both

**Day 5:**
- [ ] Write unit tests
- [ ] Write integration tests
- [ ] Documentation

### Week 2: Enhancements (10-15 hours)
**Days 1-2:**
- [ ] SlumberChunker extraction mode
- [ ] Tests and documentation

**Days 3-4:**
- [ ] OpenAI exception handling
- [ ] FastChunker UTF-8 verification
- [ ] Tests

**Day 5:**
- [ ] Exception chaining review
- [ ] Fix issues found

### Week 3: Quality & Maintenance (10-15 hours)
**Days 1-2:**
- [ ] Dependency updates
- [ ] Security review
- [ ] Breaking change testing

**Days 3-4:**
- [ ] CI/CD optimization
- [ ] Model registry enhancements
- [ ] Documentation updates

**Day 5:**
- [ ] Final testing
- [ ] Release preparation
- [ ] Update CHANGELOG.md

---

## 📊 Summary Table

| Task | Priority | Effort | Status |
|------|----------|--------|--------|
| GroqGenie | 🔴 HIGH | 8-10h | ❌ Not Started |
| CerebrasGenie | 🔴 HIGH | 8-10h | ❌ Not Started |
| SlumberChunker Extraction | 🟡 MEDIUM | 5-8h | ❌ Not Started |
| OpenAI Exception Handling | 🟡 MEDIUM | 3-5h | ❌ Not Started |
| FastChunker UTF-8 | 🟡 MEDIUM | 2-3h | ❌ Not Started |
| Exception Chaining | 🟡 MEDIUM | 4-6h | ❌ Not Started |
| Model Registry | 🟢 LOW | 1-2h | ❌ Not Started |
| Dependency Updates | 🟢 LOW | 2-3h | ❌ Not Started |
| CI/CD Optimization | 🟢 LOW | 2-3h | ❌ Not Started |

**Total:** 35-50 hours over 2-3 weeks

---

## 🚀 Getting Started

### Step 1: Create Genies Project
```powershell
cd c:\Projects\Personal\Chonkie.Net\src
dotnet new classlib -n Chonkie.Genies -f net10.0
dotnet sln ..\Chonkie.Net.sln add Chonkie.Genies\Chonkie.Genies.csproj
```

### Step 2: Add Dependencies
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Http" Version="10.0.0" />
  <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.0" />
  <PackageReference Include="System.Text.Json" Version="10.0.0" />
  <PackageReference Include="Polly" Version="8.5.0" />
</ItemGroup>
```

### Step 3: Create Tests Project
```powershell
cd c:\Projects\Personal\Chonkie.Net\tests
dotnet new xunit -n Chonkie.Genies.Tests -f net10.0
dotnet sln ..\Chonkie.Net.sln add Chonkie.Genies.Tests\Chonkie.Genies.Tests.csproj
```

---

## 📖 References

- **Python Analysis:** [PYTHON_CHANGES_FEBRUARY_2026.md](PYTHON_CHANGES_FEBRUARY_2026.md)
- **Previous Analysis:** [docs/archived/PYTHON_CHANGES_ANALYSIS_JAN2025.md](docs/archived/PYTHON_CHANGES_ANALYSIS_JAN2025.md)
- **Python Repository:** https://github.com/chonkie-inc/chonkie
- **Groq API:** https://groq.com/
- **Cerebras API:** https://cerebras.ai/
- **C# Guidelines:** [AGENTS.md](AGENTS.md)

---

## ✅ Acceptance Criteria

### GroqGenie
- [ ] Implements IGeneration interface
- [ ] Supports text generation
- [ ] Supports JSON schema-based generation
- [ ] Has retry logic with exponential backoff
- [ ] Configurable via environment variable
- [ ] Has comprehensive unit tests (>80% coverage)
- [ ] Has integration tests
- [ ] Has XML documentation

### CerebrasGenie
- [ ] Implements IGeneration interface
- [ ] Supports text generation
- [ ] Supports JSON generation (basic mode)
- [ ] Has retry logic with exponential backoff
- [ ] Configurable via environment variable
- [ ] Has comprehensive unit tests (>80% coverage)
- [ ] Has integration tests
- [ ] Has XML documentation

### SlumberChunker
- [ ] Has ExtractionMode parameter
- [ ] Supports JSON extraction
- [ ] Supports text extraction
- [ ] Has safe fallback behavior
- [ ] Has tests for all extraction modes

### Quality
- [ ] All exceptions properly chained
- [ ] UTF-8 handling verified
- [ ] Dependencies updated
- [ ] CI/CD optimized
- [ ] Documentation complete

---

## 🎓 Notes for Developer

1. **Leverage Microsoft.Extensions.AI**
   - Consider using Microsoft.Extensions.AI for unified interface
   - Groq and Cerebras are OpenAI-compatible

2. **Retry Logic**
   - Use Polly library for retry policies
   - Exponential backoff: 2, 4, 8, 16, 32 seconds

3. **JSON Schema**
   - Use System.Text.Json for serialization
   - Consider JSON Schema validation libraries if needed

4. **Testing Strategy**
   - Unit tests: Mock HTTP responses
   - Integration tests: Use real APIs with test keys
   - Use [SkippableFact] for integration tests

5. **C# 14 Features**
   - Use extension members where appropriate
   - Use field keyword in properties
   - Use null-conditional assignment

6. **Error Handling**
   - Create specific exception types
   - Always preserve inner exceptions
   - Log errors with structured logging
