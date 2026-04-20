---
title: About Assets and Cost Centers
summary: How asset records and cost center records work together to describe what runs on the floor and who pays for it.
tags: [enterprise, assets, finance]
category: entity-guide
author: AWBlazor
---

The Assets page at `/enterprise/assets` and the Cost centers page at `/enterprise/cost-centers` are two sides of the same coin. Assets describe the physical equipment on the floor, cost centers describe the financial buckets that equipment rolls up to. Every maintenance work order, every production run, every OEE rollup eventually lands against either an asset or a cost center, so the data in these two pages underpins almost every operational report.

## The Asset record

An Asset represents a specific machine, tool, or fixture. Each row carries a unique ID, a description, a type (press, lathe, conveyor, etc.), a current station assignment, and an audit trail at `/enterprise/assets/history`. Assets link to maintenance-side data through the Asset profile at `/maintenance/asset-profiles`, which holds PM schedules, meter readings, and service history. Keeping the asset catalog clean — no duplicates, correct types, current station assignments — pays back every time you pull a maintenance report or investigate a downtime event.

## Asset history

Every change to an asset is recorded. The history view at `/enterprise/assets/history/{id}` shows who changed what, when, and why. This matters when troubleshooting cross-module issues — if a maintenance work order references an asset that has since been reassigned to a different station, the history view lets you see when the move happened and whether the work order was written before or after.

## The Cost center record

A Cost center groups operational activity into a financial bucket. Unlike an org unit, a cost center does not have to mirror the organizational tree; a single cost center can aggregate activity from several org units. Each cost center row has an identifier, a display name, a parent cost center (for roll-ups), and an active flag. Retired cost centers stay in the table for historical reporting but no longer appear in dropdowns on operational pages.

## Linking assets to cost centers

Assets carry a cost center reference so that any financial roll-up (Performance reports, maintenance spend, OEE financial impact) can attribute cost correctly. When you move an asset between stations, the cost center does not automatically follow — if the new location belongs to a different cost center, you edit the asset's cost center field separately. The audit trail shows both the station move and the cost center change as distinct events.

## Practical setup

When onboarding a new site, build the cost centers first, then the stations, then the assets — this lets you set the correct linkages as you create each asset row rather than patching them afterward. Use the Reference data subgroup under Enterprise to verify that underlying person and contact data is clean before wiring any of it to assets.
