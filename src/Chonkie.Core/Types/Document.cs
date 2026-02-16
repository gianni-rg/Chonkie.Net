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
/// Represents a document that can be chunked and processed.
/// </summary>
public class Document
{
    /// <summary>
    /// Unique identifier for the document.
    /// </summary>
    public string Id { get; set; } = $"doc_{Guid.NewGuid():N}";

    /// <summary>
    /// The text content of the document.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// List of chunks extracted from the document.
    /// </summary>
    public List<Chunk> Chunks { get; set; } = new();

    /// <summary>
    /// Metadata associated with the document.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Optional source path or identifier.
    /// </summary>
    public string? Source { get; set; }
}
