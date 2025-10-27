using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Chonkie.Core.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chonkie.Pipeline;

/// <summary>
/// A fluent API for building and executing chonkie pipelines.
/// The Pipeline class provides a clean, chainable interface for processing
/// documents through the CHOMP pipeline: Fetcher → Chef → Chunker → Refinery → Porter → Handshake
/// </summary>
/// <example>
/// <code>
/// // Simple pipeline with single text
/// var doc = new Pipeline()
///     .ProcessWith("text")
///     .ChunkWith("recursive", new { chunk_size = 512 })
///     .Run(texts: "Your text here");
///
/// // Complex pipeline with file loading
/// var doc = await new Pipeline()
///     .FetchFrom("file", new { path = "document.txt" })
///     .ProcessWith("text")
///     .ChunkWith("recursive", new { chunk_size = 512 })
///     .RefineWith("overlap", new { context_size = 50 })
///     .RunAsync();
/// </code>
/// </example>
[RequiresUnreferencedCode("Pipeline uses reflection and JSON serialization which are not compatible with trimming")]
[RequiresDynamicCode("Pipeline uses reflection and JSON serialization which may require runtime code generation")]
public sealed class Pipeline
{
    private readonly List<PipelineStep> _steps = new();
    private readonly Dictionary<string, object> _componentCache = new();
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new Pipeline.
    /// </summary>
    /// <param name="logger">Optional logger for pipeline execution</param>
    public Pipeline(ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;

        // Ensure components are registered
        ComponentRegistrar.RegisterAllComponents();
    }

    /// <summary>
    /// Creates a pipeline from a configuration list or JSON file path.
    /// </summary>
    /// <param name="config">Either a list of step configs or path to JSON file</param>
    /// <param name="logger">Optional logger</param>
    /// <returns>Configured Pipeline instance</returns>
    /// <exception cref="ArgumentException">If config format is invalid</exception>
    /// <exception cref="FileNotFoundException">If config file doesn't exist</exception>
    public static Pipeline FromConfig(string config, ILogger? logger = null)
    {
        if (!File.Exists(config))
        {
            throw new FileNotFoundException($"Config file not found: {config}");
        }

        var json = File.ReadAllText(config);
        var steps = JsonSerializer.Deserialize<List<PipelineStepConfig>>(json);

        if (steps == null || steps.Count == 0)
        {
            throw new ArgumentException("Config must contain at least one step");
        }

        return FromConfig(steps, logger);
    }

    /// <summary>
    /// Creates a pipeline from a configuration list.
    /// </summary>
    public static Pipeline FromConfig(IEnumerable<PipelineStepConfig> config, ILogger? logger = null)
    {
        var pipeline = new Pipeline(logger);

        int stepIndex = 0;
        foreach (var step in config)
        {
            try
            {
                var stepType = step.Type?.ToLowerInvariant() ??
                    throw new ArgumentException($"Step {stepIndex + 1}: 'type' is required");
                var component = step.Component ??
                    throw new ArgumentException($"Step {stepIndex + 1}: 'component' is required");

                switch (stepType)
                {
                    case "fetch":
                        pipeline.FetchFrom(component, step.Parameters);
                        break;
                    case "process":
                        pipeline.ProcessWith(component, step.Parameters);
                        break;
                    case "chunk":
                        pipeline.ChunkWith(component, step.Parameters);
                        break;
                    case "refine":
                        pipeline.RefineWith(component, step.Parameters);
                        break;
                    case "export":
                        pipeline.ExportWith(component, step.Parameters);
                        break;
                    case "write":
                    case "store":
                        pipeline.StoreIn(component, step.Parameters);
                        break;
                    default:
                        throw new ArgumentException($"Unknown step type: '{stepType}'");
                }

                stepIndex++;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error processing step {stepIndex + 1}: {ex.Message}", ex);
            }
        }

        return pipeline;
    }

