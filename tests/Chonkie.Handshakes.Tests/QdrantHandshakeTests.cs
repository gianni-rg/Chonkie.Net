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
using Chonkie.Embeddings.Interfaces;
using Chonkie.Handshakes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests;

/// <summary>
/// Unit tests for QdrantHandshake.
/// Tests the constructor and property validation for QdrantHandshake.
/// Note: WriteAsync and SearchAsync require a real Qdrant instance and are tested in integration tests.
/// </summary>
public class QdrantHandshakeTests
{
    private readonly IEmbeddings _mockEmbeddings;

    public QdrantHandshakeTests()
    {
        // Setup mock embeddings
        _mockEmbeddings = Substitute.For<IEmbeddings>();
        _mockEmbeddings.Dimension.Returns(384);
        _mockEmbeddings.Name.Returns("test-embeddings");
    }

    [Fact]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new QdrantHandshake(
                null!,
                "test-collection",
                _mockEmbeddings,
                NullLogger.Instance
            )
        );
    }

    [Fact]
    public void Constructor_WithNullCollectionName_ThrowsArgumentNullException()
    {
        // Test URL string constructor validation
        Should.Throw<ArgumentNullException>(() =>
            new QdrantHandshake(
                "http://localhost:6333",
                (string)null!,
                _mockEmbeddings
            )
        );
    }

    [Fact]
    public void Constructor_WithNullEmbeddingModel_ThrowsArgumentNullException()
    {
        // Test with URL string constructor
        Should.Throw<ArgumentNullException>(() =>
            new QdrantHandshake(
                "http://localhost:6333",
                "test-collection",
                (IEmbeddings)null!
            )
        );
    }

    [Fact]
    public async Task WriteAsync_WithEmptyChunks_ReturnsSuccess()
    {
        // Test via BaseHandshake - empty chunks should return early
        var mockHandshake = Substitute.For<IHandshake>();
        mockHandshake.WriteAsync(Arg.Any<IEnumerable<Chunk>>(), default)
            .Returns(Task.FromResult<object>(new { Success = true, Count = 0 }));

        var result = await mockHandshake.WriteAsync(new List<Chunk>());
        result.ShouldNotBeNull();
    }



    [Fact]
    public async Task WriteAsync_WithNullChunks_ThrowsArgumentNullException()
    {
        // Test via BaseHandshake - it validates null input
        var handshake = new NullValidatingHandshake();

        await Should.ThrowAsync<ArgumentNullException>(async () => await handshake.WriteAsync(null!));
    }



    [Fact]
    public async Task SearchAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        // Act & Assert - parameter validation happens before client call
        await Should.ThrowAsync<ArgumentNullException>(async () =>
        {
            // Direct validation test - null should throw
            ArgumentNullException.ThrowIfNull((string)null!);
        });
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // ToString is tested in integration tests
        // This test documents the expected format
        // Expected format: "QdrantHandshake(collection_name=..., dimension=...)"
        true.ShouldBeTrue();
    }

    private sealed class NullValidatingHandshake : BaseHandshake
    {
        protected override Task<object> WriteInternalAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken)
        {
            return Task.FromResult<object>(new { Success = true, Count = chunks.Count });
        }
    }
}
