---
description: 'Guidelines for building C# applications'
---

# Guidelines for building C# applications

- Always use the latest version of .NET and C#, currently .NET 10 and C# 14 features.

## C# 14 New Features

Use these C# 14 features where appropriate:

- **Extension members**: Properties and static members, not just methods (`public static extension SequenceExtensions for IEnumerable<T>`)
- **Null-conditional assignment**: `customer?.Order = GetCurrentOrder();` instead of null checks. Works with `+=`, `-=`, etc.
- **Field keyword**: Access backing fields in properties (`set => field = value ?? throw new ArgumentNullException();`)
- **Partial constructors/events**: One defining + one implementing declaration
- **User-defined compound assignment**: Custom operators for domain types

## General Instructions

- Make only high confidence suggestions when reviewing code changes.
- Write code with good maintainability practices, including comments on why certain design decisions were made.
- If in doubt about a design decision or a technical detail, ask for clarification rather than making any assumptions.
- Write clear and concise comments for each function, including purpose, parameters, return values, and any exceptions thrown.
- Handle edge cases and write clear exception handling.
- Add logging where appropriate for debugging and monitoring, including error and performance logs. Set log levels appropriately.
- Add unit tests for new functionality and edge cases first, before implementing the functionality.
- Follow SOLID principles and best practices for object-oriented design.
- When suggesting code, ensure it adheres to the project's existing coding style and conventions.
- For libraries or external dependencies, mention their usage and purpose in comments. Verify their licenses are compatible with the project (MIT, Apache 2.0, BSD are acceptable). If unsure, ask for clarification.

## Naming Conventions

- Follow PascalCase for component names, method names, and public members.
- Use camelCase for private fields and local variables.
- Prefix private fields names with "_" (e.g., "_userService").
- Prefix interface names with "I" (e.g., "IUserService").

## Formatting

- Apply code-formatting style defined in `.editorconfig`.
- Follow standard C# conventions if no `.editorconfig` is provided.
- Follow SonarLint rules for C# if no other style guide is specified and/or overrides are not provided.
- Use spaces for indentation (no tabs).
- Prefer file-scoped namespace declarations and single-line using directives.
- Insert a newline before the opening curly brace of any code block (e.g., after `if`, `for`, `while`, `foreach`, `using`, `try`, etc.).
- Ensure that the final return statement of a method is on its own line.
- Use pattern matching and switch expressions wherever possible.
- Use `nameof` instead of string literals when referring to member names.
- Ensure that XML doc comments are created for any public APIs. When applicable, include `<example>` and `<code>` documentation in the comments.

## Project Setup and Structure

### Repository Structure

- Follow the standardized clean architecture/onion architecture repository structure, with clear separation of concerns and support for vertical slicing of features.
- Split the solution into multiple projects and libraries if necessary or required by specification (if no specification is provided, evaluate splitting by the complexity and size of the application).
- Try to reuse existing libraries and projects within the organization to avoid duplication.
- Keep it simple for small applications, demos, prototypes, and/or tools.

Try to respect the following structure as a guideline:

```text
<solution-name>/
├── .github/workflows/                         # CI/CD pipelines
├── docs/                                      # Technical documentation
├── scripts/                                   # Automation scripts
├── src/
│   ├── <ProjectName>.Api/
│   │   ├── Extensions/                        # Extensions and helper classes
│   │   ├── Modules/                           # Feature-based organization
│   │   │   ├── <Module-Name-1>/
│   │   │   │   └── ...                        # (endpoints, controllers, models, business logic for module 1)
│   │   │   ├── <Module-Name-2>/
│   │   │   │   └── ...                        # (endpoints, controllers, models, business logic for module 2)
│   │   │   └── <Module-Name-N>/
│   │   │       └── ...                        # (endpoints, controllers, models, business logic for module N)
│   │   ├── Program.cs                         # Entry point
│   │   └── appsettings.json                   # Configuration
│   │       └── appsettings.{Environment}.json # Configuration for development, staging, production, etc.
│   ├── <ProjectName>.Abstractions/            # Shared interfaces and DTOs
│   ├── <ProjectName>.Core/                    # Domain models, interfaces, services
│   ├── <ProjectName>.Infrastructure/          # Data access and/or external services
│   ├── <ProjectName>.<Library1>/              # Reusable libraries
│   ├── <ProjectName>.<Library2>/              # Reusable libraries
│   ├── <ProjectName>.<LibraryN>/              # Reusable libraries
|
├── tests/                                     # Unit and integration tests in parallel folder
│   └── <ProjectName>.Tests/
├── Dockerfile                                 # Container build instructions
├── .gitignore                                 # Git ignore rules
├── docker-compose.yaml                        # Local orchestration
├── .env.example                               # Environment variables template
├── .editorconfig                              # Solution-level editorconfig
├── <solution-name>.sln                        # Visual Studio Solution
├── README.md                                  # Project overview
├── LICENSE                                    # Project license
└── CHANGELOG.md                               # Project changelog
```

### Organization Guidelines

- Use feature folders or domain-driven design for complex applications
- Separate concerns: models, services, data access layers
- Place tests in parallel `tests/` folder structure
- Explain Program.cs and configuration system in ASP.NET Core 10 or .NET 10 Console apps

### Configuration Management

- **ALWAYS** use Docker environment variables for environment-specific settings
- Use `appsettings.json` only for default/base configurations that are not environment-specific
- Configure in `Program.cs`: `builder.Configuration.AddEnvironmentVariables();`
- Pass settings via Docker Compose `.env` files or container orchestrator (Kubernetes secrets/configmaps)
- Example in `docker-compose.yaml`:

  ```yaml
  environment:
    - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
    - Logging__LogLevel__Default=${LOG_LEVEL}
  ```

