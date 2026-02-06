# PDF Generation

This document describes the PDF report generation system implemented in NERBA Backoffice, using the **QuestPDF** library.

## Index

- [Overview](#overview)
- [Architecture](#architecture)
- [Main Components](#main-components)
- [Composer Pattern](#composer-pattern)
- [Generation Flow](#generation-flow)
- [How to Add a New Report](#how-to-add-a-new-report)
- [References](#references)

---

## Overview

The system uses the **QuestPDF** library to generate PDF documents programmatically. The architecture was designed to separate data retrieval logic (Services) from visual layout logic (Composers).

### Technologies

- **QuestPDF**: Fluent layout library for .NET.
- **ASP.NET Core DI**: Dependency injection for managing services and composers.

---

## Architecture

The system follows an architecture based on services and visual composers:

```
Core/
├── Reports/
│   ├── Controllers/
│   │   └── PdfController.cs       # API Endpoints
│   ├── Services/
│   │   ├── IPdfService.cs         # Service Interface
│   │   └── PdfService.cs          # Data and persistence orchestrator
│   ├── Composers/                 # Visual Logic (Layouts)
│   │   ├── HelperComposer.cs      # Shared styles and components
│   │   ├── SessionsTimelineComposer.cs
│   │   ├── TeacherFormComposer.cs
│   │   └── ...
│   └── Models/
│       └── SavedPdf.cs            # Model for metadata persistence
```

---

## Main Components

### PdfController

Responsible for exposing HTTP endpoints. It checks permissions, retrieves the current user, and calls `PdfService`. It returns the generated PDF file (`FileResult`) or an error (`ProblemDetails`).

### PdfService

The central service that:
1. **Retrieves Data**: Queries `AppDbContext` to get all necessary entities (Actions, Sessions, Teachers, etc.).
2. **Invokes Composers**: Instantiates the appropriate composer injected via DI and passes the data.
3. **Persistence**: Saves the generated PDF to disk (`storage/pdfs`) and logs metadata in the `SavedPdfs` table.
4. **Caching**: Checks if content has changed (via SHA256 hash) before replacing existing files.

### Composers

Classes responsible exclusively for the document layout. Each report has its own composer.

**Example structure of a Composer:**

```csharp
public class MyReportComposer
{
    private readonly HelperComposer _helperComposer;

    public MyReportComposer(HelperComposer helperComposer)
    {
        _helperComposer = helperComposer;
    }

    public async Task<Document> ComposeAsync(MyEntity data, GeneralInfo infos)
    {
        // Load resources (logos, images)
        // Define document structure (Page, Header, Content, Footer)
        return Document.Create(container => { ... });
    }
}
```

### HelperComposer

Contains shared styles and reusable components to maintain visual consistency across reports:
- Standard Headers and Footers.
- Logo loading.
- Text and table styles.

---

## Composer Pattern

The Composer pattern isolates the QuestPDF layout complexity.

### Dependency Injection

All composers are registered in the DI container in `Program.cs`:

```csharp
// Register Pdf Composer Services
builder.Services.AddScoped<HelperComposer>();
builder.Services.AddScoped<SessionsTimelineComposer>();
builder.Services.AddScoped<CoverActionReportComposer>();
// ...
```

This allows a composer to use other composers (like `HelperComposer`) via constructor injection.

---

## Generation Flow

1. **Request**: The client requests `GET /api/Pdf/action/{id}/report`.
2. **Service**: `PdfService.GenerateReportAsync` is called.
3. **Data Fetch**: Data is loaded from `AppDbContext` using `Include()` for relationships.
4. **Validation**: If the entity does not exist, returns `Result.Fail`.
5. **Composition**: The `ComposeAsync` method of the respective Composer is called.
6. **Generation**: `document.GeneratePdf()` converts the layout to bytes.
7. **Saving**: `SavePdfAsync` saves the file and creates a record in the database.
8. **Response**: The controller returns the PDF file.

---

## How to Add a New Report

To add a new report to the system, follow these steps:

### 1. Create the Composer

Create a new class in `Core/Reports/Composers/` (e.g., `NewReportComposer.cs`). Inject `HelperComposer`.

```csharp
public class NewReportComposer(HelperComposer helperComposer)
{
    public async Task<Document> ComposeAsync(MyData data, GeneralInfo infos)
    {
        return Document.Create(container => 
        {
            // Define layout
        });
    }
}
```

### 2. Register in DI

Add the registration in `Program.cs`:

```csharp
builder.Services.AddScoped<NewReportComposer>();
```

### 3. Update PdfService

Add a new method to `IPdfService` and implement it in `PdfService`.

```csharp
public async Task<Result<byte[]>> GenerateNewReportAsync(long id, string userId)
{
    // 1. Get data
    // 2. Call _newReportComposer.ComposeAsync(...)
    // 3. Generate and save PDF
}
```

Don't forget to inject the new composer into the `PdfService` constructor.

### 4. Create Endpoint in Controller

Add an action in `PdfController`:

```csharp
[HttpGet("new-report/{id}")]
public async Task<IActionResult> GenerateNewReportAsync(long id)
{
    // ... call service and return File
}
```

---

## References

- [QuestPDF Documentation](https://www.questpdf.com/documentation/getting-started.html)
- [Result Pattern](./RESULT_PATTERN.md)
