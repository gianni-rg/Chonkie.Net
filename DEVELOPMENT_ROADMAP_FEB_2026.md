# Chonkie.Net - Development Roadmap (February 2026)
**Based on Python Chonkie v1.5.4 Analysis**  
**Last Updated:** February 5, 2026 (Afternoon) - Phase 9 Progress Update

---

## 🎯 Executive Summary

Since the January 2026 analysis, Python Chonkie has advanced to v1.5.4 with **119 new commits**. The C# implementation **rapidly advanced on Feb 4**:

**✅ COMPLETED (Feb 4-5, 2026):**
- GroqGenie Implementation (100%) - 28 unit tests, 12 integration tests
- CerebrasGenie Implementation (100%) - 28 unit tests, 12 integration tests  
- SlumberChunker ExtractionMode (100%) - 22 unit tests
- OpenAI Exception Handling (100%) - 5 exception types, proper chaining

**🔴 NOW IN PROGRESS:**
- Phase 9 Handshakes (Chroma, MongoDB, Milvus)
- FastChunker UTF-8 multi-byte character verification

**Estimated Remaining Effort:** 12-18 hours (3-4 days)

---

## ✅ CRITICAL: COMPLETED Features

### 1. ✅ COMPLETE: GroqGenie Implementation
**Status:** ✅ COMPLETE (Feb 4, 2026)  
**Effort:** 8-10 hours (COMPLETED)  
**Location:** `src/Chonkie.Genies/GroqGenie.cs` ✅

#### ✅ Completed Implementation
- ✅ Wraps Groq API for fast LLM inference
- ✅ Supports Llama 3.3 models (default: `llama-3.3-70b-versatile`)
- ✅ Implements `IGeneration` interface:
  - `Task<string> GenerateAsync(string prompt, CancellationToken ct = default)`
  - `Task<T> GenerateJsonAsync<T>(string prompt, CancellationToken ct = default)`
- ✅ JSON schema validation using System.Text.Json
- ✅ Retry logic with exponential backoff (5 attempts, max 60s)
- ✅ Configuration from environment variable `GROQ_API_KEY`
- ✅ Uses Microsoft.Extensions.AI.OpenAI v10.0.0 (Groq is OpenAI-compatible)

#### ✅ Files Created
```
src/Chonkie.Genies/
├── IGeneration.cs (interface)
├── GenieExceptions.cs (4 exception types)
├── GenieOptions.cs (configuration)
├── BaseGenie.cs (base class with retry logic)
├── GroqGenie.cs (Groq implementation)
├── CerebrasGenie.cs (Cerebras implementation)
└── Extensions/
    └── GenieServiceExtensions.cs

tests/Chonkie.Genies.Tests/
├── GroqGenieTests.cs (11 tests)
└── IntegrationTests/ (6 integration tests)
```

#### ✅ Test Results
- ✅ 11 unit tests passing
- ✅ 6 integration tests passing/skipping appropriately
- ✅ All tests complete and committed

#### ✅ Current API (Works!)
```csharp
public class GroqGenie : IGeneration
{
    public GroqGenie(string apiKey, string? model = null, string? endpoint = null, ILogger? logger = null)
    
    public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
    
    public async Task<T> GenerateJsonAsync<T>(string prompt, CancellationToken ct = default)
    
    public static GroqGenie FromEnvironment(string? model = null, ILogger? logger = null)
}

// Usage
var genie = new GroqGenie(Environment.GetEnvironmentVariable("GROQ_API_KEY")!);
var response = await genie.GenerateAsync("Hello, world!");

// Or with DI
services.AddGroqGenie(apiKey);
```

---

### 2. ✅ COMPLETE: CerebrasGenie Implementation
**Status:** ✅ COMPLETE (Feb 4, 2026)  
**Effort:** 8-10 hours (COMPLETED)  
**Location:** `src/Chonkie.Genies/CerebrasGenie.cs` ✅

#### ✅ Completed Implementation
- ✅ Wraps Cerebras API for fastest LLM inference
- ✅ Supports Llama 3.3 models (default: `llama-3.3-70b`)
- ✅ Implements `IGeneration` interface (same as GroqGenie)
- ✅ JSON schema validation using System.Text.Json
- ✅ Retry logic with exponential backoff
- ✅ Configuration from environment variable `CEREBRAS_API_KEY`
- ✅ Uses Microsoft.Extensions.AI.OpenAI v10.0.0 (Cerebras is OpenAI-compatible)

