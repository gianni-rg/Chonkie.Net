using System;
using System.Collections.Generic;
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
    /// </summary>
    public class SentenceTransformerEmbeddings : BaseEmbeddings, IDisposable
    {
        private readonly InferenceSession _session;
        private readonly string _modelPath;
        private bool _disposed;

        /// <inheritdoc />
        public override string Name => "sentence-transformers";
        
        /// <inheritdoc />
        public override int Dimension { get; }

        /// <summary>
        /// Initializes a new instance of SentenceTransformerEmbeddings.
        /// </summary>
        /// <param name="modelPath">Path to the ONNX model file.</param>
        /// <param name="dimension">Embedding dimension.</param>
        public SentenceTransformerEmbeddings(string modelPath, int dimension = 384)
        {
            _modelPath = modelPath ?? throw new ArgumentNullException(nameof(modelPath));
            Dimension = dimension;

            var sessionOptions = new SessionOptions
            {
                GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
            };

            _session = new InferenceSession(modelPath, sessionOptions);
        }

        /// <inheritdoc />
        public override Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                // Tokenize text (simplified - in production use proper tokenizer)
                var tokens = SimpleTokenize(text);
                
                // Create input tensors
                var inputIds = new DenseTensor<long>(new[] { 1, tokens.Length });
                var attentionMask = new DenseTensor<long>(new[] { 1, tokens.Length });
                
                for (int i = 0; i < tokens.Length; i++)
                {
                    inputIds[0, i] = tokens[i];
                    attentionMask[0, i] = 1;
                }

                // Create inputs
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
                    NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask)
                };

                // Run inference
                using var results = _session.Run(inputs);
                var output = results.First().AsEnumerable<float>().ToArray();
                
                // Take the first Dimension values (pooled output)
                return output.Take(Dimension).ToArray();
            }, cancellationToken);
        }

        /// <inheritdoc />
        public override async Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
        {
            var results = new List<float[]>();
            foreach (var text in texts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                results.Add(await EmbedAsync(text, cancellationToken));
            }
            return results;
        }

        /// <summary>
        /// Simple tokenization (placeholder - use proper tokenizer in production).
        /// </summary>
        private long[] SimpleTokenize(string text)
        {
            // This is a simplified tokenizer
            // In production, use Microsoft.ML.Tokenizers or similar
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return words.Select(w => (long)w.GetHashCode() % 30000).ToArray();
        }

        /// <summary>
        /// Disposes the ONNX inference session.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _session?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}