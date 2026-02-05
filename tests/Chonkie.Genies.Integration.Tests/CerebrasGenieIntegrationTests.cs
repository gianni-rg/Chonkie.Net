using Shouldly;
using Xunit;

namespace Chonkie.Genies.Integration.Tests;

public class CerebrasGenieIntegrationTests
{
    private static string? GetApiKey() => Environment.GetEnvironmentVariable("CEREBRAS_API_KEY");

    
    public async Task GenerateAsync_WithValidPrompt_ShouldReturnResponse()
    {
        // Arrange
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            Assert.Skip("CEREBRAS_API_KEY environment variable not set");

        var genie = new CerebrasGenie(apiKey!, null, null, null);
        var prompt = "Say 'Hello, Chonkie!' and nothing else.";

        // Act
        var response = await genie.GenerateAsync(prompt);

        // Assert
        response.ShouldNotBeNullOrWhiteSpace();
        response.ShouldContain("Chonkie", Case.Insensitive);
    }

    
    public async Task GenerateJsonAsync_WithValidPrompt_ShouldReturnStructuredData()
    {
        // Arrange
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            Assert.Skip("CEREBRAS_API_KEY environment variable not set");

        var genie = new CerebrasGenie(apiKey!, null, null, null);
        var prompt = "Generate a person with name 'Jane Smith' and age 25 in JSON format";

        // Act
        var response = await genie.GenerateJsonAsync<PersonData>(prompt);

        // Assert
        response.ShouldNotBeNull();
        response.Name.ShouldNotBeNullOrWhiteSpace();
        response.Age.ShouldBeGreaterThan(0);
    }

    
    public async Task GenerateAsync_WithLongPrompt_ShouldHandleSuccessfully()
    {
        // Arrange
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            Assert.Skip("CEREBRAS_API_KEY environment variable not set");

        var genie = new CerebrasGenie(apiKey!, null, null, null);
        var prompt = "Explain the benefits of fast inference in AI applications in 2-3 sentences.";

        // Act
        var response = await genie.GenerateAsync(prompt);

        // Assert
        response.ShouldNotBeNullOrWhiteSpace();
        response.Length.ShouldBeGreaterThan(50);
    }

    
    public async Task GenerateJsonAsync_WithComplexObject_ShouldDeserializeCorrectly()
    {
        // Arrange
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            Assert.Skip("CEREBRAS_API_KEY environment variable not set");

        var genie = new CerebrasGenie(apiKey!, null, null, null);
        var prompt = @"Generate a product with name, price (use 99.99), 
                       and list of 3 features in JSON format";

        // Act
        var response = await genie.GenerateJsonAsync<ProductData>(prompt);

        // Assert
        response.ShouldNotBeNull();
        response.Name.ShouldNotBeNullOrWhiteSpace();
        response.Price.ShouldBeGreaterThan(0);
        response.Features.ShouldNotBeNull();
        response.Features.Count.ShouldBeGreaterThan(0);
    }

    
    public async Task FromEnvironment_WithValidKey_ShouldWork()
    {
        // Arrange
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            Assert.Skip("CEREBRAS_API_KEY environment variable not set");

        var genie = CerebrasGenie.FromEnvironment();
        var prompt = "Say 'Test passed!' and nothing else.";

        // Act
        var response = await genie.GenerateAsync(prompt);

        // Assert
        response.ShouldNotBeNullOrWhiteSpace();
    }

    
    public async Task GenerateAsync_WithCustomModel_ShouldWork()
    {
        // Arrange
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            Assert.Skip("CEREBRAS_API_KEY environment variable not set");

        // Using the default model explicitly
        var genie = new CerebrasGenie(apiKey!, "llama-3.3-70b", null, null);
        var prompt = "Say 'Model test successful!' and nothing else.";

        // Act
        var response = await genie.GenerateAsync(prompt);

        // Assert
        response.ShouldNotBeNullOrWhiteSpace();
    }

    private class PersonData
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private class ProductData
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public List<string> Features { get; set; } = new();
    }
}