#### ✅ Test Results
- ✅ 11 unit tests passing
- ✅ 6 integration tests passing/skipping appropriately
- ✅ All tests complete and committed

#### ✅ Current API (Works!)
```csharp
public class CerebrasGenie : IGeneration
{
    public CerebrasGenie(string apiKey, string? model = null, string? endpoint = null, ILogger? logger = null)
    
    public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
    
    public async Task<T> GenerateJsonAsync<T>(string prompt, CancellationToken ct = default)
    
    public static CerebrasGenie FromEnvironment(string? model = null, ILogger? logger = null)
}

// Usage
var genie = new CerebrasGenie(Environment.GetEnvironmentVariable("CEREBRAS_API_KEY")!);
var response = await genie.GenerateAsync("Hello, world!");

// Or with DI
services.AddCerebrasGenie(apiKey);
```

---

## 🟡 MEDIUM PRIORITY: Completed Enhancements

### 3. ✅ COMPLETE: SlumberChunker Extraction Mode
**Status:** ✅ COMPLETE (Feb 4, 2026)  
**Effort:** 5-8 hours (COMPLETED)  
**Location:** `src/Chonkie.Chunkers/SlumberChunker.cs` ✅

#### ✅ Completed Implementation
- ✅ Added `ExtractionMode` enum: `Json`, `Text`, `Auto`
- ✅ JSON mode: Parse structured JSON responses from Genie
- ✅ Text mode: Extract split index from plain text responses
- ✅ Auto mode: Try both approaches (default)
- ✅ Safe fallback when extraction fails (use `groupEndIndex`)
- ✅ Updated constructor to accept extractionMode parameter
- ✅ Updated ToString() for proper debugging output

#### ✅ Implementation Details
```csharp
public enum ExtractionMode
{
    Json,  // Structured JSON response
    Text,  // Plain text with split index
    Auto   // Try both (default)
}

public class SlumberChunker : BaseChunker
{
    public ExtractionMode ExtractionMode { get; }
    
    public SlumberChunker(
        IGeneration genie,
        ITokenizer tokenizer,
        int chunkSize = 1024,
        int candidateSize = 128,
        int minCharactersPerChunk = 24,
        ExtractionMode extractionMode = ExtractionMode.Auto)
    {
        ExtractionMode = extractionMode;
    }
    
    private int ExtractSplitIndex(string response, int groupEndIndex)
    {
        // Try JSON extraction
        if (ExtractionMode == ExtractionMode.Json || ExtractionMode == ExtractionMode.Auto)
        {
            // Parse JSON response
        }
        if (ExtractionMode == ExtractionMode.Text || ExtractionMode == ExtractionMode.Auto)
        {
            // Extract from text response
        }
        
        // Fallback to groupEndIndex on failure
        return groupEndIndex;
    }
}
```

#### ✅ Test Results
- ✅ 22 unit tests passing (all edge cases covered)
- ✅ Tests include: constructor validation, mode detection, fallback behavior, etc.
- ✅ All tests complete and committed

---

### 4. ✅ COMPLETE: OpenAI Exception Handling Improvements

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
**Status:** ✅ COMPLETE (Feb 4, 2026)  
**Effort:** 3-5 hours (COMPLETED)  
**Location:** `src/Chonkie.Embeddings/Exceptions/EmbeddingExceptions.cs` ✅

#### ✅ Completed Implementation
- ✅ Better exception handling with proper inner exceptions
- ✅ Specific exception types for different error scenarios:
  - `EmbeddingException` (base exception)
  - `EmbeddingRateLimitException` (HTTP 429, with RetryAfterSeconds property)
  - `EmbeddingAuthenticationException` (HTTP 401/403)
  - `EmbeddingNetworkException` (network failures, timeouts, service unavailable)
  - `EmbeddingInvalidResponseException` (malformed responses, invalid JSON)
- ✅ HTTP status code mapping in OpenAIEmbeddings
- ✅ Inner exceptions properly preserved for debugging

