using Chonkie.Core.Interfaces;

namespace Chonkie.Tokenizers;

/// <summary>
/// Factory class for creating tokenizer instances.
/// </summary>
public static class AutoTokenizer
{
    /// <summary>
    /// Creates a tokenizer from a string identifier or returns the provided tokenizer.
    /// </summary>
    /// <param name="tokenizerOrIdentifier">Either a tokenizer instance or a string identifier ("character", "word").</param>
    /// <returns>An ITokenizer instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the identifier is not recognized.</exception>
    public static ITokenizer Create(object tokenizerOrIdentifier)
    {
        return tokenizerOrIdentifier switch
        {
            ITokenizer tokenizer => tokenizer,
            string identifier => CreateFromIdentifier(identifier),
            _ => throw new ArgumentException(
                $"Unsupported tokenizer type: {tokenizerOrIdentifier.GetType().Name}. " +
                "Expected ITokenizer instance or string identifier.",
                nameof(tokenizerOrIdentifier))
        };
    }

    /// <summary>
    /// Creates a tokenizer from a string identifier.
    /// </summary>
    /// <param name="identifier">The tokenizer identifier ("character", "word").</param>
    /// <returns>An ITokenizer instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the identifier is not recognized.</exception>
    public static ITokenizer CreateFromIdentifier(string identifier)
    {
        return identifier.ToLowerInvariant() switch
        {
            "character" or "char" => new CharacterTokenizer(),
            "word" => new WordTokenizer(),
            _ => throw new ArgumentException(
                $"Unknown tokenizer identifier: '{identifier}'. " +
                "Supported identifiers: 'character', 'char', 'word'.",
                nameof(identifier))
        };
    }

    /// <summary>
    /// Creates a character tokenizer.
    /// </summary>
    /// <returns>A CharacterTokenizer instance.</returns>
    public static CharacterTokenizer CreateCharacter() => new();

    /// <summary>
    /// Creates a word tokenizer.
    /// </summary>
    /// <returns>A WordTokenizer instance.</returns>
    public static WordTokenizer CreateWord() => new();
}
