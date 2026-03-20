# Telenor.Api.TestCycleManagement — Design Document

## Problem Statement

Zephyr Squad Cloud has a limited number of API requests per hour, causing test result publishing failures during large test runs. The Jira plugin UI is inconvenient for observing cycle progress and release readiness. We need an internal replacement that mirrors the current Zephyr integration while removing these limitations.

## Current Zephyr Usage (from onescreen-uitests)

### API Endpoints in Use

| Zephyr Endpoint | Method | Purpose |
|---|---|---|
| `/public/rest/api/1.0/cycles/search` | GET | List test cycles by project/version |
| `/public/rest/api/1.0/folders` | GET | List folders within a cycle |
| `/public/rest/api/2.0/executions/search/folder/{folderId}` | GET | Get executions by folder (paginated, batch 50) |
| `/public/rest/api/1.0/execution/{executionId}` | PUT | Update single execution (with comment) |
| `/public/rest/api/1.0/executions` | POST | Bulk update execution statuses (max 1000) |
| `/public/rest/api/1.0/zql/search` | POST | Search executions via ZQL query |

### Jira Endpoints in Use

| Jira Endpoint | Method | Purpose |
|---|---|---|
| `rest/api/2/issue/{issueKey}` | GET | Resolve Jira issue ID from key |
| `rest/api/latest/project/{projectId}/versions` | GET | List project versions |

### Test Execution Flow

1. **Setup** — `ZephyrTestManager.InitTestExecutions()` fetches all executions for the configured cycle/version/folders
2. **Per test** — Checks execution status to decide skip/run; after test, queues result
3. **Publish** — `ZephyrResultPublisher.FlushResults()` sends bulk updates (status-only) and individual updates (with comments)
4. **Recovery** — `ZephyrPublishFromBackup` replays unpublished results from `logs/zephyr-results-backup.json`

### Execution Status Mapping

| TestState | Status ID | Name |
|---|---|---|
| UnExecuted | -1 | UnExecuted |
| Pass | 1 | Pass |
| Failed | 2 | Fail |
| Warning | 1 | Pass |
| Broken | 2 | Fail |
| InProgress | 3 | WIP |
| Blocked | 4 | Blocked |
| PartiallyPassed | 5 | PartiallyPassed |
| FailedWithIssue | 6 | Failed With Issue |

---

## Architecture

### Tech Stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 9.0 Web API with `[ApiController]` controllers |
| ORM | Entity Framework Core 9.0 |
| Database | MSSQL (SQL Server) |
| Auth | Windows Authentication / API keys (configurable) |
| Caching | IMemoryCache (upgrade to Redis if needed) |
| Real-time | SignalR (for live cycle dashboard — future) |
| Export | ClosedXML for Excel reports (future) |

### Solution Structure

Follows the same pattern as `telenor.api.accountmanagement.service` in the microservices repository.

```
/
  Telenor.Api.TestCycleManagement.sln
  Dockerfile                                        — Multi-stage Alpine build (build → publish → final)
  docker-compose.yml                                — API + SQL Server orchestration
  nuget.config                                      — NuGet package sources
  .dockerignore
  .env                                              — Default SA password
  DESIGN.md                                         — This file

  src/
    Telenor.Api.TestCycleManagement/                — ASP.NET Core Web API host
      Controllers/
        ProjectsController.cs                       — CRUD for projects, list versions
        VersionsController.cs                       — Create/update versions
        CyclesController.cs                         — CRUD for cycles, clone, nested folder create/list
        FoldersController.cs                        — Update/delete folders
        ExecutionsController.cs                     — Folder search, structured search, single/bulk update, history
        TestCasesController.cs                      — Search, Jira key lookup, CRUD
        DashboardController.cs                      — Cycle summary, version summary, versions overview
      Extensions/
        SwaggerExtensions.cs                        — Swashbuckle setup (mirrors Telenor.Api.Core.Web.Extensions.SwaggerExtensions)
        ServiceCollectionExtensions.cs              — DI registration + ConfigureInvalidModelBehavior
      Models/
        ApiError.cs                                 — Standard error response model
      Program.cs                                    — Class-based Program with service registration, CORS, Swagger, middleware pipeline
      appsettings.json / appsettings.Development.json

    Telenor.Api.TestCycleManagement.Core/           — Domain layer (zero dependencies)
      Entities/                                     — Project, Version, TestCycle, CycleFolder, TestCase, TestExecution, ExecutionHistory
      Enums/                                        — ExecutionStatus (matches TestState IDs), CycleStatus
      DTOs/                                         — Request/response records for all endpoints
      Interfaces/                                   — Repository contracts (IProjectRepository, ICycleRepository, etc.)

    Telenor.Api.TestCycleManagement.Infrastructure/ — Data access layer
      Data/
        AppDbContext.cs                             — EF Core context with Fluent API configuration
        DesignTimeDbContextFactory.cs               — For `dotnet ef` CLI commands
        Migrations/                                 — EF Core migrations (InitialCreate included)
      Repositories/                                 — Repository implementations
      DependencyInjection.cs                        — AddInfrastructure() extension method
```