    /// <summary>
    /// Fetches data from a source.
    /// </summary>
    /// <param name="sourceType">Type of source fetcher to use (e.g., "file")</param>
    /// <param name="parameters">Parameters for the fetcher</param>
    /// <returns>Pipeline instance for method chaining</returns>
    public Pipeline FetchFrom(string sourceType, object? parameters = null)
    {
        var component = ComponentRegistry.Instance.GetFetcher(sourceType);
        _steps.Add(new PipelineStep
        {
            Type = "fetch",
            Component = component,
            Parameters = parameters ?? new { }
        });
        return this;
    }

    /// <summary>
    /// Processes data with a chef component.
    /// </summary>
    /// <param name="chefType">Type of chef to use (e.g., "text")</param>
    /// <param name="parameters">Parameters for the chef</param>
    /// <returns>Pipeline instance for method chaining</returns>
    public Pipeline ProcessWith(string chefType, object? parameters = null)
    {
        var component = ComponentRegistry.Instance.GetChef(chefType);
        _steps.Add(new PipelineStep
        {
            Type = "process",
            Component = component,
            Parameters = parameters ?? new { }
        });
        return this;
    }

    /// <summary>
    /// Chunks data with a chunker component.
    /// </summary>
    /// <param name="chunkerType">Type of chunker to use (e.g., "recursive", "semantic")</param>
    /// <param name="parameters">Parameters for the chunker</param>
    /// <returns>Pipeline instance for method chaining</returns>
    public Pipeline ChunkWith(string chunkerType, object? parameters = null)
    {
        var component = ComponentRegistry.Instance.GetChunker(chunkerType);
        _steps.Add(new PipelineStep
        {
            Type = "chunk",
            Component = component,
            Parameters = parameters ?? new { }
        });
        return this;
    }

    /// <summary>
    /// Refines chunks with a refinery component.
    /// </summary>
    /// <param name="refineryType">Type of refinery to use (e.g., "overlap", "embeddings")</param>
    /// <param name="parameters">Parameters for the refinery</param>
    /// <returns>Pipeline instance for method chaining</returns>
    public Pipeline RefineWith(string refineryType, object? parameters = null)
    {
        var component = ComponentRegistry.Instance.GetRefinery(refineryType);
        _steps.Add(new PipelineStep
        {
            Type = "refine",
            Component = component,
            Parameters = parameters ?? new { }
        });
        return this;
    }

    /// <summary>
    /// Exports chunks with a porter component.
    /// </summary>
    /// <param name="porterType">Type of porter to use (e.g., "json")</param>
    /// <param name="parameters">Parameters for the porter</param>
    /// <returns>Pipeline instance for method chaining</returns>
    public Pipeline ExportWith(string porterType, object? parameters = null)
    {
        var component = ComponentRegistry.Instance.GetPorter(porterType);
        _steps.Add(new PipelineStep
        {
            Type = "export",
            Component = component,
            Parameters = parameters ?? new { }
        });
        return this;
    }

    /// <summary>
    /// Stores chunks in a vector database with a handshake component.
    /// </summary>
    /// <param name="handshakeType">Type of handshake to use (e.g., "chroma", "qdrant")</param>
    /// <param name="parameters">Parameters for the handshake</param>
    /// <returns>Pipeline instance for method chaining</returns>
    public Pipeline StoreIn(string handshakeType, object? parameters = null)
    {
        var component = ComponentRegistry.Instance.GetHandshake(handshakeType);
        _steps.Add(new PipelineStep
        {
            Type = "write",
            Component = component,
            Parameters = parameters ?? new { }
        });
        return this;
    }

