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
/// Represents a code block found in a markdown document.
/// </summary>
public class MarkdownCode
{
    /// <summary>
    /// The content of the code block.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The programming language of the code block.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// The start index of the code block in the document.
    /// </summary>
    public int StartIndex { get; set; }

    /// <summary>
    /// The end index of the code block in the document.
    /// </summary>
    public int EndIndex { get; set; }
}
