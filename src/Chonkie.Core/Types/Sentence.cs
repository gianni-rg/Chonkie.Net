namespace Chonkie.Core.Types;

/// <summary>
/// Represents a sentence with position and token information.
/// </summary>
public record Sentence
{
    /// <summary>
    /// The text content of the sentence.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Starting character index in the original text.
    /// </summary>
    public int StartIndex { get; init; }

    /// <summary>
    /// Ending character index in the original text.
    /// </summary>
    public int EndIndex { get; init; }

    /// <summary>
    /// Number of tokens in this sentence.
    /// </summary>
    public int TokenCount { get; init; }
}
