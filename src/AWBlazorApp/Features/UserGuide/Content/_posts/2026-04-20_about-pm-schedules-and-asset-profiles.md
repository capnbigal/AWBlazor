---
title: About PM Schedules and Asset Profiles
summary: How preventive maintenance is planned ahead of time, and how each asset's profile drives that plan.
tags: [maintenance, pm-schedules, assets]
category: entity-guide
author: AWBlazor
---

Preventive maintenance is how you stop breakdowns before they happen. PM schedules at `/maintenance/pm-schedules` define the recurring work that needs to be done, and asset profiles at `/maintenance/asset-profiles` describe each machine in enough detail for the scheduler to know when and how. The two pages work together — a PM schedule without an asset profile has nothing to schedule against, and an asset profile without PMs is just a description.

## What a PM schedule holds

A PM schedule at `/maintenance/pm-schedules/{id}` describes one recurring maintenance activity. It specifies the asset or asset class it applies to, the task to be performed (often with a link to a work-instruction document in Engineering), the expected duration, the parts typically needed, and the trigger rule. The trigger is the key: calendar-based triggers fire every N days or at specific dates, while usage-based triggers fire when a meter reading at `/maintenance/meter-readings` crosses a threshold.

## How a PM schedule generates work

When a PM schedule's trigger fires, it creates a new maintenance work order at `/maintenance/work-orders` with the task, parts, and duration pre-populated from the schedule. The work order enters the New status and goes into the scheduler's planning queue. From the maintenance supervisor's perspective, the PM workflow is "my PM schedules generate my work orders; I plan my work orders; my technicians complete them". The work orders carry the PM schedule reference so you can always see how a given day's work traces back to the scheduled plan.

## Asset profiles

An asset profile at `/maintenance/asset-profiles` is the maintenance-specific view of an asset. It pulls the basic asset data from `/enterprise/assets` and adds maintenance-specific fields: expected PM frequency, service history, known failure modes, typical spare parts used, recommended technician skill level, and warranty information. The profile is what a planner consults when setting up new PM schedules for the asset.

## Service history

Every work order completed against an asset is recorded in the asset profile's service history section. The history shows date, work type (reactive, scheduled, inspection), duration, parts used, cost, and technician. When investigating whether an asset is trending toward end-of-life, the service history is the primary evidence — rising frequency of reactive work and rising cost per hour are the signals of a machine that needs replacement rather than another repair.

## Tuning the schedule

PM schedules are not "set and forget". Too-frequent PMs waste labour; too-infrequent PMs let failures through. The Maintenance metrics page at `/performance/maintenance-metrics` shows PM compliance (did the PM actually happen on time), PM value (did the PM prevent a failure that would otherwise have occurred), and reactive-vs-scheduled ratio. Use those metrics quarterly to adjust PM frequencies on schedules where the data says you are either over- or under-servicing.
