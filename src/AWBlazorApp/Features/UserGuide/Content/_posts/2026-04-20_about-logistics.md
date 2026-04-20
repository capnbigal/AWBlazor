---
title: About Logistics
summary: The Logistics module — goods receipts, shipments, and stock transfers.
tags: [logistics]
category: entity-guide
author: AWBlazor
---

The Logistics module at `/logistics` handles inventory movement across the plant's boundaries — goods coming in from suppliers, products going out to customers, and transfers between internal locations. All three page types produce inventory transactions at `/inventory/transactions` when they post, so Logistics is the "front door" for most of the inventory movement that eventually shows up in the balances. The module is intentionally narrower than Inventory itself — Logistics deals with specific movement events, while Inventory provides the overall item-balance view.

## Logistics summary

The Logistics summary page at `/logistics` is the landing view. It shows counts of open goods receipts, in-transit shipments, pending stock transfers, and items sitting in receiving or staging for more than the configured dwell-time threshold. Each count is a filtered link into the detail pages. A warehouse supervisor opens this page to see whether anything is stuck.

## Goods receipts

Goods receipts at `/logistics/receipts` record material arriving from suppliers. Each receipt has a receipt number, a vendor, a purchase order reference, a received date, a receiving location, a list of lines with item and quantity, and inspection state. Receipt lines can reference expected purchase-order lines or be unexpected overages; receipts that do not match a PO open an exception that requires supervisor review before the inventory transaction posts.

## Shipments

Shipments at `/logistics/shipments` record material leaving the plant for customers. Each shipment has a shipment number, a customer, a sales-order reference, a ship date, a shipping location, a carrier, and a list of lines. Shipments produce outbound inventory transactions when they post, decrementing stock from the shipping location. Shipment tracking numbers and carrier info are captured so they can be surfaced to customer-facing interfaces through the Inventory outbox.

## Stock transfers

Stock transfers at `/logistics/transfers` record internal movements between locations — moving product from receiving into warehouse storage, from warehouse to production staging, from production to shipping. Each transfer has a from-location, a to-location, a list of items with quantities, a status (Planned, In transit, Completed, Cancelled), and a reason. Transfers produce two inventory transactions — one decrementing the from-location, one incrementing the to-location — with a transfer reference linking them so they are always treated as a matched pair.

## Two-stage transfers

For transfers between physically distant locations, a transfer can be posted in two stages. The Departure stage decrements the from-location and moves the quantity into an In-transit virtual location. The Arrival stage moves it from In-transit into the to-location. This prevents the quantity from disappearing from the books during physical movement while correctly reflecting that it is neither at the source nor the destination. Two-stage transfers require both a departure and an arrival post before the transfer is Completed.

## Cross-module linkage

Logistics pages link heavily into Purchasing (for receipt-to-PO matching), Sales (for shipment-to-order matching), and Inventory (for the transactions and balances that result). The Logistics summary serves as the daily-operations view; the Inventory status reports serve as the aggregate view; transactions are the common ledger underneath.
