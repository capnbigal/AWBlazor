---
title: About Training Courses and Records
summary: How training courses are catalogued and how individual completions are logged into training records.
tags: [workforce, training]
category: entity-guide
author: AWBlazor
---

Training drives qualifications. Almost every qualification an employee holds was obtained by completing one or more training courses, and keeping those qualifications current means continuing to take renewal training. Training courses at `/workforce/training-courses` define the curriculum, and training records at `/workforce/training-records` log what each employee has actually completed.

## The training course catalog

A Training course at `/workforce/training-courses` has a name, a description, a duration, a delivery method (classroom, online, on-the-job), a list of prerequisites, and a list of qualifications the course issues or renews. Some courses are for initial qualification, some are for renewal, and some are for incremental skill-building. The course record is metadata — it does not hold the training content itself; it describes what the content is and what completing it leads to.

## Sessions and enrollment

A Session is an instance of a course being delivered — "Forklift certification, Tuesday 10am, Classroom B". Sessions have capacity, instructor, start and end timestamps, and an enrollment roster. Employees enroll in sessions either self-service or by manager assignment. The Training courses page lists upcoming sessions per course and lets enrolled employees check themselves in when the session runs.

## Training records

A Training record at `/workforce/training-records` is an individual completion. The record references the employee, the course, the session (if applicable), the completion date, the outcome (Pass, Fail, Incomplete), the instructor, and any score or grade. Records are write-once after the supervisor signs off — you cannot retroactively change whether someone passed, only create a new record for a re-take.

## Issuing and renewing qualifications

When a Pass record is saved for a course that issues a qualification, the corresponding qualification in `/workforce/employee-qualifications` is either created (for a first-time qualification) or renewed (its validity window is reset). If the course was a renewal course, the existing qualification's expiry is pushed forward by the renewal period. This is the automatic link that keeps qualification data in sync with actual training activity — you do not manually update qualifications from training; the system does it when training records are saved.

## Reporting on training

HR analytics at `/analytics/hr` aggregates training data into headline metrics — completion rate by course, time to qualification, renewal compliance, mandatory training status. These metrics feed decisions about training investment — which courses need more sessions, which are too expensive, which are failing to produce the intended qualifications. The training records are the raw data; HR analytics is where it turns into management information.
