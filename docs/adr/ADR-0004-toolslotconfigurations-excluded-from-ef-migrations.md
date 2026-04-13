# ADR-0004: ToolSlotConfigurations excluded from EF migrations

**Status:** Accepted  
**Date:** 2026-04-12  
**Context:** The ToolSlotConfigurations table is owned by the DBA in production. Column names are uppercase/snake_case. EF reads/writes via [Column] attributes but must never ALTER the schema.  
**Decision:** `ExcludeFromMigrations()` in OnModelCreating. Test fixture creates the table shape in OneTimeSetUp.  
**Consequences:** +DBA retains schema ownership, +no migration conflicts. -Test setup must compensate, -schema changes require DBA coordination.
