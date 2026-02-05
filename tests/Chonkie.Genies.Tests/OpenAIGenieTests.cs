using Shouldly;
using Xunit;

namespace Chonkie.Genies.Tests;

/// <summary>
/// Unit tests for OpenAIGenie class.
/// </summary>
public class OpenAIGenieTests
{
    [Fact]
    public void Constructor_WithNullApiKey_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new OpenAIGenie(null!, null, null));
    }

    [Fact]
    public void Constructor_WithEmptyApiKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new OpenAIGenie(string.Empty, null, null));
    }

    [Fact]
    public void Constructor_WithWhitespaceApiKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new OpenAIGenie("   ", null, null));
    }

    [Fact]
    public void Constructor_WithValidApiKey_ShouldSucceed()
    {
        // Act
        var genie = new OpenAIGenie("test-api-key", null, null);

        // Assert
        genie.ShouldNotBeNull();
        genie.ShouldBeAssignableTo<IGeneration>();
    }

    [Fact]
    public void Constructor_WithCustomModel_ShouldSucceed()
    {
        // Act
        var genie = new OpenAIGenie("test-api-key", "gpt-4-turbo", null);

        // Assert
        genie.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithCustomBaseUrl_ShouldSucceed()
    {
        // Act
        var genie = new OpenAIGenie("test-api-key", null, "https://custom.endpoint.com");

        // Assert
        genie.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithOptions_ShouldSucceed()
    {
        // Arrange
        var options = new GenieOptions
        {
            ApiKey = "test-api-key",
            Model = "gpt-3.5-turbo",
            Endpoint = new Uri("https://custom.endpoint.com"),
            MaxRetries = 5,
            Temperature = 0.7f
        };

        // Act
        var genie = new OpenAIGenie(options);

        // Assert
        genie.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new OpenAIGenie((GenieOptions)null!));
    }

    [Fact]
    public void Constructor_WithOptionsHavingNullApiKey_ShouldThrowArgumentNullException()
    {
        // Arrange
        var options = new GenieOptions { ApiKey = null!, Model = "gpt-4o" };

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new OpenAIGenie(options));
    }

    [Fact]
    public void Constructor_WithOptionsHavingEmptyApiKey_ShouldThrowArgumentException()
    {
        // Arrange
        var options = new GenieOptions { ApiKey = string.Empty, Model = "gpt-4o" };

        // Act & Assert
        Should.Throw<ArgumentException>(() => new OpenAIGenie(options));
    }

    [Fact]
    public void FromEnvironment_WithoutEnvironmentVariable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", null);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => OpenAIGenie.FromEnvironment());
        exception.Message.ShouldContain("OPENAI_API_KEY");
    }

    [Fact]
    public void FromEnvironment_WithEnvironmentVariable_ShouldSucceed()
    {
        // Arrange
        var testKey = "test-openai-key";
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", testKey);

        try
        {
            // Act
            var genie = OpenAIGenie.FromEnvironment();

            // Assert
            genie.ShouldNotBeNull();
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", null);
        }
    }

    [Fact]
    public void FromEnvironment_WithCustomModel_ShouldSucceed()
    {
        // Arrange
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", "test-key");

        try
        {
            // Act
            var genie = OpenAIGenie.FromEnvironment("gpt-4-turbo");

            // Assert
            genie.ShouldNotBeNull();
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", null);
        }
    }

    [Fact]
    public async Task GenerateAsync_WithNullPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new OpenAIGenie("test-api-key", null, null);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () => await genie.GenerateAsync(null!));
    }

    [Fact]
    public async Task GenerateAsync_WithEmptyPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new OpenAIGenie("test-api-key", null, null);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () => await genie.GenerateAsync(string.Empty));
    }

    [Fact]
    public async Task GenerateJsonAsync_WithNullPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new OpenAIGenie("test-api-key", null, null);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await genie.GenerateJsonAsync<TestData>(null!));
    }

    [Fact]
    public async Task GenerateJsonAsync_WithEmptyPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new OpenAIGenie("test-api-key", null, null);
        var sample = new TestData();

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await genie.GenerateJsonAsync<TestData>(string.Empty));

        sample.ToString().ShouldContain("sample");
    }

    private class TestData
    {
        public string? Name { get; }
        public int Value { get; }

        public TestData(string? name = "sample", int value = 1)
        {
            Name = name;
            Value = value;
        }

        public override string ToString() => $"{Name}:{Value}";
    }
}
