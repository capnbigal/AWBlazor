---
title: About Production Runs
summary: What a production run records, how it links work orders to actual output, and why the run is the data unit for most MES reports.
tags: [production, mes, runs]
category: entity-guide
author: AWBlazor
---

A production run at `/mes/runs` is the atomic unit of manufacturing execution in AWBlazor. Every other MES data point — downtime, scrap, OEE, operator labour — attaches to a run. If you can tie an event to a specific run, you can report on it; if you cannot, it floats outside the analytics and effectively does not exist. Getting operators into the habit of starting and ending runs correctly is the single most important operational discipline for getting good production data out of the system.

## What a run records

Each run carries a run number, the work order it is executing against, the routing revision, the station, the operator, the planned quantity, the actual good quantity, the scrap quantity, the start timestamp, and the end timestamp. The derived fields — duration, throughput rate, yield percentage — are computed on the fly from these. Every run references a specific revision of the routing and BOM that were current when the run started, so historical runs are always interpretable against the definitions that were in effect at the time.

## The run lifecycle

A run is Scheduled once created but not yet started. It becomes Active when the operator signs on and hits start. It goes into Paused when a downtime event opens against it, and returns to Active when downtime closes. It becomes Completed when the operator ends the run and records the final counts. Completed runs can be Voided only by a supervisor, which flags the data as invalid without deleting it — the audit trail stays intact.

## Downtime against runs

While a run is Active, it can go into downtime. A downtime event at `/mes/downtime` opens, the operator selects a reason code, and the event accumulates time until the run resumes. A run with no downtime is rare; even the best lines have setup time and breaks. The run's total downtime minutes feed directly into availability — the A in OEE — and the breakdown by reason code feeds into improvement initiatives.

## Scrap and yield

Scrap quantity recorded on the run is what drives yield. Yield is good quantity divided by total quantity (good plus scrap). A run with 100 planned, 95 good, and 5 scrap has 95% yield. Scrap categories — attached to each scrap entry, not the run as a whole — identify what kind of defect caused the loss. High-yield runs with low scrap are the target; chronic low-yield runs point at process issues that belong in a CAPA case.

## Why the run matters for reporting

Almost every report in the Production analytics and Performance modules starts from run data. Throughput, OEE, yield, scrap reasons, operator efficiency, and cost per unit are all computed from runs aggregated in different ways. The analytics are only as accurate as the runs; this is why training operators to start and end runs cleanly is worth the time it takes.
