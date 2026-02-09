# Course Action Lifecycle Guide

This guide describes the step-by-step process to create, manage, and conclude a Training Action in the NERBA Backoffice system, including managing all its dependencies.

## Lifecycle Overview

The Action lifecycle follows this logical order:

1.  **Context Definition** (Frame and Course)
2.  **Action Creation**
3.  **Pedagogical Setup** (Team and Schedule)
4.  **Student Management**
5.  **Monitoring and Conclusion**

---

## 1. Context Definition (Prerequisites)

Before creating an action, you must ensure the base structure exists.

### 1.1. Create Frame (Enquadramento)
The Frame defines the funding program (e.g., PO CH, PRR).
*   **Where:** Sidebar > Frames > "Create" Button
*   **Required Data:**
    *   Program and Measure
    *   Operation Code
    *   Logos (Program and Financing) for official documents

### 1.2. Create Course
The Course defines the curricular structure that will be replicated in actions.
*   **Where:** Sidebar > Courses > "Create" Button
*   **Required Data:**
    *   Frame (select the one created previously)
    *   Course Title
    *   Qualification Level
    *   Total Hours
*   **Critical Step:** After creating the course, it is mandatory to **associate Modules** (UFCDs) that make up the course, defining each module's hours. The Action will inherit these modules.

---

## 2. Action Creation

The Action is a temporal instance of a Course (a specific class).

*   **Where:** Sidebar > Actions > "Create Action" Button
*   **Required Data:**
    *   **Base Course:** Select the course to be taught.
    *   **Administrative Code:** Unique code for the action (e.g., `ACTION_01_2024`).
    *   **Coordinator:** The user creating the action is automatically set as coordinator (can be changed).
    *   **Dates:** Estimated Start and End Date.
    *   **Schedule/Regime:** Standard/After-hours and usual weekdays.
    *   **Locality:** Where the training takes place.

> **Note:** When creating the action, course modules are automatically associated, but they don't have teachers or specific dates yet.

---

## 3. Pedagogical Setup

In this phase, you define who teaches and when. Access **Action Details**.

### 3.1. Assign Teachers (Pedagogical Team)
For each module in the action, you must indicate the responsible teacher.
*   **Where:** Action Details > "Pedagogical Team" Tab (or Modules)
*   **Action:** Edit a module or click "Assign Teacher".
*   **Rules:**
    *   The teacher must be registered in the system.
    *   A module cannot be left without a teacher if you want to schedule sessions for it.

### 3.2. Schedule Sessions (Cronograma)
The schedule defines the actual dates and times for each class.
*   **Where:** Action Details > "Schedule" or "Sessions" Tab
*   **Process:**
    1.  Click "Create Session" (or bulk scheduling if available).
    2.  Select the **Module** and **Teacher** (auto-filled based on previous assignment).
    3.  Define Date, Start Time, End Time, and Room.
    4.  Write the Summary (can be filled later by the teacher).

---

## 4. Student Management

### 4.1. Enroll Students
*   **Where:** Action Details > "Students" Tab
*   **Process:**
    *   Click "Add Student".
    *   Search for an existing person or create a new person record.
    *   Define Enrollment Date and Status (e.g., "In Training").
*   **Automation:** When enrolling a student, the system automatically generates:
    *   Evaluation records (empty) for all modules.
    *   Attendance records for sessions already scheduled.

---

## 5. Monitoring and Conclusion

During the action, the coordinator and teachers manage day-to-day activities.

### 5.1. Register Attendance and Summaries
*   Teachers or the coordinator must access sessions and mark student absences.
*   This information feeds into payment processing (grants).

### 5.2. Launch Evaluations
*   At the end of each module, grades must be entered in the Evaluations tab.

### 5.3. Generate Documentation
At any time, you can check the **"Files"** tab to generate:
*   Technical Pedagogical Dossier.
*   Payment Reports.
*   Summary and Attendance Sheets.
*(See details in [Technical Dossier](TECHNICAL_DOSSIER.md))*

### 5.4. Conclude Action
When training ends:
1.  Verify if all evaluations and attendances are entered.
2.  Access general action data ("Edit").
3.  Change the **Status** to "Concluded" or "Finished".
4.  This action locks future edits and archives the action in history.
