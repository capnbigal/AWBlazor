---
title: About Inventory Items and Locations
summary: The two catalog pages that define what can exist in inventory and where it can live.
tags: [inventory, items, locations]
category: entity-guide
author: AWBlazor
---

Items at `/inventory/items` and Locations at `/inventory/locations` are the two master-data pages the rest of the Inventory module depends on. Every transaction, every balance row, every adjustment references one item and one or two locations. Getting the items and locations right — clean names, correct categories, no duplicates — is the foundation that makes the rest of the module reliable. Changes here propagate everywhere.

## The Items catalog

An Item at `/inventory/items` is a thing that can exist in inventory. Each item has a unique item number, a description, a category, a unit of measure, a product reference (linking to the AdventureWorks Products catalog at `/aw/products`), a tracking method (lot, serial, or bulk), a reorder point, a reorder quantity, a preferred vendor, and an active flag. The item record is master data — it does not hold quantity; quantity lives on balances.

## Tracking methods

The tracking method determines how individual units are distinguished. Bulk items are counted only by quantity — 1000 washers is 1000 washers, no serial numbers. Lot-tracked items group units into lots for traceability — a lot number ties a receipt to the downstream issues so you can trace which specific batch went where. Serial-tracked items identify every unit individually — critical for high-value or safety-sensitive items. The tracking method affects how transactions on the item are structured and is hard to change once the item has history, so set it correctly at creation.

## The Locations catalog

A Location at `/inventory/locations` is a place where inventory can sit. Locations can be hierarchical — a warehouse contains rooms, rooms contain racks, racks contain bins. The hierarchy is useful for aggregating balances at different granularities; you can ask "how much of item X is in warehouse A" as well as "how much is in bin 3B-07". Each location has a unique identifier, a description, a type (Warehouse, Staging, Production, Quarantine, Shipping, Receiving), a parent reference, and an active flag.

## Location types and rules

The location type affects what transactions are allowed. Quarantine locations cannot be issued to production — stock there is held for quality review. Staging locations are meant for short-term consolidation and trigger alerts when stock sits there too long. Production locations are the consumption points during runs. Shipping and Receiving are the integration points where inventory enters and leaves the plant. The type is a soft rule enforced through warnings rather than hard blocks so you can override with justification.

## Retirement without deletion

Items and locations with any history cannot be deleted — doing so would orphan transaction records. Instead they are retired by setting the active flag to false. Retired items and locations disappear from dropdowns on operational pages but remain on the catalog pages with an Active filter toggle. Historical transactions referencing them still resolve correctly. This is how you clean up the catalog without breaking the audit trail.
