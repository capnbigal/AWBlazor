---
title: About Quality
summary: The Quality module — inspection plans, inspections, non-conformances, and CAPA cases.
tags: [quality]
category: entity-guide
author: AWBlazor
---

The Quality module at `/quality` manages the evidence that what you built meets the standard you promised. Inspection plans define what to check. Inspections record the results. Non-conformances capture the things that failed a check and what happened about them. CAPA cases formalize corrective and preventive action when a non-conformance points to a systemic issue. Together these four pages trace the full path from "this is what we measure" to "this is how we are making sure it does not happen again".

## Quality summary

The Quality summary page at `/quality` is the landing view. It shows the number of open inspections, new non-conformances this week, and CAPA cases approaching their due date. The counts link directly to filtered versions of the detail pages. A quality manager opens this page first thing in the morning to see whether anything needs their attention today.

## Inspection plans

An Inspection plan at `/quality/inspection-plans` is a reusable template describing what gets checked on a product, a station, or a routing operation. Each plan lists the characteristics to measure, the acceptance limits, the sampling rule (100%, AQL, first-piece only), and the equipment required. Plans are versioned and revisioned through the ECO process at `/engineering/ecos` — so a plan change is traced through the same review and approval workflow as a BOM or routing change.

## Inspections

An Inspection at `/quality/inspections` is a concrete event — somebody applied an inspection plan to a specific work order, run, or lot, and recorded the results. Inspections reference the plan revision in effect at the time, so the results are always interpretable against the standard that was current when the check was performed. The Inspections page shows every inspection with its result (pass, fail, needs review) and links to the underlying plan, the work order, and any non-conformance that was generated.

## Non-conformances

A Non-conformance, or NCR, at `/quality/ncrs` is a record that something did not meet its standard. Every NCR has a root cause category, a disposition (scrap, rework, use-as-is, return to supplier), a cost impact, and an action owner. NCRs link back to the inspection that discovered them and forward to any CAPA case that was raised because of them. The NCR board is the work-in-progress view of quality issues; clearing the board each day is a routine operational cadence.

## CAPA cases

CAPA stands for Corrective And Preventive Action. A CAPA case at `/quality/capa` is opened when an NCR or a pattern of NCRs points to a systemic issue that needs more than a one-time fix. Each case carries a root-cause investigation, a corrective action (fixing the immediate problem), a preventive action (making sure it does not recur), and an effectiveness check that closes the loop after the change has been in place long enough to judge. CAPA cases are the vehicle for continuous improvement and are the artifact that ISO auditors ask to see when evaluating the maturity of a quality system.
