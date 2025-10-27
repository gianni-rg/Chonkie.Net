using System;
using System.IO;

namespace Chonkie.Pipeline.Tests;

/// <summary>
/// Helper methods for tests.
/// </summary>
internal static class TestHelpers
{
    /// <summary>
    /// Checks if the model directory exists.
    /// </summary>
    /// <returns>True if the model directory exists, false otherwise.</returns>
    public static bool IsModelAvailable()
    {
        var modelPath = "/workspace/models/all-MiniLM-L6-v2";
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
            throw new Xunit.SkipException("Model directory not found. Skipping test that requires ONNX model.");
        }
    }
}
