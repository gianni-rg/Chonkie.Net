# API Reference - Genies
**Scope:** LLM generation interfaces, options, and providers.

## Python Reference
- [chonkie/docs/oss/quick-start.mdx](chonkie/docs/oss/quick-start.mdx)

## Chonkie.Genies

### IGeneration
Interface for text and JSON generation.

Members:
- Methods: `Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)`, `Task<T> GenerateJsonAsync<T>(string prompt, CancellationToken cancellationToken = default) where T : class`

### BaseGenie
Abstract base class implementing retry logic and JSON parsing.

Members:
- Constructors: `protected BaseGenie(IChatClient chatClient, GenieOptions options, ILogger? logger = null)`
- Properties (protected): `GenieOptions Options { get; }`, `ILogger Logger { get; }`
- Methods: `virtual Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)`, `virtual Task<T> GenerateJsonAsync<T>(string prompt, CancellationToken cancellationToken = default) where T : class`
- Methods (protected): `Task<T> ExecuteWithRetryAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)`

### GenieOptions
Configuration options for genie providers.

Members:
- Properties: `string ApiKey { get; set; }`, `string Model { get; set; }`, `Uri? Endpoint { get; set; }`, `int MaxRetries { get; set; }`, `int MaxRetryDelaySeconds { get; set; }`, `int TimeoutSeconds { get; set; }`, `float Temperature { get; set; }`, `int MaxTokens { get; set; }`

### GenieException
Base exception type for genie failures.

Members:
- Constructors: `GenieException()`, `GenieException(string message)`, `GenieException(string message, Exception innerException)`

### RateLimitException
Rate limit error.

Members:
- Constructors: `RateLimitException()`, `RateLimitException(string message)`, `RateLimitException(string message, Exception innerException)`

### AuthenticationException
Authentication error.

Members:
- Constructors: `AuthenticationException()`, `AuthenticationException(string message)`, `AuthenticationException(string message, Exception innerException)`

### JsonParsingException
JSON parsing error.

Members:
- Constructors: `JsonParsingException()`, `JsonParsingException(string message)`, `JsonParsingException(string message, Exception innerException)`

### OpenAIGenie
OpenAI provider (OpenAI-compatible).

Members:
- Constructors: `OpenAIGenie(string apiKey, string? model = null, string? baseUrl = null, ILogger<OpenAIGenie>? logger = null)`, `OpenAIGenie(GenieOptions options, ILogger<OpenAIGenie>? logger = null)`
- Methods: `static OpenAIGenie FromEnvironment(string? model = null, ILogger<OpenAIGenie>? logger = null)`, `override string ToString()`

### AzureOpenAIGenie
Azure OpenAI provider.

Members:
- Constructors: `AzureOpenAIGenie(string endpoint, string apiKey, string deploymentName, string? apiVersion = null, ILogger<AzureOpenAIGenie>? logger = null)`
- Methods: `static AzureOpenAIGenie FromEnvironment(ILogger<AzureOpenAIGenie>? logger = null)`, `override string ToString()`

### GeminiGenie
Google Gemini provider.

Members:
- Constructors: `GeminiGenie(string apiKey, string? model = null, ILogger<GeminiGenie>? logger = null)`
- Methods: `static GeminiGenie FromEnvironment(string? model = null, ILogger<GeminiGenie>? logger = null)`, `override string ToString()`

### GroqGenie
Groq provider (OpenAI-compatible endpoint).

Members:
- Constructors: `GroqGenie(string apiKey, string? model, string? endpoint, ILogger<GroqGenie>? logger = null)`, `GroqGenie(GenieOptions options, ILogger<GroqGenie>? logger = null)`
- Methods: `static GroqGenie FromEnvironment(string? model = null, ILogger<GroqGenie>? logger = null)`

### CerebrasGenie
Cerebras provider (OpenAI-compatible endpoint).

Members:
- Constructors: `CerebrasGenie(string apiKey, string? model, string? endpoint, ILogger<CerebrasGenie>? logger = null)`, `CerebrasGenie(GenieOptions options, ILogger<CerebrasGenie>? logger = null)`
- Methods: `static CerebrasGenie FromEnvironment(string? model = null, ILogger<CerebrasGenie>? logger = null)`

## Chonkie.Genies.Extensions

### GenieServiceExtensions
DI registration helpers.

Members:
- Methods: `static IServiceCollection AddGroqGenie(this IServiceCollection services, string apiKey, string? model = null)`, `static IServiceCollection AddGroqGenie(this IServiceCollection services, GenieOptions options)`, `static IServiceCollection AddGroqGenieFromEnvironment(this IServiceCollection services, string? model = null)`, `static IServiceCollection AddCerebrasGenie(this IServiceCollection services, string apiKey, string? model = null)`, `static IServiceCollection AddCerebrasGenie(this IServiceCollection services, GenieOptions options)`, `static IServiceCollection AddCerebrasGenieFromEnvironment(this IServiceCollection services, string? model = null)`
