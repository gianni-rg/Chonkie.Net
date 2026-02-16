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
using Chonkie.Embeddings.Azure;

namespace Chonkie.Embeddings.Tests.Providers
{
    /// <summary>
    /// Unit tests for the <see cref="AzureOpenAIEmbeddings"/> provider.
    /// </summary>
    public class AzureOpenAIEmbeddingsTests
    {
        /// <summary>
        /// Tests that the constructor initializes properties correctly.
        /// </summary>
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange & Act
            var embeddings = new AzureOpenAIEmbeddings(
                "https://test.openai.azure.com",
                "test-api-key",
                "test-deployment",
                1536
            );

            // Assert
            Assert.Equal("azure-openai", embeddings.Name);
            Assert.Equal(1536, embeddings.Dimension);
        }

        // Note: Integration tests for actual API calls should be in a separate test suite
        // and run only when Azure credentials are available
    }
}
