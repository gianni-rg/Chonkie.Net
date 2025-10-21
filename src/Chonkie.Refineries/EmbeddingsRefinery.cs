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
        public EmbeddingsRefinery(IEmbeddings embeddings)
        {
            _embeddings = embeddings;
        }

        public async Task<IReadOnlyList<Chunk>> RefineAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken = default)
        {
            var result = new List<Chunk>(chunks.Count);
            foreach (var chunk in chunks)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var embedding = await _embeddings.GetEmbeddingAsync(chunk.Text, cancellationToken);
                result.Add(chunk with { Embedding = embedding });
            }
            return result;
        }
    }
}
