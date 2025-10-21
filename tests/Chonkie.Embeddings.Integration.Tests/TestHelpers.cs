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
        if (string.IsNullOrEmpty(value))
        {
            throw new Xunit.SkipException($"Environment variable {variableName} not set. Skipping integration test.");
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
        if (a.Length != b.Length)
        {
            throw new ArgumentException("Vectors must have the same length");
        }

        float dotProduct = 0;
        float normA = 0;
        float normB = 0;

        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        return dotProduct / (MathF.Sqrt(normA) * MathF.Sqrt(normB));
    }
}
