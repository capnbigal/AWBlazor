---
title: About Processes
summary: The Processes module for tracking cross-functional workflows outside of routings, BOMs, and work orders.
tags: [processes, planning]
category: entity-guide
author: AWBlazor
---

Not everything that happens in a manufacturing business fits neatly into a routing or a work order. New-product introductions, customer-specific certifications, plant audits, capital projects, major supplier qualifications — these are cross-functional workflows that span engineering, quality, production, and often finance. The Processes module at `/processes` is where these broader workflows are tracked. It is deliberately more flexible than the strict lifecycles of ECOs or CAPA cases, because the shapes of these workflows vary.

## What a process is

A process at `/processes` is a named workflow with a defined set of phases and participants. Examples include "New product introduction", "Supplier qualification", "ISO 9001 recertification", "Line expansion project". Each process instance has a unique identifier, a type (from a configured catalog), a current phase, an owner, a list of stakeholders, a start date, and a target completion date. Unlike a work order — which has a strict lifecycle — a process can have custom phases defined per type, so NPI and supplier qualification can have different flow shapes even though both live on the same page.

## Phases and milestones

A process phase is a stage of the workflow with a defined completion criterion. Phases have entry requirements (what must be true to enter) and exit criteria (what must be true to leave). Completion of a phase is a milestone; the milestone date is recorded so the process's timeline can be reconstructed. Some phases run sequentially (one must complete before the next starts), others can run in parallel (design review and prototype build can both be in progress simultaneously).

## The timeline view

Each process instance has a timeline view at `/processes/{id}/timeline` showing the phases as bars on a horizontal time axis. Completed phases appear as solid bars with their actual duration; in-progress phases appear as partial bars; future phases appear as planned bars at their target dates. The view makes schedule slip and critical-path issues visible at a glance — a late phase pushing all subsequent phases forward shows up as a visible gap.

## Stakeholders and notifications

A process carries a list of stakeholders with defined roles — Sponsor, Owner, Reviewer, Contributor, Informed. Role assignments drive the notification rules at `/notifications`; the Owner gets notified at every phase transition, Reviewers get notified when their review is required, and Informed stakeholders get periodic status updates without interactive notifications.

## Process analytics

Process analytics at `/analytics/processes` aggregates process data across instances — average time per phase by process type, stall rate (processes stuck in a phase beyond the expected duration), throughput (processes completed per quarter), and success rate (processes that completed versus those cancelled). These metrics help identify which process types have structural issues and which are running smoothly.
