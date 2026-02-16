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

namespace Chonkie.Embeddings.Interfaces
{
    /// <summary>
    /// Defines the contract for embedding providers.
    /// </summary>
    public interface IEmbeddings
    {
        /// <summary>
        /// Gets the name of the embedding provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the dimension of the embedding vectors.
        /// </summary>
        int Dimension { get; }

        /// <summary>
        /// Generates an embedding for a single text.
        /// </summary>
        Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates embeddings for a batch of texts.
        /// </summary>
        Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default);

        /// <summary>
        /// Computes the cosine similarity between two embedding vectors.
        /// </summary>
        /// <param name="u">First embedding vector.</param>
        /// <param name="v">Second embedding vector.</param>
        /// <returns>Cosine similarity score between 0 and 1.</returns>
        float Similarity(float[] u, float[] v);
    }
}
