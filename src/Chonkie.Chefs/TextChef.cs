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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Chonkie.Chefs
{
    /// <summary>
    /// Basic text preprocessing: trims, normalizes whitespace, removes control characters.
    /// </summary>
    public class TextChef : IChef
    {
        /// <inheritdoc />
        public Task<string> ProcessAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Task.FromResult(string.Empty);

            // Remove control characters
            var cleaned = Regex.Replace(text, "[\x00-\x1F\x7F]", "");
            // Normalize whitespace
            cleaned = Regex.Replace(cleaned, "\\s+", " ").Trim();
            return Task.FromResult(cleaned);
        }

        /// <summary>
        /// Processes text span directly without creating intermediate strings.
        /// C# 14 implicit span conversion allows passing strings directly.
        /// </summary>
        /// <param name="text">The text span to process.</param>
        /// <returns>Processed text.</returns>
        public string Process(ReadOnlySpan<char> text)
        {
            if (text.IsEmpty || text.IsWhiteSpace())
                return string.Empty;

            // Convert to string for regex processing (regex doesn't support spans directly)
            var str = text.ToString();
            var cleaned = Regex.Replace(str, "[\x00-\x1F\x7F]", "");
            cleaned = Regex.Replace(cleaned, "\\s+", " ").Trim();
            return cleaned;
        }
    }
}
