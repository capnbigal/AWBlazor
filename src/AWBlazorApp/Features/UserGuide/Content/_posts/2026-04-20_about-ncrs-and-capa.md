---
title: About Non-Conformances and CAPA Cases
summary: How the NCR workflow and CAPA workflow together handle both one-off quality issues and systemic improvement.
tags: [quality, ncr, capa]
category: entity-guide
author: AWBlazor
---

Non-conformances and CAPA cases are the two-tier response to quality problems in AWBlazor. A non-conformance, or NCR, at `/quality/ncrs` handles the immediate question — what do we do with this specific unit that failed. A CAPA case at `/quality/capa` handles the longer question — what are we going to change so this stops happening. Most non-conformances never escalate to a CAPA; a few do, and those are usually the ones that actually improve the process.

## The NCR workflow

When an inspection at `/quality/inspections` produces a failing result, the quality engineer decides whether to open an NCR. An NCR captures the affected unit or lot, the characteristic that failed, the measured value, the root cause category, and a disposition — Scrap, Rework, Use-as-is, or Return-to-supplier. Each NCR has an owner who is responsible for driving it to closure, and a cost impact in currency so the financial effect is visible.

## Disposition in practice

Scrap disposes of the material and closes the loop. Rework creates a new routing operation that corrects the defect, typically with a re-inspection at the end. Use-as-is means the material does not meet spec but is acceptable for this use — it requires a formal concession, often with customer notification. Return-to-supplier ships the bad material back and triggers a supplier corrective action request. The NCR detail page shows the current disposition and the history of how it got there.

## When to open a CAPA

A CAPA case is opened when the NCR points to a systemic issue rather than a one-off problem. Triggers include three or more similar NCRs in a rolling window, a high-cost NCR above a defined threshold, a customer complaint, an audit finding, or an NCR that the quality engineer flags for escalation on judgment. The `/quality/capa` page is where these cases live.

## CAPA structure

A CAPA has four parts. The investigation documents the root cause using techniques like the 5 Whys or fishbone analysis. The corrective action describes the immediate fix applied to stop the issue in its tracks. The preventive action describes the longer-term change — a process update, a training addition, a design change — intended to keep the issue from recurring. The effectiveness check is a scheduled review, typically 30-90 days after the preventive action takes effect, to verify the issue has actually stopped.

## Closing the loop

A CAPA is only closed after the effectiveness check confirms the problem is resolved. If it is not, the case stays open and the preventive action is revised. This is where CAPA differs from simple issue tracking: the workflow forces a return visit to check the fix worked. That discipline is why CAPAs are the artifact that external auditors look for when assessing whether a quality system is mature.
