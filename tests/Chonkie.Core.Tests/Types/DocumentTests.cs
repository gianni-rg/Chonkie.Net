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

using Chonkie.Core.Types;

namespace Chonkie.Core.Tests.Types;

/// <summary>
/// Unit tests for the <see cref="Document"/> type.
/// </summary>
public class DocumentTests
{
    [Fact]
    /// <summary>
    /// Tests that the constructor creates a document with required content.
    /// </summary>
    public void Constructor_WithRequiredContent_CreatesDocument()
    {
        // Arrange & Act
        var document = new Document
        {
            Content = "Test content"
        };

        // Assert
        Assert.Equal("Test content", document.Content);
        Assert.StartsWith("doc_", document.Id);
        Assert.Empty(document.Chunks);
        Assert.Empty(document.Metadata);
        Assert.Null(document.Source);
    }

    [Fact]
    public void Constructor_WithAllProperties_CreatesDocument()
    {
        // Arrange
        var chunks = new List<Chunk>
        {
            new Chunk { Text = "Chunk 1" },
            new Chunk { Text = "Chunk 2" }
        };
        var metadata = new Dictionary<string, object>
        {
            ["key1"] = "value1",
            ["key2"] = 42
        };

        // Act
        var document = new Document
        {
            Id = "custom_doc_id",
            Content = "Test content",
            Chunks = chunks,
            Metadata = metadata,
            Source = "test.txt"
        };

        // Assert
        Assert.Equal("custom_doc_id", document.Id);
        Assert.Equal("Test content", document.Content);
        Assert.Equal(2, document.Chunks.Count);
        Assert.Equal(2, document.Metadata.Count);
        Assert.Equal("test.txt", document.Source);
    }

    [Fact]
    public void Chunks_CanBeModified()
    {
        // Arrange
        var document = new Document { Content = "Test" };
        var chunk = new Chunk { Text = "Test chunk" };

        // Act
        document.Chunks.Add(chunk);

        // Assert
        Assert.Single(document.Chunks);
        Assert.Same(chunk, document.Chunks[0]);
    }

    [Fact]
    public void Metadata_CanBeModified()
    {
        // Arrange
        var document = new Document { Content = "Test" };
        var now = DateTime.Now;

        // Act
        document.Metadata["author"] = "Test Author";
        document.Metadata["date"] = now;

        // Assert
        Assert.Equal(2, document.Metadata.Count);
        Assert.Equal("Test Author", document.Metadata["author"]);
    }
}