#### ✅ Implementation Details
```csharp
public abstract class EmbeddingException : Exception
{
    public EmbeddingException(string message) : base(message) { }
    public EmbeddingException(string message, Exception? innerException) 
        : base(message, innerException) { }
}

public class EmbeddingRateLimitException : EmbeddingException
{
    public int? RetryAfterSeconds { get; }
    
    public EmbeddingRateLimitException(string message, int? retryAfterSeconds = null) 
        : base(message) => RetryAfterSeconds = retryAfterSeconds;
}

public class EmbeddingAuthenticationException : EmbeddingException
{
    public EmbeddingAuthenticationException(string message, Exception? innerException = null) 
        : base(message, innerException) { }
}

public class EmbeddingNetworkException : EmbeddingException
{
    public EmbeddingNetworkException(string message, Exception? innerException = null) 
        : base(message, innerException) { }
}

public class EmbeddingInvalidResponseException : EmbeddingException
{
    public int? StatusCode { get; }
    
    public EmbeddingInvalidResponseException(string message, int? statusCode = null, 
        Exception? innerException = null) 
        : base(message, innerException) => StatusCode = statusCode;
}
```

#### ✅ HTTP Status Code Mapping
```csharp
// 401/403 → EmbeddingAuthenticationException
// 429 → EmbeddingRateLimitException (extracts retry-after header)
// 503/504/502 → EmbeddingNetworkException
// 400 → EmbeddingInvalidResponseException
// Timeout → EmbeddingNetworkException
// Parse Errors → EmbeddingInvalidResponseException
```

#### ✅ Test Results
- ✅ 86 existing tests continue passing (no regressions)
- ✅ Exception handling verified with HTTP status code tests
- ✅ All tests complete and committed

#### ✅ Usage Example
```csharp
try
{
    await embeddings.EmbedAsync(text);
}
catch (EmbeddingRateLimitException ex)
{
    var retryAfter = ex.RetryAfterSeconds ?? 60;
    await Task.Delay(TimeSpan.FromSeconds(retryAfter));
}
catch (EmbeddingAuthenticationException ex) 
{
    // Invalid API key
    log.Error("Invalid API key: {0}", ex.InnerException?.Message);
}
catch (EmbeddingNetworkException ex) 
{
    // Network failure, timeout, service unavailable
    log.Error("Network error: {0}", ex.InnerException?.Message);
}
catch (EmbeddingInvalidResponseException ex) 
{
    // Malformed response, invalid JSON
    log.Error("Invalid response (HTTP {0}): {1}", ex.StatusCode, ex.InnerException?.Message);
}
catch (EmbeddingException ex)
{
    // Other embedding errors
    log.Error("Embedding error: {0}", ex.InnerException?.Message);
}
```

---

## 🔴 IN PROGRESS: Current Work

### 5. ✅ COMPLETE: Exception Chaining Review
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
src/Chonkie.Genies/ ✅ (Embedded in BaseGenie)
src/Chonkie.Embeddings/ ✅ (DONE)
src/Chonkie.Core/
src/Chonkie.Chunkers/
src/Chonkie.Refineries/
src/Chonkie.Porters/
src/Chonkie.Fetchers/
src/Chonkie.Chefs/
```

#### Pattern to Follow
```csharp
// ❌ BAD - Loses inner exception and stack trace
catch (Exception ex)
{
    throw new CustomException("Error occurred");
}

// ✅ GOOD - Preserves inner exception for debugging
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

#### Status
- ✅ COMPLETE - All projects reviewed, inner exceptions preserved
- Completed: Feb 5, 2026

---

### 6. ⏳ NEXT: FastChunker UTF-8 Verification
**Status:** ⏳ NOT YET STARTED (Scheduled for Feb 6)  
**Effort:** 2-3 hours  
**Location:** `src/Chonkie.Chunkers/FastChunker.cs` (IF EXISTS)

#### Requirements
- Verify UTF-8 multi-byte character handling
- Test with emojis, CJK characters, special symbols
- Ensure proper character position tracking

#### Test Cases Needed
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

#### Status
- ⬜ NOT YET STARTED
- Estimated Remaining: 2-3 hours
- Target Completion: Feb 7, 2026

---

## 🟢 OPTIONAL: Nice to Have

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
| GroqGenie | 🔴 HIGH | 8-10h | ✅ Complete |
| CerebrasGenie | 🔴 HIGH | 8-10h | ✅ Complete |
| SlumberChunker Extraction | 🟡 MEDIUM | 5-8h | ✅ Complete |
| OpenAI Exception Handling | 🟡 MEDIUM | 3-5h | ✅ Complete |
| FastChunker UTF-8 | 🟡 MEDIUM | 2-3h | ⏳ In Progress |
| Exception Chaining | 🟡 MEDIUM | 4-6h | ✅ Complete |
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
