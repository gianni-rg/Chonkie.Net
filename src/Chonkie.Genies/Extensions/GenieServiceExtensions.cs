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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chonkie.Genies.Extensions;

/// <summary>
/// Extension methods for registering Genie services with dependency injection.
/// </summary>
public static class GenieServiceExtensions
{
    /// <summary>
    /// Adds GroqGenie to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiKey">The Groq API key.</param>
    /// <param name="model">The model name to use. Defaults to llama-3.3-70b-versatile.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGroqGenie(
        this IServiceCollection services,
        string apiKey,
        string? model = null)
    {
        services.AddSingleton<IGeneration>(sp =>
        {
            var logger = sp.GetService<ILogger<GroqGenie>>();
            return new GroqGenie(apiKey, model, null, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds GroqGenie to the service collection using custom options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">Configuration options for GroqGenie.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGroqGenie(
        this IServiceCollection services,
        GenieOptions options)
    {
        services.AddSingleton<IGeneration>(sp =>
        {
            var logger = sp.GetService<ILogger<GroqGenie>>();
            return new GroqGenie(options, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds GroqGenie to the service collection using the GROQ_API_KEY environment variable.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="model">The model name to use. Defaults to llama-3.3-70b-versatile.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGroqGenieFromEnvironment(
        this IServiceCollection services,
        string? model = null)
    {
        services.AddSingleton<IGeneration>(sp =>
        {
            var logger = sp.GetService<ILogger<GroqGenie>>();
            return GroqGenie.FromEnvironment(model, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds CerebrasGenie to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiKey">The Cerebras API key.</param>
    /// <param name="model">The model name to use. Defaults to llama-3.3-70b.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCerebrasGenie(
        this IServiceCollection services,
        string apiKey,
        string? model = null)
    {
        services.AddSingleton<IGeneration>(sp =>
        {
            var logger = sp.GetService<ILogger<CerebrasGenie>>();
            return new CerebrasGenie(apiKey, model, null, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds CerebrasGenie to the service collection using custom options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">Configuration options for CerebrasGenie.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCerebrasGenie(
        this IServiceCollection services,
        GenieOptions options)
    {
        services.AddSingleton<IGeneration>(sp =>
        {
            var logger = sp.GetService<ILogger<CerebrasGenie>>();
            return new CerebrasGenie(options, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds CerebrasGenie to the service collection using the CEREBRAS_API_KEY environment variable.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="model">The model name to use. Defaults to llama-3.3-70b.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCerebrasGenieFromEnvironment(
        this IServiceCollection services,
        string? model = null)
    {
        services.AddSingleton<IGeneration>(sp =>
        {
            var logger = sp.GetService<ILogger<CerebrasGenie>>();
            return CerebrasGenie.FromEnvironment(model, logger);
        });

        return services;
    }
}
