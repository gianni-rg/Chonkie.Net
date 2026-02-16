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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chonkie.Fetchers
{
    /// <summary>
    /// Fetches text files from a file or directory, with optional filtering.
    /// </summary>
    public class FileFetcher : IFetcher
    {
        /// <inheritdoc />
        public async Task<IReadOnlyList<(string Path, string Content)>> FetchAsync(string path, string? filter = null, CancellationToken cancellationToken = default)
        {
            var results = new List<(string Path, string Content)>();
            IEnumerable<string> files;

            if (Directory.Exists(path))
            {
                files = Directory.EnumerateFiles(path, filter ?? "*.*", SearchOption.AllDirectories);
            }
            else if (File.Exists(path))
            {
                files = new[] { path };
            }
            else
            {
                throw new FileNotFoundException($"Path not found: {path}");
            }

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var content = await File.ReadAllTextAsync(file, cancellationToken);
                results.Add((file, content));
            }

            return results;
        }
    }
}
