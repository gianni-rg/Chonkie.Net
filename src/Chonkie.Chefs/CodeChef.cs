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
using System.Threading;
using System.Threading.Tasks;

namespace Chonkie.Chefs
{
    /// <summary>
    /// Code preprocessing that preserves exact whitespace, newlines, and formatting.
    /// Unlike TextChef, this does not normalize whitespace or remove newlines.
    /// </summary>
    public class CodeChef : IChef
    {
        /// <inheritdoc />
        public Task<string> ProcessAsync(string text, CancellationToken cancellationToken = default)
        {
            // Pass through unchanged - preserve exact formatting for code
            return Task.FromResult(text ?? string.Empty);
        }

        /// <summary>
        /// Processes code text span directly without allocations.
        /// C# 14 implicit span conversion allows passing strings directly.
        /// </summary>
        /// <param name="text">The code text span to process.</param>
        /// <returns>The text as a string (pass-through for code).</returns>
        public string Process(ReadOnlySpan<char> text)
        {
            // Code chef is pass-through, but we must convert to string
            return text.ToString();
        }
    }
}
