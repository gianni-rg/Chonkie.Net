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
using Chonkie.Embeddings.OpenAI;

namespace Chonkie.Embeddings.Tests.Providers
{
    /// <summary>
    /// Unit tests for the <see cref="OpenAIEmbeddings"/> provider.
    /// </summary>
    public class OpenAIEmbeddingsTests
    {
        /// <summary>
        /// Tests that the constructor initializes properties correctly.
        /// </summary>
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange & Act
            var embeddings = new OpenAIEmbeddings("test-api-key", "text-embedding-ada-002", 1536);

            // Assert
            Assert.Equal("openai", embeddings.Name);
            Assert.Equal(1536, embeddings.Dimension);
        }

        /// <summary>
        /// Tests that the constructor throws an exception when API key is null.
        /// </summary>
        [Fact]
        public void Constructor_ThrowsException_WhenApiKeyIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new OpenAIEmbeddings(null!));
        }

        // Note: Integration tests for actual API calls should be in a separate test suite
        // and run only when API keys are available
    }
}
