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
using Chonkie.Handshakes;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests;

/// <summary>
/// Unit tests for MilvusHandshake constructor validation and parameter handling.
/// </summary>
public class MilvusHandshakeTests
{
    [Fact]
    public void Constructor_WithDefaultParameters_InitializesSuccessfully()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act & Assert - should not throw
        var handshake = new MilvusHandshake(embeddingModel, "http://localhost:19530");

        handshake.ShouldNotBeNull();
        handshake.Dimension.ShouldBe(384);
    }

    [Fact]
    public void Constructor_WithNullEmbeddingModel_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new MilvusHandshake(null!));
    }

    [Fact]
    public void Constructor_WithCustomHostAndPort_InitializesSuccessfully()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new MilvusHandshake(
            embeddingModel,
            serverUrl: "http://milvus.example.com:19530");

        // Assert
        handshake.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithRandomCollectionName_GeneratesUniqueName()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new MilvusHandshake(
            embeddingModel,
            "http://localhost:19530",
            collectionName: "random");

        // Assert
        handshake.CollectionName.ShouldNotBe("random");
        handshake.CollectionName.ShouldStartWith("collection_");
    }

    [Fact]
    public void Constructor_WithCustomCollectionName_UsesProvidedName()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);

        // Act
        var handshake = new MilvusHandshake(
            embeddingModel,
            "http://localhost:19530",
            collectionName: "my_collection");

        // Assert
        handshake.CollectionName.ShouldBe("my_collection");
    }

    [Fact]
    public void Constructor_WithServerUrl_InitializesSuccessfully()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(768);

        // Act
        var handshake = new MilvusHandshake(
            embeddingModel,
            serverUrl: "http://milvus-server:19530");

        // Assert
        handshake.Dimension.ShouldBe(768);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);
        var handshake = new MilvusHandshake(
            embeddingModel,
            "http://localhost:19530",
            collectionName: "test_collection");

        // Act
        var result = handshake.ToString();

        // Assert
        result.ShouldContain("MilvusHandshake");
        result.ShouldContain("test_collection");
    }

    [Fact]
    public void Dimension_ReturnsEmbeddingModelDimension()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(1024);

        // Act
        var handshake = new MilvusHandshake(embeddingModel, "http://localhost:19530");

        // Assert
        handshake.Dimension.ShouldBe(1024);
    }

    [Fact]
    public async Task SearchAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        var embeddingModel = NSubstitute.Substitute.For<Chonkie.Embeddings.Interfaces.IEmbeddings>();
        embeddingModel.Dimension.Returns(384);
        var handshake = new MilvusHandshake(embeddingModel, "http://localhost:19530");

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            handshake.SearchAsync(null!));
    }

}