    /// <summary>
    /// Runs the pipeline synchronously and returns the final result.
    /// </summary>
    /// <param name="texts">Optional text input (single string or array)</param>
    /// <returns>Document or array of Documents with processed chunks</returns>
    public object Run(object? texts = null)
    {
        return RunAsync(texts).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Runs the pipeline asynchronously and returns the final result.
    /// The pipeline automatically reorders steps according to the CHOMP flow:
    /// Fetcher → Chef → Chunker → Refinery(ies) → Porter/Handshake
    /// </summary>
    /// <param name="texts">Optional text input (single string or array)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document or array of Documents with processed chunks</returns>
    public async Task<object> RunAsync(object? texts = null, CancellationToken cancellationToken = default)
    {
        if (_steps.Count == 0)
        {
            throw new InvalidOperationException("Pipeline has no steps to execute");
        }

        // Reorder steps according to CHOMP flow
        var orderedSteps = ReorderSteps();

        // Validate pipeline
        ValidatePipeline(orderedSteps, texts != null);

        // If we have direct text input but no chef, convert text to Document(s) before processing
        object? data = texts;
        var hasChef = orderedSteps.Any(s => s.Type == "process");
        if (texts != null && !hasChef)
        {
            if (texts is string singleText)
            {
                data = new Document { Content = singleText };
            }
            else if (texts is IEnumerable<string> multipleTexts)
            {
                data = multipleTexts.Select(t => new Document { Content = t }).ToList();
            }
        }

        // Execute pipeline steps
        for (int i = 0; i < orderedSteps.Count; i++)
        {
            var step = orderedSteps[i];

            // Skip fetcher if we have direct text input
            if (texts != null && step.Type == "fetch")
            {
                continue;
            }

            try
            {
                _logger.LogDebug("Executing step {StepNumber}/{TotalSteps}: {StepType}({Component})",
                    i + 1, orderedSteps.Count, step.Type, step.Component.Alias);

                data = await ExecuteStepAsync(step, data, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pipeline failed at step {StepNumber} ({StepType})",
                    i + 1, step.Type);
                throw new InvalidOperationException(
                    $"Pipeline failed at step {i + 1} ({step.Type}): {ex.Message}", ex);
            }
        }

        // If the result is a single-item list, unwrap it to return the single item
        // This matches Python behavior where single text input returns Document, not List[Document]
        if (data is System.Collections.IList list && list.Count == 1)
        {
            return list[0] ?? throw new InvalidOperationException("Pipeline produced null result");
        }

        return data ?? throw new InvalidOperationException("Pipeline produced null result");
    }

    /// <summary>
    /// Resets the pipeline to its initial state.
    /// </summary>
    public Pipeline Reset()
    {
        _steps.Clear();
        _componentCache.Clear();
        return this;
    }

    /// <summary>
    /// Exports pipeline configuration to list format.
    /// </summary>
    public IReadOnlyList<PipelineStepConfig> ToConfig()
    {
        return _steps.Select(s => new PipelineStepConfig
        {
            Type = s.Type,
            Component = s.Component.Alias,
            Parameters = s.Parameters
        }).ToList();
    }

