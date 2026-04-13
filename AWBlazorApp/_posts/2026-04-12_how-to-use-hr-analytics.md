---
title: Using the HR Analytics Dashboard
summary: How to explore headcount, tenure, compensation, and department growth metrics.
tags: [analytics, hr]
category: how-to
author: Elementary App
---

The HR Analytics Dashboard provides workforce insights derived from the `HumanResources.Employee`, `HumanResources.Department`, `HumanResources.EmployeeDepartmentHistory`, and `HumanResources.EmployeePayHistory` tables in AdventureWorks2022. It helps managers and HR professionals understand staffing levels, compensation trends, and organizational growth over time.

## Headcount Overview

The headcount section shows the total number of employees and breaks them down by relevant dimensions. A summary KPI card displays the current headcount — defined as employees with an active department assignment (no `EndDate` in `EmployeeDepartmentHistory`). Additional cards show counts by employment type (salaried vs. hourly), gender distribution, and marital status. A trend chart plots headcount over time by using `HireDate` and department history records to reconstruct staffing levels at any point in the past. This makes it easy to see hiring waves, attrition periods, and net growth.

## Tenure Distribution

The tenure chart groups employees by how long they have been with the company, calculated from their `HireDate` to the current date. Typical buckets include less than 1 year, 1-3 years, 3-5 years, 5-10 years, and 10+ years. This histogram helps identify whether the workforce skews toward recent hires (which may indicate rapid growth or high turnover) or long-tenured employees (which suggests stability but may signal a need for succession planning). You can filter by department to compare tenure distributions across teams.

## Compensation Trend

The compensation section draws from `HumanResources.EmployeePayHistory`, which tracks every pay rate change for each employee. The dashboard plots average pay rate over time, segmented by department or job title. A box-and-whisker or range chart can show the spread of compensation within each group, highlighting pay equity or disparity. The KPI cards display the current average hourly and salaried rates alongside period-over-period change percentages. This data is useful for benchmarking compensation against budget targets and understanding how pay levels have evolved as the company has grown.

## Department Growth

The department growth view uses `HumanResources.EmployeeDepartmentHistory` to show how each department's headcount has changed over time. Each department row shows its starting headcount, current headcount, and net change for the selected period. A bar chart ranks departments by growth rate, making it easy to spot which teams are expanding and which are contracting. You can click on a department to see a detailed timeline of employee joins and departures, including which specific roles were filled or vacated.

## Filtering and Export

The HR Dashboard supports the same period selectors and filtering controls as other analytics dashboards. You can scope all views to a specific date range, filter by department, shift, or job title, and export the underlying data to CSV. The export includes all computed metrics — headcount by period, average tenure, compensation averages — formatted for direct use in spreadsheets or external HR reporting tools.
