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
/// Represents a document containing markdown content with extracted structural elements.
/// </summary>
public class MarkdownDocument : Document
{
    /// <summary>
    /// List of tables found in the markdown document.
    /// </summary>
    public List<MarkdownTable> Tables { get; set; } = new();

    /// <summary>
    /// List of code blocks found in the markdown document.
    /// </summary>
    public List<MarkdownCode> Code { get; set; } = new();

    /// <summary>
    /// List of images found in the markdown document.
    /// </summary>
    public List<MarkdownImage> Images { get; set; } = new();
}
