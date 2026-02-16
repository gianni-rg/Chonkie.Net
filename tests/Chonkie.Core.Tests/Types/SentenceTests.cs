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
/// Unit tests for the <see cref="Sentence"/> type.
/// </summary>
public class SentenceTests
{
    [Fact]
    /// <summary>
    /// Tests that the constructor creates a sentence with required text.
    /// </summary>
    public void Constructor_WithRequiredText_CreatesSentence()
    {
        // Arrange & Act
        var sentence = new Sentence
        {
            Text = "This is a sentence."
        };

        // Assert
        Assert.Equal("This is a sentence.", sentence.Text);
        Assert.Equal(0, sentence.StartIndex);
        Assert.Equal(0, sentence.EndIndex);
        Assert.Equal(0, sentence.TokenCount);
    }

    [Fact]
    public void Constructor_WithAllProperties_CreatesSentence()
    {
        // Arrange & Act
        var sentence = new Sentence
        {
            Text = "This is a sentence.",
            StartIndex = 10,
            EndIndex = 29,
            TokenCount = 5
        };

        // Assert
        Assert.Equal("This is a sentence.", sentence.Text);
        Assert.Equal(10, sentence.StartIndex);
        Assert.Equal(29, sentence.EndIndex);
        Assert.Equal(5, sentence.TokenCount);
    }

    [Fact]
    public void RecordEquality_ComparesValues()
    {
        // Arrange
        var sentence1 = new Sentence
        {
            Text = "Test",
            StartIndex = 0,
            EndIndex = 4,
            TokenCount = 1
        };

        var sentence2 = new Sentence
        {
            Text = "Test",
            StartIndex = 0,
            EndIndex = 4,
            TokenCount = 1
        };

        var sentence3 = new Sentence
        {
            Text = "Different",
            StartIndex = 0,
            EndIndex = 4,
            TokenCount = 1
        };

        // Act & Assert
        Assert.Equal(sentence1, sentence2);
        Assert.NotEqual(sentence1, sentence3);
    }
}
