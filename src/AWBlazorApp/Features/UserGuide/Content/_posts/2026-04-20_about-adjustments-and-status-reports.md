---
title: About Inventory Adjustments and Status Reports
summary: How adjustments correct the book when reality differs, and how the status reports summarize inventory health.
tags: [inventory, adjustments, reports]
category: entity-guide
author: AWBlazor
---

Inventory adjustments at `/inventory/adjustments` and the status reports at `/inventory/reports` are the two Inventory pages that focus on keeping the data honest. Adjustments is where the book gets corrected when a physical count finds something different from what the system says. Status reports is the aggregated view of inventory health — low stock, overstock, ageing, and value summaries. Both exist because raw transactions are too detailed to manage by directly, and balances are too narrow.

## The adjustment workflow

An adjustment at `/inventory/adjustments` is a transaction that changes the on-hand quantity of an item-location without a corresponding receipt or issue. Each adjustment has an item, a location, a from-quantity (what the book said), a to-quantity (what the count found), the delta, a reason code, a comment, and the user who posted it. Adjustments are real transactions, not edits — they appear in the `/inventory/transactions` log like every other movement, keeping the audit trail continuous.

## Reason codes and approval

Adjustments require a reason code from a controlled list: Physical count variance, Damage, Theft, Obsolescence, System error, Found stock, Other. Reason codes matter because they roll up in the status reports — a rising trend in Damage or Theft is a signal that warrants investigation. Adjustments above a configurable dollar threshold require supervisor approval before posting; below the threshold they are posted immediately. The threshold is one lever a finance team uses to control exposure from uncontrolled adjustments.

## Cycle counting

Ongoing cycle counts — where inventory is counted section-by-section on a rotating basis rather than all at once annually — generate adjustments whenever counts differ from book. The cycle count process ends with a batch of adjustments hitting the Adjustments page. Regular cycle counting is the discipline that keeps inventory data trustworthy; long gaps between counts let errors accumulate silently.

## The status reports page

Status reports at `/inventory/reports` aggregates inventory data into operational summaries. Common reports are Low stock (items below reorder point), Overstock (items significantly above expected levels), Slow movers (items with no transactions in N days), Ageing (how long current stock has been sitting), and Value summary (inventory value by category, location, or age band). Each report is parameterized and exportable.

## Reading the Ageing report

Ageing is the report inventory planners open most often. It breaks current on-hand by how long the stock has been in its current location — 0-30 days, 30-60, 60-90, 90-180, over 180. High ageing in a fast-moving category signals over-buying or a demand drop; high ageing in a slow-moving category is normal if the item is a critical spare but a problem if it is supposed to turn. Cross-checking ageing against recent adjustments is how long-standing count issues get flushed out.
