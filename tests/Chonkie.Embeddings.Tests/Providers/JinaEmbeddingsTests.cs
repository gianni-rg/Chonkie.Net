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

using Xunit;
using Chonkie.Embeddings.Jina;

namespace Chonkie.Embeddings.Tests.Providers
{
    /// <summary>
    /// Unit tests for the <see cref="JinaEmbeddings"/> provider.
    /// </summary>
    public class JinaEmbeddingsTests
    {
        /// <summary>
        /// Tests that the constructor initializes properties correctly.
        /// </summary>
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange & Act
            var embeddings = new JinaEmbeddings("test-api-key", "jina-embeddings-v2-base-en", 768);

            // Assert
            Assert.Equal("jina", embeddings.Name);
            Assert.Equal(768, embeddings.Dimension);
        }

        /// <summary>
        /// Tests that the constructor throws an exception when API key is null.
        /// </summary>
        [Fact]
        public void Constructor_ThrowsException_WhenApiKeyIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new JinaEmbeddings(null!));
        }

        /// <summary>
        /// Tests that the constructor uses default model.
        /// </summary>
        [Fact]
        public void Constructor_UsesDefaultModel()
        {
            // Arrange & Act
            var embeddings = new JinaEmbeddings("test-api-key");

            // Assert
            Assert.Equal("jina", embeddings.Name);
            Assert.Equal(768, embeddings.Dimension);
        }

        /// <summary>
        /// Tests that the Dimension property returns the correct value.
        /// </summary>
        [Fact]
        public void DimensionProperty_ReturnsCorrectValue()
        {
            // Arrange
            var embeddings = new JinaEmbeddings("test-api-key", "jina-embeddings-v2-base-en", 512);

            // Act
            var dimension = embeddings.Dimension;

            // Assert
            Assert.Equal(512, dimension);
        }

        /// <summary>
        /// Tests that ToString returns a formatted string.
        /// </summary>
        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var embeddings = new JinaEmbeddings("test-api-key");

            // Act
            var result = embeddings.ToString();

            // Assert
            Assert.Contains("JinaEmbeddings", result);
            Assert.Contains("jina", result);
        }

        // Note: Integration tests for actual API calls should be in a separate test suite
        // and run only when API keys are available
    }
}