### API Pattern

Follows the `telenor.api.accountmanagement.service` approach:

- **Class-based `Program`** with `[ExcludeFromCodeCoverage]` attribute, `Main(string[] args)` entry point
- **`[ApiController]` controllers** with attribute routing via `[Route("api/[controller]")]`
- **Swagger annotations** — `[SwaggerOperation]`, `[SwaggerParameter]` on all actions
- **Response type annotations** — `[ProducesResponseType<T>]`, `[Produces]`, `[Consumes]` on all actions
- **`ApiError` model** used in `[ProducesResponseType<ApiError>]` for error responses
- **`ConfigureInvalidModelBehavior()`** for consistent model validation error format
- **CORS policy** with `_myAllowSpecificOrigins`
- **Swagger UI** with `DisplayOperationId()` and `DisplayRequestDuration()`
- **XML documentation** included in Swagger via `GenerateDocumentationFile` in csproj
- No custom endpoint mapping extensions — routes discovered via `app.MapControllers()`

**Controller responsibilities:**
- Receive HTTP requests and validate model binding
- Delegate to repository interfaces for data operations
- Return appropriate HTTP status codes (200, 201, 204, 404)

**Repository responsibilities:**
- Execute EF Core queries and commands
- Map between entities and DTOs
- Record execution history for audit trail

### Database Schema

```
Projects
  Id (int, PK, identity)
  JiraProjectId (int, unique)
  Name (nvarchar 256)
  CreatedAt (datetime2)

Versions
  Id (int, PK, identity)
  ProjectId (int, FK → Projects)
  JiraVersionId (int, nullable)
  Name (nvarchar 256)
  IsReleased (bit)
  ReleaseDate (datetime2, nullable)
  CreatedAt (datetime2)

TestCycles
  Id (int, PK, identity)
  ProjectId (int, FK → Projects, restrict)
  VersionId (int, FK → Versions, restrict)
  Name (nvarchar 512)
  Status (nvarchar 64)       — "Active", "Completed", "Draft"
  CreatedAt (datetime2)
  StartDate (datetime2, nullable)
  EndDate (datetime2, nullable)

CycleFolders
  Id (int, PK, identity)
  CycleId (int, FK → TestCycles)
  Name (nvarchar 256)
  SortOrder (int)

TestCases
  Id (int, PK, identity)
  ProjectId (int, FK → Projects)
  JiraIssueKey (nvarchar 64, indexed)
  JiraIssueId (int, nullable)
  Summary (nvarchar 1024)
  CreatedAt (datetime2)
  UpdatedAt (datetime2)

TestExecutions
  Id (int, PK, identity)
  CycleId (int, FK → TestCycles, restrict)
  FolderId (int, FK → CycleFolders, restrict)
  TestCaseId (int, FK → TestCases, restrict)
  VersionId (int, FK → Versions, restrict)
  ProjectId (int, FK → Projects, restrict)
  StatusId (int)             — matches TestState enum IDs (-1, 1, 2, 3, 4, 5, 6)
  AssignedTo (nvarchar 256, nullable)
  Comment (nvarchar max, nullable)
  UpdatedAt (datetime2)

ExecutionHistory
  Id (bigint, PK, identity)
  ExecutionId (int, FK → TestExecutions)
  StatusId (int)
  Comment (nvarchar max, nullable)
  ChangedBy (nvarchar 256)
  ChangedAt (datetime2)
```

