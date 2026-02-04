# Chonkie.Net Implementation Quick Start
**Last Updated:** February 4, 2026  
**For:** Developers implementing new features  
**Time to Read:** 10 minutes

---

## 🎯 What You Need to Know Right Now

### Current Status
- ✅ **65% Complete** - Core functionality done
- 🔴 **Missing:** Genies (LLM integrations)
- 🔴 **Missing:** Handshakes (Vector DB integrations)
- 🟡 **Needs Update:** SlumberChunker, Exception Handling

### Your First Task
**Implement GroqGenie and CerebrasGenie** (15-20 hours total)

---

## 🚀 Quick Start: Implementing Genies

### Step 1: Setup (30 minutes)

```powershell
# 1. Navigate to solution
cd c:\Projects\Personal\Chonkie.Net

# 2. Create new projects
cd src
dotnet new classlib -n Chonkie.Genies -f net10.0
cd ../tests
dotnet new xunit -n Chonkie.Genies.Tests -f net10.0
cd ..

# 3. Add to solution
dotnet sln add src/Chonkie.Genies/Chonkie.Genies.csproj
dotnet sln add tests/Chonkie.Genies.Tests/Chonkie.Genies.Tests.csproj

# 4. Add dependencies
cd src/Chonkie.Genies
dotnet add package Microsoft.Extensions.Http
dotnet add package Microsoft.Extensions.Logging.Abstractions
dotnet add package System.Text.Json
dotnet add package Polly

# 5. Add test dependencies
cd ../../tests/Chonkie.Genies.Tests
dotnet add reference ../../src/Chonkie.Genies/Chonkie.Genies.csproj
dotnet add package xUnit
dotnet add package NSubstitute
dotnet add package Shouldly
```

### Step 2: Create IGeneration Interface (30 minutes)

**File:** `src/Chonkie.Genies/IGeneration.cs`

```csharp
namespace Chonkie.Genies;

/// <summary>
/// Defines the contract for text generation using LLM providers.
/// </summary>
public interface IGeneration
{
    /// <summary>
    /// Generates text based on the given prompt.
    /// </summary>
    /// <param name="prompt">The input prompt for generation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated text response.</returns>
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a structured JSON response conforming to the specified schema.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON response into.</typeparam>
    /// <param name="prompt">The input prompt for generation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response object.</returns>
    Task<T> GenerateJsonAsync<T>(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates text for multiple prompts in batch.
    /// </summary>
    /// <param name="prompts">Collection of prompts to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of generated responses.</returns>
    Task<IReadOnlyList<string>> GenerateBatchAsync(
        IEnumerable<string> prompts, 
        CancellationToken cancellationToken = default);
}
```

### Step 3: Create BaseGenie with Retry Logic (1 hour)

**File:** `src/Chonkie.Genies/BaseGenie.cs`

```csharp
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Chonkie.Genies;

/// <summary>
/// Base class for all Genie implementations with retry logic.
/// </summary>
public abstract class BaseGenie : IGeneration
{
    protected ILogger Logger { get; }
    protected ResiliencePipeline RetryPipeline { get; }

    protected BaseGenie(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        RetryPipeline = CreateRetryPipeline();
    }

    /// <summary>
    /// Creates a retry pipeline with exponential backoff.
    /// </summary>
    private ResiliencePipeline CreateRetryPipeline()
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 5,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(60),
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>(),
                OnRetry = args =>
                {
                    Logger.LogWarning(
                        "Retry attempt {Attempt} after {Delay}ms delay due to: {Exception}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message);
                    return default;
                }
            })
            .Build();
    }

    public abstract Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
    
    public abstract Task<T> GenerateJsonAsync<T>(string prompt, CancellationToken cancellationToken = default);

    public virtual async Task<IReadOnlyList<string>> GenerateBatchAsync(
        IEnumerable<string> prompts, 
        CancellationToken cancellationToken = default)
    {
        var results = new List<string>();
        foreach (var prompt in prompts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await GenerateAsync(prompt, cancellationToken);
            results.Add(result);
        }
        return results;
    }
}
```

### Step 4: Implement GroqGenie (3-4 hours)

**File:** `src/Chonkie.Genies/GroqGenie.cs`