    /// <summary>
    /// Saves pipeline configuration to a JSON file.
    /// </summary>
    public void SaveConfig(string path)
    {
        var config = ToConfig();
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Gets a human-readable description of the pipeline.
    /// Shows steps in CHOMP execution order (not definition order).
    /// </summary>
    public string Describe()
    {
        if (_steps.Count == 0)
        {
            return "Empty pipeline";
        }

        var orderedSteps = ReorderSteps();
        var descriptions = orderedSteps.Select(s => $"{s.Type}({s.Component.Alias})");
        return string.Join(" → ", descriptions);
    }

    /// <summary>
    /// Returns string representation of the pipeline.
    /// </summary>
    public override string ToString() => $"Pipeline({Describe()})";

    // Private helper methods continue in next part...
    private List<PipelineStep> ReorderSteps()
    {
        // Group steps by type
        var stepsByType = _steps.GroupBy(s => s.Type)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Build ordered list following CHOMP: Fetch → Process → Chunk → Refine → Export → Write
        var ordered = new List<PipelineStep>();
        var stepOrder = new[] { "fetch", "process", "chunk", "refine", "export", "write" };

        foreach (var stepType in stepOrder)
        {
            if (stepsByType.TryGetValue(stepType, out var steps))
            {
                if (stepType == "process")
                {
                    // Only one chef allowed - use the last one if multiple
                    ordered.Add(steps[^1]);
                }
                else
                {
                    // Multiple allowed - preserve order
                    ordered.AddRange(steps);
                }
            }
        }

        return ordered;
    }

    private void ValidatePipeline(List<PipelineStep> orderedSteps, bool hasTextInput)
    {
        var stepTypes = orderedSteps.Select(s => s.Type).ToHashSet();

        // Must have a chunker
        if (!stepTypes.Contains("chunk"))
        {
            throw new InvalidOperationException(
                "Pipeline must include a chunker component (use ChunkWith())");
        }

        // Must have fetcher OR text input
        if (!hasTextInput && !stepTypes.Contains("fetch"))
        {
            throw new InvalidOperationException(
                "Pipeline must include a fetcher component (use FetchFrom()) " +
                "or provide text input to Run(texts: ...)");
        }

        // Only one chef allowed
        var userProcessCount = _steps.Count(s => s.Type == "process");
        if (userProcessCount > 1)
        {
            throw new InvalidOperationException(
                $"Multiple process steps found ({userProcessCount}). Only one chef is allowed per pipeline.");
        }
    }

    private async Task<object> ExecuteStepAsync(
        PipelineStep step,
        object? inputData,
        CancellationToken cancellationToken)
    {
        // Get or create component instance
        var componentKey = GetComponentCacheKey(step);
        if (!_componentCache.TryGetValue(componentKey, out var componentInstance))
        {
            componentInstance = CreateComponentInstance(step);
            _componentCache[componentKey] = componentInstance;
        }

        // Execute the component based on step type
        return step.Type switch
        {
            "fetch" => await ExecuteFetchAsync(componentInstance, step, cancellationToken),
            "process" => await ExecuteProcessAsync(componentInstance, inputData, cancellationToken),
            "chunk" => await ExecuteChunkAsync(componentInstance, inputData, cancellationToken),
            "refine" => await ExecuteRefineAsync(componentInstance, inputData, cancellationToken),
            "export" => await ExecuteExportAsync(componentInstance, inputData, step, cancellationToken),
            "write" => await ExecuteWriteAsync(componentInstance, inputData, step, cancellationToken),
            _ => throw new InvalidOperationException($"Unknown step type: {step.Type}")
        };
    }

    private string GetComponentCacheKey(PipelineStep step)
    {
        var parametersJson = JsonSerializer.Serialize(step.Parameters);
        return $"{step.Component.Name}:{parametersJson}";
    }

    private object CreateComponentInstance(PipelineStep step)
    {
        // Convert parameters object to dictionary for easier handling
        var parameters = ConvertParametersToDictionary(step.Parameters);

        try
        {
            object instance;

            // Special handling for different component types
            if (step.Type == "chunk")
            {
                // Chunkers need a tokenizer - create with default tokenizer if not provided
                var tokenizer = new Chonkie.Tokenizers.WordTokenizer();
                var chunkSize = parameters.TryGetValue("chunk_size", out var cs)
                    ? ConvertToInt32(cs)
                    : 512;

                // Create chunker with constructor parameters
                instance = step.Component.Name switch
                {
                    "TokenChunker" => new Chonkie.Chunkers.TokenChunker(tokenizer, chunkSize),
                    "SentenceChunker" => new Chonkie.Chunkers.SentenceChunker(tokenizer, chunkSize),
                    "RecursiveChunker" => new Chonkie.Chunkers.RecursiveChunker(tokenizer, chunkSize),
                    "CodeChunker" => new Chonkie.Chunkers.CodeChunker(tokenizer, chunkSize),
                    "TableChunker" => CreateTableChunker(tokenizer, chunkSize, parameters),
                    "SemanticChunker" => CreateSemanticChunker(tokenizer, chunkSize, parameters),
                    "LateChunker" => CreateLateChunker(tokenizer, chunkSize, parameters),
                    _ => throw new InvalidOperationException($"Unknown chunker: {step.Component.Name}")
                };
            }
            else if (step.Type == "process")
            {
                // Chefs typically have parameterless constructors
                instance = step.Component.Name switch
                {
                    "TextChef" => new Chonkie.Chefs.TextChef(),
                    "MarkdownChef" => new Chonkie.Chefs.MarkdownChef(),
                    "CodeChef" => new Chonkie.Chefs.CodeChef(),
                    _ => Activator.CreateInstance(step.Component.ComponentClass)!
                };
            }
            else if (step.Type == "refine")
            {
                // Refineries
                instance = step.Component.Name switch
                {
                    "OverlapRefinery" => new Chonkie.Refineries.OverlapRefinery(),
                    "EmbeddingsRefinery" => CreateEmbeddingsRefinery(parameters),
                    _ => Activator.CreateInstance(step.Component.ComponentClass)!
                };
            }
            else if (step.Type == "fetch")
            {
                // Fetchers typically have parameterless constructors
                instance = Activator.CreateInstance(step.Component.ComponentClass)!;
            }
            else if (step.Type == "export")
            {
                // Porters typically have parameterless constructors
                instance = Activator.CreateInstance(step.Component.ComponentClass)!;
            }
            else
            {
                instance = Activator.CreateInstance(step.Component.ComponentClass)!;
            }

            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Failed to create instance of {step.Component.Name}");
            }

            return instance;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to create component {step.Component.Name}: {ex.Message}", ex);
        }
    }

