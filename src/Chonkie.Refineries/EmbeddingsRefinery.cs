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
