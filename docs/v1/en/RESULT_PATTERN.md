# Result Pattern and Response Handler

This document describes the Result pattern implemented in the NERBA Backoffice system for managing responses between services and controllers.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Result Classes](#result-classes)
- [Response Handler](#response-handler)
- [Data Flow](#data-flow)
- [Status Code Mapping](#status-code-mapping)
- [Practical Examples](#practical-examples)
- [Implementing in a New Feature](#implementing-in-a-new-feature)
- [Best Practices](#best-practices)
- [References](#references)

---

## Overview

The Result pattern encapsulates the outcome of business operations, separating domain logic from HTTP response transformation. This pattern provides:

| Benefit | Description |
|---------|-------------|
| Separation of concerns | Services return business results, controllers transform to HTTP |
| Consistency | Uniform response format across the entire API |
| Strong typing | Errors and successes are typed, avoiding exceptions for expected flows |
| Testability | Services can be tested independently of HTTP context |

### Core Principle

```
Services SHOULD NOT know about IActionResult
Controllers SHOULD NOT contain business logic
```

---

## Architecture

### Main Components

```
Shared/
├── Models/
│   ├── Result.cs         # Result and Result<T> classes
│   └── OkMessage.cs      # Wrapper for success responses
└── Services/
    ├── IResponseHandler.cs   # Transformation interface
    └── ResponseHandler.cs    # Implementation
```

### Flow Diagram

![alt text](../modeling/result_pattern_high_diagram.png)

---

## Result Classes

### Result (Base)

Non-generic class for operations without return data.

```csharp
public class Result
{
    public bool Success { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public int? StatusCode { get; set; }
    public object? Errors { get; set; }
}
```

#### Static Methods

| Method | Description |
|--------|-------------|
| `Ok(title, message, status?)` | Creates success result (2xx) |
| `Fail(title, message, statusCode?)` | Creates failure result |
| `Fail(title, message, errors, statusCode?)` | Creates failure result with details |

#### Usage Examples

```csharp
// Simple success
return Result.Ok("Person Deleted.", "Person deleted successfully.");

// Success with custom status
return Result.Ok("Resource Created.", "Operation completed.", StatusCodes.Status201Created);

// Failure with default status (400)
return Result.Fail("Validation Error.", "NIF must be unique.");

// Failure with specific status
return Result.Fail("Not Found.", "Person not found.", StatusCodes.Status404NotFound);
```

### Result&lt;T&gt; (Generic)

Generic class that inherits from Result, adding a data payload.

```csharp
public class Result<T> : Result
{
    public T? Data { get; set; }
}
```

#### Static Methods

| Method | Description |
|--------|-------------|
| `Ok(data)` | Creates result with data only |
| `Ok(data, title, message)` | Creates result with data and messages |
| `Ok(data, title, message, status)` | Creates complete result |
| `Fail(title, message, statusCode?)` | Creates failure result |
| `Fail(title, message, errors, statusCode?)` | Creates failure result with details |

#### Usage Examples

```csharp
// Success with data only (status 200)
return Result<IEnumerable<RetrievePersonDto>>.Ok(people);

// Success with data and messages
return Result<RetrievePersonDto>.Ok(
    person,
    "Person Updated.",
    $"Person {person.FullName} was updated.");

// Success with creation (status 201)
return Result<RetrievePersonDto>.Ok(
    createdPerson,
    "Person Created.",
    "The person was registered in the system.",
    StatusCodes.Status201Created);

// Validation failure
return Result<RetrievePersonDto>.Fail(
    "Validation Error.",
    "Person NIF must be unique. Already exists in the system.");

// Failure with specific status
return Result<RetrievePersonDto>.Fail(
    "Not Found.",
    "Person not found.",
    StatusCodes.Status404NotFound);
```

---

## Response Handler

The `ResponseHandler` transforms `Result` objects into `IActionResult` for HTTP response.

### Interface

```csharp
public interface IResponseHandler
{
    IActionResult HandleResult<T>(Result<T> result);
    IActionResult HandleResult(Result result);
}
```

### Transformation Logic

#### For Failure Results

Returns `ProblemDetails` (RFC 7807) with the appropriate status code:

```csharp
var problemDetails = new ProblemDetails
{
    Title = result.Title,
    Detail = result.Message,
    Status = result.StatusCode,
};

if (result.Errors is not null)
{
    problemDetails.Extensions["errors"] = result.Errors;
}

return new ObjectResult(problemDetails)
{
    StatusCode = result.StatusCode ?? StatusCodes.Status400BadRequest
};
```

#### For Success Results with Messages

Returns `OkMessage<T>` wrapping the data:

```csharp
var okResult = new OkMessage<T>(
    result.Title,
    result.Message,
    result.Data,
    result.StatusCode
);

return new ObjectResult(okResult)
{
    StatusCode = result.StatusCode ?? StatusCodes.Status200OK
};
```

#### For Success Results without Messages

Returns only the data:

```csharp
return new ObjectResult(result.Data)
{
    StatusCode = result.StatusCode ?? StatusCodes.Status200OK
};
```

### OkMessage Model

```csharp
public class OkMessage<T>
{
    public int? StatusCode { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
}
```

---

## Data Flow

### Scenario: Create Person

```
1. Controller receives CreatePersonDto
        │
        ▼
2. Service validates data
        │
        ├── Duplicate NIF? ──► Result<T>.Fail("Validation Error.", ...)
        │
        ├── Invalid enum? ──► Result<T>.Fail("Not Found.", ..., 404)
        │
        ▼
3. Service persists entity
        │
        ▼
4. Service returns Result<T>.Ok(person, "Person Created.", ..., 201)
        │
        ▼
5. Controller calls _responseHandler.HandleResult(result)
        │
        ▼
6. ResponseHandler transforms to IActionResult
        │
        ├── If Fail ──► ProblemDetails with status code
        │
        └── If Ok ──► OkMessage<T> or raw data
```

### Scenario: Delete Person

```
1. Controller receives id
        │
        ▼
2. Service checks existence
        │
        ├── Doesn't exist? ──► Result.Fail("Not Found.", ..., 404)
        │
        ├── Is a user? ──► Result.Fail("Validation Error.", ...)
        │
        ▼
3. Service removes entity
        │
        ▼
4. Service returns Result.Ok("Person Deleted.", ...)
        │
        ▼
5. Controller calls _responseHandler.HandleResult(result)
        │
        ▼
6. ResponseHandler returns { Title, Message } with status 200
```

---

## Status Code Mapping

### Common Status Codes

| Scenario | Status Code | Result Method |
|----------|-------------|---------------|
| Generic success | 200 OK | `Result.Ok()` or `Result<T>.Ok(data)` |
| Resource created | 201 Created | `Result<T>.Ok(data, ..., 201)` |
| Validation error | 400 Bad Request | `Result.Fail()` (default) |
| Unauthorized | 401 Unauthorized | `Result.Fail(..., 401)` |
| Not found | 404 Not Found | `Result.Fail(..., 404)` |
| Internal error | 500 Internal Server Error | `Result.Fail(..., 500)` |

### Project Conventions

| Error Type | Title | Status |
|------------|-------|--------|
| Uniqueness validation | "Validation Error." | 400 |
| Entity not found | "Not Found." | 404 |
| Invalid enum | "Not Found." | 404 |
| Business constraint | "Validation Error." | 400 |
| File failure | "Internal Error." | 500 |

---

## Practical Examples

### Service: Read Operation

```csharp
public async Task<Result<RetrievePersonDto>> GetByIdAsync(long id)
{
    // Check cache
    var cachedPerson = await _cache.GetSinglePersonCacheAsync(id);
    if (cachedPerson is not null)
        return Result<RetrievePersonDto>.Ok(cachedPerson);

    // Fetch from database
    var existingPerson = await _context.People
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == id);

    if (existingPerson is null)
        return Result<RetrievePersonDto>
            .Fail("Not Found.", "Person not found.",
            StatusCodes.Status404NotFound);

    var retrievePerson = Person.ConvertEntityToRetrieveDto(existingPerson);

    // Update cache
    await _cache.SetSinglePersonCacheAsync(retrievePerson);

    return Result<RetrievePersonDto>.Ok(retrievePerson);
}
```

### Service: Create Operation

```csharp
public async Task<Result<RetrieveSessionDto>> CreateAsync(CreateSessionDto entityDto)
{
    // Validate relationships
    var moduleTeaching = await _context.ModuleTeachings.FindAsync(entityDto.ModuleTeachingId);
    if (moduleTeaching is null)
        return Result<RetrieveSessionDto>
            .Fail("Not Found.", "Teacher-Module relationship not found.",
            StatusCodes.Status404NotFound);

    // Validate business rules
    if (!action.IsActionActive)
        return Result<RetrieveSessionDto>
            .Fail("Validation Error.",
            "Cannot create session when action is inactive.");

    // Validate enums
    if (!EnumHelp.IsValidEnum<WeekDaysEnum>(entityDto.Weekday))
        return Result<RetrieveSessionDto>
            .Fail("Validation Error.", "The weekday entered is not valid.");

    // Create entity
    var createdEntity = _context.Sessions.Add(Session.ConvertCreateDtoToEntity(entityDto));
    await _context.SaveChangesAsync();

    var retrieveSession = Session.ConvertEntityToRetrieveDto(createdEntity.Entity);

    return Result<RetrieveSessionDto>
        .Ok(retrieveSession, "Session Created.", "The session was scheduled successfully.");
}
```

### Service: Delete Operation

```csharp
public async Task<Result> DeleteAsync(long id)
{
    var existingSession = await _context.Sessions.FindAsync(id);
    if (existingSession is null)
        return Result
            .Fail("Not Found.", "Session not found.",
            StatusCodes.Status404NotFound);

    // Validate constraints
    if (existingSession.TeacherPresence.Equals(PresenceEnum.Present))
        return Result
            .Fail("Validation Error.", "Session was already taught, cannot delete.");

    _context.Sessions.Remove(existingSession);
    await _context.SaveChangesAsync();

    return Result.Ok("Session Deleted.", "Session deleted successfully.");
}
```

### Controller: Standard Usage

```csharp
[HttpGet("{id:long}")]
[Authorize(Policy = "ActiveUser")]
public async Task<IActionResult> GetPersonAsync(long id)
{
    Result<RetrievePersonDto> result = await _peopleService.GetByIdAsync(id);
    return _responseHandler.HandleResult(result);
}

[HttpPost("create")]
[Authorize(Policy = "ActiveUser")]
public async Task<IActionResult> CreatePersonAsync([FromBody] CreatePersonDto person)
{
    Result<RetrievePersonDto> result = await _peopleService.CreateAsync(person);
    return _responseHandler.HandleResult(result);
}

[HttpDelete("delete/{id:long}")]
[Authorize(Policy = "ActiveUser")]
public async Task<IActionResult> DeletePersonAsync(long id)
{
    Result result = await _peopleService.DeleteAsync(id);
    return _responseHandler.HandleResult(result);
}
```

### Controller: Special Case (File Downloads)

When the result is a file, the controller handles it manually:

```csharp
[HttpGet("{id:long}/habilitation-pdf")]
[Authorize(Policy = "ActiveUser")]
public async Task<IActionResult> DownloadHabilitationPdfAsync(long id)
{
    var result = await _peopleService.GetHabilitationPdfAsync(id);

    // Use handler for errors
    if (!result.Success)
        return _responseHandler.HandleResult(result);

    // Return file for success
    var fileResult = result.Data!;
    return File(fileResult.Content, "application/pdf", fileResult.FileName);
}
```

---

## Implementing in a New Feature

### 1. Define Service Interface

```csharp
public interface ICompanyService
{
    Task<Result<IEnumerable<RetrieveCompanyDto>>> GetAllAsync();
    Task<Result<RetrieveCompanyDto>> GetByIdAsync(long id);
    Task<Result<RetrieveCompanyDto>> CreateAsync(CreateCompanyDto entityDto);
    Task<Result<RetrieveCompanyDto>> UpdateAsync(UpdateCompanyDto entityDto);
    Task<Result> DeleteAsync(long id);
}
```

### 2. Implement Service

```csharp
public class CompanyService : ICompanyService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(AppDbContext context, ILogger<CompanyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<RetrieveCompanyDto>> CreateAsync(CreateCompanyDto entityDto)
    {
        // Uniqueness validation
        if (await _context.Companies.AnyAsync(c => c.NIF == entityDto.NIF))
        {
            _logger.LogWarning("Duplicated NIF detected: {NIF}", entityDto.NIF);
            return Result<RetrieveCompanyDto>
                .Fail("Validation Error.", "Company NIF must be unique.");
        }

        // Creation
        var entity = Company.ConvertCreateDtoToEntity(entityDto);
        var created = _context.Companies.Add(entity);
        await _context.SaveChangesAsync();

        var retrieveDto = Company.ConvertEntityToRetrieveDto(created.Entity);

        return Result<RetrieveCompanyDto>
            .Ok(retrieveDto, "Company Created.",
                $"Company {retrieveDto.Name} was created.",
                StatusCodes.Status201Created);
    }

    // ... other methods
}
```

### 3. Implement Controller

```csharp
[Route("api/[controller]")]
[ApiController]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IResponseHandler _responseHandler;

    public CompaniesController(
        ICompanyService companyService,
        IResponseHandler responseHandler)
    {
        _companyService = companyService;
        _responseHandler = responseHandler;
    }

    [HttpPost("create")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> CreateCompanyAsync([FromBody] CreateCompanyDto company)
    {
        Result<RetrieveCompanyDto> result = await _companyService.CreateAsync(company);
        return _responseHandler.HandleResult(result);
    }
}
```

### 4. Register in DI Container

```csharp
// Program.cs
builder.Services.AddScoped<ICompanyService, CompanyService>();
```

---

## Best Practices

### Services

1. **Validate early** - Return `Fail` as soon as an invalid condition is detected
2. **User-friendly messages** - Use clear messages for the end user
3. **Log before Fail** - Log technical details before returning errors
4. **Appropriate status codes** - Use 404 for "not found", 400 for validation

### Controllers

1. **Delegate to service** - Never contain business logic
2. **Use ResponseHandler** - Always transform Result with the handler
3. **Special cases** - Handle downloads and streams manually
4. **Document endpoints** - Use XML comments for Swagger

### Error Messages

| Type | Title | Example Message |
|------|-------|-----------------|
| Entity doesn't exist | "Not Found." | "Person not found." |
| Duplicate | "Validation Error." | "NIF already exists in the system." |
| Business constraint | "Validation Error." | "Cannot delete a person who is a user." |
| Invalid enum | "Not Found." | "Gender not found." |

### Avoid

- Throwing exceptions for expected flows (use `Result.Fail`)
- Returning `IActionResult` directly from service
- Mixing business logic in controller
- Using inconsistent status codes for the same error type

---

## References

### Source Code

| Component | Location |
|-----------|----------|
| Result.cs | `Shared/Models/Result.cs` |
| OkMessage.cs | `Shared/Models/OkMessage.cs` |
| IResponseHandler | `Shared/Services/IResponseHandler.cs` |
| ResponseHandler | `Shared/Services/ResponseHandler.cs` |

### Project Examples

| Entity | Service | Controller |
|--------|---------|------------|
| People | `Core/People/Services/PeopleService.cs` | `Core/People/Controllers/PeopleController.cs` |
| Sessions | `Core/Sessions/Services/SessionService.cs` | `Core/Sessions/Controllers/SessionsController.cs` |
| Notifications | `Core/Notifications/Services/NotificationService.cs` | `Core/Notifications/Controllers/NotificationController.cs` |
| Auth | `Core/Authentication/Services/JwtService.cs` | `Core/Authentication/Controllers/AuthController.cs` |

### Related Documentation

> See: [Entity Validations](./VALIDATIONS.md)

> See: [Business Logic](./BUSINESS_LOGIC.md)

> See: [Side Caching](./CACHING.md)

### External References

> Ref: [RFC 7807 - Problem Details](https://datatracker.ietf.org/doc/html/rfc7807)

> Ref: [Result Pattern (Microsoft)](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/implement-value-objects)
