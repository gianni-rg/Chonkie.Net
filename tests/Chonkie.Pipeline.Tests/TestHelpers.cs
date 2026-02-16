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
    /// Uses CHONKIE_SENTENCE_TRANSFORMERS_MODEL_PATH only; no hard-coded fallback path.
    /// </summary>
    /// <returns>The model path to use for tests, or empty string if not set.</returns>
    public static string GetModelPath()
    {
        var envPath = Environment.GetEnvironmentVariable("CHONKIE_SENTENCE_TRANSFORMERS_MODEL_PATH");
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
    public static void SkipIfModelNotAvailable()
    {
        if (!IsModelAvailable())
            Assert.Skip("CHONKIE_SENTENCE_TRANSFORMERS_MODEL_PATH not set or model.onnx not found. Skipping test that requires ONNX model.");
    }
}
