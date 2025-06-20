using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Shared.Services
{
    // T is a Retrieve Entity dto
    // U is a Update Entity dto
    // C is a Create Entity dto
    public interface IGenericService<T, C, U>
    {
        Task<Result<IEnumerable<T>>> GetAllAsync();
        Task<Result<T>> GetByIdAsync(long id);
        Task<Result<T>> CreateAsync(C entityDto);
        Task<Result<T>> UpdateAsync(U entityDto);
        Task<Result> DeleteAsync(long id);

    }
}