    private object CreateSemanticChunker(Chonkie.Core.Interfaces.ITokenizer tokenizer, int chunkSize, Dictionary<string, object?> parameters)
    {
        // SemanticChunker needs an embedding model
        var modelName = parameters.TryGetValue("embedding_model", out var m) && m is not null
            ? NormalizeModelName(m.ToString()!)
            : "all-MiniLM-L6-v2";
        var modelPath = FindModelPath(modelName);
        var embeddings = new Chonkie.Embeddings.SentenceTransformers.SentenceTransformerEmbeddings(modelPath);
        return new Chonkie.Chunkers.SemanticChunker(tokenizer, embeddings, null, 0.8f, chunkSize);
    }

    private object CreateLateChunker(Chonkie.Core.Interfaces.ITokenizer tokenizer, int chunkSize, Dictionary<string, object?> parameters)
    {
        // LateChunker needs an embedding model
        var modelName = parameters.TryGetValue("embedding_model", out var m) && m is not null
            ? NormalizeModelName(m.ToString()!)
            : "all-MiniLM-L6-v2";
        var modelPath = FindModelPath(modelName);
        var embeddings = new Chonkie.Embeddings.SentenceTransformers.SentenceTransformerEmbeddings(modelPath);
        return new Chonkie.Chunkers.LateChunker(tokenizer, embeddings, chunkSize);
    }

    private object CreateTableChunker(Chonkie.Core.Interfaces.ITokenizer tokenizer, int chunkSize, Dictionary<string, object?> parameters)
    {
        // TableChunker supports optional repeat_headers parameter
        var repeatHeaders = false;
        if (parameters.TryGetValue("repeat_headers", out var rh) && rh is not null)
        {
            if (rh is bool boolValue)
            {
                repeatHeaders = boolValue;
            }
            else if (rh is System.Text.Json.JsonElement jsonElement)
            {
                repeatHeaders = jsonElement.ValueKind == System.Text.Json.JsonValueKind.True;
            }
            else
            {
                repeatHeaders = Convert.ToBoolean(rh);
            }
        }
        return new Chonkie.Chunkers.TableChunker(tokenizer, chunkSize, repeatHeaders);
    }

    private static string NormalizeModelName(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
            return "all-MiniLM-L6-v2";

        var name = modelName.Trim();

        // If given as a HuggingFace-style path (e.g., "sentence-transformers/all-MiniLM-L6-v2"),
        // prefer the final segment which should match our local folder name.
        if (name.Contains('/'))
        {
            var parts = name.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            name = parts.Length > 0 ? parts[^1] : name;
        }

        return name;
    }

