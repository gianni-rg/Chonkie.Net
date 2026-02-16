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

namespace Chonkie.Core.Extensions;

using Chonkie.Core.Interfaces;

/// <summary>
/// C# 14 extension members for ITokenizer interface.
/// Provides additional utility methods and properties for tokenizer implementations.
/// </summary>
public static class TokenizerExtensions
{
    /// <summary>
    /// Instance extension members for ITokenizer - extends instances of tokenizers.
    /// These members can be called on any ITokenizer instance.
    /// </summary>
    extension(ITokenizer tokenizer)
    {
        /// <summary>
        /// Gets the tokenizer name (type name without "Tokenizer" suffix).
        /// </summary>
        public string TokenizerName => tokenizer.GetType().Name.Replace("Tokenizer", string.Empty);

        /// <summary>
        /// Checks if the given text produces zero tokens.
        /// </summary>
        /// <param name="text">The text to check.</param>
        /// <returns>True if the text produces zero tokens; otherwise, false.</returns>
        public bool IsEmpty(string text)
        {
            return tokenizer.CountTokens(text) == 0;
        }

        /// <summary>
        /// Encodes text asynchronously using Task.Run for background processing.
        /// Useful for offloading CPU-intensive tokenization to a background thread.
        /// </summary>
        /// <param name="text">The text to encode.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<IReadOnlyList<int>> EncodeAsync(
            string text,
            CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => tokenizer.Encode(text), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Decodes tokens asynchronously using Task.Run for background processing.
        /// Useful for offloading CPU-intensive decoding to a background thread.
        /// </summary>
        /// <param name="tokens">The tokens to decode.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<string> DecodeAsync(
            IReadOnlyList<int> tokens,
            CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => tokenizer.Decode(tokens), cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Static extension members for ITokenizer type - extends the type itself.
    /// These members can be called on the ITokenizer type.
    /// </summary>
    extension(ITokenizer)
    {
        /// <summary>
        /// Gets the maximum recommended token length for processing.
        /// This represents the maximum number of tokens that should be processed in a single operation.
        /// </summary>
        public static int MaxTokenLength => 1024 * 1024; // 1M tokens
    }
}
