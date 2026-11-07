# AGENTS.md — F1 Statistics API

## 1. Purpose and scope

This file contains the persistent technical instructions for AI agents working on F1 Statistics API.

These instructions define development standards, architectural constraints and validation requirements. They do not authorize new features, independent tasks, agent creation or changes outside an assigned task.

The assigned task defines what must be developed. This document defines the technical standards to respect while developing it.

## 2. Project context

F1 Statistics API is an ASP.NET Core application built with .NET 8, designed to collect, process, store and expose Formula 1 statistics, primarily using OpenF1.

Before implementing a task:

- Consult `README.md` for project context.
- Inspect the relevant existing implementation.
- Identify the services, interfaces, entities and tests affected.
- Follow the current implementation style whenever applicable.
- Do not assume documentation is complete or fully up to date. Verify behavior against the source code.

Do not modify the README or this file unless explicitly assigned.

## 3. Architecture

Preserve the existing layered architecture:

- `WebApi/`: HTTP endpoints and application configuration.
- `Services/`: business logic.
- `Dao/`: persistence and Entity Framework Core operations.
- `ExternalApi/`: external API integrations.
- `Entities/`: domain models and DTOs.
- `Responses/`: response models and shared wrappers.
- `UnitTests/`: automated tests.

Do not bypass established architectural boundaries without explicit authorization.

Reuse existing abstractions when appropriate. Avoid introducing unnecessary layers, patterns or dependencies.

## 4. Coding conventions

- Follow established naming conventions and code organization.
- Keep changes limited to the assigned scope.
- Preserve existing public contracts unless changes are explicitly requested.
- Follow existing interface and dependency injection conventions.
- Use `[IncludeDependencyInjection]` and matching interfaces where required by the existing architecture.
- Use existing response wrappers where applicable.
- Avoid code duplication without introducing unnecessary abstractions.
- Do not perform unrelated refactoring.

When multiple valid solutions exist, prefer the solution most consistent with the existing codebase.

## 5. External API and persistence

When working with OpenF1:

- Follow documented endpoint contracts.
- Handle empty, incomplete and unsuccessful responses.
- Respect applicable rate limits.
- Avoid unnecessary or duplicate requests.
- Preserve existing processing and data mapping conventions.
- Do not introduce undocumented assumptions about F1 data.

When working with persistence:

- Follow existing DAO and EF Core patterns.
- Avoid unintended duplicate data.
- Preserve existing relationships and data contracts.
- Do not execute destructive database changes without explicit authorization.


## 6. Change boundaries

Agents must not:

- Modify unrelated functionality.
- Introduce major architectural changes without approval.
- Replace existing frameworks without authorization.