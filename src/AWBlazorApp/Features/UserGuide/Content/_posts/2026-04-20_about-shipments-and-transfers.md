---
title: About Shipments and Stock Transfers
summary: How shipments record outbound movement to customers, and how stock transfers record movement between internal locations.
tags: [logistics, shipments, transfers]
category: entity-guide
author: AWBlazor
---

Shipments at `/logistics/shipments` and stock transfers at `/logistics/transfers` are the two Logistics pages that produce outbound movement. Shipments move material out of the plant to a customer. Stock transfers move material between internal locations. Both generate inventory transactions when they post, and both have detail views — `/logistics/shipments/{id}` and `/logistics/transfers/{id}` — where the line items and status are managed.

## The shipment record

A shipment at `/logistics/shipments` has a shipment number, a customer reference, a sales-order reference, a shipping location, a carrier, a tracking number, a ship date, and a list of lines. Each line identifies the item, the lot or serial if tracked, the quantity, and any special handling notes. Shipments carry a status through Planned, Picking, Packed, Shipped, and Delivered; the state transitions reflect the physical process and each one may or may not produce an inventory movement depending on whether the material has physically left the location yet.

## Picking and packing

Before a shipment can post, the material has to be picked from the shipping location's source bins. The picking step generates internal stock transfers — material moves from storage bins to a staging area. Packing consolidates picked material into cartons or pallets. These intermediate steps are visible on the shipment detail view and help explain why a shipment might show Picked but not yet Shipped — physical packing is in progress.

## When the inventory transaction posts

The inventory transaction for a shipment posts when the status moves to Shipped — the moment the material physically leaves. Before Shipped, the material is still technically on-hand at the shipping location (or its staging sub-location). After Shipped, the quantity is removed from the plant's inventory and either held in an in-transit bucket (for customers that want visibility into in-transit stock) or removed entirely (for customers where title transfers at ship time).

## The stock transfer record

A stock transfer at `/logistics/transfers` records internal movement. Each transfer has a from-location, a to-location, a list of lines, a status (Planned, In transit, Completed, Cancelled), and a reason code. Transfers between adjacent locations (same warehouse, different rack) are typically single-step — the transfer posts and both locations update simultaneously. Transfers between physically distant locations (across sites) are typically two-step.

## Two-step transfers

A two-step transfer posts a Departure event first (decrementing the from-location, moving quantity into an In-transit virtual location) and a separate Arrival event later (moving from In-transit into the to-location). The In-transit bucket is important for accurate accounting — the material is neither at the source nor the destination during shipping, and pretending it is in either place gives misleading balances. Two-step transfers require both posts to complete; if the Arrival never happens, the In-transit bucket accumulates and appears as an exception on the Logistics summary.
