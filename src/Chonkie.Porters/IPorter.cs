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

namespace Chonkie.Porters
{
    /// <summary>
    /// Defines the contract for exporting chunked data.
    /// </summary>
    public interface IPorter
    {
        /// <summary>
        /// Exports a list of chunks to a destination (file, stream, etc).
        /// </summary>
        /// <param name="chunks">Chunks to export.</param>
        /// <param name="destination">Destination path or identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if export succeeded.</returns>
        Task<bool> ExportAsync(IReadOnlyList<Chunk> chunks, string destination, CancellationToken cancellationToken = default);
    }
}
