namespace Chonkie.Core.Types;

/// <summary>
/// Represents a document that can be chunked and processed.
/// </summary>
public class Document
{
    /// <summary>
    /// Unique identifier for the document.
    /// </summary>
    public string Id { get; set; } = $"doc_{Guid.NewGuid():N}";

    /// <summary>
    /// The text content of the document.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// List of chunks extracted from the document.
    /// </summary>
    public List<Chunk> Chunks { get; set; } = new();

    /// <summary>
    /// Metadata associated with the document.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Optional source path or identifier.
    /// </summary>
    public string? Source { get; set; }
}
