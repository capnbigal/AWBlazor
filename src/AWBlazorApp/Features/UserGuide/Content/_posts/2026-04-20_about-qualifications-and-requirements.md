---
title: About Qualifications and Station Requirements
summary: The three qualification pages — qualification catalog, employee qualifications, and station requirements — and how they interact.
tags: [workforce, qualifications]
category: entity-guide
author: AWBlazor
---

The qualification system in Workforce has three pages that work together. The Qualifications catalog at `/workforce/qualifications` defines the named competencies. Employee qualifications at `/workforce/employee-qualifications` tracks which employees hold which qualifications. Station requirements at `/workforce/station-qualifications` defines what qualifications a station requires. The three together answer the scheduling question: "who can work at station X right now?"

## The Qualifications catalog

A Qualification at `/workforce/qualifications` is a named competency with a description, a validity period (in days), and a list of training courses that issue or renew it. Qualifications are categorized — Safety, Operator, Technical, Regulatory — so reports can aggregate by category. New qualifications are added when a new process or piece of equipment is introduced that requires a distinct competency; retiring a qualification is done by marking it Inactive rather than deleting, so historical assignments stay interpretable.

## Employee qualifications

Employee qualifications at `/workforce/employee-qualifications` is the many-to-many table linking employees to qualifications. Each row has the employee, the qualification, the issue date, the expiry date, and the state (Active, Expiring soon, Expired, Revoked). The expiry date is set from the qualification's validity period when the record is issued. The Expiring soon state appears when an expiry date is within the configured warning window — typically 30 or 60 days — and triggers a qualification alert in `/workforce/qualification-alerts`.

## Station requirements

Station requirements at `/workforce/station-qualifications` defines what qualifications are needed to operate a given station. A station typically requires a combination — a safety qualification plus an operator qualification plus any station-specific certification. The requirements can specify "any-of" alternatives so that equivalent qualifications are accepted. When the scheduler is assigning an operator to a station for a run, it checks the employee's current qualifications against the station's requirements and flags a mismatch before the run starts.

## Renewal and lapse

Qualifications expire. When an expiry date passes without a renewal training record being logged, the employee qualification moves to Expired state and the employee can no longer be scheduled to stations that require it. Managers receive an alert in the qualification alerts inbox before the lapse happens, giving time to schedule renewal training. Renewal is typically a shorter refresher course compared to the initial qualification; the course record in `/workforce/training-courses` specifies which.

## Revocation

Occasionally a qualification needs to be revoked before its natural expiry — after an incident, a failed re-inspection, or a policy change. Revocation is done by setting the employee qualification record to Revoked with a reason. Revoked qualifications behave like expired ones for scheduling purposes but carry an audit reason that distinguishes them. The Workforce summary and HR analytics pages report revocations separately from natural expiries.
