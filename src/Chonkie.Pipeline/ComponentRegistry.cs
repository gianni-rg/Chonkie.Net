using System.Collections.Concurrent;
using Chonkie.Core.Pipeline;

namespace Chonkie.Pipeline;

/// <summary>
/// Global component registry for pipeline components.
/// This registry manages all available pipeline components and provides
/// lookup functionality for the Pipeline class.
/// </summary>
public sealed class ComponentRegistry
{
    private static readonly Lazy<ComponentRegistry> _instance = new(() => new ComponentRegistry());

    /// <summary>
    /// Gets the singleton instance of the ComponentRegistry.
    /// </summary>
    public static ComponentRegistry Instance => _instance.Value;

    private readonly ConcurrentDictionary<string, Component> _components = new();
    private readonly ConcurrentDictionary<(ComponentType, string), string> _aliases = new();
    private readonly ConcurrentDictionary<ComponentType, List<string>> _componentsByType = new();

    private ComponentRegistry()
    {
        // Initialize component type lists
        foreach (ComponentType type in Enum.GetValues<ComponentType>())
        {
            _componentsByType[type] = new List<string>();
        }
    }

    /// <summary>
    /// Registers a component in the registry.
    /// </summary>
    /// <param name="name">Full name of the component (usually class name)</param>
    /// <param name="alias">Short alias for the component (used in string pipelines)</param>
    /// <param name="componentClass">The actual component class type</param>
    /// <param name="componentType">Type of component (fetcher, chunker, etc.)</param>
    /// <exception cref="ArgumentException">If component name/alias conflicts exist</exception>
    public void Register(string name, string alias, Type componentClass, ComponentType componentType)
    {
        // Check for name conflicts
        if (_components.TryGetValue(name, out var existing))
        {
            if (existing.ComponentClass == componentClass)
            {
                // Same class, same registration - idempotent
                return;
            }
            throw new ArgumentException($"Component name '{name}' already registered with different class");
        }

        // Check for alias conflicts within the same component type
        var aliasKey = (componentType, alias.ToLowerInvariant());
        if (_aliases.TryGetValue(aliasKey, out var existingName) && existingName != name)
        {
            throw new ArgumentException(
                $"Alias '{alias}' already used by {componentType} component '{existingName}'");
        }

        // Create component info
        var component = new Component
        {
            Name = name,
            Alias = alias,
            ComponentClass = componentClass,
            ComponentType = componentType
        };
        component.Validate();

        // Register the component
        _components[name] = component;
        _aliases[aliasKey] = name;

        var typeList = _componentsByType[componentType];
        lock (typeList)
        {
            if (!typeList.Contains(name))
            {
                typeList.Add(name);
            }
        }
    }

    /// <summary>
    /// Gets component info by name or alias.
    /// </summary>
    /// <param name="nameOrAlias">Component name or alias</param>
    /// <param name="componentType">Optional component type to scope alias lookup</param>
    /// <returns>Component for the requested component</returns>
    /// <exception cref="ArgumentException">If component is not found</exception>
    public Component GetComponent(string nameOrAlias, ComponentType? componentType = null)
    {
        var lowerNameOrAlias = nameOrAlias.ToLowerInvariant();

        // If component_type provided, try scoped alias lookup first
        if (componentType.HasValue)
        {
            var aliasKey = (componentType.Value, lowerNameOrAlias);
            if (_aliases.TryGetValue(aliasKey, out var name))
            {
                return _components[name];
            }
        }

        // Try unscoped: check if it's a direct name match
        if (_components.TryGetValue(nameOrAlias, out var component))
        {
            // If type specified, verify it matches
            if (componentType.HasValue && component.ComponentType != componentType.Value)
            {
                throw new ArgumentException(
                    $"Component '{nameOrAlias}' is a {component.ComponentType}, not a {componentType.Value}");
            }
            return component;
        }

        // Try to find by alias across all types (ambiguous lookup)
        var matches = new List<(ComponentType Type, string Name)>();
        foreach (var kvp in _aliases)
        {
            if (kvp.Key.Item2 == lowerNameOrAlias)
            {
                matches.Add((kvp.Key.Item1, kvp.Value));
            }
        }

        if (matches.Count == 1)
        {
            var foundComponent = _components[matches[0].Name];
            // If type specified, verify it matches
            if (componentType.HasValue && foundComponent.ComponentType != componentType.Value)
            {
                throw new ArgumentException(
                    $"Component '{nameOrAlias}' is a {foundComponent.ComponentType}, not a {componentType.Value}");
            }
            return foundComponent;
        }

        if (matches.Count > 1)
        {
            var types = string.Join(", ", matches.Select(m => m.Type));
            throw new ArgumentException(
                $"Ambiguous alias '{nameOrAlias}' found in multiple types: {types}. " +
                "Specify componentType to disambiguate.");
        }

        // Not found
        var available = _aliases.Keys.Take(10).Select(k => $"{k.Item1}:{k.Item2}");
        throw new ArgumentException(
            $"Unknown component: '{nameOrAlias}'. Available: {string.Join(", ", available)}...");
    }

