---
title: About Spare Parts and Meter Readings
summary: Parts inventory for maintenance, and the usage data that drives usage-based PMs.
tags: [maintenance, spare-parts, meters]
category: entity-guide
author: AWBlazor
---

Spare parts at `/maintenance/spare-parts` and meter readings at `/maintenance/meter-readings` are the two supporting data pages that make maintenance planning work. Spare parts holds the inventory of maintenance-specific parts — bearings, belts, filters, seals — that are consumed when work orders close. Meter readings holds the usage data — cycle counts, run hours, odometer readings — that triggers usage-based PMs. Both are small pages but both are load-bearing.

## The spare-parts catalog

Each spare part row has a part number, description, current on-hand quantity, reorder point, reorder quantity, preferred vendor, and unit cost. Reserved quantity tracks parts allocated to planned work orders that have not yet consumed them. Available-to-plan is on-hand minus reserved — this is what the planner sees when creating a new work order and wondering whether the parts are on the shelf. Parts are typed (consumable, repairable, critical, insurance) so reports can roll up by category.

## Part consumption on work-order closure

When a maintenance work order at `/maintenance/work-orders` moves to Completed, the parts it consumed decrement the on-hand quantity automatically. The work order carries the actual quantities used, which may differ from the planned quantities. If actual exceeds planned, the difference is logged so the planner can adjust the standard parts list for similar jobs. If actual is less than planned, the reserved but unused quantity is returned to available stock.

## Reorder signals

When on-hand minus reserved drops below the reorder point, the part appears on the Spare parts page's Low stock filter. The maintenance supervisor reviews this list daily and creates purchase orders through the Purchasing module. Parts flagged as Critical have a separate high-priority indicator because their stock-out would halt the ability to respond to an unplanned failure on a specific asset.

## Meter readings

A meter reading at `/maintenance/meter-readings` is a recorded measurement of an asset's usage — typically cycle count, running hours, or kilometers. Readings are entered manually by technicians or can be imported from controls systems that expose the value. Each reading has a timestamp, the reading value, and the asset reference. The asset's profile at `/maintenance/asset-profiles` shows the current reading and the reading trend over time.

## Meter-driven PMs

Usage-based PM schedules at `/maintenance/pm-schedules` reference a meter and a threshold. When a meter reading crosses the threshold (the reading minus the previous PM's reading reaches the trigger quantity), the PM schedule fires and generates a maintenance work order. This is how a "replace oil every 1000 hours" PM actually gets triggered — somebody is recording hours, the scheduler is watching, and when the delta hits 1000, the work order appears. Without the meter data, usage-based PMs quietly become calendar-based on the longest estimate.
