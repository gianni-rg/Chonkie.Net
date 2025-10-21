using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Core.Types;
using Chonkie.Embeddings.Interfaces;

namespace Chonkie.Refineries
{
    /// <summary>
    /// Adds embeddings to each chunk using the provided embeddings provider.
    /// </summary>
    public class EmbeddingsRefinery : IRefinery
    {
        private readonly IEmbeddings _embeddings;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddingsRefinery"/> class.
        /// </summary>
        /// <param name="embeddings">The embeddings provider to use.</param>
        public EmbeddingsRefinery(IEmbeddings embeddings)
        {
            _embeddings = embeddings;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Chunk>> RefineAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken = default)
        {
            var result = new List<Chunk>(chunks.Count);
            foreach (var chunk in chunks)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var embedding = await _embeddings.EmbedAsync(chunk.Text, cancellationToken);
                result.Add(chunk with { Embedding = embedding });
            }
            return result;
        }
    }
}
