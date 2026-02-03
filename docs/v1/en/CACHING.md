# Side Caching

This document describes the caching strategy implemented in the NERBA Backoffice system.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Cached Entities](#cached-entities)
- [Non-Cached Entities](#non-cached-entities)
- [Key Patterns](#key-patterns)
- [Invalidation Strategy](#invalidation-strategy)
- [Implementing Cache for a New Entity](#implementing-cache-for-a-new-entity)
- [Performance Considerations](#performance-considerations)
- [Best Practices](#best-practices)
- [References](#references)

---

## Overview

The system uses the **Cache-Aside** pattern (Side Caching) with **Redis** as the distributed store. This pattern reduces database load by storing frequently accessed data in memory.

### Characteristics

| Feature | Description |
|---------|-------------|
| Technology | Redis via StackExchange.Redis |
| Pattern | Cache-Aside (Read-Through) |
| Default TTL | 60 minutes |
| Serialization | JSON (System.Text.Json) |
| Scope | Scoped (per request) |

### Operation Flow

```
                  ┌─────────────┐
                  │   Client    │
                  └──────┬──────┘
                         │
                         ▼
                  ┌─────────────┐
                  │   Service   │
                  └──────┬──────┘
                         │
            ┌────────────┼────────────┐
            │            │            │
            ▼            │            ▼
      ┌──────────┐       │      ┌──────────┐
      │  Cache?  │───No──┘      │  Cache   │
      └────┬─────┘              │  (Redis) │
           │                    └──────────┘
           Yes                        ▲
           │                          │
           ▼                          │
      ┌──────────┐              ┌─────┴────┐
      │  Return  │              │  Set in  │
      │   data   │              │  cache   │
      └──────────┘              └─────┬────┘
                                      │
                                ┌─────┴────┐
                                │ Database │
                                │(PostgreSQL)│
                                └──────────┘
```

---

## Architecture

### Main Components

```
Shared/
├── Cache/
│   ├── ICacheKeyFabric.cs     # Key generation interface
│   └── CacheKeyFabirc.cs      # Generic implementation
└── Services/
    ├── ICacheService.cs       # Redis operations interface
    └── CacheService.cs        # Redis implementation

Core/[Entity]/Cache/
├── ICache[Entity]Repository.cs   # Specific interface
└── Cache[Entity]Repository.cs    # Specific implementation
```

### ICacheService

Central interface that abstracts Redis operations.

| Method | Description |
|--------|-------------|
| `GetAsync<T>(key)` | Gets deserialized value from cache |
| `SetAsync<T>(key, value, expiry?)` | Stores serialized value in cache |
| `RemoveAsync(key)` | Removes specific entry |
| `RemovePatternAsync(pattern)` | Removes entries by pattern (wildcard) |

### ICacheKeyFabric<T>

Generic interface for consistent key generation.

| Method | Output Example |
|--------|----------------|
| `GenerateCacheKey(id)` | `Course:123` |
| `GenerateCacheKeyList()` | `Course:list` |
| `GenerateCacheKeyList(filter)` | `Course:list:active` |
| `GenerateCacheKeyManyToOne(id, Type)` | `Course:list:Frame:5` |
| `GenerateCacheKeyManyToOnePattern(Type)` | `Course:list:Frame:*` |

---

## Cached Entities

The following entities have cache implementation.

### Course

| Key | Description | Operations |
|-----|-------------|------------|
| `Course:{id}` | Single course | Get, Set, Remove |
| `Course:list` | All courses list | Get, Set, Remove |
| `Course:list:active` | Active courses list | Get, Set, Remove |
| `Course:list:Frame:{id}` | Courses by frame | Get, Set, Remove |
| `Course:list:Module:{id}` | Courses by module | Get, Set, Remove |

> See: `Core/Courses/Cache/CacheCourseRepository.cs`

### Module

| Key | Description | Operations |
|-----|-------------|------------|
| `Module:{id}` | Single module | Get, Set, Remove |
| `Module:list` | All modules list | Get, Set, Remove |
| `Module:list:active` | Active modules list | Get, Set, Remove |

> See: `Core/Modules/Cache/CacheModuleRepository.cs`

### CourseAction

| Key | Description | Operations |
|-----|-------------|------------|
| `CourseAction:{id}` | Single action | Get, Set, Remove |
| `CourseAction:list` | All actions list | Get, Set, Remove |
| `CourseAction:list:Course:{id}` | Actions by course | Get, Set, Remove |
| `CourseAction:list:Module:{id}` | Actions by module | Get, Set, Remove |

> See: `Core/Actions/Cache/CacheActionRepository.cs`

### Person

| Key | Description | Operations |
|-----|-------------|------------|
| `Person:{id}` | Single person | Get, Set, Remove |
| `Person:list` | All people list | Get, Set, Remove |
| `Person:list:without:{profile}` | People without specific profile | Get, Set, Remove |

> See: `Core/People/Cache/CachePeopleRepository.cs`

### Teacher

| Key | Description | Operations |
|-----|-------------|------------|
| `Teacher:{id}` | Single teacher | Get, Set, Remove |
| `Teacher:list` | All teachers list | Get, Set, Remove |

> See: `Core/Teachers/Cache/CacheTeacherRepository.cs`

### Student

| Key | Description | Operations |
|-----|-------------|------------|
| `Student:{id}` | Single student | Get, Set, Remove |
| `Student:list` | All students list | Get, Set, Remove |
| `Student:list:Company:{id}` | Students by company | Get, Set, Remove |

> See: `Core/Students/Cache/CacheStudentsRepository.cs`

---

## Non-Cached Entities

The following entities do not currently have cache implementation.

| Entity | Reason |
|--------|--------|
| Company | Low access volume |
| Frame | Static data, rarely changed |
| Session | Frequently changed data |
| SessionParticipation | Transactional relationship |
| ActionEnrollment | Transactional relationship |
| ModuleTeaching | Transactional relationship |
| ModuleAvaliation | Evaluation data |
| Payment | Sensitive financial data |
| Notification | Volatile data |
| Account/User | Security (authentication/authorization) |
| Kpi | Dynamically calculated data |

---

## Key Patterns

The system follows consistent conventions for cache keys.

### Base Format

```
{EntityName}:{identifier}
```

### Conventions

| Pattern | Format | Example |
|---------|--------|---------|
| Single entity | `Entity:{id}` | `Course:123` |
| Full list | `Entity:list` | `Course:list` |
| Filtered list | `Entity:list:{filter}` | `Course:list:active` |
| N:1 relationship | `Entity:list:{RelatedEntity}:{id}` | `Course:list:Frame:5` |
| Wildcard | `Entity:list:{RelatedEntity}:*` | `Course:list:Frame:*` |

### Naming

- Use **PascalCase** for entity names
- Use **camelCase** for filters
- Separator: `:` (colon)
- Wildcard: `*` (asterisk)

---

## Invalidation Strategy

Cache invalidation follows the principle of **aggressive invalidation** to ensure data consistency.

### Invalidation Rules

1. **On create**: Invalidate related lists, set individual entry
2. **On update**: Invalidate individual entry and all related lists
3. **On delete**: Invalidate individual entry and all related lists

### Invalidation Example (Course)

```csharp
public async Task RemoveCourseCacheAsync(long? id = null)
{
    if (id is not null)
    {
        // Remove individual entry
        await _cacheService.RemoveAsync(_cacheKeyFabric.GenerateCacheKey($"{id}"));

        // Remove general lists
        await _cacheService.RemoveAsync(_cacheKeyFabric.GenerateCacheKeyList());
        await _cacheService.RemoveAsync(_cacheKeyFabric.GenerateCacheKeyList("active"));

        // Remove relationship lists (wildcard)
        await _cacheService.RemovePatternAsync(
            _cacheKeyFabric.GenerateCacheKeyManyToOnePattern(typeof(Frame)));
        await _cacheService.RemovePatternAsync(
            _cacheKeyFabric.GenerateCacheKeyManyToOnePattern(typeof(Module)));
    }
    else
    {
        // Remove everything related to courses
        await _cacheService.RemovePatternAsync($"{typeof(Course).Name}:*");
    }
}
```

### Cascade Invalidation

When an entity is changed, dependent entity caches should also be invalidated.

```csharp
// Example in PeopleService
private async Task RemoveRelatedCache(long? id = null)
{
    await _cache.RemovePeopleCacheAsync(id);
    await _cacheStudents.RemoveStudentsCacheAsync();  // Invalidate students
    await _cacheTeacher.RemoveTeacherCacheAsync();    // Invalidate teachers
}
```

---

## Implementing Cache for a New Entity

To add caching to a new entity, follow these steps.

### 1. Create Cache Repository Interface

```csharp
// Core/[Entity]/Cache/ICache[Entity]Repository.cs
public interface ICacheCompanyRepository
{
    Task RemoveCompanyCacheAsync(long? id = null);
    Task SetSingleCompanyCacheAsync(RetrieveCompanyDto company);
    Task<RetrieveCompanyDto?> GetSingleCompanyCacheAsync(long id);
    Task<IEnumerable<RetrieveCompanyDto>?> GetCacheAllCompaniesAsync();
    Task SetAllCompaniesCacheAsync(IEnumerable<RetrieveCompanyDto> companies);
}
```

### 2. Implement Cache Repository

```csharp
// Core/[Entity]/Cache/Cache[Entity]Repository.cs
public class CacheCompanyRepository(
    ICacheKeyFabric<Company> cacheKeyFabric,
    ICacheService cacheService
) : ICacheCompanyRepository
{
    private readonly ICacheKeyFabric<Company> _cacheKeyFabric = cacheKeyFabric;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<IEnumerable<RetrieveCompanyDto>?> GetCacheAllCompaniesAsync()
    {
        return await _cacheService.GetAsync<IEnumerable<RetrieveCompanyDto>>(
            _cacheKeyFabric.GenerateCacheKeyList());
    }

    public async Task<RetrieveCompanyDto?> GetSingleCompanyCacheAsync(long id)
    {
        return await _cacheService.GetAsync<RetrieveCompanyDto>(
            _cacheKeyFabric.GenerateCacheKey(id.ToString()));
    }

    public async Task RemoveCompanyCacheAsync(long? id = null)
    {
        if (id is not null)
        {
            await _cacheService.RemoveAsync(_cacheKeyFabric.GenerateCacheKey($"{id}"));
            await _cacheService.RemoveAsync(_cacheKeyFabric.GenerateCacheKeyList());
        }
        else
        {
            await _cacheService.RemovePatternAsync($"{typeof(Company).Name}:*");
        }
    }

    public async Task SetAllCompaniesCacheAsync(IEnumerable<RetrieveCompanyDto> companies)
    {
        await _cacheService.SetAsync(_cacheKeyFabric.GenerateCacheKeyList(), companies);
    }

    public async Task SetSingleCompanyCacheAsync(RetrieveCompanyDto company)
    {
        await _cacheService.SetAsync(
            _cacheKeyFabric.GenerateCacheKey(company.Id.ToString()), company);
    }
}
```

### 3. Register in DI Container

```csharp
// Program.cs
builder.Services.AddScoped<ICacheKeyFabric<Company>, CacheKeyFabirc<Company>>();
builder.Services.AddScoped<ICacheCompanyRepository, CacheCompanyRepository>();
```

### 4. Inject into Service

```csharp
public class CompanyService(
    AppDbContext context,
    ILogger<CompanyService> logger,
    ICacheCompanyRepository cache
) : ICompanyService
{
    // ...
}
```

### 5. Implement Cache-Aside Pattern in Service

```csharp
public async Task<Result<RetrieveCompanyDto>> GetByIdAsync(long id)
{
    // 1. Try to get from cache
    var cachedCompany = await _cache.GetSingleCompanyCacheAsync(id);
    if (cachedCompany is not null)
        return Result<RetrieveCompanyDto>.Ok(cachedCompany);

    // 2. Get from database
    var company = await _context.Companies.FindAsync(id);
    if (company is null)
        return Result<RetrieveCompanyDto>
            .Fail("Not found.", "Company not found.",
            StatusCodes.Status404NotFound);

    var retrieveCompany = Company.ConvertEntityToRetrieveDto(company);

    // 3. Store in cache
    await _cache.SetSingleCompanyCacheAsync(retrieveCompany);

    return Result<RetrieveCompanyDto>.Ok(retrieveCompany);
}

public async Task<Result<RetrieveCompanyDto>> UpdateAsync(UpdateCompanyDto dto)
{
    // ... update logic ...

    // Invalidate cache after change
    await _cache.RemoveCompanyCacheAsync(dto.Id);
    await _cache.SetSingleCompanyCacheAsync(retrieveCompany);

    return Result<RetrieveCompanyDto>.Ok(retrieveCompany, "Company Updated");
}
```

### Final File Structure

```
Core/Companies/
├── Cache/
│   ├── ICacheCompanyRepository.cs
│   └── CacheCompanyRepository.cs
├── Controllers/
│   └── CompanyController.cs
├── Dtos/
│   └── ...
├── Models/
│   └── Company.cs
└── Services/
    ├── ICompanyService.cs
    └── CompanyService.cs
```

---

## Performance Considerations

### TTL (Time-To-Live)

| Data Type | Recommended TTL |
|-----------|-----------------|
| General lists | 60 minutes (default) |
| Individual entities | 60 minutes |
| Static data | 120+ minutes |
| Volatile data | 15-30 minutes |

### Customize TTL

```csharp
// Set custom TTL
await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(30));
```

### Monitoring

Use **RedisInsight** to monitor:
- Hit/miss rates
- Memory usage
- Cached keys
- Remaining TTLs

---

## Best Practices

### Consistency

1. **Always invalidate before setting** - Prevents stale data
2. **Cascade invalidation** - Consider dependent entities
3. **Use wildcards carefully** - Can affect performance

### Naming

1. **Descriptive keys** - Easier debugging
2. **Consistent patterns** - Follow project conventions
3. **Avoid long keys** - Impacts memory and network

### Security

1. **Don't cache sensitive data** - Passwords, tokens
2. **Financial data** - Avoid or use short TTL
3. **Authentication data** - Prefer specific mechanisms

### When NOT to Use Cache

- Frequently changing data
- Critical transactional data
- Data requiring strong consistency
- Complex or dynamic queries

---

## References

### Source Code

| Component | Location |
|-----------|----------|
| ICacheService | `Shared/Services/ICacheService.cs` |
| CacheService | `Shared/Services/CacheService.cs` |
| ICacheKeyFabric | `Shared/Cache/ICacheKeyFabric.cs` |
| CacheKeyFabirc | `Shared/Cache/CacheKeyFabirc.cs` |
| Cache Repositories | `Core/[Entity]/Cache/` |

### Redis Configuration

| File | Description |
|------|-------------|
| `.env` | Redis connection string |
| `Program.cs` | Service registration |
| `docker-compose.yml` | Redis container |

### External Documentation

> Ref: [Redis Documentation](https://redis.io/docs/)

> Ref: [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)

> Ref: [Cache-Aside Pattern (Microsoft)](https://learn.microsoft.com/en-us/azure/architecture/patterns/cache-aside)

### Related Documentation

> See: [Business Logic](./BUSINESS_LOGIC.md)

> See: [Entity Validations](./VALIDATIONS.md)
