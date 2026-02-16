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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Chonkie.Embeddings.Base;
using Microsoft.ML.Tokenizers;

namespace Chonkie.Embeddings.SentenceTransformers
{
    /// <summary>
    /// Embedding provider using ONNX Runtime for local Sentence Transformer models.
    /// Supports proper tokenization, pooling strategies, and batch processing.
    /// </summary>
    public class SentenceTransformerEmbeddings : BaseEmbeddings, IDisposable
    {
        private readonly InferenceSession _session;
        private readonly SentenceTransformerTokenizer _tokenizer;
        private readonly string _modelPath;
        private readonly ModelConfig _modelConfig;
        private readonly PoolingConfig _poolingConfig;
        private readonly PoolingMode _poolingMode;
        private readonly bool _normalize;
        private bool _disposed;
        private Microsoft.ML.Tokenizers.BertTokenizer? _bertTokenizer;

        /// <inheritdoc />
        public override string Name => "sentence-transformers";

        /// <inheritdoc />
        public override int Dimension { get; }

        /// <summary>
        /// Gets the maximum sequence length.
        /// </summary>
        public int MaxSequenceLength => _tokenizer.MaxLength;

        /// <summary>
        /// Initializes a new instance of SentenceTransformerEmbeddings.
        /// </summary>
        /// <param name="modelPath">Path to the model directory containing ONNX model and tokenizer files.</param>
        /// <param name="poolingMode">Pooling mode (default: auto-detect from config).</param>
        /// <param name="normalize">Whether to apply L2 normalization (default: true).</param>
        /// <param name="maxLength">Maximum sequence length (default: from config).</param>
        /// <param name="sessionOptions">Optional ONNX Runtime SessionOptions. If null, defaults are used.</param>
        public SentenceTransformerEmbeddings(
            string modelPath,
            PoolingMode? poolingMode = null,
            bool normalize = true,
            int? maxLength = null,
            SessionOptions? sessionOptions = null)
        {
            if (string.IsNullOrEmpty(modelPath))
            {
                throw new ArgumentNullException(nameof(modelPath));
            }

            if (!Directory.Exists(modelPath))
            {
                throw new DirectoryNotFoundException($"Model directory not found: {modelPath}");
            }

            _modelPath = modelPath;
            _normalize = normalize;

            // Load model configuration
            var configPath = Path.Combine(modelPath, "config.json");
            _modelConfig = ModelConfig.Load(configPath);

            // Load pooling configuration
            var poolingConfigPath = Path.Combine(modelPath, "1_Pooling", "config.json");
            if (!File.Exists(poolingConfigPath))
            {
                // Try alternative path
                poolingConfigPath = Path.Combine(modelPath, "pooling_config.json");
            }
            _poolingConfig = PoolingConfig.Load(poolingConfigPath);

            // Set dimension from pooling config
            Dimension = _poolingConfig.WordEmbeddingDimension > 0
                ? _poolingConfig.WordEmbeddingDimension
                : _modelConfig.EffectiveHiddenSize;


            // Determine pooling mode
            _poolingMode = poolingMode ?? _poolingConfig.GetPrimaryPoolingMode();

            // Initialize tokenizer
            _tokenizer = new SentenceTransformerTokenizer(modelPath, maxLength);

            // Initialize ONNX session
            var onnxModelPath = Path.Combine(modelPath, "model.onnx");
            if (!File.Exists(onnxModelPath))
            {
                throw new FileNotFoundException($"ONNX model file not found: {onnxModelPath}");
            }

            // Allow external configuration of ONNX Runtime SessionOptions.
            // If not provided, use recommended defaults for stability and performance.
            if (sessionOptions == null)
            {
                sessionOptions = new SessionOptions
                {
                    GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
                    // Limit parallelism to prevent resource exhaustion on large models
                    // Using fewer threads reduces memory pressure and improves stability
                    IntraOpNumThreads = Math.Max(1, Environment.ProcessorCount / 2),
                    InterOpNumThreads = 1
                };
            }

            _session = new InferenceSession(onnxModelPath, sessionOptions);
            // Try to load a suitable tokenizer via Microsoft.ML.Tokenizers
            try
            {
                var modelDir = Path.GetDirectoryName(modelPath) ?? "";
                var vocabTxt = Path.Combine(modelDir, "vocab.txt");
                if (File.Exists(vocabTxt))
                {
                    // Create a BERT-style WordPiece tokenizer
                    _bertTokenizer = Microsoft.ML.Tokenizers.BertTokenizer.Create(vocabTxt, new Microsoft.ML.Tokenizers.BertOptions());
                }
            }
            catch
            {
                // If tokenizer cannot be loaded, we'll fallback to SimpleTokenize
            }
        }

        /// <inheritdoc />
        public override Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrEmpty(text))
                {
                    return new float[Dimension];
                }

                // Tokenize text with proper tokenizer
                // Prefer BertTokenizer (WordPiece) when available because many
                // SentenceTransformer ONNX exports are based on BERT-derived models
                // and expect WordPiece token IDs and attention-mask semantics.
                // Fallback to SentenceTransformerTokenizer to support models that
                // ship only sentence-transformers configs.
                // Note: Using different tokenizers can change token IDs and thus
                // embeddings. Ensure the selected tokenizer matches the model
                // artifacts (e.g., vocab.txt) for best compatibility.
                EncodingResult encoding;
                if (_bertTokenizer != null)
                {
                    encoding = EncodeWithBertTokenizer(text);
                }
                else
                {
                    encoding = _tokenizer.Encode(text, addSpecialTokens: true);
                }

                // Create input tensors
                var inputIds = CreateInputIdsTensor(new[] { encoding.InputIds });
                var attentionMask = CreateAttentionMaskTensor(new[] { encoding.AttentionMask });

