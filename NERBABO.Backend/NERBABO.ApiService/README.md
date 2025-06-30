## Using ZLinq

You can knwow more about ZLinq at https://github.com/Cysharp/ZLinq, basically is a high performance wrapper for the normal C# LINQ. To use, any time you want to use normal linq, use `.AsValueEnumerable()` method before, like this:

```C#
using ZLinq;

var seq = source
    .AsValueEnumerable() // only add this line
    .Where(x => x % 2 == 0)
    .Select(x => x * 3);

foreach (var item in seq)
{
    // DO SOMETHING...
}
```

## API Documentation

Follow this example as a pattern for documenting api endpoints:
```c#
/// <summary>
/// Creates a TodoItem.
/// </summary>
/// <param name="item"></param>
/// <returns>A newly created TodoItem</returns>
/// <remarks>
/// 
///
/// </remarks>
/// <response code="201">Returns the newly created item</response>
/// <response code="400">If the item is null</response>
[HttpPost]
[ProducesResponseType(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> CreateTodoItemAsync(TodoItem item)
{
    //..
}
```

## Modifiying database

Always that one Service handles more than one critical database operation, use transactions:

```C#
var transaction = await _context.Database.BeginTransactionAsync();

try
{
    // do operations to database
    await transaction.CommitAsync();
} catch (Exception)
{
    transaction.Rollback();
}
```

## Using Humanizer Pt

Humanizer meets all your .NET needs for manipulating and displaying strings, enums, dates, times, timespans, numbers and quantities. Please refer to https://github.com/Humanizr/Humanizer

## Global Exceptions Handler

Global Exceptions Handler The GlobalExceptionHandlerMiddleware is a centralized error-handling component that implements the IMiddleware interface. It intercepts unhandled exceptions that occur during the request pipeline and converts them into standardized API responses. This approach promotes cleaner, more maintainable, and scalable code by separating error-handling logic from business logic.

Purpose Centralized exception handling across the application.

Consistent error responses to clients.

Improved logging and diagnostics.

Facilitates environment-aware error reporting (e.g., hiding stack traces in production).

Exception Handling Logic The middleware processes exceptions based on their type and maps them to appropriate HTTP status codes and user-friendly messages:

| Exception Type | HTTP Status Code | Message | Description |
| --- | --- | --- | --- |
| `KeyNotFoundException`, `ObjectNullException` | `404 Not Found` | `"Recurso não encontrado."` | Used when a requested resource does not exist. |
| `InvalidOperationException` | `400 Bad Request` | `"Operação inválida."` | Indicates that the operation is not allowed in the current context. |
| `ArgumentNullException`, `ArgumentException` | `400 Bad Request` | `"Parâmetros inválidos."` | Thrown when an invalid or null parameter is passed. |
| `UnauthorizedAccessException` | `401 Unauthorized` | `"Acesso não autorizado."` | Used when the request lacks valid authentication credentials. |
| `ValidationException` | `400 Bad Request` | `"Erro de validação."` | Indicates one or more validation errors occurred. |
| _(Default case — all other exceptions)_ | `500 Internal Server Error` | `"Erro interno do servidor."` | Catch-all for unexpected errors. Detailed information is included **only in development** environments. |

Response Format Each handled exception returns a structured JSON response typically containing:

```json
{
  "message": "Erro de validação.",
  "details": "The field 'Email' is required.",
  "stackTrace": "..." // Included only in Development
}
```
