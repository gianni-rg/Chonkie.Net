using System;
using System.IO;

namespace Chonkie.Pipeline.Tests;

/// <summary>
/// Helper methods for tests.
/// </summary>
internal static class TestHelpers
{
    /// <summary>
    /// Gets the model path from environment variable.
    /// Uses SENTENCE_TRANSFORMERS_MODEL_PATH only; no hard-coded fallback path.
    /// </summary>
    /// <returns>The model path to use for tests, or empty string if not set.</returns>
    public static string GetModelPath()
    {
        var envPath = Environment.GetEnvironmentVariable("SENTENCE_TRANSFORMERS_MODEL_PATH");
        return envPath ?? string.Empty;
    }

    /// <summary>
    /// Checks if the model directory exists.
    /// </summary>
    /// <returns>True if the model directory exists, false otherwise.</returns>
    public static bool IsModelAvailable()
    {
        var modelPath = GetModelPath();
        if (string.IsNullOrWhiteSpace(modelPath))
        {
            return false;
        }
        var modelFile = Path.Combine(modelPath, "model.onnx");
        return Directory.Exists(modelPath) && File.Exists(modelFile);
    }

    /// <summary>
    /// Skips the test if the model is not available.
    /// </summary>
    /// <exception cref="Xunit.SkipException">Thrown if the model is not available.</exception>
    public static void SkipIfModelNotAvailable()
    {
        if (!IsModelAvailable())
        {
            throw new Xunit.SkipException("SENTENCE_TRANSFORMERS_MODEL_PATH not set or model.onnx not found. Skipping test that requires ONNX model.");
        }
    }
}
