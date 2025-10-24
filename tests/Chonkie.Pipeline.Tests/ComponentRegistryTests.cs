using Chonkie.Core.Pipeline;
using Chonkie.Pipeline;

namespace Chonkie.Pipeline.Tests;

/// <summary>
/// Tests for ComponentRegistry functionality.
/// </summary>
public class ComponentRegistryTests
{
    [Fact]
    public void ComponentRegistry_Instance_IsNotNull()
    {
        // Act
        var registry = ComponentRegistry.Instance;

        // Assert
        Assert.NotNull(registry);
    }

    [Fact]
    public void ComponentRegistry_GetChunker_ReturnsValidComponent()
    {
        // Arrange
        ComponentRegistrar.RegisterAllComponents();

        // Act
        var component = ComponentRegistry.Instance.GetChunker("recursive");

        // Assert
        Assert.NotNull(component);
        Assert.Equal("recursive", component.Alias.ToLowerInvariant());
        Assert.Equal(ComponentType.Chunker, component.ComponentType);
    }

    [Fact]
    public void ComponentRegistry_GetChef_ReturnsValidComponent()
    {
        // Arrange
        ComponentRegistrar.RegisterAllComponents();

        // Act
        var component = ComponentRegistry.Instance.GetChef("text");

        // Assert
        Assert.NotNull(component);
        Assert.Equal("text", component.Alias.ToLowerInvariant());
        Assert.Equal(ComponentType.Chef, component.ComponentType);
    }

    [Fact]
    public void ComponentRegistry_GetFetcher_ReturnsValidComponent()
    {
        // Arrange
        ComponentRegistrar.RegisterAllComponents();

        // Act
        var component = ComponentRegistry.Instance.GetFetcher("file");

        // Assert
        Assert.NotNull(component);
        Assert.Equal("file", component.Alias.ToLowerInvariant());
        Assert.Equal(ComponentType.Fetcher, component.ComponentType);
    }

    [Fact]
    public void ComponentRegistry_GetRefinery_ReturnsValidComponent()
    {
        // Arrange
        ComponentRegistrar.RegisterAllComponents();

        // Act
        var component = ComponentRegistry.Instance.GetRefinery("overlap");

        // Assert
        Assert.NotNull(component);
        Assert.Equal("overlap", component.Alias.ToLowerInvariant());
        Assert.Equal(ComponentType.Refinery, component.ComponentType);
    }

    [Fact]
    public void ComponentRegistry_GetPorter_ReturnsValidComponent()
    {
        // Arrange
        ComponentRegistrar.RegisterAllComponents();

        // Act
        var component = ComponentRegistry.Instance.GetPorter("json");

        // Assert
        Assert.NotNull(component);
        Assert.Equal("json", component.Alias.ToLowerInvariant());
        Assert.Equal(ComponentType.Porter, component.ComponentType);
    }

    [Fact]
    public void ComponentRegistry_GetComponent_WithInvalidAlias_ThrowsException()
    {
        // Arrange
        ComponentRegistrar.RegisterAllComponents();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            ComponentRegistry.Instance.GetChunker("nonexistent"));
        Assert.Contains("Unknown component", ex.Message);
    }

    [Fact]
    public void ComponentRegistry_ListComponents_ReturnsAllComponents()
    {
        // Arrange
        ComponentRegistrar.RegisterAllComponents();

        // Act
        var components = ComponentRegistry.Instance.ListComponents();

        // Assert
        Assert.NotNull(components);
        Assert.NotEmpty(components);
    }

    [Fact]
    public void ComponentRegistry_ListComponents_FilteredByType_ReturnsOnlyThatType()
    {
        // Arrange
        ComponentRegistrar.RegisterAllComponents();

        // Act
        var chunkers = ComponentRegistry.Instance.ListComponents(ComponentType.Chunker);

        // Assert
        Assert.NotNull(chunkers);
        Assert.NotEmpty(chunkers);
        Assert.All(chunkers, c => Assert.Equal(ComponentType.Chunker, c.ComponentType));
    }

    [Fact]
    public void ComponentRegistry_GetAliases_ReturnsAllAliases()
    {
        // Arrange
        ComponentRegistrar.RegisterAllComponents();

        // Act
        var aliases = ComponentRegistry.Instance.GetAliases();

        // Assert
        Assert.NotNull(aliases);
        Assert.NotEmpty(aliases);
    }

    [Fact]
    public void ComponentRegistry_GetAliases_FilteredByType_ReturnsOnlyThatType()
    {
        // Arrange
        ComponentRegistrar.RegisterAllComponents();

        // Act
        var chunkerAliases = ComponentRegistry.Instance.GetAliases(ComponentType.Chunker);

        // Assert
        Assert.NotNull(chunkerAliases);
        Assert.NotEmpty(chunkerAliases);
        Assert.Contains("token", chunkerAliases, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("recursive", chunkerAliases, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("sentence", chunkerAliases, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void ComponentRegistry_IsRegistered_ReturnsTrueForRegisteredComponents()
    {
        // Arrange
        ComponentRegistrar.RegisterAllComponents();

        // Act & Assert
        Assert.True(ComponentRegistry.Instance.IsRegistered("recursive"));
        Assert.True(ComponentRegistry.Instance.IsRegistered("token"));
        Assert.True(ComponentRegistry.Instance.IsRegistered("text"));
    }

    [Fact]
    public void ComponentRegistry_IsRegistered_ReturnsFalseForUnregisteredComponents()
    {
        // Arrange
        ComponentRegistrar.RegisterAllComponents();

        // Act & Assert
        Assert.False(ComponentRegistry.Instance.IsRegistered("nonexistent_component"));
    }

    [Fact]
    public void ComponentRegistry_CaseInsensitive_FindsComponents()
    {
        // Arrange
        ComponentRegistrar.RegisterAllComponents();

        // Act
        var component1 = ComponentRegistry.Instance.GetChunker("recursive");
        var component2 = ComponentRegistry.Instance.GetChunker("RECURSIVE");
        var component3 = ComponentRegistry.Instance.GetChunker("Recursive");

        // Assert
        Assert.Equal(component1.Name, component2.Name);
        Assert.Equal(component1.Name, component3.Name);
    }

    [Fact]
    public void ComponentRegistrar_RegisterAllComponents_IsIdempotent()
    {
        // Act
        ComponentRegistrar.RegisterAllComponents();
        ComponentRegistrar.RegisterAllComponents(); // Should not throw or duplicate

        // Assert
        var components = ComponentRegistry.Instance.ListComponents();
        Assert.NotEmpty(components);
    }

    [Fact]
    public void ComponentRegistry_GetComponent_WithWrongType_ThrowsException()
    {
        // Arrange
        ComponentRegistrar.RegisterAllComponents();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            ComponentRegistry.Instance.GetComponent("recursive", ComponentType.Chef));

        Assert.Contains("is a Chunker, not a Chef", ex.Message);
    }
}
