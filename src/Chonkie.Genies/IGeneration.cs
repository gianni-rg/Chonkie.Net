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

namespace Chonkie.Genies;

/// <summary>
/// Interface for LLM generation services (Genies).
/// Provides text and structured JSON generation capabilities.
/// </summary>
public interface IGeneration
{
    /// <summary>
    /// Generates a text response from the LLM based on the provided prompt.
    /// </summary>
    /// <param name="prompt">The prompt to send to the LLM.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The generated text response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when prompt is null or empty.</exception>
    /// <exception cref="GenieException">Thrown when generation fails.</exception>
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a structured JSON response from the LLM based on the provided prompt.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON response into.</typeparam>
    /// <param name="prompt">The prompt to send to the LLM.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The deserialized JSON response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when prompt is null or empty.</exception>
    /// <exception cref="GenieException">Thrown when generation or JSON parsing fails.</exception>
    Task<T> GenerateJsonAsync<T>(string prompt, CancellationToken cancellationToken = default) where T : class;
}
