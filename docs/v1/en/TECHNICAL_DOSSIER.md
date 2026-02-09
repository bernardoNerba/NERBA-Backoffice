# Technical Dossier and Action Documentation

This document details the documents that make up the Digital Technical Dossier and other reports that can be generated in the context of a Training Action.

## Overview

The system allows for the automatic generation of documents in PDF format for each training action. These documents are generated based on the data entered into the system (trainers, trainees, schedule, evaluations, etc.) and are stored for future reference.

Documents can be generated and viewed in the **"Files"** section within the details page of a Training Action.

## Available Documents

Below is the list of documents available for generation:

| Document | Technical Identifier | Description | Typical Content |
|----------|----------------------|-------------|-----------------|
| **Cover** | `cover` | Identification cover for the Technical Dossier. | Action title, code, dates, funding logos, and entity identification. |
| **Schedule** | `sessions` / `session-report` | Detailed calendar of all action sessions. | List of all sessions with date, time, module, trainer, and room. |
| **General Training Information** | `course-action-information-report` | Administrative and financial summary of the action. | Approval data, funding, curriculum structure, and pedagogical team. |
| **Trainer Form** | `teacher-form` | Individual document for each trainer. | Trainer data, modules taught in the action, workload, and specific trainer schedule. |
| **Student Payment Processing** | `course-action-student-payments-report` | Report for financial processing of grants/support. | List of students with amounts to be paid (grants, subsidies), based on registered attendance. |

## Generation Process

1. **On-Demand Generation**: The user requests the document generation by clicking the "Generate PDF" button (download icon).
2. **Processing**: The backend compiles the current data and generates the PDF file using the `QuestPDF` library.
3. **Storage**: The generated PDF is saved on the server (`wwwroot/storage/pdfs/`) and registered in the database (`SavedPdf`).
4. **Cache**: If the document has already been generated and the data hasn't changed (or if the user only clicks "View"), the system can return the saved version to save resources.

Read more on [PDF_GENERATION.md](PDF_GENERATION.md).

## Available Actions

For each document type, the user has the following options:

- **Generate/Regenerate**: Creates a new version of the document with the most recent data.
- **View**: Opens the generated PDF in a new browser tab.
- **Print**: Sends the print command to the browser.

## Technical References

- **Controller**: `PdfController.cs` (`Core/Reports/Controllers`)
- **Service**: `PdfService.cs` and `IPdfService.cs` (`Core/Reports/Services`)
- **Report Composers**: `Core/Reports/Composers/`
    - `CoverActionReportComposer.cs`
    - `SessionsTimelineComposer.cs` (used in Schedule)
    - `TeacherFormComposer.cs`
    - `CourseActionInformationReportComposer.cs`
    - `CourseActionProcessStudentPaymentsComposer.cs`
- **Frontend**: `pdf-actions.component.ts` (Reusable component for PDF action buttons)
