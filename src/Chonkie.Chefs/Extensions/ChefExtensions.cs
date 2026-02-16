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

namespace Chonkie.Chefs.Extensions;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// C# 14 extension members for IChef interface.
/// Provides additional utility methods and properties for chef implementations.
/// </summary>
public static class ChefExtensions
{
    /// <summary>
    /// Extension members for IChef instances.
    /// </summary>
    extension(IChef chef)
    {
        /// <summary>
        /// Gets the chef type name (type name without "Chef" suffix).
        /// </summary>
        public string ChefType => chef.GetType().Name.Replace("Chef", string.Empty);

        /// <summary>
        /// Processes multiple texts in batch asynchronously.
        /// </summary>
        /// <param name="texts">The texts to process.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation with processed texts.</returns>
        public async Task<IReadOnlyList<string>> ProcessBatchAsync(
            IEnumerable<string> texts,
            CancellationToken cancellationToken = default)
        {
            var results = new List<string>();
            foreach (var text in texts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var processed = await chef.ProcessAsync(text, cancellationToken);
                results.Add(processed);
            }
            return results;
        }

        /// <summary>
        /// Checks if the chef would modify the given text.
        /// </summary>
        /// <param name="text">The text to check.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the text would be modified; otherwise, false.</returns>
        public async Task<bool> WouldModifyAsync(
            string text,
            CancellationToken cancellationToken = default)
        {
            var processed = await chef.ProcessAsync(text, cancellationToken);
            return !string.Equals(text, processed, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Static extension members for IChef type.
    /// </summary>
    extension(IChef)
    {
        /// <summary>
        /// Gets an empty string for fallback scenarios.
        /// </summary>
        public static string Empty => string.Empty;
    }
}
