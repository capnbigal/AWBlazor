---
title: About the OEE Rollup
summary: The live OEE view at /mes/oee — how availability, performance, and quality combine into a single shift-level number.
tags: [production, oee, mes]
category: entity-guide
author: AWBlazor
---

Overall Equipment Effectiveness, or OEE, is the most widely used single-number summary of how well a production line is running. The OEE rollup at `/mes/oee` shows the live, current-shift OEE for every station, rolled up by product, by shift, and by line. It is distinct from OEE snapshots at `/performance/oee`, which hold historical daily and weekly values for trend analysis — the rollup is now, the snapshots are then.

## The three components

OEE is the product of three independent factors. Availability is the ratio of actual running time to planned running time — if the line was planned to run 8 hours and downtime events stole 1 hour, availability is 87.5%. Performance is the ratio of actual throughput to the ideal cycle time — if the routing says 60 units per hour and the line produced 50, performance is 83%. Quality is the ratio of good units to total units — 95 good out of 100 produced is 95%. OEE is the product: 0.875 × 0.83 × 0.95 = 69%.

## Why the product, not the average

Multiplying rather than averaging is what makes OEE useful. An average would let a 100% score in one factor compensate for a 50% score in another, which hides the worst problem. The product forces every factor to matter — you cannot have a high OEE without all three being reasonable. A line at 100% availability with 50% quality is producing a lot of scrap fast; OEE correctly reports that as mediocre rather than celebrating the availability.

## Reading the rollup page

The `/mes/oee` page shows current OEE per station in a dashboard layout. Each station card shows the composite OEE number plus the three components below it, so you can see which factor is pulling the number down. Colour coding flags stations whose OEE has dropped below a configurable threshold — typically 70% for the warning tier and 50% for the critical tier. Clicking any station navigates to `/mes/runs` filtered to the active run on that station, letting you investigate what is happening right now.

## Sources of data

The rollup computes its numbers from currently-Active and recently-Completed runs at `/mes/runs`, downtime events at `/mes/downtime`, and the routing cycle times from `/engineering/routings`. If operators are not ending runs or opening downtime events correctly, the rollup becomes misleading — the math is right but the inputs are wrong. Data discipline on the shop floor directly affects whether the OEE number means anything.

## Rollup vs snapshots

At the end of each day or shift, a batch job computes the period's final OEE and stores it as a snapshot at `/performance/oee`. The snapshots are what you chart to see OEE trending over weeks and months. The rollup is what you look at during the shift to decide whether to intervene. Both are built from the same underlying run and downtime data.
