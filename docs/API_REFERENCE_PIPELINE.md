# API Reference - Pipeline
**Scope:** Pipeline orchestration and component registry.

## Python Reference
- [chonkie/docs/oss/pipelines.mdx](chonkie/docs/oss/pipelines.mdx)

## Chonkie.Pipeline

### Pipeline
Fluent CHOMP pipeline builder and executor.

Members:
- Constructors: `Pipeline(ILogger? logger = null)`
- Methods: `static Pipeline FromConfig(string config, ILogger? logger = null)`, `static Pipeline FromConfig(IEnumerable<PipelineStepConfig> config, ILogger? logger = null)`, `Pipeline FetchFrom(string sourceType, object? parameters = null)`, `Pipeline ProcessWith(string chefType, object? parameters = null)`, `Pipeline ChunkWith(string chunkerType, object? parameters = null)`, `Pipeline RefineWith(string refineryType, object? parameters = null)`, `Pipeline ExportWith(string porterType, object? parameters = null)`, `Pipeline StoreIn(string handshakeType, object? parameters = null)`, `object Run(object? texts = null)`, `Task<object> RunAsync(object? texts = null, CancellationToken cancellationToken = default)`, `Pipeline Reset()`, `IReadOnlyList<PipelineStepConfig> ToConfig()`, `void SaveConfig(string path)`, `string Describe()`, `override string ToString()`

### PipelineStepConfig
Serializable pipeline step configuration.

Members:
- Properties: `string? Type { get; set; }`, `string? Component { get; set; }`, `object? Parameters { get; set; }`

### ComponentRegistry
Component registry for pipeline components.

Members:
- Properties: `static ComponentRegistry Instance { get; }`
- Methods: `void Register(string name, string alias, Type componentClass, ComponentType componentType)`, `Component GetComponent(string nameOrAlias, ComponentType? componentType = null)`, `IReadOnlyList<Component> ListComponents(ComponentType? componentType = null)`, `IReadOnlyList<string> GetAliases(ComponentType? componentType = null)`, `Component GetFetcher(string alias)`, `Component GetChef(string alias)`, `Component GetChunker(string alias)`, `Component GetRefinery(string alias)`, `Component GetPorter(string alias)`, `Component GetHandshake(string alias)`, `bool IsRegistered(string nameOrAlias)`, `void Unregister(string nameOrAlias, ComponentType? componentType = null)`, `void Clear()`

### Component
Metadata for a pipeline component.

Members:
- Properties: `string Name { get; init; }`, `string Alias { get; init; }`, `Type ComponentClass { get; init; }`, `ComponentType ComponentType { get; init; }`
- Methods: `void Validate()`

### ComponentRegistrar
Registers all built-in components.

Members:
- Methods: `static void RegisterAllComponents()`

### ComponentType
Re-export of core `ComponentType`.

Members:
- Enum values: `Fetcher`, `Chef`, `Chunker`, `Refinery`, `Porter`, `Handshake`
