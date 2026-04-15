---
title: About Tool Slot Configurations
summary: How AWBlazor integrates with the externally managed ToolSlotConfigurations table.
tags: [tool-slots]
category: entity-guide
author: AWBlazor
---

Tool Slot Configurations represent machine tooling setups used in manufacturing operations. In AWBlazor, the `ToolSlotConfiguration` entity maps to the `dbo.ToolSlotConfigurations` table in SQL Server, but unlike every other table in the application, this table is not created or modified by EF Core migrations. It is externally managed by a DBA, and the application treats it as a read/write data source with a fixed schema.

## External Table Management

The `ToolSlotConfigurations` table is configured in `ApplicationDbContext.OnModelCreating` with `.ExcludeFromMigrations()`. This tells EF Core to include the entity in the runtime model for queries and writes, but to skip it entirely when generating migration scripts. If you run `dotnet ef migrations add`, no `CREATE TABLE` or `ALTER TABLE` statement will appear for this table. Any structural changes — adding columns, changing data types, modifying constraints — must be coordinated directly with the DBA who owns the table in production.

## Column Mapping

The C# entity uses standard PascalCase property names, but the underlying database columns follow an uppercase or snake_case naming convention inherited from the external system. Every property on `ToolSlotConfiguration` has an explicit `[Column("...")]` attribute that maps it to the real column name. For example, `Id` maps to `CID`, `MtCode` maps to `MT_CODE`, and `Family` maps to `FAMILY`. This mapping is critical — if you add or rename a property in C#, you must add the corresponding `[Column]` attribute pointing to the exact column name in the database, and that column must already exist.

## Audit Trail

Tool slot configuration changes are tracked through audit columns on the entity. When a row is created or updated through the application, timestamp and user identity fields are populated automatically. This provides traceability for configuration changes without relying on SQL Server triggers or Change Data Capture. The audit data is visible in the tool slot detail views and can be used to investigate when and by whom a particular tooling setup was modified.

## Working with Tool Slots in the UI

The Tool Slot Configurations page presents a MudDataGrid with sortable and filterable columns. Each row can be expanded to reveal additional detail fields and the audit history. Creating and editing tool slot configurations is restricted to users with appropriate roles. Because the table schema is externally controlled, the application validates input against the known column constraints before attempting a write — this prevents cryptic SQL errors when a value exceeds a column's maximum length or violates a check constraint.

## Test Environment Considerations

Since EF Core migrations never touch this table, the test environment needs special handling. The integration test fixture in `AWBlazorApp.Tests` runs an idempotent `CREATE TABLE IF NOT EXISTS` SQL block during `OneTimeSetUp` to ensure the `ToolSlotConfigurations` table exists in the `AdventureWorks2022_dev` database. Tests that write rows to this table are responsible for cleaning up after themselves to avoid polluting the shared dev database.
