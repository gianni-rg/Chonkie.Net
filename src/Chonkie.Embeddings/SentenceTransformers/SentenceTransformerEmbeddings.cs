using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Chonkie.Embeddings.Base;

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
        public SentenceTransformerEmbeddings(
            string modelPath,
            PoolingMode? poolingMode = null,
            bool normalize = true,
            int? maxLength = null)
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
                : _modelConfig.HiddenSize;

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

            var sessionOptions = new SessionOptions
            {
                GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
            };

            _session = new InferenceSession(onnxModelPath, sessionOptions);
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
                var encoding = _tokenizer.Encode(text, addSpecialTokens: true);

                // Create input tensors
                var inputIds = CreateInputIdsTensor(new[] { encoding.InputIds });
                var attentionMask = CreateAttentionMaskTensor(new[] { encoding.AttentionMask });

                // Prepare inputs for ONNX
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
                    NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask)
                };

                // Add token_type_ids if the model expects it
                if (_session.InputMetadata.ContainsKey("token_type_ids"))
                {
                    var tokenTypeIds = CreateTokenTypeIdsTensor(new[] { encoding.TokenTypeIds });
                    inputs.Add(NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds));
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

                // Apply pooling
                var pooledEmbeddings = PoolingUtilities.ApplyPooling(
                    outputTensor,
                    encoding.AttentionMask,
                    batchSize,
                    seqLength,
                    hiddenDim,
                    _poolingMode,
                    _normalize
                );

                return pooledEmbeddings[0];
            }, cancellationToken);
        }

        /// <inheritdoc />
        public override Task<IReadOnlyList<float[]>> EmbedBatchAsync(
            IEnumerable<string> texts,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                var textList = texts.ToList();
                if (textList.Count == 0)
                {
                    return (IReadOnlyList<float[]>)Array.Empty<float[]>();
                }

                // Tokenize all texts with proper padding
                var batchEncoding = _tokenizer.EncodeBatch(textList, addSpecialTokens: true);

                // Create input tensors
                var inputIds = CreateInputIdsTensor(batchEncoding.InputIds);
                var attentionMask = CreateAttentionMaskTensor(batchEncoding.AttentionMask);

                // Prepare inputs for ONNX
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
                    NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask)
                };

                // Add token_type_ids if the model expects it
                if (_session.InputMetadata.ContainsKey("token_type_ids"))
                {
                    var tokenTypeIds = CreateTokenTypeIdsTensor(batchEncoding.TokenTypeIds);
                    inputs.Add(NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds));
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
                var flatAttentionMask = batchEncoding.AttentionMask
                    .SelectMany(m => m)
                    .ToArray();

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

                return (IReadOnlyList<float[]>)pooledEmbeddings;
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
        /// Counts the number of tokens in the text.
        /// </summary>
        /// <param name="text">The text to tokenize.</param>
        /// <returns>The number of tokens.</returns>
        public int CountTokens(string text)
        {
            return _tokenizer.CountTokens(text);
        }

        /// <summary>
        /// Counts the number of tokens in multiple texts.
        /// </summary>
        /// <param name="texts">The texts to tokenize.</param>
        /// <returns>A list of token counts for each text.</returns>
        public IReadOnlyList<int> CountTokensBatch(IEnumerable<string> texts)
        {
            var textList = texts.ToList();
            var counts = new List<int>(textList.Count);

            foreach (var text in textList)
            {
                counts.Add(_tokenizer.CountTokens(text));
            }

            return counts;
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
