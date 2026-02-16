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

namespace Chonkie.Refineries
{
    /// <summary>
    /// Defines the contract for chunk refineries (post-processing).
    /// </summary>
    public interface IRefinery
    {
        /// <summary>
        /// Refines a list of chunks (e.g., merging, adding embeddings).
        /// </summary>
        /// <param name="chunks">Input chunks.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Refined chunks.</returns>
        Task<IReadOnlyList<Chunk>> RefineAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken = default);
    }
}
