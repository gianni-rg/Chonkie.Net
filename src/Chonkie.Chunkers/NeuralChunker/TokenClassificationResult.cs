namespace Chonkie.Chunkers.Neural;

/// <summary>
/// Represents a token classification result from the ONNX model.
/// Corresponds to a single token's classification prediction.
/// </summary>
public class TokenClassificationResult
{
    /// <summary>
    /// Gets or sets the token text.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// Gets or sets the token score (confidence).
    /// </summary>
    public float Score { get; set; }

    /// <summary>
    /// Gets or sets the predicted label.
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Gets or sets the label ID (numerical representation).
    /// </summary>
    public int LabelId { get; set; }

    /// <summary>
    /// Gets or sets the start character position in the original text.
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    /// Gets or sets the end character position in the original text.
    /// </summary>
    public int End { get; set; }

    /// <summary>
    /// Gets a value indicating whether this token represents a split point.
    /// A split point is a token with label "B-SPLIT" or similar.
    /// </summary>
    public bool IsSplitPoint => Label.Contains("SPLIT", StringComparison.OrdinalIgnoreCase) && 
                                (Label.StartsWith("B-", StringComparison.OrdinalIgnoreCase) || 
                                 Label == "SPLIT");

    /// <summary>
    /// Returns a string representation of the token classification result.
    /// </summary>
    /// <returns>A string describing the result.</returns>
    public override string ToString() => $"Token(token='{Token}', label={Label}, score={Score:F4}, pos=[{Start}:{End}])";
}

/// <summary>
/// Represents aggregated token classification spans for split point detection.
/// </summary>
public class TokenClassificationSpan
{
    /// <summary>
    /// Gets or sets the span label.
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Gets or sets the start character position.
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    /// Gets or sets the end character position.
    /// </summary>
    public int End { get; set; }

    /// <summary>
    /// Gets or sets the score (confidence).
    /// </summary>
    public float Score { get; set; }

    /// <summary>
    /// Gets or sets the tokens that make up this span.
    /// </summary>
    public List<TokenClassificationResult> Tokens { get; set; } = new();

    /// <summary>
    /// Gets a value indicating whether this span represents a split point.
    /// </summary>
    public bool IsSplitPoint => Label.Contains("SPLIT", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the length of the span in characters.
    /// </summary>
    public int Length => End - Start;

    /// <summary>
    /// Returns a string representation of the span.
    /// </summary>
    /// <returns>A string describing the span.</returns>
    public override string ToString() => $"Span(label={Label}, pos=[{Start}:{End}], len={Length}, score={Score:F4})";
}
