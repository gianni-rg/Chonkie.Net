using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Chonkie.Embeddings.Base
{
    /// <summary>
    /// Base class for embedding providers.
    /// </summary>
    public abstract class BaseEmbeddings : Interfaces.IEmbeddings
    {
        public abstract string Name { get; }
        public abstract int Dimension { get; }

        public abstract Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);

        public virtual async Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
        {
            var results = new List<float[]>();
            foreach (var text in texts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                results.Add(await EmbedAsync(text, cancellationToken));
            }
            return results;
        }
    }
}