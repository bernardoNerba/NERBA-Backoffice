# Global Error Handling

This document describes the global error handling strategy implemented in the NERBA Backoffice system.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Exception Middleware](#exception-middleware)
- [Custom Exception Types](#custom-exception-types)
- [Exception to HTTP Mapping](#exception-to-http-mapping)
- [Error Response Format](#error-response-format)
- [Logging Integration](#logging-integration)
- [Result Pattern](#result-pattern)
- [Adding New Exceptions](#adding-new-exceptions)
- [Common Error Scenarios](#common-error-scenarios)
- [Best Practices](#best-practices)
- [References](#references)

---

## Overview

The system uses a centralized approach for error handling through ASP.NET Core middleware. This strategy ensures consistent and standardized responses for all application errors.

### Characteristics

| Feature | Description |
|---------|-------------|
| Approach | Centralized middleware |
| Response format | ProblemDetails (RFC 7807) |
| Serialization | JSON (camelCase) |
| Logging | Integrated ILogger |
| Environment | Different behavior for Dev/Prod |

### Error Handling Flow

```
                    ┌─────────────┐
                    │   Request   │
                    └──────┬──────┘
                           │
                           ▼
              ┌─────────────────────────┐
              │  GlobalExceptionHandler │
              │       Middleware        │
              └───────────┬─────────────┘
                          │
                    ┌─────┴─────┐
                    │try { ... }│
                    └─────┬─────┘
                          │
                          ▼
              ┌────────────────────────┐
              │    Request Pipeline    │
              │  (Controllers, etc.)   │
              └───────────┬────────────┘
                          │
           ┌──────────────┼
           │              │              
        Success       Exception         
           │              │              
           ▼              ▼              
    ┌──────────┐   ┌────────────┐       
    │ Normal   │   │catch(ex)   │       
    │ Response │   └─────┬──────┘       
    └──────────┘         │              
                         ▼              
                  ┌────────────┐        
                  │ Log error  │        
                  └─────┬──────┘        
                        │               
                        ▼               
                  ┌────────────┐        
                  │ Map to     │        
                  │ HTTP Status│        
                  └─────┬──────┘        
                        │               
                        ▼              
                  ┌────────────────┐    
                  │ ProblemDetails │    
                  │   Response     │    
                  └────────────────┘    
```

---

## Architecture

### Main Components

```
Shared/
├── Middleware/
│   ├── GlobalExceptionHandlerMiddleware.cs   # Central exception handling
│   └── TokenBlacklistMiddleware.cs           # Token validation
├── Exceptions/
│   ├── ObjectNullException.cs                # Resource not found
│   └── ValidationException.cs                # Validation error
├── Models/
│   └── Result.cs                             # Result pattern for responses
└── Services/
    ├── IResponseHandler.cs                   # Response interface
    └── ResponseHandler.cs                    # Result -> IActionResult conversion
```

### Middleware Order

The registration order in the pipeline is critical for correct operation.

```csharp
// Program.cs - Middleware order
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TokenBlacklistMiddleware>();  // Token verification
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();  // Error handling
app.MapControllers();
```

> See: `NERBABO.Backend/NERBABO.ApiService/Program.cs` (lines 489-496)

---

## Exception Middleware

### GlobalExceptionHandlerMiddleware

The central middleware implements `IMiddleware` and wraps the entire request pipeline in a try-catch block.

```csharp
public class GlobalExceptionHandlerMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly JsonSerializerOptions _serializer;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }
}
```

### DI Container Registration

```csharp
// Program.cs
builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();
```

> See: `Shared/Middleware/GlobalExceptionHandlerMiddleware.cs`

---

## Custom Exception Types

The system defines custom exceptions for specific domain scenarios.

### ObjectNullException

Used when a requested resource does not exist.

```csharp
public class ObjectNullException : Exception
{
    public ObjectNullException(string message) : base(message) { }
    public ObjectNullException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

**Usage:**

```csharp
var action = await _context.Actions.FindAsync(actionId)
    ?? throw new ObjectNullException("Action not found.");
```

> See: `Shared/Exceptions/ObjectNullException.cs`

### ValidationException

Used for business rule validation errors.

```csharp
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

> See: `Shared/Exceptions/ValidationException.cs`

---

## Exception to HTTP Mapping

The middleware maps exception types to appropriate HTTP status codes.

| Exception Type | HTTP Status | Title |
|----------------|-------------|-------|
| `KeyNotFoundException` | 404 Not Found | Resource not found |
| `ObjectNullException` | 404 Not Found | Resource not found |
| `InvalidOperationException` | 400 Bad Request | Invalid operation |
| `ArgumentNullException` | 400 Bad Request | Invalid parameters |
| `ArgumentException` | 400 Bad Request | Invalid parameters |
| `UnauthorizedAccessException` | 401 Unauthorized | Unauthorized access |
| `ValidationException` | 400 Bad Request | Validation error |
| `Exception` (default) | 500 Internal Server Error | Internal server error |

### Mapping Implementation

```csharp
switch (exception)
{
    case KeyNotFoundException or ObjectNullException:
        response.StatusCode = StatusCodes.Status404NotFound;
        problemDetails.Title = "Recurso nao encontrado.";
        break;

    case InvalidOperationException:
        response.StatusCode = StatusCodes.Status400BadRequest;
        problemDetails.Title = "Operacao invalida.";
        break;

    case ArgumentNullException or ArgumentException:
        response.StatusCode = StatusCodes.Status400BadRequest;
        problemDetails.Title = "Parametros invalidos.";
        break;

    case UnauthorizedAccessException:
        response.StatusCode = StatusCodes.Status401Unauthorized;
        problemDetails.Title = "Acesso nao autorizado.";
        break;

    case ValidationException validationEx:
        response.StatusCode = StatusCodes.Status400BadRequest;
        problemDetails.Title = "Erro de validacao.";
        break;

    default:
        response.StatusCode = StatusCodes.Status500InternalServerError;
        problemDetails.Title = "Erro interno do servidor.";
        break;
}
```

---

## Error Response Format

All error responses follow the **ProblemDetails** format (RFC 7807).

### Base Structure

```json
{
  "title": "Recurso nao encontrado.",
  "detail": "Action not found.",
  "status": 404
}
```

### Development Environment Response

In Development mode, additional debug information is included.

```json
{
  "title": "Erro interno do servidor.",
  "detail": "Object reference not set to an instance of an object.",
  "status": 500,
  "stackTrace": "at NERBABO.ApiService.Core..."
}
```

### Conditional Implementation

```csharp
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    problemDetails.Detail = exception.Message;
    problemDetails.Extensions["StackTrace"] = exception.StackTrace;
}
```

### Model State Validation

Model validation errors (Data Annotations) return a slightly different format.

```json
{
  "title": "Erro de Validacao",
  "status": 400,
  "errors": [
    "The NIF field is required.",
    "Email must be valid."
  ]
}
```

> See: `Program.cs` (lines 319-343) - `ApiBehaviorOptions`

---

## Logging Integration

The middleware uses `ILogger` to record all exceptions.

### Log Levels by Type

| Exception Type | Log Level |
|----------------|-----------|
| `KeyNotFoundException` | Warning |
| `ObjectNullException` | Warning |
| `InvalidOperationException` | Warning |
| `ArgumentException` | Warning |
| `UnauthorizedAccessException` | Warning |
| `ValidationException` | Warning |
| Others (default) | Error |

### Log Format

```csharp
// Known exceptions (Warning)
_logger.LogWarning(exception, "Resource not found: {Message}", exception.Message);

// Unexpected exceptions (Error)
_logger.LogError(exception, "Unexpected error occurred: {Message}", exception.Message);
```

---

## Result Pattern

In addition to exception handling, the system uses the **Result** pattern for flow control in service operations.

### Result Classes

```csharp
// Result without data
public class Result
{
    public bool Success { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public int? StatusCode { get; set; }
    public object? Errors { get; set; }
}

// Result with data
public class Result<T> : Result
{
    public T? Data { get; set; }
}
```

### Usage in Services

```csharp
public async Task<Result<RetrieveCompanyDto>> GetByIdAsync(long id)
{
    var company = await _context.Companies.FindAsync(id);

    if (company is null)
        return Result<RetrieveCompanyDto>.Fail(
            "Not found.",
            "Company not found.",
            StatusCodes.Status404NotFound);

    return Result<RetrieveCompanyDto>.Ok(retrieveCompany);
}
```

### ResponseHandler

The `ResponseHandler` converts `Result` to `IActionResult` with `ProblemDetails`.

```csharp
public IActionResult HandleResult<T>(Result<T> result)
{
    if (!result.Success)
    {
        var problemDetails = new ProblemDetails
        {
            Title = result.Title,
            Detail = result.Message,
            Status = result.StatusCode,
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = result.StatusCode ?? StatusCodes.Status400BadRequest
        };
    }

    return new ObjectResult(result.Data)
    {
        StatusCode = result.StatusCode ?? StatusCodes.Status200OK
    };
}
```

> See: `Shared/Models/Result.cs` and `Shared/Services/ResponseHandler.cs`

---

## Adding New Exceptions

To add a new custom exception type, follow these steps.

### 1. Create the Exception Class

```csharp
// Shared/Exceptions/BusinessRuleException.cs
namespace NERBABO.ApiService.Shared.Exceptions
{
    public class BusinessRuleException : Exception
    {
        public string RuleCode { get; }

        public BusinessRuleException(string message, string ruleCode)
            : base(message)
        {
            RuleCode = ruleCode;
        }

        public BusinessRuleException(string message, string ruleCode, Exception innerException)
            : base(message, innerException)
        {
            RuleCode = ruleCode;
        }
    }
}
```

### 2. Add to Middleware

```csharp
// GlobalExceptionHandlerMiddleware.cs
switch (exception)
{
    // ... existing cases ...

    case BusinessRuleException businessEx:
        _logger.LogWarning(exception, "Business rule violation: {Message}", exception.Message);
        response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        problemDetails.Title = "Business rule violation.";
        problemDetails.Detail = businessEx.Message;
        problemDetails.Extensions["ruleCode"] = businessEx.RuleCode;
        break;

    // ... default ...
}
```

### 3. Use in Code

```csharp
if (student.EnrollmentDate > action.EndDate)
{
    throw new BusinessRuleException(
        "Cannot enroll student after action end date.",
        "ENROLLMENT_AFTER_END"
    );
}
```

---

## Common Error Scenarios

### Resource Not Found

```csharp
// Option 1: Use exception
var person = await _context.People.FindAsync(id)
    ?? throw new ObjectNullException("Person not found.");

// Option 2: Use Result
if (person is null)
    return Result<PersonDto>.Fail(
        "Not found",
        "Person not found.",
        StatusCodes.Status404NotFound);
```

### Business Validation

```csharp
// Option 1: Use ValidationException
if (!IsValidNIF(dto.NIF))
    throw new ValidationException("Invalid NIF.");

// Option 2: Use Result with errors
return Result<PersonDto>.Fail(
    "Validation error",
    "Invalid data.",
    new[] { "Invalid NIF", "Duplicate email" },
    StatusCodes.Status400BadRequest);
```

### Invalid Operation

```csharp
if (action.Status == ActionStatus.Completed)
    throw new InvalidOperationException(
        "Cannot edit a completed action.");
```

### Unauthorized Access

```csharp
if (!await IsUserAuthorizedForAction(userId, actionId))
    throw new UnauthorizedAccessException(
        "User does not have permission for this operation.");
```

---

## Best Practices

### When to Use Exceptions vs Result

| Scenario | Recommendation |
|----------|----------------|
| Unrecoverable error | Exception |
| Expected business validation | Result.Fail() |
| Resource not found | Both valid |
| System/infrastructure error | Exception |
| Controlled flow with multiple errors | Result.Fail() with Errors |

### Error Messages

1. **Be specific** - Clearly indicate the problem
2. **Internationalization** - Messages in Portuguese for end users
3. **Don't expose internal details** - In production, hide stack traces
4. **Include context** - When appropriate, include relevant IDs or values

### Security

1. **Don't reveal sensitive information** - Avoid messages that expose internal structure
2. **Sanitize input** - Don't include user input directly in messages
3. **Differentiate environments** - Less detail in production

### Performance

1. **Avoid exceptions for flow control** - Use Result for expected scenarios
2. **Appropriate logging** - Warning for expected errors, Error for unexpected
3. **Don't catch and re-throw** - Let propagate to middleware

---

## References

### Source Code

| Component | Location |
|-----------|----------|
| GlobalExceptionHandlerMiddleware | `Shared/Middleware/GlobalExceptionHandlerMiddleware.cs` |
| TokenBlacklistMiddleware | `Shared/Middleware/TokenBlacklistMiddleware.cs` |
| ObjectNullException | `Shared/Exceptions/ObjectNullException.cs` |
| ValidationException | `Shared/Exceptions/ValidationException.cs` |
| Result | `Shared/Models/Result.cs` |
| ResponseHandler | `Shared/Services/ResponseHandler.cs` |
| Pipeline Configuration | `Program.cs` |

### External Documentation

> Ref: [RFC 7807 - Problem Details for HTTP APIs](https://datatracker.ietf.org/doc/html/rfc7807)

> Ref: [ASP.NET Core Error Handling](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)

> Ref: [ASP.NET Core Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware)

### Related Documentation

> See: [Authentication and Authorization](./AUTHENTICATION_AUTHORIZATION.md)

> See: [Entity Validations](./VALIDATIONS.md)

> See: [Business Logic](./BUSINESS_LOGIC.md)
