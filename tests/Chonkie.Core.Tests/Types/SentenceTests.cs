using Chonkie.Core.Types;

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