```csharp
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Chonkie.Genies;

/// <summary>
/// Genie implementation for Groq's fast LLM inference.
/// </summary>
public class GroqGenie : BaseGenie
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly string _apiKey;

    public GroqGenie(
        IHttpClientFactory httpClientFactory,
        ILogger<GroqGenie> logger,
        string? apiKey = null,
        string model = "llama-3.3-70b-versatile") 
        : base(logger)
    {
        _httpClient = httpClientFactory.CreateClient("Groq");
        _model = model;
        _apiKey = apiKey ?? Environment.GetEnvironmentVariable("GROQ_API_KEY") 
            ?? throw new InvalidOperationException("GROQ_API_KEY environment variable not set");

        _httpClient.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public override async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty", nameof(prompt));

        return await RetryPipeline.ExecuteAsync(async ct =>
        {
            var request = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("chat/completions", request, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GroqResponse>(ct);
            return result?.Choices?.FirstOrDefault()?.Message?.Content 
                ?? throw new InvalidOperationException("Invalid response from Groq API");
        }, cancellationToken);
    }

    public override async Task<T> GenerateJsonAsync<T>(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty", nameof(prompt));

        return await RetryPipeline.ExecuteAsync(async ct =>
        {
            // Get JSON schema for type T
            var schema = GenerateJsonSchema<T>();

            var request = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                response_format = new
                {
                    type = "json_schema",
                    json_schema = new
                    {
                        name = "response",
                        schema = schema
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("chat/completions", request, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GroqResponse>(ct);
            var content = result?.Choices?.FirstOrDefault()?.Message?.Content 
                ?? throw new InvalidOperationException("Invalid response from Groq API");

            return JsonSerializer.Deserialize<T>(content) 
                ?? throw new InvalidOperationException("Failed to deserialize JSON response");
        }, cancellationToken);
    }

    private static object GenerateJsonSchema<T>()
    {
        // Use System.Text.Json.Nodes for schema generation
        // Or leverage NJsonSchema library
        // For simplicity, return a basic schema
        return new
        {
            type = "object",
            properties = new Dictionary<string, object>()
        };
    }

    private class GroqResponse
    {
        public List<Choice>? Choices { get; set; }

        public class Choice
        {
            public Message? Message { get; set; }
        }

        public class Message
        {
            public string? Content { get; set; }
        }
    }
}
```

### Step 5: Add Configuration (30 minutes)

**File:** `src/Chonkie.Genies/GroqGenieOptions.cs`

```csharp
namespace Chonkie.Genies;

/// <summary>
/// Configuration options for GroqGenie.
/// </summary>
public class GroqGenieOptions
{
    /// <summary>
    /// The Groq API key. If not provided, uses GROQ_API_KEY environment variable.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// The model to use for generation. Default is "llama-3.3-70b-versatile".
    /// </summary>
    public string Model { get; set; } = "llama-3.3-70b-versatile";

    /// <summary>
    /// HTTP timeout in seconds. Default is 30.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum retry attempts. Default is 5.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 5;
}
```

### Step 6: Add DI Extensions (30 minutes)

**File:** `src/Chonkie.Genies/Extensions/GenieServiceExtensions.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Chonkie.Genies.Extensions;

/// <summary>
/// Extension methods for registering Genies with dependency injection.
/// </summary>
public static class GenieServiceExtensions
{
    /// <summary>
    /// Adds GroqGenie to the service collection.
    /// </summary>
    public static IServiceCollection AddGroqGenie(
        this IServiceCollection services,
        Action<GroqGenieOptions>? configure = null)
    {
        if (configure != null)
        {
            services.Configure(configure);
        }

        services.AddHttpClient("Groq")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.All
            });

        services.AddSingleton<IGeneration, GroqGenie>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<GroqGenieOptions>>().Value;
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var logger = sp.GetRequiredService<ILogger<GroqGenie>>();
            return new GroqGenie(httpClientFactory, logger, options.ApiKey, options.Model);
        });

        return services;
    }

    /// <summary>
    /// Adds CerebrasGenie to the service collection.
    /// </summary>
    public static IServiceCollection AddCerebrasGenie(
        this IServiceCollection services,
        Action<CerebrasGenieOptions>? configure = null)
    {
        // Similar implementation for Cerebras
        return services;
    }
}
```

### Step 7: Write Tests (2-3 hours)

**File:** `tests/Chonkie.Genies.Tests/GroqGenieTests.cs`

```csharp
using Shouldly;
using NSubstitute;
using Xunit;

namespace Chonkie.Genies.Tests;

public class GroqGenieTests
{
    [Fact]
    public async Task GenerateAsync_ValidPrompt_ReturnsResponse()
    {
        // Arrange
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var logger = Substitute.For<ILogger<GroqGenie>>();
        var genie = new GroqGenie(httpClientFactory, logger);

        // Act
        var result = await genie.GenerateAsync("Hello");

        // Assert
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateAsync_EmptyPrompt_ThrowsArgumentException()
    {
        // Arrange
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var logger = Substitute.For<ILogger<GroqGenie>>();
        var genie = new GroqGenie(httpClientFactory, logger);

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(async () => 
            await genie.GenerateAsync(""));
    }

    // Add 10-15 more tests...
}
```

---

## 📋 Implementation Checklist

### GroqGenie (Day 1-3)
- [ ] Create Chonkie.Genies project
- [ ] Define IGeneration interface
- [ ] Implement BaseGenie with retry logic
- [ ] Implement GroqGenie class
- [ ] Add GroqGenieOptions
- [ ] Create DI extensions
- [ ] Write 10-15 unit tests
- [ ] Write 3-5 integration tests
- [ ] Add XML documentation
- [ ] Create sample usage

