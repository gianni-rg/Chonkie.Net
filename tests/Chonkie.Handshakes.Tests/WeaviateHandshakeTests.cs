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
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using Weaviate.Client;
using Xunit;

namespace Chonkie.Handshakes.Tests;

/// <summary>
/// Unit tests for WeaviateHandshake.
/// Tests the Weaviate vector database integration constructor validation and properties.
/// NOTE: WriteAsync and SearchAsync require a real Weaviate instance and are tested in integration tests.
/// </summary>
public class WeaviateHandshakeTests
{
    private readonly IEmbeddings _mockEmbeddings;

    public WeaviateHandshakeTests()
    {
        _mockEmbeddings = Substitute.For<IEmbeddings>();
        _mockEmbeddings.Dimension.Returns(384);
        _mockEmbeddings.Name.Returns("test-embeddings");
    }

    [Fact]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new WeaviateHandshake(null!, "TestClass", _mockEmbeddings, NullLogger.Instance)
        );
    }

    [Fact]
    public async Task CreateCloudAsync_WithNullUrl_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await WeaviateHandshake.CreateCloudAsync(null!, "api-key", "TestClass", _mockEmbeddings, NullLogger.Instance)
        );
    }

    [Fact]
    public async Task CreateCloudAsync_WithEmptyUrl_ThrowsArgumentException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(async () =>
            await WeaviateHandshake.CreateCloudAsync("", "api-key", "TestClass", _mockEmbeddings, NullLogger.Instance)
        );
    }

    [Fact]
    public async Task CreateCloudAsync_WithWhitespaceUrl_ThrowsArgumentException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(async () =>
            await WeaviateHandshake.CreateCloudAsync("   ", "api-key", "TestClass", _mockEmbeddings, NullLogger.Instance)
        );
    }

    [Fact]
    public async Task CreateCloudAsync_WithNullApiKey_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await WeaviateHandshake.CreateCloudAsync("https://my-cluster.weaviate.cloud", null!, "TestClass", _mockEmbeddings, NullLogger.Instance)
        );
    }

    [Fact]
    public async Task CreateCloudAsync_WithEmptyApiKey_ThrowsArgumentException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(async () =>
            await WeaviateHandshake.CreateCloudAsync("https://my-cluster.weaviate.cloud", "", "TestClass", _mockEmbeddings, NullLogger.Instance)
        );
    }

    [Fact]
    public async Task CreateCloudAsync_WithNullClassName_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await WeaviateHandshake.CreateCloudAsync("https://my-cluster.weaviate.cloud", "api-key", null!, _mockEmbeddings, NullLogger.Instance)
        );
    }

    [Fact]
    public async Task CreateCloudAsync_WithNullEmbeddings_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await WeaviateHandshake.CreateCloudAsync("https://my-cluster.weaviate.cloud", "api-key", "TestClass", null!, NullLogger.Instance)
        );
    }
}
