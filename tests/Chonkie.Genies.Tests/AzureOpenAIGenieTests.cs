// Copyright 2025-2026 Gianni Rosa Gallina and Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Shouldly;
using Xunit;

namespace Chonkie.Genies.Tests;

/// <summary>
/// Unit tests for AzureOpenAIGenie class.
/// Tests are skipped if Azure.AI.OpenAI SDK cannot be loaded.
/// </summary>
public class AzureOpenAIGenieTests
{
    private const string TestEndpoint = "https://test.openai.azure.com";
    private const string TestApiKey = "test-api-key";
    private const string TestDeployment = "gpt-4o";

    private static bool CanCreateAzureOpenAIGenie()
    {
        try
        {
            // Try to create a simple instance to verify the Azure SDK works
            // Note: This will throw exceptions during validation, which means SDK is working
            // We're only interested in TypeLoadException which means SDK is broken
            _ = new AzureOpenAIGenie(TestEndpoint, TestApiKey, TestDeployment);
            return true;
        }
        catch (TypeLoadException ex)
        {
            // Azure SDK has a known compatibility issue - RealtimeConversationClient type not found
            // This occurs in Azure.AI.OpenAI beta versions when internally trying to load RealtimeConversationClient
            if (ex.Message.Contains("RealtimeConversationClient") || ex.Message.Contains("OpenAI.RealtimeConversation"))
            {
                return false;
            }
            // If it's a different TypeLoadException, it's still a problem
            return false;
        }
        catch
        {
            // Other exceptions (validation, argument, etc.) mean SDK is working
            // We can create instances, just might fail for other reasons
            return true;
        }
    }

    private static void SkipIfAzureNotAvailable()
    {
        if (!CanCreateAzureOpenAIGenie())
        {
            Assert.Skip("Azure.AI.OpenAI SDK is not compatible on this system. Skipping test.");
        }
    }

    [Fact]
    public void Constructor_WithNullEndpoint_ShouldThrowArgumentException()
    {
        SkipIfAzureNotAvailable();

        // Act & Assert
        Should.Throw<ArgumentException>(() => new AzureOpenAIGenie(null!, TestApiKey, TestDeployment));
    }

    [Fact]
    public void Constructor_WithEmptyEndpoint_ShouldThrowException()
    {
        SkipIfAzureNotAvailable();

        // Act & Assert
        // Note: Throws UriFormatException because Uri constructor is called before string validation
        Should.Throw<Exception>(() => new AzureOpenAIGenie(string.Empty, TestApiKey, TestDeployment));
    }

    [Fact]
    public void Constructor_WithWhitespaceEndpoint_ShouldThrowException()
    {
        SkipIfAzureNotAvailable();

        // Act & Assert
        // Note: Throws UriFormatException because Uri constructor is called before string validation
        Should.Throw<Exception>(() => new AzureOpenAIGenie("   ", TestApiKey, TestDeployment));
    }

    [Fact]
    public void Constructor_WithNullApiKey_ShouldThrowArgumentException()
    {
        // Act & Assert (validation happens before SDK instantiation)
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
        // Act & Assert (validation happens before SDK instantiation)
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
        // Act & Assert (validation happens before SDK instantiation)
        Should.Throw<ArgumentException>(() => new AzureOpenAIGenie(TestEndpoint, TestApiKey, "   "));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldSucceed()
    {
        SkipIfAzureNotAvailable();

        // Act
        var genie = new AzureOpenAIGenie(TestEndpoint, TestApiKey, TestDeployment);

        // Assert
        genie.ShouldNotBeNull();
        genie.ShouldBeAssignableTo<IGeneration>();
    }

    [Fact]
    public void Constructor_WithCustomApiVersion_ShouldSucceed()
    {
        SkipIfAzureNotAvailable();

        // Act
        var genie = new AzureOpenAIGenie(TestEndpoint, TestApiKey, TestDeployment, "2024-08-01-preview");

        // Assert
        genie.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldSucceed()
    {
        SkipIfAzureNotAvailable();

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
        Environment.SetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_LLM", TestDeployment);

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
        Environment.SetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_LLM", TestDeployment);

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
        Environment.SetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_LLM", null);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => AzureOpenAIGenie.FromEnvironment());
        exception.Message.ShouldContain("AZURE_OPENAI_DEPLOYMENT_LLM");
    }

    [Fact]
    public void FromEnvironment_WithAllEnvironmentVariables_ShouldSucceed()
    {
        SkipIfAzureNotAvailable();

        // Arrange
        Environment.SetEnvironmentVariable("AZURE_OPENAI_ENDPOINT", TestEndpoint);
        Environment.SetEnvironmentVariable("AZURE_OPENAI_API_KEY", TestApiKey);
        Environment.SetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_LLM", TestDeployment);

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
            Environment.SetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_LLM", null);
        }
    }

    [Fact]
    public async Task GenerateAsync_WithNullPrompt_ShouldThrowArgumentNullException()
    {
        SkipIfAzureNotAvailable();

        // Arrange
        var genie = new AzureOpenAIGenie(TestEndpoint, TestApiKey, TestDeployment);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () => await genie.GenerateAsync(null!));
    }

    [Fact]
    public async Task GenerateAsync_WithEmptyPrompt_ShouldThrowArgumentNullException()
    {
        SkipIfAzureNotAvailable();

        // Arrange
        var genie = new AzureOpenAIGenie(TestEndpoint, TestApiKey, TestDeployment);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () => await genie.GenerateAsync(string.Empty));
    }

    [Fact]
    public async Task GenerateJsonAsync_WithNullPrompt_ShouldThrowArgumentNullException()
    {
        SkipIfAzureNotAvailable();

        // Arrange
        var genie = new AzureOpenAIGenie(TestEndpoint, TestApiKey, TestDeployment);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await genie.GenerateJsonAsync<TestData>(null!));
    }

    [Fact]
    public async Task GenerateJsonAsync_WithEmptyPrompt_ShouldThrowArgumentNullException()
    {
        SkipIfAzureNotAvailable();

        // Arrange
        var genie = new AzureOpenAIGenie(TestEndpoint, TestApiKey, TestDeployment);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await genie.GenerateJsonAsync<TestData>(string.Empty));
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        SkipIfAzureNotAvailable();

        // Arrange
        var genie = new AzureOpenAIGenie(TestEndpoint, TestApiKey, TestDeployment);
        var sample = new TestData();

        // Act
        var result = genie.ToString();

        // Assert
        result.ShouldContain("AzureOpenAIGenie");
        result.ShouldContain(TestDeployment);
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
