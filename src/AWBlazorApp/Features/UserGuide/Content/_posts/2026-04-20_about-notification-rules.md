---
title: About Notification Rules
summary: How to configure which events ping you, through which channels, at /notifications.
tags: [my-workspace, notifications]
category: entity-guide
author: AWBlazor
---

Notification rules at `/notifications` control which events ping you, through which channels, and at what volume. The goal is to let you see the events that matter to your role without being buried in noise from events that do not. Every notification in AWBlazor comes from a rule you have explicitly enabled — there is no mandatory notification you cannot turn off except system-level security alerts tied to your account.

## Event types

Notification rules can fire on a wide range of events. Quality events include new NCRs, CAPA due dates approaching, inspections failing. Maintenance events include new reactive work orders, PM schedules going overdue, spare parts going below reorder. Workforce events include qualifications expiring, leave requests awaiting your approval. Process events include phase transitions on processes where you are a stakeholder. Integration events include failed outbox deliveries and rejected inbound queue messages. The list of event types is extensible — new event types are added as new features are released.

## Rule structure

Each rule has an event type, a filter (scope the rule to specific assets, stations, teams, or other dimensions), a channel (in-app, email, or both), a delivery mode (immediate, digest, or quiet hours), and an active flag. You can have multiple rules for the same event type with different filters — for example, an immediate email for critical-asset alerts and a daily digest for non-critical ones.

## Digest mode

Digest mode bundles multiple notifications into a single delivery per time window. A daily digest collects all matching events from a 24-hour period into one email; an hourly digest does the same over an hour. Digest is the right choice for event types that are informative but not immediately actionable — you want to know what happened without having each event interrupt you.

## Quiet hours

Quiet hours suppress non-critical notifications outside your working time. You set your working hours on the notifications page (including time-zone awareness). Notifications that match a quiet-hours rule are held until the next working-hours window and then delivered. Critical events (override-flagged at the rule level) bypass quiet hours — a safety-critical alert should not wait until morning. Non-critical events respect them.

## Default rules

New accounts are seeded with a small default set of rules — the ones most users want. You can delete defaults you do not want and add rules you do. The default set is deliberately minimal to avoid the common experience of signing up and being immediately buried in email; it is easier to add rules you need than to figure out which of the 30 pre-enabled ones to turn off.
