# Bulk Import

This document describes the bulk import feature for entities in the NERBA Backoffice system.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Current Implementation: People](#current-implementation-people)
- [Import Flow](#import-flow)
- [Validation and Error Handling](#validation-and-error-handling)
- [Template Structure](#template-structure)
- [Expanding to New Entities](#expanding-to-new-entities)
- [Best Practices](#best-practices)
- [References](#references)

---

## Overview

The bulk import feature allows loading multiple records of an entity through CSV or Excel files. The system processes each row individually, validating data and reporting detailed errors per row.

### Capabilities

| Feature | Description |
|---------|-------------|
| Supported formats | CSV (delimiter `;`) and Excel (`.xlsx`, `.xls`) |
| File limit | 10MB |
| Batch processing | Configurable (default: 100 records) |
| Stop mode | Option to stop on first error |
| Dry-run validation | Validate file without saving data |
| Templates | Download pre-formatted templates |

---

## Architecture

The feature follows a modular architecture with separation of concerns.

### Backend Components

```
Shared/BulkImport/
├── Models/
│   └── BulkImportResult.cs      # Result models
└── Services/
    ├── IBulkImportService.cs    # Generic base interface
    ├── IFileParserService.cs    # Parsing interface
    ├── CsvParserService.cs      # CSV parser
    └── ExcelParserService.cs    # Excel parser

Core/People/BulkImport/
└── Services/
    ├── IPeopleBulkImportService.cs   # Specific interface
    └── PeopleBulkImportService.cs    # Implementation
```

### Frontend Components

```
features/people/import-people/
├── import-people.component.ts       # Main component
├── import-people.component.html     # Modal template
├── import-people.component.css      # Styles
└── import-result-modal.component.ts # Results modal
```

### Data Flow

```
Frontend                    Backend
   │                           │
   ├── Upload file ──────────► Controller
   │                           │
   │                      ┌────┴────┐
   │                      │Validate │
   │                      │structure│
   │                      └────┬────┘
   │                           │
   │                      ┌────┴────┐
   │                      │ Parse   │
   │                      │  file   │
   │                      └────┬────┘
   │                           │
   │                      ┌────┴────┐
   │                      │Validate │
   │                      │each row │
   │                      └────┬────┘
   │                           │
   │                      ┌────┴────┐
   │                      │  Save   │
   │                      │in batch │
   │                      └────┬────┘
   │                           │
   ◄── BulkImportResult ───────┘
```

---

## Current Implementation: People

### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/people/import/csv` | Import from CSV |
| `POST` | `/api/people/import/excel` | Import from Excel |
| `POST` | `/api/people/import/validate/csv` | Validate CSV (dry-run) |
| `POST` | `/api/people/import/validate/excel` | Validate Excel (dry-run) |
| `GET` | `/api/people/import/template/csv` | Download CSV template |
| `GET` | `/api/people/import/template/excel` | Download Excel template |

### Query Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `stopOnFirstError` | boolean | `false` | Stop on first error |
| `batchSize` | int | `100` | Records per batch |

### Template Fields

| Field | Required | Description |
|-------|----------|-------------|
| `NomeCompleto` | Yes | First name and surname (minimum 2 words) |
| `NIF` | Yes | 9 digits, unique in system |
| `NumeroIdentificacao` | No | 5-10 characters |
| `DataValidacaoIdentificacao` | No | Future date (format: `dd/MM/yyyy`) |
| `NISS` | No | 11 digits |
| `IBAN` | No | 25 characters |
| `DataNascimento` | No | Past date (format: `dd/MM/yyyy`) |
| `Morada` | No | Free text |
| `CodigoPostal` | No | Format `NNNN-NNN` |
| `Telefone` | No | 9 digits |
| `Email` | No | Valid email format, unique in system |
| `Naturalidade` | No | 3-100 characters |
| `Nacionalidade` | No | 3-100 characters |
| `Genero` | No | See allowed values |
| `Habilitacao` | No | See allowed values |
| `TipoIdentificacao` | No | See allowed values |

### Allowed Enum Values

**Gender (Genero):**
- `Nao Especificado` (Not Specified)
- `Masculino` (Male)
- `Feminino` (Female)
- `Outro` (Other)

**Identification Type (TipoIdentificacao):**
- `Nao Especificado` (Not Specified)
- `Autorizacao de Residencia` (Residence Permit)
- `Identificacao Civil (CC/BI)` (Civil ID)
- `Militar` (Military)
- `Passaporte` (Passport)

**Qualification (Habilitacao):**
- `Sem Comprovativo` (Without Proof)
- `Sem escolaridade` (No Education)
- `1º Ano` to `12º Ano` (1st to 12th Grade)
- `Pos-Secundario` (Post-Secondary)
- `Bacharelato` (Bachelor's Degree)
- `Licenciatura` (Degree)
- `Mestrado` (Master's Degree)
- `Doutoramento` (Doctorate)

---

## Import Flow

### 1. Structure Validation

The system first validates the file structure:
- Correct extension (`.csv`, `.xlsx`, `.xls`)
- Presence of required headers
- Non-empty file

### 2. File Parsing

Each parser (CSV or Excel) converts rows into dictionaries `<header, value>`.

**CSV Specifics:**
- Delimiter: `;` (European format)
- Culture: `pt-PT`
- Dates converted to `dd/MM/yyyy`

**Excel Specifics:**
- First sheet processed
- First row as headers
- Dates converted to `dd/MM/yyyy`

### 3. DTO Mapping

The `MapRowToDto` function converts each row to the corresponding DTO:

```csharp
// Mapping example (PeopleBulkImportService.cs)
var dto = new CreatePersonDto
{
    FirstName = firstName,  // Extracted from NomeCompleto
    LastName = lastName,    // Last word of NomeCompleto
    NIF = GetValue(row, "NIF"),
    // ...
};
```

### 4. Data Validation

Two validation levels:

**Data Annotations:**
- Required fields
- Minimum/maximum lengths
- Specific formats

**Business Rules:**
- Unique NIF (system + file)
- Unique email (system + file)
- Valid enums

### 5. Batch Persistence

Valid records are saved in batches to optimize performance:

```csharp
if (successfulPeople.Count >= options.BatchSize)
{
    await CommitBatchAsync(successfulPeople, result, batchResultIndices);
    successfulPeople.Clear();
}
```

### 6. Post-Processing

- Cache invalidation
- Notification generation for missing documents

---

## Validation and Error Handling

### Result Structure

```csharp
public class BulkImportResult<T>
{
    public bool Success { get; set; }
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<ImportRowResult<T>> Results { get; set; }
    public List<string> GlobalErrors { get; set; }
}
```

### Error Types

| Type | Level | Behavior |
|------|-------|----------|
| Global error | File | Interrupts processing |
| Row error | Row | Row skipped, processing continues |
| Warning | Row | Row imported with warning |

### Response Example

```json
{
  "success": true,
  "totalRows": 50,
  "successCount": 48,
  "failureCount": 2,
  "results": [
    {
      "rowNumber": 15,
      "success": false,
      "errors": [
        {
          "field": "NIF",
          "errorMessage": "NIF already exists in the system",
          "attemptedValue": "123456789",
          "severity": "Error"
        }
      ]
    }
  ],
  "summary": "Import completed: 48 successful, 2 failed"
}
```

---

## Template Structure

### Location

```
NERBABO.Backend/NERBABO.ApiService/wwwroot/templates/
├── People_Import_Template.csv
└── People_Import_Template.xlsx
```

### CSV Example

```csv
NomeCompleto;NIF;NumeroIdentificacao;DataValidacaoIdentificacao;NISS;IBAN;DataNascimento;Morada;CodigoPostal;Telefone;Email;Naturalidade;Nacionalidade;Genero;Habilitacao;TipoIdentificacao
Joao Silva;123456789;AB123456;31/12/2030;12345678901;PT50000201231234567890154;15/01/1990;Rua Example 123;1000-001;912345678;joao.silva@example.com;Lisboa;Portuguesa;Masculino;Licenciatura;Identificacao Civil (CC/BI)
```

---

## Expanding to New Entities

To implement bulk import for a new entity, follow these steps:

### 1. Create Specific Interface

```csharp
// Core/[Entity]/BulkImport/Services/I[Entity]BulkImportService.cs
public interface ICompanyBulkImportService :
    IBulkImportService<CreateCompanyDto, Company, RetrieveCompanyDto>
{
}
```

### 2. Implement Service

```csharp
// Core/[Entity]/BulkImport/Services/[Entity]BulkImportService.cs
public class CompanyBulkImportService : ICompanyBulkImportService
{
    // Define required headers
    private readonly List<string> RequiredHeaders = new()
    {
        "Designacao", "NIF"
    };

    // Define optional headers
    private readonly List<string> OptionalHeaders = new()
    {
        "Morada", "Telefone", "Localidade", "CodigoPostal", "Email",
        "SetorAtividade", "Dimensao"
    };

    // Implement ImportFromFileAsync
    // Implement MapRowToDto
    // Implement ValidateDto
    // Implement GetTemplateFileAsync
    // Implement ValidateImportAsync
}
```

### 3. Create Controller

```csharp
// Core/[Entity]/Controllers/[Entity]ImportController.cs
[Route("api/companies/import")]
[ApiController]
public class CompanyImportController : ControllerBase
{
    // Inject ICompanyBulkImportService
    // Implement CSV/Excel endpoints
    // Implement validate endpoints
    // Implement template endpoints
}
```

### 4. Register Service

```csharp
// Program.cs
builder.Services.AddScoped<ICompanyBulkImportService, CompanyBulkImportService>();
```

### 5. Create Templates

Create files in `wwwroot/templates/`:
- `Company_Import_Template.csv`
- `Company_Import_Template.xlsx`

### 6. Implement Frontend

```typescript
// core/services/companies.service.ts
importCompaniesFromCsv(file: File): Observable<any> {
  const formData = new FormData();
  formData.append('file', file);
  return this.http.post<any>(
    `${API_ENDPOINTS.companies}import/csv`,
    formData
  );
}
```

### Complete Example: Company Import

File structure to create:

```
Core/Companies/
├── BulkImport/
│   └── Services/
│       ├── ICompanyBulkImportService.cs
│       └── CompanyBulkImportService.cs
└── Controllers/
    └── CompanyImportController.cs

wwwroot/templates/
├── Company_Import_Template.csv
└── Company_Import_Template.xlsx
```

---

## Best Practices

### Performance

1. **Pre-load existing data** - Before processing, load existing NIFs/emails for efficient validation
2. **Batch processing** - Use appropriate `BatchSize` (50-200 records)
3. **HashSets for uniqueness** - Use HashSet to check duplicates quickly

### Validation

1. **Validate structure first** - Fail early if headers are incorrect
2. **Validate duplicates in file** - Add to HashSet after each valid row
3. **Clear error messages** - Include field, attempted value, and descriptive message

### Security

1. **Size limit** - Use `[RequestSizeLimit]` (10MB recommended)
2. **Authorization** - Protect endpoints with `[Authorize]`
3. **Validate extensions** - Check file extension before processing

### UX

1. **Pre-filled templates** - Include examples in templates
2. **Modal instructions** - Document allowed values
3. **Detailed results** - Show errors per row with context

---

## References

### Source Code

| Component | Location |
|-----------|----------|
| Base interface | `Shared/BulkImport/Services/IBulkImportService.cs` |
| Result models | `Shared/BulkImport/Models/BulkImportResult.cs` |
| CSV parser | `Shared/BulkImport/Services/CsvParserService.cs` |
| Excel parser | `Shared/BulkImport/Services/ExcelParserService.cs` |
| People service | `Core/People/BulkImport/Services/PeopleBulkImportService.cs` |
| People controller | `Core/People/Controllers/PeopleImportController.cs` |
| People frontend | `features/people/import-people/` |
| Templates | `wwwroot/templates/` |

### Libraries Used

| Library | Purpose |
|---------|---------|
| CsvHelper | CSV file parsing |
| ClosedXML | Excel file parsing |
| Humanizer | Enum to readable text conversion |

### Related Documentation

> See: [Entity Validations](./VALIDATIONS.md)

> See: [Business Logic](./BUSINESS_LOGIC.md)

> Ref: [CsvHelper Documentation](https://joshclose.github.io/CsvHelper/)

> Ref: [ClosedXML Documentation](https://closedxml.github.io/ClosedXML/)
