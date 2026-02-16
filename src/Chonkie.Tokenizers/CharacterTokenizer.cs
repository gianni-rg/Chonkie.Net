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

namespace Chonkie.Tokenizers;

/// <summary>
/// Character-level tokenizer that splits text into individual characters.
/// </summary>
public class CharacterTokenizer : BaseTokenizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterTokenizer"/> class.
    /// </summary>
    public CharacterTokenizer()
    {
    }

    /// <inheritdoc/>
    public override IReadOnlyList<int> Encode(string text)
    {
        var encoded = new List<int>(text.Length);

        foreach (var character in text)
        {
            var token = character.ToString();
            var id = AddToVocabulary(token);
            encoded.Add(id);
        }

        return encoded;
    }

    /// <inheritdoc/>
    public override string Decode(IReadOnlyList<int> tokens)
    {
        if (tokens == null || tokens.Count == 0)
        {
            return string.Empty;
        }

        var chars = new char[tokens.Count];
        for (int i = 0; i < tokens.Count; i++)
        {
            var tokenId = tokens[i];
            if (tokenId < 0 || tokenId >= Vocab.Count)
            {
                throw new ArgumentException($"Token ID {tokenId} not found in vocabulary.", nameof(tokens));
            }

            var token = Vocab[tokenId];
            if (token.Length != 1)
            {
                throw new InvalidOperationException($"Expected single character token, but got '{token}'.");
            }

            chars[i] = token[0];
        }

        return new string(chars);
    }

    /// <inheritdoc/>
    public override int CountTokens(string text)
    {
        return text.Length;
    }

    /// <summary>
    /// Counts the number of tokens in the given text span.
    /// C# 14 implicit span conversion allows passing strings directly.
    /// </summary>
    /// <param name="text">The text span to count tokens for.</param>
    /// <returns>The number of tokens (characters) in the text.</returns>
    public int CountTokens(ReadOnlySpan<char> text)
    {
        return text.Length;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"CharacterTokenizer(vocab_size={Vocab.Count})";
    }
}
