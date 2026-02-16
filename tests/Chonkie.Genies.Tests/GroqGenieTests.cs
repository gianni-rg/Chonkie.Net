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

public class GroqGenieTests
{
    [Fact]
    public void Constructor_WithNullApiKey_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new GroqGenie(null!, null, null));
    }

    [Fact]
    public void Constructor_WithEmptyApiKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new GroqGenie(string.Empty, null, null));
    }

    [Fact]
    public void Constructor_WithWhitespaceApiKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new GroqGenie("   ", null, null));
    }

    [Fact]
    public void Constructor_WithValidApiKey_ShouldSucceed()
    {
        // Act
        var genie = new GroqGenie("test-api-key", null, null);

        // Assert
        genie.ShouldNotBeNull();
        genie.ShouldBeAssignableTo<IGeneration>();
    }

    [Fact]
    public void Constructor_WithCustomModel_ShouldSucceed()
    {
        // Act
        var genie = new GroqGenie("test-api-key", "custom-model", null);

        // Assert
        genie.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithCustomEndpoint_ShouldSucceed()
    {
        // Act
        var genie = new GroqGenie("test-api-key", null, "https://custom.endpoint.com");

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
            Model = "test-model",
            Endpoint = new Uri("https://test.endpoint.com"),
            MaxRetries = 3,
            Temperature = 0.5f
        };

        // Act
        var genie = new GroqGenie(options);

        // Assert
        genie.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new GroqGenie((GenieOptions)null!));
    }

    [Fact]
    public void FromEnvironment_WithoutEnvironmentVariable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("GROQ_API_KEY", null);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => GroqGenie.FromEnvironment());
        exception.Message.ShouldContain("GROQ_API_KEY");
    }

    [Fact]
    public void FromEnvironment_WithEnvironmentVariable_ShouldSucceed()
    {
        // Arrange
        var testKey = "test-groq-key";
        Environment.SetEnvironmentVariable("GROQ_API_KEY", testKey);

        try
        {
            // Act
            var genie = GroqGenie.FromEnvironment();

            // Assert
            genie.ShouldNotBeNull();
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("GROQ_API_KEY", null);
        }
    }

    [Fact]
    public async Task GenerateAsync_WithNullPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new GroqGenie("test-api-key", null, null);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () => await genie.GenerateAsync(null!));
    }

    [Fact]
    public async Task GenerateAsync_WithEmptyPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new GroqGenie("test-api-key", null, null);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () => await genie.GenerateAsync(string.Empty));
    }

    [Fact]
    public async Task GenerateJsonAsync_WithNullPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new GroqGenie("test-api-key", null, null);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await genie.GenerateJsonAsync<TestData>(null!));
    }

    [Fact]
    public async Task GenerateJsonAsync_WithEmptyPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new GroqGenie("test-api-key", null, null);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await genie.GenerateJsonAsync<TestData>(string.Empty));
    }

    private class TestData
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }
}
