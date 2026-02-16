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

using Chonkie.Embeddings.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Chonkie.Handshakes.Tests;

/// <summary>
/// Unit tests for PgvectorHandshake.
/// </summary>
public class PgvectorHandshakeTests
{
    private readonly IEmbeddings _mockEmbeddings;

    /// <summary>
    /// Initializes a new instance of the <see cref="PgvectorHandshakeTests"/> class.
    /// </summary>
    public PgvectorHandshakeTests()
    {
        _mockEmbeddings = Substitute.For<IEmbeddings>();
        _mockEmbeddings.Dimension.Returns(384);
        _mockEmbeddings.Name.Returns("test-embeddings");
    }

    /// <summary>
    /// Ensures null embeddings are rejected.
    /// </summary>
    [Fact]
    public void Constructor_WithNullEmbeddingModel_ThrowsArgumentNullException()
    {
        var options = new PgvectorHandshakeOptions
        {
            ConnectionString = "Host=localhost;Username=test;Password=test;Database=test;",
            CollectionName = "test_collection"
        };

        Should.Throw<ArgumentNullException>(() =>
            new PgvectorHandshake(options, null!)
        );
    }

    /// <summary>
    /// Ensures invalid collection names are rejected.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-name")]
    public void Constructor_WithInvalidCollectionName_ThrowsArgumentException(string collectionName)
    {
        var options = new PgvectorHandshakeOptions
        {
            ConnectionString = "Host=localhost;Username=test;Password=test;Database=test;",
            CollectionName = collectionName
        };

        Should.Throw<ArgumentException>(() =>
            new PgvectorHandshake(options, _mockEmbeddings)
        );
    }

    /// <summary>
    /// Ensures valid parameters are retained.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_SetsProperties()
    {
        var options = new PgvectorHandshakeOptions
        {
            ConnectionString = "Host=localhost;Username=test;Password=test;Database=test;",
            CollectionName = "test_collection",
            VectorDimensions = 384
        };

        var handshake = new PgvectorHandshake(options, _mockEmbeddings, NullLogger.Instance);

        handshake.CollectionName.ShouldBe("test_collection");
        handshake.VectorDimensions.ShouldBe(384);
    }

    /// <summary>
    /// Ensures custom vector dimensions are respected.
    /// </summary>
    [Fact]
    public void Constructor_WithCustomVectorDimensions_UsesProvidedValue()
    {
        var options = new PgvectorHandshakeOptions
        {
            ConnectionString = "Host=localhost;Username=test;Password=test;Database=test;",
            CollectionName = "test_collection",
            VectorDimensions = 256
        };

        var handshake = new PgvectorHandshake(options, _mockEmbeddings);

        handshake.VectorDimensions.ShouldBe(256);
    }

    /// <summary>
    /// Ensures a null data source is rejected.
    /// </summary>
    [Fact]
    public void Constructor_WithNullDataSource_ThrowsArgumentNullException()
    {
        var options = new PgvectorHandshakeOptions
        {
            CollectionName = "test_collection"
        };

        Should.Throw<ArgumentNullException>(() =>
            new PgvectorHandshake(null!, options, _mockEmbeddings)
        );
    }

    /// <summary>
    /// Ensures data source constructor retains properties.
    /// </summary>
    [Fact]
    public void Constructor_WithDataSource_SetsProperties()
    {
        var options = new PgvectorHandshakeOptions
        {
            CollectionName = "test_collection",
            VectorDimensions = 384
        };
        var dataSource = NpgsqlDataSource.Create("Host=localhost;Username=test;Password=test;Database=test;");

        var handshake = new PgvectorHandshake(dataSource, options, _mockEmbeddings);

        handshake.CollectionName.ShouldBe("test_collection");
        handshake.VectorDimensions.ShouldBe(384);
    }

    /// <summary>
    /// Ensures the string representation is stable.
    /// </summary>
    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var options = new PgvectorHandshakeOptions
        {
            ConnectionString = "Host=localhost;Username=test;Password=test;Database=test;",
            CollectionName = "test_collection",
            VectorDimensions = 384
        };

        var handshake = new PgvectorHandshake(options, _mockEmbeddings);

        var result = handshake.ToString();

        result.ShouldContain("test_collection");
        result.ShouldContain("384");
    }

    /// <summary>
    /// Ensures invalid index option keys are rejected for HNSW method.
    /// </summary>
    [Fact]
    public async Task CreateIndexAsync_WithInvalidOptionKeyForHnsw_ThrowsArgumentException()
    {
        var options = new PgvectorHandshakeOptions
        {
            ConnectionString = "Host=localhost;Username=test;Password=test;Database=test;",
            CollectionName = "test_collection"
        };

        var handshake = new PgvectorHandshake(options, _mockEmbeddings);

        var invalidOptions = new Dictionary<string, int> { { "invalid_key", 10 } };

        await Should.ThrowAsync<ArgumentException>(() =>
            handshake.CreateIndexAsync("hnsw", "vector_cosine_ops", invalidOptions)
        );
    }

    /// <summary>
    /// Ensures non-positive integer values are rejected in index options.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CreateIndexAsync_WithNonPositiveIntegerValue_ThrowsArgumentException(int invalidValue)
    {
        var options = new PgvectorHandshakeOptions
        {
            ConnectionString = "Host=localhost;Username=test;Password=test;Database=test;",
            CollectionName = "test_collection"
        };

        var handshake = new PgvectorHandshake(options, _mockEmbeddings);

        var invalidOptions = new Dictionary<string, int> { { "m", invalidValue } };

        await Should.ThrowAsync<ArgumentException>(() =>
            handshake.CreateIndexAsync("hnsw", "vector_cosine_ops", invalidOptions)
        );
    }
}