    /// <summary>
    /// Lists all registered components, optionally filtered by type.
    /// </summary>
    /// <param name="componentType">Optional filter by component type</param>
    /// <returns>List of Component objects</returns>
    public IReadOnlyList<Component> ListComponents(ComponentType? componentType = null)
    {
        if (componentType.HasValue)
        {
            var names = _componentsByType[componentType.Value];
            lock (names)
            {
                return names.Select(name => _components[name]).ToList();
            }
        }
        return _components.Values.ToList();
    }

    /// <summary>
    /// Gets all available aliases, optionally filtered by type.
    /// </summary>
    /// <param name="componentType">Optional filter by component type</param>
    /// <returns>List of component aliases</returns>
    public IReadOnlyList<string> GetAliases(ComponentType? componentType = null)
    {
        if (componentType.HasValue)
        {
            return _aliases.Keys
                .Where(k => k.Item1 == componentType.Value)
                .Select(k => k.Item2)
                .ToList();
        }
        return _aliases.Keys.Select(k => k.Item2).ToList();
    }

    /// <summary>
    /// Gets a fetcher component by alias.
    /// </summary>
    public Component GetFetcher(string alias) => GetComponent(alias, ComponentType.Fetcher);

    /// <summary>
    /// Gets a chef component by alias.
    /// </summary>
    public Component GetChef(string alias) => GetComponent(alias, ComponentType.Chef);

    /// <summary>
    /// Gets a chunker component by alias.
    /// </summary>
    public Component GetChunker(string alias) => GetComponent(alias, ComponentType.Chunker);

    /// <summary>
    /// Gets a refinery component by alias.
    /// </summary>
    public Component GetRefinery(string alias) => GetComponent(alias, ComponentType.Refinery);

    /// <summary>
    /// Gets a porter component by alias.
    /// </summary>
    public Component GetPorter(string alias) => GetComponent(alias, ComponentType.Porter);

    /// <summary>
    /// Gets a handshake component by alias.
    /// </summary>
    public Component GetHandshake(string alias) => GetComponent(alias, ComponentType.Handshake);

    /// <summary>
    /// Checks if a component is registered.
    /// </summary>
    public bool IsRegistered(string nameOrAlias)
    {
        var lowerNameOrAlias = nameOrAlias.ToLowerInvariant();

        // Check components
        if (_components.ContainsKey(nameOrAlias))
            return true;

        // Check aliases
        return _aliases.Keys.Any(k => k.Item2 == lowerNameOrAlias);
    }

    /// <summary>
    /// Unregisters a component (mainly for testing).
    /// </summary>
    public void Unregister(string nameOrAlias, ComponentType? componentType = null)
    {
        try
        {
            var component = GetComponent(nameOrAlias, componentType);

            _components.TryRemove(component.Name, out _);
            _aliases.TryRemove((component.ComponentType, component.Alias.ToLowerInvariant()), out _);

            var typeList = _componentsByType[component.ComponentType];
            lock (typeList)
            {
                typeList.Remove(component.Name);
            }
        }
        catch (ArgumentException)
        {
            // Component not registered, nothing to do
        }
    }

    /// <summary>
    /// Clears all registered components (mainly for testing).
    /// </summary>
    public void Clear()
    {
        _components.Clear();
        _aliases.Clear();
        foreach (var list in _componentsByType.Values)
        {
            lock (list)
            {
                list.Clear();
            }
        }
    }
}
