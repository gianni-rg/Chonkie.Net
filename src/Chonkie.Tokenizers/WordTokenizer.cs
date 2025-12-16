namespace Chonkie.Tokenizers;

/// <summary>
/// Word-level tokenizer that splits text by spaces.
/// </summary>
public class WordTokenizer : BaseTokenizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WordTokenizer"/> class.
    /// </summary>
    public WordTokenizer()
    {
    }

    /// <inheritdoc/>
    public override IReadOnlyList<int> Encode(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Array.Empty<int>();
        }

        var words = text.Split(' ');
        var encoded = new List<int>(words.Length);

        foreach (var word in words)
        {
            var id = AddToVocabulary(word);
            encoded.Add(id);
        }

        return encoded;
    }

    /// <inheritdoc/>
    public override string Decode(IReadOnlyList<int> tokens)
    {
        if (tokens == null || tokens.Count == 0)
        {
            return string.Empty;
        }

        var words = new string[tokens.Count];
        for (int i = 0; i < tokens.Count; i++)
        {
            var tokenId = tokens[i];
            if (tokenId < 0 || tokenId >= Vocab.Count)
            {
                throw new ArgumentException($"Token ID {tokenId} not found in vocabulary.", nameof(tokens));
            }

            words[i] = Vocab[tokenId];
        }

        return string.Join(" ", words);
    }

    /// <inheritdoc/>
    public override int CountTokens(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        return text.Split(' ', StringSplitOptions.None).Length;
    }

    /// <summary>
    /// Counts the number of tokens (words) in the given text span.
    /// C# 14 implicit span conversion allows passing strings directly.
    /// </summary>
    /// <param name="text">The text span to count tokens for.</param>
    /// <returns>The number of words in the text.</returns>
    public int CountTokens(ReadOnlySpan<char> text)
    {
        if (text.IsEmpty)
        {
            return 0;
        }

        int count = 1; // Start with 1 word
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == ' ')
            {
                count++;
            }
        }
        return count;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"WordTokenizer(vocab_size={Vocab.Count})";
    }
}
