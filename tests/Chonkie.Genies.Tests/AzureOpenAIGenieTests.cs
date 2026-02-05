using Shouldly;
using Xunit;

namespace Chonkie.Genies.Tests;

/// <summary>
/// Unit tests for AzureOpenAIGenie class.
/// </summary>
public class AzureOpenAIGenieTests
{
    private const string TestEndpoint = "https://test.openai.azure.com";
    private const string TestApiKey = "test-api-key";
    private const string TestDeployment = "gpt-4o";

    [Fact]
    public void Constructor_WithNullEndpoint_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new AzureOpenAIGenie(null!, TestApiKey, TestDeployment));
    }

    [Fact]
    public void Constructor_WithEmptyEndpoint_ShouldThrowException()
    {
        // Act & Assert
        // Note: Throws UriFormatException because Uri constructor is called before string validation
        Should.Throw<Exception>(() => new AzureOpenAIGenie(string.Empty, TestApiKey, TestDeployment));
    }

    [Fact]
    public void Constructor_WithWhitespaceEndpoint_ShouldThrowException()
    {
        // Act & Assert
        // Note: Throws UriFormatException because Uri constructor is called before string validation
        Should.Throw<Exception>(() => new AzureOpenAIGenie("   ", TestApiKey, TestDeployment));
    }

    [Fact]
    public void Constructor_WithNullApiKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new AzureOpenAIGenie(TestEndpoint, null!, TestDeployment));
    }

    [Fact]
    public void Constructor_WithEmptyApiKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new AzureOpenAIGenie(TestEndpoint, string.Empty, TestDeployment));
    }

    [Fact]
    public void Constructor_WithWhitespaceApiKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new AzureOpenAIGenie(TestEndpoint, "   ", TestDeployment));
    }

    [Fact]
    public void Constructor_WithNullDeployment_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new AzureOpenAIGenie(TestEndpoint, TestApiKey, null!));
    }

    [Fact]
    public void Constructor_WithEmptyDeployment_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new AzureOpenAIGenie(TestEndpoint, TestApiKey, string.Empty));
    }

    [Fact]
    public void Constructor_WithWhitespaceDeployment_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new AzureOpenAIGenie(TestEndpoint, TestApiKey, "   "));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldSucceed()
    {
        // Act
        var genie = new AzureOpenAIGenie(TestEndpoint, TestApiKey, TestDeployment);

        // Assert
        genie.ShouldNotBeNull();
        genie.ShouldBeAssignableTo<IGeneration>();
    }

    [Fact]
    public void Constructor_WithCustomApiVersion_ShouldSucceed()
    {
        // Act
        var genie = new AzureOpenAIGenie(TestEndpoint, TestApiKey, TestDeployment, "2024-08-01-preview");

        // Assert
        genie.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldSucceed()
    {
        // Arrange
        const string customEndpoint = "https://custom.openai.azure.com";
        const string customDeployment = "gpt-4-turbo";

        // Act
        var genie = new AzureOpenAIGenie(customEndpoint, TestApiKey, customDeployment, "2024-10-21");

        // Assert
        genie.ShouldNotBeNull();
    }

    [Fact]
    public void FromEnvironment_WithoutEndpointVariable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_OPENAI_ENDPOINT", null);
        Environment.SetEnvironmentVariable("AZURE_OPENAI_API_KEY", TestApiKey);
        Environment.SetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT", TestDeployment);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => AzureOpenAIGenie.FromEnvironment());
        exception.Message.ShouldContain("AZURE_OPENAI_ENDPOINT");
    }

    [Fact]
    public void FromEnvironment_WithoutApiKeyVariable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_OPENAI_ENDPOINT", TestEndpoint);
        Environment.SetEnvironmentVariable("AZURE_OPENAI_API_KEY", null);
        Environment.SetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT", TestDeployment);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => AzureOpenAIGenie.FromEnvironment());
        exception.Message.ShouldContain("AZURE_OPENAI_API_KEY");
    }

    [Fact]
    public void FromEnvironment_WithoutDeploymentVariable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_OPENAI_ENDPOINT", TestEndpoint);
        Environment.SetEnvironmentVariable("AZURE_OPENAI_API_KEY", TestApiKey);
        Environment.SetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT", null);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => AzureOpenAIGenie.FromEnvironment());
        exception.Message.ShouldContain("AZURE_OPENAI_DEPLOYMENT");
    }

    [Fact]
    public void FromEnvironment_WithAllEnvironmentVariables_ShouldSucceed()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_OPENAI_ENDPOINT", TestEndpoint);
        Environment.SetEnvironmentVariable("AZURE_OPENAI_API_KEY", TestApiKey);
        Environment.SetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT", TestDeployment);

        try
        {
            // Act
            var genie = AzureOpenAIGenie.FromEnvironment();

            // Assert
            genie.ShouldNotBeNull();
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("AZURE_OPENAI_ENDPOINT", null);
            Environment.SetEnvironmentVariable("AZURE_OPENAI_API_KEY", null);
            Environment.SetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT", null);
        }
    }

    [Fact]
    public async Task GenerateAsync_WithNullPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new AzureOpenAIGenie(TestEndpoint, TestApiKey, TestDeployment);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () => await genie.GenerateAsync(null!));
    }

    [Fact]
    public async Task GenerateAsync_WithEmptyPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new AzureOpenAIGenie(TestEndpoint, TestApiKey, TestDeployment);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () => await genie.GenerateAsync(string.Empty));
    }

    [Fact]
    public async Task GenerateJsonAsync_WithNullPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new AzureOpenAIGenie(TestEndpoint, TestApiKey, TestDeployment);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await genie.GenerateJsonAsync<TestData>(null!));
    }

    [Fact]
    public async Task GenerateJsonAsync_WithEmptyPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new AzureOpenAIGenie(TestEndpoint, TestApiKey, TestDeployment);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await genie.GenerateJsonAsync<TestData>(string.Empty));
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var genie = new AzureOpenAIGenie(TestEndpoint, TestApiKey, TestDeployment);

        // Act
        var result = genie.ToString();

        // Assert
        result.ShouldContain("AzureOpenAIGenie");
        result.ShouldContain(TestDeployment);
    }

    private class TestData
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }
}
