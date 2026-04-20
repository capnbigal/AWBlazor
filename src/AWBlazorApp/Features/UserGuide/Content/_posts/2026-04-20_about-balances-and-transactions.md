---
title: About Inventory Balances and Transactions
summary: How balances track current on-hand per item-location and how the transaction log explains every movement.
tags: [inventory, balances, transactions]
category: entity-guide
author: AWBlazor
---

Balances and transactions are the two core data sets of the Inventory module. Balances at `/inventory/balances` answer "how much of item X is at location Y right now?" Transactions at `/inventory/transactions` answer "how did it get that way?" Every transaction moves some quantity between states — into stock, out of stock, from one location to another — and the balance for the affected item-location pair updates as a result. Balances are the aggregate; transactions are the source.

## What a balance row shows

Each row on the Balances grid at `/inventory/balances` is the intersection of one item and one location. The row carries the on-hand quantity, the reserved quantity (allocated to planned consumption), the available quantity (on-hand minus reserved), the last-transaction timestamp, and a link to the item and location master records. Filters let you narrow to specific items, specific locations, low stock, or zero stock — the common operational filters.

## What a transaction row shows

Each row on the Transactions page at `/inventory/transactions` is a movement. It has a timestamp, an item, a source location, a destination location, a quantity, a transaction type (Receipt, Issue, Transfer, Adjustment, Return), a reason code, a reference to the source document (work order, purchase order, sales order, adjustment), and the user who posted it. Transactions are write-once after posting; corrections are made by posting an offsetting transaction rather than editing the original, so the audit trail never loses information.

## How balances are derived

The Balances page is not a separate stored table — it is a derived view computed from transactions. The current on-hand for an item-location is the sum of all transaction quantities for that pair (positive for inbound, negative for outbound). This means balances are always consistent with transactions by construction: if you can see the transaction, it has already been applied to the balance. There is no risk of balances drifting from transactions because balances are not stored separately.

## Reserved quantity

Reserved quantity tracks allocations that have been committed but not yet consumed. A planned production run reserves its input materials when the run is released; the reservation decreases available without decreasing on-hand. When the run consumes the materials, a transaction is posted that decreases on-hand and decreases the reservation correspondingly. If the run is cancelled, the reservation is released without a consumption transaction. Reserved quantity prevents the same stock being planned for two jobs.

## Drilling from balance to transactions

On any Balances row, clicking the quantity link navigates to `/inventory/transactions` filtered to that item-location pair. This is the typical investigation path: you see a balance that looks wrong, you click through to the transaction history, and the recent transactions usually explain what happened. If the transactions do not explain it, somebody posted a direct database update outside the system — which should be followed up on because it breaks the audit trail.
