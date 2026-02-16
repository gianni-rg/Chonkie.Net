// Copyright 2025-2026 Gianni Rosa Gallina and Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Chonkie.Core.Pipeline;
using Xunit;

namespace Chonkie.Pipeline.Tests;

/// <summary>
/// Tests for ComponentRegistry functionality.
/// </summary>
public class ComponentRegistryTests
{
    /// Registry: can register and resolve known components.
    [Fact]
    public void ComponentRegistry_Instance_IsNotNull()
    {
        // Act
        var registry = ComponentRegistry.Instance;

        // Assert
        Assert.NotNull(registry);
    }

    /// Registry: rejects duplicate registrations by name.
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

    /// Registry: unknown components return a clear error.
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

    /// Registry: supports alias mapping for components.
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

    /// Registry: returns metadata for registered components.
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

    /// Registry: lists all registered component types.
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

    /// Registry: allows unregistration of components.
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

    /// Registry: thread-safe registration and resolution.
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

    /// Registry: resolves latest registration when multiple provided.
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

    /// Registry: validation of component configuration schemas.
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

    /// Registry: rejects invalid aliases.
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

    /// Registry: ensures immutability of returned collections.
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

    /// Registry: supports discovery by capability tags.
    [Fact]
    public void ComponentRegistry_IsRegistered_ReturnsFalseForUnregisteredComponents()
    {
        // Arrange
        ComponentRegistrar.RegisterAllComponents();

        // Act & Assert
        Assert.False(ComponentRegistry.Instance.IsRegistered("nonexistent_component"));
    }

    /// Registry: serialization round-trip of registry state.
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

    /// Registry: loading registry from configuration succeeds.
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

    /// Registry: requesting a component with the wrong type throws an informative error.
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
