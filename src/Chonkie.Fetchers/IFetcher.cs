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

namespace Chonkie.Fetchers
{
    /// <summary>
    /// Defines the contract for data fetchers (file, directory, etc).
    /// </summary>
    public interface IFetcher
    {
        /// <summary>
        /// Fetches text data from a source (file, directory, etc).
        /// </summary>
        /// <param name="path">Source path (file or directory).</param>
        /// <param name="filter">Optional file filter (e.g., *.txt).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of documents (path, content).</returns>
        Task<IReadOnlyList<(string Path, string Content)>> FetchAsync(string path, string? filter = null, CancellationToken cancellationToken = default);
    }
}
