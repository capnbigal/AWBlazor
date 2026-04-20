---
title: About Inspections
summary: How individual quality inspections are recorded, linked to plans, and rolled up into results.
tags: [quality, inspections]
category: entity-guide
author: AWBlazor
---

An inspection at `/quality/inspections` is the concrete event where somebody applied an inspection plan to a real thing — a work order, a lot, a first piece — and recorded the outcome. Inspections are the building block of quality data in AWBlazor; everything else in the Quality module either generates an inspection, reports against inspections, or reacts to the non-conformances inspections produce.

## What an inspection records

Each inspection row captures the inspection plan and plan revision applied, the target (work order, run, lot, or sample), the inspector, the timestamp, the sample size, and the measured values for each characteristic in the plan. Measured values are stored with their units and compared against the plan's acceptance limits automatically. The inspection's overall result — Pass, Fail, or Needs review — is derived from the characteristic results using the plan's decision rule.

## Starting an inspection

Inspections can start from several places in the app. From a production run's detail page, an operator can start the inspection plan that applies to their routing operation. From an inspection plan's detail page, a quality engineer can start an ad-hoc inspection of a specific lot. From a non-conformance detail page, a re-inspection can be started against the same plan after rework to confirm the issue is resolved. All paths land in the same inspection form with the plan pre-populated.

## Viewing inspection results

The `/quality/inspections` list view shows every inspection with its target, plan, result, and timestamp. Filters let you scope to a specific plan, station, product, date range, or result category. Clicking an inspection opens the detail view, which shows every characteristic's measured value, limit, and pass/fail state, plus the narrative notes the inspector added. If the inspection failed and generated a non-conformance, a link carries you directly to the NCR.

## Sampling strategies

Inspection plans can specify 100% inspection (every unit), first-piece inspection (the first unit of a run, to validate setup), AQL-based sampling (a statistically chosen sample per lot), or time-based sampling (one check per hour). The inspection record shows which sampling strategy was in effect and how many units the sample represents, so downstream reports can compute defect rates and process capability correctly.

## Inspections and audit

Because every inspection links to its plan revision, the inspector, and the target, inspections form the audit trail for quality compliance. When an auditor asks "how do you know this lot meets the standard", the answer is a specific inspection record with plan, revision, sample, values, and disposition — all retrievable in one click from the `/quality/inspections` detail page. This makes Quality's records directly usable for ISO and customer audits without needing a separate compliance export.
