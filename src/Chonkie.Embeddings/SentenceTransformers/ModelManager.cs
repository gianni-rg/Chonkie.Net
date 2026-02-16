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
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Chonkie.Embeddings.SentenceTransformers
{
    /// <summary>
    /// Utilities for managing ONNX models (downloading, caching, validation).
    /// </summary>
    public static class ModelManager
    {
        private static readonly string _defaultCacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".cache", "chonkie", "models"
        );

        /// <summary>
        /// Gets the default cache directory for models.
        /// </summary>
        public static string DefaultCacheDirectory => _defaultCacheDir;

        /// <summary>
        /// Validates that a model directory contains all necessary files.
        /// </summary>
        /// <param name="modelPath">Path to the model directory.</param>
        /// <returns>True if the model is valid, false otherwise.</returns>
        public static bool ValidateModel(string modelPath)
        {
            if (!Directory.Exists(modelPath))
            {
                return false;
            }

            // Check for required files
            var requiredFiles = new[]
            {
                "model.onnx",      // ONNX model file
                "config.json",     // Model configuration
                "vocab.txt"        // Vocabulary file
            };

            foreach (var file in requiredFiles)
            {
                var filePath = Path.Combine(modelPath, file);
                if (!File.Exists(filePath))
                {
                    return false;
                }
            }

            // Optional files (should exist but not required)
            var optionalFiles = new[]
            {
                "tokenizer_config.json",
                "special_tokens_map.json",
                "tokenizer.json"
            };

            int optionalCount = 0;
            foreach (var file in optionalFiles)
            {
                var filePath = Path.Combine(modelPath, file);
                if (File.Exists(filePath))
                {
                    optionalCount++;
                }
            }

            // At least one optional file should exist
            return optionalCount > 0;
        }

        /// <summary>
        /// Gets model metadata from the configuration files.
        /// </summary>
        /// <param name="modelPath">Path to the model directory.</param>
        /// <returns>Model metadata.</returns>
        public static ModelMetadata GetModelMetadata(string modelPath)
        {
            if (!Directory.Exists(modelPath))
            {
                throw new DirectoryNotFoundException($"Model directory not found: {modelPath}");
            }

            var configPath = Path.Combine(modelPath, "config.json");
            var poolingConfigPath = Path.Combine(modelPath, "1_Pooling", "config.json");
            if (!File.Exists(poolingConfigPath))
            {
                poolingConfigPath = Path.Combine(modelPath, "pooling_config.json");
            }

            var config = ModelConfig.Load(configPath);
            var poolingConfig = PoolingConfig.Load(poolingConfigPath);

            return new ModelMetadata
            {
                HiddenSize = config.HiddenSize,
                MaxPositionEmbeddings = config.MaxPositionEmbeddings,
                VocabSize = config.VocabSize,
                ModelType = config.ModelType ?? "unknown",
                EmbeddingDimension = poolingConfig.WordEmbeddingDimension > 0
                    ? poolingConfig.WordEmbeddingDimension
                    : config.HiddenSize,
                PoolingMode = poolingConfig.GetPrimaryPoolingMode()
            };
        }

        /// <summary>
        /// Ensures the cache directory exists.
        /// </summary>
        /// <param name="cacheDir">Cache directory path (null for default).</param>
        /// <returns>The cache directory path.</returns>
        public static string EnsureCacheDirectory(string? cacheDir = null)
        {
            var dir = cacheDir ?? _defaultCacheDir;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        /// <summary>
        /// Gets the local path for a model in the cache.
        /// </summary>
        /// <param name="modelName">Model name (e.g., "sentence-transformers/all-MiniLM-L6-v2").</param>
        /// <param name="cacheDir">Cache directory (null for default).</param>
        /// <returns>Local path where the model should be cached.</returns>
        public static string GetModelPath(string modelName, string? cacheDir = null)
        {
            var cache = EnsureCacheDirectory(cacheDir);

            // Replace slashes with underscores for safe directory names
            var safeName = modelName.Replace("/", "_").Replace("\\", "_");

            return Path.Combine(cache, safeName);
        }

        /// <summary>
        /// Checks if a model is already cached locally.
        /// </summary>
        /// <param name="modelName">Model name.</param>
        /// <param name="cacheDir">Cache directory (null for default).</param>
        /// <returns>True if the model exists in cache and is valid.</returns>
        public static bool IsModelCached(string modelName, string? cacheDir = null)
        {
            var modelPath = GetModelPath(modelName, cacheDir);
            return ValidateModel(modelPath);
        }

        /// <summary>
        /// Downloads a model from HuggingFace Hub (placeholder - needs implementation).
        /// </summary>
        /// <param name="modelName">Model name (e.g., "sentence-transformers/all-MiniLM-L6-v2").</param>
        /// <param name="cacheDir">Cache directory (null for default).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Path to the downloaded model.</returns>
        public static async Task<string> DownloadModelAsync(
            string modelName,
            string? cacheDir = null,
            CancellationToken cancellationToken = default)
        {
            var modelPath = GetModelPath(modelName, cacheDir);

            // Check if already cached
            if (IsModelCached(modelName, cacheDir))
            {
                return modelPath;
            }

            // Note: Automatic HuggingFace Hub download not yet implemented.
            // Users must manually convert and place the model at the cache directory.
            throw new NotImplementedException(
                $"Automatic model download is not yet implemented. " +
                $"Please manually convert and place the model at: {modelPath}\n\n" +
                $"To convert a model:\n" +
                $"1. pip install optimum[onnxruntime]\n" +
                $"2. optimum-cli export onnx --model {modelName} {modelPath}\n\n" +
                $"Or download from HuggingFace Hub and place the ONNX files in the directory."
            );
        }

        /// <summary>
        /// Loads a model, downloading it if necessary.
        /// </summary>
        /// <param name="modelName">Model name or local path.</param>
        /// <param name="cacheDir">Cache directory (null for default).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Path to the model directory.</returns>
        public static async Task<string> LoadModelAsync(
            string modelName,
            string? cacheDir = null,
            CancellationToken cancellationToken = default)
        {
            // Check if it's a local path
            if (Directory.Exists(modelName) && ValidateModel(modelName))
            {
                return modelName;
            }

            // Check if it's in cache
            if (IsModelCached(modelName, cacheDir))
            {
                return GetModelPath(modelName, cacheDir);
            }

            // Try to download
            return await DownloadModelAsync(modelName, cacheDir, cancellationToken);
        }
    }

    /// <summary>
    /// Metadata about a Sentence Transformer model.
    /// </summary>
    public class ModelMetadata
    {
        /// <summary>Hidden dimension of the model.</summary>
        public int HiddenSize { get; set; }

        /// <summary>Maximum position embeddings (sequence length).</summary>
        public int MaxPositionEmbeddings { get; set; }

        /// <summary>Vocabulary size.</summary>
        public int VocabSize { get; set; }

        /// <summary>Model type (e.g., "bert", "roberta").</summary>
        public string ModelType { get; set; } = string.Empty;

        /// <summary>Output embedding dimension.</summary>
        public int EmbeddingDimension { get; set; }

        /// <summary>Pooling mode used by the model.</summary>
        public PoolingMode PoolingMode { get; set; }

        /// <summary>
        /// Returns a string representation of the metadata.
        /// </summary>
        public override string ToString()
        {
            return $"ModelMetadata(Type={ModelType}, HiddenSize={HiddenSize}, " +
                   $"EmbeddingDim={EmbeddingDimension}, MaxLength={MaxPositionEmbeddings}, " +
                   $"VocabSize={VocabSize}, Pooling={PoolingMode})";
        }
    }
}
