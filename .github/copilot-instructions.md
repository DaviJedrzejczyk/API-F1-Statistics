# Copilot instructions for API-F1-Statistics

## Repository overview

- This is a layered **.NET 8** solution for Formula 1 statistics.
- Main projects:
  - `WebApi/`: ASP.NET Core API entry point, controllers, DI, AutoMapper, JSON config.
  - `Services/`: business logic.
  - `Dao/`: Entity Framework Core data access and migrations.
  - `ExternalApi/`: HTTP clients for upstream F1/OpenF1 data.
  - `Entities/`: entity models and DTOs.
  - `Responses/`: shared response wrappers, converters, and DI attribute.
  - `UnitTests/`: NUnit + Moq tests.
- Normal dependency flow is `WebApi -> Services -> (Dao, ExternalApi) -> Entities/Responses`.

## Fast path for agents

1. Read `WebApi/Program.cs` for app wiring.
2. Read `WebApi/Config/TransientDIConfig.cs` before adding services, DAOs, or API clients.
3. Read `WebApi/Config/AutoMapperConfig.cs` before adding or renaming DTO/view-model mappings.
4. Run tests with:

```bash
dotnet test /home/runner/work/API-F1-Statistics/API-F1-Statistics/UnitTests/UnitTests.csproj
```

5. Build the solution with:

```bash
dotnet build "/home/runner/work/API-F1-Statistics/API-F1-Statistics/API OpenF1.sln"
```

## Run locally

- Start the API with:

```bash
dotnet run --project /home/runner/work/API-F1-Statistics/API-F1-Statistics/WebApi/WebApi.csproj
```

- Development launch settings expose Swagger at `/swagger`.
- Default launch URLs are in `WebApi/Properties/launchSettings.json`.

## Important repository conventions

### Dependency injection

- Concrete classes are auto-registered through Scrutor in `WebApi/Config/TransientDIConfig.cs`.
- New service/DAO/external client classes must:
  - be marked with `[IncludeDependencyInjection]`
  - have a matching interface name for `.AsMatchingInterface()`
- The attribute namespace is intentionally misspelled as `Shared.Common.Atrributes`; preserve the existing spelling unless you are doing a deliberate repo-wide refactor.

### Responses

- The codebase uses shared wrappers from `Responses/Responses/`:
  - `Response`
  - `SingleResponse<T>`
  - `DataResponse<T>`
- Many services/controllers rely on `HasSuccess`, `Message`, `Exception`, `Item`, and `Itens`.
- `Itens` is intentionally the current property name; do not rename it as a drive-by cleanup because it is used widely across production code and tests.

### Persistence and mapping

- EF Core `DbSet<>`s are declared in `Dao/ApiF1DB.cs`.
- Entity configurations live under `Dao/MapConfig/` and are applied with `ApplyConfigurationsFromAssembly(...)`.
- When adding or changing persisted entities, update:
  - entity class in `Entities/Class/`
  - `DbSet<>` in `Dao/ApiF1DB.cs`
  - mapping config in `Dao/MapConfig/`
  - migrations if schema changes are intended
- AutoMapper profiles live in `WebApi/Config/AutoMapperConfig.cs`.

### Tests

- Tests are in `UnitTests/` and use **NUnit**, **Moq**, and **EF Core InMemory**.
- DAO tests commonly use `UseInMemoryDatabase(...)`, so prefer extending existing test patterns instead of requiring a real database for unit coverage.

## Known pitfalls

- `WebApi/appsettings.json` points at a **Windows LocalDB** file path:
  - `Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Djedr\\Documents\\F1DBLocal.mdf;...`
- In Linux/cloud environments, that connection string is not usable as-is.
- Practical workaround:
  - use `dotnet test` for safe validation when possible
  - if you must run database-backed API paths, override `ConnectionStrings__F1DBLocal` with a reachable SQL Server connection first
- Swagger is only enabled in Development.

## Errors encountered during onboarding

- `dotnet test` completed successfully, but the solution emits many existing nullable-reference and NUnit analyzer warnings.
- No code changes were needed to work around them for onboarding; treat them as pre-existing noise unless your task is specifically to reduce warnings.
- The existing unit-test suite passed in the cloud environment because it uses the EF Core in-memory provider instead of the LocalDB connection from `appsettings.json`.

## Change guidance

- Keep changes surgical; this repository has many established naming quirks and response patterns.
- Do not “clean up” spelling inconsistencies like `Itens` or `Atrributes` unless the task explicitly requires a coordinated refactor.
- When adding endpoints, check the existing controller style first: services return response wrappers, controllers translate them into HTTP responses, and AutoMapper handles view-model projection.
