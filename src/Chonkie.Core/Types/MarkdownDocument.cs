namespace Chonkie.Core.Types;

/// <summary>
/// Represents a document containing markdown content with extracted structural elements.
/// </summary>
public class MarkdownDocument : Document
{
    /// <summary>
    /// List of tables found in the markdown document.
    /// </summary>
    public List<MarkdownTable> Tables { get; set; } = new();

    /// <summary>
    /// List of code blocks found in the markdown document.
    /// </summary>
    public List<MarkdownCode> Code { get; set; } = new();

    /// <summary>
    /// List of images found in the markdown document.
    /// </summary>
    public List<MarkdownImage> Images { get; set; } = new();
}
