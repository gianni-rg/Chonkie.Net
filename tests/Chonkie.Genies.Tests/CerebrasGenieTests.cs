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

public class CerebrasGenieTests
{
    [Fact]
    public void Constructor_WithNullApiKey_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new CerebrasGenie(null!, null, null));
    }

    [Fact]
    public void Constructor_WithEmptyApiKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new CerebrasGenie(string.Empty, null, null));
    }

    [Fact]
    public void Constructor_WithWhitespaceApiKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new CerebrasGenie("   ", null, null));
    }

    [Fact]
    public void Constructor_WithValidApiKey_ShouldSucceed()
    {
        // Act
        var genie = new CerebrasGenie("test-api-key", null, null);

        // Assert
        genie.ShouldNotBeNull();
        genie.ShouldBeAssignableTo<IGeneration>();
    }

    [Fact]
    public void Constructor_WithCustomModel_ShouldSucceed()
    {
        // Act
        var genie = new CerebrasGenie("test-api-key", "custom-model", null);

        // Assert
        genie.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithCustomEndpoint_ShouldSucceed()
    {
        // Act
        var genie = new CerebrasGenie("test-api-key", null, "https://custom.endpoint.com");

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
        var genie = new CerebrasGenie(options);

        // Assert
        genie.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new CerebrasGenie((GenieOptions)null!));
    }

    [Fact]
    public void FromEnvironment_WithoutEnvironmentVariable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("CEREBRAS_API_KEY", null);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => CerebrasGenie.FromEnvironment());
        exception.Message.ShouldContain("CEREBRAS_API_KEY");
    }

    [Fact]
    public void FromEnvironment_WithEnvironmentVariable_ShouldSucceed()
    {
        // Arrange
        var testKey = "test-cerebras-key";
        Environment.SetEnvironmentVariable("CEREBRAS_API_KEY", testKey);

        try
        {
            // Act
            var genie = CerebrasGenie.FromEnvironment();

            // Assert
            genie.ShouldNotBeNull();
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("CEREBRAS_API_KEY", null);
        }
    }

    [Fact]
    public async Task GenerateAsync_WithNullPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new CerebrasGenie("test-api-key", null, null);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () => await genie.GenerateAsync(null!));
    }

    [Fact]
    public async Task GenerateAsync_WithEmptyPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new CerebrasGenie("test-api-key", null, null);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () => await genie.GenerateAsync(string.Empty));
    }

    [Fact]
    public async Task GenerateJsonAsync_WithNullPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new CerebrasGenie("test-api-key", null, null);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await genie.GenerateJsonAsync<TestData>(null!));
    }

    [Fact]
    public async Task GenerateJsonAsync_WithEmptyPrompt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var genie = new CerebrasGenie("test-api-key", null, null);

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
