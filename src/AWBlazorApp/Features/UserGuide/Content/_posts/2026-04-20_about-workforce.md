---
title: About Workforce
summary: The Workforce module — qualifications, training, attendance, leave, alerts, handover notes, and announcements.
tags: [workforce]
category: entity-guide
author: AWBlazor
---

The Workforce module at `/workforce` is where the people side of the plant is managed. It holds qualifications (what people are trained to do), training records (how they got qualified), attendance and leave (who is on the floor today), handover notes (what the outgoing shift needs the incoming shift to know), and announcements (company-wide communications). The module is designed around the reality that production only happens when the right qualified people show up and know what to do.

## Workforce summary

The Workforce summary page at `/workforce` is the landing view. It shows current-shift headcount, open qualification alerts, training course enrollments, and pending leave requests. Cross-links from each card navigate to the detail pages. The HR analytics dashboard linked from the top of the nav group covers longer-horizon workforce trends; the summary covers right now.

## Qualifications and training

The core of the Workforce module is the qualification system. A Qualification at `/workforce/qualifications` is a named competency — "Certified forklift operator", "CNC setup". Each qualification has a validity period and required training. Employee qualifications at `/workforce/employee-qualifications` track which employee holds which qualification, when it expires, and its renewal state. Station requirements at `/workforce/station-qualifications` define which qualifications are required to work at each station, so the scheduler knows who can be assigned where.

## Training

Training courses at `/workforce/training-courses` describe the curriculum that leads to qualifications. Training records at `/workforce/training-records` log individual completions — who attended, when, with what outcome. The training records feed back into employee qualifications when a course is completed; a course-completion event can issue a qualification automatically or mark an existing one as renewed.

## Attendance, leave, and alerts

Attendance at `/workforce/attendance` records who was on the floor each shift. Leave requests at `/workforce/leave-requests` hold planned absences for approval. The Alert inbox at `/workforce/qualification-alerts` aggregates actionable workforce issues — qualifications expiring soon, mandatory trainings overdue, leave requests awaiting approval. The alert inbox is the single daily view for a workforce manager.

## Handover and announcements

Shift handover at `/workforce/handover-notes` is the structured place where the outgoing shift captures what the incoming shift needs to know — issues in progress, deviations in effect, equipment notes. Announcements at `/workforce/announcements` are broader communications from management. Both are chronological and can be tagged by team or station so the right audience sees the right content.

## Reference data

The Workforce group's Reference data subsection holds the raw AdventureWorks HR tables — employees, employee pay histories, job candidates, departments, shifts, and employee department histories. These back the lookup dropdowns in the workflow pages above. You open them directly when auditing pay data, maintaining the shift catalog, or building an HR report against the underlying tables.
