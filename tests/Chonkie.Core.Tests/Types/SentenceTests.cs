using Chonkie.Core.Types;
using FluentAssertions;

namespace Chonkie.Core.Tests.Types;

public class SentenceTests
{
    [Fact]
    public void Constructor_WithRequiredText_CreatesSentence()
    {
        // Arrange & Act
        var sentence = new Sentence
        {
            Text = "This is a sentence."
        };

        // Assert
        sentence.Text.Should().Be("This is a sentence.");
        sentence.StartIndex.Should().Be(0);
        sentence.EndIndex.Should().Be(0);
        sentence.TokenCount.Should().Be(0);
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
        sentence.Text.Should().Be("This is a sentence.");
        sentence.StartIndex.Should().Be(10);
        sentence.EndIndex.Should().Be(29);
        sentence.TokenCount.Should().Be(5);
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
        sentence1.Should().Be(sentence2);
        sentence1.Should().NotBe(sentence3);
    }
}
