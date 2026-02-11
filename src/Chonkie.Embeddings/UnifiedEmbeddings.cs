using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Embeddings.Base;
using Chonkie.Embeddings.Exceptions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Chonkie.Embeddings
{
    /// <summary>
    /// Unified embedding provider that wraps Microsoft.Extensions.AI IEmbeddingGenerator.
    /// Supports multiple providers through Semantic Kernel connectors including:
    /// OpenAI, Azure OpenAI, Google Gemini, Vertex AI, Amazon Bedrock, Hugging Face, MistralAI, Ollama, and ONNX.
    /// </summary>
    /// <remarks>
    /// This class provides a unified interface for accessing 8+ embedding providers through
    /// the standardized Microsoft.Extensions.AI abstractions, eliminating the need to maintain
    /// separate implementations for each provider.
    /// </remarks>
    public class UnifiedEmbeddings : BaseEmbeddings
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;
        private readonly string _providerName;
        private readonly int _dimension;
        private readonly ILogger? _logger;

        /// <inheritdoc />
        public override string Name => $"unified-{_providerName}";

        /// <inheritdoc />
        public override int Dimension => _dimension;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnifiedEmbeddings"/> class.
        /// </summary>
        /// <param name="generator">The embedding generator from Microsoft.Extensions.AI.</param>
        /// <param name="providerName">The name of the provider (e.g., "openai", "gemini", "ollama").</param>
        /// <param name="dimension">The dimension of the embedding vectors.</param>
        /// <param name="logger">Optional logger for diagnostics.</param>
        /// <exception cref="ArgumentNullException">Thrown when generator or providerName is null.</exception>
        /// <exception cref="ArgumentException">Thrown when providerName is empty or whitespace, or dimension is non-positive.</exception>
        public UnifiedEmbeddings(
            IEmbeddingGenerator<string, Embedding<float>> generator,
            string providerName,
            int dimension,
            ILogger? logger = null)
        {
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));

            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("Provider name cannot be null or whitespace.", nameof(providerName));

            if (dimension <= 0)
                throw new ArgumentException("Dimension must be positive.", nameof(dimension));

            _providerName = providerName.ToLowerInvariant();
            _dimension = dimension;
            _logger = logger;
        }

        /// <inheritdoc />
        public override async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        {
            var safeText = text ?? throw new ArgumentNullException(nameof(text));

            try
            {
                _logger?.LogDebug("Generating embedding for text of length {Length} using {Provider}",
                    safeText.Length, _providerName);

                var input = new List<string>(1) { safeText };
                var result = await _generator.GenerateAsync(input, cancellationToken: cancellationToken);

                if (result is null || result.Count == 0)
                    throw new EmbeddingInvalidResponseException("Embedding generation returned null or empty result.");

                var embedding = result[0];

                if (embedding?.Vector is null || embedding.Vector.Length == 0)
                    throw new EmbeddingInvalidResponseException("Generated embedding vector is null or empty.");

                return embedding.Vector.ToArray();
            }
            catch (EmbeddingException)
            {
                // Re-throw our own exceptions
                throw;
            }
            catch (OperationCanceledException ex)
            {
                _logger?.LogWarning(ex, "Embedding operation was cancelled");
                throw new EmbeddingException($"Embedding operation was cancelled: {ex.Message}", ex);
            }
            catch (TimeoutException ex)
            {
                _logger?.LogError(ex, "Embedding operation timed out");
                throw new EmbeddingNetworkException($"Embedding operation timed out: {ex.Message}", ex);
            }
            catch (Exception ex) when (ex.Message.Contains("rate limit", StringComparison.OrdinalIgnoreCase) ||
                                      ex.Message.Contains("429", StringComparison.OrdinalIgnoreCase))
            {
                _logger?.LogWarning(ex, "Rate limit encountered");
                throw new EmbeddingRateLimitException($"Rate limit exceeded: {ex.Message}", ex);
            }
            catch (Exception ex) when (ex.Message.Contains("authentication", StringComparison.OrdinalIgnoreCase) ||
                                      ex.Message.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) ||
                                      ex.Message.Contains("401", StringComparison.OrdinalIgnoreCase) ||
                                      ex.Message.Contains("403", StringComparison.OrdinalIgnoreCase))
            {
                _logger?.LogError(ex, "Authentication failed");
                throw new EmbeddingAuthenticationException($"Authentication failed: {ex.Message}", ex);
            }
            catch (Exception ex) when (ex is HttpRequestException ||
                                      ex.Message.Contains("network", StringComparison.OrdinalIgnoreCase))
            {
                _logger?.LogError(ex, "Network error occurred");
                throw new EmbeddingNetworkException($"Network error occurred: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error during embedding generation");
                throw new EmbeddingException($"Unexpected error during embedding: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public override async Task<IReadOnlyList<float[]>> EmbedBatchAsync(
            IEnumerable<string> texts,
            CancellationToken cancellationToken = default)
        {
            var textList = texts?.ToList() ?? throw new ArgumentNullException(nameof(texts));

            if (textList.Count == 0)
                return Array.Empty<float[]>();

            _logger?.LogDebug("Generating {Count} embeddings using {Provider}", textList.Count, _providerName);

            try
            {
                var result = await _generator.GenerateAsync(textList, cancellationToken: cancellationToken);

                if (result is null)
                    throw new EmbeddingInvalidResponseException("Batch embedding generation returned null result.");

                var embeddings = new List<float[]>(result.Count);
                foreach (var embedding in result)
                {
                    if (embedding?.Vector is null || embedding.Vector.Length == 0)
                        throw new EmbeddingInvalidResponseException("Generated embedding vector is null or empty.");

                    embeddings.Add(embedding.Vector.ToArray());
                }

                return embeddings;
            }
            catch (Exception ex)
            {
                HandleBatchEmbeddingException(ex);
                throw; // unreachable, but required by compiler
            }
        }

        private void HandleBatchEmbeddingException(Exception ex)
        {
            if (ex is EmbeddingException)
            {
                // Re-throw our own exceptions
                throw ex;
            }

            if (ex is OperationCanceledException)
            {
                _logger?.LogWarning(ex, "Batch embedding operation was cancelled");
                throw new EmbeddingException($"Batch embedding operation was cancelled: {ex.Message}", ex);
            }

            if (ex is TimeoutException)
            {
                _logger?.LogError(ex, "Batch embedding operation timed out");
                throw new EmbeddingNetworkException($"Batch embedding operation timed out: {ex.Message}", ex);
            }

            if (IsRateLimitException(ex))
            {
                _logger?.LogWarning(ex, "Rate limit encountered during batch embedding");
                throw new EmbeddingRateLimitException($"Rate limit exceeded during batch operation: {ex.Message}", ex);
            }

            if (IsAuthenticationException(ex))
            {
                _logger?.LogError(ex, "Authentication failed during batch embedding");
                throw new EmbeddingAuthenticationException($"Authentication failed during batch operation: {ex.Message}", ex);
            }

            if (ex is HttpRequestException || IsNetworkException(ex))
            {
                _logger?.LogError(ex, "Network error occurred during batch embedding");
                throw new EmbeddingNetworkException($"Network error occurred during batch operation: {ex.Message}", ex);
            }

            _logger?.LogError(ex, "Unexpected error during batch embedding generation");
            throw new EmbeddingException($"Unexpected error during batch embedding: {ex.Message}", ex);
        }

        private static bool IsRateLimitException(Exception ex)
            => ex.Message.Contains("rate limit", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("429", StringComparison.OrdinalIgnoreCase);

        private static bool IsAuthenticationException(Exception ex)
            => ex.Message.Contains("authentication", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("401", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("403", StringComparison.OrdinalIgnoreCase);

        private static bool IsNetworkException(Exception ex)
            => ex.Message.Contains("network", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Returns a string representation of the unified embeddings provider.
        /// </summary>
        public override string ToString()
        {
            return $"UnifiedEmbeddings(Provider={_providerName}, Dimension={_dimension})";
        }
    }
}
