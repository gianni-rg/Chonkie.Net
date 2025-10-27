namespace Chonkie.Core.Types;

/// <summary>
/// Represents a code block found in a markdown document.
/// </summary>
public class MarkdownCode
{
    /// <summary>
    /// The content of the code block.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The programming language of the code block.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// The start index of the code block in the document.
    /// </summary>
    public int StartIndex { get; set; }

    /// <summary>
    /// The end index of the code block in the document.
    /// </summary>
    public int EndIndex { get; set; }
}
