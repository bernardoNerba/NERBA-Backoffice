# Dashboard and KPIs

This document describes the Key Performance Indicators (KPIs) presented on the dashboard and other areas of the NERBA Backoffice system, detailing their data source, calculation rules, applied filters, and display format.

## Table of Contents

- [Main Dashboard](#main-dashboard)
  - [Numeric Indicators](#numeric-indicators)
  - [Charts](#charts)
  - [Top 5 Lists](#top-5-lists)
- [Course KPIs](#course-kpis)
- [Action KPIs](#action-kpis)

---

## Main Dashboard

The main dashboard aggregates metrics from various areas of the system to provide a quick overview of the training status.

### Numeric Indicators

These indicators are presented as simple cards at the top of the dashboard.

| Title | Description | Data Source | Time Filter | Calculation | Visual Format |
|-------|-------------|-------------|-------------|-------------|---------------|
| **Teacher Payments** | Total paid to teachers | `ModuleTeachings` Table | Current Month | Sum of `PaymentTotal` where `PaymentDate` is not null | Numeric Value + "€" (e.g., 1500€) |
| **Student Payments** | Total paid to students | `ActionEnrollments` Table | Current Month | Sum of `PaymentTotal` where `PaymentDate` is not null | Numeric Value + "€" (e.g., 850€) |
| **Total Companies** | New registered companies | `Companies` Table | Current Year | Count of records created (`CreatedAt`) | Integer Number |

### Charts

Charts provide analytical visualizations about students and actions.

#### Students by Qualification Level
- **Type:** Line Chart
- **Source:** `People` Table (only records linked to `Student`)
- **Filter:** Created in the Current Year (last 12 months)
- **Logic:** Groups students by their qualification level and counts occurrences.
- **Axes:** 
  - X: Qualification Levels (humanized)
  - Y: Quantity of Students

#### Student Results
- **Type:** Doughnut Chart (Pie Chart)
- **Source:** `ActionEnrollments` Table
- **Filter:** Created in the Current Year
- **Logic:** Groups enrollments by approval status (`ApprovalStatus`).
- **Statuses:** Approved, Rejected, Dropped, Not Specified.

#### Actions by Minimum Qualification Level
- **Type:** Doughnut Chart
- **Source:** `Actions` Table -> `Course`
- **Filter:** All history ("Ever")
- **Logic:** Groups actions based on the `MinHabilitationLevel` of the associated course.
- **Categories:**
  - **Level 1:** Up to 9th grade (inclusive) and "No Qualification"
  - **Level 2:** 10th, 11th, and 12th grade
  - **Level 3:** Higher Education (Post-Secondary to Doctorate)

#### Students by Gender Over Time
- **Type:** Bar Chart
- **Source:** `People` Table (Student)
- **Filter:** Created in the Current Year (for annual chart) or History (for "Ever")
- **Logic:** Groups students by gender and creation month.
- **Visualization:** Monthly time series for each gender.

### Top 5 Lists

Presented in tabs for quick consultation of the most active categories.

| Category | Source | Sorting Criteria | Description |
|----------|--------|------------------|-------------|
| **Top Locations** | `Actions` Table | Descending Count | The 5 locations with the highest number of actions created in the current year. |
| **Top Regimes** | `Actions` Table | Descending Count | The 5 regimes (e.g., Laboral, Pós-Laboral) most frequent in the current year. |
| **Top Statuses** | `Actions` Table | Descending Count | The 5 statuses (e.g., In Progress, Completed) with the most actions in the current year. |

---

## Course KPIs

These indicators are calculated when viewing the details of a specific course (`GetCourseKpisAsync`). They aggregate data from **all** actions associated with that course.

| Indicator | Data Source | Calculation | Description |
|-----------|-------------|-------------|-------------|
| **Total Students** | `Course.Actions` | Sum of `Action.TotalStudents` | Total number of students enrolled in all editions of this course. |
| **Total Approved** | `Course.Actions` | Sum of `Action.TotalApproved` | Total number of students who completed with success. |
| **Total Volume (Hours)** | `Course.Actions` | Sum of `Action.TotalVolumeHours` | Total volume of training hours delivered (Workload x Participants). |
| **Total Volume (Days)** | `Course.Actions` | Sum of `Action.TotalVolumeDays` | Total accumulated training days. |

---

## Action KPIs

These indicators are specific to a single training action (`GetActionKpisAsync`).

| Indicator | Data Source | Calculation | Description |
|-----------|-------------|-------------|-------------|
| **Total Students** | `ActionEnrollments` | Count (`Count`) | Number of students enrolled in the action. |
| **Total Approved** | `ActionEnrollments` | Count where `ApprovalStatus == Approved` | Number of approved students. |
| **Total Volume (Hours)** | `SessionParticipations` | Sum of `Attendance` (presence hours) | Sum of all effective presence hours of all students. |
| **Total Volume (Days)** | `SessionParticipations` | Count where `Presence == Present` | Total number of registered presences (each record counts as 1 day/session). |

---

## Technical References

- **API Controller:** `NERBABO.Backend/NERBABO.ApiService/Core/Dashboard/Controllers/DashboardController.cs`
- **Calculation Service:** `NERBABO.Backend/NERBABO.ApiService/Core/Kpis/Services/KpisService.cs`
- **Frontend Component:** `NERBABO.Frontend/src/app/features/dashboard/dashboard.component.ts`