**Key indexes:**
- `TestExecutions (CycleId, FolderId)` — folder-based execution search
- `TestExecutions (CycleId, StatusId)` — cycle summary aggregation
- `TestCases (JiraIssueKey)` — test case lookup by Jira key
- `ExecutionHistory (ExecutionId, ChangedAt)` — audit trail queries

**Delete behavior:**
- All FK references from TestExecutions use `Restrict` to prevent accidental cascade deletes
- CycleFolders cascade delete with their parent TestCycle
- ExecutionHistory cascades with its parent TestExecution

### API Endpoints

These mirror the Zephyr API surface to minimize changes in the test framework.

#### Projects — `ProjectsController`
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/projects` | List all projects |
| POST | `/api/projects` | Create a project |
| GET | `/api/projects/{projectId}/versions` | List versions for a project |

#### Versions — `VersionsController`
| Method | Route | Purpose |
|---|---|---|
| POST | `/api/versions` | Create a version |
| PUT | `/api/versions/{id}` | Update a version (name, release status, date) |

#### Cycles — `CyclesController`
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/cycles?projectId=&versionId=` | List cycles (replaces Zephyr cycles/search) |
| GET | `/api/cycles/{id}` | Get single cycle |
| POST | `/api/cycles` | Create cycle |
| PUT | `/api/cycles/{id}` | Update cycle |
| DELETE | `/api/cycles/{id}` | Delete cycle |
| POST | `/api/cycles/{id}/clone?targetVersionId=` | Clone cycle to new version |
| GET | `/api/cycles/{cycleId}/folders` | List folders in cycle |
| POST | `/api/cycles/{cycleId}/folders` | Create folder in cycle |

#### Folders — `FoldersController`
| Method | Route | Purpose |
|---|---|---|
| PUT | `/api/folders/{id}` | Update folder |
| DELETE | `/api/folders/{id}` | Delete folder |

#### Test Cases — `TestCasesController`
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/testcases?projectId=&search=` | Search test cases |
| GET | `/api/testcases/by-jira-key/{key}` | Lookup by Jira issue key |
| POST | `/api/testcases` | Create test case |
| PUT | `/api/testcases/{id}` | Update test case |

#### Executions — `ExecutionsController`
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/executions/folder/{folderId}?offset=&size=` | Get by folder (replaces Zephyr folder search) |
| POST | `/api/executions/search` | Structured query search (replaces ZQL) |
| PUT | `/api/executions/{id}` | Update single execution with comment |
| POST | `/api/executions/bulk` | Bulk status update |
| GET | `/api/executions/{id}/history` | Execution audit trail (new) |

#### Dashboard — `DashboardController`
| Method | Route | Purpose |
|---|---|---|
| GET | `/api/dashboard/cycles/{cycleId}/summary` | Pass/fail/blocked counts per folder |
| GET | `/api/dashboard/versions/{versionId}/summary` | All cycles for a version with status breakdown |
| GET | `/api/dashboard/versions?projectId=` | Released vs unreleased versions overview |

#### Import — `ImportController`
| Method | Route | Purpose |
|---|---|---|
| POST | `/api/import` | Import cycles, folders, test cases, and executions from Zephyr Squad Cloud |

### Zephyr Data Import

The `ImportController` provides a one-time (or repeatable) migration path from Zephyr Squad Cloud to the local database.

**Import flow:**
1. Ensures the project exists locally (matched by `JiraProjectId`)
2. Fetches Jira versions via Jira REST API (or uses the explicitly provided `versionId`)
3. For each version, calls Zephyr API to get cycles → folders → executions
4. Creates corresponding records in the local database
5. Deduplicates test cases by `JiraIssueKey` within the project

**Request body:**
```json
{
  "projectId": "11786",
  "versionId": "12345",
  "cycleName": "TI UI Microservices Tests"
}
```
- `projectId` (required) — Jira project ID
- `versionId` (optional) — specific version to import; omit to import all versions
- `cycleName` (optional) — filter to import only matching cycles

**Configuration** (in `appsettings.json` → `ZephyrImport` section):
- `BaseUrl` — Zephyr Squad Cloud API base URL
- `AccountId`, `AccessKey`, `SecretKey` — JWT authentication credentials (same as in `onescreen-uitests` appsettings)
- `JwtValiditySeconds` — JWT token expiry
- `JiraBaseUrl`, `JiraUser` — Jira credentials for fetching version metadata

