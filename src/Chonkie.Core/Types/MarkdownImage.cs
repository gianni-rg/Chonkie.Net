namespace Chonkie.Core.Types;

/// <summary>
/// Represents an image found in a markdown document.
/// </summary>
public class MarkdownImage
{
    /// <summary>
    /// The alias/alt text of the image.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// The content/description of the image.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The start index of the image reference in the document.
    /// </summary>
    public int StartIndex { get; set; }

    /// <summary>
    /// The end index of the image reference in the document.
    /// </summary>
    public int EndIndex { get; set; }

    /// <summary>
    /// The link/URL to the image.
    /// </summary>
    public string? Link { get; set; }
}
