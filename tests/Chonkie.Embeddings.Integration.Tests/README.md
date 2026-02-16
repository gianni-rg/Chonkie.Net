# Chonkie.Embeddings Integration Tests

This project contains integration tests for the Chonkie embeddings providers.
These tests make real API calls to external services and require proper
credentials to run.

## Purpose

Integration tests verify that the embeddings providers work correctly with
real APIs and services. Unlike unit tests, these tests:

- Make actual HTTP requests to embedding APIs
- Require valid API keys and credentials
- May incur costs depending on the provider
- Take longer to execute
- Can be flaky due to network issues or service availability

## Running the Tests

### Prerequisites

Before running integration tests, you need to set up environment
variables for the providers you want to test:

#### OpenAI

```powershell
$env:OPENAI_API_KEY = "your-openai-api-key"
```

#### Azure OpenAI

```powershell
$env:AZURE_OPENAI_API_KEY = "your-azure-openai-api-key"
$env:AZURE_OPENAI_ENDPOINT = "https://your-resource.openai.azure.com/"
$env:AZURE_OPENAI_DEPLOYMENT_EMBEDDINGS = "your-deployment-name"
```

#### Cohere

```powershell
$env:COHERE_API_KEY = "your-cohere-api-key"
```

#### Gemini

```powershell
$env:GEMINI_API_KEY = "your-gemini-api-key"
```

#### Jina AI

```powershell
$env:JINA_API_KEY = "your-jina-api-key"
```

#### Voyage AI

```powershell
$env:VOYAGE_API_KEY = "your-voyage-api-key"
```

#### Sentence Transformers (ONNX)

```powershell
$env:CHONKIE_SENTENCE_TRANSFORMERS_MODEL_PATH = "C:\path\to\model.onnx"
```

### Running All Tests

```powershell
# From the solution root
dotnet test tests\Chonkie.Embeddings.Integration.Tests\Chonkie.Embeddings.Integration.Tests.csproj
```

### Running Specific Provider Tests

```powershell
# Run only OpenAI tests
dotnet test tests\Chonkie.Embeddings.Integration.Tests\
Chonkie.Embeddings.Integration.Tests.csproj \
--filter "FullyQualifiedName~OpenAI"

# Run only Azure OpenAI tests
dotnet test tests\Chonkie.Embeddings.Integration.Tests\
Chonkie.Embeddings.Integration.Tests.csproj \
--filter "FullyQualifiedName~AzureOpenAI"

# Run only Cohere tests
dotnet test tests\Chonkie.Embeddings.Integration.Tests\
Chonkie.Embeddings.Integration.Tests.csproj \
--filter "FullyQualifiedName~Cohere"
```

### Skipping Tests

Tests will automatically skip if required environment variables are
not set. You'll see output like:

```text
[SKIP] OpenAIEmbeddingsIntegrationTests.EmbedAsync_WithRealAPI_ReturnsValidEmbedding
  Reason: Environment variable OPENAI_API_KEY not set. Skipping integration test.
```

## Test Categories

Each provider has integration tests covering:

1. **Basic Embedding Generation**: Verify that embeddings can be
   generated for simple text
2. **Custom Model Support**: Test using different models offered by
   the provider
3. **Batch Processing**: Verify batch embedding generation works
   correctly
4. **Similarity Testing**: Ensure similar texts produce similar
   embeddings
5. **Edge Cases**: Test empty strings, long texts, special
   characters, etc.
6. **Provider-Specific Features**: Test unique features like input
   types, truncation options, etc.

## Best Practices

1. **Don't commit API keys**: Never commit environment variables or
   API keys to source control
2. **Use test accounts**: Use separate test accounts/projects for
   integration testing when possible
3. **Monitor costs**: Be aware that these tests make real API calls
   that may incur costs
4. **Run selectively**: You don't need to run all integration tests
   every time - focus on what you're working on
5. **CI/CD considerations**: In CI/CD pipelines, store credentials
   securely using secret management systems
6. **Rate limiting**: Be mindful of API rate limits - you may need
   to add delays between tests

## Troubleshooting

### Tests are skipped

- Verify environment variables are set correctly
- Check that variable names match exactly (they're case-sensitive)
- For Sentence Transformers, verify the ONNX model file exists at
  the specified path

### Tests fail with authentication errors

- Verify API keys are valid and not expired
- Check that you have sufficient credits/quota with the provider
- For Azure, ensure the endpoint and deployment name are correct

### Tests fail with network errors

- Check your internet connection
- Verify the provider's API service is available (check status pages)
- Some providers may have regional restrictions

### Tests are slow

- This is expected for integration tests making real API calls
- Consider running specific test subsets instead of all tests
- Network latency and provider response times vary

## Contributing

When adding new integration tests:

1. Follow the existing pattern of using `TestHelpers.GetEnvironmentVariableOrSkip()`
2. Document required environment variables in this README
3. Include tests for both success and edge cases
4. Consider adding provider-specific feature tests
5. Ensure tests are idempotent and can run in any order

## Related Documentation

- [Unit Tests](../Chonkie.Embeddings.Tests/README.md) - Fast,
  isolated unit tests
- [Test Coverage Comparison](../../docs/TEST_COVERAGE_EMBEDDINGS.md)
  - Comparison with Python implementation
- [Embeddings Documentation](../../src/Chonkie.Embeddings/README.md)
  - Full embeddings documentation