- Keep `appsettings.{Environment}.json` files for local development only. Support all environment overrides through Docker env vars as well.

## Nullable Reference Types

- Declare variables non-nullable, and check for `null` at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.
- Prefer using the null-coalescing operator (`??`) and null-coalescing assignment operator (`??=`) for default values.
- Use the null-conditional operator (`?.`) to safely access members of potentially null objects.
- Avoid using the null-forgiving operator (`!`) unless absolutely necessary, and document why it's safe to do so.
- Prefer the `OperationResult` pattern for methods that can fail instead of returning `null`.

## Data Access Patterns

- Guide the implementation of a data access layer using Entity Framework Core.
- Explain different options (SQL Server, SQLite, In-Memory) for development and production.
- Demonstrate repository pattern implementation and when it's beneficial.
- Show how to implement database migrations and data seeding.
- Explain efficient query patterns to avoid common performance issues.

## Authentication and Authorization

- Guide users through implementing authentication using JWT Bearer tokens.
- Explain OAuth 2.0 and OpenID Connect concepts as they relate to ASP.NET Core.
- Show how to implement role-based and policy-based authorization.
- Demonstrate integration with Microsoft Entra ID (formerly Azure AD).
- Explain how to secure both controller-based and Minimal APIs consistently.

## Validation and Error Handling

- Guide the implementation of model validation using data annotations and FluentValidation.
- Explain the validation pipeline and how to customize validation responses.
- Demonstrate a global exception handling strategy using middleware.
- Show how to create consistent error responses across the API.
- Explain problem details (RFC 7807) implementation for standardized error responses.

## API Versioning and Documentation

- Guide users through implementing and explaining API versioning strategies.
- Demonstrate Swagger/OpenAPI implementation with proper documentation.
- Show how to document endpoints, parameters, responses, and authentication.
- Explain versioning in both controller-based and Minimal APIs.
- Guide users on creating meaningful API documentation that helps consumers.

## Logging and Monitoring

- Guide the implementation of structured logging using Serilog.
- Explain the logging levels and when to use each.
- Write logs to multiple sinks (console, files, external systems).
- Demonstrate integration with OpenTelemetry for telemetry collection.
- Support exporting telemetry to Aspire, Grafana, or other monitoring platforms.
- Show how to implement custom telemetry and correlation IDs for request tracking.
- Explain how to monitor API performance, errors, and usage patterns.

## Testing

### Unit Test Best Practices

- **Test framework**: Use xUnit v3 (preferred) or check what test framework is used by the project (do not introduce new frameworks).
- **Naming**: `MethodName_Scenario_ExpectedBehavior` (e.g., `Add_SingleNumber_ReturnsSameNumber`)
- **Organization**: Separate test projects per application/library project; use folders to mirror namespace structure
- Place test projects in a dedicated `tests/` folder from the root of the repository.
- **One concern per test**: Each test should verify a single behavior
- **AAA pattern**: Arrange, Act, Assert (no comments for these sections)
- **Avoid infrastructure dependencies**: Mock databases, file systems, network calls:
  - Use in-memory databases (e.g., InMemory provider for EF Core)
  - Use mocking frameworks (always use NSubstitute, do not use Moq)
- For **human-readable assertions and messages** use Shouldly (do not use FluentAssertions)
- **Fast execution**: Unit tests should run in milliseconds
- Use `[Fact]` for invariant tests, `[Theory]` with `[InlineData]` for parameterized tests
- Use test fixtures for shared setup/teardown logic
- Ensure tests are independent and can run in any order
- **Integration Tests**: if tests require specific environment variables, infrastructure or configurations, document them clearly in the technical documentation. Consider them integration tests instead of unit tests.
  - Use Assert.Skip to handle dynamically running tests, verify conditions, and skip those that cannot run in certain environments and/or if runtime conditions are not fulfilled.

### Testing Guidelines

- Always include test cases for critical paths
- Copy existing naming style in nearby test files
- Explain integration testing for API endpoints
- Demonstrate mocking dependencies with libraries like NSubstitute
- Use Shouldly for assertions to improve readability
- Show how to test authentication and authorization logic
- Explain test-driven development principles for API development

## Performance Optimization

- Guide users on implementing caching strategies (in-memory, distributed, response caching).
- Explain asynchronous programming patterns and why they matter for API performance.
- Demonstrate pagination, filtering, and sorting for large data sets.
- Show how to implement compression and other performance optimizations.
- Explain how to measure and benchmark API performance.

## Container Publishing

- **ALWAYS** use Podman and/or Docker with Dockerfiles following standard Docker best practices
- Development environment: Podman and/or Docker on Linux (mandatory)
- Use multi-stage builds for optimized images
- Follow Docker standards: layer optimization, minimal base images, security scanning
- Container images must be compatible with standard Docker registries and orchestrators

## MCP Tools

Use always MCP tools for latest documentation and best practices:

- Querying Microsoft Documentation
  - #microsoftdocs/mcp  
    You have access to MCP tools called `microsoft_docs_search`, `microsoft_docs_fetch`, and `microsoft_code_sample_search` - these tools allow you to search through and fetch Microsoft's latest official documentation and code samples, and that information might be more detailed or newer than what's in your training data set. When handling questions around how to work with native Microsoft technologies, such as C#, F#, ASP.NET Core, Microsoft.Extensions, NuGet, Entity Framework, the `dotnet` runtime - please use these tools for research purposes when dealing with specific / narrowly defined questions that may occur.

## Tools

When launching terminal commands, especially on Windows, always consider to use Powershell for scripting, unless specified otherwise.
