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

using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Chonkie.Chefs
{
    /// <summary>
    /// Extracts and processes tables from text (basic implementation).
    /// </summary>
    public class TableChef : IChef
    {
        /// <inheritdoc />
        public Task<string> ProcessAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Task.FromResult(string.Empty);

            // Simple markdown table extraction (first table only)
            var match = Regex.Match(text, @"((?:\|.+\|\r?\n)+)");
            if (!match.Success)
                return Task.FromResult(string.Empty);

            var tableText = match.Groups[1].Value;
            // Optionally, parse to DataTable or CSV
            return Task.FromResult(tableText);
        }
    }
}
