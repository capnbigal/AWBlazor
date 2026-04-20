---
title: How to Use the Org Tree Explorer
summary: Walk the organization hierarchy at /enterprise/tree, expand nodes, and drill into any org unit.
tags: [enterprise, navigation]
category: how-to
author: AWBlazor
---

The Org tree explorer at `/enterprise/tree` is the fastest way to navigate a large organizational hierarchy. It shows every organization and org unit as a collapsible tree, with counts of child nodes next to each branch. You can expand from the top down, jump directly to a known node via the search box, and click through to any org unit's detail page without leaving the tree.

## Reading the tree

Each node shows the name of the organization or org unit, an icon that identifies its type, and a small counter in parentheses showing how many children it contains. Organizations are always the roots of the tree. Org units nest below organizations and can themselves contain further org units, reflecting multi-level reporting structures. Leaf nodes — org units with no children — display with a flat icon instead of a folder.

## Expanding and collapsing

Click the chevron to the left of any node to expand or collapse it. Holding Alt while clicking expands the entire subtree under that node in one step; this is useful when you want to print or screenshot a full organization. The initial load shows only the top-level organizations; subtrees load on demand when you expand them, so large hierarchies do not slow down the initial render.

## Searching within the tree

The search box above the tree filters nodes by name as you type. Matching nodes are highlighted and their ancestor chains are auto-expanded, so you can see where a match sits in the hierarchy. Clearing the search box collapses the tree back to whatever expansion state it had before. This makes it easy to look up a specific org unit by name without guessing which branch it lives under.

## Drilling into a node

Clicking the name of any node navigates to that entity's detail page. Clicking an organization takes you to `/enterprise/organizations` filtered to that row. Clicking an org unit takes you to `/enterprise/org-units` with the selected row expanded. From there you can see related stations, cost centers, and audit history. Use the browser back button to return to the tree with your expansion state preserved.

## When the tree is out of date

The tree loads from the current state of the Organizations and OrgUnits tables each time you open the page, so it always reflects the latest data. If you create a new org unit in another tab, refresh the tree page to pick up the change — the tree does not auto-refresh on external edits.
