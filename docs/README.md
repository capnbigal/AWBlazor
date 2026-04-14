# AWBlazorApp Documentation

A structured knowledge library for the AWBlazorApp codebase. Organized so future developers (human and AI) can find authoritative guidance fast.

## Navigation

| Folder | Purpose |
|---|---|
| [research/](./research/) | Distilled summaries of official .NET / Blazor / MudBlazor / EF Core / SQL Server / Identity docs |
| [architecture/](./architecture/) | App-level architecture notes, layer responsibilities, conventions |
| [patterns/](./patterns/) | Reusable code patterns (ServiceStack-style adaptations, etc.) |
| [reviews/](./reviews/) | Code review reports — point-in-time snapshots of findings |
| [features/](./features/) | Feature backlog and roadmap |
| [agents/](./agents/) | Specialist reference index for AI assistants |
| [adr/](./adr/) | Architecture decision records |

## Quick start for new contributors

1. Read [`/CLAUDE.md`](../CLAUDE.md) at repo root for the project's ground rules
2. Read [`docs/architecture/application-architecture.md`](./architecture/application-architecture.md) for the high-level shape
3. Read [`docs/architecture/conventions.md`](./architecture/conventions.md) for naming / structural rules
4. Skim [`docs/agents/specialist-references.md`](./agents/specialist-references.md) for task-based checklists

## Quick start for AI assistants

Read [`docs/agents/specialist-references.md`](./agents/specialist-references.md) — it has task-based decision trees and direct links to the right reference.

## Reference docs

| Topic | Document |
|---|---|
| Blazor Web App on .NET 10 | [research/blazor-net10-reference.md](./research/blazor-net10-reference.md) |
| MudBlazor 9 patterns | [research/mudblazor-9-reference.md](./research/mudblazor-9-reference.md) |
| EF Core 10 + SQL Server | [research/efcore-sqlserver-reference.md](./research/efcore-sqlserver-reference.md) |
| ASP.NET Core Identity + auth | [research/identity-auth-reference.md](./research/identity-auth-reference.md) |
| ServiceStack-style patterns | [patterns/servicestack-style-patterns.md](./patterns/servicestack-style-patterns.md) |

## Reports

| What | Where |
|---|---|
| Latest code review | [reviews/code-review-2026-04-13.md](./reviews/code-review-2026-04-13.md) |
| Feature backlog (prioritized) | [features/backlog.md](./features/backlog.md) |
| Phase plan history | [phase-plan.md](./phase-plan.md) |
| Tool slots extraction guide | [tool-slots-extraction-guide.md](./tool-slots-extraction-guide.md) |

## Architecture decisions

ADRs (Architecture Decision Records) capture *why* certain choices were made. Currently:
- [adr/ADR-0004-toolslotconfigurations-excluded-from-ef-migrations.md](./adr/ADR-0004-toolslotconfigurations-excluded-from-ef-migrations.md)

Add new ADRs as `adr/ADR-XXXX-short-name.md` with sections: Status, Context, Decision, Consequences.

## Documentation conventions

- Markdown files only (no Word/PDF in repo)
- One topic per file — keep files scannable
- Use tables for any "options matrix" content
- Code samples are expected to compile against the current codebase
- Date-stamp time-sensitive content (review reports, etc.)
- When documenting a fix to a tricky problem, link the problem AND the fix
