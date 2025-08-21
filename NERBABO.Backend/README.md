# NERBA Backend

This is the backend portion of the NERBA Backoffice system, built as a distributed application using .NET 9 and .NET Aspire for orchestration. The backend manages all business logic for training and qualification management, user authentication, and data persistence.

## Architecture Overview

The NERBA Backend consists of three main projects:

### 1. NERBABO.ApiService

The main REST API service providing all business functionality:

- **Authentication & Authorization**: JWT-based authentication with role-based access control
- **Training Management**: Courses, modules, sessions, and actions management
- **People Management**: Students, teachers, and general people records
- **Company Management**: Business entities and relationships
- **Reports**: PDF generation for training reports using QuestPDF
- **File Management**: Image upload and storage for frames
- **Global Configuration**: Tax settings and general application info

### 2. NERBABO.AppHost

.NET Aspire orchestrator that manages the entire application stack:

- **Database**: PostgreSQL with pgAdmin for management
- **Caching**: Redis with RedisInsight for monitoring
- **API Service**: ASP.NET Core Web API
- **Frontend**: Angular application integration
- **Development Tools**: Hot reload and file watching

### 3. NERBABO.ServiceDefaults

Shared configuration and common services:

- **OpenTelemetry**: Distributed tracing and metrics
- **Health Checks**: Application health monitoring
- **Service Discovery**: Automatic service resolution
- **Resilience**: HTTP client retry policies and circuit breakers

## Core Entities

The system manages the following core business entities:

- **User**: Authentication and account management
- **Person**: Base entity for individuals (students, teachers, contacts)
- **Student**: Training participants with enrollment tracking
- **Teacher**: Instructors with qualifications and module assignments
- **Company**: Business entities for corporate training
- **Course**: Training programs with modules and sessions
- **Module**: Individual learning units within courses
- **Session**: Scheduled training sessions with attendance tracking
- **ModuleTeaching**: Teacher assignments to specific modules
- **CourseAction**: Administrative actions on courses
- **Frame**: Document templates and imagery
- **Tax**: Financial configuration and tax settings
- **GeneralInfo**: Application-wide settings and branding

## Project Structure

```text
NERBABO.Backend/
├── NERBABO.ApiService/           # Main REST API
│   ├── Core/                     # Business logic organized by domain
│   │   ├── Account/              # User management
│   │   ├── Authentication/       # JWT and authorization
│   │   ├── Companies/            # Company management
│   │   ├── Courses/              # Course management
│   │   ├── Modules/              # Module management
│   │   ├── Sessions/             # Session scheduling
│   │   ├── Students/             # Student management
│   │   ├── Teachers/             # Teacher management
│   │   ├── People/               # General people management
│   │   ├── Reports/              # PDF generation
│   │   ├── Frames/               # Document templates
│   │   └── Global/               # Application settings
│   ├── Data/                     # Entity Framework Core
│   │   ├── Configurations/       # Entity configurations
│   │   └── Migrations/           # Database migrations
│   ├── Helper/                   # Utility classes and validators
│   └── Shared/                   # Cross-cutting concerns
│       ├── Cache/                # Redis caching abstractions
│       ├── Middleware/           # Global exception handling
│       └── Services/             # Shared services
├── NERBABO.AppHost/              # Aspire orchestrator
└── NERBABO.ServiceDefaults/      # Shared Aspire configuration
```

## Development Standards and Conventions

### Architecture Patterns

- **Clean Architecture**: Core business logic separated from infrastructure
- **Service Layer**: Business operations encapsulated in services
- **DTO Pattern**: Data transfer objects for API boundaries
- **Dependency Injection**: Constructor injection throughout

### Code Organization

Each domain follows a consistent structure:

```text
Domain/
├── Controllers/          # API endpoints
├── Services/            # Business logic
├── Models/              # Domain entities
├── Dtos/                # Data transfer objects
└── Cache/               # Redis caching (when applicable)
```

### Naming Conventions

- **Controllers**: `{Domain}Controller` (e.g., `CoursesController`)
- **Services**: `I{Domain}Service` interface, `{Domain}Service` implementation
- **DTOs**: `Create{Entity}Dto`, `Update{Entity}Dto`, `Retrieve{Entity}Dto`
- **Models**: `{Entity}` (e.g., `Course`, `Student`)
- **Cache**: `Cache{Entity}Repository`, `ICache{Entity}Repository`

### Database Conventions

- **Entity Framework Core**: Code-first approach with migrations
- **Configuration**: Separate configuration classes in `Data/Configurations/`
- **Relationships**: Properly configured navigation properties
- **Constraints**: Database constraints and indexes defined in configurations

### API Documentation

