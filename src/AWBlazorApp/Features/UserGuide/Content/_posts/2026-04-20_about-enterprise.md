---
title: About Enterprise
summary: The Enterprise module — organizations, org units, stations, assets, cost centers, and product lines.
tags: [enterprise, getting-started]
category: entity-guide
author: AWBlazor
---

The Enterprise module in AWBlazor is where you model the physical and organizational shape of your business. It holds the entities that other modules reference constantly — stations for production, assets for maintenance, cost centers for finance, product lines for engineering. Getting Enterprise set up correctly is the first thing that happens in a new deployment, because almost every other feature expects these rows to exist.

## Organizations and org units

At the top of the hierarchy sit Organizations, each representing a legal entity or major business unit. Below them, Org units subdivide an organization into plants, divisions, or cost-bearing groups. The relationship is one-to-many: an organization has many org units, and each org unit rolls up to exactly one organization. The Org tree explorer at `/enterprise/tree` visualizes this hierarchy as a collapsible tree so you can see the whole structure on one page.

## Stations, assets, and machines

A Station represents a workstation on the shop floor — a place where work happens. An Asset or machine represents a specific piece of equipment. Stations and assets have a many-to-many relationship: one station can contain several assets, and one asset can be moved between stations over time. The Stations page at `/enterprise/stations` shows the workstation catalog. The Assets / machines page at `/enterprise/assets` shows the equipment catalog and links to maintenance history. Both pages expose per-row history at `*/history/{id}` so you can see every change with timestamp and user.

## Cost centers

Cost centers group financial reporting across organizational boundaries. Unlike org units, cost centers can cross organizations — a shared IT function or a centralized maintenance pool may bill to cost centers that apply to several org units at once. Cost centers link to assets and stations so that operational events can be rolled up financially in the Performance module.

## Product lines

Product lines group related products for engineering and production planning. They sit in Enterprise rather than Inventory because they are a structural decision — you set up your product lines to match how engineering is organized, not how the warehouse is arranged. Engineering change orders and routings can scope to a product line.

## Reference data

Beneath the workflow entities in the Enterprise nav group sits a collapsed Reference data subsection. It contains the raw AdventureWorks person tables — business entities, persons, addresses, contact types, country regions, and so on. You rarely open these directly; they back the lookup dropdowns in Organizations, Stations, and the customer/vendor pages elsewhere in the app. Open them when you need to audit a value, fix bad contact data, or build a report against the underlying catalog.