    private static string FindModelPath(string modelName)
    {
        // Try common model directory locations relative to base directory and current directory
        var basePath = AppContext.BaseDirectory;
        var possiblePaths = new List<string>();

        // Add paths relative to AppContext.BaseDirectory (where the assembly is loaded from)
        for (int levels = 0; levels <= 6; levels++)
        {
            var relativePath = string.Join(Path.DirectorySeparatorChar, Enumerable.Repeat("..", levels));
            if (string.IsNullOrEmpty(relativePath))
            {
                possiblePaths.Add(Path.Combine(basePath, "models", modelName));
            }
            else
            {
                possiblePaths.Add(Path.Combine(basePath, relativePath, "models", modelName));
            }
        }

        // Also try relative to current directory
        for (int levels = 0; levels <= 3; levels++)
        {
            var relativePath = string.Join(Path.DirectorySeparatorChar, Enumerable.Repeat("..", levels));
            if (string.IsNullOrEmpty(relativePath))
            {
                possiblePaths.Add(Path.Combine("models", modelName));
            }
            else
            {
                possiblePaths.Add(Path.Combine(relativePath, "models", modelName));
            }
        }

        foreach (var path in possiblePaths)
        {
            try
            {
                var fullPath = Path.GetFullPath(path);
                // Check if the model directory exists and contains model.onnx
                if (Directory.Exists(fullPath) && File.Exists(Path.Combine(fullPath, "model.onnx")))
                {
                    return fullPath;
                }
            }
            catch
            {
                // Ignore invalid paths
            }
        }

        // Fall back to models/modelName and let it fail with a clear error
        return Path.Combine("models", modelName);
    }

    private static int ConvertToInt32(object? value)
    {
        if (value is null)
            return 0;

        // Handle JsonElement from JSON deserialization
        if (value is System.Text.Json.JsonElement jsonElement)
        {
            return jsonElement.GetInt32();
        }

        return Convert.ToInt32(value);
    }

    private object CreateEmbeddingsRefinery(Dictionary<string, object?> parameters)
    {
        // EmbeddingsRefinery needs an embedding model
        var modelName = parameters.TryGetValue("embedding_model", out var m) && m is not null
            ? NormalizeModelName(m.ToString()!)
            : "all-MiniLM-L6-v2";
        var modelPath = FindModelPath(modelName);
        var embeddings = new Chonkie.Embeddings.SentenceTransformers.SentenceTransformerEmbeddings(modelPath);
        return new Chonkie.Refineries.EmbeddingsRefinery(embeddings);
    }

    private Dictionary<string, object?> ConvertParametersToDictionary(object parameters)
    {
        if (parameters is Dictionary<string, object?> dict)
            return dict;

        // Convert anonymous object or other objects to dictionary
        var json = JsonSerializer.Serialize(parameters);
        return JsonSerializer.Deserialize<Dictionary<string, object?>>(json)
               ?? new Dictionary<string, object?>();
    }

    private void SetInstanceProperties(object instance, Dictionary<string, object?> parameters)
    {
        var type = instance.GetType();
        foreach (var param in parameters)
        {
            var property = type.GetProperty(param.Key,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.IgnoreCase);

            if (property != null && property.CanWrite && param.Value != null)
            {
                try
                {
                    var value = Convert.ChangeType(param.Value, property.PropertyType);
                    property.SetValue(instance, value);
                }
                catch
                {
                    // Skip properties that can't be set
                }
            }
        }
    }

    // Execution methods for each step type
    private async Task<object> ExecuteFetchAsync(object component, PipelineStep step, CancellationToken ct)
    {
        // Call FetchAsync method via reflection
        var method = component.GetType().GetMethod("FetchAsync");
        if (method == null)
            throw new InvalidOperationException($"Fetcher {component.GetType().Name} missing FetchAsync method");

        // Get parameters from step configuration
        var parameters = ConvertParametersToDictionary(step.Parameters);
        // Support both "path" and "dir" parameter names
        var path = parameters.TryGetValue("path", out var p) ? p?.ToString() ?? "" :
                   parameters.TryGetValue("dir", out var d) ? d?.ToString() ?? "" : "";
        var filter = parameters.TryGetValue("filter", out var f) ? f?.ToString() : null;

        // Invoke FetchAsync(path, filter, cancellationToken)
        var task = method.Invoke(component, new object?[] { path, filter, ct }) as Task;
        if (task == null)
            throw new InvalidOperationException("FetchAsync did not return a Task");

        await task.ConfigureAwait(false);

        // Get the Result property from Task<T>
        var resultProperty = task.GetType().GetProperty("Result");
        var result = resultProperty?.GetValue(task);

        return result ?? throw new InvalidOperationException("Fetch returned null");
    }

