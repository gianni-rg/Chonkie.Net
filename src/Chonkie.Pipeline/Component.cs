using Chonkie.Core.Pipeline;

namespace Chonkie.Pipeline;

/// <summary>
/// Minimal metadata about a pipeline component.
/// This class stores the essential information needed to identify
/// and instantiate components in a Chonkie pipeline.
/// </summary>
public sealed class Component
{
    /// <summary>
    /// Full class name (e.g., "RecursiveChunker")
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Short alias for string-based configs (e.g., "recursive")
    /// </summary>
    public required string Alias { get; init; }

    /// <summary>
    /// The actual type to instantiate
    /// </summary>
    public required Type ComponentClass { get; init; }

    /// <summary>
    /// Which CHOMP stage this component belongs to
    /// </summary>
    public required ComponentType ComponentType { get; init; }

    /// <summary>
    /// Validates component after creation.
    /// </summary>
    /// <exception cref="ArgumentException">If any required property is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Component name cannot be empty", nameof(Name));

        if (string.IsNullOrWhiteSpace(Alias))
            throw new ArgumentException("Component alias cannot be empty", nameof(Alias));

        if (ComponentClass == null)
            throw new ArgumentException("Component class cannot be null", nameof(ComponentClass));
    }
}
