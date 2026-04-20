---
title: About Inventory
summary: The Inventory module — product explorer, balances, transactions, adjustments, items, locations, outbox, queue, and reports.
tags: [inventory]
category: entity-guide
author: AWBlazor
---

The Inventory module at `/inventory` is where the physical material state of the plant is tracked. The product explorer is the landing view. Below it are pages for real-time balances, the transaction history that explains how those balances got there, adjustments for when the book and the reality disagree, the catalogs of items and locations, reports for status summaries, and the outbox and inbound queue that handle integration with other systems. The AWBlazor Inventory module is designed around the reality that inventory data is only as good as the transactions that produce it.

## Product explorer

The product explorer at `/inventory` is the landing view. It shows items grouped by category with on-hand quantity per item, quick filters for low-stock or overstock, and direct links to the underlying item and balance detail pages. The page is optimized for answering "do we have X" quickly — search by name or item number, see current stock, click through to detail if you need more.

## Balances and transactions

Balances at `/inventory/balances` is the on-hand view per item per location. Every item-location pair that has ever had a movement appears on this page with its current quantity, last-transaction timestamp, and links to the item master and location. Transactions at `/inventory/transactions` is the complete audit trail of every movement — receipts, issues, transfers, adjustments — with quantity, from/to location, reason code, and reference to the source document. Balances are derived from transactions; if balances and transactions ever disagree, the transactions are authoritative.

## Adjustments

Adjustments at `/inventory/adjustments` is where the book is corrected when the physical count does not match. An adjustment is itself a transaction, so the audit trail preserves both the before-state and the reason. Adjustments require a reason code (Physical count variance, Damage, Theft, System error, Other) and typically require supervisor approval above a configurable dollar threshold.

## Items, locations, transaction types

The three catalog pages support the rest. Items at `/inventory/items` is the item master — what can exist in inventory. Locations at `/inventory/locations` is the location master — where inventory can be. Transaction types at `/inventory/transaction-types` defines the allowed reason codes and their accounting behaviour (some increase stock, some decrease, some move stock between locations). Changes to these catalogs are rare and carry an audit trail.

## Outbox and inbound queue

Outbox at `/inventory/outbox` and Inbound queue at `/inventory/queue` are the integration endpoints. Outbox holds outbound messages to external systems — stock updates going to an ERP, ship notifications going to a WMS. Inbound queue holds incoming messages waiting to be processed — receipts from suppliers, adjustments from barcode scanners. Both pages support retry and manual reprocess for messages that fail the first time.

## Reference data

Below the workflow pages sits the Reference data subgroup with the full AdventureWorks product catalog — products, product models, categories, subcategories, descriptions, photos, inventories, list and cost price histories, and storage locations. The curated Inventory workflow pages above give you the operational view; the reference data is where you go for master-data maintenance and catalog audits.
