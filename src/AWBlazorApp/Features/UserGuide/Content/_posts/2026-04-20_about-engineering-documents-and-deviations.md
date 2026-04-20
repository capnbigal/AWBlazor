---
title: About Engineering Documents and Deviations
summary: Where specs and drawings live, and how temporary exceptions are recorded without requiring a full ECO.
tags: [engineering, documents, deviations]
category: entity-guide
author: AWBlazor
---

Not every engineering artifact is a routing or a BOM. Specifications, drawings, test procedures, and quality standards all live as documents, and not every change to how a product is built deserves a full change-order process. The Engineering Documents page at `/engineering/documents` holds the document library, and the Deviations page at `/engineering/deviations` records approved temporary exceptions that would otherwise clutter the ECO backlog.

## The document library

Engineering documents are stored as typed records with metadata — document number, title, revision, category, effective date, superseded-by reference — plus a file attachment. The Documents page at `/engineering/documents` lists every document with sortable and filterable columns. The page distinguishes between documents that are currently effective and documents that have been superseded; superseded documents are kept for historical reference but do not appear in lookups on Production pages.

## Document revisions

When a document is updated, a new revision is created rather than overwriting the previous file. The old revision's effective date range is closed off and the new revision takes over from that date forward. Production work orders that were created against the old revision continue to reference it, so an auditor can always reconstruct which exact document the shop floor was working from on any given date.

## Linking documents to products and processes

Documents can be attached to specific products, routings, BOMs, or stations. A torque specification, for example, may be attached to a specific station and routing operation so that it appears in context when an operator opens the work instructions for that operation. The same document can be attached to multiple contexts, and each attachment has its own effective date range so one document can cover several generations of the things it describes.

## Deviations at /engineering/deviations

A Deviation is a time-bound exception to an engineering definition. When a material is substituted on a single work order because the preferred part is out of stock, when a routing operation is skipped because a station is down for maintenance, or when a quality check is relaxed for a pilot run — these are deviations. Each deviation has a scope (product, work order, date range, station) and an expiry, so the exception does not silently persist after the original situation is resolved.

## Deviation approvals

Deviations require approval from a defined list of roles — typically an engineering approver and a quality approver. Until approved, a deviation is proposed but not in effect; Production sees it as pending. Once approved, it becomes active over its scoped range. At expiry, the deviation automatically closes and Production reverts to the underlying definition. This gives you a controlled way to handle real-world exceptions without either ignoring them or burying them in a full ECO cycle.
