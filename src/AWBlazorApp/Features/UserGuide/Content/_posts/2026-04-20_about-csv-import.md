---
title: About CSV Import
summary: The bulk-import utility at /import — loading large batches of data from CSV files with validation and audit.
tags: [developer, import, bulk-data]
category: entity-guide
author: AWBlazor
---

The CSV import tool at `/import` is the bulk-data entry point for AWBlazor. It is designed for situations where loading data one row at a time through the UI is impractical — migrating from another system, loading a large price update, bulk-creating customer records, seeding a new plant's employee list. The page is Admin-only because bulk imports can write thousands of rows at once and need tight controls.

## Supported import types

The page lists every import type the system supports. Each type corresponds to one target entity — Products, Customers, Employees, Assets, Price updates, and so on. Selecting a type reveals the expected CSV format for that import — the exact column names, data types, and whether each column is required. A downloadable template file is linked so you can start from the correct shape rather than guessing.

## The import workflow

You upload a CSV file, and the importer processes it in three phases. Parse phase reads the file and checks for structural issues — missing columns, malformed rows, encoding problems. Validate phase checks each row against business rules — required fields populated, foreign keys resolvable, values within allowed ranges, no duplicates against existing data. Apply phase writes the validated rows to the database as a single transaction. If any row fails validation, the entire apply is rolled back and no rows are written.

## Previewing before committing

Between Validate and Apply is a Preview step. The preview shows a sample of the rows that will be created or updated, highlights any warnings (rows that validated but have unusual values), and summarizes the counts — how many new rows, how many updates, how many unchanged. You can accept the preview and commit, or cancel and fix the CSV and re-upload. Commit is an explicit action so you never write data accidentally.

## Audit trail for imports

Every import is logged as a single activity entry with a generated import identifier, the user who ran it, the file name, the timestamp, the counts of rows affected, and a link to the downloadable summary report. Individual rows written through the import are marked with the import identifier in their audit trail, so you can always trace a specific row back to the bulk operation that created it. This is crucial for troubleshooting — if something looks wrong after an import, the identifier tells you which import affected it.

## When not to use CSV import

CSV import is the wrong tool when data comes from another system on an ongoing basis — for that, use the Inventory inbound queue or set up an integration that calls the REST API directly. CSV import is for one-off or occasional batch loads. Integrations that run on a schedule should go through Outbox and Queue so they participate in the normal retry and error-handling workflow.
