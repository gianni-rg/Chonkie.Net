using Shouldly;
using Xunit;

namespace Chonkie.Genies.Integration.Tests;

public class AzureOpenAIGenieIntegrationTests
{
    private static string? GetEndpoint() => Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
    private static string? GetApiKey() => Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
    private static string? GetDeployment() => Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_LLM");

    [Fact]
    public async Task GenerateAsync_WithValidPrompt_ShouldReturnResponse()
    {
        // Arrange
        var endpoint = GetEndpoint();
        var apiKey = GetApiKey();
        var deployment = GetDeployment();

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(deployment))
            Assert.Skip("Azure OpenAI environment variables not set (AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_API_KEY, AZURE_OPENAI_DEPLOYMENT_LLM)");

        var genie = new AzureOpenAIGenie(endpoint!, apiKey!, deployment!, null, null);
        var prompt = "Say 'Hello, Chonkie!' and nothing else.";

        // Act
        var response = await genie.GenerateAsync(prompt);

        // Assert
        response.ShouldNotBeNullOrWhiteSpace();
        response.ShouldContain("Chonkie", Case.Insensitive);
    }

    [Fact]
    public async Task GenerateJsonAsync_WithValidPrompt_ShouldReturnStructuredData()
    {
        // Arrange
        var endpoint = GetEndpoint();
        var apiKey = GetApiKey();
        var deployment = GetDeployment();

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(deployment))
            Assert.Skip("Azure OpenAI environment variables not set");

        var genie = new AzureOpenAIGenie(endpoint!, apiKey!, deployment!, null, null);
        var prompt = "Generate a person with name 'Alice Johnson' and age 28 in JSON format";

        // Act
        var response = await genie.GenerateJsonAsync<PersonData>(prompt);

        // Assert
        response.ShouldNotBeNull();
        response.Name.ShouldNotBeNullOrWhiteSpace();
        response.Age.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GenerateAsync_WithLongPrompt_ShouldHandleSuccessfully()
    {
        // Arrange
        var endpoint = GetEndpoint();
        var apiKey = GetApiKey();
        var deployment = GetDeployment();

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(deployment))
            Assert.Skip("Azure OpenAI environment variables not set");

        var genie = new AzureOpenAIGenie(endpoint!, apiKey!, deployment!, null, null);
        var prompt = "Explain the concept of semantic chunking in RAG systems in 2-3 sentences.";

        // Act
        var response = await genie.GenerateAsync(prompt);

        // Assert
        response.ShouldNotBeNullOrWhiteSpace();
        response.Length.ShouldBeGreaterThan(50);
    }

    [Fact]
    public async Task GenerateJsonAsync_WithComplexObject_ShouldDeserializeCorrectly()
    {
        // Arrange
        var endpoint = GetEndpoint();
        var apiKey = GetApiKey();
        var deployment = GetDeployment();

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(deployment))
            Assert.Skip("Azure OpenAI environment variables not set");

        var genie = new AzureOpenAIGenie(endpoint!, apiKey!, deployment!, null, null);
        var prompt = @"Generate a project with name, status (use 'Active'), 
                       and list of 3 team member names (as strings) in JSON format";

        // Act
        var response = await genie.GenerateJsonAsync<ProjectData>(prompt);

        // Assert
        response.ShouldNotBeNull();
        response.Name.ShouldNotBeNullOrWhiteSpace();
        response.Status.ShouldNotBeNullOrWhiteSpace();
        response.TeamMembers.ShouldNotBeNull();
        response.TeamMembers.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task FromEnvironment_WithValidCredentials_ShouldWork()
    {
        // Arrange
        var endpoint = GetEndpoint();
        var apiKey = GetApiKey();
        var deployment = GetDeployment();

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(deployment))
            Assert.Skip("Azure OpenAI environment variables not set");

        var genie = AzureOpenAIGenie.FromEnvironment();
        var prompt = "Say 'Test passed!' and nothing else.";

        // Act
        var response = await genie.GenerateAsync(prompt);

        // Assert
        response.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GenerateAsync_WithCustomApiVersion_ShouldWork()
    {
        // Arrange
        var endpoint = GetEndpoint();
        var apiKey = GetApiKey();
        var deployment = GetDeployment();

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(deployment))
            Assert.Skip("Azure OpenAI environment variables not set");

        // Using a specific API version
        var genie = new AzureOpenAIGenie(endpoint!, apiKey!, deployment!, "2024-10-21", null);
        var prompt = "Say 'API version test successful!' and nothing else.";

        // Act
        var response = await genie.GenerateAsync(prompt);

        // Assert
        response.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GenerateAsync_WithMultipleRequests_ShouldHandleConcurrency()
    {
        // Arrange
        var endpoint = GetEndpoint();
        var apiKey = GetApiKey();
        var deployment = GetDeployment();

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(deployment))
            Assert.Skip("Azure OpenAI environment variables not set");

        var genie = new AzureOpenAIGenie(endpoint!, apiKey!, deployment!, null, null);
        var prompts = new[]
        {
            "Say 'First' and nothing else.",
            "Say 'Second' and nothing else.",
            "Say 'Third' and nothing else."
        };

        // Act
        var tasks = prompts.Select(p => genie.GenerateAsync(p)).ToArray();
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Length.ShouldBe(3);
        responses.All(r => !string.IsNullOrWhiteSpace(r)).ShouldBeTrue();
    }

    private class PersonData
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private class ProjectData
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<string> TeamMembers { get; set; } = new();
    }
}
