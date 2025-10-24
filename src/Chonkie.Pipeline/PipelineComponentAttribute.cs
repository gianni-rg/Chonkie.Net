using Chonkie.Chefs;
using Chonkie.Chunkers;
using Chonkie.Core.Pipeline;
using Chonkie.Fetchers;
using Chonkie.Porters;
using Chonkie.Refineries;

namespace Chonkie.Pipeline;

// Re-export PipelineComponentAttribute from Core for convenience
using PipelineComponentAttribute = Chonkie.Core.Pipeline.PipelineComponentAttribute;

/// <summary>
/// Helper class to register all available pipeline components.
/// </summary>
public static class ComponentRegistrar
{
    private static bool _initialized;
    private static readonly object _lock = new();

    /// <summary>
    /// Registers all available pipeline components.
    /// This method is idempotent and thread-safe.
    /// </summary>
    public static void RegisterAllComponents()
    {
        if (_initialized) return;

        lock (_lock)
        {
            if (_initialized) return;

            RegisterChunkers();
            RegisterChefs();
            RegisterFetchers();
            RegisterRefineries();
            RegisterPorters();

            _initialized = true;
        }
    }

    private static void RegisterChunkers()
    {
        ComponentRegistry.Instance.Register(
            name: nameof(TokenChunker),
            alias: "token",
            componentClass: typeof(TokenChunker),
            componentType: ComponentType.Chunker
        );

        ComponentRegistry.Instance.Register(
            name: nameof(SentenceChunker),
            alias: "sentence",
            componentClass: typeof(SentenceChunker),
            componentType: ComponentType.Chunker
        );

        ComponentRegistry.Instance.Register(
            name: nameof(RecursiveChunker),
            alias: "recursive",
            componentClass: typeof(RecursiveChunker),
            componentType: ComponentType.Chunker
        );

        ComponentRegistry.Instance.Register(
            name: nameof(SemanticChunker),
            alias: "semantic",
            componentClass: typeof(SemanticChunker),
            componentType: ComponentType.Chunker
        );

        ComponentRegistry.Instance.Register(
            name: nameof(LateChunker),
            alias: "late",
            componentClass: typeof(LateChunker),
            componentType: ComponentType.Chunker
        );
    }

    private static void RegisterChefs()
    {
        ComponentRegistry.Instance.Register(
            name: nameof(TextChef),
            alias: "text",
            componentClass: typeof(TextChef),
            componentType: ComponentType.Chef
        );

        ComponentRegistry.Instance.Register(
            name: nameof(MarkdownChef),
            alias: "markdown",
            componentClass: typeof(MarkdownChef),
            componentType: ComponentType.Chef
        );
    }

    private static void RegisterFetchers()
    {
        ComponentRegistry.Instance.Register(
            name: nameof(FileFetcher),
            alias: "file",
            componentClass: typeof(FileFetcher),
            componentType: ComponentType.Fetcher
        );
    }

    private static void RegisterRefineries()
    {
        ComponentRegistry.Instance.Register(
            name: nameof(OverlapRefinery),
            alias: "overlap",
            componentClass: typeof(OverlapRefinery),
            componentType: ComponentType.Refinery
        );

        ComponentRegistry.Instance.Register(
            name: nameof(EmbeddingsRefinery),
            alias: "embeddings",
            componentClass: typeof(EmbeddingsRefinery),
            componentType: ComponentType.Refinery
        );
    }

    private static void RegisterPorters()
    {
        ComponentRegistry.Instance.Register(
            name: nameof(JsonPorter),
            alias: "json",
            componentClass: typeof(JsonPorter),
            componentType: ComponentType.Porter
        );
    }

    /// <summary>
    /// Resets the initialization state (for testing purposes).
    /// </summary>
    internal static void Reset()
    {
        lock (_lock)
        {
            _initialized = false;
            ComponentRegistry.Instance.Clear();
        }
    }
}
