using System;

namespace Chonkie.Embeddings.Integration.Tests;

/// <summary>
/// Helper methods for integration tests
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Gets an environment variable or skips the test if not found
    /// </summary>
    public static string GetEnvironmentVariableOrSkip(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);

        // Treat missing or obviously placeholder/invalid values as "not set" to avoid false failures
        if (string.IsNullOrWhiteSpace(value) || IsObviouslyPlaceholder(value!, variableName))
        {
            throw new Xunit.SkipException($"Environment variable {variableName} not set or invalid. Skipping integration test.");
        }

        return value!;
    }

    /// <summary>
    /// Checks if an environment variable is set
    /// </summary>
    public static bool IsEnvironmentVariableSet(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        return !string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Gets multiple environment variables or skips the test if any are not found
    /// </summary>
    public static Dictionary<string, string> GetEnvironmentVariablesOrSkip(params string[] variableNames)
    {
        var result = new Dictionary<string, string>();
        foreach (var name in variableNames)
        {
            result[name] = GetEnvironmentVariableOrSkip(name);
        }
        return result;
    }

    /// <summary>
    /// Calculates cosine similarity between two vectors
    /// </summary>
    public static float CosineSimilarity(float[] a, float[] b)
    {
        return Chonkie.Embeddings.VectorMath.CosineSimilarity(a, b);
    }

    /// <summary>
    /// Heuristic check for obviously placeholder/invalid environment variable values.
    /// This helps ensure integration tests are skipped when keys are present but not real.
    /// </summary>
    private static bool IsObviouslyPlaceholder(string value, string variableName)
    {
        var v = value.Trim();
        if (v.Length == 0)
            return true;

        // Common placeholder words
        var lower = v.ToLowerInvariant();
        if (lower.Contains("your-") || lower.Contains("<your") || lower.Contains("{your") || lower.Contains("replace") ||
            lower.Contains("changeme") || lower.Contains("dummy") || lower.Contains("placeholder") ||
            lower.Contains("sample") || lower.Contains("example") || lower.Contains("fake"))
        {
            return true;
        }

        // If it's an API key, very short strings are almost certainly invalid
        var isApiKey = variableName.EndsWith("API_KEY", StringComparison.OrdinalIgnoreCase) ||
                       variableName.Contains("APIKEY", StringComparison.OrdinalIgnoreCase);
        if (isApiKey && v.Length < 16)
        {
            return true;
        }

        // Simple provider-specific hints
        if (variableName.Equals("OPENAI_API_KEY", StringComparison.OrdinalIgnoreCase))
        {
            // Most OpenAI keys start with "sk-" and have substantial length.
            if (!lower.StartsWith("sk-") || v.Length < 20)
                return true;
        }

        // For endpoints, skip obvious placeholders
        if (variableName.EndsWith("ENDPOINT", StringComparison.OrdinalIgnoreCase))
        {
            if (lower.Contains("your-endpoint") || lower.Contains("localhost:0000"))
                return true;
        }

        return false;
    }
}
