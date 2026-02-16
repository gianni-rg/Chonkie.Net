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

using Chonkie.Core.Interfaces;

namespace Chonkie.Tokenizers;

/// <summary>
/// Abstract base class for tokenizers with common functionality.
/// </summary>
public abstract class BaseTokenizer : ITokenizer
{
    /// <summary>
    /// The vocabulary of tokens.
    /// </summary>
    protected readonly List<string> Vocab = new();

    /// <summary>
    /// Mapping from token to token ID.
    /// </summary>
    protected readonly Dictionary<string, int> Token2Id = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseTokenizer"/> class.
    /// </summary>
    protected BaseTokenizer()
    {
        // Add space to vocabulary by default
        AddToVocabulary(" ");
    }

    /// <summary>
    /// Adds a token to the vocabulary if it doesn't exist.
    /// </summary>
    /// <param name="token">The token to add.</param>
    /// <returns>The ID of the token.</returns>
    protected int AddToVocabulary(string token)
    {
        if (Token2Id.TryGetValue(token, out var existingId))
        {
            return existingId;
        }

        var id = Vocab.Count;
        Vocab.Add(token);
        Token2Id[token] = id;
        return id;
    }

    /// <inheritdoc/>
    public abstract IReadOnlyList<int> Encode(string text);

    /// <inheritdoc/>
    public abstract string Decode(IReadOnlyList<int> tokens);

    /// <inheritdoc/>
    public virtual int CountTokens(string text)
    {
        return Encode(text).Count;
    }

    /// <inheritdoc/>
    public virtual IReadOnlyList<IReadOnlyList<int>> EncodeBatch(IEnumerable<string> texts)
    {
        return texts.Select(Encode).ToList();
    }

    /// <inheritdoc/>
    public virtual IReadOnlyList<string> DecodeBatch(IEnumerable<IReadOnlyList<int>> tokenSequences)
    {
        return tokenSequences.Select(Decode).ToList();
    }

    /// <inheritdoc/>
    public virtual IReadOnlyList<int> CountTokensBatch(IEnumerable<string> texts)
    {
        return texts.Select(CountTokens).ToList();
    }

    /// <summary>
    /// Gets the current vocabulary.
    /// </summary>
    /// <returns>A read-only list of tokens in the vocabulary.</returns>
    public IReadOnlyList<string> GetVocabulary() => Vocab.AsReadOnly();

    /// <summary>
    /// Gets the token-to-ID mapping.
    /// </summary>
    /// <returns>A read-only dictionary of token-to-ID mappings.</returns>
    public IReadOnlyDictionary<string, int> GetTokenMapping() => Token2Id;
}
