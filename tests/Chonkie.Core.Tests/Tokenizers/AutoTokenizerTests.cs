using Chonkie.Core.Interfaces;
using Chonkie.Tokenizers;
using FluentAssertions;

namespace Chonkie.Core.Tests.Tokenizers;

public class AutoTokenizerTests
{
    [Fact]
    public void Create_WithCharacterIdentifier_ReturnsCharacterTokenizer()
    {
        // Act
        var tokenizer = AutoTokenizer.Create("character");

        // Assert
        tokenizer.Should().BeOfType<CharacterTokenizer>();
    }

    [Fact]
    public void Create_WithCharIdentifier_ReturnsCharacterTokenizer()
    {
        // Act
        var tokenizer = AutoTokenizer.Create("char");

        // Assert
        tokenizer.Should().BeOfType<CharacterTokenizer>();
    }

    [Fact]
    public void Create_WithWordIdentifier_ReturnsWordTokenizer()
    {
        // Act
        var tokenizer = AutoTokenizer.Create("word");

        // Assert
        tokenizer.Should().BeOfType<WordTokenizer>();
    }

    [Fact]
    public void Create_WithTokenizerInstance_ReturnsSameInstance()
    {
        // Arrange
        var existingTokenizer = new CharacterTokenizer();

        // Act
        var tokenizer = AutoTokenizer.Create(existingTokenizer);

        // Assert
        tokenizer.Should().BeSameAs(existingTokenizer);
    }

    [Fact]
    public void Create_WithInvalidIdentifier_ThrowsException()
    {
        // Act
        var act = () => AutoTokenizer.Create("invalid");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unknown tokenizer identifier*")
            .WithMessage("*invalid*");
    }

    [Fact]
    public void Create_WithInvalidType_ThrowsException()
    {
        // Act
        var act = () => AutoTokenizer.Create(123);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unsupported tokenizer type*");
    }

    [Fact]
    public void CreateFromIdentifier_WithValidIdentifiers_ReturnsCorrectTokenizers()
    {
        // Act & Assert
        AutoTokenizer.CreateFromIdentifier("character").Should().BeOfType<CharacterTokenizer>();
        AutoTokenizer.CreateFromIdentifier("char").Should().BeOfType<CharacterTokenizer>();
        AutoTokenizer.CreateFromIdentifier("word").Should().BeOfType<WordTokenizer>();
        AutoTokenizer.CreateFromIdentifier("CHARACTER").Should().BeOfType<CharacterTokenizer>();
        AutoTokenizer.CreateFromIdentifier("WORD").Should().BeOfType<WordTokenizer>();
    }

    [Fact]
    public void CreateFromIdentifier_WithInvalidIdentifier_ThrowsException()
    {
        // Act
        var act = () => AutoTokenizer.CreateFromIdentifier("unknown");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unknown tokenizer identifier*");
    }

    [Fact]
    public void CreateCharacter_ReturnsCharacterTokenizer()
    {
        // Act
        var tokenizer = AutoTokenizer.CreateCharacter();

        // Assert
        tokenizer.Should().BeOfType<CharacterTokenizer>();
        tokenizer.Should().BeAssignableTo<ITokenizer>();
    }

    [Fact]
    public void CreateWord_ReturnsWordTokenizer()
    {
        // Act
        var tokenizer = AutoTokenizer.CreateWord();

        // Assert
        tokenizer.Should().BeOfType<WordTokenizer>();
        tokenizer.Should().BeAssignableTo<ITokenizer>();
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
            tokenizer.Should().BeAssignableTo<ITokenizer>($"because '{identifier}' should create an ITokenizer");
        }
    }
}
