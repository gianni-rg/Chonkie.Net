namespace Chonkie.Core.Interfaces;

/// <summary>
/// Defines the contract for text tokenization.
/// </summary>
public interface ITokenizer
{
    /// <summary>
    /// Encodes text into a sequence of token IDs.
    /// </summary>
    /// <param name="text">The text to encode.</param>
    /// <returns>A read-only list of token IDs.</returns>
    IReadOnlyList<int> Encode(string text);

    /// <summary>
    /// Decodes a sequence of token IDs back into text.
    /// </summary>
    /// <param name="tokens">The token IDs to decode.</param>
    /// <returns>The decoded text string.</returns>
    string Decode(IReadOnlyList<int> tokens);

    /// <summary>
    /// Counts the number of tokens in the given text.
    /// </summary>
    /// <param name="text">The text to count tokens in.</param>
    /// <returns>The number of tokens.</returns>
    int CountTokens(string text);

    /// <summary>
    /// Encodes multiple texts in a batch for efficiency.
    /// </summary>
    /// <param name="texts">The texts to encode.</param>
    /// <returns>A read-only list of encoded token sequences.</returns>
    IReadOnlyList<IReadOnlyList<int>> EncodeBatch(IEnumerable<string> texts);

    /// <summary>
    /// Decodes multiple token sequences in a batch.
    /// </summary>
    /// <param name="tokenSequences">The token sequences to decode.</param>
    /// <returns>A read-only list of decoded text strings.</returns>
    IReadOnlyList<string> DecodeBatch(IEnumerable<IReadOnlyList<int>> tokenSequences);

    /// <summary>
    /// Counts tokens for multiple texts in a batch.
    /// </summary>
    /// <param name="texts">The texts to count tokens in.</param>
    /// <returns>A read-only list of token counts.</returns>
    IReadOnlyList<int> CountTokensBatch(IEnumerable<string> texts);
}
