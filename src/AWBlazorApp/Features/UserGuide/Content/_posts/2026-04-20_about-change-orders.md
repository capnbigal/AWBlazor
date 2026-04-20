---
title: About Change Orders (ECOs)
summary: The Engineering Change Order workflow — drafting, review, approval, release, and supersession.
tags: [engineering, change-orders]
category: entity-guide
author: AWBlazor
---

Engineering Change Orders, or ECOs, are the formal mechanism for changing anything that Production runs against. A routing update, a BOM revision, a document replacement, a station configuration change — all of them are expected to be introduced through an ECO so there is an audit trail showing who proposed the change, who reviewed it, who approved it, and when it took effect. The ECO workflow lives at `/engineering/ecos` and the detail view for any ECO is at `/engineering/ecos/{id}`.

## The ECO lifecycle

An ECO moves through five statuses. Draft is the initial state — the author is still composing the change. In review means reviewers have been assigned and the ECO is awaiting their decisions. Approved means every required reviewer has approved but the ECO has not yet taken effect. Released is the transition that makes the change live; from this point on, Production works against the new definition. Superseded is set automatically when a later ECO replaces this one. The current status is the single source of truth for "is this change in effect yet".

## Authors, reviewers, and approvers

Each ECO has one author and a list of required reviewers determined by the scope of the change. A BOM change typically requires an engineering reviewer, a manufacturing reviewer, and a quality reviewer. A document-only change may require only engineering. Reviewers approve or reject individually; any rejection sends the ECO back to Draft. The current reviewer assignments and their decisions are visible on the detail page.

## Scope and affected items

The ECO detail page lists every item the change affects — specific routings, BOMs, documents, or stations. This scope matters because Production uses the scope to determine which work orders need to be reviewed against the new definition. When an ECO is released, the scope is snapshotted so later audits show exactly what was changed at release time, even if the affected items change afterward.

## Deviations vs ECOs

If you need a short-term exception rather than a permanent change, use a Deviation at `/engineering/deviations` instead of an ECO. Deviations are scoped to a specific work order or date range and expire automatically. Use an ECO when the change is intended to be the new normal; use a Deviation when the change is a one-off.

## Audit trail

Every ECO carries a complete audit log of status transitions, reviewer decisions, and scope edits. The log is visible on the detail page and is write-once — entries cannot be edited or deleted. This makes ECOs usable as evidence in audits, ISO reviews, and regulatory reporting without requiring a separate compliance system.
