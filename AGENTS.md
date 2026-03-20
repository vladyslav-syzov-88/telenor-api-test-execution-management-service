# AI Agent Instructions (Junie / JetBrains)

This repository is maintained with help from JetBrains Junie.
Follow these instructions strictly unless explicitly overridden by the user.

If there is a conflict between these instructions and Junie defaults,
THIS FILE TAKES PRECEDENCE.

---

## Project Context

- Platform: ASP.NET Core Web API
- Language: C# (.NET 9)
- IDE: JetBrains Rider
- Development: Windows computer with PowerShell
- Architecture: Clean Architecture / Layered (3-project solution)
- Deployment: Linux containers (Docker)
- Database: MSSQL (SQL Server) via Entity Framework Core 9

---

## How Junie Should Work in This Repo

Before writing code:
1. Identify the **exact task** and affected projects
2. Scan nearby files for existing patterns
3. Prefer **small, localized changes**
4. Ask for confirmation before:
   - Cross-project refactors
   - Renaming public APIs
   - Introducing new abstractions

After writing code:
- Ensure the solution builds
- Ensure existing tests still pass
- Do NOT reformat unrelated files

---

## Solution Structure & Boundaries

```
/
  Telenor.Api.TestCycleManagement.sln
  Dockerfile
  docker-compose.yml
  nuget.config
  .editorconfig
  .env
  DESIGN.md
  AGENTS.md

  Telenor.Api.TestCycleManagement/              — ASP.NET Core Web API host
    Program.cs                                   — Class-based entry point with service registration, CORS, Swagger, middleware
    Controllers/
      ProjectsController.cs                      — CRUD for projects, list versions
      VersionsController.cs                      — Create/update versions
      CyclesController.cs                        — CRUD for cycles, clone, nested folder create/list
      FoldersController.cs                       — Update/delete folders
      ExecutionsController.cs                    — Folder search, structured search, single/bulk update, history
      TestCasesController.cs                     — Search, Jira key lookup, CRUD
      DashboardController.cs                     — Cycle summary, version summary, versions overview
      ImportController.cs                        — Zephyr Squad Cloud data import
    Extensions/
      SwaggerExtensions.cs                       — Swashbuckle setup
      ServiceCollectionExtensions.cs             — DI registration + ConfigureInvalidModelBehavior
    Import/
      Configuration/ZephyrImportSettings.cs      — Configuration POCO for Zephyr import
      Models/ImportDtos.cs                       — Import request/response DTOs
      Models/ZephyrModels.cs                     — Zephyr/Jira API response models
      ZephyrClient/ZephyrApiClient.cs            — HTTP client with JWT auth for Zephyr API
      ZephyrClient/ZephyrJwtGenerator.cs         — JWT token generation
      ZephyrImportService.cs                     — Import orchestration service
      ImportConfigurationException.cs            — Custom exception for import config errors
    Models/
      ApiError.cs                                — Standard error response model
    appsettings.json / appsettings.Development.json

  Telenor.Api.TestCycleManagement.Core/          — Domain layer (zero external dependencies)
    Entities/                                    — Project, Version, TestCycle, CycleFolder, TestCase, TestExecution, ExecutionHistory
    Enums/                                       — ExecutionStatus, CycleStatus
    DTOs/                                        — Request/response records for all endpoints
    Interfaces/                                  — Repository contracts (IProjectRepository, ICycleRepository, etc.)

  Telenor.Api.TestCycleManagement.Infrastructure/ — Data access layer
    Data/
      AppDbContext.cs                            — EF Core context with Fluent API configuration
      DesignTimeDbContextFactory.cs              — For `dotnet ef` CLI commands
    Migrations/                                  — EF Core migrations
    Repositories/                                — Repository implementations (ProjectRepository, CycleRepository, etc.)
    DependencyInjection.cs                       — AddInfrastructure() extension method
```

### Project Dependencies

```
Telenor.Api.TestCycleManagement (Web API)
  └── Telenor.Api.TestCycleManagement.Core (Domain)
  └── Telenor.Api.TestCycleManagement.Infrastructure (Data Access)
        └── Telenor.Api.TestCycleManagement.Core (Domain)
```

### Layer Rules

- **Core** has zero project/package dependencies — only domain entities, enums, DTOs, and repository interfaces
- **Infrastructure** depends on Core — implements repository interfaces using EF Core
- **Web API** depends on both Core and Infrastructure — hosts controllers, DI composition, and Swagger

---

## C# Coding Standards (Important)

- Nullable reference types are enabled
- Use `record` for DTOs and immutable models
- Prefer `required` properties
- Prefer `init` over `set` where possible
- Avoid `dynamic`
- Avoid reflection unless explicitly required
- Specify types explicitly unless obvious

### Async & Concurrency
- Never use `async void`
- Always propagate `CancellationToken`
- Do not block on async (`.Result`, `.Wait()`)

---

## Preferred Language Constructs

Junie should prefer:
- Pattern matching over `if/else`
- Guard clauses over nested logic
- LINQ only when readable (avoid clever one-liners)
- `switch` expressions over `switch` statements

Avoid:
- Over-abstracted helper classes
- "Magic" extension methods
- Premature optimizations

## Persistence & EF Core

- ORM: Entity Framework Core 9
- No lazy loading
- DbContext: `AppDbContext` in `Infrastructure/Data/`
- Entity configurations via Fluent API in `OnModelCreating`
- Migrations in `Infrastructure/Migrations/`
- `DesignTimeDbContextFactory` for `dotnet ef` CLI commands
- Avoid shadow properties
- Migrations are **manual** — do not auto-generate
- Auto-migration on startup in Development mode only

## API Design Rules

- REST-style endpoints with `[ApiController]` attribute routing via `[Route("api/[controller]")]`
- Swagger annotations: `[SwaggerOperation]`, `[SwaggerParameter]` on all actions
- Response type annotations: `[ProducesResponseType<T>]`, `[Produces]`, `[Consumes]` on all actions
- `ApiError` model used in `[ProducesResponseType<ApiError>]` for error responses
- Proper HTTP status codes (200, 201, 204, 404)
- Controllers should be thin and declarative — delegate to repository interfaces
- No custom endpoint mapping — routes discovered via `app.MapControllers()`

## Configuration & Security

- Never hardcode secrets
- Never log sensitive data
- Avoid exposing internal IDs when possible

## Dependency Injection

- Use built-in Microsoft DI (`IServiceCollection`)
- Infrastructure services registered via `AddInfrastructure()` extension method
- Web API services registered via `AddTestCycleManagement()` extension method
- Prefer `Scoped` for application services
- `Singleton` only for stateless services
- No service locator pattern

## Explicit DO NOT List (Critical)

Junie MUST NOT:
- Change existing public APIs without instruction
- Refactor unrelated code
- Reorder files or folders
- Introduce new NuGet packages silently
- Change architectural boundaries
- Remove or weaken validation
- Remove existing tests

## Decision Heuristics for Junie

When multiple solutions exist:
1. Prefer the simplest correct solution
2. Prefer explicit code over clever code
3. Prefer consistency with existing patterns
4. Prefer maintainability over performance

If uncertain:
- Stop and ask for clarification
- Do NOT guess intent

## Completion Checklist

A task is complete only if:
- Code compiles without warnings
- All tests pass
- No unrelated diffs exist
- Changes respect this document