All endpoints must include XML documentation:

```csharp
/// <summary>
/// Creates a new course.
/// </summary>
/// <param name="createDto">Course creation data</param>
/// <returns>The created course</returns>
/// <response code="201">Course created successfully</response>
/// <response code="400">Invalid request data</response>
/// <response code="401">Unauthorized access</response>
/// <response code="500">Internal server error</response>
[HttpPost]
[Authorize]
public async Task<IActionResult> CreateCourseAsync(CreateCourseDto createDto)
```

### Exception Handling

- **Global Middleware**: `GlobalExceptionHandlerMiddleware` for centralized error handling
- **Custom Exceptions**: `ObjectNullException`, `ValidationException`
- **Structured Responses**: Consistent error response format

### Performance Optimization

- **ZLinq**: High-performance LINQ operations using `.AsValueEnumerable()`
- **Redis Caching**: Frequently accessed data cached with proper invalidation
- **Database Transactions**: Critical operations wrapped in transactions
- **Async/Await**: All I/O operations are asynchronous

## Dependencies

### Core Framework

- **.NET 9**: Latest .NET framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core**: ORM for database operations

### Database & Caching

- **Npgsql.EntityFrameworkCore.PostgreSQL** (9.0.4): PostgreSQL provider for EF Core
- **StackExchange.Redis** (2.8.37): Redis client for caching

### Authentication & Security

- **Microsoft.AspNetCore.Authentication.JwtBearer** (9.0.5): JWT authentication
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore** (9.0.5): Identity management

### Documentation & API

- **Swashbuckle.AspNetCore** (9.0.1): Swagger/OpenAPI documentation
- **Microsoft.AspNetCore.OpenApi** (9.0.5): OpenAPI specification support

### PDF Generation

- **QuestPDF** (2025.7.0): Advanced PDF document generation

### Performance & Utilities

- **ZLinq** (1.4.9): High-performance LINQ operations
- **Humanizer.Core.pt** (2.14.1): Portuguese language humanization

### .NET Aspire (Orchestration)

- **Aspire.Hosting.AppHost** (9.3.0): Application orchestration
- **Aspire.Hosting.PostgreSQL** (9.3.0): PostgreSQL hosting integration
- **Aspire.Hosting.Redis** (9.3.0): Redis hosting integration
- **Aspire.Hosting.NodeJs** (9.3.0): Node.js application integration

### Observability

- **OpenTelemetry** packages: Distributed tracing and metrics
- **Microsoft.Extensions.Http.Resilience** (9.5.0): HTTP resilience patterns

## Getting Started

### Prerequisites

- .NET SDK 9.0
- Docker Desktop
- PostgreSQL (via Docker)
- Redis (via Docker)

### Running the Application

1. **Navigate to the backend directory:**

   ```bash
   cd NERBABO.Backend
   ```

2. **Build the application:**

   ```bash
   dotnet build --project NERBABO.AppHost
   ```

3. **Run database migrations:**

   ```bash
   dotnet ef migrations add "InitialMigration" -o Data/Migrations --project NERBABO.ApiService
   ```

4. **Start the application stack:**

   ```bash
   dotnet run --project NERBABO.AppHost
   ```

This will start:

- PostgreSQL database with pgAdmin
- Redis cache with RedisInsight
- ASP.NET Core API
- Angular frontend (if configured)

### Development URLs

- **API**: http://localhost:8080
- **Swagger**: http://localhost:8080/swagger
- **pgAdmin**: Available through Aspire dashboard
- **RedisInsight**: Available through Aspire dashboard

## Contributing Guidelines

### Code Quality

1. Follow established naming conventions
2. Write comprehensive XML documentation for all public APIs
3. Use ZLinq for performance-critical LINQ operations
4. Implement proper error handling and logging

### Database Changes

1. Always create migrations for schema changes
2. Include proper entity configurations
3. Document breaking changes

### Performance Considerations

1. Use caching for frequently accessed data
2. Implement proper database indexes
3. Use transactions for multi-step operations
4. Profile and optimize query performance

### Security Requirements

1. Use proper authorization attributes
2. Sanitize file uploads
3. Follow OWASP security guidelines
4. Never expose sensitive information in logs

## Testing

The project follows testing best practices:

- **Unit Tests**: Test business logic in isolation
- **Integration Tests**: Test API endpoints with real database
- **Performance Tests**: Validate caching and query performance

## Deployment

The application is designed for containerized deployment:

- **Docker**: Each service can be containerized
- **Aspire**: Production orchestration capabilities
- **PostgreSQL**: Production-ready database configuration
- **Redis**: Distributed caching for scalability

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](../LICENSE.txt) file for details
