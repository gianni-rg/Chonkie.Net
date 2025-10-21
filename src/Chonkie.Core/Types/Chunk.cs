namespace Chonkie.Core.Types;

/// <summary>
/// Represents a chunk of text with metadata and optional embeddings.
/// </summary>
public record Chunk
{
    /// <summary>
    /// Unique identifier for the chunk.
    /// </summary>
    public string Id { get; init; } = $"chnk_{Guid.NewGuid():N}";

    /// <summary>
    /// The text content of the chunk.
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
    /// Number of tokens in this chunk.
    /// </summary>
    public int TokenCount { get; init; }

    /// <summary>
    /// Optional context metadata for the chunk.
    /// </summary>
    public string? Context { get; init; }

    /// <summary>
    /// Optional embedding vector for the chunk.
    /// </summary>
    public float[]? Embedding { get; init; }

    /// <summary>
    /// Character length of the chunk text.
    /// </summary>
    public int Length => Text.Length;

    /// <summary>
    /// Converts the chunk to a dictionary representation.
    /// </summary>
    /// <returns>A dictionary containing all chunk properties.</returns>
    public Dictionary<string, object?> ToDictionary()
    {
        return new Dictionary<string, object?>
        {
            [nameof(Id)] = Id,
            [nameof(Text)] = Text,
            [nameof(StartIndex)] = StartIndex,
            [nameof(EndIndex)] = EndIndex,
            [nameof(TokenCount)] = TokenCount,
            [nameof(Context)] = Context,
            [nameof(Embedding)] = Embedding
        };
    }

    /// <summary>
    /// Creates a Chunk from a dictionary representation.
    /// </summary>
    /// <param name="data">Dictionary containing chunk data.</param>
    /// <returns>A new Chunk instance.</returns>
    public static Chunk FromDictionary(Dictionary<string, object?> data)
    {
        return new Chunk
        {
            Id = data.TryGetValue(nameof(Id), out var id) ? id?.ToString() ?? $"chnk_{Guid.NewGuid():N}" : $"chnk_{Guid.NewGuid():N}",
            Text = data[nameof(Text)]?.ToString() ?? string.Empty,
            StartIndex = data.TryGetValue(nameof(StartIndex), out var si) ? Convert.ToInt32(si) : 0,
            EndIndex = data.TryGetValue(nameof(EndIndex), out var ei) ? Convert.ToInt32(ei) : 0,
            TokenCount = data.TryGetValue(nameof(TokenCount), out var tc) ? Convert.ToInt32(tc) : 0,
            Context = data.TryGetValue(nameof(Context), out var ctx) ? ctx?.ToString() : null,
            Embedding = data.TryGetValue(nameof(Embedding), out var emb) ? emb as float[] : null
        };
    }

    /// <summary>
    /// Creates a shallow copy of the chunk.
    /// </summary>
    /// <returns>A new Chunk instance with the same values.</returns>
    public Chunk Copy() => this with { };
}
