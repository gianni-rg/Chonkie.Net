using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Core.Types;

namespace Chonkie.Refineries
{
    /// <summary>
    /// Merges overlapping chunks based on text similarity (simple implementation).
    /// </summary>
    public class OverlapRefinery : IRefinery
    {
        private readonly int _minOverlap;
        public OverlapRefinery(int minOverlap = 16)
        {
            _minOverlap = minOverlap;
        }

        public Task<IReadOnlyList<Chunk>> RefineAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken = default)
        {
            if (chunks.Count < 2)
                return Task.FromResult(chunks);

            var refined = new List<Chunk>();
            Chunk? prev = null;
            foreach (var chunk in chunks)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (prev != null && prev.EndIndex >= chunk.StartIndex && (chunk.StartIndex - prev.EndIndex) < _minOverlap)
                {
                    // Merge chunks
                    var mergedText = prev.Text + chunk.Text;
                    var mergedChunk = prev with
                    {
                        Text = mergedText,
                        EndIndex = chunk.EndIndex,
                        TokenCount = prev.TokenCount + chunk.TokenCount
                    };
                    refined[refined.Count - 1] = mergedChunk;
                    prev = mergedChunk;
                }
                else
                {
                    refined.Add(chunk);
                    prev = chunk;
                }
            }
            return Task.FromResult((IReadOnlyList<Chunk>)refined);
        }
    }
}
