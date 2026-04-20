---
title: About Engineering
summary: The Engineering module — change orders, routings, bills of materials, documents, and deviations.
tags: [engineering]
category: entity-guide
author: AWBlazor
---

The Engineering module at `/engineering` is where product and process definitions live. Change orders propose and approve updates to how a product is built. Routings describe the steps a product goes through on the floor. Bills of materials describe what goes into the product. Documents hold the specs and work instructions. Deviations record approved temporary exceptions to any of the above. Together they form the authoritative definition that Production and Quality work against.

## Engineering summary

The Engineering summary page at `/engineering` is the landing view for the module. It shows counts of open change orders, routings pending release, BOMs modified recently, and deviations currently in effect. It is designed to be the first stop when a process engineer sits down — one look tells you whether there is pending approval work waiting for you before you dig into anything else.

## Change orders

Change orders live at `/engineering/ecos` and represent a formal proposal to change a routing, BOM, document, or station definition. Each ECO has a lifecycle (Draft, In review, Approved, Released, Superseded) and an auditable trail of who reviewed what and when. A released ECO is the document that authorizes Production to update their actual build — until an ECO is released, the change remains a proposal.

## Routings and BOMs

Routings at `/engineering/routings` describe the sequence of operations for a given product — which station, in what order, with what setup. Bills of materials at `/engineering/boms` describe the parts and quantities that go into the product. Both are versioned so Production always knows which revision was in effect on a given date, and both reference the raw AdventureWorks catalogs underneath (BOM catalog and work-order routings) for historical data comparisons.

## Documents and deviations

Engineering documents at `/engineering/documents` hold the specifications, drawings, and work instructions that Production references from the shop floor. Deviations at `/engineering/deviations` record approved temporary exceptions — when a material substitution or a process variation is permitted for a specific run without requiring a full ECO. Deviations have a defined scope (a work order, a date range, a station) and expire automatically so they do not quietly become the new normal.

## Reference data

The Reference data subgroup under Engineering folds in the raw AdventureWorks catalogs that back several Engineering screens: the BOM catalog (the raw, historical bill-of-materials data), Illustrations, Cultures, and Units of measure. You normally work from the curated workflow pages above; the reference data is there when you need to audit a code, fix a bad lookup row, or build a report against the historical catalog directly.
