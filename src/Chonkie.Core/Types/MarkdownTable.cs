namespace Chonkie.Core.Types;

/// <summary>
/// Represents a table found in a markdown document.
/// </summary>
public class MarkdownTable
{
    /// <summary>
    /// The content of the table.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The start index of the table in the document.
    /// </summary>
    public int StartIndex { get; set; }

    /// <summary>
    /// The end index of the table in the document.
    /// </summary>
    public int EndIndex { get; set; }
}
