---
title: About Downtime Events and Work Instructions
summary: How production downtime is captured and categorized, and how operators find the documents that describe the work.
tags: [production, downtime, instructions]
category: entity-guide
author: AWBlazor
---

Two of the Production pages support everything the operator does during a run. Downtime events at `/mes/downtime` capture every minute the line is not producing, with a reason code that tells you why. Work instructions at `/mes/instructions` are the step-by-step documents operators follow while the line is running. Both pages are designed to be touched briefly and often — a few taps to open a downtime event, a glance at the current work instruction to check the next step.

## The downtime event lifecycle

A downtime event opens whenever a run goes from Active to Paused. The event captures the run, the asset, the station, the start timestamp, and a reason code. While the event is open, the affected run is accumulating non-producing time. When the run resumes, the event closes and the end timestamp is recorded. Duration is derived from start and end. Every downtime event is visible at `/mes/downtime` with filters for asset, station, date range, and reason.

## Reason codes

Reason codes at `/mes/downtime/reasons` are the categories operators choose from when opening a downtime event. Typical top-level categories are Mechanical, Electrical, Setup, Material shortage, Quality hold, Operator break, and Planned maintenance. Each category can have subcategories so reports can drill from "Mechanical" to "Hydraulic failure" or "Pneumatic failure". The reason-code catalog is maintained centrally; operators only choose from the catalog, so reports aggregate correctly.

## Using downtime data

Downtime events are the raw material for availability calculations in OEE and for improvement targeting. A Pareto chart of downtime minutes by reason code usually identifies the one or two categories that account for most of the losses; those become the targets for corrective and preventive action. Downtime events also link to maintenance work orders when the downtime was caused by an equipment issue, so the maintenance team can see which downtime events they resolved and how long the repair took (MTTR).

## Work instructions at /mes/instructions

A work instruction is the operator's copy of the engineering document that describes how to perform a routing operation. It references the underlying document at `/engineering/documents` but is presented in a shop-floor-friendly format — large images, numbered steps, inline tool and gauge callouts, safety warnings highlighted. Each routing operation at `/engineering/routings` links to the work instruction that applies, so when an operator is working on a run, the instruction for the current step is one click away.

## Revision control for instructions

Work instructions are revisioned like other engineering documents. The run snapshots the instruction revision in effect at run-start, so later instruction edits do not change what the historical run actually did. When an ECO at `/engineering/ecos` releases a new instruction revision, Production operators see the new version on the next run that starts after the release date; runs already in progress continue with the revision they started under.
