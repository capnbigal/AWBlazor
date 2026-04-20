---
title: About Shift Handover and Announcements
summary: How the outgoing shift tells the incoming shift what they need to know, and how plant-wide announcements reach everyone.
tags: [workforce, communication]
category: entity-guide
author: AWBlazor
---

Two Workforce pages handle communication across the floor. Shift handover at `/workforce/handover-notes` is the structured place where the outgoing shift captures what the incoming shift needs to know. Announcements at `/workforce/announcements` is the plant-wide broadcast channel for messages from management. Both are deliberately simple — they earn their keep by being short to write and quick to read.

## Shift handover notes

A handover note at `/workforce/handover-notes` is created near the end of a shift and documents anything the next shift should know. Common entries are equipment currently down for maintenance, active deviations in effect, material shortages, quality holds, unusual operator absences, and anything in progress that was not finished. Each note has an author, a timestamp, a target audience (team, station, or plant-wide), and a body. Notes carry a read-receipt state so the incoming shift supervisor can confirm they have reviewed the handover before signing on.

## Structure vs free-form

Handover notes are free-form text rather than a rigid template. Experience across multiple plants has shown that templates either get ignored (operators fill in what they feel like) or become theatre (operators fill in every field with "OK" to satisfy the form). A short narrative entry from an engaged operator is more useful than a complete-looking template from a disengaged one. The structure the page enforces — timestamp, author, audience, read state — is the minimum needed for the note to be findable later.

## Referencing other entities

Handover note bodies support Markdown, and cross-links to other parts of the app are encouraged. A note about a down machine can link directly to the maintenance work order; a note about a quality hold can link to the NCR. This keeps the note concise — "Line 3 press down, see WO-12847" is shorter and more useful than restating everything the linked page already shows. The linked pages are authoritative; the handover note is the pointer.

## Announcements

Announcements at `/workforce/announcements` are management communications that apply to the whole plant or a defined audience — team, shift, or department. Each announcement has a title, a body (also Markdown), an author, a publish date, an optional expiry, and an audience. Published announcements appear on the Home page for the users in their audience and in the Announcements list.

## Expiring announcements

Announcements can have an expiry date. When an announcement expires, it disappears from the Home page but remains in the list for historical reference. This keeps the dashboard uncluttered while preserving the record — "we told everyone about this on May 3" is an answer you may need later. Unexpired announcements are pinned to the top of the list; expired ones fall to a collapsed "Past announcements" section below.