                // Create inputs (add token_type_ids if the model expects it)
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
                    NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask)
                };

                // Some ONNX-exported transformer models require token_type_ids
                // Provide a zero tensor if the input exists in the model
                try
                {
                    var inputNames = _session.InputMetadata.Keys;
                    if (inputNames.Contains("token_type_ids"))
                    {
                        var tokenTypeIds = new DenseTensor<long>(new[] { 1, (int)inputIds.Dimensions[1] });
                        // initialized to zeros by default
                        inputs.Add(NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds));
                    }
                }
                catch
                {
                    // If metadata not available, proceed without token_type_ids
                }

                // Run inference
                using var results = _session.Run(inputs);

                // Get token embeddings from output
                var outputTensor = results.First().AsEnumerable<float>().ToArray();

                // Extract dimensions from output
                var outputMetadata = results.First();
                var shape = outputMetadata.AsTensor<float>().Dimensions.ToArray();

                int batchSize = shape[0];
                int seqLength = shape[1];
                int hiddenDim = shape[2];

                // Flatten attention mask for pooling
                // PoolingUtilities expects a flat int[] mask, not a tensor. This conversion
                // is necessary for compatibility, but could be inefficient for large batches.
                // Replaced LINQ conversion with a for-loop for better performance.
                // Further optimization: If PoolingUtilities supports Span<int>, this can avoid allocation entirely.
                var flatAttentionMask = new int[attentionMask.Length];
                for (int i = 0; i < attentionMask.Length; i++)
                {
                    flatAttentionMask[i] = (int)attentionMask.GetValue(i);
                }

                // Apply pooling
                var pooledEmbeddings = PoolingUtilities.ApplyPooling(
                    outputTensor,
                    flatAttentionMask,
                    batchSize,
                    seqLength,
                    hiddenDim,
                    _poolingMode,
                    _normalize
                );

                // Return the first (and only) embedding
                return pooledEmbeddings[0];
            }, cancellationToken);
        }

        /// <summary>
        /// Creates input_ids tensor from token IDs.
        /// </summary>
        private DenseTensor<long> CreateInputIdsTensor(int[][] inputIds)
        {
            int batchSize = inputIds.Length;
            int seqLength = inputIds[0].Length;

            var tensor = new DenseTensor<long>(new[] { batchSize, seqLength });

            for (int b = 0; b < batchSize; b++)
            {
                for (int s = 0; s < seqLength; s++)
                {
                    tensor[b, s] = inputIds[b][s];
                }
            }

            return tensor;
        }

        /// <summary>
        /// Creates attention_mask tensor from attention masks.
        /// </summary>
        private DenseTensor<long> CreateAttentionMaskTensor(int[][] attentionMask)
        {
            int batchSize = attentionMask.Length;
            int seqLength = attentionMask[0].Length;

            var tensor = new DenseTensor<long>(new[] { batchSize, seqLength });

            for (int b = 0; b < batchSize; b++)
            {
                for (int s = 0; s < seqLength; s++)
                {
                    tensor[b, s] = attentionMask[b][s];
                }
            }

            return tensor;
        }

        /// <summary>
        /// Creates token_type_ids tensor from token type IDs.
        /// </summary>
        private DenseTensor<long> CreateTokenTypeIdsTensor(int[][] tokenTypeIds)
        {
            int batchSize = tokenTypeIds.Length;
            int seqLength = tokenTypeIds[0].Length;

            var tensor = new DenseTensor<long>(new[] { batchSize, seqLength });

            for (int b = 0; b < batchSize; b++)
            {
                for (int s = 0; s < seqLength; s++)
                {
                    tensor[b, s] = tokenTypeIds[b][s];
                }
            }

            return tensor;
        }

        /// <summary>
        /// Encodes text using the Microsoft.ML.Tokenizers BertTokenizer.
        /// </summary>
        /// <param name="text">The text to encode.</param>
        /// <returns>An encoding result compatible with the ONNX model.</returns>
        private EncodingResult EncodeWithBertTokenizer(string text)
        {
            if (_bertTokenizer == null)
            {
                throw new InvalidOperationException("BertTokenizer is not available");
            }

            // Encode the text with BertTokenizer
            var tokenIds = _bertTokenizer.EncodeToIds(text, addSpecialTokens: true);

            // Convert to int array
            var inputIds = tokenIds.Select(id => (int)id).ToArray();

            // Create attention mask (all 1s for real tokens)
            var attentionMask = Enumerable.Repeat(1, inputIds.Length).ToArray();

            // Create token type IDs (all 0s for single sequence)
            var tokenTypeIds = Enumerable.Repeat(0, inputIds.Length).ToArray();

            return new EncodingResult
            {
                InputIds = inputIds,
                AttentionMask = attentionMask,
                TokenTypeIds = tokenTypeIds
            };
        }

        /// <summary>
        /// Counts the number of tokens in the text.
        /// </summary>
        /// <param name="text">The text to tokenize.</param>
        /// <returns>The number of tokens.</returns>
        public int CountTokens(string text)
        {
            if (_bertTokenizer != null)
            {
                return _bertTokenizer.CountTokens(text);
            }
            return _tokenizer.CountTokens(text);
        }

        /// <summary>
        /// Counts the number of tokens in multiple texts.
        /// </summary>
        /// <param name="texts">The texts to tokenize.</param>
        /// <returns>A list of token counts for each text.</returns>
        public IReadOnlyList<int> CountTokensBatch(IEnumerable<string> texts)
        {
            return texts.Select(t => CountTokens(t)).ToList();
        }

        /// <summary>
        /// Disposes the ONNX inference session and tokenizer.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _session?.Dispose();
                _tokenizer?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
