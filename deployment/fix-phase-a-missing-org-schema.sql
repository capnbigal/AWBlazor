-- fix-phase-a-missing-org-schema.sql
--
-- One-off fix for droplets where the Phase A migration was incorrectly stamped
-- as "applied" in __EFMigrationsHistory (via the DashboardItems marker) but the
-- org.* tables were never actually created. When Phase B's migration then tries
-- to create inv.InventoryLocation with a FK to org.OrgUnit, SQL rejects it with:
--
--   Foreign key 'FK_InventoryLocation_OrgUnit_OrgUnitId' references invalid table 'org.OrgUnit'.
--
-- Every statement here is guarded by existence checks so re-running is a no-op.
-- After this runs, the Phase B migration will find org.OrgUnit and succeed on
-- the next container start.

SET NOCOUNT ON;
SET XACT_ABORT ON;
-- sqlcmd connects with QUOTED_IDENTIFIER OFF by default, but the filtered index on
-- Organization.IsPrimary (and any future SET-sensitive index/view) needs it ON.
-- Same for ANSI_NULLS.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

IF SCHEMA_ID(N'org') IS NULL EXEC(N'CREATE SCHEMA [org];');

IF OBJECT_ID(N'[org].[Organization]', N'U') IS NULL
BEGIN
    CREATE TABLE [org].[Organization] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(32) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [IsPrimary] bit NOT NULL,
        [ParentOrganizationId] int NULL,
        [ExternalRef] nvarchar(128) NULL,
        [IsActive] bit NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Organization] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Organization_Organization_ParentOrganizationId]
            FOREIGN KEY ([ParentOrganizationId]) REFERENCES [org].[Organization] ([Id]) ON DELETE NO ACTION
    );
END;

IF OBJECT_ID(N'[org].[CostCenter]', N'U') IS NULL
BEGIN
    CREATE TABLE [org].[CostCenter] (
        [Id] int NOT NULL IDENTITY,
        [OrganizationId] int NOT NULL,
        [Code] nvarchar(32) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [OwnerBusinessEntityId] int NULL,
        [IsActive] bit NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_CostCenter] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CostCenter_Organization_OrganizationId]
            FOREIGN KEY ([OrganizationId]) REFERENCES [org].[Organization] ([Id]) ON DELETE NO ACTION
    );
END;

IF OBJECT_ID(N'[org].[ProductLine]', N'U') IS NULL
BEGIN
    CREATE TABLE [org].[ProductLine] (
        [Id] int NOT NULL IDENTITY,
        [OrganizationId] int NOT NULL,
        [Code] nvarchar(32) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(2000) NULL,
        [IsActive] bit NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_ProductLine] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductLine_Organization_OrganizationId]
            FOREIGN KEY ([OrganizationId]) REFERENCES [org].[Organization] ([Id]) ON DELETE NO ACTION
    );
END;

IF OBJECT_ID(N'[org].[OrgUnit]', N'U') IS NULL
BEGIN
    CREATE TABLE [org].[OrgUnit] (
        [Id] int NOT NULL IDENTITY,
        [OrganizationId] int NOT NULL,
        [ParentOrgUnitId] int NULL,
        [Kind] tinyint NOT NULL,
        [Code] nvarchar(32) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Path] nvarchar(1024) NOT NULL,
        [Depth] tinyint NOT NULL,
        [CostCenterId] int NULL,
        [ManagerBusinessEntityId] int NULL,
        [IsActive] bit NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_OrgUnit] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrgUnit_CostCenter_CostCenterId]
            FOREIGN KEY ([CostCenterId]) REFERENCES [org].[CostCenter] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_OrgUnit_OrgUnit_ParentOrgUnitId]
            FOREIGN KEY ([ParentOrgUnitId]) REFERENCES [org].[OrgUnit] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrgUnit_Organization_OrganizationId]
            FOREIGN KEY ([OrganizationId]) REFERENCES [org].[Organization] ([Id]) ON DELETE NO ACTION
    );
END;

IF OBJECT_ID(N'[org].[Asset]', N'U') IS NULL
BEGIN
    CREATE TABLE [org].[Asset] (
        [Id] int NOT NULL IDENTITY,
        [OrganizationId] int NOT NULL,
        [OrgUnitId] int NULL,
        [AssetTag] nvarchar(64) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Manufacturer] nvarchar(128) NULL,
        [Model] nvarchar(128) NULL,
        [SerialNumber] nvarchar(128) NULL,
        [AssetType] tinyint NOT NULL,
        [CommissionedAt] datetime2 NULL,
        [DecommissionedAt] datetime2 NULL,
        [Status] tinyint NOT NULL,
        [ParentAssetId] int NULL,
        [ModifiedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Asset] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Asset_Asset_ParentAssetId]
            FOREIGN KEY ([ParentAssetId]) REFERENCES [org].[Asset] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Asset_OrgUnit_OrgUnitId]
            FOREIGN KEY ([OrgUnitId]) REFERENCES [org].[OrgUnit] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Asset_Organization_OrganizationId]
            FOREIGN KEY ([OrganizationId]) REFERENCES [org].[Organization] ([Id]) ON DELETE NO ACTION
    );