    private async Task<object> ExecuteProcessAsync(object component, object? inputData, CancellationToken ct)
    {
        // Chef processing - call ProcessAsync or Process or Parse method
        if (inputData == null)
            throw new InvalidOperationException("Process step requires input data");

        var asyncMethod = component.GetType().GetMethod("ProcessAsync");
        var syncMethod = component.GetType().GetMethod("Process") ?? component.GetType().GetMethod("Parse");

        if (asyncMethod == null && syncMethod == null)
            throw new InvalidOperationException($"Chef {component.GetType().Name} missing ProcessAsync/Process/Parse method");

        object? result;

        // Handle fetcher output: List<(string Path, string Content)>
        if (inputData is System.Collections.IList list && list.Count > 0)
        {
            var firstItem = list[0];
            if (firstItem is ValueTuple<string, string>)
            {
                // Convert List<(string, string)> from fetcher to List<string>
                var fetchedTexts = new List<string>();
                foreach (var item in list)
                {
                    if (item is ValueTuple<string, string> tuple)
                    {
                        fetchedTexts.Add(tuple.Item2); // Extract content
                    }
                }
                inputData = fetchedTexts;
            }
        }

        if (inputData is IEnumerable<string> texts)
        {
            // Process multiple texts
            var documents = new List<Document>();
            foreach (var text in texts)
            {
                string processed;
                if (asyncMethod != null)
                {
                    var task = (Task<string>)asyncMethod.Invoke(component, new object[] { text, ct })!;
                    processed = await task;
                }
                else
                {
                    processed = (string)syncMethod!.Invoke(component, new object[] { text })!;
                }
                documents.Add(new Document { Content = processed });
            }
            result = documents;
        }
        else if (inputData is string text)
        {
            string processed;
            if (asyncMethod != null)
            {
                var task = (Task<string>)asyncMethod.Invoke(component, new object[] { text, ct })!;
                processed = await task;
            }
            else
            {
                processed = (string)syncMethod!.Invoke(component, new object[] { text })!;
            }
            result = new Document { Content = processed };
        }
        else
        {
            throw new InvalidOperationException($"Unexpected input type for chef: {inputData.GetType()}");
        }

        return result ?? throw new InvalidOperationException("Process returned null");
    }

    private Task<object> ExecuteChunkAsync(object component, object? inputData, CancellationToken ct)
    {
        if (inputData == null)
            throw new InvalidOperationException("Chunk step requires input data");

        var method = component.GetType().GetMethod("ChunkDocument")
                     ?? component.GetType().GetMethod("Chunk");
        if (method == null)
            throw new InvalidOperationException($"Chunker {component.GetType().Name} missing Chunk method");

        object? result;
        if (inputData is List<Document> docs)
        {
            result = docs.Select(d => (Document)method.Invoke(component, new object[] { d })!).ToList();
        }
        else if (inputData is Document doc)
        {
            result = method.Invoke(component, new object[] { doc });
        }
        else if (inputData is string text)
        {
            // Create document from text
            var document = new Document { Content = text };
            result = method.Invoke(component, new object[] { document });
        }
        else
        {
            throw new InvalidOperationException($"Unexpected input type for chunker: {inputData.GetType()}");
        }

        return Task.FromResult(result ?? throw new InvalidOperationException("Chunk returned null"));
    }

