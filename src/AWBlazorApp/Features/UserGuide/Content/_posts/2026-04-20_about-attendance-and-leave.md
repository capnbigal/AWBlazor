---
title: About Attendance and Leave
summary: How attendance is tracked per shift and how leave requests are proposed, approved, and reflected on the schedule.
tags: [workforce, attendance, leave]
category: entity-guide
author: AWBlazor
---

Attendance at `/workforce/attendance` and Leave requests at `/workforce/leave-requests` are the two pages that answer "who is on the floor today?" Attendance records actual presence after the fact — who showed up, when they clocked in, when they clocked out. Leave requests record planned absences ahead of time — who will not be here next Thursday, and has it been approved? Together they feed the shift-planning process and the HR analytics dashboard.

## Attendance records

Attendance at `/workforce/attendance` is a record per employee per shift. Each row has the employee, the shift (from the shift catalog under Reference data), the scheduled start and end, the actual clock-in and clock-out timestamps, and any status flags (Late, Left early, No-show, Full attendance). Clock-in and clock-out can come from a physical time-clock system, from operator sign-on at a station, or from manual entry by a supervisor. The actual-versus-scheduled comparison is what surfaces attendance issues.

## Shift definitions

Shifts are defined in the AdventureWorks reference data under `/aw/shifts`. A shift has a name, a start time, and an end time. The shift catalog is relatively stable — you change it when the plant's operating pattern changes, not daily. Employee-to-shift assignments are in employee department histories under Reference data; those track both current and historical shift assignments for each employee.

## Leave requests

A Leave request at `/workforce/leave-requests` captures a planned absence. Each request has the employee, the start date, the end date, the leave type (Vacation, Sick, Personal, Training, Jury duty, Other), a reason field, and an approval state (Submitted, Approved, Denied, Cancelled). Employees submit their own requests; supervisors approve or deny them. Approved requests are visible to the shift scheduler so open shifts can be filled in advance.

## Leave balances

Each leave type has an annual allocation per employee. The Leave balance column on the requests page shows how much the employee has used and how much remains. The balance updates when a leave request is approved, and resets according to the leave type's cycle — vacation typically on the employee's anniversary or the calendar year, sick on a use-it-or-lose-it rolling basis. Over-allocation is allowed with supervisor override but is flagged for HR review.

## Alerts and the Workforce summary

Pending leave requests appear in the qualification alerts inbox at `/workforce/qualification-alerts` so supervisors see them alongside other workforce issues. The Workforce summary at `/workforce` shows current-shift attendance percentage and planned absences for the next seven days. Persistent patterns — repeated lateness, frequent sick-day clusters, planned-absence clashes — feed into the HR analytics dashboard at `/analytics/hr` for longer-term review.
