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
/// Represents a sentence with position and token information.
/// </summary>
public record Sentence
{
    /// <summary>
    /// The text content of the sentence.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Starting character index in the original text.
    /// </summary>
    public int StartIndex { get; init; }

    /// <summary>
    /// Ending character index in the original text.
    /// </summary>
    public int EndIndex { get; init; }

    /// <summary>
    /// Number of tokens in this sentence.
    /// </summary>
    public int TokenCount { get; init; }
}