    private async Task<object> ExecuteRefineAsync(object component, object? inputData, CancellationToken ct)
    {
        if (inputData == null)
            throw new InvalidOperationException("Refine step requires input data");

        var asyncMethod = component.GetType().GetMethod("RefineAsync");
        var syncMethod = component.GetType().GetMethod("RefineDocument")
                     ?? component.GetType().GetMethod("Refine");
        if (asyncMethod == null && syncMethod == null)
            throw new InvalidOperationException($"Refinery {component.GetType().Name} missing RefineAsync/Refine method");

        object? result;
        if (inputData is List<Document> docs)
        {
            var refined = new List<Document>();
            foreach (var doc in docs)
            {
                if (asyncMethod != null)
                {
                    // RefineAsync works on chunks, not documents
                    var task = (Task<IReadOnlyList<Chunk>>)asyncMethod.Invoke(component, new object[] { doc.Chunks, ct })!;
                    var refinedChunks = await task;
                    refined.Add(new Document { Content = doc.Content, Chunks = refinedChunks.ToList() });
                }
                else
                {
                    refined.Add((Document)syncMethod!.Invoke(component, new object[] { doc })!);
                }
            }
            result = refined;
        }
        else if (inputData is Document doc)
        {
            if (asyncMethod != null)
            {
                // RefineAsync works on chunks, not documents
                var task = (Task<IReadOnlyList<Chunk>>)asyncMethod.Invoke(component, new object[] { doc.Chunks, ct })!;
                var refinedChunks = await task;
                result = new Document { Content = doc.Content, Chunks = refinedChunks.ToList() };
            }
            else
            {
                result = syncMethod!.Invoke(component, new object[] { doc });
            }
        }
        else
        {
            throw new InvalidOperationException($"Unexpected input type for refinery: {inputData.GetType()}");
        }

        return result ?? throw new InvalidOperationException("Refine returned null");
    }

    private Task<object> ExecuteExportAsync(object component, object? inputData, PipelineStep step, CancellationToken ct)
    {
        if (inputData == null)
            throw new InvalidOperationException("Export step requires input data");

        // Extract chunks from documents
        List<Chunk> chunks;
        if (inputData is List<Document> docs)
        {
            chunks = docs.SelectMany(d => d.Chunks).ToList();
        }
        else if (inputData is Document doc)
        {
            chunks = doc.Chunks.ToList();
        }
        else
        {
            throw new InvalidOperationException($"Unexpected input type for porter: {inputData.GetType()}");
        }

        var method = component.GetType().GetMethod("Export");
        if (method == null)
            throw new InvalidOperationException($"Porter {component.GetType().Name} missing Export method");

        // Get parameters for export method
        var parameters = ConvertParametersToDictionary(step.Parameters);
        method.Invoke(component, new object[] { chunks, parameters });

        // Return input data for chaining
        return Task.FromResult(inputData);
    }

    private Task<object> ExecuteWriteAsync(object component, object? inputData, PipelineStep step, CancellationToken ct)
    {
        if (inputData == null)
            throw new InvalidOperationException("Write step requires input data");

        // Extract chunks from documents
        List<Chunk> chunks;
        if (inputData is List<Document> docs)
        {
            chunks = docs.SelectMany(d => d.Chunks).ToList();
        }
        else if (inputData is Document doc)
        {
            chunks = doc.Chunks.ToList();
        }
        else
        {
            throw new InvalidOperationException($"Unexpected input type for handshake: {inputData.GetType()}");
        }

        var method = component.GetType().GetMethod("Write");
        if (method == null)
            throw new InvalidOperationException($"Handshake {component.GetType().Name} missing Write method");

        // Get parameters for write method
        var parameters = ConvertParametersToDictionary(step.Parameters);
        var result = method.Invoke(component, new object[] { chunks, parameters });

        return Task.FromResult(result ?? inputData);
    }
}

/// <summary>
/// Internal class representing a pipeline step.
/// </summary>
internal sealed class PipelineStep
{
    public required string Type { get; init; }
    public required Component Component { get; init; }
    public required object Parameters { get; init; }
}

/// <summary>
/// Configuration for a pipeline step (for serialization).
/// </summary>
public sealed class PipelineStepConfig
{
    /// <summary>
    /// Type of step (fetch, process, chunk, refine, export, write)
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Component alias (e.g., "recursive", "semantic")
    /// </summary>
    public string? Component { get; set; }

    /// <summary>
    /// Parameters for the component
    /// </summary>
    public object? Parameters { get; set; }
}
