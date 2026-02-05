using Shouldly;
using Xunit;

namespace Chonkie.Genies.Tests;

/// <summary>
/// Unit tests for GeminiGenie class.
/// </summary>
public class GeminiGenieTests
{
    [Fact]
    public void Constructor_WithNullApiKey_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new GeminiGenie(null!, null));
    }

    [Fact]
    public void Constructor_WithEmptyApiKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new GeminiGenie(string.Empty, null));
    }

    [Fact]
    public void Constructor_WithWhitespaceApiKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new GeminiGenie("   ", null));
    }

    [Fact]
    public void Constructor_WithValidApiKey_ShouldSucceed()
    {
        // Act
        var genie = new GeminiGenie("test-api-key", null);

        // Assert
        genie.ShouldNotBeNull();
        genie.ShouldBeAssignableTo<IGeneration>();
    }

    [Fact]
    public void Constructor_WithValidApiKeyAndDefaultModel_ShouldSucceed()
    {
        // Act
        var genie = new GeminiGenie("test-api-key", null);

        // Assert
        genie.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithCustomModel_ShouldSucceed()
    {
        // Act
        var genie = new GeminiGenie("test-api-key", "gemini-1.5-pro");

        // Assert
        genie.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithGemini15Flash_ShouldSucceed()
    {
        // Act
        var genie = new GeminiGenie("test-api-key", "gemini-1.5-flash");

        // Assert
        genie.ShouldNotBeNull();
    }

    [Fact]
    public void FromEnvironment_WithoutEnvironmentVariable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("GEMINI_API_KEY", null);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => GeminiGenie.FromEnvironment());
        exception.Message.ShouldContain("GEMINI_API_KEY");
    }

    [Fact]
    public void FromEnvironment_WithEnvironmentVariable_ShouldSucceed()
    {
        // Arrange
        var testKey = "test-gemini-key";
        Environment.SetEnvironmentVariable("GEMINI_API_KEY", testKey);

        try
        {
            // Act
            var genie = GeminiGenie.FromEnvironment();

            // Assert
            genie.ShouldNotBeNull();
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("GEMINI_API_KEY", null);
        }
    }

    [Fact]
    public void FromEnvironment_WithCustomModel_ShouldSucceed()
    {
        // Arrange
        Environment.SetEnvironmentVariable("GEMINI_API_KEY", "test-key");

        try
        {
            // Act
            var genie = GeminiGenie.FromEnvironment("gemini-1.5-pro");

            // Assert
            genie.ShouldNotBeNull();
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("GEMINI_API_KEY", null);
        }
    }

    [Fact]
    public async Task GenerateAsync_WithNullPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new GeminiGenie("test-api-key", null);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () => await genie.GenerateAsync(null!));
    }

    [Fact]
    public async Task GenerateAsync_WithEmptyPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new GeminiGenie("test-api-key", null);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () => await genie.GenerateAsync(string.Empty));
    }

    [Fact]
    public async Task GenerateJsonAsync_WithNullPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new GeminiGenie("test-api-key", null);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await genie.GenerateJsonAsync<TestData>(null!));
    }

    [Fact]
    public async Task GenerateJsonAsync_WithEmptyPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new GeminiGenie("test-api-key", null);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await genie.GenerateJsonAsync<TestData>(string.Empty));
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var genie = new GeminiGenie("test-api-key", "gemini-1.5-pro");

        // Act
        var result = genie.ToString();

        // Assert
        result.ShouldContain("GeminiGenie");
        result.ShouldContain("gemini-1.5-pro");
    }

    private class TestData
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }
}
