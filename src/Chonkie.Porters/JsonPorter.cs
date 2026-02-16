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
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Chonkie.Core.Types;

namespace Chonkie.Porters
{
    /// <summary>
    /// Exports chunked data to a JSON file.
    /// </summary>
    public class JsonPorter : IPorter
    {
        /// <inheritdoc />
        public async Task<bool> ExportAsync(IReadOnlyList<Chunk> chunks, string destination, CancellationToken cancellationToken = default)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            using var stream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(stream, chunks, options, cancellationToken);
            return true;
        }
    }
}