### CerebrasGenie (Day 4-5)
- [ ] Implement CerebrasGenie class
- [ ] Add CerebrasGenieOptions
- [ ] Update DI extensions
- [ ] Write 10-15 unit tests
- [ ] Write 3-5 integration tests
- [ ] Add XML documentation
- [ ] Create sample usage

### Additional Genies (Week 2)
- [ ] Implement OpenAIGenie
- [ ] Implement AzureOpenAIGenie
- [ ] Implement GeminiGenie
- [ ] Update documentation
- [ ] Integration testing
- [ ] Performance benchmarks

---

## 🎯 Testing Strategy

### Unit Tests (xUnit + NSubstitute + Shouldly)

```csharp
[Fact]
public async Task GenerateAsync_ValidPrompt_ReturnsResponse()
{
    // Arrange - Setup mocks and test data
    var httpClientFactory = Substitute.For<IHttpClientFactory>();
    var logger = Substitute.For<ILogger<GroqGenie>>();
    
    // Act - Execute the method
    var result = await genie.GenerateAsync("Test prompt");
    
    // Assert - Verify results using Shouldly
    result.ShouldNotBeNullOrEmpty();
}
```

### Integration Tests (Real API calls)

```csharp
[SkippableFact]
public async Task GenerateAsync_RealAPI_ReturnsResponse()
{
    Skip.IfNot(Environment.GetEnvironmentVariable("GROQ_API_KEY") != null, 
        "GROQ_API_KEY not set");
    
    // Test with real API
}
```

---

## 📚 Key Resources

### Internal Docs
- **MASTER_ROADMAP.md** - Complete roadmap
- **DEVELOPMENT_ROADMAP_FEB_2026.md** - Detailed implementation
- **PYTHON_CHANGES_FEBRUARY_2026.md** - Python reference
- **AGENTS.md** - C# coding guidelines

### External Resources
- **Groq API:** https://groq.com/docs
- **Cerebras API:** https://cerebras.ai/docs
- **Polly:** https://www.thepollyproject.org/
- **System.Text.Json:** https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json

---

## 💡 Pro Tips

### 1. Use Polly for Retries
```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 5,
        BackoffType = DelayBackoffType.Exponential
    })
    .Build();
```

### 2. Use HttpClientFactory
```csharp
services.AddHttpClient("Groq")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.All
    });
```

### 3. Test with [SkippableFact]
```csharp
[SkippableFact]
public async Task Integration_Test()
{
    Skip.IfNot(apiKeyPresent, "API key not set");
    // Test code
}
```

### 4. Follow C# 14 Patterns
```csharp
// Extension members
extension(IGeneration)
{
    public static string DefaultModel => "llama-3.3-70b-versatile";
}

// Null-conditional assignment
genie?.Options = newOptions;
```

### 5. Use Structured Logging
```csharp
_logger.LogInformation(
    "Generated response for model {Model} in {Duration}ms",
    _model,
    stopwatch.ElapsedMilliseconds);
```

---

## ⚡ Common Issues & Solutions

### Issue 1: API Key Not Found
**Solution:** Set environment variable or pass in options
```powershell
$env:GROQ_API_KEY = "your-key-here"
```

### Issue 2: HTTP Timeouts
**Solution:** Increase timeout in options
```csharp
services.AddGroqGenie(options =>
{
    options.TimeoutSeconds = 60;
});
```

### Issue 3: JSON Deserialization Errors
**Solution:** Use custom JsonSerializerOptions
```csharp
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
```

---

## 🚀 Next Steps

1. **Complete GroqGenie** (3-4 hours)
2. **Complete CerebrasGenie** (3-4 hours)
3. **Write comprehensive tests** (2-3 hours)
4. **Create sample project** (1 hour)
5. **Update documentation** (1 hour)
6. **Submit PR** (Review & merge)

**Total Time:** 10-15 hours for first implementation

---

## ✅ Definition of Done

### For Each Genie:
- [ ] Implementation complete
- [ ] All methods working
- [ ] 10+ unit tests passing
- [ ] 3+ integration tests (skippable)
- [ ] XML documentation complete
- [ ] Sample usage created
- [ ] DI extensions working
- [ ] Configuration options functional
- [ ] Error handling comprehensive
- [ ] Logging implemented

### For Phase 8 Complete:
- [ ] All 4-6 genies implemented
- [ ] Test coverage >80%
- [ ] All tests passing
- [ ] Documentation complete
- [ ] Samples runnable
- [ ] Performance acceptable
- [ ] Code review passed
- [ ] Merged to main

---

**Ready to Start?** → Begin with Step 1 above!  
**Need Help?** → Check DEVELOPMENT_ROADMAP_FEB_2026.md for details  
**Questions?** → Review MASTER_ROADMAP.md for context
