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

namespace Chonkie.Genies.Integration.Tests;

public class GroqGenieIntegrationTests
{
    private static string? GetApiKey() => Environment.GetEnvironmentVariable("GROQ_API_KEY");

    [Fact]
    public async Task GenerateAsync_WithValidPrompt_ShouldReturnResponse()
    {
        // Arrange
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            Assert.Skip("GROQ_API_KEY environment variable not set");

        var genie = new GroqGenie(apiKey!, null, null, null);
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
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            Assert.Skip("GROQ_API_KEY environment variable not set");

        var genie = new GroqGenie(apiKey!, null, null, null);
        var prompt = "Generate a person with name 'John Doe' and age 30 in JSON format";

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
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            Assert.Skip("GROQ_API_KEY environment variable not set");

        var genie = new GroqGenie(apiKey!, null, null, null);
        var prompt = "Explain the concept of chunking in RAG systems in 2-3 sentences.";

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
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            Assert.Skip("GROQ_API_KEY environment variable not set");

        var genie = new GroqGenie(apiKey!, null, null, null);
        var prompt = @"Generate a book with title, author, year published (use 2024), 
                       and list of 3 genres in JSON format";

        // Act
        var response = await genie.GenerateJsonAsync<BookData>(prompt);

        // Assert
        response.ShouldNotBeNull();
        response.Title.ShouldNotBeNullOrWhiteSpace();
        response.Author.ShouldNotBeNullOrWhiteSpace();
        response.Year.ShouldBeGreaterThan(2000);
        response.Genres.ShouldNotBeNull();
        response.Genres.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task FromEnvironment_WithValidKey_ShouldWork()
    {
        // Arrange
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            Assert.Skip("GROQ_API_KEY environment variable not set");

        var genie = GroqGenie.FromEnvironment();
        var prompt = "Say 'Test passed!' and nothing else.";

        // Act
        var response = await genie.GenerateAsync(prompt);

        // Assert
        response.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GenerateAsync_WithCustomModel_ShouldWork()
    {
        // Arrange
        var apiKey = GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            Assert.Skip("GROQ_API_KEY environment variable not set");

        // Using a different supported model
        var genie = new GroqGenie(apiKey!, "llama-3.3-70b-versatile", null, null);
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

    private class BookData
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int Year { get; set; }
        public List<string> Genres { get; set; } = new();
    }
}
