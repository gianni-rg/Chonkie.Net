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

using Chonkie.Core.Interfaces;
using Chonkie.Tokenizers;

namespace Chonkie.Core.Tests.Tokenizers;

/// <summary>
/// Unit tests for the <see cref="AutoTokenizer"/> class.
/// </summary>
public class AutoTokenizerTests
{
    [Fact]
    /// <summary>
    /// Tests that creating with 'character' identifier returns a CharacterTokenizer.
    /// </summary>
    public void Create_WithCharacterIdentifier_ReturnsCharacterTokenizer()
    {
        // Act
        var tokenizer = AutoTokenizer.Create("character");

        // Assert
        Assert.IsType<CharacterTokenizer>(tokenizer);
    }

    [Fact]
    public void Create_WithCharIdentifier_ReturnsCharacterTokenizer()
    {
        // Act
        var tokenizer = AutoTokenizer.Create("char");

        // Assert
        Assert.IsType<CharacterTokenizer>(tokenizer);
    }

    [Fact]
    public void Create_WithWordIdentifier_ReturnsWordTokenizer()
    {
        // Act
        var tokenizer = AutoTokenizer.Create("word");

        // Assert
        Assert.IsType<WordTokenizer>(tokenizer);
    }

    [Fact]
    public void Create_WithTokenizerInstance_ReturnsSameInstance()
    {
        // Arrange
        var existingTokenizer = new CharacterTokenizer();

        // Act
        var tokenizer = AutoTokenizer.Create(existingTokenizer);

        // Assert
        Assert.Same(existingTokenizer, tokenizer);
    }

    [Fact]
    public void Create_WithInvalidIdentifier_ThrowsException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => AutoTokenizer.Create("invalid"));
        Assert.Contains("Unknown tokenizer identifier", ex.Message);
        Assert.Contains("invalid", ex.Message);
    }

    [Fact]
    public void Create_WithInvalidType_ThrowsException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => AutoTokenizer.Create(123));
        Assert.Contains("Unsupported tokenizer type", ex.Message);
    }

    [Fact]
    public void CreateFromIdentifier_WithValidIdentifiers_ReturnsCorrectTokenizers()
    {
        // Act & Assert
        Assert.IsType<CharacterTokenizer>(AutoTokenizer.CreateFromIdentifier("character"));
        Assert.IsType<CharacterTokenizer>(AutoTokenizer.CreateFromIdentifier("char"));
        Assert.IsType<WordTokenizer>(AutoTokenizer.CreateFromIdentifier("word"));
        Assert.IsType<CharacterTokenizer>(AutoTokenizer.CreateFromIdentifier("CHARACTER"));
        Assert.IsType<WordTokenizer>(AutoTokenizer.CreateFromIdentifier("WORD"));
    }

    [Fact]
    public void CreateFromIdentifier_WithInvalidIdentifier_ThrowsException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => AutoTokenizer.CreateFromIdentifier("unknown"));
        Assert.Contains("Unknown tokenizer identifier", ex.Message);
    }

    [Fact]
    public void CreateCharacter_ReturnsCharacterTokenizer()
    {
        // Act
        var tokenizer = AutoTokenizer.CreateCharacter();

        // Assert
        Assert.IsType<CharacterTokenizer>(tokenizer);
        Assert.IsAssignableFrom<ITokenizer>(tokenizer);
    }

    [Fact]
    public void CreateWord_ReturnsWordTokenizer()
    {
        // Act
        var tokenizer = AutoTokenizer.CreateWord();

        // Assert
        Assert.IsType<WordTokenizer>(tokenizer);
        Assert.IsAssignableFrom<ITokenizer>(tokenizer);
    }

    [Fact]
    public void CreatedTokenizers_ImplementITokenizer()
    {
        // Arrange
        var identifiers = new[] { "character", "char", "word" };

        foreach (var identifier in identifiers)
        {
            // Act
            var tokenizer = AutoTokenizer.Create(identifier);

            // Assert
            Assert.IsAssignableFrom<ITokenizer>(tokenizer);
        }
    }
}