**Implementation:**
- `Import/ZephyrClient/ZephyrApiClient.cs` — standalone HTTP client with JWT auth (mirrors `ZephyrHttpClient` from `onescreen-uitests`)
- `Import/ZephyrClient/ZephyrJwtGenerator.cs` — JWT token generation (mirrors `ZephyrJwtGenerator` from `onescreen-uitests`)
- `Import/ZephyrImportService.cs` — orchestrates the full import pipeline with error resilience (individual folder/cycle failures don't abort the entire import)
- `Import/Models/ZephyrModels.cs` — Zephyr/Jira API response models (using `System.Text.Json`)
- `Import/Configuration/ZephyrImportSettings.cs` — configuration POCO

**Considerations:**
- Test cases are **deduplicated** by `JiraIssueKey` — running import twice won't create duplicate test cases
- Cycles and executions are **always created as new records** — running import twice for the same data will create duplicate cycles
- Errors fetching individual folders/cycles are logged as warnings but don't abort the import
- The Zephyr API rate limit still applies during import — for large datasets, consider running during off-hours
- The Jira API call for versions is optional — if credentials are not configured, supply `versionId` explicitly

### Zephyr API to New API Mapping

| Zephyr Endpoint | New Endpoint | Notes |
|---|---|---|
| `GET /cycles/search` | `GET /api/cycles?projectId=&versionId=` | Query params instead of path segments |
| `GET /folders` | `GET /api/cycles/{cycleId}/folders` | Nested under cycle |
| `GET /executions/search/folder/{folderId}` | `GET /api/executions/folder/{folderId}` | Same pagination model |
| `PUT /execution/{executionId}` | `PUT /api/executions/{id}` | Same request shape |
| `POST /executions` (bulk) | `POST /api/executions/bulk` | Simplified request body |
| `POST /zql/search` | `POST /api/executions/search` | Typed filter object replaces ZQL string |

### ZQL Replacement

Current ZQL queries like:
```
cycleName = "TI UI Microservices Tests" AND fixVersion = "1.0.0" AND folderName in ("OSE Microservices")
```

Are replaced by a typed request body:
```json
{
  "cycleName": "TI UI Microservices Tests",
  "versionName": "1.0.0",
  "folderNames": ["OSE Microservices"],
  "projectId": 11786,
  "maxRecords": 100,
  "offset": 0
}
```

---

## Migration Considerations

### Phase 1 — Parallel Run
- Deploy the new API alongside existing Zephyr integration
- Seed database with existing Zephyr data (one-time export via Zephyr API)
- Modify `ZephyrResultPublisher` to write to both Zephyr and the new API
- Validate data consistency between both systems

### Phase 2 — Read Switchover
- Switch `ZephyrTestManager.InitTestExecutions()` to read from new API
- Keep writing to both systems
- Compare results to verify correctness

### Phase 3 — Full Cutover
- Replace `ZephyrHttpClient` with new API client in the test framework
- Remove Zephyr NuGet packages and configuration
- Decommission Zephyr Jira plugin

### Backward Compatibility
- Execution status IDs (-1, 1, 2, 3, 4, 5, 6) match the existing `TestState` enum — zero mapping changes needed
- Response shapes (`ExecutionResponse` with nested `ExecutionDetailsResponse`) mirror Zephyr's `ZephyrTestExecution` structure
- Consider implementing an adapter layer that maps new API responses to existing Zephyr DTOs during the parallel-run phase

### Test Framework Integration Changes

The following files in `onescreen-uitests` will need modification:

| File | Change |
|---|---|
| `Telenor.ZephyrApi.HttpClient/Clients/ZephyrHttpClient.cs` | Replace with new API client or add `ITestManagementClient` interface |
| `Telenor.UiTests.Tests/Managers/ZephyrTestManager.cs` | Switch data source from Zephyr to new API |
| `Telenor.UiTests.Tests/Managers/ZephyrResultPublisher.cs` | Update publish target URL and request format |
| `Telenor.UITests.Core/Configuration/ZephyrConfiguration.cs` | Replace with new API configuration (simpler — just base URL + optional API key) |
| `OneTimeSetup.cs` | Update DI registration |

---

## Gains Over Zephyr

| Problem | Solution |
|---|---|
| Rate limits (429 errors) | No external rate limits; your own infrastructure |
| No execution history | `ExecutionHistory` table tracks every status change with who/when |
| Slow ZQL queries | Direct SQL with proper indexes; typed search instead of string parsing |
| Cycle cloning is manual | `POST /api/cycles/{id}/clone` with automatic folder + execution duplication |
| Limited dashboard in Jira | Custom dashboard endpoints; can build dedicated UI later |
| JWT token complexity | Simple API key or Windows auth — no per-request token generation |
| Zephyr outages block CI/CD | Internal service with controlled uptime and no third-party dependency |
| No bulk read operations | Search endpoint supports filtering by cycle, version, folders in one call |
| Zephyr plugin license cost | No licensing fees for internal service |

---

## Docker Deployment

### Files

| File | Purpose |
|---|---|
| `Dockerfile` | Multi-stage build: SDK image for build/publish, ASP.NET runtime image for final |
| `docker-compose.yml` | Orchestrates API + SQL Server containers |
| `.dockerignore` | Excludes bin/obj/IDE files from build context |
| `.env` | Default SA password (override for production) |

### Container Architecture

```
docker-compose
  ├── api (testcycle-api)
  │     Port 5100 → 8080
  │     ASP.NET Core 9.0 runtime
  │     Auto-applies EF migrations on startup (Development mode)
  │
  └── sqlserver (testcycle-sqlserver)
        Port 1433 → 1433
        MSSQL 2022 with persistent volume
        Health check: sqlcmd SELECT 1
```

### Quick Start

```bash
# Start both containers (API + SQL Server)
docker compose up -d

# API is available at http://localhost:5100
# SQL Server is available at localhost:1433

# View logs
docker compose logs -f api

# Stop and remove containers (data persists in volume)
docker compose down

# Stop and remove everything including data
docker compose down -v
```

### Configuration

**Environment variables (override in `.env` or docker-compose):**

| Variable | Default | Purpose |
|---|---|---|
| `SA_PASSWORD` | `YourStrong!Passw0rd` | SQL Server SA password |
| `ASPNETCORE_ENVIRONMENT` | `Development` | Enables auto-migration on startup |
| `ConnectionStrings__DefaultConnection` | (set in compose) | DB connection string |

**Production considerations:**
- Change `SA_PASSWORD` to a strong password
- Set `ASPNETCORE_ENVIRONMENT=Production` (disables auto-migration and enables HTTPS redirect)
- Run migrations explicitly via `dotnet ef database update` or a migration job
- Add a reverse proxy (nginx/Traefik) for TLS termination
- Consider using a managed SQL Server instance instead of the container

---

## Jira Connect App Deployment

The service can be installed as an Atlassian Connect app inside Jira Cloud, providing the same in-Jira experience as Zephyr Squad (cycle board, dashboard, issue panel).

### Architecture

```
Jira Cloud
  ├── Top Nav: "Test Cycles"       → iframe → GET /connect/pages/cycles.html
  ├── Top Nav: "Test Dashboard"    → iframe → GET /connect/pages/dashboard.html
  ├── Project Tab: "Test Summary"  → iframe → GET /connect/pages/project-summary.html
  ├── Issue Panel: "Test Executions" → iframe → GET /connect/panels/issue.html
  └── Webhooks (version_created/updated) → POST /api/connect/webhooks/*
        │
        ▼
  This API (existing REST endpoints + Connect layer)
```

### Connect Module Files

| File | Purpose |
|---|---|
| `Connect/ConnectSettings.cs` | Configuration POCO (app key, name, baseUrl, vendor) |
| `Connect/ConnectDescriptorBuilder.cs` | Builds the `atlassian-connect.json` descriptor |
| `Connect/AtlassianJwtMiddleware.cs` | Verifies Atlassian JWT on `/connect/*` iframe requests |
| `Controllers/ConnectDescriptorController.cs` | Serves `GET /atlassian-connect.json` |
| `Controllers/ConnectController.cs` | Lifecycle endpoints (install/uninstall) + version webhooks |
| `Controllers/ConnectPagesController.cs` | Serves iframe HTML pages |
| `wwwroot/connect/` | Frontend HTML/CSS/JS for Jira iframe pages |
| `Core/Entities/ConnectTenant.cs` | Stores tenant credentials (clientKey, sharedSecret) |

### Prerequisites

1. **Public HTTPS URL** — Jira Cloud must be able to reach your API over the internet with a valid TLS certificate. During development, use a tunnel (e.g., ngrok, Cloudflare Tunnel).
2. **Database migration** — Run `dotnet ef database update` to create the `ConnectTenants` table.

### Deployment Steps

1. **Configure the base URL** — Set `AtlassianConnect:BaseUrl` in `appsettings.json` (or via environment variable `AtlassianConnect__BaseUrl`) to the public HTTPS URL where the API is hosted:
   ```json
   "AtlassianConnect": {
     "BaseUrl": "https://tcm.your-domain.com"
   }
   ```

2. **Verify the descriptor is accessible** — Navigate to `https://tcm.your-domain.com/atlassian-connect.json` and confirm it returns valid JSON with the correct `baseUrl`.

3. **Install in Jira Cloud** — In your Jira Cloud instance:
   - Go to **Settings** → **Apps** → **Manage apps**
   - Enable **Development mode** (under Settings at the bottom)
   - Click **Upload app**
   - Enter the descriptor URL: `https://tcm.your-domain.com/atlassian-connect.json`
   - Click **Upload**

4. **Verify installation** — Jira will call `POST /api/connect/installed` with the tenant's `clientKey` and `sharedSecret`. Check your API logs to confirm the lifecycle event was received and the tenant was stored.

5. **Test the integration** — After install:
   - A **"Test Cycles"** and **"Test Dashboard"** link will appear in Jira's top navigation
   - A **"Test Summary"** tab will appear on project pages
   - A **"Test Executions"** panel will appear on the right side of issue views

### Development with ngrok

For local development, expose your API via ngrok:

```bash
# Start the API locally
dotnet run --project Telenor.Api.TestCycleManagement

# In another terminal, start ngrok pointing to the API port
ngrok http 5100

# Use the ngrok HTTPS URL as your BaseUrl
# e.g., https://abc123.ngrok-free.app
```

Then install the app in Jira using `https://abc123.ngrok-free.app/atlassian-connect.json`.

### Configuration

| Setting | Default | Purpose |
|---|---|---|
| `AtlassianConnect:Key` | `telenor-test-cycle-management` | Unique app key (must not change after install) |
| `AtlassianConnect:Name` | `Test Cycle Management` | Display name in Jira |
| `AtlassianConnect:BaseUrl` | (auto-detected) | Public HTTPS URL; auto-detected from request if empty |
| `AtlassianConnect:VendorName` | `Telenor A/S` | Vendor info shown in Jira app listing |

### Security

- Jira sends a `sharedSecret` during install — stored in the `ConnectTenants` table and used to verify JWT signatures on all iframe requests
- The `AtlassianJwtMiddleware` validates JWT tokens on `/connect/*` paths, rejecting requests from unknown or deactivated tenants
- Lifecycle endpoints (`/api/connect/installed`, `/api/connect/uninstalled`) are unauthenticated by design — Atlassian requires this
- Existing REST API endpoints (`/api/*`) are not affected by the Connect middleware and continue to use the existing API key auth

### Production Considerations

- Use a stable, production-grade HTTPS URL (not ngrok) — Jira will stop rendering iframes if the URL becomes unreachable
- The `AtlassianConnect:Key` must remain constant after installation — changing it forces a reinstall
- Store the `sharedSecret` securely; consider encrypting the `ConnectTenants.SharedSecret` column at rest
- For multi-tenant scenarios (multiple Jira instances), each instance gets its own `ConnectTenant` record
- Monitor the `/api/connect/installed` and `/api/connect/uninstalled` endpoints for unexpected lifecycle events

---

## Future Enhancements

- **Authentication** — Add API key or Windows auth middleware (currently `changedBy` is hardcoded to "system")
- **SignalR hub** — Real-time cycle progress updates for dashboard consumers
- **Excel export** — `GET /api/dashboard/cycles/{id}/export` returning .xlsx via ClosedXML
- **Swagger/OpenAPI** — Add `builder.Services.AddOpenApi()` for API documentation
- **Health check** — `GET /health` endpoint for monitoring
- **Data seeding** — One-time migration script to import existing Zephyr data via their API
