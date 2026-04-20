---
title: About Routings and Bills of Materials
summary: How routings describe the process and BOMs describe the product — two sides of the manufacturing definition.
tags: [engineering, routings, boms]
category: entity-guide
author: AWBlazor
---

A routing and a bill of materials together define how a product is built. The routing describes the sequence of operations — which stations, in what order, with what setup and cycle time. The bill of materials describes the physical inputs — which parts, in what quantities, with what substitutions allowed. Both are versioned and both are authoritative for Production, which always works against a specific released revision rather than the current draft.

## Routings at /engineering/routings

A routing is a sequence of operations, each tied to a station. An operation has a setup time, a cycle time per unit, a scrap allowance, and a link to the work instructions that describe how to perform it. The Routings page at `/engineering/routings` lists every routing in the system; the detail view at `/engineering/routings/{id}` shows the full operation sequence and lets you compare revisions side-by-side. Production runs reference the specific routing revision that was current when they were released, so later routing edits do not retroactively change historical runs.

## Bills of materials at /engineering/boms

A BOM lists the parts that go into a product and the quantity of each. BOMs can be single-level (just the immediate children) or multi-level (the full explosion down to raw stock). The BOMs page at `/engineering/boms` shows the current BOM for every product; the detail view at `/engineering/boms/{id}` renders the BOM as a tree with quantities rolled up at each level. Alternate parts and make-or-buy preferences are captured in the BOM line rather than managed separately, so everything about what goes into a product is in one place.

## Versioning and revision control

Both routings and BOMs are versioned. Every change creates a new revision; the old revision is kept for historical reference. An ECO at `/engineering/ecos` is the formal mechanism for moving from one revision to the next. When Production creates a new work order, the work order snapshots the then-current revisions of both the routing and the BOM, so even if the routing is later updated, the work order continues to reference the revision it started with.

## BOM catalog (raw)

The Reference data subgroup under Engineering links to the raw BOM catalog at `/aw/bill-of-materials`. This is the historical AdventureWorks data that predates the curated `/engineering/boms` workflow and is kept for comparison, migration, and audit purposes. In normal operation you work from `/engineering/boms`; you open the raw catalog only when you need to trace a value back to its original source or compare against a legacy export.

## Cross-linking

From any routing or BOM detail page, cross-links navigate to related entities — the product master record, the list of work orders that use this revision, the inspection plans that apply, and the change orders that have modified it. This makes it easy to trace an issue from a problem part all the way back to the engineering change that introduced it.
