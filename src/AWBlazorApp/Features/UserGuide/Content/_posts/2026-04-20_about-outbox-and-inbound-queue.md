---
title: About the Inventory Outbox and Inbound Queue
summary: How outbound messages to external systems are staged and how inbound messages from external systems are processed.
tags: [inventory, integration]
category: entity-guide
author: AWBlazor
---

Real plants almost always integrate their inventory data with other systems — an ERP that books the accounting, a WMS that runs the warehouse, a barcode scanner network that records physical movements, a customer portal that needs shipment status. AWBlazor exposes two pages that handle the traffic in both directions. Outbox at `/inventory/outbox` holds outbound messages waiting to be sent. Inbound queue at `/inventory/queue` holds incoming messages waiting to be processed.

## How the Outbox works

When an event happens in Inventory that external systems need to know about — a balance change, a new adjustment, a shipment — a message is created on the Outbox. Each message has a target system identifier, a payload (typically JSON), a created timestamp, a status (Pending, In flight, Delivered, Failed), a retry count, and a last-attempt timestamp. A background worker picks up Pending messages, sends them to the target system, and updates the status based on the response.

## Retry and failure handling

If a delivery fails — the target system is down, a validation error came back, a timeout — the message moves to Failed with an error description captured. Failed messages are retried automatically with exponential backoff for transient errors (network issues, timeouts) up to a retry cap. Non-transient errors (validation, schema mismatch) go to Failed immediately without retrying, because retrying them will not help. Supervisors review the Failed list on the Outbox page and either manually retry, mark as ignored, or open an issue against the integration.

## The Inbound queue

Inbound queue at `/inventory/queue` holds messages arriving from external systems — a receipt notification from a supplier, a scan event from a barcode reader, an adjustment from a customer portal. Each message has a source system identifier, a raw payload, a received timestamp, a status, and any processing errors. A background worker dequeues messages, translates them into inventory transactions, and posts the transactions through the same code path as interactive adjustments — so queued operations hit the same validations and audit trail.

## Validation and rejection

Inbound messages that fail validation are not silently dropped. They move to a Rejected status with the validation errors captured. A supervisor reviews the Rejected list, corrects the source data or the mapping, and manually retries. This is how you catch systematic integration issues — the Rejected list rising is a signal that something upstream changed, not a problem to be fixed by ignoring the messages.

## Why both pages exist

Outbox and Inbound queue exist for the same reason: to decouple external systems from the core inventory processing. When an external system is slow or unavailable, events keep accumulating in the Outbox (for outbound) or on the Queue (for inbound) without blocking the users posting transactions in the app. When the external system comes back, the backlog drains automatically. The pages exist so operators can see the backlog state, diagnose blockages, and intervene manually on stuck messages.
