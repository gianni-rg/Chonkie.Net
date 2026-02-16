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

namespace Chonkie.Core.Pipeline;

/// <summary>
/// Types of pipeline components.
/// These represent the stages in the CHOMP pipeline:
/// Fetcher → Chef → Chunker → Refinery → Porter → Handshake
/// </summary>
public enum ComponentType
{
    /// <summary>
    /// Retrieves raw data from sources
    /// </summary>
    Fetcher,

    /// <summary>
    /// Preprocesses and transforms data
    /// </summary>
    Chef,

    /// <summary>
    /// Splits text into chunks
    /// </summary>
    Chunker,

    /// <summary>
    /// Post-processes chunks (e.g., add embeddings, merge)
    /// </summary>
    Refinery,

    /// <summary>
    /// Exports chunks to storage formats
    /// </summary>
    Porter,

    /// <summary>
    /// Ingests chunks into vector databases
    /// </summary>
    Handshake
}

/// <summary>
/// Attribute to register a class as a pipeline component.
/// Applied to classes to automatically register them in the ComponentRegistry.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PipelineComponentAttribute : Attribute
{
    /// <summary>
    /// Short alias for the component (used in string pipelines)
    /// </summary>
    public string Alias { get; }

    /// <summary>
    /// Type of component (fetcher, chef, chunker, etc.)
    /// </summary>
    public ComponentType ComponentType { get; }

    /// <summary>
    /// Initializes a new instance of the PipelineComponentAttribute.
    /// </summary>
    /// <param name="alias">Short name for the component</param>
    /// <param name="componentType">Type of component</param>
    public PipelineComponentAttribute(string alias, ComponentType componentType)
    {
        Alias = alias ?? throw new ArgumentNullException(nameof(alias));
        ComponentType = componentType;
    }
}
