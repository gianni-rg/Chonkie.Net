using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Chonkie.Core.Interfaces;
using Chonkie.Core.Types;
using Chonkie.Embeddings.Interfaces;

namespace Chonkie.Chunkers
{
    /// <summary>
    /// LateChunker implements the "embed-then-chunk" approach for late interaction chunking.
    /// It first chunks the text using recursive splitting, then generates token-level embeddings
    /// and aggregates them for each chunk.
    /// </summary>
    public class LateChunker : RecursiveChunker
    {
        private readonly IEmbeddings _embeddingModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="LateChunker"/> class.
        /// </summary>
        /// <param name="tokenizer">The tokenizer to use for token counting.</param>
        /// <param name="embeddingModel">The embedding model to use for generating embeddings.</param>
        /// <param name="chunkSize">Maximum tokens allowed per chunk. Default is 2048.</param>
        /// <param name="rules">Recursive rules to chunk by. If null, default rules will be used.</param>
        /// <param name="minCharactersPerChunk">Minimum number of characters in a single chunk. Default is 24.</param>
        /// <param name="logger">Logger for diagnostic output.</param>
        public LateChunker(
            ITokenizer tokenizer,
            IEmbeddings embeddingModel,
            int chunkSize = 2048,
            RecursiveRules? rules = null,
            int minCharactersPerChunk = 24,
            ILogger<LateChunker>? logger = null)
            : base(tokenizer, chunkSize, rules, minCharactersPerChunk, logger)
        {
            _embeddingModel = embeddingModel ?? throw new ArgumentNullException(nameof(embeddingModel));
        }

        /// <summary>
        /// Chunks the text using late interaction approach.
        /// First performs recursive chunking, then adds token-level embeddings to each chunk.
        /// </summary>
        /// <param name="text">The text to chunk.</param>
        /// <returns>A list of chunks with embeddings.</returns>
        public override IReadOnlyList<Chunk> Chunk(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                Logger.LogDebug("Empty or whitespace-only text provided");
                return Array.Empty<Chunk>();
            }

            Logger.LogDebug("Starting late chunking for text of length {Length}", text.Length);

            // First, perform recursive chunking
            var chunks = base.Chunk(text);
            Logger.LogDebug("Created {Count} initial chunks from recursive splitting", chunks.Count);

            if (chunks.Count == 0)
                return chunks;

            // TODO: This requires token-level embedding support which needs to be added to IEmbeddings
            // For now, we'll generate embeddings for each chunk as a whole
            var chunkTexts = chunks.Select(c => c.Text).ToList();
            var embeddings = _embeddingModel.EmbedBatchAsync(chunkTexts).GetAwaiter().GetResult();

            // Create new chunks with embeddings
            var result = new List<Chunk>();
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                result.Add(new Chunk
                {
                    Text = chunk.Text,
                    StartIndex = chunk.StartIndex,
                    EndIndex = chunk.EndIndex,
                    TokenCount = chunk.TokenCount,
                    Embedding = embeddings[i]
                });
            }

            Logger.LogInformation("Created {Count} chunks with late interaction embeddings", result.Count);
            return result;
        }

        /// <summary>
        /// Gets a string representation of the LateChunker.
        /// </summary>
        public override string ToString()
        {
            return $"LateChunker(ChunkSize={ChunkSize}, MinCharactersPerChunk={MinCharactersPerChunk}, " +
                   $"EmbeddingModel={_embeddingModel.Name})";
        }
    }
}
