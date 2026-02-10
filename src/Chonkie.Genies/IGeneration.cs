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
