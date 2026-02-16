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

namespace Chonkie.Core.Types;

/// <summary>
/// Express the chunking rules at a specific level for the recursive chunker.
/// </summary>
public class RecursiveLevel
{
    /// <summary>
    /// Gets the delimiters to use for chunking at this level.
    /// </summary>
    public IReadOnlyList<string>? Delimiters { get; init; }

    /// <summary>
    /// Gets whether to use whitespace as a delimiter.
    /// </summary>
    public bool Whitespace { get; init; }

    /// <summary>
    /// Gets whether to include delimiters in previous chunk, next chunk or not at all.
    /// </summary>
    public string? IncludeDelimiter { get; init; } = "prev";

    /// <summary>
    /// Initializes a new instance of the <see cref="RecursiveLevel"/> class.
    /// </summary>
    public RecursiveLevel()
    {
        Validate();
    }

    /// <summary>
    /// Validates that the recursive level configuration is valid.
    /// </summary>
    private void Validate()
    {
        // Check for mutually exclusive options
        var activeOptions = 0;
        if (Delimiters != null && Delimiters.Count > 0) activeOptions++;
        if (Whitespace) activeOptions++;

        if (activeOptions > 1)
        {
            throw new InvalidOperationException(
                "Cannot use multiple splitting methods simultaneously. Choose one of: delimiters or whitespace.");
        }

        if (Delimiters != null)
        {
            if (Delimiters.Any(d => string.IsNullOrEmpty(d)))
            {
                throw new ArgumentException("Delimiters cannot contain empty strings.");
            }

            if (Delimiters.Any(d => d == " "))
            {
                throw new ArgumentException(
                    "Delimiters cannot be whitespace only. Set Whitespace to true instead.");
            }
        }

        if (IncludeDelimiter != null && IncludeDelimiter != "prev" && IncludeDelimiter != "next")
        {
            throw new ArgumentException("IncludeDelimiter must be 'prev', 'next' or null.");
        }
    }

    /// <summary>
    /// Returns a string representation of the RecursiveLevel.
    /// </summary>
    public override string ToString()
    {
        var delimStr = Delimiters != null ? $"[{string.Join(", ", Delimiters)}]" : "null";
        return $"RecursiveLevel(delimiters={delimStr}, whitespace={Whitespace}, include_delim={IncludeDelimiter})";
    }
}

/// <summary>
/// Expression rules for recursive chunking containing multiple levels.
/// </summary>
public class RecursiveRules
{
    /// <summary>
    /// Gets the list of recursive levels defining the chunking hierarchy.
    /// </summary>
    public IReadOnlyList<RecursiveLevel> Levels { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecursiveRules"/> class with custom levels.
    /// </summary>
    /// <param name="levels">The list of recursive levels.</param>
    public RecursiveRules(IReadOnlyList<RecursiveLevel>? levels = null)
    {
        if (levels == null)
        {
            // Default hierarchy: paragraphs -> sentences -> pauses -> words -> tokens
            var paragraphs = new RecursiveLevel
            {
                Delimiters = new[] { "\n\n", "\r\n", "\n", "\r" },
                IncludeDelimiter = "prev"
            };

            var sentences = new RecursiveLevel
            {
                Delimiters = new[] { ". ", "! ", "? " },
                IncludeDelimiter = "prev"
            };

            var pauses = new RecursiveLevel
            {
                Delimiters = new[] { "{", "}", "\"", "[", "]", "<", ">", "(", ")", ":", ";", ",", "—", "|", "~", "-", "...", "`", "'" },
                IncludeDelimiter = "prev"
            };

            var words = new RecursiveLevel
            {
                Whitespace = true,
                IncludeDelimiter = null
            };

            var tokens = new RecursiveLevel
            {
                Delimiters = null,
                Whitespace = false,
                IncludeDelimiter = null
            };

            Levels = new[] { paragraphs, sentences, pauses, words, tokens };
        }
        else
        {
            Levels = levels;
        }
    }

    /// <summary>
    /// Gets the number of levels in this recursive rules configuration.
    /// </summary>
    public int Count => Levels.Count;

    /// <summary>
    /// Gets the recursive level at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The recursive level at the specified index.</returns>
    public RecursiveLevel this[int index] => Levels[index];

    /// <summary>
    /// Returns a string representation of the RecursiveRules.
    /// </summary>
    public override string ToString()
    {
        return $"RecursiveRules(levels={Levels.Count})";
    }
}
