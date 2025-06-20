using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Shared.Services;

/// <summary>
/// Generic service interface providing basic CRUD operations for a given entity.
/// </summary>
/// <typeparam name="R">The DTO used for retrieving data (Read/View).</typeparam>
/// <typeparam name="C">The DTO used for creating new entities (Create).</typeparam>
/// <typeparam name="U">The DTO used for updating existing entities (Update).</typeparam>
/// <typeparam name="T">The type of the entity's identifier (e.g., int, long, Guid).</typeparam>
public interface IGenericService<R, C, U, T>
{
    /// <summary>
    /// Retrieves all entities.
    /// </summary>
    /// <returns>A result containing a collection of retrieve DTOs.</returns>
    Task<Result<IEnumerable<R>>> GetAllAsync();

    /// <summary>
    /// Retrieves a single entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns>A result containing the retrieve DTO of the entity.</returns>
    Task<Result<R>> GetByIdAsync(T id);

    /// <summary>
    /// Creates a new entity.
    /// </summary>
    /// <param name="entityDto">The DTO containing data to create the entity.</param>
    /// <returns>A result containing the created entity's retrieve DTO.</returns>
    Task<Result<R>> CreateAsync(C entityDto);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entityDto">The DTO containing updated data.</param>
    /// <returns>A result containing the updated entity's retrieve DTO.</returns>
    Task<Result<R>> UpdateAsync(U entityDto);

    /// <summary>
    /// Deletes an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> DeleteAsync(T id);
}
