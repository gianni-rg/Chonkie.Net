using Chonkie.Core.Interfaces;
using Chonkie.Tokenizers;

namespace Chonkie.Core.Tests.Tokenizers;

public class AutoTokenizerTests
{
    [Fact]
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
