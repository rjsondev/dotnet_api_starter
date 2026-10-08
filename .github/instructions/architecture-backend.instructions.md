---
description: "Use for backend changes in this .NET API repository; covers architecture, project boundaries, API, and data-access conventions."
applyTo: "**"
---

# Project

`DotnetApiStarter.slnx` is a .NET 10 solution. Products is the sample feature; `/weatherforecast` is template-only.

| Project/folder | Responsibility |
|---|---|
| `apps/api/DotnetApiStarter.Api` | Startup/composition; Minimal API routes in `Features/`; response envelope in `Common/Responses/`; exception handling in `Middleware/`. |
| `core/DotnetApiStarter.Application` | MediatR use cases in `Features/`; abstractions, behaviors, and exceptions in `Common/`. |
| `core/DotnetApiStarter.Domain` | Entities in `Entities/`; shared entity types in `Common/`; no project dependencies. |
| `core/DotnetApiStarter.Infrastructure` | Application abstraction implementations, SQL Server/EF Core, DI, mappings, and migrations. |

# Architecture and Rules

- Flow: API → Application → Domain. Infrastructure implements Application abstractions; API composes Application and Infrastructure.
- Keep Domain independent. Never add API or Infrastructure dependencies to Application or Domain.
- Keep HTTP concerns in API, use-case/business behavior in Application/Domain, and provider-specific data access in Infrastructure.
- Extend existing MediatR feature-slice and Minimal API patterns; avoid parallel request, routing, or response conventions.

# Patterns to Reuse

- Add use cases under `Application/Features/<Feature>/Commands` or `Queries`; colocate MediatR request, handler, FluentValidation validator, and result/DTO.
- Register services through `AddApplication` and `AddInfrastructure`; use the existing validation pipeline.
- Add route groups under API `Features/` (see Products); inject `ISender`, forward `CancellationToken`, and return `ResponseResult<T>`.
- Keep exception-to-HTTP mapping in `GlobalExceptionHandler`.
- Use `IApplicationDbContext` in handlers. Put EF mappings in `Infrastructure/Persistence/Configurations`; update `Infrastructure/Migrations` for schema changes. Use `AsNoTracking()` for read-only queries.

# Conventions and Current Gaps

- Preserve nullable reference types, implicit usings, PascalCase naming, constructor injection, and async EF APIs.
- `DefaultConnection` configures SQL Server. Do not commit secrets or environment-specific credentials.
- Authentication/authorization are not configured; do not assume routes are protected.
- No test project exists. TODO: Verify test conventions before adding one. Build with `dotnet build DotnetApiStarter.slnx`.
- SQL Server is the only established integration. TODO: Verify architecture before adding other integrations.

# Context Loading Rules

Inspect only files relevant to the requested change. Start with the affected project and a neighboring implementation; read other layers only as needed to follow dependencies or cross-cutting behavior. Do not re-read the whole repository by default.
