using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Sessions.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Sessions.Services;

public interface ISessionService : IGenericService<RetrieveSessionDto, CreateSessionDto, UpdateSessionDto, long>
{
    Task<Result<IEnumerable<RetrieveSessionDto>>> GetAllByActionIdAsync(long actionId);
}