END;

IF OBJECT_ID(N'[org].[Station]', N'U') IS NULL
BEGIN
    CREATE TABLE [org].[Station] (
        [Id] int NOT NULL IDENTITY,
        [OrgUnitId] int NOT NULL,
        [Code] nvarchar(32) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [StationKind] tinyint NOT NULL,
        [OperatorBusinessEntityId] int NULL,
        [AssetId] int NULL,
        [IsActive] bit NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Station] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Station_Asset_AssetId]
            FOREIGN KEY ([AssetId]) REFERENCES [org].[Asset] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Station_OrgUnit_OrgUnitId]
            FOREIGN KEY ([OrgUnitId]) REFERENCES [org].[OrgUnit] ([Id]) ON DELETE NO ACTION
    );
END;

-- Indexes. PKs above already create clustered unique indexes, so org.Organization.Id,
-- org.OrgUnit.Id, etc. are ready to be FK targets. These extra indexes match what the
-- Phase A migration installs and what EnsureMissingTablesAsync expects at runtime.

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Organization_Code' AND object_id = OBJECT_ID(N'[org].[Organization]'))
    CREATE UNIQUE INDEX [IX_Organization_Code] ON [org].[Organization] ([Code]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Organization_IsPrimary' AND object_id = OBJECT_ID(N'[org].[Organization]'))
    EXEC(N'CREATE UNIQUE INDEX [IX_Organization_IsPrimary] ON [org].[Organization] ([IsPrimary]) WHERE [IsPrimary] = 1');

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Organization_ParentOrganizationId' AND object_id = OBJECT_ID(N'[org].[Organization]'))
    CREATE INDEX [IX_Organization_ParentOrganizationId] ON [org].[Organization] ([ParentOrganizationId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CostCenter_OrganizationId_Code' AND object_id = OBJECT_ID(N'[org].[CostCenter]'))
    CREATE UNIQUE INDEX [IX_CostCenter_OrganizationId_Code] ON [org].[CostCenter] ([OrganizationId], [Code]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductLine_OrganizationId_Code' AND object_id = OBJECT_ID(N'[org].[ProductLine]'))
    CREATE UNIQUE INDEX [IX_ProductLine_OrganizationId_Code] ON [org].[ProductLine] ([OrganizationId], [Code]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OrgUnit_OrganizationId_Code' AND object_id = OBJECT_ID(N'[org].[OrgUnit]'))
    CREATE UNIQUE INDEX [IX_OrgUnit_OrganizationId_Code] ON [org].[OrgUnit] ([OrganizationId], [Code]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OrgUnit_ParentOrgUnitId' AND object_id = OBJECT_ID(N'[org].[OrgUnit]'))
    CREATE INDEX [IX_OrgUnit_ParentOrgUnitId] ON [org].[OrgUnit] ([ParentOrgUnitId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OrgUnit_Path' AND object_id = OBJECT_ID(N'[org].[OrgUnit]'))
    CREATE INDEX [IX_OrgUnit_Path] ON [org].[OrgUnit] ([Path]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OrgUnit_CostCenterId' AND object_id = OBJECT_ID(N'[org].[OrgUnit]'))
    CREATE INDEX [IX_OrgUnit_CostCenterId] ON [org].[OrgUnit] ([CostCenterId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Asset_AssetTag' AND object_id = OBJECT_ID(N'[org].[Asset]'))
    CREATE UNIQUE INDEX [IX_Asset_AssetTag] ON [org].[Asset] ([AssetTag]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Asset_OrganizationId' AND object_id = OBJECT_ID(N'[org].[Asset]'))
    CREATE INDEX [IX_Asset_OrganizationId] ON [org].[Asset] ([OrganizationId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Asset_OrgUnitId' AND object_id = OBJECT_ID(N'[org].[Asset]'))
    CREATE INDEX [IX_Asset_OrgUnitId] ON [org].[Asset] ([OrgUnitId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Asset_ParentAssetId' AND object_id = OBJECT_ID(N'[org].[Asset]'))
    CREATE INDEX [IX_Asset_ParentAssetId] ON [org].[Asset] ([ParentAssetId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Asset_Status' AND object_id = OBJECT_ID(N'[org].[Asset]'))
    CREATE INDEX [IX_Asset_Status] ON [org].[Asset] ([Status]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Station_OrgUnitId_Code' AND object_id = OBJECT_ID(N'[org].[Station]'))
    CREATE UNIQUE INDEX [IX_Station_OrgUnitId_Code] ON [org].[Station] ([OrgUnitId], [Code]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Station_AssetId' AND object_id = OBJECT_ID(N'[org].[Station]'))
    CREATE INDEX [IX_Station_AssetId] ON [org].[Station] ([AssetId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Station_OperatorBusinessEntityId' AND object_id = OBJECT_ID(N'[org].[Station]'))
    CREATE INDEX [IX_Station_OperatorBusinessEntityId] ON [org].[Station] ([OperatorBusinessEntityId]);

PRINT 'org.* schema and tables are now in place. Restart the awblazor-app container.';
